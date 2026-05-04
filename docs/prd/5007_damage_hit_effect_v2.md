# 5007 — 적 타격 데미지 이펙트 개선 (Damage Hit Effect v2)

> **목적**: PIVOT v4 Cinematic Combat 의 OnHit 이펙트 시스템을 강화 — 적 티어 차별화 / 데미지 크기 동적 강도 / 무기 종류별 색감 / 풀 부족 해결 / SO 화 (튜닝 가능). 만족도가 가장 높은 "타격감" 게임 경험 제공 (Vampire Survivors / Maple 키우기 수준).
> **적용 우선순위**: §15 OOP 정합 + §2 PG-12 톤 + §7 성능 (cap 300 / GC <50KB).
> **연계 시스템**: PIVOT v4 Cinematic Combat / Damage v2 / weapon_v2 (WeaponCategory) / 5001 boss_system_v2 (BossTier).

## 0. 결정 이력

- 2026-05-04: 사용자 발화 "적 타격시 데미지 이펙트효과 개선안 도출" → 본 디자인 문서 작성 시작
- 분석 발견: 모든 적이 동일 효과 (티어 차별화 X), DamageFloaterPool 50개 (cap 300 대비 부족), magic number 모두 hardcode

## 1. 현재 시스템 분석

### 1.1 호출 흐름 (PIVOT v4)

```
Player attack
  ↓
DamageDealer.ApplyMulti / ApplySingle
  ↓
DamageSystem.DealDamage(enemy, damage)
  ├─ RollCrit (5% baseline) + GetCritDamageMul (1.5×)
  ├─ Mythic / Armor / Lifesteal 보정
  └─ enemy.OnHit(damage, isCrit)  ← 이펙트 진입점
        ↓
EnemyController.OnHit
  [글로벌 효과 — SetActive=false 여도 실행]
  ├─ VfxPool.SpawnHitStar (scale 0.6× / 1.4× crit)
  ├─ AudioManager.Play (EnemyHit / CritHit)
  ├─ DamageFloaterPool.Spawn (24pt white / 36pt gold)
  └─ if crit: CinematicEffects.PlayHitPause(0.08s) + ScreenShake(0.3, 0.2s)

  [로컬 효과 — 활성 GameObject 만]
  ├─ PunchRoutine (1.4× / 1.7× crit, 0.05+0.10s / 0.07+0.15s)
  └─ FlashRoutine (Color.red / Color.yellow, 0.05+0.10s / 0.07+0.15s)
```

### 1.2 현재 효과 인벤토리 (11종)

| # | 효과 | 일반 | 크리 | 적 티어 차별화 | 무기 차별화 | 데미지 크기 차별화 |
|---|---|---|---|---|---|---|
| 1 | Punch Scale | 1.4× | 1.7× | ❌ | ❌ | ❌ |
| 2 | Color Flash | Red | Yellow | ❌ | ❌ | ❌ |
| 3 | HitStar VFX | 0.6× | 1.4× gold | ❌ | ❌ | ❌ |
| 4 | Damage Floater | 24pt white | 36pt gold | ❌ | ❌ | ❌ |
| 5 | Floater Shake | — | ±0.05px | ❌ | — | ❌ |
| 6 | Hit Pause (slow-mo) | — | 0.05× × 0.08s | ❌ | — | ❌ |
| 7 | Screen Shake | — | 0.3 × 0.2s | ❌ | — | ❌ |
| 8 | SFX Normal | 350Hz 0.07s | — | ❌ | ❌ | ❌ |
| 9 | SFX Crit | — | 1200+1800Hz 0.15s | ❌ | ❌ | ❌ |

> **핵심 문제**: 9종 효과 모두 binary (일반/크리) 만 분기. 적 티어·무기·데미지 크기 무관.

### 1.3 위반·한계 (CLAUDE.md §15 + §7 검증)

| 항목 | 상태 | 영향 |
|---|---|---|
| §15.4 SO 우선 | ❌ — 모든 magic number hardcode (1.4×/1.7×/0.05f/0.08s/350Hz/...) | 디자이너 튜닝 불가, 밸런스 조정 어려움 |
| §15.6 100줄 룰 | ✅ — EnemyController.OnHit ~30L, CinematicEffects ~80L | OK |
| §15.11 GC | ⚠️ — DamageFloaterPool=50, cap 300 환경 매 프레임 new() 위험 | GC pressure 중간~높음 |
| §15.12 Validator | ❌ — 룰 없음 (이펙트 일관성 검증 X) | OK (선택 추가 가능) |
| §7 cap 300 GC <50KB | ⚠️ — 풀 부족 시 30~60KB GC/frame 추정 | 60fps 위협 가능 |

