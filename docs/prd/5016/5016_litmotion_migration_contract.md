# Sprint Contract: 5016 LitMotion 마이그레이션

> **PGE Planner — Phase 4 Contract**
> PRD: `5016_litmotion_migration_v2.md` · breakdown: `5016_litmotion_migration_breakdown_00.md`
> 생성일: 2026-05-05

---

## 1. C1 컴파일 기준

- **C1.1**: 모든 신규 .cs 파일 컴파일 errors=0
  - `Scripts/Core/LMotionExtensions.cs`
  - `Tests/Editor/LMotionExtensionsSmokeTest.cs`
  - `Tests/Editor/LMotionExtensionsTests.cs`
  - 수정: `EnemyController.cs` / `CinematicEffects.cs` / `DamageNumberFloater.cs` / `BossVineAttack.cs` / `BossTornadoAttack.cs` / `BossSummonAttack.cs` / `BossSpreadAttack.cs` / `BossPulseAttack.cs` / `FadeOverlay.cs` / `WaveTransitionToast.cs` / `HUDController.cs` / `MythicEffectOverlay.cs`
- **C1.2**: 도메인 리로드 후 10초 내 안정화 (Burst 컴파일 포함)
- **C1.3**: 각 Sprint commit 후 errors=0

**검증 방법**:
```
mcp__UnityMCP__refresh_unity
sleep 10  # Burst 컴파일 포함
mcp__UnityMCP__read_console (errors)  # 0 이어야 함
```

---

## 2. C2 Editor Test 기준 (NUnit)

| Test 클래스 | 신규 메서드 수 | 목적 |
|---|---:|---|
| `LMotionExtensionsSmokeTest` (T01) | 1 | LMotion 동작 smoke (D4=A) |
| `LMotionExtensionsTests` (T06) | 6 | 6 extension method 검증 |
| **누적** | **7** | **총 NUnit 100 (94 + 6 + 1 — Sprint 1 smoke 별도)** |

### 2.1 LMotionExtensionsSmokeTest (T01)

- `Sprint1_LMotion_Create_Bind_ProducesActiveHandle` — 활성 handle 반환 검증

### 2.2 LMotionExtensionsTests (T06)

- `DOFadeAlpha_SpriteRenderer_ReturnsActiveHandle` — SpriteRenderer fade 검증
- `DOFadeAlpha_Image_ReturnsActiveHandle` — Image fade
- `DOFade_CanvasGroup_ReturnsActiveHandle` — CanvasGroup fade
- `DOPunchScale_Transform_ReturnsActiveHandle` — Transform punch
- `DOMove_Transform_ReturnsActiveHandle` — Transform move
- `Type_LMotionExtensions_IsStaticClass` — 정적 클래스 확인

**검증 방법**: Test Runner Edit Mode > Run All → 7/7 PASS (100 NUnit 누적)

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings ≤ 기존 (5014 시작 시점 — 21 추정, 측정 후 갱신)
- **C3.3**: 신규 위반 없음 (LMotionExtensions ~70L < 100L ✅)

**검증 방법**: `Tools/LittleHero/Validate Project` 실행 → 콘솔 출력 비교

**Validator 룰 추가 명세** (T06):
- `5016_LitMotion_Migration` — DOTween 재도입 검출 + LitMotion import ≥ 22 files

---

## 4. C4 Reflection 검증 (정적)

| Type | Method/Field |
|---|---|
| `LittleHero.LMotionExtensions` (정적 클래스) | `DOFadeAlpha(SpriteRenderer, float, float)` / `DOFadeAlpha(Image, float, float)` / `DOFade(CanvasGroup, float, float)` / `DOPunchScale(Transform, float, float)` / `DOMove(Transform, Vector3, float)` / `DOFillAmount(Image, float, float)` |
| `LittleHero.EnemyController` | `DoPunch(float, float, float)` 신규 / `DoFlash(Color, float, float)` 신규 / `_punchHandle` 필드 / `_flashHandle` 필드 |
| `LittleHero.CinematicEffects` | `PlayHitPause(float)` / `PlayScreenShake(float, float)` / `PlayCameraZoom(float, float, float)` |
| `LittleHero.FadeOverlay` | `FadeIn(float)` 반환 `MotionHandle` / `FadeOut(float)` 반환 `MotionHandle` |

**검증 방법** (script):
```csharp
var t = System.Type.GetType("LittleHero.LMotionExtensions, Assembly-CSharp");
return t != null && t.GetMethod("DOFadeAlpha", new[] {typeof(SpriteRenderer), typeof(float), typeof(float)}) != null;
```

---

## 5. C5 SO 검증

(LitMotion 마이그레이션은 코드 영역만 — SO 신규/마이그레이션 0건)

- `BalanceConfig.attackIntervalMul = 0.7f` 유지 (사용자 baseline)
- `CinematicEffectsConfig` 필드 유지 (HitPause duration 등)

---

## 6. C6 Play 검증 시나리오

### 시나리오 S0.1: Sprint 1 직후 Player 빌드 (D2=A)

**사전 조건**: T01 commit 완료

**단계**:
1. Unity Editor → File → Build Settings → Mac/iOS → Build
2. Builds/<platform>/ 산출물 용량 측정

