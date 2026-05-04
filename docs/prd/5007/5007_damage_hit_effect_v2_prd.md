# 5007 — Damage Hit Effect v2 — Sprint PRD

> **출처**: `docs/prd/5007_damage_hit_effect_v2.md` (디자인) — 옵션 D1~D8 권장 채택 가정
> **PGE Phase 2 review 가능성 알림**: 본 PRD 는 v1. `/prd-pipeline 5007` 호출 시 critique → rebuttal → mediator 자동.

## 1. 전체 개요

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | CinematicEffectsConfig SO + TierEffectProfile struct + Editor seed | 2 | 60분 |
| 2 | HitEffectResolver 정적 + ResolvedHitEffect struct | 1 | 40분 |
| 3 | DamageSystem / DamageDealer signature 확장 (WeaponCategory) | 2 | 50분 |
| 4 | EnemyController.OnHit reflow (cfg 참조 + Resolver 호출) | 2 | 60분 |
| 5 | CinematicEffects.PlayCameraZoom 신규 + 기존 메서드 SO 화 | 1 | 30분 |
| 6 | DamageFloaterPool prewarm 300 + VfxPool 100 + monitoring | 1 | 30분 |
| 7 | NUnit HitEffectV2Tests 8 + Validator 룰 | 1 | 45분 |
| 8 | docs (changelog + CLAUDE.md §11/§12/§14) | 1 | 25분 |
| **총** | | **11** | **5시간 40분** |

## 2. Sprint 명세

### Sprint 1 — CinematicEffectsConfig SO (2 commit, 60분)

#### 1.1 CinematicEffectsConfig.cs + TierEffectProfile struct

**파일**: `Assets/_Project/Scripts/Data/CinematicEffectsConfig.cs`

**산출물**:
- `[CreateAssetMenu]` SO 클래스 (~80L)
- 7 Header 그룹: Punch / Flash / HitPause / ScreenShake / Zoom / VFX / Pool / Tier / DamageIntensity / WeaponColor
- `TierEffectProfile` struct (4 float: punchMul / vfxScaleMul / floaterSizeMul / shakeAmpMul)
- 5 BossTier 별 default profile (None/Mini/Chapter/Daily/Weekly)

**검증 기준**:
- 컴파일 errors=0
- §15.6 100L 통과 (~80L)
- Inspector 에서 모든 필드 노출

**완료 조건**:
- `CinematicEffectsConfig.asset` 1개 생성 (`Data/Combat/CinematicEffectsConfig.asset`)
- 모든 필드 default 값 확인

#### Sprint 1.1 commit

```
5007 Sprint 1.1 / T01 — CinematicEffectsConfig SO + TierEffectProfile (1/11)

[변경]
- Scripts/Data/CinematicEffectsConfig.cs 신규 (~80L)
  - 7 Header 그룹, 25 SerializeField
  - TierEffectProfile struct (BossTier × 4 multiplier)

[검증]
- 컴파일 errors=0
- §15.4 SO 우선 + §15.6 100L 통과

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

#### 1.2 Editor 시드 메뉴

**파일**: `Assets/_Project/Scripts/Editor/CinematicEffectsConfigSeed.cs`

**산출물**:
- 메뉴 `Tools > LittleHero > Seed CinematicEffectsConfig`
- `Data/Combat/CinematicEffectsConfig.asset` 자동 생성 + 디자인 §3.3 default 값 적용
- 기존 인스턴스 있으면 보존 (idempotent)

#### Sprint 1.2 commit

```
5007 Sprint 1.2 / T02 — CinematicEffectsConfig 시드 + asset 생성 (2/11)

[변경]
- Scripts/Editor/CinematicEffectsConfigSeed.cs 신규 (~50L)
- Data/Combat/CinematicEffectsConfig.asset 신규 (default 값)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 2 — HitEffectResolver 정적 헬퍼 (1 commit, 40분)

#### 2.1 HitEffectResolver.cs

