# Little Hero — 다음 작업 Backlog (v1)

> **운영 규칙**:
> - System 번호 (5021~) 으로 분리. PGE 6-Phase 또는 약식 cycle 적용.
> - 완료 시 → `changelog_v1.md` 로 이동 + 본 파일에서 ✅ 마크.
> - 신규 발견 시 본 파일 끝에 추가, 우선순위 재정렬.
> - CLAUDE.md §13 은 본 파일의 요약 view (3~5건만 maintain).
>
> **2026-05-06 정책 변경**: 출시 (5026 ios_build_testflight) 는 완성도 미흡으로 보류. 당분간 개발/개선 집중. 5026~5028 일시 제거 — 5021~5025 완료 + 시스템 안정성 확인 후 재등재.

---

## 우선순위 (2026-05-06 갱신)

| # | 시스템 | 우선순위 | scope | 예상 | 의존성 | PRD |
|---|---|---|---|---|---|---|
| **5021** | starter_hero_chapter_flow | 🔴 Critical | 5005 R3+R4 fix → 5020 C6 시나리오 2b 완성 | 2h15m | 5020 | [디자인](../prd/5021_starter_hero_chapter_flow_prd.md) · [Sprint](../prd/5021/5021_starter_hero_chapter_flow_prd.md) |
| **5022** | oop_validator_split | 🟠 High | ProjectValidator 650L → 책임별 분리 (룰 자기위반 해소) | 2h30m | - | [디자인](../prd/5022_oop_validator_split_prd.md) · [Sprint](../prd/5022/5022_oop_validator_split_prd.md) |
| **5023** | oop_metaprogression_split | 🟠 High | MetaProgression 331L → 책임 분리 (5020 init 한계 정합 보강) | 2h15m | 5021 | [디자인](../prd/5023_oop_metaprogression_split_prd.md) · [Sprint](../prd/5023/5023_oop_metaprogression_split_prd.md) |
| **5024** | editor_test_expansion | 🟡 Medium | NUnit 117 → 144 (회귀 안전망 강화) | 5h30m | - | [디자인](../prd/5024_editor_test_expansion_prd.md) · [Sprint](../prd/5024/5024_editor_test_expansion_prd.md) |
| **5025** | ui_korean_polish | 🟡 Medium | G3 한국어 검수 + Legacy Text 폰트 잘림 spot fix | 2h30m | - | [디자인](../prd/5025_ui_korean_polish_prd.md) · [Sprint](../prd/5025/5025_ui_korean_polish_prd.md) |

**합계**: 약 14h45m / 17 commit / 5 시스템

---

## 5021 — starter_hero_chapter_flow 🔴 Critical

> **5020 직접 cascade. C6 시나리오 2b 완성.**

5020 evaluation 에서 발견된 5005 attic_decomposition_v1 회귀 2건 (R3 AtticChapterEntry._buttons[] / R4 MetaProgression init 책임).

**산출 (예상)**:
- `Data/CharacterPool.cs` (~15L SO 인덱스)
- `Data/Characters/CharacterPool_Default.asset`
- `Meta/MetaProgression.cs` Awake +15L (ResolveSelectedCharacterFromSave)
- `Scenes/Attic.unity` MetaProgression._characterPool wiring + AtticChapterEntry._buttons[] 결정
- `Tests/Editor/MetaProgressionInitTests.cs` (~50L NUnit 3)
- ProjectValidator ValidateCharacterPool 룰

**검증**: NUnit 117 → 120, 사용자 시나리오 1~3 PASS.

**권장 발화**: `/prd-pipeline 5021` 또는 `5021 시작`

---

## 5022 — oop_validator_split 🟠 High

> **§15.6 100줄 룰 자기위반 해소.**

ProjectValidator.cs 650L → 11 Rule 클래스 + Issue/Severity public 분리.

**산출 (예상)**:
- `Editor/ProjectValidator/Issue.cs` (15L public struct + enum)
- `Editor/ProjectValidator.cs` (650L → 80L, Run + Print 만)
- `Editor/ProjectValidator/Rules/<11 클래스>` (각 40~85L)

**검증**: C3 결과 분리 전후 동일 (errors=0, warnings=172).

**권장 발화**: `/prd-pipeline 5022` 또는 `5022 시작`

---

## 5023 — oop_metaprogression_split 🟠 High (5021 의존)

> **MetaProgression 331L 7+ 책임 분리.**

7 service 분리 (Loader / Bootstrapper / Character / Chapter / Currency / Equipment / Endgame).

**산출 (예상)**:
- `Meta/MetaProgressionLoader.cs` (50L) — 5021 ResolveSelectedCharacter 흡수
- `Meta/MetaProgressionBootstrapper.cs` (70L)
- `Meta/MetaProgressionCharacter.cs` (50L)
- `Meta/MetaProgressionChapter.cs` (30L)
- `Meta/MetaProgressionCurrency.cs` (80L)
- `Meta/MetaProgressionEquipment.cs` (70L)
- `Meta/MetaProgressionEndgame.cs` (50L)
- `Meta/MetaProgression.cs` (331L → 80L)
- `Tests/Editor/MetaProgressionServicesTests.cs` (60L NUnit 6)

**검증**: NUnit 117 → 123, 호출처 30+ 마이그레이션, MetaProgression OOP warning 제거.

**권장 발화**: `/prd-pipeline 5023` (5021 PASS 후)

---

## 5024 — editor_test_expansion 🟡 Medium

> **NUnit 117 → 144 (+27).**

4 service 단위 테스트 신규.

**산출 (예상)**:
- `Tests/Editor/DamageQueryTests.cs` (~80L NUnit 6)
- `Tests/Editor/EquipmentManagerTests.cs` (~110L NUnit 7)
- `Tests/Editor/PlayerStatsTests.cs` (~90L NUnit 6)
- `Tests/Editor/ItemDropServiceTests.cs` (~140L NUnit 8)

**검증**: NUnit 117 → 144 PASS, 회귀 0.

**권장 발화**: `/prd-pipeline 5024`

---

## 5025 — ui_korean_polish 🟡 Medium

> **G3 한국어 검수 + 폰트 spot fix.**

KoreanLabelAuditor Editor 도구 + 9 우선순위 영역 spot fix.

**산출 (예상)**:
- `Editor/KoreanLabelAuditor.cs` (~80L)
- 보고서 `docs/little_hero/korean_label_audit_report.md`
- 9 영역 SO/Panel 영문 → 한글 갱신
- 폰트 잘림 spot fix N건

**검증**: KoreanLabelAuditor 영문 잔존 0, 사용자 시각 검증.

**권장 발화**: `/prd-pipeline 5025`

---

## 권장 진행 순서

```
5021 (Critical, 2h15m, 5020 cascade)
  ↓
5022 (High, 2h30m, 독립)
  ↓
5023 (High, 2h15m, 5021 의존)
  ↓ + 병렬 가능
5024 (Medium, 5h30m, 독립)
5025 (Medium, 2h30m, 독립)
```

**총**: ~14h45m / 17 commit / 5 시스템

---

## 변경 이력

| 일자 | 변동 |
|---|---|
| 2026-05-06 | 초기 backlog 생성 — 5021~5028 |
| 2026-05-06 | 5021~5025 PRD 작성 완료 (디자인 5 + Sprint 5 = 10 docs). 5026~5028 제거 (출시 보류 정책) |
