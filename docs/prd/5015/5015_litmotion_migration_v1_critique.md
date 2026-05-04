# 5015 — LitMotion Migration PRD 리뷰

## 0. 결정 이력
- 2026-05-04: prd-critiquer 자동 리뷰 (Phase 2.1)

## 1. 종합 점수: 5.5 / 10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | wrapper 패턴 §15.1 정합 |
| PG-12 톤 | 10/10 | 무관 |
| 성능 (PIVOT v4 §7) | 8/10 | LitMotion 0 GC 매력적 (다만 현 IEnumerator 도 적정) |
| **시스템 통합** | **2/10** | 🚨 **PRD 전제 무효 — DOTween 사용 0건** |
| Sprint 분해 | 7/10 | 6 sprint 정합 (전제 invalid 시 N/A) |

## 2. 🚨 Critical 발견 — PRD 전제 무효

### 2.1 DOTween 미사용

**증거**:
- `LittleHero/Packages/manifest.json` — DOTween 패키지 **미설치**
  ```
  com.coplaydev.unity-mcp / com.unity.2d.* / com.unity.cinemachine /
  com.unity.purchasing 5.0.0 / com.unity.render-pipelines.universal 17.4.0 / ...
  ```
  Demigiant.DOTween / com.dotween 등 **0건**
- Scripts/ 전체 `using DG.Tweening` grep — **0건**
- Scripts/ 전체 `DOFade / DOScale / DOMove / DOAnchor` grep — **0건**

### 2.2 CLAUDE.md §10 outdated

CLAUDE.md §10 외부 라이브러리:
```
| 애니메이션 | DOTween | 커스텀 |
```

→ **사실 X**. 실제 애니메이션 패턴 — IEnumerator + Lerp 직접:
- `EnemyController.PunchRoutine / FlashRoutine` — Mathf.Lerp + WaitForSecondsRealtime
- `FadeOverlay.FadeRoutine` — Color.Lerp
- `WaveTransitionToast` / `MythicEffectOverlay` / `HUDController` / `SimpleFx` / `DamageNumberFloater` — IEnumerator + Coroutine

### 2.3 PRD 영향

PRD §1.1 "DOTween 사용 현황" + §1.3 "마이그레이션 영향 (estimated ~50~100 호출)" — **모두 invalid**.

실제 시나리오:
- DOTween 마이그레이션 작업 X (할 게 없음)
- LitMotion 도입은 **신규 도입 + IEnumerator+Lerp 영역을 LitMotion 으로 전환** 디자인 변경
- 또는 LitMotion 미도입 + 현 IEnumerator+Lerp 패턴 유지

## 3. Accept 항목 (1)

- ✅ LitMotion MIT 라이선스 매력 (DOTween 보다 깔끔, 다만 비교 대상 없음)

## 4. Reject 항목 (0)

- (PRD 자체 전제가 invalid 라 일반 reject X)

## 5. Discuss 항목 (3)

### 🟡 D1. PRD 전제 무효 — 5015 방향 재정의 (Critical)

**Original PRD**: DOTween 마이그레이션 (50~100 call site).

**Reality**: DOTween 0건. 실제 패턴은 IEnumerator + Lerp.

**옵션**:
- **A (★권장)**: **5015 RESERVED** (DOTween 마이그레이션 → 무의미). LitMotion 도입은 별도 PRD (예 5015b — IEnumerator → LitMotion 전환).
- B: 5015 재정의 → "IEnumerator + Lerp → LitMotion 전환". scope 동일하지만 의미 변경.
- C: 5015 보류 + CLAUDE.md §10 "DOTween" 항목만 제거 (1 commit fix).
- D: LitMotion 도입 + 점진 마이그레이션 (신규 코드만 LitMotion).

### 🟡 D2. 현 IEnumerator+Lerp 패턴 GC 측정 필요

**Reality**: PIVOT v4 §7 GC <50KB/s 마진 측정 X.

**옵션**:
- A: Profiler 측정 후 결정 (LitMotion 도입 ROI 정량 평가)
- B: 마진 충분 가정 → IEnumerator 유지 (5015 보류)

### 🟡 D3. CLAUDE.md §10 외부 라이브러리 정합

**현 §10**:
```
| 애니메이션 | DOTween | 커스텀 |
```

**수정 옵션**:
- A: "DOTween" 행 제거 + 신규 "(없음 — IEnumerator+Lerp 직접)" 명시
- B: "DOTween" 행 제거 + LitMotion 도입 (5015 진행) 후 "LitMotion | MIT" 추가
- C: 현 §10 그대로 (변경 X — 부정확 정보 잔존)

## 6. CLAUDE.md §15 정책 위반 점검

5015 자체는 §15 위반 X. 다만 CLAUDE.md §10 outdated 정보가 향후 신규 작업자 혼란 유발.

## 7. 권장 변경

**즉시 변경 (Accept)**:
- §10 outdated DOTween 항목 제거 (D3 C 거부, A 채택)

**사용자 결정 후 변경 (Discuss)**:
- D1: 5015 RESERVED 또는 재정의
- D2: GC 측정 (LitMotion ROI 평가)
- D3: §10 갱신 후 LitMotion 추가 여부

## 8. 변경 이력
- 2026-05-04: 초기 작성 (Phase 2.1 Critique).
