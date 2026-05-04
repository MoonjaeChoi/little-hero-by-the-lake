# 5009 Chapter Overrides v1 — PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: 2026-05-04

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| OOP §15 | 위반 0 | — | — | 신규 코드 없음 | ✅ |
| PG-12 톤 | 위반 0 | — | — | 5008 baseline 재사용 | ✅ |
| Sprint 분해 (D3) | 시드 offset 공식 검증 | Low | **Accepted** | (N-1)*6 공식 정정 (PRD 의 22252 = CH 5 정합) | ✅ |
| 성능 (§7.1) | SpriteAtlas 재빌드 명시 부족 | Low | **Accepted** | Sprint 6 docs 에 SpriteAtlas 재빌드 단계 명시 | ✅ |
| 시스템 통합 (D1) | chapter override V2 미사용 — ROI 의문 | Medium | **Discuss** | discussion_topics D1 | △ |
| Sprint 분해 (D2) | Sprint 5 (Fantasy 48) 분할 검토 | Low | **Discuss** | discussion_topics D2 | △ |

## 2. 품질 메트릭

- 총 Discussion Points: 5 (Critique Discuss 3 + Critique 추가 관찰 2)
- **Accepted** (수정 반영): 2건 (40%)
- **Rejected**: 0건 (0%)
- **Discuss** (사용자 결정): 2건 (40%)
- 평가 (정보, 자동 검증 포함): 2건 (Accept 5 의 일부)
- §15 정합률: 100% (위반 0건)

## 3. Detailed Responses

### 3.1 [Sprint 분해 D3 — Low Priority — Accepted]

**Original Criticism** (critique §4 D3):
> 디자인 §3.2 "예 CH 1 시드 22222 / CH 2 시드 22228 / ... CH 5 시드 22252". chapter*6 이라면 CH 5 = 22246 인데 22252 명시 — typo 의심.

- **Rebuttal**: **수용**. 단순 typo 정정 (사용자 결정 X). (N-1)*6 공식이 archetype 시드 22222 와 CH 1 일치 + PRD 의 CH 5 = 22252 (= 22222 + 5*6) 매칭.
- **Proposed Action**: 디자인 §3.2 + Sprint PRD 시드 공식 정정 — `시드 base + (chapter-1) * 6` (1-indexed offset).
  - CH 1 = 22222 / CH 2 = 22228 / CH 3 = 22234 / CH 4 = 22240 / CH 5 = 22246
  - 또는 PRD 의 22252 가 의도였다면 `시드 base + chapter * 6` (CH 1 = 22228, CH 5 = 22252) — 다만 archetype 시드 22222 와 CH 1 충돌
- **PRD Impact**: §3.2 시드 공식 명시 + Sprint 1~5 의 실제 시드 값 정정.
- **Little Hero Context**: 5008 의 시드 패턴 (Meta 22222 / Nature 33333) 과 일관 유지 위해 archetype = chapter 0 또는 chapter 1 일치 권장.

---

### 3.2 [성능 §7.1 — Low Priority — Accepted]

**Original Criticism** (critique §7.1):
> 210 PNG 갱신 시 EnemyAtlas 재빌드 필수 (5008 §6 Definition of Done 패턴). Sprint 6 docs 에 명시 필요.

- **Rebuttal**: **수용**. 5008 Rebuttal accepted 와 동일 패턴 — SpriteAtlas SRP Batcher 의 §7 cap (draw call <10/frame) 보존을 위해 재빌드 필수.
- **Proposed Action**: Sprint 6 commit 메시지 + Definition of Done 에 "EnemyAtlas SpriteAtlas 재빌드 + draw call <10/frame 검증" 단계 추가.
- **PRD Impact**: §2 Sprint 6 commit 메시지 추가 + §5 Definition of Done 1줄 추가.

---

### 3.3 [시스템 통합 D1 — Medium Priority — Discuss]

**Original Criticism** (critique §4 D1):
> chapter override 가 PhaseFContentSeed V2 시스템 미사용. ROI 의문. 5008 EnemySpriteAuditor V2 fallback = OK 확인됨.

- **Status**: **Discuss** → discussion_topics D1
- **이유**: 디자인 옵션 변경 (진행/후순위/보류) — 사용자 결정 사안. mediator 단계에서 결정.
- **Current Position**: PRD 그대로 진행 시 ~3.5~5h 투자, 즉시 활용처 X.
- **Resolution Path**: mediator 에서 옵션 A (그대로) / B (후순위) / C (보류) 결정. Claude 권장 B.

---

### 3.4 [Sprint 분해 D2 — Low Priority — Discuss]

**Original Criticism** (critique §4 D2):
> Sprint 5 (Fantasy 48 PNG) 분할 검토 — 사용자 review 부담.

- **Status**: **Discuss** → discussion_topics D2
- **이유**: Sprint 분할 = 사용자 review 빈도 trade-off — 사용자 결정.
- **Current Position**: 6 commit 유지, Sprint 5 가장 큰 (48 PNG).
- **Resolution Path**: mediator 에서 A (그대로) / B (Nature+Fantasy 분할) / C (Nature 만 분할) 결정. Claude 권장 A.

## 4. PRD v2 적용 변경 요약

(수용 항목만 v2.md 에 반영. 미결은 v2 미반영, mediator 후 최종.)

### Accepted (2건 반영)

- **§3.2 시드 공식 명시** — `시드 base + (chapter-1) * 6` 또는 `시드 base + chapter * 6` 통일 (PRD 의 22252 = CH 5 위치 정합)
- **§2 Sprint 6 + §5 Definition of Done** — SpriteAtlas 재빌드 단계 추가

### Discuss (2건 — mediator 결정 대기)

- D1 ROI (chapter override V2 미사용)
- D2 Sprint 5 (Fantasy 48 PNG) 분할

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal). Accepted 2 / Discuss 2 / Rejected 0.
