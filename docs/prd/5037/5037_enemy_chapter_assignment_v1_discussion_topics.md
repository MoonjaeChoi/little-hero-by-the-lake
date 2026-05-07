# 5037 enemy_chapter_assignment_v1 PRD Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목

## High Priority Items

(없음 — 자동 수용 처리됨)

## Medium Priority Items

### D1. Validator 룰 — 통합 vs 분리

- **Original Criticism**: §15.1 SRP 강화 시 룰 분리 권장
- **Current Position (PRD)**: 1 클래스 `EnemyVariantConsistencyRule` 에 두 검사 (Splitter + Projectile)
- **Discussion Points**:
  - 옵션 A: **1 클래스 50L 통합** (현재 PRD) — 단일 응집 + §15.6 100L 정합. 5022 의 단일 룰 클래스 답습.
  - 옵션 B: **2 클래스 분리** (`SplitterConsistency` + `ProjectileConsistency`) — §15.1 SRP 강화. 단점: 작은 클래스 다중 → over-engineering
  - 옵션 C: **기존 OOPPolicyRule 통합** — 응집 ↑, 단점: 룰 카테고리 mix
- **Proposed Resolution**: 옵션 A — 단일 50L, §15 정합 모두 통과. 5022 답습.
- **PRD Impact**: §2.2 표기 (옵션 A 시 변경 없음)

### D2. CH5 ChapterData 매칭 방식

- **Original Criticism**: 파일명 매칭 강함 vs 약함
- **Current Position (PRD)**: `Chapter_05_<TBD>.asset` find 로 결정
- **Discussion Points**:
  - 옵션 A: `find` + chapterNumber 매칭 (현재 PRD) — 동적
  - 옵션 B: 파일명 명시 (사전 분석 필요) — 정적
  - 옵션 C: **Seeder 가 `AssetDatabase.FindAssets("t:ChapterData")` + chapterNumber 스캔** — 5037.1+ reuse ↑
- **Proposed Resolution**: 옵션 C — 35 챕터 일괄 적용 시 reuse 최강. 시드 도구가 모든 ChapterData SO 스캔 → chapterNumber 매칭.
- **PRD Impact**: §2.1 Seeder 명세에 `WireChapterMiniBoss(int chapterNum, ...)` 가 chapterNumber 스캔 명시

### D3. 룰 매트릭스 SO 화 시점

- **Original Position**: PRD 본체 = 정적 룰 (Seeder 코드 내 if-else). §15.4 SO 우선 약 위반 가능.
- **Discussion Points**:
  - 옵션 A: **5037 본체 = 정적 룰** + 5037.1 cascade 시 SO 도입 — 단계 도입
  - 옵션 B: **5037 본체 = ChapterVariantRule SO** 즉시 도입 — §15.4 정합 강화. 단점: SO + Seeder 동시 변경 → 작업량 ↑
  - 옵션 C: **둘 다** (Seeder 코드 + SO 백업) — 중복
- **Proposed Resolution**: 옵션 A — 시범 본체는 정적 룰 (간단), 후속 cascade 시 SO 도입. 5037.1 backlog 명시.
- **PRD Impact**: §2.1 Seeder 룰 정적 if-else 표기

## Low Priority Items

(없음)

## 요약
- Total Items: 3
- High: 0 / Medium: 3 / Low: 0
- 권장 mode: ai-assisted (Claude 권장 + 사용자 확인)

권장안 요약:
- **D1=A** (1 클래스 통합 50L)
- **D2=C** (chapterNumber 스캔)
- **D3=A** (정적 룰 + 5037.1 cascade SO)
