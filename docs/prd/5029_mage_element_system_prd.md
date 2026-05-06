# 5029 mage_element_system — 디자인 문서

> **목적**: 마법사 직업 다양화 — Element 4종 + 신규 attack pattern 4 + 적 저항 시스템
> **상태**: PRD (디자인 + Sprint)
> **시작**: 2026-05-06

## 0. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | 사용자 6 결정 채택: ❶=E (A+B 통합) / ❷=4종 / ❸=C 마법사 우선 + 5030 후속 / ❹=무료 에셋 + 자체 / ❺=Claude 추천 (C+B+4 prefab+B float[4]) / ❻=무기 8→element 매핑 |
| 2026-05-06 | `mage_diversity_design_questions.md` 결정 가이드 답습 |

## 1. 현재 시스템 분석

### 1.1 마법사 무기 8개 + 신규 1개 (weapon_v2 정합)

**기존 8개**:
| 무기 | 카테고리 | element |
|---|---|---|
| weapon_firebottle | Tome | **Fire** (불병) |
| weapon_meteor_rod | Wand | **Fire** (운석) |
| weapon_frost_rune | Tome | **Ice** (서리 룬) |
| mage_glacier_rune | Tome | **Ice** (빙하 룬) |
| weapon_annihilation_rain | Tome | **Lightning** (소멸의 비) |
| weapon_rune_staff | Staff | **Arcane** (룬 지팡이) |
| weapon_spirit_pet | Tome | **Arcane** (정령 펫) |
| weapon_time_spirit | Tome | **Arcane** (시간 정령) |

**신규 1개 (사용자 결정 — 옵션 B)**:
| 무기 | 카테고리 | element | 신규 |
|---|---|---|---|
| **weapon_thunder_wand** | **Wand** | **Lightning** (천둥 완드) | ★ |

**최종 분포**: Fire 2 / Ice 2 / Lightning **2** / Arcane 3 = **9 무기** (균등에 근접)

**카테고리 균형**: Tome 6 + Wand **2** + Staff 1 (Wand 추가로 Tome 편중 일부 완화)

**영향**:
- EquipmentTemplate SO 24 → **25**
- GachaPool 등급 풀 1개 추가 (Rare 또는 Epic 등급 권장, 가챠 분포 균형 검토 필요)

### 1.2 기존 8 IAttackPattern

`AttackPatternType { None, Projectile, Aura, Orbit, Rain, Trail, Pulse, Summon, Slash }` (EquipmentData.cs:8).

**현재 element 시스템 없음** — 모든 pattern element-agnostic.

### 1.3 EnemyData

262 SO 모두 elementResistance 필드 미존재. 마이그레이션 시 default `{1,1,1,1}` (중립) 추가.

## 2. 옵션 비교 (사용자 결정 후 정합)

| 결정 | 채택 | 사유 |
|---|---|---|
| ❶ 방향성 | **E** A+B 통합 | 신규 pattern 4 + Element 4 = 마법사 다양화 |
| ❷ Element | **4종** Fire/Ice/Lightning/Arcane | VFX Pack 정합 |
| ❸ 범위 | **C** 마법사 우선 + 5030 후속 | Sprint 작게, 균형 보장 |
| ❹ VFX | **무료 에셋 + 자체 합성** | 비용 0 + PG-12 톤 정밀 제어 |
| ❺.1 Element 위치 | **C** 둘 다 | 무기 부여 + 적 저항 (전략적 깊이) |
| ❺.2 조합 모델 | **B** Strategy (IAttackPattern 매개변수) | 코드 8+4 = 12 (Cartesian 32 폭증 회피) |
| ❺.3 VFX | element 4 prefab + VfxPool 등록 | 자체 합성 + 무료 에셋 hook |
| ❺.4 적 저항 | **B** float[4] | 정밀 밸런싱 (1.5×/0.7× 등) |
| ❻ 무기 매핑 | **8 → element 부여** | Editor 마이그레이션 일괄 |

## 3. 권장 설계

### 3.1 신규 enum + 데이터 모델

**MagicElement enum**:
```csharp
public enum MagicElement {
    None,
    Fire,      // 불 — 도트 데미지 + 약점 보너스
    Ice,       // 얼음 — 둔화 (movespeed 50%) 0.5s
    Lightning, // 번개 — 사슬 (인접 적 1 추가 데미지)
    Arcane,    // 신비 — 무시 저항 (모든 element 1.0× 강제)
}
```

### 3.2 신규 4 attack pattern (E 채택)

| Pattern | 매핑 element | 동작 |
|---|---|---|
| **Beam** | Arcane | 직선 광선 — N초 지속 + 라인 데미지 (Aura 변형) |
| **Chain** | Lightning | 사슬 — 첫 적 → 인접 N 적 자동 점프 |
| **Vortex** | Ice | 소용돌이 — 흡인 + 빙결 0.5s |
| **Meteor** | Fire | 운석 낙하 — 광역 폭발 (Rain 변형, 단발 + 큰 폭발) |

`AttackPatternType` enum 확장: `+ Beam, Chain, Vortex, Meteor`.

### 3.3 데이터 모델 (5028 답습 패턴)

