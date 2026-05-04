# 5008 Chapter Enemy Asset Quality v1 — PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: 2026-05-04

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| OOP §15 | (위반 없음) | — | — | — | ✅ |
| PG-12 톤 | (위반 없음) | — | — | — | ✅ |
| 성능 | SpriteAtlas 재구성 영향 명시 부족 | Low | **Accepted** | Sprint 6 docs 에 SpriteAtlas 재빌드 단계 명시 | ✅ |
| Auditor 구현 | GetPixels32 vs PNG byte 파싱 vs A+복원 모호 | Medium | **Discuss** | discussion_topics D1 | △ |
| 손상 8 chapter override | 5008.1 anchor vs 즉시 흡수 vs 검증 후 결정 | Medium | **Discuss** | discussion_topics D2 | △ |
| skip sprite 후속 정책 | A 무시 / B 5008.1 enroll / C 강제 0 | Medium | **Discuss** | discussion_topics D3 | △ |
| Sprint 1 fallback 명시도 | DreamShaperXL Turbo native alpha 한계 명시 | Low | **Accepted** | Sprint 1 commit message + SKILL.md 가이드에 한계 명시 | ✅ |

## 2. 품질 메트릭

- 총 Discussion Points: 5 (Critique Discuss 3 + 추가 관찰 2)
- **Accepted** (수정 반영): 2건 (40%)
- **Rejected**: 0건 (0%)
- **Discuss** (사용자 결정): 3건 (60%)
- §15 정합률: 100% (위반 0건)

## 3. Detailed Responses

### 3.1 [성능 — Low Priority — Accepted]

**Original Criticism** (critique §7.1):
> 30 PNG 재생성 시 SpriteAtlas 자동 재빌드 필요. Sprint 6 docs 에 SpriteAtlas 재빌드 단계 명시 권장.

- **Rebuttal**: **수용**. EnemyAtlas SpriteAtlas SRP Batcher 영향 (CLAUDE.md §7) 은 빌드 단계의 중요한 검증 항목.
- **Proposed Action**: Sprint 6 docs commit 메시지 + Definition of Done 에 "SpriteAtlas 재빌드 / Atlas 무결성 검증" 단계 추가.
- **PRD Impact**: §6 Sprint 6 commit + §5 Definition of Done 에 1줄 추가.
- **Little Hero Context**: SpriteAtlas SRP Batcher 가 적 sprite draw call <10/frame 보장 (§7) — 재빌드 누락 시 임포트 후 첫 빌드에서 atlas 누락 가능.

---

### 3.2 [Sprint 1 fallback — Low Priority — Accepted]

**Original Criticism** (critique §7.2):
> fallback 워크플로의 alpha 처리 한계 (DreamShaperXL Turbo native alpha 없음, dark BG flood-fill 후처리) 명시되면 사용자 기대치 정렬.

- **Rebuttal**: **수용**. 5006.1 / 5008 직전 사용자 보고 ("1라운드 적 흰 박스") 의 직접 원인이 Turbo native alpha 부재 였음. 사용자 기대치 정렬을 위해 fallback 한계 명시 정당.
- **Proposed Action**: comfyui-image SKILL.md LayerDiffuse 가이드 섹션에 "fallback 워크플로 alpha 한계: dark BG + flood-fill 후처리, 정확도 ~80%. LayerDiffuse 권장" 1단락 추가.
- **PRD Impact**: §3.2 (수정 파일) 의 SKILL.md 변경 명세에 fallback 한계 항목 추가.
- **Little Hero Context**: 사용자가 LayerDiffuse 노드 미설치 시 기존 워크플로 사용 가능하지만 후처리 필요함을 사전 인지.

---

### 3.3 [Auditor 구현 방식 — Medium Priority — Discuss]

**Original Criticism** (critique §4 D1):
> Sprint 2.1 "Pillow 알고리즘 C# 구현 또는 Texture2D.GetPixels32 사용" — 두 옵션 모호. 구현 차이 큼.

- **Status**: **Discuss** → discussion_topics D1
- **이유**: 구현 방식 선택은 (a) 메모리 (b) 임포트 설정 (c) 단순도 trade-off — 사용자 결정 사안.
- **Current Position**: PRD 그대로 진행 시 Sprint 2 commit 시점 결정 모호 → 구현자 임의 선택 위험.
- **Resolution Path**: mediator 단계에서 옵션 A/B/C 선택. Claude 권장: B (PNG byte 파싱) 메모리 효율 / 또는 A+복원 패턴 단순.

---

### 3.4 [손상 8 chapter override 처리 — Medium Priority — Discuss]

**Original Criticism** (critique §4 D2):
> 5008 PRD PASS 후에도 손상 8 chapter override 가 남아있으면 사용자 회귀 리스크. V2 시스템 archetype 폴백 검증 필요.

- **Status**: **Discuss** → discussion_topics D2
- **이유**: V2 시스템 archetype 폴백 작동 여부에 따라 Sprint 5.5 추가 여부 결정 — 사용자 우선순위 결정.
- **Current Position**: PRD 그대로 진행 시 5008.1 anchor 만, 사용자가 "1라운드 적 흰 박스" 회귀 시 5008.1 즉시 진입 필요.
- **Resolution Path**: mediator 단계에서 A (anchor 만) / B (Sprint 5.5 흡수) / C (검증 후 결정) 선택. Claude 권장: C (Auditor 검증 후 결정).

---

### 3.5 [skip sprite 후속 정책 — Medium Priority — Discuss]

**Original Criticism** (critique §4 D3):
> max 3 iteration 내 PASS 못 한 sprite 의 후속 처리 정책 미명시.

- **Status**: **Discuss** → discussion_topics D3
- **이유**: skip 정책은 사용자 부담과 추적성의 trade-off — 사용자 결정.
- **Current Position**: PRD 그대로 진행 시 skip 처리 sprite 가 5008 종료 후 추적 안 됨.
- **Resolution Path**: mediator 단계에서 A (skip 무시) / B (5008.1 우선 enroll) / C (skip 0 강제) 선택. Claude 권장: A+B 결합 (Sprint 6 docs skip list 명시 + 5008.1 anchor 우선 표시).

---

## 4. PRD v2 적용 변경 요약

(수용 항목만 v2.md 에 반영. 미결은 v2 미반영, mediator 후 최종.)

### Accepted (2건 반영)

- **§3.2 (수정 파일)** — comfyui-image SKILL.md LayerDiffuse 가이드 섹션에 fallback 한계 명시 추가 (Low accepted)
- **§5 Definition of Done** — "SpriteAtlas 재빌드 / Atlas 무결성 검증" 단계 추가 (Low accepted)
- **Sprint 6 commit 메시지** — SpriteAtlas 재빌드 단계 추가

### Discuss (3건 — mediator 결정 대기)

- D1 Auditor 구현 방식 (Sprint 2 영향)
- D2 손상 8 chapter override 처리 (Sprint 5.5 추가 여부)
- D3 skip sprite 후속 정책 (Sprint 6 docs + 5008.1 anchor)

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal). Accepted 2 / Discuss 3 / Rejected 0.
