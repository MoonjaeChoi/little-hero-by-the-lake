# Weapon System v2 — 직업별 분류 + OOP 재설계

> **목표**: 현재 무기 시스템을 분석 → 직업별 분류 가능성 검토 → CLAUDE.md §15 OOP 정책에 따라 재설계.
> 메이플스토리 표준 직업·무기 분류 + Strategy 패턴 강화.

## 0. 결정 이력
- 2026-05-03: 사용자 요청 — 무기 시스템 직업별 분류 + OOP 재설계

## 1. 현재 무기 시스템 분석

### 1.1 18 가챠 무기 분포 (pattern × rarity)

| AttackPatternType | 무기 (rarity 0=Common / 1=Magic / 2=Rare / 3=Epic) |
|---|---|
| **1=Projectile** | rune_staff(0) / quick_dagger(0) / fan_bow(0) / pierce_bow(1) / storm_bow(3) |
| **2=Aura** | firebottle(0) / frost_rune(1) |
| **3=Orbit** | spin_dagger(0) / return_boomerang(2) |
| **4=Rain** | meteor_rod(2) / annihilation_rain(3) |
| **5=Trail** | phantom_dagger(1) |
| **6=Pulse** | impact_hammer(0) / thunder_hammer(2) / abyss_hammer(3) |
| **7=Summon** | spirit_pet(1) / time_spirit(3) |
| **8=Slash** | vampiric_katana(2) |

### 1.2 현재 직업 매핑 (없음)
- `EquipmentTemplate` 에 className / weaponClass 필드 **부재**
- 무기 = pattern + rarity 만 분류
- 모든 직업이 모든 무기 사용 가능 (직업 차별 X)

### 1.3 현재 시스템 구조
```
EquipmentTemplate (SO)
├── id / displayName
├── slot: EquipSlot (Weapon 만)
├── rarity: Rarity
└── attackPattern: AttackPatternType (8 종)

EquipmentManager
├── EquipInstance(EquipmentInstance)
└── ActivatePattern → EquipmentPatternFactory.Create(pattern)
                      ↓
                  IAttackPattern (Strategy)
                      ├── ProjectileAttack
                      ├── AuraAttack
                      ├── ... (8 구현체)
```

## 2. 메이플스토리 직업·무기 표준

| 직업 | 메이플 무기 | 특성 |
|---|---|---|
| **전사 Warrior** | 검 / 도끼 / 둔기 / 창 / 폴암 | 근접 / 높은 HP·방어 |
| **마법사 Magician** | 완드 / 스태프 | 원거리 마법 / MP·INT |
| **궁수 Archer** | 활 / 석궁 | 원거리 물리 / DEX |
| **도적 Rogue** | 단검 / 표창 | 중거리 / LUK |
| **해적 Pirate** | 너클 / 총 / 대포 | 다양 / STR or DEX |

Little Hero 는 **3 직업** (전사 / 궁수 / 마법사) → 도적·해적 폐기. 단검은 도적 무기지만 Little Hero 환경에서는 직업 선택 무기로 통합.

## 3. 현재 18 무기 직업별 매핑 분석

### 3.1 무기 이름 + attack pattern → 직업 추론

| 무기 | Pattern | 추정 직업 | 이유 |
|---|---|---|---|
| weapon_quick_dagger | Projectile | **궁수** (또는 전사) | 단검 = 도적/전사, 발사형 = 던지는 단검 |
| weapon_phantom_dagger | Trail | **궁수** | 환영 단검 = 그림자 흔적 |
| weapon_spin_dagger | Orbit | **궁수** | 회전 단검 = 부메랑 |
| weapon_fan_bow | Projectile | **궁수** | 부채꼴 활 |
| weapon_pierce_bow | Projectile | **궁수** | 관통의 활 |
| weapon_storm_bow | Projectile | **궁수** | 폭풍 활 |
| weapon_return_boomerang | Orbit | **궁수** (또는 전사) | 회귀 부메랑 |
| weapon_vampiric_katana | Slash | **전사** | 흡혈 카타나 = 검 |
| weapon_impact_hammer | Pulse | **전사** | 충격 망치 |
| weapon_thunder_hammer | Pulse | **전사** | 천둥 망치 |
| weapon_abyss_hammer | Pulse | **전사** | 심연의 망치 |
| weapon_rune_staff | Projectile | **마법사** | 룬 지팡이 (마법 발사) |
| weapon_meteor_rod | Rain | **마법사** | 유성 막대 (소환·마법) |
| weapon_annihilation_rain | Rain | **마법사** | 멸살의 비 (광역 마법) |
| weapon_firebottle | Aura | **마법사** | 화염병 (오라) |
| weapon_frost_rune | Aura | **마법사** | 빙결 룬 (마법) |
| weapon_spirit_pet | Summon | **마법사** | 영혼 동료 (소환술) |
| weapon_time_spirit | Summon | **마법사** | 시간 정령 (소환술) |

