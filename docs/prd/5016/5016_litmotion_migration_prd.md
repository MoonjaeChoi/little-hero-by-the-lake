# 5016 — LitMotion 마이그레이션 — Sprint PRD

> **출처**: `docs/prd/5016_litmotion_migration_prd.md` (디자인) — 옵션 A (22 파일 일괄)
> **사전조사**: `docs/prd/5015b_premigration_research.md`
> **측정 결과**: GCAllocMeasurer 시나리오 C (> 50 KB/s) — 전체 마이그레이션 정당화

## 1. 전체 개요

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | LitMotion 패키지 + 의존성 3종 설치 + LMotionExtensions wrapper | 1 | 30분 |
| 2 | EnemyController PunchRoutine + FlashRoutine | 1 | 40분 |
| 3 | CinematicEffects HitPause + Shake + Zoom | 1 | 40분 |
| 4 | DamageNumberFloater + Boss attacks 5종 (Vine/Tornado/Summon/Spread/Pulse) | 1 | 50분 |
| 5 | UI 영역 — FadeOverlay + WaveTransitionToast + HUDController + MythicEffectOverlay | 1 | 45분 |
| 6 | Validator 룰 + NUnit + GCAllocMeasurer 재측정 + docs | 1 | 35분 |
| **총** | **22 파일 + 6 commit** | **6** | **약 4시간** |

> Sprint 2~5 = 핫 패스 + UI 영역 마이그레이션. Sprint 1 인프라 + Sprint 6 검증/docs.

## 2. Sprint 명세

### Sprint 1 — LitMotion 인프라

#### 1.1 패키지 설치

**파일**: `LittleHero/Packages/manifest.json`

**산출물**:
```json
"com.annulusgames.lit-motion": "https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion",
"com.unity.burst": "1.6.0",
"com.unity.collections": "1.5.1",
"com.unity.mathematics": "1.0.0"
```

**검증 기준**:
- Package Manager 에서 LitMotion v2.0.1 인식
- 컴파일 errors=0
- `using LitMotion;` 임포트 가능
- Burst Compiler 동작 (Unity Editor 컴파일 후 인지)

#### 1.2 LMotionExtensions wrapper

**파일**: `LittleHero/Assets/_Project/Scripts/Core/LMotionExtensions.cs` (신규, ~70L)

**산출물**:
```csharp
public static class LMotionExtensions
{
    // SpriteRenderer alpha
    public static MotionHandle DOFadeAlpha(this SpriteRenderer sr, float endAlpha, float duration);
    // Image alpha
    public static MotionHandle DOFadeAlpha(this Image img, float endAlpha, float duration);
    // CanvasGroup alpha
    public static MotionHandle DOFade(this CanvasGroup cg, float endValue, float duration);
    // Transform punch scale
    public static MotionHandle DOPunchScale(this Transform tr, float strength, float duration);
    // Transform position
    public static MotionHandle DOMove(this Transform tr, Vector3 endValue, float duration);
    // Image fillAmount
    public static MotionHandle DOFillAmount(this Image img, float endValue, float duration);
}
```

**검증 기준**:
- 컴파일 errors=0
- §15.6 100L 통과 (~70L)
- 6 extension methods 모두 MotionHandle 반환