### 1.4 SO 인스턴스 분포

- 데미지·이펙트 관련 SO **0개** — BalanceConfig 에 PIVOT v4 §6 필드 6종 (enemyHpMul 등) 만, 이펙트 무관

## 2. 옵션 비교

### 2.1 D1 — SO 아키텍처

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A** | BalanceConfig.asset 에 필드 추가 (단일 SO 확장) | 간단, 1 SO 만 관리 | BalanceConfig 비대화 (현재 PIVOT v4 6 필드 + 신규 ~20 필드) |
| **B (★권장)** | **`CinematicEffectsConfig.asset` 신규 SO** + BossTier 매트릭스 | SRP ✅, 디자이너 분리 작업 ✅, weapon_v2 / 5001 와 결합도 ↓ | 1 SO 신규 |
| **C** | 적 티어별 SO (4개: None / MiniBoss / ChapterBoss / DailyBoss) | 세밀 튜닝 | 4 SO 동기화 부담, 공통 필드 중복 |

### 2.2 D2 — 적 티어별 차별화 정책

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A** | enum BossTier 별 multiplier (×1.0 / ×1.2 / ×1.5) | 단순, 일관성 | 효과 종류별 fine-tune 불가 |
| **B (★권장)** | **TierEffectProfile 구조체** (punchMul / shakeMul / floaterSizeMul / vfxScaleMul 분리) | 효과별 fine-tune ✅ | 약간 복잡 |
| **C** | 5001 EnemyData.bossTier 에 직접 필드 추가 | 데이터 일체화 | 137개 EnemyData 마이그레이션 필요 |

### 2.3 D3 — 데미지 크기 동적 강도

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A** | 무시 (현재 그대로) | 변경 0 | 큰 데미지 무시되는 단조로움 |
| **B (★권장)** | **damageRatio = damage / PlayerStats.AttackDamage 평균, intensity = clamp(ratio/5, 0, 1)** | 자동 스케일, 직관적 | PlayerStats 의존성 |
| **C** | 누적 백분위 (이전 100hit 평균 대비) | 정확한 백분위 | 메모리 (ring buffer) + 계산 |
| **D** | Damage threshold (1K / 10K / 100K / 1M / 10M 단계) | 명확한 segmentation | 단계 사이 jump |

### 2.4 D4 — 무기 종류별 차별화

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A** | 무시 | 변경 0 | 무기 타격 감 없음 |
| **B (★권장)** | **OnHit(damage, isCrit, weaponCategory)** 인자 추가 + WeaponCategory → flashColor / sfxId 매핑 | weapon_v2 와 정합, 11 카테고리 활용 | DamageSystem.DealDamage signature 확장 |
| **C** | DamageType enum 추가 (Sword/Bow/Magic/...) | 무기와 독립적 | weapon_v2 중복 |

### 2.5 D5 — DamageFloaterPool 크기

| 옵션 | 설명 |
|---|---|
| **A** | 50 유지 | ❌ cap 300 환경 부족 |
| **B (★권장)** | **300 으로 prewarm** (cap 300 동시 hit 30 × 1s × 60fps = 60 active 예상, 여유 5×) |
| **C** | 동적 확장 (필요 시 new()) | GC 폭증 |

### 2.6 D6 — 연쇄 피격 stacking

| 옵션 | 설명 |
|---|---|
| **A** | 무시 (현재 — coroutine restart 마다 0 부터) | 시각적 떨림 |
| **B** | 0.1s 내 재hit 시 punch scale 누적 (cap 2.0×) | 간단, 임팩트 |
| **C (★권장)** | **A + 0.05s 디바운스 (그 이내는 효과 무시, GC 절약)** | 성능 + 시각 안정 |

> 사용자 결정 사안 — Discuss

### 2.7 D7 — 카메라 효과 확장 (zoom / tilt)

| 옵션 | 설명 |
|---|---|
| **A** | screen shake 만 (현재) | 변경 0 |
| **B (★권장)** | **크리 시 zoom 1.0→1.05 (0.08s) + return** 추가 (subtle) | 임팩트 ↑ 작은 변경 |
| **C** | zoom + tilt + DOF | 과도 (멀미 위험) |

