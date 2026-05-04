# 5013 EnemyVisibilityCuller v1 — PRD 리뷰

## 0. 결정 이력
- 2026-05-04: Claude Code (자동) 리뷰 — Phase 2.1 Critique
- 입력: `5013_enemy_visibility_culler_v1_prd.md` (디자인) + `5013/5013_enemy_visibility_culler_v1_prd.md` (PRD)
- 컨텍스트: CLAUDE.md §7 PIVOT v4 cap 300 / §15 OOP 정책 / Damage v2 OverlapCircle 패턴 / EnemyAnimationManager 정합

## 1. 종합 점수: 9.4 / 10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 10/10 | 정적 헬퍼 + 80L + GC 0 ✓ |
| PG-12 톤 | 10/10 | 영향 없음 (성능 영역) |
| 성능 | 10/10 | 60fps 마진 강화 직접 |
| 시스템 통합 | 9/10 | Damage v2 OverlapCircle 영향 0 명시 ✓, EnemyAnimationManager 정합 ✓ |
| Sprint 분해 | 8/10 | 5 sprint 균형 / Validator 룰 명시 부족 (-1) / Profiler 측정 기준 모호 (-1) |

## 2. Accept 항목 (7)

- ✅ **옵션 B (Sprite + Animator + Manager 통합 cull)** — 적당히 강력 + EnemyAnimationManager 패턴 활용. 옵션 A (Sprite only) 의 Update 잔존 + 옵션 C (SetActive false) 의 trigger 영향 회피
- ✅ **정적 헬퍼 + 정적 cache** — PlayerFinder 패턴 답습 (5005 검증), §15.7 DI 정합
- ✅ **0.1s throttle (Time.unscaledDeltaTime)** — pause 시 영향 X, 매 프레임 비용 회피
- ✅ **Margin 1.5x** — 사전 활성화 (화면 진입 적의 sprite 깜빡임 방지)
- ✅ **Damage v2 OverlapCircle 영향 0 명시** — 디자인 §6 위험 분석에 명시 (sprite 비활성 X 충돌 layer 만 사용)
- ✅ **EnemyAnimationManager Register/Unregister 정합** — 단일 Manager Update 의 List 에서 cull 제거 → CPU 절감
- ✅ **NUnit 5+ 명시** — `IsVisible_Origin_True` / `IsVisible_FarOffscreen_False` / `Margin_True` / `InvalidateCache` / `NoCamera_DefaultsTrue` (안전 fallback)

## 3. Reject 항목 (0)

(없음 — §15 정합 + PG-12 영향 X)

## 4. Discuss 항목 (3)

### 🟡 D1. Throttle 빈도 (0.1s)

**현재 PRD**: Sprint 2 의 throttle = 0.1s.

**문제**: 0.1s 가 적절한가? 너무 짧으면 비용 ↑ / 너무 길면 화면 진입 적 sprite 늦게 표시.

**옵션**:
- **A. 0.1s (PRD)** — 균형 (10 회/초 visibility 체크)
- **B. 매 프레임** — 정확성 ↑, 300 적 환경 0.5ms ↑
- **C. 0.05s** — 더 정확 (20 회/초)
- **D. 0.2s + margin 2.0x 결합** — 비용 ↓ + sprite 깜빡임 회피 (margin 으로 보완)

**Claude 권장**: **A (0.1s)** — 60fps 환경 = 6 frame 간격, 사용자 인지 어려움. 비용 / 정확성 균형.

---

### 🟡 D2. Margin 크기 (1.5x viewport)

**현재 PRD**: Sprint 1 의 margin = 1.5x.

**문제**: 화면 진입 적 sprite 깜빡임 회피 + 비용 균형.

**옵션**:
- **A. 1.5x (PRD)** — 균형 (viewport 외부 50% 영역도 활성)
- **B. 1.2x (보수)** — 화면 가까운 적만 활성, cull 효과 ↑ (~40% 더 cull)
- **C. 2.0x (안전)** — 화면 외 100% 영역 활성, 깜빡임 X but cull 효과 ↓

**Claude 권장**: **A (1.5x)** — PIVOT v4 §7 cap 300 환경에서 화면 가시 ~50 적 + margin 50 = 100, off-screen 200 cull. 효과 ↑.

---

### 🟡 D3. Validator 룰 추가 (PRD 명시 부족)

**현재 PRD**: Sprint 4 ProjectValidator 룰 1줄 명시 — "EnemyController 의 Update 첫 줄이 visibility 체크" — 그러나 구체적 검증 룰 미명시.

**문제**: §15.12 ProjectValidator 룰 추가 시 자동 검증 가능.

**옵션**:
- **A. ProjectValidator 룰 X (PRD 그대로)** — Sprint 4 NUnit 만 검증
- **B. Validator 룰 추가** — `EnemyController_Update_Visibility_Check` (정적 분석으로 Update 첫 줄에 IsVisible 호출 검사)
- **C. Validator + NUnit 둘 다 강화** — 더 강력 정합

**Claude 권장**: **B (Validator 룰 추가)** — §15.12 정책 / 5004 SceneTransitionService Validator 패턴 답습. 향후 EnemyController 변경 시 visibility 누락 자동 경고.

## 5. CLAUDE.md §15 정책 위반 점검

| 정책 | 결과 | 비고 |
|---|---|---|
| §15.1 SRP | ✅ | EnemyVisibilityCuller 단일 책임 (visibility 판정) |
| §15.4 SO 우선 | ✅ | margin/throttle 은 const (실제 변동 X 추정) |
| §15.6 100L | ✅ | ~80L 추정 |
| §15.7 DI | ✅ | 정적 헬퍼 + InvalidateCache (씬 전환 hook) |
| §15.9 Test | ✅ | NUnit 5+ 명시 |
| §15.11 GC | ✅ | Vector3 struct, Camera cache, 0 GC |
| §15.12 Validator | △ | D3 결정에 따라 ✅ 또는 △ |

## 6. 권장 변경

### 즉시 변경 (Reject 없음)

(없음)

### 사용자 결정 후 변경 (3 Discuss)

- **D1**: Throttle (A 0.1s / B 매 프레임 / C 0.05s / D 0.2s+margin)
- **D2**: Margin (A 1.5x / B 1.2x / C 2.0x)
- **D3**: Validator 룰 (A 없음 / B 추가 / C 강화)

## 7. 추가 관찰 (정보)

### 7.1 Profiler 측정 기준 모호

Sprint 5 의 Profiler 측정 — "GPU draw call -50%, CPU Update -30%" 추정. 실측 기준 모호.

권장: Profiler 측정 절차 구체화 (Editor 또는 iPhone 12 Build, cap 300 시나리오, 측정 시점, 측정 도구 = Unity Profiler / Xcode Instruments).

### 7.2 Camera 변경 시나리오

InvalidateCache 호출 hook — 5004 SceneTransitionService 의 OnAfterSceneLoad 이벤트와 연계 권장.

### 7.3 EnemyAnimationManager 의 Register/Unregister 빈도

매 프레임 visibility 변동 시 Register/Unregister 호출 — GC 가능성. List 의 add/remove 비용 검토 필요.

권장: HashSet 사용 + Manager 내부 cache (Sprint 3 명시).

## 8. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.1 Critique). Accept 7 / Reject 0 / Discuss 3.