#### Sprint 1 commit
```
5016 Sprint 1 / T01 — LitMotion 패키지 + LMotionExtensions wrapper (1/6)

[변경]
- Packages/manifest.json — com.annulusgames.lit-motion + Burst 1.6 + Collections 1.5.1 + Mathematics 1.0
- Scripts/Core/LMotionExtensions.cs 신규 (~70L)
- 6 extension: DOFadeAlpha (SpriteRenderer/Image) / DOFade (CanvasGroup) / DOPunchScale / DOMove / DOFillAmount

[검증]
- 컴파일 errors=0
- §15.6 100L 통과
- using LitMotion; 임포트 가능

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 2 — EnemyController 핫 패스

#### 2.1 PunchRoutine 마이그레이션

**파일**: `Scripts/Gameplay/Enemies/EnemyController.cs` 수정

**변경 전** (line 151~169):
```csharp
private IEnumerator PunchRoutine(float targetScale, float expandTime, float returnTime) {
    float t = 0f;
    while (t < expandTime) {
        t += Time.unscaledDeltaTime;
        _punchScale = Mathf.Lerp(1f, targetScale, t / expandTime);
        yield return null;
    }
    ...
}
```

**변경 후**:
```csharp
private MotionHandle _punchHandle;
private void DoPunch(float targetScale, float expandTime, float returnTime) {
    if (_punchHandle.IsActive()) _punchHandle.Cancel();
    _punchHandle = LSequence.Create()
        .Append(LMotion.Create(1f, targetScale, expandTime).WithIgnoreTimeScale(true).Bind(s => _punchScale = s))
        .Append(LMotion.Create(targetScale, 1f, returnTime).WithIgnoreTimeScale(true).Bind(s => _punchScale = s))
        .Run();
}
```

#### 2.2 FlashRoutine 마이그레이션

**변경 전** (line 171~186):
```csharp
private IEnumerator FlashRoutine(Color flashColor, float holdTime, float returnTime) {
    if (_sr == null) yield break;
    _sr.color = flashColor;
    yield return new WaitForSecondsRealtime(holdTime);
    float t = 0f;
    while (t < returnTime) { ... }
}
```

**변경 후**:
```csharp
private MotionHandle _flashHandle;
private void DoFlash(Color flashColor, float holdTime, float returnTime) {
    if (_sr == null) return;
    if (_flashHandle.IsActive()) _flashHandle.Cancel();
    _sr.color = flashColor;
    _flashHandle = LMotion.Create(0f, 1f, returnTime)
        .WithDelay(holdTime)
        .WithIgnoreTimeScale(true)
        .Bind(k => _sr.color = Color.Lerp(flashColor, _baseColor, k));
}
```

#### 2.3 OnHit 호출 갱신

`StartCoroutine(_punchCo)` / `StartCoroutine(_flashCo)` → `DoPunch(...)` / `DoFlash(...)` 직접 호출.

**검증 기준**:
- 컴파일 errors=0
- Play Mode 적 hit 시 punch scale + flash 색상 시각 동일
- _punchScale 1f 복귀 정상
- flash 후 _baseColor 복귀 정상 (5012 D3=B 정합)

#### Sprint 2 commit
```
5016 Sprint 2 / T02 — EnemyController PunchRoutine + FlashRoutine 마이그레이션 (2/6)

[변경]
- EnemyController.PunchRoutine → DoPunch (LSequence.Append × 2)
- EnemyController.FlashRoutine → DoFlash (LMotion + WithDelay)
- StartCoroutine 폐기 — MotionHandle 보유
- 5012 D3=B (_baseColor 복귀) 정합

[검증]
- Play Mode hit 시각 동일 (punch + flash)
- IEnumerator 2 → MotionHandle 2 (state machine alloc 0)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 3 — CinematicEffects 핫 패스

#### 3.1 HitPauseRoutine

```csharp
// 변경 전 (line 40~47)
private IEnumerator HitPauseRoutine(float duration) { ... }

// 변경 후
private MotionHandle _hitPauseHandle;
public void PlayHitPause(float duration) {
    if (_hitPauseHandle.IsActive()) _hitPauseHandle.Cancel();
    float prevScale = Time.timeScale > 0.06f ? Time.timeScale : 1f;
    Time.timeScale = 0.05f;
    _hitPauseHandle = LMotion.Create(0f, 1f, duration)
        .WithIgnoreTimeScale(true)
        .WithOnComplete(() => Time.timeScale = prevScale)
        .RunWithoutBinding();
}
```

#### 3.2 ShakeRoutine (new Vector3 alloc 회피)

```csharp
// 변경 전 (line 56~71) — 매 frame new Vector3 alloc
private IEnumerator ShakeRoutine(float amplitude, float duration) { ... }

// 변경 후 (LMotion.Shake — 0 alloc)
private MotionHandle _shakeHandle;
public void PlayScreenShake(float amplitude, float duration) {
    var cam = Camera.main;
    if (cam == null) return;
    var origin = cam.transform.localPosition;
    if (_shakeHandle.IsActive()) _shakeHandle.Cancel();
    _shakeHandle = LMotion.Shake.Create(Vector3.zero, Vector3.one * amplitude, duration)
        .Bind(offset => cam.transform.localPosition = origin + offset);
}
```

