# 5029 Sprint PRD — mage_element_system

> **PRD v1 (초안)** · 디자인: `docs/prd/5029_mage_element_system_prd.md`
> 사용자 결정 채택: ❶=E + ❷=4종 + ❸=C + ❹=무료에셋 + ❺=Claude추천 + ❻=무기8매핑

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | MagicElement enum + 데이터 마이그레이션 | 1 | 45분 |
| 2 | ElementResolver + DamageDealer 통합 + OnHit | 1 | 50분 |
| 3a | Beam + Chain pattern (신규 2/4) | 1 | 40분 |
| 3b | Vortex + Meteor pattern (신규 2/4) + Factory 확장 | 1 | 40분 |
| 4 | 마법사 무기 8 element 매핑 + ElementVfxBank | 1 | 50분 |
| 5 | UI — Inventory + GachaResult element 아이콘 | 1 | 40분 |
| 6 | NUnit + Validator + changelog + CLAUDE.md | 1 | 30분 |
| **합계** | | **7** | **~4h 55m** |

---

## 2. Sprint 1 — MagicElement enum + 마이그레이션 (1 commit)

### 2.1 신규 파일

**파일**: `Scripts/Data/MagicElement.cs` (~15L)
```csharp
public enum MagicElement { None, Fire, Ice, Lightning, Arcane }
```

### 2.2 기존 파일 수정

**EquipmentTemplate.cs** +필드:
```csharp
public MagicElement element;  // default None (전사/궁수 호환)
```

**EnemyData.cs** +필드:
```csharp
public float[] elementResistance = { 1f, 1f, 1f, 1f };
// [0]=Fire / [1]=Ice / [2]=Lightning / [3]=Arcane
// 1.0=중립, 1.5=약점, 0.7=저항
```

### 2.3 Editor 마이그레이션 도구

**파일**: `Scripts/Editor/ElementMigrator.cs` (~50L)
- `Tools/LittleHero/Migrate Element System` 메뉴
- 262 EnemyData → elementResistance 기본값 `{1,1,1,1}` 자동 추가
- 24 EquipmentTemplate → element=None default 유지
- DisplayDialog 보고

### 2.4 검증
- 컴파일 errors=0
- 262 EnemyData 모두 elementResistance.Length == 4
- 24 EquipmentTemplate 변경 0 (element=None default)

#### Sprint 1 commit
```
5029 Sprint 1 — MagicElement enum + 마이그레이션 (1/7)

[변경]
  + Data/MagicElement.cs (~15L) enum (None+4)
  - Data/EquipmentTemplate.cs +element 필드 (default None)
  - Data/EnemyData.cs +elementResistance[4] (default {1,1,1,1})
  + Editor/ElementMigrator.cs (~50L)
    - Tools/LittleHero/Migrate Element System

[검증]
  - 262 EnemyData elementResistance.Length == 4
  - 전사/궁수 무기 element=None 호환

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 3. Sprint 2 — ElementResolver + DamageDealer (1 commit)

### 3.1 신규 파일

**`Scripts/Gameplay/Combat/ElementResolver.cs`** (~80L)
- `ResolveMultiplier(EnemyController, MagicElement) → float` (저항 곱셈)
- `ApplyElementEffect(EnemyController, MagicElement)`:
  - Fire → DOT 0.5s × 3 tick (10% baseDamage)
  - Ice → MoveSpeed × 0.5 for 0.5s
  - Lightning → 인접 적 1 추가 데미지 (50%)
  - Arcane → 저항 무시 (1.0× 강제)

### 3.2 기존 파일 수정

**DamageDealer.cs** — element 매개변수 추가:
```csharp
public static int DealDamage(..., MagicElement element = MagicElement.None) {
    float elemMul = ElementResolver.ResolveMultiplier(enemy, element);
    int finalDmg = (int)(baseDmg * elemMul);
    ElementResolver.ApplyElementEffect(enemy, element);
    // ... 기존 로직
}
```

**EnemyController.cs** — `OnHit` element-aware (이미 5007 HitEffectResolver 통합 → element 색조 추가)

### 3.3 검증
- 5007 HitEffectV2 8 NUnit 회귀 0
- 기존 DamageDealer 7 NUnit 회귀 0 (default None 호환)

#### Sprint 2 commit
```
5029 Sprint 2 — ElementResolver + DamageDealer 통합 (2/7)

[변경]
  + Gameplay/Combat/ElementResolver.cs (~80L)
    - ResolveMultiplier (저항 곱셈)
    - ApplyElementEffect (Fire DOT / Ice 둔화 / Lightning 사슬 / Arcane 무시)
  - Gameplay/Combat/DamageDealer.cs +element 매개변수 (default None)
  - Gameplay/Enemies/EnemyController.cs OnHit element-aware

[검증]
  - DamageDealer 7 NUnit 회귀 0
  - HitEffectV2 8 NUnit 회귀 0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. Sprint 3 — 신규 4 pattern (2 commit)

