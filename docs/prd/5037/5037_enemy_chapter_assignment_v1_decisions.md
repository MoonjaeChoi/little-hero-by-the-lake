# 5037 enemy_chapter_assignment_v1 PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 3건 결정 기록
> 모드: ai-assisted (사용자 "권장 그대로") · 일시: 2026-05-07

## 1. 요약 표

| Pri | Item | Topic | Decision | Status |
|---|---|---|---|---|
| Medium | D1 | Validator 룰 분리 | **옵션 A — 1 클래스 50L 통합** | Done |
| Medium | D2 | CH5 매칭 방식 | **옵션 C — chapterNumber 스캔** | Done |
| Medium | D3 | 룰 매트릭스 SO화 시점 | **옵션 A — 5037 본체 정적, 5037.1 cascade SO** | Done |

## 2. Detailed Decisions

### 2.1 D1 — Validator 룰 통합

- **Decision**: 1 클래스 `EnemyVariantConsistencyRule` (50L 추정)
- **Mode**: ai-assisted (사용자 "권장 그대로")
- **Rationale**: 5022 의 단일 룰 클래스 (예 EquipmentClassRule) 답습. 50L 이내 단일 응집 + §15.6 100L 정합. 분리 시 over-engineering.
- **PRD Impact**: §2.2 변경 없음 (옵션 A = 현재 PRD).
- **Trade-off**: §15.1 SRP 강화 손실 — 두 검사가 같은 EnemyData SO 대상이라 응집 ↑로 보상.

### 2.2 D2 — CH5 매칭 (chapterNumber 스캔)

- **Decision**: Seeder 가 `AssetDatabase.FindAssets("t:ChapterData")` + chapterNumber 매칭
- **Mode**: ai-assisted
- **Rationale**: 35 챕터 일괄 적용 시 reuse 최강. 파일명 변동에 강함. 5037.1+ cascade 의 핵심 헬퍼.
- **PRD Impact**: §2.1 Seeder 명세에 `LoadChapterByNumber(int chapterNum) -> ChapterData` 헬퍼 추가.
- **Trade-off**: AssetDatabase 스캔 비용 — Editor only + 1회 호출이라 무시 가능.

### 2.3 D3 — 룰 매트릭스 정적 (5037 본체) + SO 후속

- **Decision**: 5037 본체 = Seeder 코드 내 정적 if-else. 5037.1 cascade 에서 ChapterVariantRule SO 도입.
- **Mode**: ai-assisted
- **Rationale**: 시범 본체는 단순성 우선 (CH3/4/5 만). 35 챕터 룰 SO 화는 5037.1 본격 확장 시 의미 있음.
- **PRD Impact**: §2.1 Seeder 명세에 정적 if-else 명시. §10 backlog 에 5037.1 ChapterVariantRule SO 도입 명시.
- **Trade-off**: §15.4 SO 우선 약 위반 — 5037.1 cascade 에서 보강.

## 3. 통계

- Total Items: 3
- AI Recommendations Followed: 3/3 (100%)
- User Overrides: 0
- 모드: ai-assisted ("권장 그대로")

## 4. v2_prd.md 변경 diff 요약

(rebuttal 단계 → mediator 최종)

- §2.1 — `ChapterVariantSeeder` (60L 진입점) + `VariantSoFactory` (60L 정적 헬퍼) 분리 (rebuttal 자동 수용)
- §2.1 — Seeder 의 `LoadChapterByNumber(int)` 헬퍼 명시 (D2)
- §2.1 — 룰 매트릭스 정적 if-else (D3 — 5037.1 cascade backlog)
- §2.2 — Validator 1 클래스 통합 (D1 — 변경 없음)
- §10 backlog — 5037.1 ChapterVariantRule SO 도입 (D3)

## 5. 변경 이력
- 2026-05-07: 초기 작성 (Phase 2.3 Mediator)