**파일**: `Assets/_Project/Scripts/Gameplay/Combat/HitEffectResolver.cs`

**산출물**:
- 정적 클래스 (`public static class HitEffectResolver`) ~70L
- `Resolve(cfg, enemyData, damage, isCrit, weaponCategory?, avgDamage?)` → `ResolvedHitEffect`
- `ResolvedHitEffect` struct (PunchScale / FlashColor / VfxScale / FloaterSizeMul / ShakeAmplitude / EnableHitPause / EnableZoom)
- BossTier switch (5종) + WeaponCategory switch (4 그룹: Sword/Bow/Dagger/Magic) + intensity 계산

**검증 기준**:
- 컴파일 errors=0
- §15.6 100L 통과
- `Resolve(...)` 호출 시 0 GC (struct 반환, no allocation)

#### Sprint 2 commit

```
5007 Sprint 2 / T03 — HitEffectResolver 정적 헬퍼 + ResolvedHitEffect struct (3/11)

[변경]
- Scripts/Gameplay/Combat/HitEffectResolver.cs 신규 (~70L)
  - Resolve() 정적 메서드 (BossTier 5 × WeaponCategory 4 그룹 × intensity)
  - ResolvedHitEffect struct (no boxing)

[검증]
- 컴파일 errors=0
- 0 GC (struct return)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 3 — DamageSystem / DamageDealer 시그니처 확장 (2 commit, 50분)

#### 3.1 DamageSystem.DealDamage 시그니처 확장

**파일**: `Assets/_Project/Scripts/Gameplay/Combat/DamageSystem.cs`

**변경**:
- `DealDamage(EnemyController enemy, float damage)` → `DealDamage(EnemyController enemy, float damage, WeaponCategory? weaponCategory = null)`
- enemy.OnHit 호출 시 weaponCategory 전달

**검증 기준**:
- 컴파일 errors=0
- 기존 호출처 자동 호환 (default null)

#### Sprint 3.1 commit

```
5007 Sprint 3.1 / T04 — DamageSystem.DealDamage WeaponCategory 인자 추가 (4/11)

[변경]
- Scripts/Gameplay/Combat/DamageSystem.cs — DealDamage 시그니처 확장
  - WeaponCategory? weaponCategory = null (optional)
  - enemy.OnHit(damage, isCrit, weaponCategory) 호출

[검증]
- 컴파일 errors=0 (기존 호출처 default null 자동 호환)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

#### 3.2 DamageDealer / IAttackPattern 마이그레이션

**파일**:
- `Scripts/Gameplay/Combat/DamageDealer.cs`
- `Scripts/Gameplay/Equipment/EquipmentManager.cs` (현재 장착 무기 카테고리 노출)
- 8 IAttackPattern 구현체 (Projectile/Aura/Orbit/...) — 장착 무기 카테고리 전달

**변경**:
- `DamageDealer.ApplyMulti / ApplySingle` 시그니처 확장
- 8 IAttackPattern: `EquipmentManager.GetActiveWeaponCategory()` 호출 후 DamageDealer 에 전달

#### Sprint 3.2 commit

```
5007 Sprint 3.2 / T05 — DamageDealer + 8 IAttackPattern WeaponCategory 전달 (5/11)

[변경]
- Scripts/Gameplay/Combat/DamageDealer.cs — ApplyMulti/ApplySingle WeaponCategory 인자
- Scripts/Gameplay/Equipment/EquipmentManager.cs — GetActiveWeaponCategory() public API
- 8 IAttackPattern 구현체 — DamageDealer 호출 시 weaponCategory 전달

[검증]
- 컴파일 errors=0
- 무기 직업 검증 회귀 X (weapon_v2)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 4 — EnemyController.OnHit reflow (2 commit, 60분)

#### 4.1 OnHit 시그니처 + Resolver 호출

**파일**: `Assets/_Project/Scripts/Gameplay/Enemies/EnemyController.cs`

**변경**:
- `OnHit(float damage, bool isCrit)` → `OnHit(float damage, bool isCrit, WeaponCategory? weaponCategory = null)`
- HitEffectResolver.Resolve 호출 → ResolvedHitEffect 획득
- 모든 hardcode 값 (1.4f / 1.7f / Color.red / 0.05f / etc.) 제거 → resolved 또는 cfg 참조
- `_config` SerializeField 신규 (CinematicEffectsConfig)

**검증 기준**:
- 컴파일 errors=0
- 기존 일반 적 피격 동작 유지 (default Tier=None × intensity=0 → 동일 효과)
- 보스 피격 시 punch / shake amplitude 시각 차이 확인

#### Sprint 4.1 commit

```
5007 Sprint 4.1 / T06 — EnemyController.OnHit reflow (Resolver + cfg 참조) (6/11)