### 4.1 Sprint 3a — Beam + Chain (1 commit, 40분)

**파일 2 신규** (Patterns/ 하위):
- `BeamAttackPattern.cs` (~70L) — 직선 광선 N초 지속 + 라인 데미지 (Arcane)
- `ChainAttackPattern.cs` (~80L) — 첫 적 → 인접 N 적 자동 점프 (Lightning)

**`AttackPatternType` enum 확장**:
```csharp
public enum AttackPatternType {
    None, Projectile, Aura, Orbit, Rain, Trail, Pulse, Summon, Slash,
    Beam, Chain  // ← Sprint 3a
}
```

**`EquipmentPatternFactory.cs` 확장** — Beam/Chain switch case.

#### Sprint 3a commit
```
5029 Sprint 3a — Beam + Chain attack pattern (3/7)

[변경]
  + Patterns/BeamAttackPattern.cs (~70L) Arcane 직선 광선
  + Patterns/ChainAttackPattern.cs (~80L) Lightning 사슬
  - EquipmentData.cs AttackPatternType +Beam/Chain
  - EquipmentPatternFactory.cs +Beam/Chain case

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### 4.2 Sprint 3b — Vortex + Meteor (1 commit, 40분)

- `VortexAttackPattern.cs` (~75L) — 흡인 + 빙결 (Ice)
- `MeteorAttackPattern.cs` (~70L) — 운석 낙하 광역 (Fire, Rain 변형)

`AttackPatternType` 확장: `+ Vortex, Meteor`.

#### Sprint 3b commit
```
5029 Sprint 3b — Vortex + Meteor attack pattern (4/7)

[변경]
  + Patterns/VortexAttackPattern.cs (~75L) Ice 소용돌이
  + Patterns/MeteorAttackPattern.cs (~70L) Fire 운석
  - EquipmentData.cs AttackPatternType +Vortex/Meteor
  - EquipmentPatternFactory.cs +Vortex/Meteor case

[검증]
  - AttackPatternType 13개 (8 기존 + 4 신규 + None)
  - Factory 13개 case 모두 정합

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 5. Sprint 4 — 마법사 무기 8 → element 매핑 (1 commit, 50분)

### 5.1 신규 파일

**`Scripts/Editor/MageWeaponElementMigrator.cs`** (~50L)
- `Tools/LittleHero/Migrate Mage Weapon Elements` 메뉴
- 매핑:
  - Fire: weapon_firebottle, weapon_meteor_rod
  - Ice: weapon_frost_rune, mage_glacier_rune
  - Lightning: weapon_annihilation_rain
  - Arcane: weapon_rune_staff, weapon_spirit_pet, weapon_time_spirit

**`Scripts/Core/ElementVfxBank.cs`** (~45L)
- 4 element VFX prefab (Resources/VFX/Element/Fire 등)
- VfxPool 등록 + Spawn(MagicElement, position)

### 5.2 VFX prefab 4 (placeholder 자체 합성, 무료 에셋 후속)

**임시**: 5007 HitEffectResolver 답습 + element 색조 (Fire 빨강 / Ice 파랑 / Lightning 노랑 / Arcane 보라).

#### Sprint 4 commit
```
5029 Sprint 4 — 마법사 무기 8 element + ElementVfxBank (5/7)

[변경]
  + Editor/MageWeaponElementMigrator.cs (~50L)
    - 8 무기 element 일괄 매핑
  + Core/ElementVfxBank.cs (~45L)
    - 4 element VFX prefab + VfxPool 등록
  + Resources/VFX/Element/{Fire,Ice,Lightning,Arcane} prefab placeholder

[데이터]
  - Fire: weapon_firebottle, weapon_meteor_rod
  - Ice: weapon_frost_rune, mage_glacier_rune
  - Lightning: weapon_annihilation_rain
  - Arcane: weapon_rune_staff, weapon_spirit_pet, weapon_time_spirit

[Trade-off]
  Lightning 1개 부족 — 5029.1 후속 무기 추가 또는 그대로

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 6. Sprint 5 — UI element 아이콘 (1 commit, 40분)

### 6.1 InventoryPanel + GachaResultPanel

- 무기 카드 우측 상단 element 아이콘 표시 (Fire 빨강 / Ice 파랑 / Lightning 노랑 / Arcane 보라)
- element=None 시 아이콘 숨김 (전사/궁수 호환)

### 6.2 신규 파일

**`Scripts/UI/Element/ElementIcon.cs`** (~30L) — 작은 아이콘 컴포넌트, color + sprite 분기

#### Sprint 5 commit
```
5029 Sprint 5 — UI element 아이콘 (6/7)

[변경]
  + UI/Element/ElementIcon.cs (~30L)
  - UI/InventoryGridCard.cs +element icon wiring
  - UI/GachaResultPanel.cs +element icon wiring
  + 4 element sprite (placeholder, Resources/UI/Element/)

