using System;
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
        private float _punchScale = 1f;
        private Color _baseColor = Color.white;
        private System.Collections.IEnumerator _punchCo;
        private System.Collections.IEnumerator _flashCo;

        // 보스 공격 패턴 (boss_attack_v1.md) — 5001 T07: IBossPhase State 패턴으로 대체
        // _bossAttack 제거. CurrentPhase 가 내부에서 BossPatternFactory 로 IBossAttackPattern 생성.
        public IBossPhase CurrentPhase { get; private set; }
        public void SetCurrentPhase(IBossPhase phase) => CurrentPhase = phase;

        public EnemyData Data => _data;
        public bool IsDead => _isDead;
        public float MaxHealth => _maxHealth;
        public float AttackDamage => _attackDamage;
        public float HealthRatio => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;

        // A4 — chapter_curve §9.1: HP/공격력에 챕터 곱셈 적용. multiplier=1f면 영향 없음.
        public void Initialize(EnemyData data, float statMultiplier = 1f)
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
            ApplyAppearance(data);
            _sr = GetComponentInChildren<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
            _baseScaleX = transform.localScale.x;
            _baseScaleY = transform.localScale.y;
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
            float wobble = Mathf.Sin(time * 4f + _swayPhase) * 0.04f;
            transform.localScale = new Vector3(
                _baseScaleX * _punchScale,
                _baseScaleY * (1f + wobble) * _punchScale,
                1f);
        }

        /// <summary>PIVOT v4 — Premium 피격 (cinematic_combat_v1.md §3).</summary>
        public void OnHit(float damage, bool isCrit)
        {
            if (_isDead) return;
            // 글로벌 효과 — 적이 비활성돼도 적용 (floater/audio/vfx/shake)
            // 3. Hit particle
            VfxPool.SpawnHitStar(transform.position, isCrit);
            // 4. Sound
            AudioManager.Instance?.Play(isCrit ? SfxType.CritHit : SfxType.EnemyHit);
            // 5. Damage number
            DamageFloaterPool.Spawn(
                transform.position + Vector3.up * 0.5f,
                Mathf.RoundToInt(damage),
                isCrit ? DamageType.Critical : DamageType.Normal);
            // 6. Critical only — slow-mo + screen shake
            if (isCrit)
            {
                CinematicEffects.Instance?.PlayHitPause(0.08f);
                CinematicEffects.Instance?.PlayScreenShake(0.3f);
            }
            // 적별 효과 — gameObject active 인 경우만 (1발 처치 시 의미 X, 효과만 노출)
            if (!gameObject.activeInHierarchy) return;
            // 1. Punch scale
            if (_punchCo != null) StopCoroutine(_punchCo);
            _punchCo = PunchRoutine(isCrit ? 1.7f : 1.4f, isCrit ? 0.07f : 0.05f, isCrit ? 0.15f : 0.10f);
            StartCoroutine(_punchCo);
            // 2. Color flash
            if (_flashCo != null) StopCoroutine(_flashCo);
            _flashCo = FlashRoutine(isCrit ? Color.yellow : Color.red, isCrit ? 0.07f : 0.05f, isCrit ? 0.15f : 0.10f);
            StartCoroutine(_flashCo);
        }

        private System.Collections.IEnumerator PunchRoutine(float targetScale, float expandTime, float returnTime)
        {
            float t = 0f;
            while (t < expandTime)
            {
                t += Time.unscaledDeltaTime;
                _punchScale = Mathf.Lerp(1f, targetScale, t / expandTime);
                yield return null;
            }
            t = 0f;
            while (t < returnTime)
            {
                t += Time.unscaledDeltaTime;
                _punchScale = Mathf.Lerp(targetScale, 1f, t / returnTime);
                yield return null;
            }
            _punchScale = 1f;
            _punchCo = null;
        }

        private System.Collections.IEnumerator FlashRoutine(Color flashColor, float holdTime, float returnTime)
        {
            if (_sr == null) yield break;
            _sr.color = flashColor;
            yield return new WaitForSecondsRealtime(holdTime);
            float t = 0f;
            while (t < returnTime)
            {
                t += Time.unscaledDeltaTime;
                float k = t / returnTime;
                _sr.color = Color.Lerp(flashColor, _baseColor, k);
                yield return null;
            }
            _sr.color = _baseColor;
            _flashCo = null;
        }

        private void ApplyAppearance(EnemyData data)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                // Phase G2 — data.icon 이 있으면 sprite 동적 변경 (placeholder 원형 → 실 적 idle)
                if (data.icon != null) sr.sprite = data.icon;
                // tint 적용 — 신규 sprite는 흰색 유지(원본 색 보존), placeholder 원형은 tintColor 적용
                sr.color = data.icon != null ? Color.white : data.tintColor;
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