### 2.8 D8 — 색맹 친화 / 접근성

| 옵션 | 설명 |
|---|---|
| **A (★권장)** | **본 PRD 범위 외** — 5008 후속 사이클 분리 | scope 제한, 핵심 우선 |
| **B** | 본 PRD 에 ColorBlindMode toggle 포함 | 부가 + scope ↑ |

## 3. 권장 설계 (옵션 B 일괄 채택, CLAUDE.md §15 정합)

### 3.1 신규 파일 (3개)

| 파일 | 책임 | 예상 라인 |
|---|---|---|
| `Scripts/Data/CinematicEffectsConfig.cs` | SO 정의 — 모든 magic number 외부화 + TierEffectProfile 매트릭스 | ~80 |
| `Scripts/Gameplay/Combat/HitEffectResolver.cs` | 정적 헬퍼 — (enemy, damage, isCrit, weaponCategory) → ResolvedEffect | ~70 |
| `Scripts/Editor/CinematicEffectsConfigSeed.cs` | Editor 메뉴 — default 값 시드 | ~50 |

### 3.2 수정 파일 (4개)

| 파일 | 변경 |
|---|---|
| `Scripts/Gameplay/Enemies/EnemyController.cs` | OnHit 시그니처 확장 (`weaponCategory` 추가) + HitEffectResolver 호출, hardcode → SO 참조 |
| `Scripts/Gameplay/Combat/DamageSystem.cs` | DealDamage 시그니처 확장 + WeaponCategory 전달 |
| `Scripts/Gameplay/Combat/DamageDealer.cs` | EnhanceDamage 호출 시 weaponCategory 전달 (각 IAttackPattern 의 무기 정보 확보) |
| `Scripts/Gameplay/Combat/CinematicEffects.cs` | PlayCameraZoom 신규 + 기존 PlayHitPause / PlayScreenShake → SO 파라미터 |

### 3.3 신규 데이터 구조

```csharp
[CreateAssetMenu(fileName = "CinematicEffectsConfig", menuName = "LittleHero/Combat/Cinematic Effects Config")]
public class CinematicEffectsConfig : ScriptableObject
{
    [Header("Punch (적 몸체 확대)")]
    public float normalPunchScale = 1.4f;
    public float critPunchScale   = 1.7f;
    public float punchUpDuration   = 0.05f;
    public float punchDownDuration = 0.10f;
    public float punchUpCritDuration   = 0.07f;
    public float punchDownCritDuration = 0.15f;

    [Header("Flash (색감)")]
    public Color normalFlashColor = Color.red;
    public Color critFlashColor   = Color.yellow;

    [Header("Hit Pause (Slow-Mo)")]
    public float slowMoScale    = 0.05f;
    public float slowMoDuration = 0.08f;

    [Header("Screen Shake")]
    public float shakeAmplitude = 0.3f;
    public float shakeDuration  = 0.2f;

    [Header("Camera Zoom (D7=B 신규)")]
    public bool  enableZoomOnCrit  = true;
    public float zoomMul           = 1.05f;
    public float zoomUpDuration    = 0.05f;
    public float zoomDownDuration  = 0.15f;

    [Header("VFX Hit Star")]
    public float vfxNormalScale = 0.6f;
    public float vfxCritScale   = 1.4f;

    [Header("Pool 크기 (D5=B 부족 해결)")]
    public int floaterPoolSize = 300;
    public int vfxPoolSize     = 100;

    [Header("Tier Profile (D2=B 적 티어 차별화)")]
    public TierEffectProfile noneProfile        = new() { punchMul=1.0f, vfxScaleMul=1.0f, floaterSizeMul=1.0f, shakeAmpMul=1.0f };
    public TierEffectProfile miniBossProfile    = new() { punchMul=1.10f, vfxScaleMul=1.20f, floaterSizeMul=1.15f, shakeAmpMul=1.20f };
    public TierEffectProfile chapterBossProfile = new() { punchMul=1.25f, vfxScaleMul=1.50f, floaterSizeMul=1.35f, shakeAmpMul=1.50f };
    public TierEffectProfile dailyBossProfile   = new() { punchMul=1.30f, vfxScaleMul=1.60f, floaterSizeMul=1.40f, shakeAmpMul=1.60f };
    public TierEffectProfile weeklyBossProfile  = new() { punchMul=1.40f, vfxScaleMul=1.80f, floaterSizeMul=1.50f, shakeAmpMul=1.80f };

    [Header("Damage Intensity (D3=B)")]
    public float damageIntensityCap = 5.0f;        // damage / avg cap (5× 이상 동일)
    public float damageIntensityWeight = 0.4f;     // intensity → punch+flash 영향력 (40%)

    [Header("Weapon Category Color (D4=B)")]
    public Color swordFlashColor    = Color.red;       // Sword/Axe/Mace/Spear
    public Color bowFlashColor      = new Color(0.4f, 0.9f, 1.0f);  // Bow/Crossbow
    public Color daggerFlashColor   = new Color(1.0f, 0.6f, 0.3f);  // Dagger/Boomerang (orange)
    public Color magicFlashColor    = new Color(0.8f, 0.4f, 1.0f);  // Staff/Wand/Tome (purple)

    [Header("Hit Stun Stacking (D6=C — 5008 후속 검토 가능)")]
    public float hitDebounceTime = 0.05f;          // 그 이내 재hit 무시
    public float hitStackPunchAdd = 0.0f;          // C=A+디바운스, stacking 0
}

[System.Serializable]
public struct TierEffectProfile
{
    public float punchMul;
    public float vfxScaleMul;
    public float floaterSizeMul;
    public float shakeAmpMul;
}
```