#### 3.3 ZoomRoutine

```csharp
// 변경 전 (line 86~) — Coroutine 기반 zoom in/out
// 변경 후 — LSequence 2 단계 (zoom in + zoom out)
private MotionHandle _zoomHandle;
public void PlayCameraZoom(float zoomMul, float upDuration, float downDuration) {
    var cam = Camera.main;
    if (cam == null) return;
    float origSize = cam.orthographicSize;
    float zoomedSize = origSize / zoomMul;
    if (_zoomHandle.IsActive()) _zoomHandle.Cancel();
    _zoomHandle = LSequence.Create()
        .Append(LMotion.Create(origSize, zoomedSize, upDuration).WithIgnoreTimeScale(true).Bind(s => cam.orthographicSize = s))
        .Append(LMotion.Create(zoomedSize, origSize, downDuration).WithIgnoreTimeScale(true).Bind(s => cam.orthographicSize = s))
        .Run();
}
```

**검증 기준**:
- HitPause timeScale 정상 복귀 (1.0)
- Screen shake 시각 동일
- Camera zoom in/out 정상

#### Sprint 3 commit
```
5016 Sprint 3 / T03 — CinematicEffects HitPause + Shake + Zoom 마이그레이션 (3/6)

[변경]
- HitPauseRoutine → PlayHitPause (LMotion + WithOnComplete timeScale 복원)
- ShakeRoutine → PlayScreenShake (LMotion.Shake — new Vector3 alloc 회피)
- ZoomRoutine → PlayCameraZoom (LSequence 2단)

[검증]
- HitPause timeScale 1.0 복귀 정상
- Shake new Vector3/frame alloc 제거 (cap 300 환경 GC 핫스팟 ↓)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 4 — DamageNumberFloater + Boss attacks

#### 4.1 DamageNumberFloater

**파일**: `Scripts/Gameplay/Combat/DamageNumberFloater.cs`

- `Update()` 의 Lerp 처리 → `LMotion.Create(start, end, duration).Bind(...)`
- `new Vector3` (line 60-62) — start/end 1회만 계산 → Bind 내 활용
- alpha fade — 별도 LMotion

#### 4.2 Boss attacks 5종

**파일**:
- `Scripts/Gameplay/Enemies/BossAttacks/BossVineAttack.cs`
- `BossTornadoAttack.cs` / `BossSummonAttack.cs` / `BossSpreadAttack.cs` / `BossPulseAttack.cs`

각 파일 내 `Lerp` 사용처 → LMotion 전환. 다만 사용 빈도 낮음 (보스 공격 시만) — Sprint 4 묶음.

**검증 기준**:
- Floater 시각 동일 (위로 떠오르며 fade)
- Boss 공격 패턴 시각 동일 (vine 자라남 / tornado 회전 / spread / pulse)

#### Sprint 4 commit
```
5016 Sprint 4 / T04 — DamageNumberFloater + Boss attacks 5종 마이그레이션 (4/6)

[변경]
- DamageNumberFloater Lerp + new Vector3 → LMotion.Create + Bind
- Boss attacks 5종 (Vine/Tornado/Summon/Spread/Pulse) Lerp → LMotion

[검증]
- Floater 시각 동일 (위로 떠오르며 fade)
- Boss 공격 패턴 시각 회귀 X

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 5 — UI 영역

#### 5.1 FadeOverlay

**변경**:
```csharp
public IEnumerator FadeOut(float duration) → public MotionHandle FadeOut(float duration)
```

`new Color` × N frame → LMotion 1 alloc.

#### 5.2 WaveTransitionToast (사용자 baseline 가독성 강화 정합)

- Time.unscaledDeltaTime 기반 카운트다운 → LMotion remaining 0→1 + WithIgnoreTimeScale(true) + Bind
- progress dots 갱신 — Bind 내부에서 처리

#### 5.3 HUDController + MythicEffectOverlay

- Coroutine fade/scale → LMotion

