using System;
using LitMotion;
using UnityEngine;

namespace LittleHero
{
    public class EnemyController : MonoBehaviour
    {
        public event Action<EnemyController> OnDied;

        private EnemyData _data;
        private float _currentHealth;
        private float _maxHealth;
        private float _attackDamage;
        private bool _isDead;

        // 코드 애니메이션 (P10 패턴 답습 — Rigidbody Y 안 건드림)
        private SpriteRenderer _sr;
        private Rigidbody2D _rb;
        private float _swayPhase;
        private float _baseScaleX;
        private float _baseScaleY;

        // PIVOT v4 — Premium 피격 (cinematic_combat_v1.md §3)
        // 5016 T02 — IEnumerator + Lerp → LitMotion MotionHandle (state machine alloc 0)
        private float _punchScale = 1f;
        private Color _baseColor = Color.white;
        private MotionHandle _punchHandle;
        private MotionHandle _flashHandle;

        // 5017 T03 — wobble 변동 임계 0.005f (D3=B). native interop 절감.
        private const float WOBBLE_DELTA_THRESHOLD = 0.005f;
        private float _lastAppliedWobble = float.MinValue;
        private float _lastAppliedPunch = float.MinValue;

        // 보스 공격 패턴 (boss_attack_v1.md) — 5001 T07: IBossPhase State 패턴으로 대체
        // _bossAttack 제거. CurrentPhase 가 내부에서 BossPatternFactory 로 IBossAttackPattern 생성.
        public IBossPhase CurrentPhase { get; private set; }
        public void SetCurrentPhase(IBossPhase phase) => CurrentPhase = phase;

        public EnemyData Data => _data;
        public bool IsDead => _isDead;
        public float MaxHealth => _maxHealth;
        public float AttackDamage => _attackDamage;
        public float HealthRatio => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;

        // 5013 — Off-screen culling. Manager(EnemyAnimationManager) 가 매 0.1s 호출.
        private bool _isVisible = true;
        public bool IsVisible => _isVisible;
        public void SetVisible(bool visible)
        {
            if (_isVisible == visible) return;
            _isVisible = visible;
            if (_sr != null) _sr.enabled = visible;
        }

        // A4 — chapter_curve §9.1: HP/공격력에 챕터 곱셈 적용. multiplier=1f면 영향 없음.
        // 5012 — variationIndex (default 0) hue 변형. BossTier!=None 시 무시.
        public void Initialize(EnemyData data, float statMultiplier = 1f, int variationIndex = 0)
        {
            _data = data;
            float m = Mathf.Max(0.01f, statMultiplier);
            // PIVOT v3 — 적 1/10 (보스 제외, 보스 의미 보존). game_pivot_v2.md PIVOT v3.
            var balance = GameManager.Instance != null ? GameManager.Instance.Balance : null;
            float v3HpMul = (balance != null && !data.isBoss) ? balance.enemyHpMul : 1f;
            float v3AtkMul = (balance != null && !data.isBoss) ? balance.enemyAtkMul : 1f;
            _maxHealth     = data.maxHealth * m * v3HpMul;
            _attackDamage  = data.damage    * m * v3AtkMul;
            _currentHealth = _maxHealth;
            _isDead = false;
            ApplyAppearance(data, variationIndex);
            _sr = GetComponentInChildren<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
            _baseScaleX = transform.localScale.x;
            _baseScaleY = transform.localScale.y;
            // 5012 D3=B — _baseColor hue 결과로 캐시 (flash 후 hue 색상 유지)
            _baseColor = _sr != null ? _sr.color : Color.white;
            _punchScale = 1f;
            // 적별 다른 위상으로 전체 sway 가 동기화되지 않게 무작위 시작점
            _swayPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            // PIVOT v3 Phase 2 — 단일 Update 패턴 (EnemyAnimationManager 위임)
            EnemyAnimationManager.Register(this);

            // 5001 T07 — IBossPhase State 패턴으로 보스 공격 통합.
            // BossPhaseManager.CreateInitial 이 bossTier 따라 Phase 1 또는 null 반환.
            CurrentPhase = BossPhaseManager.CreateInitial(this);
            CurrentPhase?.OnEnter(this);
        }