```csharp
public static class HitEffectResolver
{
    public static ResolvedHitEffect Resolve(
        CinematicEffectsConfig cfg,
        EnemyData enemyData,
        float damage,
        bool isCrit,
        WeaponCategory? weaponCategory = null,
        float? avgPlayerDamage = null)
    {
        // 1. Tier profile 선택
        var profile = enemyData.bossTier switch {
            BossTier.MiniBoss     => cfg.miniBossProfile,
            BossTier.ChapterBoss  => cfg.chapterBossProfile,
            BossTier.DailyBoss    => cfg.dailyBossProfile,
            BossTier.WeeklyBoss   => cfg.weeklyBossProfile,
            _                     => cfg.noneProfile
        };

        // 2. damage intensity (D3=B)
        float intensity = 0f;
        if (avgPlayerDamage.HasValue && avgPlayerDamage.Value > 0) {
            float ratio = damage / avgPlayerDamage.Value;
            intensity = Mathf.Clamp01(ratio / cfg.damageIntensityCap);
        }
        float intensityFactor = 1f + intensity * cfg.damageIntensityWeight;

        // 3. weapon flash color (D4=B)
        Color flashColor = isCrit ? cfg.critFlashColor :
            weaponCategory switch {
                WeaponCategory.Bow or WeaponCategory.Crossbow => cfg.bowFlashColor,
                WeaponCategory.Dagger or WeaponCategory.Boomerang => cfg.daggerFlashColor,
                WeaponCategory.Staff or WeaponCategory.Wand or WeaponCategory.Tome => cfg.magicFlashColor,
                _ => cfg.swordFlashColor
            };

        // 4. 최종 계산
        float basePunch = isCrit ? cfg.critPunchScale : cfg.normalPunchScale;
        return new ResolvedHitEffect {
            PunchScale     = basePunch * profile.punchMul * intensityFactor,
            FlashColor     = flashColor,
            VfxScale       = (isCrit ? cfg.vfxCritScale : cfg.vfxNormalScale) * profile.vfxScaleMul,
            FloaterSizeMul = profile.floaterSizeMul,
            ShakeAmplitude = cfg.shakeAmplitude * profile.shakeAmpMul * intensityFactor,
            EnableHitPause = isCrit,
            EnableZoom     = isCrit && cfg.enableZoomOnCrit
        };
    }
}

public struct ResolvedHitEffect
{
    public float PunchScale;
    public Color FlashColor;
    public float VfxScale;
    public float FloaterSizeMul;
    public float ShakeAmplitude;
    public bool  EnableHitPause;
    public bool  EnableZoom;
}
```

### 3.4 §15 OOP 정합 검증

| 정책 | 검증 |
|---|---|
| §15.1 SOLID-S | CinematicEffectsConfig (data) + HitEffectResolver (logic) + EnemyController (apply) 분리 ✅ |
| §15.1 SOLID-O | enum BossTier / WeaponCategory switch — 새 tier 추가 시 SO 필드만 (코드 X) ✅ |
| §15.4 SO 우선 | ✅ 모든 magic number → SO |
| §15.6 100L 룰 | 신규 파일 모두 100L 이내 (~80/~70/~50) ✅ |
| §15.7 DI | HitEffectResolver 정적 + Resolve 시 cfg 인자 (Singleton 직접 X) ✅ |
| §15.11 안티 패턴 | new()/string concat in OnHit X, struct ResolvedHitEffect (no boxing) ✅ |
| §15.12 Validator | (선택) `BossTier_HasEffectProfile` 룰 추가 가능 |

