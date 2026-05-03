# 5003 Mini Boss Scene Separation — Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.
> 입력: `5003_*_critique.md` (Discuss 3건) → rebuttal 자동 수용 1건 (D3) 제외 → 본 문서 2건.

## High Priority Items

### D1 — 디자인 옵션 채택 (핵심)

- **Original Criticism**: 디자인 §2 권장 = **옵션 E (분리 X)** — UX 단조로움 회피. 그러나 본 PRD 는 **옵션 B (wave 15만 분리)** 가정으로 작성. 사용자가 `/prd-pipeline 5003` 호출 → 진행 의도 명확. 그러나 옵션 채택은 mediator 단계 결정 필요.

- **Current Position (PRD v1)**: 옵션 B (wave 15 만 분리). Sprint 1~5 작성됨.

- **Discussion Points**:
  - **옵션 E (분리 X)** ★디자인 권장: 5002 종료 상태 그대로 유지. 본 PRD 폐기 (또는 RESERVED 영구). UX 단조로움 회피. 미니보스 = "양념" 의도 보존.
    - 장점: 0 Sprint / 0 commit / UX 안정. 5002 의도 보존.
    - 단점: 본 PRD 작성 노력 폐기. 보스 빌드업 효과 X.
  - **옵션 B (wave 15만 분리)**: 현 PRD 그대로 진행. 5 Sprint × 5 commit × 1시간 40분.
    - 장점: 보스(20) 직전 빌드업 효과. 씬 전환 1회/챕터 (~1초).
    - 단점: 미니보스 5/10 은 분리 X (일관성 약함).
  - **옵션 A (모든 미니보스 분리)**: Sprint 3 변경만 (`wave==15` → `wave % 5 == 0`). 5 Sprint 그대로.
    - 장점: 일관 — 모든 보스 cinematic 통일. enum `All` 옵션 활용.
    - 단점: 4 transitions/챕터 (~4초). UX 단조로움 위험.
  - **옵션 D (BossEncounter 재사용 + 일반 적 동시 spawn)**: WaveManager 흐름 차이 ↓ — 미니보스 + 일반 적 함께 spawn.
    - 장점: 기존 흐름 보존.
    - 단점: 씬 분리 의미 약화 — "일반 적 가득한 보스 씬".

- **Proposed Resolution Path** (Claude 권장): **옵션 E** (디자인 §2 권장 답습) — UX 안정 + 5002 의도 보존.
  - 그러나 사용자가 진행 의도 명시 (`/prd-pipeline 5003`) → **옵션 B 그대로** 도 합리적. 옵션 B = 디자인 §2 후보 (★). 보스 빌드업 효과 가치 인정.

- **PRD Impact**:
  - 옵션 E 채택 시: PRD 폐기 + RESERVED 영구 보존. Phase 3~6 진행 X.
  - 옵션 B 채택 시: 현 PRD 그대로 진행 (D3 자동 수용 반영). 5 Sprint 진행.
  - 옵션 A 채택 시: Sprint 3 ChapterRunner 분기를 enum `All` 로 변경. 5 Sprint 그대로.
  - 옵션 D 채택 시: Sprint 2~3 재작성 (BossEncounter 씬에 일반 적 spawn 흐름 추가). +30분.

---

## Medium Priority Items

(없음)

---

## Low Priority Items

### D2 — §11 마일스톤 등재 여부

- **Original Criticism**: 작은 변경 (5 Sprint × 1시간 40분) — §11 마일스톤 등재 가치 검토.

- **Current Position (PRD v1)**: Sprint 5 에 §11 등재 명시.

- **Discussion Points**:
  - **A** ★권장: §11 등재 (5001/5002 패턴 답습) — 일관성 우선
  - **B**: §14 회전만 (작은 변경 — §11 미등재)
  - **C**: 옵션 E 채택 시 자동 폐기

- **Proposed Resolution Path**: D1 결정 후 자동:
  - 옵션 E → C (자동 폐기)
  - 옵션 B/A/D → A (등재)

- **PRD Impact**: Sprint 5 changelog/CLAUDE.md 작업 분량만 영향. 옵션 E 시 Sprint 5 자체 폐기.

## 요약

- **Total Items**: 2 (D1 핵심 / D2 사소)
- **High**: 1 / **Medium**: 0 / **Low**: 1
- **권장 mode**: ai-assisted (Claude 추천 + 사용자 확인)
- **Claude 권장 일괄 채택 시**: D1=E (디자인 권장 답습), D2=C (자동 폐기)
- **사용자 결정 입력 형식 예**: `D1=E` / `D1=B (그대로 진행)` / `권장대로 (E)` / `옵션 B 채택 진행` 등
