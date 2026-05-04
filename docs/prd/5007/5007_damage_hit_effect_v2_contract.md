# Sprint Contract: 5007 damage_hit_effect_v2

> **PGE Planner — Phase 4 Contract**
> v2 PRD: `_v2_prd.md` (mediator 결정 D1=B/D2=B/D3=A/D4=B 반영) · breakdown: `_breakdown_00.md`
> 생성일: 2026-05-04

## 1. C1 컴파일 기준

- **C1.1**: 신규 .cs 4개 (Config / Resolver / Loader / Tests) errors=0
- **C1.2**: DamageSystem / DamageDealer / 8 IAttackPattern / EnemyController / CinematicEffects / DamageFloaterPool / VfxPool 수정 errors=0
- **C1.3**: 12 commit 모두 errors=0

## 2. C2 Editor Test (NUnit)

| Test | 메서드 |
|---|---|
| `HitEffectV2Tests` | 8 |

### 신규 NUnit

- `CinematicEffectsConfig_Type_Exists`
- `HitEffectResolver_Type_Exists`
- `ResolvedHitEffect_IsStruct_NoBoxing`
- `HitEffectResolver_NoneTier_NormalHit_ReturnsBasePunch_1_4`
- `HitEffectResolver_ChapterBoss_AmplifiedPunch_GreaterThanBase`
- `HitEffectResolver_DamageIntensity_5xRatio_FullIntensity`
- `HitEffectResolver_BowWeapon_FlashColor_Cyan`
- `HitEffectResolver_NullWeapon_FlashColor_Sword_Default`

**검증**: 8/8 PASS, 누적 73 → 81

## 3. C3 ProjectValidator

- errors=0 / warnings 변동 ±0
- §15.6 통과: 신규 모두 100L 이내 (~80/~70/~50/~20)
- (선택) `BossTier_HasEffectProfile` 룰 추가 가능

## 4. C4 Reflection 검증

| Type | 검증 |
|---|---|
| `LittleHero.CinematicEffectsConfig` | SO 클래스 + 25+ SerializeField |
| `LittleHero.TierEffectProfile` | struct + 4 float field |
| `LittleHero.HitEffectResolver` | static class + Resolve 메서드 |
| `LittleHero.ResolvedHitEffect` | struct (값 타입) + 7 public field |
| `LittleHero.CinematicEffectsConfigLoader` | static + Get() + RuntimeInitializeOnLoadMethod |
| `LittleHero.DamageSystem.DealDamage` | WeaponCategory? 인자 |
| `LittleHero.EnemyController.OnHit` | WeaponCategory? + avgPlayerDamage 인자 |
| `LittleHero.CinematicEffects.PlayCameraZoom` | 신규 메서드 |

## 5. C5 SO 검증

- `Assets/_Project/Resources/Combat/CinematicEffectsConfig.asset` 1개 생성
- 5 BossTier profile 모두 default 값 (None×1.0 / Mini×1.10 / Chapter×1.25 / Daily×1.30 / Weekly×1.40)
- 25+ SerializeField 모두 default 값 적용

## 6. C6 Play 시나리오

### S1 — 적 티어 차별화 (D2)
1. CH_01 wave 1 — 일반 적 피격: punch ~1.4×
2. CH_01 wave 5 — 미니보스 피격: punch ~1.54× (1.4 × 1.10)
3. CH_01 wave 20 — 챕터보스 피격: punch ~1.75×+ (1.4 × 1.25)
- **검증**: 시각적 차이 명확

### S2 — 데미지 크기 동적 강도 (D3)
1. 강화 +0 무기 — 일반 데미지, intensity ≈ 0
2. 강화 +15 무기 — 데미지 5× cap, intensity = 1.0
- **검증**: punch / shake 강도 ↑

### S3 — 무기 종류 flash 색 (D4)
1. Sword 장착 → Red flash
2. Bow 장착 → Cyan flash
3. Staff 장착 → Purple flash
- **검증**: 무기 카테고리 변경 시 flash 색 즉시 변경

### S4 — Pool 부족 해결 (D5)
1. Cap 300 환경 wave 진행
2. Profiler 모니터링
- **검증**: GC <50KB/s, Debug.LogWarning 0건

## 7. 비기능

- 컴파일 errors=0 (12 commit)
- NUnit 73 → 81 PASS
- ProjectValidator warnings 변동 0
- 5001~5006 회귀 X (Combat 영역만 변경)
- 60fps 유지 (cap 300, Profiler 권장)

## 8. 합격 기준

- 최소: 자동 (C1~C5) + S1, S2 PASS
- 완전: 자동 (C1~C5) + S1~S4 모두 PASS

## 9. Trade-off (mediator 결정 기반)

- **D1=B**: Sprint 3 = 3 commit (인프라+8AttackPattern). 회수성 ↑
- **D2=B**: avgDamage DamageSystem 에서 query → OnHit 인자 전달. EnemyController 결합도 ↓
- **D3=A**: Resources.Load 정적 cache. 137 prefab wiring 부담 X
- **D4=B**: Resolver 내부 cap 2.0× hardcode. SO 잘못된 값 방어

## 10. 변경 이력

- 2026-05-04: 초기 작성 (Phase 4 Contract).
