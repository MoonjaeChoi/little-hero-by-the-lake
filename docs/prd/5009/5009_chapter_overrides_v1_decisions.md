# 5009 Chapter Overrides v1 — PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 2건 결정 기록
> 모드: ai-assisted (Claude 권장 + 사용자 "권장안대로" 일괄 채택) · 일시: 2026-05-04

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| Medium | D1 | Chapter override V2 미사용 ROI | **B** — 후순위 (5012/5013/5015 종결 후) | **RESERVED** |
| Low | D2 | Sprint 5 Fantasy 48 PNG 분할 | **A** — 그대로 (1 sprint, 5 batch review) | Done (변경 X) |

## 2. Detailed Decisions

### 2.1 D1 — Chapter Override V2 미사용 ROI

- **Decision**: **옵션 B — 후순위 (5012/5013/5015 종결 후 진행)**
- **Mode**: ai-assisted (Claude 권장 + 사용자 "권장안대로" 채택)
- **Rationale**:
  - 5008 EnemySpriteAuditor V2 fallback 검증 = OK 통과 — chapter override 즉시 활용처 X
  - 5012 (MPB hue) / 5013 (culling) / 5015 (LitMotion) 코드 작업이 즉각 게임플레이 영향 ↑
    - 5013 EnemyVisibilityCuller — 60fps 마진 (PIVOT v4 §7 cap 300 환경)
    - 5015 LitMotion — GC <50KB/s 마진 + DOTween 라이선스 cleanup (MIT 전환)
    - 5012 MPB hue — 30 → 150 시각 다양성 (자산 추가 0)
  - 5009 = ~3.5~5h 자산 작업 vs 5012/5013/5015 코드 작업 = 즉각 60fps + GC 0 + 라이선스 효과
- **PRD Impact**:
  - **5009 Phase 3~6 보류 (RESERVED)**
  - PRD 자체 (디자인 + Sprint PRD + Rebuttal v2) 보존 — 향후 재진입 시 그대로 활용 가능
  - 재진입 시: `/prd-pipeline 5009 --from breakdown` (Phase 3 부터)
  - 5009 anchor 5009.x 후속 사이클로 추적 가능
- **Trade-off**:
  - 옵션 A (그대로 진행) 의 즉시 chapter override 인프라 정비 손실 — 미래 활용 지연
  - 옵션 C (보류) 와 다름 — 5009 는 "후순위 진행" (재진입 가능), C 는 "회귀 시에만 즉시 처리"

### 2.2 D2 — Sprint 5 Fantasy 48 PNG 분할

- **Decision**: **옵션 A — 그대로 (1 sprint, 5 batch review)**
- **Mode**: ai-assisted (Claude 권장 일괄 채택)
- **Rationale**:
  - 5008 검증된 batch 단위 (Civ 6 + Fantasy 6 batch) 가 안정 — 사용자 "good" 보고
  - 48 PNG 한 번 review 도 카테고리 톤 일관 검증 효율 ↑
  - 분할 시 commit 수 ↑ + review 사이클 추가 부담
- **PRD Impact**:
  - 변경 X — 6 sprint, 6 commit 그대로 유지
- **Trade-off**:
  - 옵션 B (Nature + Fantasy 분할) 의 review 단위 균등 손실 — 다만 5008 검증된 패턴 우선

## 3. 통계

- **Total Items**: 2
- **AI Recommendations Followed**: 2/2 (100%)
- **User Overrides**: 0
- **모드**: ai-assisted (사용자 "권장안대로" 일괄 채택)

## 4. v2_prd.md 변경 diff 요약

(rebuttal 단계 v2 초안 → mediator 최종 의 변경사항)

### D1=B 채택 결과

- **5009 = RESERVED** — Phase 3~6 미실행, PRD 보존
- v2.md 상단에 "RESERVED — D1=B 후순위" 명시
- 후속 진행 시 anchor: `/prd-pipeline 5009 --from breakdown`

### D2=A 채택 결과

- 변경 X (Sprint 표 그대로)

## 5. 5009 → 5012/5013/5015 우선순위 재정렬

본 mediator 결정으로 5009 후순위 → 다음 즉시 진행 PRD 권장:

| PRD | 효과 | 우선순위 |
|---|---|---|
| **5013 EnemyVisibilityCuller** | 60fps 마진 (PIVOT v4 §7 cap 300) | **★ 최우선** |
| 5015 DOTween → LitMotion | GC <50KB/s + MIT 라이선스 | 2 |
| 5012 MPB hue shift | 30 → 150 시각 다양성 | 3 |
| 5009 chapter overrides | 자산 인프라 (RESERVED) | 4 (5012/5013/5015 종결 후) |
| 5010 V1 11종 | legacy 도감 anchor | 5 |
| 5011 Daily/Weekly 보스 | endgame 시각 | 6 |
| 5014 V5 sticker LoRA | 자동화 | 7 (외부 학습) |

## 6. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.3 Mediator). ai-assisted 모드, 사용자 "권장안대로" 일괄 채택, AI 권장 채택률 2/2. **5009 RESERVED 상태** — Phase 3~6 보류.