        // PIVOT v3 Phase 2 — EnemyAnimationManager 가 매 프레임 호출 (Unity Update callback 폐기)
        public void UpdateAnim(float time)
        {
            if (_isDead || _sr == null) return;

            // 5001 T07 — IBossPhase State Tick + Phase 전환 검사 (BossPhaseManager 위임)
            BossPhaseManager.Update(this, Time.deltaTime);

            // 1) 좌우 flip — 이동 방향 따라 sprite 반전
            if (_rb != null)
            {
                float vx = _rb.linearVelocity.x;
                if (vx > 0.05f) _sr.flipX = false;
                else if (vx < -0.05f) _sr.flipX = true;
            }

            // 2) 가벼운 scale wobble (호흡 효과) — Rigidbody Y 안 건드림
            // PIVOT v4 — _punchScale 곱셈 (피격 시 scale 1.4×~1.7×)
            // 5017 T03 — 변동 임계 0.005f (D3=B). 변동 미미 시 setter 생략 — native interop 절감.
            float wobble = Mathf.Sin(time * 4f + _swayPhase) * 0.04f;
            if (Mathf.Abs(wobble - _lastAppliedWobble) > WOBBLE_DELTA_THRESHOLD ||
                Mathf.Abs(_punchScale - _lastAppliedPunch) > WOBBLE_DELTA_THRESHOLD)
            {
                _lastAppliedWobble = wobble;
                _lastAppliedPunch = _punchScale;
                transform.localScale = new Vector3(
                    _baseScaleX * _punchScale,
                    _baseScaleY * (1f + wobble) * _punchScale,
                    1f);
            }
        }

        /// <summary>PIVOT v4 — Premium 피격 (cinematic_combat_v1.md §3). 5007 T07 — Resolver 통합 (적 티어 / 데미지 강도 / 무기 색).</summary>
        public void OnHit(float damage, bool isCrit, WeaponCategory? weaponCategory = null, float avgPlayerDamage = 0f)
        {
            if (_isDead) return;

            // 5007 — Resolver 호출 (cfg null 안전, BossTier 5종 + WeaponCategory 4 그룹 + intensity)
            var cfg = CinematicEffectsConfigLoader.Get();
            var resolved = HitEffectResolver.Resolve(cfg, _data, damage, isCrit, weaponCategory, avgPlayerDamage);

            // 글로벌 효과 — 적이 비활성돼도 적용
            VfxPool.SpawnHitStar(transform.position, isCrit, resolved.VfxScale);  // 5007 — tier 별 vfxScale
            AudioManager.Instance?.Play(isCrit ? SfxType.CritHit : SfxType.EnemyHit);
            DamageFloaterPool.Spawn(
                transform.position + Vector3.up * 0.5f,
                Mathf.RoundToInt(damage),
                isCrit ? DamageType.Critical : DamageType.Normal,
                resolved.FloaterSizeMul);  // 5007 — tier 별 floater 크기

            // Critical only — hit pause + screen shake (+ camera zoom 신규)
            if (resolved.EnableHitPause)
            {
                CinematicEffects.Instance?.PlayHitPause(cfg != null ? cfg.slowMoDuration : 0.08f);
                CinematicEffects.Instance?.PlayScreenShake(resolved.ShakeAmplitude);
            }
            if (resolved.EnableZoom && cfg != null)
            {
                CinematicEffects.Instance?.PlayCameraZoom(cfg.zoomMul, cfg.zoomUpDuration, cfg.zoomDownDuration);
            }

            // 적별 효과 — gameObject active 인 경우만
            if (!gameObject.activeInHierarchy) return;

            // Punch scale — Resolver 결과 (tier × intensity × cap 2.0)
            float punchUp   = isCrit ? (cfg != null ? cfg.punchUpCritDuration   : 0.07f) : (cfg != null ? cfg.punchUpDuration   : 0.05f);
            float punchDown = isCrit ? (cfg != null ? cfg.punchDownCritDuration : 0.15f) : (cfg != null ? cfg.punchDownDuration : 0.10f);
            DoPunch(resolved.PunchScale, punchUp, punchDown);

            // Color flash — Resolver 결과 (무기 카테고리별 색)
            DoFlash(resolved.FlashColor, punchUp, punchDown);
        }