## 4. Sprint 분해 (개요)

자세한 세부는 PRD 참조.

| Sprint | 작업 | 의존성 |
|---|---|---|
| 1 | CinematicEffectsConfig SO + TierEffectProfile struct + Editor seed | — |
| 2 | HitEffectResolver 정적 헬퍼 + ResolvedHitEffect struct | 1 |
| 3 | DamageSystem / DamageDealer signature 확장 (WeaponCategory 전달) | — (병렬) |
| 4 | EnemyController.OnHit reflow (cfg 참조 + Resolver 호출) | 1, 2, 3 |
| 5 | CinematicEffects.PlayCameraZoom 신규 + PlayHitPause/Shake SO 화 | 1 |
| 6 | DamageFloaterPool prewarm 300 + VfxPool 100 + monitoring | 1 |
| 7 | NUnit HitEffectV2Tests 8+ + Validator 룰 (선택) | 1~6 |
| 8 | docs (changelog + CLAUDE.md §11/§12/§14) | 모두 |

## 5. 알려진 위험 + 완화

| 위험 | 영향 | 완화 |
|---|---|---|
| OnHit signature 확장 → 호출처 마이그레이션 | DamageSystem / 모든 IAttackPattern 8+ | optional 파라미터 (default null) — 점진 마이그레이션 |
| Tier profile 곱셈 누적 → 보스 punch 너무 큼 | Visual 과장 | weeklyBoss × intensity max = 1.4 × 1.4 = 1.96, cap 2.0× 추가 권장 |
| Pool prewarm 300 → 메모리 부담 | 30~50KB 메모리 | 모바일 OK (250MB 한계의 0.02%) |
| weapon_v2 WeaponCategory 11종 — Pet 카테고리 처리 | flash color 누락 | default = sword color (graceful fallback) |
| Cinemachine 미사용 — Camera.main 직접 zoom | Camera 변경 시 호환성 | CinematicEffects 가 Camera 참조 캐시 + null 체크 |
| 5006 UI 작업과 동시 진행 시 commit 충돌 | rebase 부담 | 5007 은 Combat 영역만 — 충돌 가능성 낮음 |

## 6. Definition of Done

- [ ] CinematicEffectsConfig SO 신규 (~80L) + 기본값 시드 (Editor 메뉴)
- [ ] HitEffectResolver 정적 (~70L) + ResolvedHitEffect struct
- [ ] DamageSystem.DealDamage / EnemyController.OnHit signature 확장 (WeaponCategory)
- [ ] BossTier 5종 (None/Mini/Chapter/Daily/Weekly) 별 punch/vfx/floater/shake 차별화 동작
- [ ] 무기 카테고리 4 그룹 (Sword/Bow/Dagger/Magic) 별 flash 색 차별화
- [ ] 데미지 intensity 동적 강도 (1×~1.4×) 적용
- [ ] DamageFloaterPool prewarm 300 + VfxPool 100
- [ ] CinematicEffects.PlayCameraZoom 신규 (크리 시 1.05× 0.08s)
- [ ] NUnit HitEffectV2Tests 8 PASS (Resolver tier/intensity/weapon 분기)
- [ ] ProjectValidator 신규 위반 0
- [ ] CLAUDE.md §11/§12/§14 갱신 + changelog 등재
- [ ] Play 검증 4 시나리오 통과

## 7. 변경 이력

- 2026-05-04: 초기 작성 (Phase 1 Generate). 옵션 D1=B / D2=B / D3=B / D4=B / D5=B / D6=C / D7=B / D8=A 권장.

## 8. 출처

- 5001 boss_system_v2 (BossTier enum 5종)
- weapon_v2 (`weapon_system_v2.md` — WeaponCategory 11종)
- PIVOT v4 cinematic_combat_v1.md (현재 OnHit 흐름)
- Damage v2 (DamageSystem / DamageDealer / DamageQuery)
- Explore agent 보고서 (2026-05-04, 9 효과 + 14 magic number + 풀 크기 분석)
- CLAUDE.md §2 PG-12 / §7 cap 300 + GC <50KB / §15 OOP 정책