**검증 기준**:
- Scene 전환 fade 시각 동일
- Wave 전환 토스트 카운트다운 + dots 정상
- Mythic 발현 overlay 시각 동일

#### Sprint 5 commit
```
5016 Sprint 5 / T05 — UI 영역 (FadeOverlay + WaveTransitionToast + HUDController + MythicEffectOverlay) (5/6)

[변경]
- FadeOverlay FadeIn/FadeOut Coroutine → LMotion
- WaveTransitionToast ShowCountdown → LMotion + progress dots Bind
- HUDController fade/scale → LMotion
- MythicEffectOverlay Coroutine 2 → LMotion 2

[검증]
- Scene 전환 fade 시각 동일
- Wave 전환 카운트다운 + dots 정상
- Mythic overlay 시각 동일

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 6 — Validator + NUnit + 재측정 + docs

#### 6.1 ProjectValidator 룰

**파일**: `Scripts/Editor/ProjectValidator.cs` 추가

```csharp
private static void ValidateLitMotionMigration(List<Issue> issues) {
    string scriptDir = Path.Combine(Application.dataPath, "_Project/Scripts");
    int doTweenViolations = 0;
    int litMotionImports = 0;
    foreach (var file in Directory.GetFiles(scriptDir, "*.cs", SearchOption.AllDirectories)) {
        string content = File.ReadAllText(file);
        if (content.Contains("using DG.Tweening")) doTweenViolations++;
        if (content.Contains("using LitMotion")) litMotionImports++;
    }
    if (doTweenViolations > 0)
        issues.Add(new Issue(Severity.Warning, "5016 LitMotion", $"DOTween 재도입 회귀: {doTweenViolations} files"));
    if (litMotionImports == 0)
        issues.Add(new Issue(Severity.Warning, "5016 LitMotion", "LitMotion import 0 — 마이그레이션 미작동"));
    else
        Debug.Log($"[ProjectValidator] 5016 LitMotion OK — {litMotionImports} files imported");
}
```

#### 6.2 NUnit Test

**파일**: `Tests/Editor/LMotionExtensionsTests.cs` 신규 (5+ NUnit)

- `DOFadeAlpha_SpriteRenderer_ReturnsActiveHandle`
- `DOFadeAlpha_Image_ReturnsActiveHandle`
- `DOFade_CanvasGroup_ReturnsActiveHandle`
- `DOPunchScale_Transform_ReturnsActiveHandle`
- `DOMove_Transform_ReturnsActiveHandle`
- `Type_Exists_StaticClass`

#### 6.3 GCAllocMeasurer 재측정 (사용자)

Tools/LittleHero/Measure GC Alloc (60s) 재실행:
- 시나리오 A (< 30 KB/s) 도달 → 5016 성공
- 시나리오 B (30~50 KB/s) → 추가 fix 사이클
- 시나리오 C (> 50 KB/s) → 회귀 (불가능, 마이그레이션 후)

#### 6.4 docs

- `docs/little_hero/changelog_v1.md` 5016 항목
- CLAUDE.md §10 외부 라이브러리 — LitMotion MIT 행 추가
- §11/§12/§14 갱신

#### Sprint 6 commit
```
5016 Sprint 6 / T06 — Validator + NUnit + 재측정 + docs (6/6) — 5016 종결

[변경]
- ProjectValidator ValidateLitMotionMigration 룰 — DOTween 회귀 + LitMotion import 검사
- Tests/Editor/LMotionExtensionsTests.cs 신규 (6 NUnit)
- changelog_v1.md 5016 항목
- CLAUDE.md §10 LitMotion MIT 행 추가 + §11 마일스톤 + §12 시스템 현황 + §14 직전 5건 회전

[5016 누적 6 commit]
- T01 LitMotion 패키지 + LMotionExtensions wrapper
- T02 EnemyController 핫 패스 (PunchRoutine + FlashRoutine)
- T03 CinematicEffects 핫 패스 (HitPause + Shake + Zoom)
- T04 DamageNumberFloater + Boss attacks 5종
- T05 UI 영역 (FadeOverlay + WaveTransitionToast + HUDController + MythicEffectOverlay)
- T06 Validator + Tests + docs