[변경]
- Scripts/Gameplay/Enemies/EnemyController.cs
  - OnHit signature 확장 + WeaponCategory 인자
  - HitEffectResolver.Resolve 호출 → ResolvedHitEffect
  - 모든 hardcode 제거 → cfg / resolved 참조
  - _config SerializeField (CinematicEffectsConfig)

[검증]
- 일반 적 피격 회귀 X
- 보스 피격 시 punch / shake 차별화 ✅

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

#### 4.2 Enemy 프리팹 wiring + DamageNumberFloater size 차별화

**작업**:
- Resources / Prefabs 의 적 prefab 들에 CinematicEffectsConfig wiring (또는 정적 lookup)
- DamageNumberFloater.Initialize 시 `floaterSizeMul` 인자 추가 → fontSize 동적

**검증 기준**:
- 보스 데미지 floater 가 일반 적 대비 ~1.35× 크게 표시

#### Sprint 4.2 commit

```
5007 Sprint 4.2 / T07 — DamageNumberFloater fontSize 동적 + 적 prefab wiring (7/11)

[변경]
- Scripts/Gameplay/Combat/DamageNumberFloater.cs — Initialize 에 floaterSizeMul
- 적 Prefab — CinematicEffectsConfig 정적 로딩 (Resources)

[검증]
- 보스 floater ~1.35× 크게 표시

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 5 — CinematicEffects 확장 (1 commit, 30분)

#### 5.1 PlayCameraZoom + SO 파라미터

**파일**: `Assets/_Project/Scripts/Gameplay/Combat/CinematicEffects.cs`

**변경**:
- `PlayCameraZoom(float targetZoomMul, float upDuration, float downDuration)` 신규 메서드 (~20L)
- `PlayHitPause / PlayScreenShake` 의 default 값 → SO 참조 (또는 인자만 받게 변경)
- Camera.main null 체크 + cache (`_camera` 정적 필드)

**검증 기준**:
- 크리 시 zoom 1.0 → 1.05 → 1.0 동작 (Camera.orthographicSize 변경)
- 멀미 우려 X (subtle, 0.05× 만)

#### Sprint 5 commit

```
5007 Sprint 5 / T08 — CinematicEffects PlayCameraZoom + SO 화 (8/11)

[변경]
- Scripts/Gameplay/Combat/CinematicEffects.cs
  - PlayCameraZoom 신규 (~20L, 크리 시 1.05× 0.08s)
  - PlayHitPause / PlayScreenShake → SO 파라미터

[검증]
- 컴파일 errors=0
- Camera.main null 안전

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 6 — Pool 크기 + monitoring (1 commit, 30분)

#### 6.1 DamageFloaterPool prewarm + monitoring

**파일**:
- `Scripts/Gameplay/Combat/DamageFloaterPool.cs`
- `Scripts/Core/VfxPool.cs`

**변경**:
- `DamageFloaterPool.Prewarm(int count)` 신규 (cfg.floaterPoolSize 적용)
- `VfxPool.Prewarm(int count)`
- `_peakActiveCount` 트래킹 + Debug.LogWarning when > 80% capacity
- GameManager.Initialize 또는 Attic.OnEnable 에서 Prewarm 호출

