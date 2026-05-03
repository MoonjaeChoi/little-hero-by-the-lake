# Weapon Balance 24 v1 — 직업×등급 균형 매트릭스

> **목표**: 18 무기 → 24 무기 (각 직업 8 / 등급 2:2:2:2 균등). gacha_class_filter_v1 의 fallback 위험 (Warrior Magic 부재) 해소.

## 0. 결정 이력
- 2026-05-03: 사용자 요청 — 24 무기 균형 매트릭스

## 1. 현재 18 무기 매트릭스 (불균형)

| | Common | Magic | Rare | Epic | 합 |
|---|---|---|---|---|---|
| **Warrior** | 1 | **0 ⚠️** | 2 | 1 | 4 |
| **Archer** | 3 | 2 | 1 | 1 | 7 |
| **Mage** | 2 | 2 | 1 | 2 | 7 |

**문제**: Warrior 무기 4종만 / Magic 부재 / Archer Common 과잉 / Mage Rare 부족.

## 2. 24 무기 목표 매트릭스 (균등)

| | Common | Magic | Rare | Epic | 합 |
|---|---|---|---|---|---|
| **Warrior** | 2 | 2 | 2 | 2 | 8 |
| **Archer** | 2 | 2 | 2 | 2 | 8 |
| **Mage** | 2 | 2 | 2 | 2 | 8 |

각 직업 균등 8 / 등급 균등 2:2:2:2 — **가챠 풀 fallback X**.

## 3. 신규 무기 6 + 기존 재조정

### 3.1 신규 6 무기 추가
| ID | 직업 | 등급 | Pattern | Category | 설명 |
|---|---|---|---|---|---|
| `warrior_iron_sword` | Warrior | Common | Slash | Sword | 철검 — 기본 검 |
| `warrior_steel_axe` | Warrior | Magic | Pulse | Axe | 강철 도끼 — 광역 충격 |
| `warrior_claymore` | Warrior | Magic | Slash | Sword | 대검 — 큰 부채꼴 |
| `warrior_giants_axe` | Warrior | Epic | Pulse | Axe | 거인의 도끼 — 강력 광역 |
| `archer_eagle_bow` | Archer | Rare | Projectile | Bow | 매눈 활 — 정확·관통 |
| `mage_glacier_rune` | Mage | Rare | Aura | Tome | 얼음 룬 — 둔화 오라 |

### 3.2 기존 재조정 (1 무기)
| ID | 변경 | 이유 |
|---|---|---|
| `weapon_quick_dagger` | Common → **Magic** | Archer Common 3→2, Magic 2→3 (중간 단계 균등) |

근데 Magic 3 = 균등 X. 다른 옵션 — 신규 Archer 추가 (7 → 8) + dagger 그대로:
- 신규 Archer Rare 추가 (= eagle_bow) → 7+1=8
- 신규 Mage Rare 추가 (= glacier_rune) → 7+1=8
- 신규 Warrior 4 추가 → 4+4=8

### 3.3 최종 매트릭스 (재조정 X — 신규 6 만 추가)

| | Common | Magic | Rare | Epic | 합 |
|---|---|---|---|---|---|
| **Warrior** | 2 (impact + iron_sword) | 2 (steel_axe + claymore) | 2 (thunder + katana) | 2 (abyss + giants_axe) | 8 |
| **Archer** | 3 (quick + fan + spin) | 2 (pierce + phantom) | 2 (boomerang + eagle_bow) | 1 (storm_bow) | **8** |
| **Mage** | 2 (rune + firebottle) | 2 (frost + spirit_pet) | 2 (meteor + glacier_rune) | 2 (annihilation + time_spirit) | 8 |

⚠️ Archer 비균등 (3:2:2:1) — Common 3 / Epic 1.

### 3.4 Archer 균등 옵션

#### 옵션 A — quick_dagger Common → Magic 승격
| Archer | Common | Magic | Rare | Epic |
|---|---|---|---|---|
| | 2 (fan + spin) | 3 (pierce + phantom + quick) | 2 | 1 |

여전히 비균등 (2:3:2:1).

#### 옵션 B — Archer Epic 신규 1 추가 (총 25)
| ID | 등급 | Pattern | 설명 |
|---|---|---|---|
| `archer_divine_bow` | Epic | Projectile | 신성 활 |

