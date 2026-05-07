# 5037 enemy_chapter_assignment_v1 PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 적용
> 일시: 2026-05-07

## 1. 요약 표

| Cat | Criticism | Pri | Status | Action | §15 |
|---|---|---|---|---|---|
| §15.6 | Seeder ~120L | High | Accepted | 분리 (60L+60L) — 5022 답습 | ✅ |
| §15.1 | Validator 룰 분리 | Medium | Discuss → mediator (D1) | 사용자 결정 | △ |
| 매칭 | CH5 파일명 매칭 | Medium | Discuss → mediator (D2) | 사용자 결정 | △ |
| 룰 매트릭스 SO화 | Medium | Discuss → mediator (D3) | 사용자 결정 (5037 본체 vs 5037.1 cascade) | △ |

## 2. 품질 메트릭

- 총 Discussion Points: 3 (D1: Validator 룰 / D2: CH5 매칭 / D3: 룰 SO화 시점)
- Accepted: 1 (§15.6 Seeder 분리 — 자동 수용)
- Rejected: 0
- Discuss: 3
- §15 정합률: 95% (D1 결정 후 100%)

## 3. Detailed Responses

### 3.1 Seeder 분리 — High Priority — Accepted ✅

**Original**: Seeder ~120L → §15.6 100L 룰 위반 가능

- **Rebuttal**: 자동 수용 — §15.6 절대 원칙. 5022 oop_validator_split / 5023 oop_metaprogression_split 답습.
- **Proposed Action**: PRD §2.1 갱신 — `ChapterVariantSeeder` (진입점, 60L) + `VariantSoFactory` (정적 헬퍼, 60L) 분리.
- **PRD Impact**: §2.1 명세 갱신, §6 OOP 정합표 §15.1 SRP 정합 추가.
- **Little Hero Context**: 5022 ProjectValidator 666L → 45L orchestrator 답습 (분리 후 호출처 0 변경).

### 3.2 D1 Validator 룰 분리 — Discuss

**Original**: 1 클래스에 2 룰 통합 vs 분리

- **Status**: Discuss → discussion_topics
- **Current Position**: PRD 옵션 A (1 클래스 50L) 진행 시 §15.6 정합 OK. 분리 시 over-engineering 우려.
- **Resolution Path**: mediator ai-assisted

### 3.3 D2 CH5 파일명 매칭 — Discuss

**Original**: chapterNumber 매칭 옵션 A/B/C

- **Status**: Discuss → discussion_topics
- **Current Position**: 옵션 C (chapterNumber scan) 가 35 챕터 reuse 에 유리.
- **Resolution Path**: mediator ai-assisted

### 3.4 D3 룰 매트릭스 SO 화 시점 — Discuss

**Original**: 룰 (CH N % 5) 을 SO 화? — §15.4 SO 우선

- **Status**: Discuss (신규 — critique §5 §15.4 indirectly)
- **Current Position**: PRD 본체 = 정적 룰 (Seeder 코드 내 if-else). 5037.1 cascade 시 SO 도입 권장.
- **Resolution Path**: mediator — 5037 본체 vs 5037.1 분리 결정

## 4. PRD v2 적용 변경 (Accepted only)

- §2.1 — `ChapterVariantSeeder` ~120L → `ChapterVariantSeeder` (60L 진입점) + `VariantSoFactory` (60L 정적 헬퍼) 분리
- §6 — §15.1 SRP 항목에 "Seeder 분리 (Seeder + Factory)" 명시
- §1 Sprint 표 — S2 변경 없음 (1 commit 내 두 클래스 동시 신규)

(D1/D2/D3 미반영 — mediator 후 최종)

## 5. 변경 이력
- 2026-05-07: 초기 작성 (Phase 2.2 Rebuttal)
