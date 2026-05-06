# 5026 Sprint PRD — boss_attack_type_migration

> **PRD v1 (초안)** · 디자인: `docs/prd/5026_boss_attack_type_migration_prd.md`
> 옵션 A 채택 — Editor 메뉴 일괄 마이그레이션

---

## 1. 전체 개요

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | BossAttackTypeMigrator (Editor) + 96 SO 일괄 적용 | 1 | 30분 |
| 2 | NUnit Test 4 + Validator 회귀 검증 | 1 | 30분 |
| 3 | changelog + CLAUDE.md §14 | 1 | 10분 |
| **합계** | | **3** | **1h 10m** |

---

## 2. Sprint 1 — BossAttackTypeMigrator + 일괄 적용 (1 commit)

### 2.1 Editor 도구 신규

**파일**: `LittleHero/Assets/_Project/Scripts/Editor/BossAttackTypeMigrator.cs` (~80L)

**산출물**:
- `LittleHero.EditorTools.BossAttackTypeMigrator` 정적 클래스
- `[MenuItem("Tools/LittleHero/Migrate Boss Attack Type")]` 진입점
- `CategoryMap` static readonly Dictionary<string, BossAttackType>:
  - "Meta" → Summon, "Nature" → Vine, "Animal" → Charge, "Civ" → Pulse, "Fantasy" → Spread
- `V1FairytaleNames` static readonly HashSet<string>: V1 11종 (Meta 분류)
- `ResolveByName(string assetName)` — V2 archetype regex 매칭 (Enemy_(Meta|Nature|...))
- `ResolveByChapterId(string chapterId)` — CH(\d+) 매칭 → (n-1) % 5 분할 → 카테고리 (Hardcore 통합)
  - 1~5 Meta / 6~14 Nature / 15~20 Animal / 21~27 Civ / 28~35 Fantasy
  - Hardcore: 36~50 = (n-1) % 35 적용 후 동일 매핑
- `Run()` — 96 SO 일괄 갱신:
  1. AssetDatabase.FindAssets("t:LittleHero.EnemyData") 로 모든 EnemyData
  2. 각 SO 의 `bossTier != None && bossAttackType == None` 만 대상
  3. ResolveByName + ResolveByChapterId 매칭 → 미매칭 시 Charge fallback
  4. EditorUtility.SetDirty + AssetDatabase.SaveAssets
  5. EditorUtility.DisplayDialog: "Migrated N SO (Meta:X / Nature:Y / Animal:Z / Civ:W / Fantasy:V / fallback:U)"

**검증 기준**:
- 컴파일 errors=0
- 96 SO 갱신 후 0 미설정 잔존
- 기존 wiring (DailyBoss/WeeklyBoss/ChapterBoss 41) 보존

**완료 조건**:
- Tools 메뉴 실행 → DisplayDialog "96 SO 갱신" 보고
- ProjectValidator C3 재실행 시 BossSystem warnings 96 → 0

#### Sprint 1 commit

```
5026 Sprint 1 — BossAttackTypeMigrator + 96 SO 일괄 적용 (1/3)

[변경]
  + Editor/BossAttackTypeMigrator.cs (~80L)
    - Tools/LittleHero/Migrate Boss Attack Type 메뉴
    - CategoryMap (5 카테고리) + V1FairytaleNames (11)
    - ResolveByName V2 archetype regex
    - ResolveByChapterId CH 35 + Hardcore 36~50 매핑
    - 96 SO 일괄 갱신 + EditorUtility.DisplayDialog 보고

[데이터]
  - 96 EnemyData.asset bossAttackType 갱신
    Meta:N / Nature:M / Animal:K / Civ:J / Fantasy:V / fallback:U

[검증]
  - 기존 wiring (41 SO) 보존
  - bossAttackType 필드 0건 잔존

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 3. Sprint 2 — NUnit Test 4 + Validator (1 commit)

### 3.1 NUnit 테스트 신규

**파일**: `LittleHero/Assets/_Project/Tests/Editor/BossAttackTypeMigratorTests.cs` (~50L)

**산출물**:
- 4 NUnit Test (NUnit 누적 153 → 157):
  1. `ResolveByName_MetaArchetype_ReturnsSummon` — "Enemy_Meta_Boss" → Summon
  2. `ResolveByName_V1Fairytale_ReturnsSummon` — "Enemy_BeanGiant" → Summon
  3. `ResolveByChapterId_CH06_ReturnsVine` — "ch_06_xxx" → Vine (Nature 카테고리)
  4. `ResolveByChapterId_HardcoreCH36_ReturnsSummon` — Hardcore CH36 → CH1 카테고리 (Meta)

**검증 기준**:
- 모두 PASS
- TestUtils 활용 (5024 헬퍼)

### 3.2 Validator 회귀 검증

**기존 룰 검증** (코드 변경 0):
- WeaponBossRule.ValidateBossSystem 의 `bossAttackType=None` 검사 → 96 → 0 자동 검증
- ProjectValidator C3 재실행 시 warnings 171 → ~75 감소 기대

**검증 기준**:
- ProjectValidator C3 errors=0 + warnings 변동 = -96 (96건 감소)

#### Sprint 2 commit

```
5026 Sprint 2 — NUnit Test 4 + Validator 회귀 검증 (2/3)