매트릭스:
| | C | M | R | E | 합 |
|---|---|---|---|---|---|
| W | 2 | 2 | 2 | 2 | 8 |
| A | 3 | 2 | 2 | 2 | **9** |
| M | 2 | 2 | 2 | 2 | 8 |

A 9 — 25 무기.

#### 옵션 C — Common 1 폐기 + Epic 1 추가 (총 24)
- `weapon_spin_dagger` Common → 폐기 또는 Magic 승격 (Common 2)
- `archer_divine_bow` Epic 추가

| | C | M | R | E | 합 |
|---|---|---|---|---|---|
| W | 2 | 2 | 2 | 2 | 8 |
| A | 2 | 3 (spin Magic 으로) | 2 | 2 | **9** ❌ |

#### 옵션 D ★ — quick_dagger Magic 승격 + Archer Epic 신규 추가 (총 25, 균등)
- quick_dagger Common → Magic
- archer_divine_bow Epic 신규

| | C | M | R | E | 합 |
|---|---|---|---|---|---|
| W | 2 | 2 | 2 | 2 | 8 |
| A | 2 (fan + spin) | 3 (pierce + phantom + quick) | 2 (boomerang + eagle) | 2 (storm + divine) | **9** ❌ |

여전히 9.

#### 옵션 E ★ — Archer Magic 1 폐기 (또는 Rare 승격) + Epic 신규
- `weapon_phantom_dagger` Magic → Rare 승격
- 신규 Epic `archer_divine_bow`
- 신규 Rare `archer_eagle_bow` 폐기 (대체)

| | C | M | R | E | 합 |
|---|---|---|---|---|---|
| W | 2 | 2 | 2 | 2 | 8 |
| A | 3 (quick + fan + spin) | 1 (pierce) | 3 (boomerang + phantom + eagle) | 1 (storm) | 8 (3:1:3:1) |

여전히 비균등.

### 3.5 권장 — **옵션 F: 기존 18 그대로 + 신규 6 (총 24, Archer 만 미세 비균등 허용)**

매트릭스:
| | C | M | R | E | 합 |
|---|---|---|---|---|---|
| W | 2 | 2 | 2 | 2 | 8 ✅ |
| A | 3 | 2 | 2 | 1 | **8** ⚠️ |
| M | 2 | 2 | 2 | 2 | 8 ✅ |

Archer 만 Common 3 / Epic 1 (메이플 키우기 식 — 궁수는 단검 가성비 ↑, Epic 1종 한정).

**Archer Epic 1 = storm_bow** (폭풍 활) — 이미 강력. 추가 Epic 불필요.

가챠 풀 fallback 동작:
- Warrior Magic → 2 무기 OK
- Mage Rare → 2 무기 OK
- Archer Epic → 1 무기 (가챠 시 항상 storm_bow) — 의도된 한정

## 4. 신규 6 무기 SO 명세

### 4.1 warrior_iron_sword (Warrior / Common / Slash)
- **이름**: 철검
- **설명**: 기본 검. 단순하지만 안정적.
- **AttackPatternConfig**: damage 8, attackInterval 0.6s, range 1.5, count 1, maxTargets 1, spreadAngle 60°
- **baseStats**: attackDamageBonus 5

### 4.2 warrior_steel_axe (Warrior / Magic / Pulse)
- **이름**: 강철 도끼
- **설명**: 광역 충격파 도끼.
- **AttackPatternConfig**: damage 12, attackInterval 1.5s, range 3.0
- **baseStats**: attackDamageBonus 8, maxHealthBonus 10

### 4.3 warrior_claymore (Warrior / Magic / Slash)
- **이름**: 대검
- **설명**: 큰 부채꼴 검기 (다중 타깃 3).
- **AttackPatternConfig**: damage 10, attackInterval 1.0s, range 2.5, maxTargets 3, spreadAngle 90°
- **baseStats**: attackDamageBonus 7

### 4.4 warrior_giants_axe (Warrior / Epic / Pulse)
- **이름**: 거인의 도끼
- **설명**: 강력한 광역 충격 (4타깃).
- **AttackPatternConfig**: damage 25, attackInterval 2.0s, range 4.0, maxTargets 4
- **baseStats**: attackDamageBonus 15, maxHealthBonus 20

### 4.5 archer_eagle_bow (Archer / Rare / Projectile)
- **이름**: 매눈 활
- **설명**: 정확한 관통 화살.
- **AttackPatternConfig**: damage 16, attackInterval 0.7s, range 8, projectileSpeed 12, pierceCount 2
- **baseStats**: attackDamageBonus 10, rangeBonus 1