        // 5016 T02 — LitMotion 마이그레이션. Coroutine state machine alloc 0.
        // LSequence.Append × 2 — 1f → targetScale → 1f. WithScheduler(UpdateIgnoreTimeScale) 으로 HitPause/paused 환경에서도 동작.
        private void DoPunch(float targetScale, float expandTime, float returnTime)
        {
            if (_punchHandle.IsActive()) _punchHandle.Cancel();
            _punchHandle = LSequence.Create()
                .Append(LMotion.Create(1f, targetScale, expandTime).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale).Bind(this, (s, t) => t._punchScale = s))
                .Append(LMotion.Create(targetScale, 1f, returnTime).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale).Bind(this, (s, t) => t._punchScale = s))
                .Run();
        }

        // 5016 T02 — LitMotion 마이그레이션. WithDelay 로 hold 후 Lerp.
        // 5012 D3=B 정합 — _baseColor 복귀 (hue 색상 유지).
        private void DoFlash(Color flashColor, float holdTime, float returnTime)
        {
            if (_sr == null) return;
            if (_flashHandle.IsActive()) _flashHandle.Cancel();
            _sr.color = flashColor;
            Color baseColor = _baseColor;
            _flashHandle = LMotion.Create(0f, 1f, returnTime)
                .WithDelay(holdTime)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(this, (k, t) =>
                {
                    if (t._sr != null) t._sr.color = Color.Lerp(flashColor, baseColor, k);
                });
        }

        // 5012.1 — icon != null 인 5008 sprite multiplicative tint baseline.
        // Color.white * sprite_pixel = sprite_pixel (효과 0) → HUE_BASE_TINT (saturation 있는 light pink)
        // * sprite_pixel = 약한 색조 변형. HueShift(HUE_BASE_TINT, offset) 으로 5 단계 다양화.
        private static readonly Color HUE_BASE_TINT = new Color(1f, 0.92f, 0.92f, 1f);

        private void ApplyAppearance(EnemyData data, int variationIndex = 0)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                // Phase G2 — data.icon 이 있으면 sprite 동적 변경 (placeholder 원형 → 실 적 idle)
                if (data.icon != null) sr.sprite = data.icon;
                // 5012 D1=B / 5012.1 — variationIndex>0 + Boss=None 시 HUE_BASE_TINT(saturated) 에 hue 적용,
                //   else icon 있으면 white(원색 유지), 없으면 placeholder tintColor.
                if (variationIndex > 0 && data.bossTier == BossTier.None)
                {
                    Color tintForHue = data.icon != null ? HUE_BASE_TINT : data.tintColor;
                    sr.color = EnemyHueVariation.HueShift(tintForHue, EnemyHueVariation.OFFSETS[variationIndex]);
                }
                else
                {
                    sr.color = data.icon != null ? Color.white : data.tintColor;
                }
            }
            // P11 — 768×768 PNG 베이스 0.4 → 0.55. PIVOT v3 — 적 1/10 scale (보스 제외).
            float baseScale = data.scaleMultiplier > 0f ? data.scaleMultiplier : 1f;
            float spriteScale = data.icon != null ? 0.55f : 1f;
            // PIVOT v3 적 scale × 0.1 (보스 제외, 위압감 보존)
            var balance = GameManager.Instance != null ? GameManager.Instance.Balance : null;
            float v3ScaleMul = (balance != null && !data.isBoss) ? balance.enemyScaleMul : 1f;
            float finalScale = baseScale * spriteScale * v3ScaleMul;
            transform.localScale = new Vector3(finalScale, finalScale, 1f);
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - damage);

            if (_currentHealth <= 0f)
                Die();
        }

        private static GameObject _cachedDeathFx;

        private void Die()
        {
            _isDead = true;
            // PIVOT v3 Phase 2 — Animation Manager 에서 unregister (Update 폐기)
            EnemyAnimationManager.Unregister(this);
            // 5001 T07 — IBossPhase 정리 (Phase OnExit + null)
            if (CurrentPhase != null) { CurrentPhase.OnExit(this); CurrentPhase = null; }
            GameEvents.RaiseEnemyKilled(_data);
            OnDied?.Invoke(this);

            // PIVOT v2 — 적 사망 시 장비 드랍 (game_pivot_v2.md §3).
            TryDropEquipment();

            // PIVOT v3 Phase 2 — VfxPool 재사용 (Instantiate 매 처치 폐기, GC critical)
            VfxPool.SpawnStarBurst(transform.position);

            // 5001 Sprint 3.2 — BossLootTable 보상 적용 (lootTable wired 시만)
            if (_data != null && _data.lootTable != null)
            {
                BossLootGrantService.Apply(_data.lootTable);
            }

            // 풀에서 반환되지 않은 경우 안전 비활성화
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        private void TryDropEquipment()
        {
            if (!ItemDropService.IsInitialized || _data == null) return;
            var ctx = new DropContext
            {
                source = _data.isBoss ? DropSource.Boss
                       : _data.isMiniB ? DropSource.MiniBoss
                       : DropSource.Enemy,
                chapterNumber = MetaProgression.Instance != null && MetaProgression.Instance.SelectedChapter != null
                    ? Mathf.Max(1, MetaProgression.Instance.SelectedChapter.chapterNumber)
                    : 1,
                enemy = _data,
            };
            var instance = ItemDropService.TryDrop(ctx);
            if (instance == null) return;
            // PIVOT v3 Phase 2 — drop batching (GC critical). 매 드랍 SaveData append+Save 폐기.
            // MetaProgression._pendingDrops 에 메모리만 append, ChapterCleared/PlayerDied 시 일괄 Save.
            MetaProgression.Instance?.QueueDrop(instance);
        }
    }
}