**검증 기준**:
- Play 모드 진입 시 300 floater + 100 vfx 인스턴스 사전 할당 확인
- Cap 300 적 동시 피격 시 GC 0 (allocation 발생 안 함)

#### Sprint 6 commit

```
5007 Sprint 6 / T09 — Pool prewarm 300/100 + peak monitoring (9/11)

[변경]
- Scripts/Gameplay/Combat/DamageFloaterPool.cs — Prewarm + monitoring
- Scripts/Core/VfxPool.cs — Prewarm
- GameManager.cs — Initialize 시 Prewarm 호출

[검증]
- Play 진입 시 300 floater 사전 할당
- cap 300 환경 GC 0 (Profiler 권장)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 7 — NUnit + Validator (1 commit, 45분)

#### 7.1 HitEffectV2Tests.cs

**파일**: `Assets/_Project/Tests/Editor/HitEffectV2Tests.cs`

**산출물**:
- `HitEffectResolver_NoneTier_Damage100_Crit_ReturnsBasePunch_1_7x`
- `HitEffectResolver_ChapterBoss_AmplifiedPunch_2_125x` (1.7 × 1.25)
- `HitEffectResolver_DamageIntensity_5xRatio_FullIntensity` (intensity = 1.0)
- `HitEffectResolver_BowWeapon_FlashColor_Cyan`
- `HitEffectResolver_MagicWeapon_FlashColor_Purple`
- `HitEffectResolver_NullWeapon_FlashColor_Sword_Default`
- `CinematicEffectsConfig_Type_Exists`
- `ResolvedHitEffect_IsValueType_NoBoxing` (struct)

**검증 기준**:
- 8/8 PASS
- 누적 NUnit 73 → 81

#### Sprint 7 commit

```
5007 Sprint 7 / T10 — HitEffectV2Tests 8 NUnit + Validator (10/11)

[변경]
- Tests/Editor/HitEffectV2Tests.cs 신규 (8 NUnit)
- (선택) ProjectValidator BossTier_HasEffectProfile 룰

[검증]
- Test Runner 8/8 PASS
- 누적 73 → 81

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 8 — docs (1 commit, 25분)

**작업**:
- `docs/little_hero/changelog_v1.md` — 5007 항목 추가 (11 commit 묶음)
- `CLAUDE.md §11` — 마일스톤 row 추가
- `CLAUDE.md §12` — Combat 시스템 갱신 (CinematicEffectsConfig + HitEffectResolver) + Tests 73→81
- `CLAUDE.md §14` — 최근 5건 회전

#### Sprint 8 commit

```
5007 Sprint 8 / T11 — docs (changelog + CLAUDE.md §11/§12/§14) (11/11)

[변경]
- docs/little_hero/changelog_v1.md — 5007 damage_hit_effect_v2 항목
- CLAUDE.md §11 — 마일스톤 row
- CLAUDE.md §12 — Combat + Tests 73→81
- CLAUDE.md §14 — 최근 5건 회전 (5007 등재)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
Sprint 1 (Config SO) ─┬→ Sprint 2 (Resolver) ────────┐
                       │                              │
                       └→ Sprint 5 (CamZoom + SO 화) │
                       └→ Sprint 6 (Pool prewarm)    │
                                                      │
Sprint 3 (DealDamage signature) ──────────────────────┤
                                                      ↓
                                              Sprint 4 (OnHit reflow)
                                                      ↓
                                              Sprint 7 (NUnit + Validator) → Sprint 8 (docs)
```

**병렬 가능**:
- Sprint 1 후 — Sprint 2, 5, 6 병렬
- Sprint 3 — Sprint 1 과 병렬 (signature 확장만)
- Sprint 4 — 1+2+3 모두 완료 후

## 4. 검증 시나리오 (Play 검증)

### S1 — 적 티어 차별화 (D2)

**사전 조건**: 챕터 1 진입 (Meta 카테고리, 일반 적)

