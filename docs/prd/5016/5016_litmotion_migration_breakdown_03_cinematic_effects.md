# T03: CinematicEffects HitPause + Shake + Zoom 마이그레이션

> **레이어**: Logic (핫 패스 — critical hit)
> **의존성**: T01
> **예상 시간**: 40분 / 1 commit
> **PRD 출처**: `5016_litmotion_migration_v2.md` §2 Sprint 3

## 1. 작업 항목

### 1.1 CinematicEffects.cs 수정

**파일**: `LittleHero/Assets/_Project/Scripts/Gameplay/Combat/CinematicEffects.cs`

**변경 1 — HitPauseRoutine → PlayHitPause**:
- `IEnumerator HitPauseRoutine` (line ~40~47) 폐기
- `MotionHandle _hitPauseHandle` 필드 신규
- `WithIgnoreTimeScale(true)` + `WithOnComplete(() => Time.timeScale = prevScale)` + `RunWithoutBinding()`
- `WaitForSecondsRealtime` alloc 제거

**변경 2 — ShakeRoutine → PlayScreenShake**:
- `IEnumerator ShakeRoutine` (line ~56~71) 폐기 — **매 frame `new Vector3` alloc** 가 GC 핫스팟
- `LMotion.Shake.Create(Vector3.zero, Vector3.one * amplitude, duration).Bind(offset => cam.transform.localPosition = origin + offset)` 으로 전환
- `_shakeHandle.IsActive() && Cancel()` 가드

**변경 3 — ZoomRoutine → PlayCameraZoom**:
- `IEnumerator ZoomRoutine` (line ~86~) 폐기
- `LSequence.Create().Append(zoomIn).Append(zoomOut).Run()` 으로 전환
- `WithIgnoreTimeScale(true)` 적용 (HitPause 와 동시 동작 호환)

**예상 라인**: ~40L 추가 / ~50L 삭제 (순감 ~10L)

## 2. 검증 기준

### 2.1 컴파일
- errors=0, 5초 대기

### 2.2 Play Mode (수동)
- Critical hit (5% baseline) 발생
- HitPause timeScale 0.05 × 0.08s → **1.0 복귀** 정상
- Screen shake amplitude 정상 (cap 300 환경 GC 핫스팟 ↓)
- Camera zoom in/out 정상

### 2.3 회귀
- HitPause 후 Time.timeScale = 1.0 정확 복귀 (게임 흐름 정지 회피)

## 3. commit 메시지

(PRD §2 Sprint 3 commit 메시지 그대로)

## 4. 관련 문서

- PRD §2 Sprint 3
- 디자인 §3.3 CinematicEffects.ShakeRoutine 마이그레이션 예시

## 5. 알려진 위험

- HitPause WithOnComplete 콜백 미실행 시 timeScale 미복귀 → 게임 정지. **prevScale 캡처 + WithOnComplete 검증 필수**
- LMotion.Shake API 동작 — Vector3 offset 누적/단일 전환 검증