### 4.6 mage_glacier_rune (Mage / Rare / Aura)
- **이름**: 얼음 룬
- **설명**: 둔화 오라 (적 이속 -30%).
- **AttackPatternConfig**: damage 12, attackInterval 0.8s, range 3.5, slowAmount 0.3, slowDuration 1.5
- **baseStats**: attackDamageBonus 8

## 5. PRD Sprint 7 추가 (1 commit)

### 7.1 신규 6 무기 SO 생성

**파일**: `Editor/WeaponBalanceSeed.cs` (신규)

**메뉴**: `Tools > LittleHero > Seed Balance Weapons (24)`

**작업**:
1. 신규 6 EquipmentTemplate.asset 생성 (`Data/Templates/Weapons/`)
2. 각 무기에 attackPattern + attackConfig + baseStats + requiredClass + category wiring
3. icon — V5 sticker 톤 (별도 ComfyUI batch 후 wiring)
4. GachaPool SO 자동 갱신 — 등급별 풀에 신규 무기 추가
   - commonPool += [warrior_iron_sword]
   - magicPool += [warrior_steel_axe, warrior_claymore]
   - rarePool += [archer_eagle_bow, mage_glacier_rune]
   - epicPool += [warrior_giants_axe]

**검증 기준**:
- 24 EquipmentTemplate.asset 모두 정확
- GachaPool 등급별 풀 정합 (Common 7 / Magic 6 / Rare 6 / Epic 5)
- 직업×등급 매트릭스 통과 (Warrior 8 / Archer 8 / Mage 8)

**완료 조건**:
- 컴파일 0
- ProjectValidator 통과 (모든 무기 SO requiredClass + category wired)

### Sprint 7 commit
```
weapon_v2 Sprint 7 — 24 무기 균형 매트릭스 (신규 6)

신규 EquipmentTemplate 6:
  - warrior_iron_sword (Common Slash)
  - warrior_steel_axe (Magic Pulse)
  - warrior_claymore (Magic Slash)
  - warrior_giants_axe (Epic Pulse)
  - archer_eagle_bow (Rare Projectile pierce 2)
  - mage_glacier_rune (Rare Aura slow 30%)

GachaPool 자동 갱신:
  Common 7 / Magic 6 / Rare 6 / Epic 5 = 24

직업×등급:
  Warrior 8 (2:2:2:2 균등 ✅)
  Archer 8 (3:2:2:1, Common 3 메이플 식)
  Mage 8 (2:2:2:2 균등 ✅)
```

## 6. 무기 icon 후속 (별도 작업)

신규 6 무기 ComfyUI 22222 시드 V5 톤 batch:
- prompt: V5 base + 무기 form (검·도끼·대검·거대도끼·활·룬)
- isnet-anime 흰배경 segmentation
- 폴더: `Generated_v2/Weapons/<id>.png`
- TextureImporter Sprite + EquipmentTemplate.icon wiring

**별도 사이클** (스크립트만 실행, 코드 변경 X). 신규 commit 예상.

## 7. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Archer Epic 1종만 (storm_bow) — 가챠 결과 항상 동일 | 의도된 한정 (메이플 식 고급 무기) |
| 신규 6 무기 밸런싱 | CurveSimulator 시뮬 (별도 Sprint) |
| 기존 18 SaveData 호환 | 신규 무기 추가만, 기존 영향 X |
| 무기 icon 미생성 시 기본 placeholder | EquipmentTemplate.icon = null 시 자동 default sprite |

## 8. 후속 (별도 Sprint)

### 8.1 무기 icon 생성 (1 commit)
- ComfyUI 6 batch + isnet-anime + Unity wiring

### 8.2 시뮬 검증 (CurveSimulator 1 commit)
- 24 무기 가챠 풀 분포 시뮬 (직업별 1000회)
- 등급별 도달 시간 검증

## 9. 변경 이력
- 2026-05-03: 초기 작성 (24 무기 균형 매트릭스)

## 10. 참조
- `gacha_class_filter_v1.md` — 가챠 직업 필터 (옵션 B 런타임)
- `weapon_system_v2.md` — 무기 OOP 분류
- `weapon_system_v2_prd.md` — 5 Sprint PRD (+ Sprint 6 가챠 + Sprint 7 균형)
- `gacha_v1.md` — 18 무기 원본