[변경]
  + Tests/Editor/BossAttackTypeMigratorTests.cs (~50L)
    - ResolveByName_MetaArchetype_ReturnsSummon
    - ResolveByName_V1Fairytale_ReturnsSummon
    - ResolveByChapterId_CH06_ReturnsVine
    - ResolveByChapterId_HardcoreCH36_ReturnsSummon

[NUnit 누적] 153 → 157

[검증]
  - WeaponBossRule.ValidateBossSystem warnings 96 → 0
  - ProjectValidator C3 warnings 171 → ~75 (-96)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. Sprint 3 — changelog + CLAUDE.md (1 commit)

### 4.1 docs 갱신

**파일**:
- `docs/little_hero/changelog_v1.md` — 5026 항목 추가 (1 줄)
- `CLAUDE.md` §14 직전 5건 — 5026 헤드 추가 + 5024 또는 5025 제거

#### Sprint 3 commit

```
5026 Sprint 3 — changelog + CLAUDE.md §14 갱신 (3/3)

[변경]
  - docs/little_hero/changelog_v1.md +5026 항목
  - CLAUDE.md §14 직전 5건 갱신

[5026 종결 요약]
  - bossAttackType=None 잔존 96 → 0
  - ProjectValidator warnings 171 → ~75 (-56% 감소)
  - 5001 IBossAttackPattern 7종 인프라 활성

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 5. 의존성 그래프

```
Sprint 1 (Migrator + 96 SO 갱신)
   ↓
Sprint 2 (NUnit + Validator 회귀)
   ↓
Sprint 3 (docs)
```

병렬 가능: 없음 (순차).

## 6. 완료 정의 (DoD)

- [ ] Editor/BossAttackTypeMigrator.cs 신규 (~80L)
- [ ] Tools 메뉴 실행 → 96 SO 갱신 보고
- [ ] 137 보스 SO 모두 bossAttackType != None
- [ ] NUnit 4 PASS (153 → 157)
- [ ] ProjectValidator C3 warnings 171 → ~75
- [ ] changelog_v1.md + CLAUDE.md §14 갱신

## 7. 검증 시나리오 (Play 검증)

### 시나리오 1 — Migrator 실행 + 일괄 적용

**사전 조건**: Unity Editor 열기

**단계**:
1. `Tools/LittleHero/Migrate Boss Attack Type` 실행
2. EditorUtility.DisplayDialog 확인 (96 SO 갱신 보고)
3. AssetDatabase.SaveAssets 자동 저장

**기대 결과**:
- 콘솔 로그: `[BossAttackTypeMigrator] 96 SO 갱신 (Meta:X / Nature:Y / ...)`
- 96 EnemyData.asset 의 bossAttackType 필드 != 0
- 기존 wiring (DailyBoss 7 + WeeklyBoss 4 + ChapterBoss 30) 보존

### 시나리오 2 — Validator 회귀

**사전 조건**: Sprint 1 완료 후

**단계**:
1. `Tools/LittleHero/Validate Project` 실행
2. 콘솔 warnings 카운트 확인

**기대 결과**:
- BossSystem 카테고리 warnings = 0 (이전 96)
- 종합 warnings: 171 → ~75 (-96)

### 시나리오 3 — Boss 행동 검증 (선택)

**사전 조건**: Chapter 1 진입

**단계**:
1. CH1 wave 5 (MiniBoss) 진입
2. MiniBoss 출현 시 BossPatternFactory 인스턴스화 확인

**기대 결과**:
- 콘솔 로그: `[BossPhaseManager] BossSummonAttack instantiated` (Meta CH1 = Summon)
- MiniBoss 가 작은 적 소환 시작

## 8. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| V1 11종 매핑 누락 (HashSet typo 등) | NUnit `ResolveByName_V1Fairytale_ReturnsSummon` 검증 |
| chapterId regex mismatch (특수 케이스) | fallback Charge default + ProjectValidator 사후 검증 |
| 디자이너 후속 조정 (예 Animal MiniBoss 를 Pulse 로) | SO Inspector 직접 변경 가능 (마이그레이션은 None → 채움만) |
| Hardcore CH36~50 매핑 mismatch | (n-1) % 35 후 (n-1) % 5 분할 — 같은 카테고리 유지 |

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 옵션 A 채택, 3 Sprint / 1h10m |
