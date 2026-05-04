# 5015b — IEnumerator+Lerp → LitMotion 마이그레이션 v1 (디자인)

> **목적**: IEnumerator + Lerp 패턴 22 파일 → LitMotion v2.0.1 (MIT, 0 GC, Burst 가속) 일괄 전환.
> 5015b GCAllocMeasurer 측정 결과 시나리오 C (> 50 KB/s) 임계 초과 — 전체 마이그레이션 정당화.
> **우선순위**: 높음 (PIVOT v4 §7 GC <50KB/s 임계 위반 — 60fps 마진 강화 + frame time spike 회피).

## 0. 결정 이력

- 2026-05-04: 5015 RESERVED 결정 (D1=A) — PRD 전제 무효 (DOTween 사용 0건)
- 2026-05-04: 5015b 사전조사 보고서 — `docs/prd/5015b_premigration_research.md` (코드 인벤토리 + GC 정적 분석 + LitMotion 외부 정보)
- 2026-05-05: 사용자 baseline 변경 — AttackSpeed × 5 영구 + 공격 간격 × 0.7 (BalanceConfig.attackIntervalMul=0.7) → ~7.14× hit 빈도 (정상 게임플레이)
- 2026-05-05: GCAllocMeasurer 60s 측정 → **시나리오 C (> 50 KB/s) 확정** → 5015b 전체 마이그레이션 진행

## 1. 현재 시스템 분석

### 1.1 IEnumerator + Lerp 패턴 인벤토리

| 카테고리 | 파일 수 | 핵심 파일 |
|---|---:|---|
| **UI** (fade / scale / floating) | 4 | FadeOverlay / HUDController / MythicEffectOverlay / WaveTransitionToast |
| **Combat/VFX** (HitEffect / Floater / Pool) | 1 | CinematicEffects |
| **Core** (SceneTransition / Audio) | 4 | SceneTransitionService / ISceneTransition / AudioManager / SmokeTest |
| **Gameplay 기타** | 5 | EnemyController / WaveManager / ChapterRunner / InfiniteTowerRunner / BossEncounterRunner |
| **Boss attacks** (Lerp 직접) | 5 | BossVineAttack / BossTornadoAttack / BossSummonAttack / BossSpreadAttack / BossPulseAttack |
| **VFX 추가** | 3 | PulseVisual / SimpleFx / DamageNumberFloater |
| **합계** | **22** | |

### 1.2 핫 패스 (cap 300 환경 빈도 ↑)

| 파일 | Coroutine | Lerp | new alloc | 호출 빈도 |
|---|---:|---:|---:|---|
| **EnemyController** | 6 | 3 | 4 | 🔥 매 hit (PunchRoutine + FlashRoutine) |
| **CinematicEffects** | 6 | 2 | 2 | 🔥 매 critical hit (HitPause + Shake + Zoom) |
| **DamageNumberFloater** | 0 | 2 | 9 | 🔥 매 데미지 floater |

### 1.3 GC 측정 결과 (사용자 baseline 환경)

- 측정 환경: AttackSpeed × 5 + interval × 0.7 = ~7.14× hit 빈도
- 측정 도구: GCAllocMeasurer (Tools/LittleHero/Measure GC Alloc 60s)
- 결과: **시나리오 C (> 50 KB/s)** — PIVOT v4 §7 임계 초과
- 핵심 alloc 원인:
  1. Coroutine state machine alloc (매 hit ~2 alloc)
  2. `new Vector3` / `new Color` in while loop (FadeOverlay, CinematicEffects.ShakeRoutine)
  3. `WaitForSecondsRealtime` 객체 alloc (HitPauseRoutine 매 호출)

### 1.4 CLAUDE.md §10 외부 라이브러리

5015 RESERVED 사이클에서 fix 완료 — DOTween 행 제거 + "(없음 — IEnumerator + Lerp 직접 사용)" 명시. 본 PRD 진행 시 LitMotion MIT 행 추가 예정.

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A (★권장)** | **22 파일 일괄 마이그레이션** (사전조사 시나리오 C) | 일관 + 0 GC + Burst 가속 + 라이선스 cleanup | 큰 작업 (~6.5h) |
| B | 핫 패스 3 파일만 (시나리오 B) | 빠른 검증 | 일관성 ↓ (혼재) |
| C | DOTween 신규 도입 후 마이그레이션 | wrapper 호환 ↑ | 라이선스 부담 (DOTween 커스텀) |
| D | UniTask + async/await 직접 (LitMotion 미도입) | C# 표준 | tween API 부재, 학습 곡선 ↑ |

