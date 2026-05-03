# Weapon System v2 — PRD (Product Requirements Document)

> **출처**: `weapon_system_v2.md` 디자인 문서 (2026-05-03)
> **목표**: 5 Sprint 일괄 진행 (9 commit). 각 작업 단위의 인풋/아웃풋/검증/완료 기준 명세.
> **CLAUDE.md §15 OOP 정책 준수**.

## 0. 전체 개요

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | 인프라 (enum 2 + EquipmentTemplate + IWeapon) | 3 | 30분 |
| 2 | 18 SO 자동 마이그레이션 | 1 | 15분 |
| 3 | EquipmentManager 검증 + WeaponClassBonus | 2 | 45분 |
| 4 | UI (InventorySelectionPanel + GachaResultPanel + EquipmentCardPanel) | 2 | 60분 |
| 5 | Test + ProjectValidator | 1 | 30분 |
| **총** | | **9** | **3시간** |

## Sprint 1 — 인프라 (3 commit)

### 1.1 CharacterClass + WeaponCategory enum 신규

**파일**: `Scripts/Data/CharacterClass.cs` (신규)

**산출물**:
```
public enum CharacterClass {
    Warrior = 0,   // 전사 — 검·둔기·도끼·창
    Archer  = 1,   // 궁수 — 활·석궁·단검·부메랑
    Mage    = 2,   // 마법사 — 지팡이·완드·마도서
}

public enum WeaponCategory {
    Sword = 0, Axe = 1, Mace = 2, Spear = 3,         // 전사 4종
    Bow = 4, Crossbow = 5, Dagger = 6, Boomerang = 7, // 궁수 4종
    Staff = 8, Wand = 9, Tome = 10,                    // 마법사 3종
    None = 99,
}
```

**검증 기준**:
- enum 컴파일 OK
- CharacterData 기존 코드 호환 (className 필드와 매핑 가능)

**완료 조건**:
- 컴파일 에러 0
- enum 값 11종 (Warrior 3 + Archer 4 + Mage 3 + None) 정확

---

### 1.2 EquipmentTemplate 확장 (2 필드 추가)

**파일**: `Scripts/Data/EquipmentTemplate.cs` (갱신)

**추가 필드**:
```
public CharacterClass requiredClass;  // 직업 제한
public WeaponCategory category;        // 무기 카테고리
```

**SerializeField 패턴** (CLAUDE.md §15.2):
- public 유지 (SO 디자이너 친화)
- Inspector 표시: `[Header("직업·카테고리")]`

**검증 기준**:
- 기존 18 EquipmentTemplate.asset 모두 default 값 (requiredClass=Warrior, category=None) 로 마이그레이션 OK
- SerializedObject API 로 필드 접근 가능

**완료 조건**:
- 컴파일 에러 0
- 기존 18 SO Inspector 에서 신규 필드 표시

---

### 1.3 IWeapon 인터페이스 신규

**파일**: `Scripts/Gameplay/Equipment/IWeapon.cs` (신규)

**산출물**:
```
public interface IWeapon {
    EquipmentTemplate Template { get; }
    CharacterClass RequiredClass { get; }
    WeaponCategory Category { get; }
    bool CanEquip(CharacterClass playerClass);
}

// EquipmentInstance + Template 결합 wrapper
public class WeaponInstance : IWeapon {
    public EquipmentInstance Instance { get; }
    public EquipmentTemplate Template { get; }
    public CharacterClass RequiredClass => Template.requiredClass;
    public WeaponCategory Category => Template.category;
    public bool CanEquip(CharacterClass cls) => Template.requiredClass == cls;

    public WeaponInstance(EquipmentInstance inst, EquipmentTemplate tpl) { ... }
}
```

**검증 기준**:
- IWeapon 인터페이스로 무기 추상화
- WeaponInstance 구현체 (EquipmentInstance + Template wrapper)

**완료 조건**:
- 컴파일 OK
- WeaponInstance.CanEquip(Warrior) 정확

---

### Sprint 1 commit 메시지

```
weapon_v2 Sprint 1 — 인프라 (enum + EquipmentTemplate + IWeapon)

[신규]
  - CharacterClass enum (Warrior/Archer/Mage)
  - WeaponCategory enum (11종)
  - IWeapon interface + WeaponInstance wrapper

[갱신]
  - EquipmentTemplate.cs — requiredClass + category 필드 추가
```

---

## Sprint 2 — 18 SO 자동 마이그레이션 (1 commit)

### 2.1 무기 SO 일괄 갱신

**스크립트**: `Editor/WeaponMigrationSeed.cs` (신규)

**메뉴**: `Tools > LittleHero > Migrate Weapon Class`

