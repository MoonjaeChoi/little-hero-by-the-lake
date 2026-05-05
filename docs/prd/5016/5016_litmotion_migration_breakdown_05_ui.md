# T05: UI 영역 마이그레이션

> **레이어**: UI
> **의존성**: T01
> **예상 시간**: 45분 / 1 commit
> **PRD 출처**: `5016_litmotion_migration_v2.md` §2 Sprint 5

## 1. 작업 항목

### 1.1 FadeOverlay.cs

**파일**: `LittleHero/Assets/_Project/Scripts/Core/FadeOverlay.cs`

**변경**:
- `IEnumerator FadeIn(float duration)` → `MotionHandle FadeIn(float duration)` (`_blackImage.DOFadeAlpha(0f, duration).WithIgnoreTimeScale(true)`)
- `IEnumerator FadeOut(float duration)` → `MotionHandle FadeOut(float duration)` (`_blackImage.DOFadeAlpha(1f, duration).WithIgnoreTimeScale(true)`)
- 매 frame `new Color` alloc 폐기 → 1 alloc
- 호출처 (`SceneTransitionService`) — `yield return overlay.FadeOut(...)` → `yield return overlay.FadeOut(...).ToYieldInteraction()` 호환 wrapper 활용 (Coroutine 잔존 영역)

### 1.2 WaveTransitionToast.cs (사용자 baseline 가독성 정합)

**파일**: `LittleHero/Assets/_Project/Scripts/UI/WaveTransitionToast.cs`

**변경**:
- `IEnumerator ShowCountdown(...)` 의 `Time.unscaledDeltaTime` 기반 카운트다운 → `LMotion.Create(remaining, 0f, remaining).WithIgnoreTimeScale(true).Bind(...)`
- progress dots `●●●○○` 갱신 — Bind 내부 처리
- `_countdownText` / `_progressBar` SerializeField 정합

### 1.3 HUDController.cs

**파일**: `LittleHero/Assets/_Project/Scripts/UI/HUDController.cs`

**변경**:
- `IEnumerator FadeIn` / `FadeOut` (있으면) → LMotion
- HP/MP 바 fade 효과 → `Image.DOFillAmount` 활용

### 1.4 MythicEffectOverlay.cs

**파일**: `LittleHero/Assets/_Project/Scripts/UI/MythicEffectOverlay.cs`

**변경**:
- `IEnumerator FlashRoutine` / `FadeRoutine` (Coroutine 2종) → LMotion 2

**예상 라인**: 4 파일 합계 ~50L 변경

## 2. 검증 기준

### 2.1 컴파일
- errors=0, 5초 대기

### 2.2 Play Mode (수동)
- Scene 전환 fade in/out 시각 동일 (FadeOverlay)
- Wave 1 → wave 2 토스트 카운트다운 + progress dots 정상 (사용자 baseline 정합)
- Mythic 발현 시 overlay flash 정상

### 2.3 회귀
- `Time.timeScale = 0` 환경에서 fade 진행 정상 (`WithIgnoreTimeScale(true)`)
- WaveManager Coroutine 잔존 (D3=B 5016 범위 외) — wave 진행 timing 동작 유지

## 3. commit 메시지

(PRD §2 Sprint 5 commit 메시지 그대로)

## 4. 관련 문서

- PRD §2 Sprint 5
- 디자인 §3.3 FadeOverlay.FadeOut 마이그레이션 예시
- decisions D3=B (큰 timing Coroutine 잔존)

## 5. 알려진 위험

- `WithIgnoreTimeScale(true)` 누락 시 paused state 에서 fade 멈춤 — 모든 UI 영역 명시
- WaveTransitionToast 카운트다운 정확 — LMotion duration 과 사용자 baseline timing 일치