[검증]
  - 마법사 무기 element 아이콘 표시
  - 전사/궁수 무기 element=None 시 숨김

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 7. Sprint 6 — NUnit + Validator + docs (1 commit, 30분)

### 7.1 NUnit 신규

**`Tests/Editor/ElementResolverTests.cs`** (~60L, NUnit 6+)
- `ResolveMultiplier_Neutral_Returns1`
- `ResolveMultiplier_Weakness_Returns1_5`
- `ResolveMultiplier_Resist_Returns0_7`
- `ResolveMultiplier_Arcane_IgnoresResistance`
- `ApplyElementEffect_Fire_AppliesDOT`
- `ApplyElementEffect_Ice_SlowsMovement`

NUnit 누적: 158 → 164 (+6).

### 7.2 Validator 룰

**Editor/ProjectValidator/Rules/MageWeaponRule.cs** 또는 기존 Rule 확장:
- 마법사 무기 8개 element != None 검증
- 4 element 분포 보고

### 7.3 docs

- `changelog_v1.md` +5029 항목
- CLAUDE.md §14 직전 5건 갱신

#### Sprint 6 commit
```
5029 Sprint 6 — NUnit 6 + Validator + docs (7/7)

[변경]
  + Tests/Editor/ElementResolverTests.cs (~60L, 6 NUnit)
  - Editor/ProjectValidator/Rules/MageWeaponRule.cs 또는 기존 확장
  - changelog_v1.md +5029
  - CLAUDE.md §14 갱신

[NUnit 누적] 158 → 164

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 8. 의존성 그래프

```
Sprint 1 (enum + 마이그레이션)
   ↓
Sprint 2 (ElementResolver + DamageDealer)
   ↓
Sprint 3a (Beam + Chain) → Sprint 3b (Vortex + Meteor)
   ↓
Sprint 4 (무기 매핑 + VFX) → Sprint 5 (UI)
   ↓
Sprint 6 (NUnit + docs)
```

병렬 가능: Sprint 5 + Sprint 6 (Sprint 4 후 독립).

## 9. 완료 정의 (DoD)

- [ ] MagicElement enum 5종 + EquipmentTemplate.element + EnemyData.elementResistance[4]
- [ ] 262 EnemyData 마이그레이션 완료
- [ ] ElementResolver 4 효과 (Fire DOT / Ice 둔화 / Lightning 사슬 / Arcane 무시)
- [ ] 신규 4 pattern (Beam/Chain/Vortex/Meteor) + Factory 13개 정합
- [ ] 마법사 무기 8 element 매핑 (Fire 2 / Ice 2 / Lightning 1 / Arcane 3)
- [ ] ElementVfxBank + 4 prefab 등록
- [ ] InventoryPanel + GachaResultPanel element 아이콘
- [ ] NUnit 158 → 164 회귀 0
- [ ] Validator warnings 변동 ≤ 0
- [ ] changelog + CLAUDE.md

## 10. 검증 시나리오

### 시나리오 1 — Element 마이그레이션
`Tools/LittleHero/Migrate Element System` → 262 EnemyData 갱신 + DisplayDialog 보고

### 시나리오 2 — 마법사 무기 매핑
`Tools/LittleHero/Migrate Mage Weapon Elements` → 8 무기 element 부여 + 분포 보고 (Fire 2 / Ice 2 / Lightning 1 / Arcane 3)

### 시나리오 3 — Play 마법사 전투
- 마법사 캐릭터 선택 → CH1 진입
- 마법사 무기 (예 weapon_firebottle Fire) 활성화
- 적 처치 시 Fire DOT 발동 + 콘솔 로그 `[ElementResolver] Fire DOT applied`

### 시나리오 4 — UI 검증
- Inventory → 마법사 무기 카드 element 아이콘 표시
- GachaResult → 신규 무기 element 아이콘 표시

### 시나리오 5 — Validator
Validate Project → warnings 변동 ≤ 0 + 마법사 무기 element 분포 보고

## 11. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 기존 8 pattern 회귀 (element 매개변수) | default MagicElement.None — 전사/궁수 호환 |
| Lightning 무기 1개 (불균등) | 5029.1 후속 보정 또는 운영 결정 |
| EnemyData 262 마이그레이션 데이터 손상 | Editor 메뉴 + DisplayDialog 사전 확인 |
| EquipmentManager.SyncEquippedInstances | 5028 PatternActivator 답습, element 매개변수 명시 전달 |
| InventoryPanel UI wiring 회귀 | element=None 시 아이콘 숨김 (호환) |
| 무료 VFX 에셋 결정 보류 | Sprint 4 placeholder 자체 합성 + 5029.2 후속 |

## 12. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 사용자 6 결정 채택, 7 Sprint / ~4h55m |