### 3.2 직업별 분포 (제안)
| 직업 | 무기 수 | 패턴 |
|---|---|---|
| **전사** Warrior | 4 | Slash 1 + Pulse 3 |
| **궁수** Archer | 7 | Projectile 4 (3 활 + 1 단검) + Orbit 2 + Trail 1 |
| **마법사** Mage | 7 | Projectile 1 (룬) + Aura 2 + Rain 2 + Summon 2 |

✅ **3 직업 분류 가능** — 18 무기 모두 매핑 가능. 패턴 자체는 직업과 1:1 매핑 X (Projectile 은 활/단검/룬 모두 사용) 이지만 무기 이름·외형 으로 직업 매핑 분명.

## 4. OOP 재설계 — Weapon System v2

### 4.1 핵심 변경 (CLAUDE.md §15 정책 정합)

#### A. **CharacterClass enum** 신규
```
public enum CharacterClass { Warrior, Archer, Mage }
```

#### B. **WeaponCategory enum** 신규 (메이플 무기 분류)
```
public enum WeaponCategory {
    Sword, Axe, Mace, Spear,         // 전사
    Bow, Crossbow, Dagger, Boomerang, // 궁수 (단검은 궁수로 통합)
    Staff, Wand, Tome,                // 마법사
}
```

#### C. **EquipmentTemplate 확장** (2 필드 추가)
```
public CharacterClass requiredClass;   // 신규 — Warrior/Archer/Mage
public WeaponCategory category;         // 신규 — Sword/Bow/Staff 등
```

#### D. **IWeapon 인터페이스 분리** (SOLID-I)
```
IWeapon (계약)
├── EquipmentTemplate Template { get; }
├── CharacterClass RequiredClass { get; }
├── WeaponCategory Category { get; }
└── bool CanEquip(CharacterClass playerClass);
```

#### E. **무기 장착 검증** (SOLID-O)
- `EquipmentManager.EquipInstance(inst)` 진입부에서 직업 검증
- `inst.template.requiredClass != player.character.classType` → 거부
- UI: 잘못된 직업 무기는 회색 + "전사 전용" 표시

### 4.2 Strategy 패턴 강화

기존 `EquipmentPatternFactory.Create(AttackPatternType)` 유지 + 직업 검증:

```
EquipmentManager.EquipInstance:
  1. 검증: instance.template.requiredClass == player.class
  2. Pattern: EquipmentPatternFactory.Create(template.attackPattern)
  3. baseStats: ApplyEquipmentBonus(template.baseStats)
  4. 직업 보정: WeaponClassBonus.Apply(template.category, stats)
  5. EffectResolver: ApplyResolvedBonus
```

#### F. **WeaponClassBonus** 정적 헬퍼 (신규)
직업별 무기 카테고리 보정 (메이플 풍):
- 전사 + Sword/Hammer → Damage% +10%
- 궁수 + Bow/Dagger → AttackSpeed +5%
- 마법사 + Staff/Wand → MagicDamage +10%
- 직업 X 카테고리 → 보정 X (또는 -10% 패널티)

### 4.3 신규 파일 구조

```
Scripts/Data/
├── CharacterClass.cs (enum) — 신규
├── WeaponCategory.cs (enum) — 신규
└── EquipmentTemplate.cs — 갱신 (requiredClass + category 추가)

Scripts/Gameplay/Equipment/
├── IWeapon.cs (interface) — 신규
├── WeaponClassBonus.cs (정적) — 신규
└── EquipmentManager.cs — 갱신 (직업 검증 + Bonus)

Scripts/UI/
└── InventoryGridCard.cs — 갱신 (잘못된 직업 회색 + 라벨)
```

### 4.4 18 무기 SO 갱신 (자동 마이그레이션)

| weapon | requiredClass | category |
|---|---|---|
| weapon_vampiric_katana | Warrior | Sword |
| weapon_impact_hammer / thunder_hammer / abyss_hammer | Warrior | Mace |
| weapon_quick_dagger / phantom_dagger / spin_dagger | Archer | Dagger |
| weapon_fan_bow / pierce_bow / storm_bow | Archer | Bow |
| weapon_return_boomerang | Archer | Boomerang |
| weapon_rune_staff | Mage | Staff |
| weapon_meteor_rod | Mage | Wand |
| weapon_annihilation_rain | Mage | Tome |
| weapon_firebottle / frost_rune | Mage | Tome |
| weapon_spirit_pet / time_spirit | Mage | Tome |

## 5. UI 영향

