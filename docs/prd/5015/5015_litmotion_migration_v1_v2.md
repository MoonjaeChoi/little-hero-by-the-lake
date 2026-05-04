# 5015 — LitMotion Migration v2 (최종, RESERVED)

> **⛔ RESERVED — D1=A 결정 (Mediator 확정 2026-05-04)**
> PRD 전제 무효 (DOTween 사용 0건) → RESERVED + CLAUDE.md §10 docs fix only.
> 후속: 별도 5015b 신규 디자인 (Profiler 측정 후 LitMotion ROI 평가).

> **PRD v2 (최종, Mediator 확정)**
> 원본: `5015_litmotion_migration_v1_prd.md` · Rebuttal: `5015_litmotion_migration_v1_rebuttal.md` · Decisions: `5015_litmotion_migration_v1_decisions.md`
> 결정 2건 반영 (D1=A RESERVED / D2=A 측정 후 결정).

## 🚨 RESERVED 사유

PRD 전제 무효 — Critical 발견:
- `Packages/manifest.json` — DOTween 패키지 **미설치**
- Scripts/ `using DG.Tweening` grep — **0건**
- 실제 패턴: IEnumerator + Mathf.Lerp / Color.Lerp 직접 (15+ 파일)
- CLAUDE.md §10 의 "DOTween 커스텀" 는 **outdated 정보**

## 1. 즉시 작업 (1 commit)

### CLAUDE.md §10 docs fix

DOTween 행 제거 + IEnumerator+Lerp 직접 사용 명시.

## 2. RESERVED 후속 — 5015b 신규 PRD

후속 5015b 생성 시 사전 작업:
1. Profiler 측정 (cap 300 환경 + wave 5 spawn 시 GC alloc/sec)
2. 결과 < 50KB → 5015b 보류 (IEnumerator 유지)
3. 결과 > 100KB → 5015b 진행 (LitMotion 도입 디자인 + Sprint 분해)

5015b 진입 시: `/prd-pipeline 5015b --from generate`

## 3. Phase 3~6 보류

- Phase 3 Breakdown — 미실행
- Phase 4 Contract — 미실행
- Phase 5 Implement — 1 commit (§10 docs fix) 만
- Phase 6 Evaluate — N/A

## 4. 변경 이력

- 2026-05-04: v1 작성 (Phase 1)
- 2026-05-04: v2 초안 (Phase 2.2 Rebuttal) — Critical 발견
- 2026-05-04: **v2 최종 (Phase 2.3 Mediator, D1=A RESERVED + D2=A 측정 후 결정)**
