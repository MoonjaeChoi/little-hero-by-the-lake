# 5020 Starter Hero Visual — Discussion Topics (Phase 2.3 Mediator 결정 대기)

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.
> **R1 / I1 / D-A / D-B 는 rebuttal 에서 Accepted 처리** — 본 문서 미포함.
> **남은 Discuss**: 2건 (D-C, D-D).

---

## High Priority Items

(없음)

---

## Medium Priority Items

### D-C. character_progression_v1.md 갱신 범위

- **Original Criticism** (critique §4 D-C):
  - `docs/little_hero/character_progression_v1.md` 가 옛 4 코어 (힘/빠름/슬기/마음) + 옛 키즈명 ("용감한 소년", "씩씩한 소녀", "조용한 친구") 사용
  - PIVOT v2 stats_v2 (3 코어 — Spirit 폐기) 이후 미갱신 — 디자인 문서 ↔ 자산 미스매치 잔존

- **Current Position**: 5020 PRD §2.4 에 "별도 후속 PR 권장" 으로 명시했지만 결정 X — 문서 stale 상태.

- **Discussion Points**:
  - **옵션 A (★권장)**: **별도 PR 분리** — 5020 은 자산·SO·코드만, 디자인 문서 갱신은 5020.1 후속 PR
    - 장점: 5020 단일 책임 commit 유지 (§15 정합), review 부담 ↓
    - 단점: 디자인 문서 stale 잔존 (1~2일 정도)
  - **옵션 B**: 5020 Sprint 6 추가 — character_progression_v1.md 갱신 commit (8 commit 됨)
    - 장점: 5020 안에 모든 정합성 완결, stale 0
    - 단점: PRD 범위 확대 (자산 → 디자인 문서), Sprint 7개 → 8개
  - **옵션 C**: 무시 — 5020 종결 후 다른 작업 시 별도 처리 (또는 영구 미갱신)
    - 장점: 5020 가장 빠름
    - 단점: 디자인 문서 stale 영구 위험, 향후 작업자 혼란

- **Proposed Resolution Path**:
  - **옵션 A 권장** — 5020 단일 책임 (자산 정합) 유지, character_progression_v1.md 갱신은 별도 작은 PR (5020.1)
  - 사용자가 "5020 안에 다 끝내고 싶다" 라면 옵션 B 채택 가능
  - 옵션 C 는 비권장 (stale 영구 위험)

- **PRD Impact**:
  - 옵션 A: 변경 X (Sprint PRD §2.4 그대로, 후속 PR 메모 유지)
  - 옵션 B: Sprint PRD §1 합산 7 → 8 commit, Sprint 6 신규 (character_progression_v1.md edit + commit)
  - 옵션 C: Sprint PRD §2.4 메모 삭제

---

### D-D. References/Selected_Characters/ 옛 자료 처리

- **Original Criticism** (critique §4 D-D):
  - 옛 키즈 reference PNG 3장 보존됨 (`Char_BraveGirl_idle.png` 등)
  - 새 캐릭터 (시우/가영/연우) 와 톤 불일치 — 미래 혼란 가능

- **Current Position**: PRD 미명시 (보존 default).

- **Discussion Points**:
  - **옵션 A (★권장)**: **보존** — history 자료, 5020 범위 외, 향후 작업 시 참고 가능
    - 장점: 변경 0, history 손실 X
    - 단점: 폴더 안에 옛 톤 자료 잔존
  - **옵션 B**: `References/Selected_Characters_v1_legacy/` 로 rename — 명확 분리
    - 장점: "옛 자료" 명시적 표기
    - 단점: rename commit 1개 추가, GUID 보존 필요 (비-wired 자료라 영향 미미)
  - **옵션 C**: 삭제
    - 장점: 폴더 정리
    - 단점: history 손실 (재생성 어려움)

- **Proposed Resolution Path**:
  - **옵션 A 권장** — 5020 범위 외, 변경 0
  - 사용자가 "프로젝트 정리" 차원 결정 시 옵션 B 가능
  - 옵션 C 는 비권장 (history 손실)

- **PRD Impact**:
  - 옵션 A: 변경 X
  - 옵션 B: Sprint 3a 에 rename commit 1줄 추가 (commit 합산 7 → 8 — D-C 옵션 B 와 합치면 9)
  - 옵션 C: Sprint 3a 에 `git rm` 1줄 추가

---

## Low Priority Items

(없음)

---

## 요약

| 항목 | 값 |
|---|---|
| Total Items | 2 |
| High | 0 |
| Medium | 2 (D-C, D-D) |
| Low | 0 |
| 권장 mode | **ai-assisted** (Claude 추천 + 사용자 확인) |
| Claude 권장 | **D-C 옵션 A (별도 PR) + D-D 옵션 A (보존)** — 5020 단일 책임 유지 |

---

## 사용자 결정 입력 형식

```
"D-C=A, D-D=A"           # 컴팩트 (★ Claude 권장 그대로)
"권장 그대로 진행"        # 일괄 채택
"D-C 옵션 B 5020 안에 포함, D-D 옵션 A 보존"  # 자연어
"D-D rename 으로"         # 부분 결정
```

---

## 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-rebuttal) | 초기 — D-C / D-D 2건, R1/I1/D-A/D-B 는 Accepted 처리로 별도 |
