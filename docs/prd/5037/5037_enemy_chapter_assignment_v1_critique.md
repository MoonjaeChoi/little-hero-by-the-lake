# 5037 enemy_chapter_assignment_v1 PRD 리뷰

## 0. 결정 이력
- 2026-05-07: Claude Code 자동 리뷰 (Phase 2.1 Critique)

## 1. 종합 점수: 8.6/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 8/10 | Seeder ~120L 추정 — 분리 권장 (D1) |
| PG-12 톤 | 10/10 | Bomber/Splitter 만화풍 자폭/분열 ✅ |
| 성능 | 9/10 | Splitter 1→4 분열 cap 영향 — miniBoss 1마리 한정으로 완화 |
| 시스템 통합 | 9/10 | 5035 인프라 답습 + ChapterData 변경 X |
| Sprint 분해 | 8/10 | 5 Sprint 균형 OK. S2 Seeder + Rule 분리 검토 (D2) |

## 2. Accept 항목 (4)

- ✅ **옵션 A 채택** (변형 SO + ChapterData.miniBossPool 답습) — 5035 인프라 정합 + ChapterData 변경 0
- ✅ **시범 + 점진 확장 분리** — 5037 본체 CH3/4/5, 5037.1+ 후속 — 5022/5028 답습 (incremental)
- ✅ **Validator 룰 신규 2** (Splitter/Projectile consistency) — §15.12 정합 자동화
- ✅ **NUnit 6 분리 Sprint** — §15.9 정합

## 3. Reject 항목 (0)

(없음)

## 4. Discuss 항목 (3)

### 🟡 D1. ChapterVariantSeeder ~120L — 분리 vs 통합

**Original**: PRD §2.1 — Seeder 클래스 ~120L 추정. §15.6 100줄 룰 위반 가능.

- **Current Position**: PRD 는 "메서드 짧게 분리, 영역 grouped" 표기 — 한 클래스 내 정적 헬퍼.
- **Discussion Points**:
  - 옵션 A: 한 클래스 + 영역 grouped (현재 PRD) — 100L 위반 가능 (가독성↑)
  - 옵션 B: `ChapterVariantSeeder` (60L 진입점) + `VariantSoFactory` (60L 정적 헬퍼) 분리 — §15.1 SRP 정합
  - 옵션 C: 큰 클래스 그대로 + `// disable warning ProjectValidator-LineCount` (5022 답습)
- **Proposed Resolution**: **옵션 B** — 5022 답습 + §15.6 100L 룰 정합. Seeder 진입점 + Factory 정적 헬퍼.

### 🟡 D2. ProjectValidator 룰 — 통합 vs 분리

**Original**: PRD §2.2 — 신규 룰 2종 (`EnemyVariantConsistencyRule`).

- **Current Position**: 1 클래스에 두 룰 통합.
- **Discussion Points**:
  - 옵션 A: 1 클래스에 2 룰 통합 (현재 PRD) — 50L 이내, §15.6 정합
  - 옵션 B: 룰별 분리 (`SplitterConsistency` + `ProjectileConsistency`) — §15.1 SRP 강화 (5022 11 룰 답습)
  - 옵션 C: 기존 `OOPPolicyRule` 에 추가 — 응집 ↑
- **Proposed Resolution**: **옵션 A** — 50L 이내 단일 클래스, 5022 의 `EquipmentClassRule` 같은 단일 클래스 답습. 분리 시 작은 클래스 다중 생성 — over-engineering.

### 🟡 D3. CH5 ChapterData 파일 매칭

**Original**: PRD §3.2 — `Chapter_05_<TBD>.asset` chapterNumber=5 매칭.

- **Current Position**: 파일명 unknown — find 로 결정.
- **Discussion Points**:
  - 옵션 A: `find` + chapterNumber 매칭 (현재 PRD) — 동적 매칭, 파일명 변동 강함
  - 옵션 B: 정확한 파일명 명시 (예 `Chapter_05_TheUglyDuckling.asset`) — 사전 명시
  - 옵션 C: 시드 도구가 chapterNumber 로 모든 ChapterData SO 스캔 — Seeder 정적 헬퍼
- **Proposed Resolution**: **옵션 C** — Seeder 가 `AssetDatabase.FindAssets("t:ChapterData")` + chapterNumber 매칭. 파일명 무관 + 35 챕터 일괄 적용 시 reuse ↑.

## 5. CLAUDE.md §15 정책 위반 점검

- §15.1 SRP: Seeder 분리 검토 (D1)
- §15.4 SO 우선: 룰 매트릭스도 SO 화 검토 (5037.1 cascade 에서 ChapterVariantRule SO 도입 권장)
- §15.6 100L: Seeder 분리 (D1) + Validator 룰 단일 50L (D2 옵션 A) 정합
- §15.7 DI: Editor 정적 — 불필요
- §15.9 Test: NUnit 6 분리 Sprint ✅
- §15.11 안티: public 필드 X / Update X (Editor) ✅
- §15.12 Validator: 신규 룰 2 ✅

종합: §15 정합 (D1 분리 시 완전 정합)

## 6. 권장 변경

- **즉시 변경** (Accept):
  - §2.1 Seeder 의 `예상 라인` 표기에 "분리 시 60L+60L" 추가 표기
  - §6 OOP 정합 표 의 §15.1 항목에 "Seeder 분리" 명시
- **사용자 결정 후 변경** (Discuss):
  - D1: 분리 vs 통합
  - D2: Validator 1 클래스 vs 분리
  - D3: CH5 파일 매칭 방식

## 7. 변경 이력
- 2026-05-07: 초기 작성 (Phase 2.1 Critique)
