# 5020 Starter Hero Visual — PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 2건 결정 기록
> **모드**: ai-assisted
> **일시**: 2026-05-06

---

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| Medium | D-C | character_progression_v1.md 갱신 범위 | **A — 별도 PR (5020.1 후속)** | Done |
| Medium | D-D | References/Selected_Characters/ 처리 | **B — `Selected_Characters_v1_legacy/` rename** | Done |

---

## 2. Detailed Decisions

### 2.1 D-C — character_progression_v1.md 갱신 범위

- **Decision**: **옵션 A — 별도 PR (5020.1 후속)**
- **Mode**: ai-assisted (Claude 권장 채택)
- **Rationale**:
  - CLAUDE.md §15.1 SRP — 5020 의 책임은 "자산 + SO + 코드 정합". 디자인 문서 갱신은 별도 책임 도메인.
  - 5020 PRD §2.4 가 이미 "별도 후속 PR 권장" 으로 명시한 default 채택.
  - 별도 PR 분리해도 1~2일 내 처리 가능 (작은 docs-only 작업).
- **PRD Impact**: **변경 X** — Sprint PRD 본문 그대로. 후속 작업 메모만 유지.
  - Sprint PRD §10.1 "후속 PRD" 섹션에 "5020.1 character_progression_v1.md stats_v2 정합 갱신" 추가
- **Trade-off**: character_progression_v1.md 가 1~2일 stale 잔존. 사용자가 5020 종결 후 즉시 5020.1 진행하면 회피.

### 2.2 D-D — References/Selected_Characters/ 처리

- **Decision**: **옵션 B — `References/Selected_Characters_v1_legacy/` rename**
- **Mode**: ai-assisted (Claude 권장 [A 보존] 오버라이드 → B 채택)
- **Rationale (사용자 결정)**:
  - "옛 자료" 명시적 표기로 미래 작업자 혼란 방지
  - history 보존 (옵션 C 삭제 회피) + 명확 분리 (옵션 A 단순 보존보다 정합성 ↑)
  - rename 은 비-wired 자료라 게임 영향 0 (CharacterData portrait 참조 없음)
- **PRD Impact**:
  - Sprint 3a `git mv` 명령에 1줄 추가:
    ```bash
    git mv LittleHero/Assets/_Project/Art/References/Selected_Characters \
           LittleHero/Assets/_Project/Art/References/Selected_Characters_v1_legacy
    ```
  - Sprint 3a 의 commit 메시지 [변경] 섹션에 한 줄 추가
  - Sprint 3a 시간 20분 → 25분 (+5분)
  - 합산 commit 수 변경 X (7 commit 유지) — Sprint 3a 안에 통합
- **Trade-off**: rename commit 1줄 추가 (5분 작업), 그러나 명확성 ↑.

---

## 3. 통계

| 항목 | 값 |
|---|---|
| Total Items | 2 |
| AI Recommendations Followed | **1 / 2** (D-C 권장 채택, D-D 권장 [A] → 사용자 [B] 오버라이드) |
| User Overrides | 1 (D-D) |
| 모드 | ai-assisted |

---

## 4. v2_prd.md 변경 diff 요약 (rebuttal 단계 → mediator 최종)

| 영역 | 변경 |
|---|---|
| Sprint 3a `git mv` 목록 | + `References/Selected_Characters → Selected_Characters_v1_legacy` 1줄 (D-D=B) |
| Sprint 3a 시간 | 20분 → 25분 (+5분, D-D rename 작업) |
| Sprint 3a commit 메시지 [변경] | + "References/Selected_Characters → _v1_legacy rename (D-D=B)" 한 줄 |
| Sprint PRD §10.1 후속 PRD | + "5020.1 character_progression_v1.md stats_v2 정합 갱신" (D-C=A) |
| 합산 commit | 7 (변경 X) |
| 합산 시간 | 4~5h → **4h05m~5h05m** (+5분) |

---

## 5. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-mediator, ai-assisted) | 초기 — D-C=A 권장 채택, D-D=B 사용자 오버라이드 |