**작업 매핑** (weapon_system_v2.md §4.4):
| weapon | requiredClass | category |
|---|---|---|
| weapon_vampiric_katana | Warrior | Sword |
| weapon_impact_hammer | Warrior | Mace |
| weapon_thunder_hammer | Warrior | Mace |
| weapon_abyss_hammer | Warrior | Mace |
| weapon_quick_dagger | Archer | Dagger |
| weapon_phantom_dagger | Archer | Dagger |
| weapon_spin_dagger | Archer | Dagger |
| weapon_fan_bow | Archer | Bow |
| weapon_pierce_bow | Archer | Bow |
| weapon_storm_bow | Archer | Bow |
| weapon_return_boomerang | Archer | Boomerang |
| weapon_rune_staff | Mage | Staff |
| weapon_meteor_rod | Mage | Wand |
| weapon_annihilation_rain | Mage | Tome |
| weapon_firebottle | Mage | Tome |
| weapon_frost_rune | Mage | Tome |
| weapon_spirit_pet | Mage | Tome |
| weapon_time_spirit | Mage | Tome |

**검증 기준**:
- 18 SO 모두 requiredClass + category 갱신
- AssetDatabase.SaveAssets

**완료 조건**:
- 18/18 wired (검증 grep 으로 확인)
- 직업별 분포: Warrior 4 / Archer 7 / Mage 7

### Sprint 2 commit
```
weapon_v2 Sprint 2 — 18 SO 자동 마이그레이션

WeaponMigrationSeed Editor 메뉴 + 18 SO 자동 갱신:
  - Warrior 4 (Sword 1 + Mace 3)
  - Archer 7 (Bow 3 + Dagger 3 + Boomerang 1)
  - Mage 7 (Staff 1 + Wand 1 + Tome 5)
```

---

## Sprint 3 — 검증 + Bonus (2 commit)

### 3.1 EquipmentManager 직업 검증

**파일**: `Scripts/Gameplay/Equipment/EquipmentManager.cs` (갱신)

**진입부 검증** (EquipInstance):
```
public void EquipInstance(EquipmentInstance inst) {
    var tpl = ResolveTemplate(inst);
    if (tpl == null) return;

    // 신규 — 직업 검증
    var playerClass = ResolvePlayerClass();
    if (!CanEquip(tpl, playerClass)) {
        Debug.LogWarning($"[EquipmentManager] {tpl.id} 는 {tpl.requiredClass} 전용 (현재 {playerClass})");
        return;
    }

    // 기존 ApplyEquipmentBonus + ActivatePattern + EffectResolver
    // ... (변경 X)
}

private bool CanEquip(EquipmentTemplate tpl, CharacterClass cls) {
    if (tpl.category == WeaponCategory.None) return true;  // 비-무기 슬롯
    return tpl.requiredClass == cls;
}

private CharacterClass ResolvePlayerClass() {
    var ch = MetaProgression.Instance?.SelectedCharacter;
    if (ch == null) return CharacterClass.Warrior;
    return ch.className switch {
        "전사" => CharacterClass.Warrior,
        "궁수" => CharacterClass.Archer,
        "마법사" => CharacterClass.Mage,
        _ => CharacterClass.Warrior,
    };
}
```

**검증 기준**:
- 잘못된 직업 무기 장착 시 LogWarning + 거부
- 올바른 직업 무기는 정상 장착

**완료 조건**:
- Play 검증: 전사 캐릭터로 활 장착 시도 → 거부

---

### 3.2 WeaponClassBonus 정적 헬퍼

**파일**: `Scripts/Gameplay/Equipment/WeaponClassBonus.cs` (신규)

**시너지 표** (메이플 키우기 풍):
| 직업 | 카테고리 | 보너스 |
|---|---|---|
| Warrior | Sword / Axe / Mace / Spear | Damage% +10% |
| Archer | Bow / Crossbow / Dagger / Boomerang | AttackSpeed +5% |
| Mage | Staff / Wand / Tome | MagicDamage +10% |

**API**:
```
public static class WeaponClassBonus {
    public static void Apply(PlayerStats stats, CharacterClass cls, WeaponCategory cat) {
        if (!IsClassMatch(cls, cat)) return;
        switch (cls) {
            case CharacterClass.Warrior: stats.AdditiveDamageBonus += 10f; break;
            case CharacterClass.Archer:  stats.AttackSpeed *= 1.05f; break;
            case CharacterClass.Mage:    stats.MagicDamageMul += 0.10f; break;
        }
    }

    public static bool IsClassMatch(CharacterClass cls, WeaponCategory cat) {
        return (cls, cat) switch {
            (CharacterClass.Warrior, WeaponCategory.Sword or WeaponCategory.Axe or WeaponCategory.Mace or WeaponCategory.Spear) => true,
            (CharacterClass.Archer, WeaponCategory.Bow or WeaponCategory.Crossbow or WeaponCategory.Dagger or WeaponCategory.Boomerang) => true,
            (CharacterClass.Mage, WeaponCategory.Staff or WeaponCategory.Wand or WeaponCategory.Tome) => true,
            _ => false,
        };
    }
}
```