> 권장 **A** — 시나리오 C 임계 초과 + 영구 baseline (사용자 결정) → 즉각 GC 0 효과 필요.

## 3. 권장 설계 (CLAUDE.md §15 정합)

### 3.1 LitMotion 패키지 도입

**Packages/manifest.json**:
```json
{
  "dependencies": {
    "com.annulusgames.lit-motion": "https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion",
    "com.unity.burst": "1.6.0",
    "com.unity.collections": "1.5.1",
    "com.unity.mathematics": "1.0.0"
  }
}
```

LitMotion v2.0.1 (2025-02-09 release) + 의존성 3종.

### 3.2 신규 파일 명세

#### Scripts/Core/LMotionExtensions.cs (~70L)

DOTween-like API wrapper. CLAUDE.md §15.6 100L 이내.

```csharp
public static class LMotionExtensions
{
    // SpriteRenderer fade
    public static MotionHandle DOFadeAlpha(this SpriteRenderer sr, float endAlpha, float duration);
    // Image fade
    public static MotionHandle DOFadeAlpha(this Image img, float endAlpha, float duration);
    // CanvasGroup fade
    public static MotionHandle DOFade(this CanvasGroup cg, float endValue, float duration);
    // Transform scale (punch)
    public static MotionHandle DOPunchScale(this Transform tr, float strength, float duration);
    // Transform position
    public static MotionHandle DOMove(this Transform tr, Vector3 endValue, float duration);
    // Image fillAmount
    public static MotionHandle DOFillAmount(this Image img, float endValue, float duration);
}
```

내부적으로 `LMotion.Create(...).WithEase(...).Bind(...)` 호출.

### 3.3 마이그레이션 패턴 (예시)

#### EnemyController.PunchRoutine (현 → LitMotion)

```csharp
// 현 (line 151~169)
private IEnumerator PunchRoutine(float targetScale, float expandTime, float returnTime)
{
    float t = 0f;
    while (t < expandTime) {
        t += Time.unscaledDeltaTime;
        _punchScale = Mathf.Lerp(1f, targetScale, t / expandTime);
        yield return null;
    }
    t = 0f;
    while (t < returnTime) {
        t += Time.unscaledDeltaTime;
        _punchScale = Mathf.Lerp(targetScale, 1f, t / returnTime);
        yield return null;
    }
    _punchScale = 1f;
    _punchCo = null;
}

// LitMotion 전환 (~10L)
private MotionHandle _punchHandle;
private void DoPunch(float targetScale, float expandTime, float returnTime)
{
    if (_punchHandle.IsActive()) _punchHandle.Cancel();
    _punchHandle = LSequence.Create()
        .Append(LMotion.Create(1f, targetScale, expandTime).Bind(s => _punchScale = s))
        .Append(LMotion.Create(targetScale, 1f, returnTime).Bind(s => _punchScale = s))
        .Run();
}
```

#### FadeOverlay.FadeOut (현 → LitMotion)

```csharp
// 현 (line 23-33)
public IEnumerator FadeOut(float duration)
{
    if (_blackImage == null) yield break;
    float t = 0f;
    while (t < duration) {
        t += Time.unscaledDeltaTime;
        _blackImage.color = new Color(0, 0, 0, Mathf.Clamp01(t / duration));
        yield return null;
    }
}

// LitMotion (~3L)
public MotionHandle FadeOut(float duration)
{
    if (_blackImage == null) return default;
    return _blackImage.DOFadeAlpha(1f, duration).WithIgnoreTimeScale(true);
}
```

#### CinematicEffects.ShakeRoutine (new Vector3 alloc 회피)

```csharp
// 현 (line 56-71) — 매 frame new Vector3 alloc
private IEnumerator ShakeRoutine(float amplitude, float duration) { ... }

// LitMotion (Vector3 struct return, no alloc)
private MotionHandle _shakeHandle;
public void PlayScreenShake(float amplitude, float duration)
{
    var cam = Camera.main;
    if (cam == null) return;
    var origin = cam.transform.localPosition;
    if (_shakeHandle.IsActive()) _shakeHandle.Cancel();
    _shakeHandle = LMotion.Shake.Create(Vector3.zero, Vector3.one * amplitude, duration)
        .Bind(offset => cam.transform.localPosition = origin + offset);
}
```

