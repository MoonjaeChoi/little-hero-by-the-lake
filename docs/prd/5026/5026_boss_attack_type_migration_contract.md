# Sprint Contract: 5026

> PGE Phase 4 · 2026-05-06

## C1 컴파일

- BossAttackTypeMigrator.cs + BossAttackTypeMigratorTests.cs errors=0

## C2 Editor Test

- NUnit 153 → 157 (+4)
- 신규 4: ResolveByName_MetaArchetype_ReturnsSummon / ResolveByName_V1Bear_ReturnsCharge / ResolveByChapterId_CH06_ReturnsVine / ResolveByChapterId_HardcoreCH36_ReturnsSummon

## C3 ProjectValidator

- errors=0
- warnings 171 → ~75 (-96 BossSystem)

## C4 Reflection

- `LittleHero.EditorTools.BossAttackTypeMigrator` 정적
- `[MenuItem("Tools/LittleHero/Migrate Boss Attack Type")]` Run() static
- `ResolveByName(string)` / `ResolveByChapterId(string)` static

## C5 SO 검증

- 137 EnemyData (bossTier != None) 모두 bossAttackType != None
- 기존 wiring 41 (DailyBoss 7 + WeeklyBoss 4 + ChapterBoss 30) 보존

## C6 Play 시나리오

### 시나리오 1 — Migrator 일괄 적용
사용자: Tools/LittleHero/Migrate Boss Attack Type 실행 → DisplayDialog 96 갱신 보고

### 시나리오 2 — Validator 회귀
warnings 171 → ~75

### 시나리오 3 — Boss 행동 (선택)
CH1 wave 5 MiniBoss 출현 → BossSummonAttack 인스턴스화 (Meta = Summon)

## 합격 기준

최소: C1+C3+C4 + 시나리오 1+2
완전: C1~C5 모두 + 시나리오 3