**호출처**: `EquipmentManager.SyncEquippedInstances` 끝부분

**검증 기준**:
- 전사 + Sword 장착 → AdditiveDamageBonus +10
- 전사 + Bow 장착 시도 → 직업 검증으로 차단 (Bonus X)

**완료 조건**:
- 시너지 정확 (단위 테스트 Sprint 5)

### Sprint 3 commit
```
weapon_v2 Sprint 3 — 직업 검증 + WeaponClassBonus 시너지

[검증]
  - EquipmentManager.EquipInstance 진입부 직업 매칭 검증
  - 잘못된 직업 → LogWarning + 거부

[Bonus]
  - WeaponClassBonus 정적 — Damage% / AttackSpeed / MagicDamage
  - SyncEquippedInstances 끝에서 일괄 적용
```

---

## Sprint 4 — UI (2 commit)

### 4.1 InventorySelectionPanel + GridCard 회색·라벨

**파일**: `Scripts/UI/InventoryGridCard.cs` (갱신) + `Scripts/UI/InventorySelectionPanel.cs` (갱신)

**갱신 내역**:

**InventoryGridCard.Bind**:
- 잘못된 직업 무기 → 카드 alpha 0.4 + 자물쇠 아이콘 + 라벨 "전사 전용" / "궁수 전용" / "마법사 전용"
- 라벨 색: requiredClass 색 (전사 빨강 / 궁수 녹 / 마법사 보라)

**Card UI 추가 SerializeField**:
- `_classLockOverlay: GameObject` (자물쇠)
- `_classLabel: Text` (직업 라벨)

**InventorySelectionPanel.CollectMatches 정렬 갱신**:
- 본인 직업 무기 우선 (장착중 → 본인 직업 → 등급 → 강화)

**검증 기준**:
- 잘못된 직업 무기 카드 회색
- 본인 직업 무기 정렬 우선

---

### 4.2 EquipmentCardPanel + GachaResultPanel 직업 표시

**파일**: `Scripts/UI/EquipmentCardPanel.cs` (갱신) + `Scripts/UI/GachaResultPanel.cs` (갱신)

**EquipmentCardPanel 헤더**:
- "심연의 망치 — 전사 / 둔기" (이름 + 직업 + 카테고리)

**GachaResultPanel 결과 표시**:
- 등급 색 + 직업 라벨 (cards 카드 형태로 디스플레이)

**검증 기준**:
- 카드 패널 진입 시 직업+카테고리 표시
- 가챠 결과 직업 라벨 표시

### Sprint 4 commit
```
weapon_v2 Sprint 4 — UI 직업 표시 (회색·라벨·정렬)

[InventorySelectionPanel]
  - 잘못된 직업 무기 카드 alpha 0.4 + 자물쇠
  - 직업 라벨 색 (전사 빨강 / 궁수 녹 / 마법사 보라)
  - 본인 직업 무기 정렬 우선

[EquipmentCardPanel]
  - 헤더 "이름 — 직업 / 카테고리"

[GachaResultPanel]
  - 결과 카드에 직업 라벨
```

---

## Sprint 5 — Test + Validator (1 commit)

### 5.1 WeaponClassBonus + EquipmentTemplate Test

**파일**: `Tests/Editor/WeaponSystemTests.cs` (신규)

**Test 항목** (10):
- IsClassMatch_WarriorSword_True
- IsClassMatch_ArcherBow_True
- IsClassMatch_MageStaff_True
- IsClassMatch_WarriorBow_False
- IsClassMatch_ArcherStaff_False
- IsClassMatch_MageDagger_False
- Apply_Warrior_DamageBoost (Mock PlayerStats)
- Apply_Archer_AttackSpeedBoost
- Apply_Mage_MagicDamageBoost
- WeaponInstance_CanEquip_RoleMatch

### 5.2 ProjectValidator OOP 룰 추가

**파일**: `Scripts/Editor/ProjectValidator.cs` (갱신)

**룰 추가** (ValidateOOPPolicy 안):
- 모든 EquipmentTemplate.asset 의 category != None 검사 (무기 슬롯만)
- requiredClass 와 attackPattern 매칭 검사 (Warrior/Slash·Pulse / Archer/Projectile·Orbit·Trail / Mage/Aura·Rain·Summon·Projectile)
- 위반 시 Warn

