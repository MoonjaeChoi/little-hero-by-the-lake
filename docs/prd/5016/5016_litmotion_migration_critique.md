# 5016 LitMotion 마이그레이션 PRD 리뷰

## 0. 결정 이력
- 2026-05-05: Claude Code (자동) Phase 2.1 Critique 진행

## 1. 종합 점수: 9.2/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9.5/10 | LMotionExtensions wrapper + ProjectValidator 룰 + MotionHandle 보유 패턴 — §15.1/15.6/15.7/15.10/15.11/15.12 모두 정합 |
| PG-12 톤 | 10/10 | 시각 회귀 X 명시 (Ease.Linear 일치, _baseColor 복귀, 5012 D3=B 정합) |
| 성능 | 9/10 | GC 0 효과 + Burst 가속 + Sprint 6 GCAllocMeasurer 재측정 명시. App 용량 임계 측정 시점 ↓ |
| 시스템 통합 | 9/10 | 5012 hue + 5013 visibility culler + WaveTransitionToast 사용자 baseline 정합 |
| Sprint 분해 | 8.5/10 | 6 Sprint 4시간 — Sprint 4 (50분) 작업량 큼 |

종합 평가: **PRD 품질 매우 양호**. 사전조사 (5015b) 기반 명확 + 시나리오 C 측정 결과 정당화 + LitMotion 외부 라이브러리 cleanup. Discuss 4건 사용자 결정 대기.

## 2. Accept 항목 (12)

- ✅ **LitMotion v2.0.1** + Burst 1.6 + Collections 1.5.1 + Mathematics 1.0 패키지 도입 (Packages/manifest.json git URL)
- ✅ **LMotionExtensions** wrapper ~70L (DOTween-like API — DOFadeAlpha/DOFade/DOPunchScale/DOMove/DOFillAmount, §15.6 100L 통과)
- ✅ **ProjectValidator LitMotion_Migration 룰** (DOTween 회귀 검출 + LitMotion import 검증, §15.12 정합)
- ✅ **6 Sprint 분해** — 인프라/EnemyController/CinematicEffects/Floater+Boss/UI/Validator+Test+docs 영역 분리 (commit 1:1 맵)
- ✅ **5012 D3=B 정합** — Sprint 2 FlashRoutine → DoFlash 마이그레이션 시 _baseColor 복귀 명시
- ✅ **사용자 baseline 가독성 정합** — Sprint 5 WaveTransitionToast `Time.unscaledDeltaTime` + LMotion `WithIgnoreTimeScale(true)` + progress dots Bind
- ✅ **LMotion.Shake 사용** — Sprint 3 ShakeRoutine → PlayScreenShake (`new Vector3` alloc 회피, cap 300 핫스팟 ↓)
- ✅ **WithIgnoreTimeScale(true)** 패턴 — HitPause / FadeOverlay / WaveTransitionToast / EnemyController 모두 적용 (Time.timeScale 영향 없는 UI/cinematic 영역)
- ✅ **MotionHandle 보유 + IsActive() + Cancel() 패턴** — 동시 실행 중복 방지 (§15.11 안티 패턴 회피)
- ✅ **NUnit 6 명시** — LMotionExtensionsTests (Type/Method 존재 + Handle 활성 검증), Tests/Editor 누적 100
- ✅ **GCAllocMeasurer 재측정** — Sprint 6 시나리오 A (<30 KB/s) 도달 검증 (PIVOT v4 §7 임계 회복)
- ✅ **CLAUDE.md §10 LitMotion MIT 행 추가** + §11 마일스톤 + §12 시스템 현황 + §14 직전 5건 회전 (5016 종결)

## 3. Reject 항목 (0)

(없음 — §15 위반 / PG-12 톤 위반 / 성능 회귀 0건)

## 4. Discuss 항목 (4)

### D1. Sprint 4 작업량 (50분 — Floater + Boss attacks 5종)

- **현재 PRD**: Sprint 4 통합 (1 commit)
- **이유 검토**: DamageNumberFloater (핫 패스) + Boss attacks 5종 (보스 공격 시만, 빈도 ↓) 영역 차이 — 검증 시나리오 분리 가능
- **옵션**:
  - **A (★권장)**: 통합 유지 (1 commit). 영역 작은 (Boss attacks 5종 각 ~5L 변경, Floater ~30L) → 1 commit 의미 단위 적절
  - **B**: Sprint 4a Floater + Sprint 4b Boss attacks 5종 (분리, 2 commit, 7 commit 총)
  - **C**: Boss attacks 5종 → T04~T08 (5 commit 분리, 10 commit 총) — 회수 용이성 ↑, 작업량 큼

### D2. Burst 의존성 App 용량 측정 시점