### 5.1 InventorySelectionPanel
- 잘못된 직업 무기 카드 → **회색 + 자물쇠** 표시
- 카드 라벨 추가: "전사 전용" / "궁수 전용" / "마법사 전용"
- 정렬 우선순위: **본인 직업** → 등급 → 강화

### 5.2 GachaResultPanel
- 가챠 결과에 직업 라벨 추가 (등급 색 + 직업 색)

### 5.3 EquipmentCardPanel
- 카드 헤더에 카테고리 + 직업 표시 (예 "심연의 망치 — 전사 / 둔기")

## 6. 마이그레이션 단계

### Sprint 1 — 인프라 (3 commit)
1. CharacterClass + WeaponCategory enum
2. EquipmentTemplate 확장 (requiredClass + category)
3. IWeapon 인터페이스

### Sprint 2 — 18 SO 마이그레이션 (1 commit)
4. 18 EquipmentTemplate.asset 자동 갱신 (requiredClass + category)

### Sprint 3 — 검증 + Bonus (2 commit)
5. EquipmentManager.EquipInstance 직업 검증
6. WeaponClassBonus 정적 + 카테고리별 보정

### Sprint 4 — UI (2 commit)
7. InventorySelectionPanel 회색·라벨
8. GachaResultPanel + EquipmentCardPanel 직업 표시

### Sprint 5 — Test + Validator (1 commit)
9. WeaponClassBonus + EquipmentTemplate.CanEquip Test
10. ProjectValidator OOP 룰 추가 (모든 무기 SO 의 requiredClass 비어있으면 경고)

**총 9 commit**

## 7. 메이플 키우기 패턴 정합

### 7.1 직업 차별화 (이미 stats_v2 적용)
- 전사 STR 2.0× → 공격력 + HP 강화
- 궁수 AGI 2.0× → 공속 + 회피 강화
- 마법사 WIS 2.0× → 크리 + 마법 데미지 강화

### 7.2 무기 + 직업 시너지 (신규)
- 전사 + 검/둔기 → Damage% 보너스
- 궁수 + 활/단검 → AttackSpeed 보너스
- 마법사 + 지팡이/완드 → MagicDamage 보너스

→ stats_v2 (직업) + weapon_v2 (무기) **시너지 명확** = 메이플 키우기 표준

## 8. CLAUDE.md §15 정책 정합

| 정책 | 적용 |
|---|---|
| §15.1 SRP | EquipmentTemplate 확장 + IWeapon 분리 |
| §15.1 OCP | 신규 무기 카테고리 추가 시 enum 만 갱신 (코드 X) |
| §15.1 LSP | 모든 무기 IWeapon 호환 |
| §15.1 ISP | IWeapon (작은 interface) + IAttackPattern 분리 |
| §15.4 SO 우선 | EquipmentTemplate SO 만 갱신 |
| §15.8 Strategy | EquipmentPatternFactory 그대로 활용 |
| §15.9 Test | WeaponClassBonus + CanEquip 단위 테스트 |
| §15.12 Validator | 모든 무기 SO requiredClass 검증 |

## 9. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 직업 변경 시 무기 강제 해제 | EquipmentManager.OnCharacterChanged 자동 unequip |
| 가챠 결과 잘못된 직업 무기 | InventorySelectionPanel 회색 + "다른 직업 선택 시 사용 가능" 안내 |
| 신규 무기 추가 시 enum 확장 | OCP 정합 — Sword/Bow 추가 시 enum 만 갱신 |
| 기존 SaveData 호환 | requiredClass=0 (Warrior) default → 마이그레이션 시 Auto fix |

## 10. 변경 이력
- 2026-05-03: 초기 작성 (사용자 요청 — 직업별 분류 + OOP 재설계)

## 11. Sources

- [메이플스토리 직업 — 나무위키](https://namu.wiki/w/%EB%A9%94%EC%9D%B4%ED%94%8C%EC%8A%A4%ED%86%A0%EB%A6%AC/%EC%A7%81%EC%97%85)
- [메이플스토리 장비 아이템 — 나무위키](https://namu.wiki/w/%EB%A9%94%EC%9D%B4%ED%94%8C%EC%8A%A4%ED%86%A0%EB%A6%AC/%EC%9E%A5%EB%B9%84%20%EC%95%84%EC%9D%B4%ED%85%9C)
- [Strategy Pattern — Composition over Inheritance — One Wheel Studio](https://onewheelstudio.com/blog/2020/8/16/strategy-pattern-composition-over-inheritance)
- [Design Patterns for FPS Game Weapons — DZone](https://dzone.com/articles/design-patterns-for-fps-game-weapons-1)
- [Strategy pattern — Unity Learn](https://learn.unity.com/course/design-patterns-unity-6/tutorial/strategy-pattern)