### 3.4 Coroutine ↔ LitMotion 호환 (점진 마이그레이션)

```csharp
// IEnumerator wrapper (yield return MotionHandle)
yield return LMotion.Create(0f, 1f, 0.5f).Bind(...).ToYieldInteraction();
```

→ Sprint 별 점진 전환 + 컴파일 호환성 유지.

### 3.5 ProjectValidator 룰 추가

```csharp
[5015b T05] ValidateLitMotionMigration
- using DG.Tweening 검출 시 경고 (DOTween 재도입 회귀 방지)
- using LitMotion 검증 (마이그레이션 후 최소 1 file 이상 import)
```

## 4. Sprint 분해

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | LitMotion 패키지 + Burst/Collections/Mathematics 설치 + LMotionExtensions wrapper (~70L) | 1 | 30분 |
| 2 | 핫 패스 — EnemyController PunchRoutine + FlashRoutine 마이그레이션 | 1 | 40분 |
| 3 | 핫 패스 — CinematicEffects HitPause + Shake + Zoom 마이그레이션 | 1 | 40분 |
| 4 | 핫 패스 — DamageNumberFloater Lerp → LMotion + Boss attacks 5종 | 1 | 50분 |
| 5 | UI 영역 — FadeOverlay + WaveTransitionToast + HUDController + MythicEffectOverlay | 1 | 45분 |
| 6 | Validator 룰 + NUnit + GCAllocMeasurer 재측정 + docs | 1 | 35분 |
| **총** | **22 파일 + 6 commit** | **6** | **약 4시간** |

## 5. CLAUDE.md §15 정합

| 정책 | 검증 |
|---|---|
| §15.1 SRP | LMotionExtensions = 단일 책임 (wrapper) ✅ |
| §15.4 SO 우선 | duration/strength 등 magic number → 기존 SO (BalanceConfig / CinematicEffectsConfig) 활용 ✅ |
| §15.6 100L | LMotionExtensions ~70L ✅ |
| §15.7 DI | 정적 헬퍼 + extension method ✅ |
| §15.10 외부 라이브러리 | LitMotion MIT — CLAUDE.md §10 추가 예정 ✅ |
| §15.11 GC 0 | LitMotion 의 핵심 가치 — 0 alloc 보장 ✅ |
| §15.12 Validator | LitMotion_Migration 룰 신규 ✅ |

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Unity 6.3 LTS 호환성 미명시 | Sprint 1 후 컴파일 검증 (5분) — 실패 시 5015c 재정의 |
| Burst 의존성 (~10MB+) App 용량 | App 용량 임계 150MB 영향 — Burst pre-built 활용, Player 빌드 시 측정 |
| 시각 회귀 (Lerp curve 차이) | 각 Sprint Play Mode 검증 + Ease.Linear 일치 |
| Coroutine 호환성 (yield return) | LitMotion `ToYieldInteraction()` API 활용 |
| LMotionExtensions wrapper 학습 곡선 | DOTween-like API 답습 (DOFade / DOMove / DOPunchScale) |
| 시나리오 C (> 50 KB/s) 측정 GC 의 LitMotion 후 변동 검증 X | Sprint 6 GCAllocMeasurer 재측정 — 0 KB/s 또는 <10 KB/s 확인 필수 |
| BossEncounterRunner / WaveManager Coroutine 회귀 | 영역별 Sprint 분리 + Play Mode wave 진행 검증 |

## 7. 변경 이력

- 2026-05-05: v1 작성 (5015 RESERVED 후속 — 측정 결과 시나리오 C 기반).

## 8. Sources

- [LitMotion GitHub](https://github.com/annulusgames/LitMotion)
- [LitMotion Installation](https://annulusgames.github.io/LitMotion/articles/en/installation.html)
- [LitMotion Design Philosophy](https://annulusgames.github.io/LitMotion/articles/en/design-philosophy.html)
- [TweenPerformance Benchmark](https://github.com/AnnulusGames/TweenPerformance)
- [5015b 사전조사 보고서](./5015b_premigration_research.md)