[검증 — 사용자]
- GCAllocMeasurer 재측정 시나리오 A (< 30 KB/s) 도달
- Play Mode 시각 회귀 X (모든 fade/scale/shake/zoom)
- 컴파일 errors=0
- NUnit 6/6 PASS

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
Sprint 1 (인프라 + wrapper) ──┐
                              ├→ Sprint 2 (EnemyController) ──┐
                              ├→ Sprint 3 (CinematicEffects) ─┤
                              ├→ Sprint 4 (Floater + Boss) ───┼→ Sprint 6 (Validator + Test + docs)
                              └→ Sprint 5 (UI) ────────────────┘
```

Sprint 2~5 = 영역 독립 (병렬 가능). 다만 컴파일 통합 + 회귀 검증 위해 순차 권장.

## 4. Definition of Done

- [ ] 6 commit + 컴파일 errors=0 (각 Sprint 별)
- [ ] LitMotion v2.0.1 + Burst 1.6 + Collections 1.5.1 + Mathematics 1.0 설치
- [ ] LMotionExtensions 6 extension methods + ~70L
- [ ] 22 파일 마이그레이션 완료 (Coroutine + Lerp 패턴 → MotionHandle)
- [ ] DOTween 재도입 0 (Validator 검증)
- [ ] LitMotion import 22+ files (Validator 검증)
- [ ] NUnit 6/6 PASS (LMotionExtensionsTests, 누적 100)
- [ ] GCAllocMeasurer 재측정 시나리오 A (< 30 KB/s) 도달
- [ ] Play Mode 시각 회귀 X (사용자 검증)
- [ ] CLAUDE.md §10 LitMotion 추가

## 5. 검증 시나리오 (Play 검증)

### S1 — EnemyController 핫 패스 (Sprint 2 후)
1. CH 1 wave 1 진입 → 적 hit
2. punch scale 1.4× (크리 1.7×) + flash 색상 적용 확인
3. flash 종료 후 _baseColor 복귀 확인 (5012.1 hue 색상 유지)

### S2 — CinematicEffects (Sprint 3 후)
1. Critical hit 발생 (5% baseline)
2. HitPause (timeScale 0.05 × 0.08s) → 1.0 복귀
3. Screen shake amplitude 정상
4. Camera zoom in/out 정상

### S3 — DamageNumberFloater (Sprint 4 후)
1. 적 hit 시 데미지 숫자 floater 출현
2. 위로 떠오르며 fade 정상
3. 1.2K / 15K / 1.5M 압축 표시

### S4 — UI 영역 (Sprint 5 후)
1. Scene 전환 fade in/out 정상
2. Wave 1 → wave 2 토스트 카운트다운 + progress dots 정상
3. Mythic 발현 시 overlay flash 정상

### S5 — GC 재측정 (Sprint 6 후)
1. Tools/LittleHero/Measure GC Alloc (60s) 실행
2. 60초 측정 후 콘솔 결과
3. **시나리오 A (< 30 KB/s)** 도달 확인 → 5016 성공

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Sprint 1 LitMotion 패키지 설치 실패 (Unity 6.3 호환 X) | 5015c 신규 PRD — DOTween 또는 UniTask 기반 재정의 |
| Burst 의존성 추가 — App 용량 ↑ | Sprint 6 빌드 후 측정 (150MB 임계 내 확인) |
| Sprint 2 EnemyController hit 시각 회귀 | Play Mode S1 검증 — punch scale 1.4× 정확 |
| Sprint 3 HitPause timeScale 미복귀 | WithOnComplete 콜백 검증 + GameManager.timeScale 모니터링 |
| Sprint 4 Boss attacks Lerp 시각 차이 | Ease.Linear 일치 + Play Mode 검증 |
| Sprint 5 Coroutine 호환 (yield return) | LitMotion `ToYieldInteraction()` API 활용 (Sprint 5 일부) |
| Sprint 6 GC 재측정 시 시나리오 B/C (회귀) | 핫 패스 누락 fix 사이클 (5016.1 추가 commit) |

## 7. 변경 이력

- 2026-05-05: 초기 작성 (5015 RESERVED 후속, 사용자 측정 시나리오 C 기반).