- **현재 PRD**: Sprint 6 빌드 후 측정 (마지막 검증)
- **이유 검토**: Burst 의존성 ~10MB+ — 임계 150MB 위반 시 Sprint 1 직후 발견하면 5015c (DOTween/UniTask) 재정의 가능. 마지막 발견 시 6 Sprint 작업 손실
- **옵션**:
  - **A (★권장)**: Sprint 1 직후 + Sprint 6 양시점 측정 (이른 검증 + 최종 검증). Sprint 1 commit 후 5분 추가
  - **B**: Sprint 6 만 (현재 PRD)
  - **C**: Sprint 1 직후 + Sprint 6 + Burst 미사용 옵션 (LitMotion BasicMode) 사전 정의

### D3. Coroutine ↔ LitMotion 호환 (yield return ToYieldInteraction())

- **현재 PRD**: Sprint 5 일부만 (FadeOverlay) 호환 wrapper 활용 명시
- **이유 검토**: WaveManager / ChapterRunner / BossEncounterRunner 등 큰 Coroutine 잔존 가능 — 점진 마이그레이션 vs 폐기?
- **옵션**:
  - **A**: Sprint 5 일부 만 (현재 PRD) — Coroutine 잔존 허용 (StartCoroutine 호출 일부 유지)
  - **B (★권장)**: 핫 패스 (Sprint 2~5) 모두 MotionHandle 폐기 + WaveManager/ChapterRunner/BossEncounterRunner Coroutine 잔존 (시간 큰 Sequencer — 순수 timing) — 5016 범위 외 명시
  - **C**: 22 파일 모두 MotionHandle (큰 Coroutine 도 LSequence 분해, 작업량 ↑)

### D4. Sprint 1 직후 LitMotion v2.0.1 + Unity 6.3 호환 검증

- **현재 PRD**: Sprint 1 commit 후 일반 컴파일 검증만
- **이유 검토**: LitMotion v2.0.1 (2025-02-09 release) — Unity 6.3 LTS (Unity 6000.4.4f1) 검증 X. Sprint 1 commit 후 컴파일 + 임포트 + 단순 LMotion.Create 동작 확인 필요?
- **옵션**:
  - **A (★권장)**: Sprint 1 직후 5분 smoke test (LMotion.Create + Bind + 결과 검증). 실패 시 5015c 재정의
  - **B**: Sprint 2 EnemyController 마이그레이션 시 통합 검증 (현재 PRD 묵시적)
  - **C**: Sprint 6 NUnit 까지 통합 (가장 늦은 검증, 손실 ↑)

## 5. CLAUDE.md §15 정책 위반 점검

| 정책 | 검증 결과 |
|---|---|
| §15.1 SRP | ✅ LMotionExtensions = wrapper 단일 책임 |
| §15.1 OCP | ✅ extension method 패턴 — 신규 타입 추가 시 기존 코드 변경 X |
| §15.4 SO 우선 | ✅ duration/strength 등 magic number → 기존 SO (BalanceConfig / CinematicEffectsConfig) 활용 |
| §15.6 100L | ✅ LMotionExtensions ~70L |
| §15.7 DI | ✅ 정적 헬퍼 + extension method (Singleton X) |
| §15.10 외부 라이브러리 | ✅ LitMotion MIT — CLAUDE.md §10 추가 명시 |
| §15.11 GC 0 | ✅ LitMotion 의 핵심 가치 — 0 alloc 보장 (Sprint 6 GCAllocMeasurer 재측정 검증) |
| §15.12 Validator | ✅ LitMotion_Migration 룰 신규 |

→ 모든 §15 정책 정합 ✅

## 6. PIVOT v4 §7 성능 목표 정합

| 목표 | 현재 | 5016 후 기대 |
|---|---|---|
| 동시 적 cap 300 | 유지 | 유지 (마이그레이션 시각 동일) |
| 60fps | 유지 | 마진 강화 (frame time spike ↓) |
| GC <50KB/s | ❌ 시나리오 C (>50 KB/s) | ✅ 시나리오 A (<30 KB/s) |
| GPU draw call <10/frame | 영향 없음 | 영향 없음 |
| 메모리 250MB | 영향 없음 | Burst native +10~20MB |
| 앱 용량 150MB | 영향 없음 | Burst pre-built +5~15MB (D2 측정 필요) |

## 7. 권장 변경

**즉시 변경 (Phase 2.2 Rebuttal)**: 없음 — 모든 Accept 항목 PRD 그대로 진행 가능

**사용자 결정 후 변경 (Phase 2.3 Mediator)**:
- D1 결정 → Sprint 4 분해/통합 → §4 Sprint 표 갱신
- D2 결정 → Sprint 1+6 양시점 측정 → §4 Sprint 1 추가 항목
- D3 결정 → 5016 범위 정의 → §1.1 인벤토리 표 + §4 Sprint 5 명시
- D4 결정 → Sprint 1 smoke test → §4 Sprint 1 추가 항목

## 8. 변경 이력
- 2026-05-05: 초기 작성 (Phase 2.1 Critique)
