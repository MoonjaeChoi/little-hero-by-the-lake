# Evaluator Checklist: 5016 LitMotion 마이그레이션

## 자동 검증 (스크립트 / MCP 자동화)

### C1 컴파일
- [ ] `mcp__UnityMCP__refresh_unity` 후 errors=0
- [ ] `editor_state.isCompiling == false` (10s 대기 — Burst 컴파일)
- [ ] 각 Sprint commit 후 errors=0

### C2 Editor Test (NUnit)
- [ ] Test Runner > Edit Mode > Run All
- [ ] `LMotionExtensionsSmokeTest` 1/1 PASS
- [ ] `LMotionExtensionsTests` 6/6 PASS
- [ ] 누적 100 NUnit 모두 PASS
- [ ] (자동) reflection 으로 7 메서드 명세 확인

### C3 ProjectValidator
- [ ] `Tools/LittleHero/Validate Project` 실행
- [ ] errors=0
- [ ] warnings ≤ 기존 + 1 (LMotion 통과 로그 1)
- [ ] DOTween 재도입 0 (검출 시 FAIL)
- [ ] LitMotion import ≥ 22 files

### C4 Reflection 정적 검증
- [ ] `LittleHero.LMotionExtensions` 정적 클래스 존재
- [ ] 6 extension method 시그니처 정확 (DOFadeAlpha × 2, DOFade, DOPunchScale, DOMove, DOFillAmount)
- [ ] `EnemyController.DoPunch` / `DoFlash` 신규 메서드 존재
- [ ] `EnemyController._punchHandle` / `_flashHandle` 필드 존재 (`MotionHandle` 타입)
- [ ] `CinematicEffects.PlayHitPause` / `PlayScreenShake` / `PlayCameraZoom` 신규 메서드
- [ ] `FadeOverlay.FadeIn` / `FadeOut` 반환 타입 `MotionHandle`

### C5 SO 검증
- [ ] `BalanceConfig.attackIntervalMul = 0.7f` 유지
- [ ] `CinematicEffectsConfig` 기존 필드 유지

## 수동 검증 (사용자 Play Mode)

### S0.1 Player 빌드 — App 용량 (T01 후)
- [ ] File → Build Settings → Mac/iOS → Build
- [ ] App 용량 < 150MB
- [ ] (FAIL 시) 5015c 재정의 검토

### S0.2 Sprint 1 smoke test (T01 후)
- [ ] Tests/Editor `LMotionExtensionsSmokeTest` 실행 PASS
- [ ] (FAIL 시) 5015c 재정의

### S1 EnemyController 핫 패스 (T02 후)
- [ ] CH 1 wave 1 적 hit → punch scale 1.4× (크리 1.7×) 시각
- [ ] flash 색상 적용 + _baseColor 복귀 (5012.1 hue 유지)
- [ ] 시각 회귀 X

### S2 CinematicEffects (T03 후)
- [ ] Critical hit → HitPause (timeScale 0.05 → 1.0 복귀)
- [ ] Screen shake 정상
- [ ] Camera zoom in/out 정상
- [ ] **timeScale 미복귀 (게임 정지) FAIL 검사 필수**

### S3 Floater + Boss attacks (T04 후)
- [ ] 데미지 숫자 floater 위로 + fade
- [ ] 1.2K / 15K / 1.5M 압축 표시
- [ ] Pool.Release 정확 (메모리 누수 X)
- [ ] Boss 공격 5종 시각 동일

### S4 UI 영역 (T05 후)
- [ ] Scene 전환 fade in/out 정상
- [ ] Wave 1 → 2 카운트다운 + progress dots 정상
- [ ] Mythic overlay flash 정상

### S5 GC 재측정 (T06 후) — 5016 핵심
- [ ] `Tools/LittleHero/Measure GC Alloc (60s)` 실행
- [ ] **시나리오 A (< 30 KB/s) 도달** — 5016 성공
- [ ] 시나리오 B (30~50) → 5016.1 fix 사이클
- [ ] 시나리오 C (> 50) → 회귀 검토

## 비기능 검증

- [ ] 기존 5012 hue tint 정상
- [ ] 기존 5013 visibility culler 정상
- [ ] Boss 시스템 (5001) 정상
- [ ] 가챠 / 한정 상점 / 도감 영향 X
- [ ] 60fps 유지 (Profiler 권장)
- [ ] App 용량 < 150MB (Sprint 6 빌드 후 재측정)

## 합격 판정

- 자동 검증 모두 ✅ + S0.1/S0.2 + S1 + S5 PASS → **최소 합격**
- 자동 검증 모두 ✅ + S0.1, S0.2, S1, S2, S3, S4, S5 모두 PASS → **완전 합격**
- 자동 검증 1개라도 ❌ → **FAIL** (Phase 5 수정)
- **GC 시나리오 A 미도달 → FAIL (5016 핵심 가치 미달성)**