**기대 결과**:
- App 용량 < 150MB (CLAUDE.md §7)
- 임계 초과 시 5015c 재정의 검토

---

### 시나리오 S0.2: Sprint 1 직후 smoke test (D4=A)

**사전 조건**: T01 commit 완료

**단계**:
1. Tests/Editor `LMotionExtensionsSmokeTest` 실행
2. `LMotion.Create + Bind` 동작 검증

**기대 결과**:
- MotionHandle.IsActive() == true
- 실패 시 5015c 재정의

---

### 시나리오 S1: EnemyController 핫 패스 (T02 후)

**사전 조건**: T02 commit 완료

**단계**:
1. CH 1 wave 1 진입 → 적 hit
2. punch scale 1.4× (크리 1.7×) 시각 확인
3. flash 색상 적용 + 종료 후 _baseColor 복귀 확인 (5012.1 hue 유지)

**기대 결과**:
- 시각 회귀 X
- 5012 hue tint 동작 유지

---

### 시나리오 S2: CinematicEffects (T03 후)

**사전 조건**: T03 commit 완료

**단계**:
1. Critical hit 발생 (5% baseline, 강제 트리거 권장)
2. HitPause 정상 (timeScale 0.05 → 1.0 복귀)
3. Screen shake 정상
4. Camera zoom in/out 정상

**기대 결과**:
- HitPause 후 timeScale = 1.0 정확 복귀 (게임 정지 회피)
- Shake amplitude 시각 동일

---

### 시나리오 S3: DamageNumberFloater + Boss attacks (T04 후)

**사전 조건**: T04 commit 완료

**단계**:
1. 적 hit 시 데미지 숫자 floater 출현
2. 위로 떠오르며 fade 정상 (1.2K / 15K / 1.5M 압축)
3. Boss 공격 (vine/tornado/spread/pulse) 시각 확인

**기대 결과**:
- Floater Pool.Release 정확 (메모리 누수 X)
- Boss 공격 timing 동일

---

### 시나리오 S4: UI 영역 (T05 후)

**사전 조건**: T05 commit 완료

**단계**:
1. Scene 전환 fade in/out
2. Wave 1 → wave 2 토스트 카운트다운 + progress dots
3. Mythic 발현 overlay flash

**기대 결과**:
- WithIgnoreTimeScale(true) 동작 (paused 환경에서도 fade)
- WaveManager Coroutine 잔존 (D3=B 5016 범위 외) — wave 진행 정상

---

### 시나리오 S5: GC 재측정 (T06 후) — 5016 핵심 검증

**사전 조건**: 모든 Sprint 완료

**단계**:
1. Unity Editor → `Tools/LittleHero/Measure GC Alloc (60s)` 실행
2. 60초 측정 (cap 300 환경)
3. 콘솔 결과 확인

**기대 결과**:
- **시나리오 A (< 30 KB/s) 도달** ← 5016 성공 기준
- 시나리오 B (30~50 KB/s) → 핫 패스 누락 fix (5016.1)
- 시나리오 C (> 50 KB/s) → 회귀, 검토 필요

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0 (각 Sprint)
- [ ] Editor Test 7/7 PASS (LMotionExtensionsSmokeTest 1 + LMotionExtensionsTests 6)
- [ ] ProjectValidator warnings ≤ 기존 + LitMotion 룰 통과
- [ ] **GCAllocMeasurer 시나리오 A (< 30 KB/s) 도달** ← 5016 핵심
- [ ] App 용량 < 150MB (D2=A Sprint 1 + Sprint 6 양시점)
- [ ] 기존 시스템 회귀 X (5012 hue + 5013 visibility culler + boss 시스템)
- [ ] Play Mode 시각 회귀 X (S1~S4)

## 8. 합격 기준

**최소 합격**:
- C1 컴파일 ✅
- C2 Test 5/7 이상 ✅
- C3 Validator errors=0 ✅
- 시나리오 S0.1 + S0.2 PASS
- 시나리오 S1 + S5 PASS (핫 패스 + GC)

**완전 합격**:
- 모든 카테고리 ✅
- 시나리오 S0.1, S0.2, S1, S2, S3, S4, S5 모두 PASS
- 비기능 요구사항 모두 ✅
- **GCAllocMeasurer 시나리오 A (< 30 KB/s) 도달**

## 9. Trade-off (mediator 결정 기반)

- **D1=A 통합**: Sprint 4 = Floater + Boss attacks 1 commit. 1 의미 단위 ("Lerp 폐기 묶음"). 회수성 미세 손실.
- **D2=A 양시점**: Sprint 1 + Sprint 6 빌드 측정. +5분 비용. 5 Sprint 손실 위험 회피.
- **D3=B 핫 패스만**: WaveManager/ChapterRunner/BossEncounterRunner Coroutine 잔존 → 단일 패러다임 ↓, 큰 timing Coroutine 가독성 ↑ + GC 효과 미미.
- **D4=A smoke test**: Sprint 1 +5분. 호환 검증 이른 발견.

## 10. 변경 이력
- 2026-05-05: 초기 작성 (Phase 4 Contract)
