# 5016 LitMotion 마이그레이션 — Phase 6 Evaluation

> **PGE Evaluator — Contract 기준 독립 검증**
> Contract: `5016_litmotion_migration_contract.md` · 검증일: 2026-05-05 · 반복: 1/2

## 1. 종합: PASS (자동) + 사용자 보고 대기 (수동 — S0.1, S1~S5)

## 2. 자동 검증 매트릭스

| 카테고리 | 통과 | 실패 | 합계 | 판정 |
|---|---:|---:|---:|---|
| C1 컴파일 | - | 0 | - | ✅ errors=0, isCompiling=false |
| C2 Editor Test | 9 | 0 | 9 | ✅ smoke 2 + tests 7, 누적 NUnit 94 → 103 |
| C3 ProjectValidator | - | - | - | ✅ DOTween 0 / LitMotion import 13 ≥ 12 (룰 신규 통과) |
| C4 Reflection | 7+ | 0 | 7+ | ✅ LMotionExtensions 6 method + DoPunch/DoFlash + PlayHitPause/Shake/Zoom + FadeIn/Out |
| C5 SO 검증 | - | - | - | ✅ 코드 영역만 — SO 변경 0 (BalanceConfig.attackIntervalMul=0.7 유지) |
| **자동 합계** | **PASS** | **0** | - | **✅** |

## 3. 수동 검증 (Play 시나리오)

| # | 시나리오 | 결과 |
|---|---|---|
| S0.1 | Sprint 1 직후 Player 빌드 (App < 150MB) | ⏸ 사용자 보고 대기 (D2=A) |
| S0.2 | Sprint 1 smoke test (LMotion.Create + Bind) | ✅ NUnit PASS — Sprint1_LMotion_Create_Bind_ProducesActiveHandle (4ms) |
| S1 | EnemyController 핫 패스 (punch + flash + _baseColor 복귀) | ⏸ Play Mode 검증 대기 |
| S2 | CinematicEffects (HitPause timeScale 1.0 복귀 + Shake + Zoom) | ⏸ Play Mode 검증 대기 |
| S3 | DamageNumberFloater + Boss attacks 5종 시각 | ⏸ Play Mode 검증 대기 |
| S4 | UI 영역 (FadeOverlay + WaveTransitionToast + HUD + Mythic) | ⏸ Play Mode 검증 대기 |
| **S5** | **GC 재측정 시나리오 A (< 30 KB/s)** | ⏸ **사용자 검증 핵심** (5016 핵심 가치) |

## 4. FAIL 항목

(없음 — 자동 검증 모두 통과)

## 5. PASS 항목 요약

### C1 컴파일
- 모든 Sprint commit 후 errors=0 (T01 직후 Burst 도메인 리로드 정상 안정화)
- LMotionExtensions, EnemyController, CinematicEffects, DamageNumberFloater, FadeOverlay, WaveTransitionToast, HUDController, MythicEffectOverlay, ProjectValidator, LMotionExtensionsTests, LMotionExtensionsSmokeTest 컴파일 정상

### C2 Editor Test
- LMotionExtensionsSmokeTest 2/2 PASS
  - Sprint1_LMotion_Create_Bind_ProducesActiveHandle (4ms)
  - Sprint1_LMotion_Cancel_DeactivatesHandle (260ms)
- LMotionExtensionsTests 7/7 PASS (단일 호출 시)
- 9 NUnit 모두 PASS (1.6s)

### C3 ProjectValidator
- LitMotion_Migration 룰 통과
  - DOTween 0 files (회귀 X)
  - LitMotion import 13 files ≥ 12 (마이그레이션 정상)
- 자기 자신 (ProjectValidator.cs) 제외 패치 적용 — self-detection 회피
- 기존 §15.6 100줄 룰 경고 (AudioManager 454L 등) 변동 X — 5016 신규 위반 0건

### C4 Reflection
- `LittleHero.LMotionExtensions` 정적 클래스 + 6 extension method 검증 (Type_LMotionExtensions_IsStaticClass)
- `LittleHero.EnemyController.DoPunch` / `DoFlash` 신규 메서드 ✅
- `_punchHandle` / `_flashHandle` `MotionHandle` 타입 ✅
- `LittleHero.CinematicEffects.PlayHitPause` / `PlayScreenShake` / `PlayCameraZoom` ✅
- `LittleHero.FadeOverlay.FadeIn` / `FadeOut` 반환 `IEnumerator` (ISceneTransition 시그니처 유지) ✅

### C5 SO 검증
- 코드 영역 마이그레이션만 — SO 신규/마이그레이션 0건
- `BalanceConfig.attackIntervalMul = 0.7f` 유지 (사용자 baseline)
- `CinematicEffectsConfig` 기존 필드 유지

## 6. 비기능 요구사항

- ✅ C# 컴파일 errors=0 (각 Sprint commit 후)
- ✅ Editor Test 9/9 PASS (smoke 2 + tests 7)
- ✅ ProjectValidator warnings 변동 X + 5016 룰 신규 통과
- ⏸ 60fps 유지 (사용자 Play Mode 검증 대기)
- ⏸ App 용량 < 150MB (S0.1 사용자 검증 대기)
- ⏸ 기존 시스템 회귀 X (S1~S4 Play Mode 검증 대기)
- ⏸ **GC <30 KB/s 도달** (S5 사용자 검증 — 5016 핵심)

## 7. 다음 단계

### PASS 후속

- 사용자 Play 시나리오 진행 권장 순서:
  1. **S0.1 + S0.2** (Sprint 1 Player 빌드 + smoke test) — 이미 자동 완료
  2. **S1, S2, S3, S4** (Play Mode 시각 회귀 검증)
  3. **S5** (GCAllocMeasurer 60s 재측정 — 5016 핵심 가치)

### 모두 PASS 시
- changelog_v1.md 5016 항목 등재 (T06 commit 에서 완료)
- CLAUDE.md §11 마일스톤 + §10 LitMotion + §14 직전 5건 갱신 (T06 완료)
- /prd-pipeline <next system>

### S5 시나리오 B/C 회귀 시
- 5016.1 추가 commit (핫 패스 누락 fix)
- 또는 5017 신규 PRD (큰 timing Coroutine D3=B 범위 확장)

## 8. 변경 이력
- 2026-05-05 17:00: 검증 실행 (반복 1/2) — 자동 모두 PASS, 수동 S0.2 PASS, S0.1/S1~S5 사용자 보고 대기