**EquipmentTemplate.cs +필드**:
```csharp
public MagicElement element;  // 신규 — 기본 None (전사/궁수 영향 X)
```

**EnemyData.cs +필드**:
```csharp
public float[] elementResistance = { 1f, 1f, 1f, 1f };
// index: [0]=Fire, [1]=Ice, [2]=Lightning, [3]=Arcane
// 1.0 = 중립, 1.5 = 약점 (50% 추가), 0.7 = 저항 (30% 감소)
```

**IAttackPattern.cs (Strategy 정합)**:
```csharp
void Initialize(AttackConfig config, Transform owner, PlayerStats stats, MagicElement element);
```

기존 8 패턴은 element 받아 무시 (전사/궁수 호환), 신규 4 패턴이 element 활용.

### 3.4 DamageDealer 통합 (5028 답습)

```csharp
// DamageDealer 에 element 매개변수 추가
public static int DealDamage(EnemyController enemy, float damage, ..., MagicElement element = MagicElement.None) {
    float elementMul = ResolveElementMultiplier(enemy, element);
    // Fire (도트) / Ice (둔화) / Lightning (사슬) effect 적용
}
```

### 3.5 신규 파일 명세

| 파일 | 위치 | 책임 | 라인 |
|---|---|---|---|
| `MagicElement.cs` | Data/ | enum 5종 (None+4) | ~15L |
| `BeamAttackPattern.cs` | Gameplay/Equipment/Patterns/ | IAttackPattern Beam | ~70L |
| `ChainAttackPattern.cs` | Gameplay/Equipment/Patterns/ | IAttackPattern Chain | ~80L |
| `VortexAttackPattern.cs` | Gameplay/Equipment/Patterns/ | IAttackPattern Vortex | ~75L |
| `MeteorAttackPattern.cs` | Gameplay/Equipment/Patterns/ | IAttackPattern Meteor | ~70L |
| `ElementResolver.cs` | Gameplay/Combat/ | float[] 저항 → 곱셈 + 적 OnHit element effect | ~80L |
| `MageWeaponElementMigrator.cs` | Editor/ | 8 무기 element 일괄 마이그레이션 | ~50L |
| `ElementVfxBank.cs` | Core/ | element 4 VFX prefab + VfxPool 등록 | ~45L |

### 3.6 §15 OOP 정합

- §15.1 SRP: ✅ 신규 pattern 4 + ElementResolver 단일 책임
- §15.4 SO: ✅ EquipmentTemplate.element + EnemyData.elementResistance[4]
- §15.6 100L: ✅ 신규 8 파일 모두 100L 이내
- §15.7 DI: ✅ Initialize(element) 명시 주입
- §15.9 Test: ElementResolverTests 5+ NUnit
- §15.12 Validator: 마법사 무기 8 element != None 검증 룰 추가

## 4. Sprint 분해

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | MagicElement enum + EquipmentTemplate/EnemyData 필드 + 마이그레이션 | 1 | 45분 |
| 2 | ElementResolver + DamageDealer element 통합 + EnemyController.OnHit | 1 | 50분 |
| 3 | 신규 4 pattern (Beam/Chain/Vortex/Meteor) + Factory 확장 | 2 | 80분 |
| 4 | 마법사 무기 8 → element 매핑 + ElementVfxBank + VFX 4 prefab | 1 | 50분 |
| 5 | UI — Inventory + GachaResult element 아이콘 | 1 | 40분 |
| 6 | NUnit + Validator + changelog + CLAUDE.md | 1 | 30분 |
| **합계** | | **7** | **~4h 55m** |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| ~~Lightning 무기 1개 (분포 불균등)~~ | ✅ 옵션 B 채택 — weapon_thunder_wand 신규 1개 추가 (Sprint 4 통합), 분포 2/2/2/3 |
| 신규 SO 가챠 풀 영향 | Sprint 4 GachaPool 등급 풀 마이그레이션 명시 |
| 기존 8 pattern 회귀 (element 매개변수 추가) | default MagicElement.None 호환 — 전사/궁수 영향 0 |
| EnemyData 262 SO 마이그레이션 | default `{1,1,1,1}` 자동 + Editor 일괄 |
| EquipmentManager.SyncEquippedInstances 영향 | element 매개변수 명시 전달 (5028 PatternActivator 답습) |
| 5028.1 UI Panel 변경 위험 | InventoryPanel + GachaResultPanel 만 영향, 작은 변경 |
| 무료 VFX 에셋 사용자 검토 필요 | Sprint 4 시점 사용자 결정 (placeholder 자체 합성 우선) |

## 6. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 사용자 6 결정 채택 (E + 4 + C + 무료에셋 + Claude추천 + 8매핑) |

## 7. Sources

- `docs/little_hero/mage_diversity_design_questions.md` — 결정 가이드
- `docs/prd/5026_boss_attack_type_migration_prd.md` — 5 카테고리 매핑 답습 패턴
- `docs/prd/5028_oop_class_split_phase2_prd.md` — facade + POCO 분리 답습
- weapon_system_v2.md — 직업 분류 (Mage + Staff/Wand/Tome)
- IAttackPattern.cs — 8 pattern Strategy