**검증 기준**:
- Window > General > Test Runner > Edit Mode > 10 Test PASS
- Tools > Validate Project → 무기 SO 정합 검증

### Sprint 5 commit
```
weapon_v2 Sprint 5 — Test + Validator (검증 자동화)

[WeaponSystemTests — 10 NUnit]
  - IsClassMatch 6 (조합 검증)
  - Apply 3 (Warrior/Archer/Mage 시너지)
  - WeaponInstance.CanEquip 1

[ProjectValidator OOP 룰]
  - 모든 무기 SO requiredClass + category 검증
  - attackPattern × requiredClass 매칭 (Slash → Warrior 등)

누적 NUnit Test 33개 (기존 23 + 10)
```

---

## 의존성 그래프

```
Sprint 1 (enum + Template + IWeapon)
   ↓
Sprint 2 (18 SO 마이그레이션)  ← Sprint 1 EquipmentTemplate 의존
   ↓
Sprint 3 (검증 + Bonus)        ← Sprint 1+2 의존
   ↓
Sprint 4 (UI)                   ← Sprint 1+2 의존 (검증 X 가능)
   ↓
Sprint 5 (Test + Validator)     ← 모두 의존
```

병렬 가능: Sprint 3 + 4 (검증과 UI 독립)

## 완료 정의 (Definition of Done)

각 Sprint:
- ✅ 컴파일 에러 0
- ✅ 기존 게임플레이 회귀 X
- ✅ commit 메시지 형식 준수 (CLAUDE.md §15.10)
- ✅ changelog_v1.md 등재 (Sprint 종료 시)

전체 (5 Sprint 완료):
- ✅ 18 SO 모두 직업+카테고리 wired
- ✅ EquipmentManager 검증 정상 (잘못된 직업 거부)
- ✅ UI 회색·라벨 표시
- ✅ NUnit 10 Test PASS
- ✅ ProjectValidator 신규 룰 통과

## 검증 시나리오 (Play 검증)

### 시나리오 1: 전사 캐릭터 + 활 무기 (잘못된 직업)
1. 전사 (default_boy) 선택
2. 가챠로 활 (storm_bow) 획득
3. 인벤토리 → Weapon 슬롯 → 카드 회색 + 자물쇠 + "궁수 전용"
4. 장착 시도 → 거부 (LogWarning)

### 시나리오 2: 마법사 + 지팡이 (시너지)
1. 마법사 (quiet_friend) 선택
2. 가챠로 룬 지팡이 (rune_staff) 획득
3. 인벤토리 → Weapon 슬롯 → 카드 정상 (밝은 색)
4. 장착 → MagicDamageMul +10% 적용 (PlayerStats 검증)
5. 챕터 진입 → 데미지 ↑ 확인

### 시나리오 3: 직업 변경 자동 해제
1. 전사 선택 + 둔기 장착
2. 다락방에서 마법사로 변경
3. 자동 unequip → 무기 슬롯 빈 상태
4. 마법사 무기 가챠 후 장착

## 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 기존 SaveData 호환 | requiredClass=Warrior default, 마이그레이션 시 Auto fix |
| 직업 변경 시 강제 unequip | OnCharacterChanged hook (별도 사이클) |
| 모든 무기 None 카테고리 (비-무기) 호환 | category == None 시 직업 검증 스킵 |
| 신규 무기 enum 확장 | OCP 정합 — enum 만 갱신 |

## 메이플 키우기 정합

stats_v2 (직업 STR/AGI/WIS 차별화) + weapon_v2 (직업+카테고리 시너지) 결합:

**전사 + Sword 시너지**:
- 캐릭터: STR 2.0× → 100 STR → ATK +10
- 무기 시너지: Damage% +10%
- 합산: ATK +10 × 1.10 = +11

**마법사 + Staff 시너지**:
- 캐릭터: WIS 2.0× → 100 WIS → MagicDmg +10
- 무기 시너지: MagicDamage +10%
- 합산: MagicDmg +10 × 1.10 = +11

→ 직업 + 무기 카테고리 시너지 = **메이플 키우기 표준 패턴**

## 11. 변경 이력
- 2026-05-03: 초기 작성 (PRD 5 Sprint 일괄)

## 12. 참조

- `weapon_system_v2.md` — 디자인 문서 (분석 + OOP 재설계)
- `stats_v2_infinite.md` — 3 직업 차별화 (STR/AGI/WIS)
- `gacha_v1.md` — 18 무기 가챠 풀
- `architecture_analysis_v1.md` — Unity OOP 평가
- `CLAUDE.md` §15 — OOP 정책 (필수 준수)