**단계**:
1. 일반 적 (HP 30) 피격 — punch 1.4×, shake 발동 X
2. wave 5 미니보스 (HP 200) 피격 — punch ~1.5× (1.4 × 1.10)
3. wave 20 챕터 보스 피격 — punch ~1.75× (1.4 × 1.25)

**기대 결과**:
- 시각적으로 보스 피격이 일반 적 대비 더 강한 punch / vfx scale 인지 확인
- 콘솔 로그 (선택): `[HitEffect] Tier=ChapterBoss × intensity=0.4`

### S2 — 데미지 크기 동적 강도 (D3)

**사전 조건**: 강화 +15 무기 + 크리 발동

**단계**:
1. 일반 데미지 (1× avgDamage) — intensity 0.2 × weight 0.4 = factor 1.08×
2. 큰 데미지 (5× avgDamage = cap) — intensity 1.0 × weight 0.4 = factor 1.4×
3. 두 케이스 비교

**기대 결과**:
- 큰 데미지일수록 punch / shake 강도 ↑ (직관적 임팩트)

### S3 — 무기 종류별 flash 색상 (D4)

**사전 조건**: 4 무기 카테고리 별 가챠 / 장착

**단계**:
1. Sword (검) 장착 → 일반 hit flash = Red ✅
2. Bow (활) 장착 → 일반 hit flash = Cyan
3. Dagger (단검) 장착 → 일반 hit flash = Orange
4. Staff (지팡이) 장착 → 일반 hit flash = Purple
5. 크리 발동 시 모두 Yellow (cfg.critFlashColor)

**기대 결과**:
- 무기 카테고리 변경 시 flash 색 즉시 변경

### S4 — Pool 부족 해결 (D5)

**사전 조건**: chapter 진행 중 동시 적 cap 300 환경

**단계**:
1. wave 진행 중 일정 시간 대기
2. Profiler 또는 Editor 콘솔 모니터링

**기대 결과**:
- DamageFloaterPool 활성 60+ 인스턴스 정상 동작 (50 한계 초과 X)
- GC Allocation/sec < 50KB 유지 (CLAUDE.md §7)
- Debug.LogWarning ("pool exhaustion") 발생 X

## 5. Definition of Done

- [ ] Sprint 1~8 모두 commit (11 commit)
- [ ] CinematicEffectsConfig.asset 생성 + 모든 default 적용
- [ ] HitEffectResolver static (~70L) + ResolvedHitEffect struct (no boxing)
- [ ] DamageSystem / DamageDealer / 8 IAttackPattern signature 확장
- [ ] EnemyController.OnHit hardcode 0 (모두 SO/resolved 참조)
- [ ] DamageFloaterPool prewarm 300 + VfxPool 100
- [ ] CinematicEffects.PlayCameraZoom 신규
- [ ] NUnit HitEffectV2Tests 8 PASS (누적 73 → 81)
- [ ] CLAUDE.md §11/§12/§14 갱신 + changelog 등재
- [ ] Play 시나리오 S1~S4 통과
- [ ] §15 OOP 정합 검증 통과

## 6. 알려진 위험 + 완화

| 위험 | Sprint | 완화 |
|---|---|---|
| Tier punch × intensity 누적 → 보스 punch 너무 큼 (max ~2.4×) | 4 | cap 2.0× 적용 (Resolver 내 Mathf.Min) |
| OnHit signature 확장 → 호출처 마이그레이션 누락 | 3, 4 | optional 인자 (default null) |
| Pool prewarm 300 → 메모리 ~30KB | 6 | 모바일 OK (250MB 한계 대비 무시) |
| Camera.main null (장면 전환 중) | 5 | _camera cache + null 체크 |
| weapon_v2 None 카테고리 | 4 | default = sword color (graceful) |
| 5006 UI commit 충돌 | 모두 | 5007 = Combat 영역만 — 충돌 0 |

## 7. 변경 이력

- 2026-05-04: 초기 작성 (Phase 1 — 디자인 옵션 권장 D1=B/D2=B/D3=B/D4=B/D5=B/D6=C/D7=B/D8=A)
