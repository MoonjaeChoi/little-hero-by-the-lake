# Sprint Contract: 5029

## C1 컴파일

errors=0, 신규 9 파일 + 수정 (EquipmentTemplate/EnemyData/IAttackPattern/DamageDealer/EnemyController/EquipmentPatternFactory/AttackPatternType enum)

## C2 NUnit

158 → 164 (+6 ElementResolverTests)

## C3 Validator

- OOPPolicyRule warnings 변동 ≤ 0
- 신규 룰: 마법사 무기 9개 element != None 검증

## C4 Reflection

- `LittleHero.MagicElement` enum 5종 (None/Fire/Ice/Lightning/Arcane)
- `LittleHero.ElementResolver` 정적 + ResolveMultiplier/ApplyElementEffect
- `LittleHero.BeamAttackPattern / ChainAttackPattern / VortexAttackPattern / MeteorAttackPattern` IAttackPattern 구현
- `LittleHero.ElementVfxBank` 정적 + Spawn(MagicElement, Vector3)
- `LittleHero.AttackPatternType` enum +Beam/Chain/Vortex/Meteor (총 13)

## C5 SO 검증

- EquipmentTemplate SO 24 → 25 (weapon_thunder_wand 추가)
- 마법사 무기 9개 element != None
- 분포: Fire 2 / Ice 2 / Lightning 2 / Arcane 3
- EnemyData 262 모두 elementResistance.Length == 4

## C6 Play 시나리오

### 시나리오 1 — 마이그레이션
Tools/LittleHero/Migrate Element System → 262 EnemyData 갱신

### 시나리오 2 — 마법사 무기 매핑
Tools/LittleHero/Seed Thunder Wand + Migrate Mage Weapon Elements → 9 무기 element 부여

### 시나리오 3 — Play 마법사 전투
CH1 진입 → 마법사 무기 활성화 → element 효과 (DOT/둔화/사슬/무시) 발동 + 콘솔 로그

### 시나리오 4 — UI 검증
Inventory + GachaResult → element 아이콘 표시

### 시나리오 5 — Validator
warnings 변동 ≤ 0 + 마법사 무기 element 분포 보고

## 합격 기준

최소: C1+C3+C4 + 시나리오 1+2
완전: 모든 + 시나리오 3+4+5
