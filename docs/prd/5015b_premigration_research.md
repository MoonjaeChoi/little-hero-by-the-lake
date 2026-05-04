# 5016 사전조사 — IEnumerator+Lerp → LitMotion 도입 ROI 평가

> **목적**: 5015 RESERVED 결정 (D2=A) 후 5016 신규 PRD 작성 전 사전조사 보고서.
> **일시**: 2026-05-04 · 출처: 코드 grep + 정적 분석 + Web search
> **결론**: ROI 평가 → Profiler 측정 후 결정 권장. 정적 분석 기준 마이그레이션 가치 **중간 (조건부 진행)**.

---

## 1. 현 IEnumerator + Lerp 패턴 인벤토리

### 1.1 카테고리별 분류 (Scripts/_Project/)

| 카테고리 | 파일 수 | 파일 목록 |
|---|---:|---|
| **UI** (fade / scale / floating) | 4 | FadeOverlay / HUDController / MythicEffectOverlay / WaveTransitionToast |
| **Combat/VFX** (HitEffect / Floater / Pool) | 1 | CinematicEffects |
| **Core** (SceneTransition / Audio) | 4 | SceneTransitionService / ISceneTransition / AudioManager / SmokeTest |
| **Gameplay 기타** (Wave / Boss / Enemy) | 5 | EnemyController / WaveManager / ChapterRunner / InfiniteTowerRunner / BossEncounterRunner |
| 합계 | **14** | |

추가 Lerp 직접 사용 (Coroutine 외):
| 파일 | Lerp 사용 |
|---|---:|
| BossVineAttack / BossTornadoAttack / BossSummonAttack / BossSpreadAttack / BossPulseAttack | 5 |
| PulseVisual / SimpleFx / DamageNumberFloater | 3 |

**총 22 파일 영향** (14 Coroutine + 8 Lerp 추가). 5015 PRD 의 "~50~100 호출" 보다 적음.

### 1.2 파일 별 정밀 분석 (상위 10)

| 파일 | Coroutine 개수 | Lerp 사용 | new alloc | 핫 패스 |
|---|---:|---:|---:|---|
| EnemyController.cs | 6 | 3 | 4 | 🔥 매 hit (PunchRoutine + FlashRoutine) |
| CinematicEffects.cs | 6 | 2 | 2 | 🔥 매 critical hit (HitPause + Shake + Zoom) |
| DamageNumberFloater.cs | 0 | 2 | 9 | 🔥 매 데미지 floater (cap 300 환경 빈도 ↑) |
| FadeOverlay.cs | 2 | 0 | 4 | scene 전환 |
| MythicEffectOverlay.cs | 2 | 2 | 1 | 신화 발현 |
| WaveTransitionToast.cs | 2 | 0 | 0 | wave 전환 |
| WaveManager.cs | 3 | 0 | 3 | spawn |
| SceneTransitionService.cs | 4 | 0 | 0 | scene 전환 |
| HUDController.cs | 2 | 0 | 3 | UI 갱신 |
| SimpleFx / PulseVisual | 0 | 1~2 | 1 | UI fx |

---

## 2. GC 정적 분석 — 핫스팟 발견

### 2.1 매 프레임 alloc 패턴 (위험 ↑)

`yield return null` (= 매 프레임) + `new Color/Vector3` 의 while loop 패턴:

#### FadeOverlay.FadeOut (line 23-33)
```csharp
while (t < duration)
{
    t += Time.unscaledDeltaTime;
    _blackImage.color = new Color(0, 0, 0, Mathf.Clamp01(t / duration));  // ← 매 프레임 new
    yield return null;
}
```
- 60fps × 0.5s fade = **30 frame × new Color = 30 alloc/scene 전환**
- scene 전환 매 1~5초 한 번 → 빈도 낮음 → **GC 영향 미미**

#### EnemyController.FlashRoutine (line 173-185)
```csharp
while (t < returnTime)
{
    t += Time.unscaledDeltaTime;
    float k = t / returnTime;
    _sr.color = Color.Lerp(flashColor, _baseColor, k);  // ← Color struct OK (no alloc)
    yield return null;
}
```
- Color.Lerp = struct return (no alloc) ✅
- 다만 Coroutine state machine 자체 alloc (1 enumerator instance/hit)
- cap 300 환경 + hit/sec ~200~500 시 **state machine alloc 의미**

#### CinematicEffects.ShakeRoutine (line 56-71)
```csharp
while (t < duration)
{
    ...
    t.localPosition = origin + new Vector3(ox, oy, 0f);  // ← 매 프레임 new Vector3
    yield return null;
}
```
- Vector3 = struct return (sizeof 12 byte) — JIT 시 stack alloc 가능
- 다만 += 연산자 내부 boxing 우려 → Profiler 검증 필수

### 2.2 Coroutine state machine alloc 추정

Unity 의 Coroutine 은 IEnumerator 인스턴스 1개 alloc (state machine + closure):
- **EnemyController.PunchRoutine + FlashRoutine** = 2 alloc/hit
- **DamageNumberFloater** = (Initialize 호출 시 alloc 가능)
- cap 300 환경 + hit/sec 100 시 = **200 alloc/sec** (≈ 4~8 KB/sec)

PIVOT v4 §7 GC <50KB/s 임계 — 마진 충분 (현 코드 alloc 추정 5~10 KB/sec).

### 2.3 정적 분석 결론

**현 IEnumerator+Lerp 패턴 GC 마진 충분 가능성 높음** (정적 추정 5~10KB/s, 임계 50KB/s 의 10~20%). LitMotion 도입 ROI **중간** (Profiler 측정 필수).

---

## 3. LitMotion 외부 정보

### 3.1 최신 버전 + 라이선스

| 항목 | 값 |
|---|---|
| **최신 release** | v2.0.1 (2025-02-09) |
| **라이선스** | **MIT** ✅ |
| Unity 호환 | 2021.3+ (Unity 6.3 LTS 명시 X — 이론상 호환, **실 검증 필요**) |
| URP 호환 | 영향 무관 (rendering pipeline 의존 X) |

### 3.2 의존성

| 의존 | 버전 |
|---|---|
| Burst | 1.6+ |
| Collections | 1.5.1+ |
| Mathematics | 1.0.0+ |

> **현 프로젝트 영향**: manifest.json 에 Burst/Collections/Mathematics 미설치 — **신규 추가 패키지 4종**.

### 3.3 설치 방법

```json
// Packages/manifest.json
{
  "dependencies": {
    "com.annulusgames.lit-motion": "https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion",
    "com.unity.burst": "1.6.0",
    "com.unity.collections": "1.5.1",
    "com.unity.mathematics": "1.0.0"
  }
}
```

또는 Window > Package Manager > Add package from git URL.

### 3.4 성능 비교 (벤치마크 인용)

| 방식 | 1000 tween 실행 시간 | GC alloc |
|---|---|---|
| **DOTween** | 33.25 ms (or 16ms 다른 벤치) | ~50 KB/frame |
| **Magic Tween** | ~10 ms | 적음 |
| **LitMotion v2** | **6.33 ms** (5× DOTween) | **0 GC** |
| IEnumerator+Lerp (수치 X) | 추정 ~20~30 ms | state machine 1 alloc/coroutine |

### 3.5 Coroutine 대체 API

```csharp
// 기존 IEnumerator
StartCoroutine(FadeRoutine(0.5f));

private IEnumerator FadeRoutine(float duration) {
    float t = 0f;
    while (t < duration) {
        t += Time.unscaledDeltaTime;
        _img.color = new Color(0, 0, 0, t/duration);
        yield return null;
    }
}

// LitMotion 대체 (zero alloc)
LMotion.Create(0f, 1f, 0.5f)
    .WithEase(Ease.OutQuad)
    .Bind(alpha => {
        var c = _img.color;
        c.a = alpha;
        _img.color = c;
    })
    .AddTo(gameObject);

// async/await 패턴
async UniTask FadeAsync() {
    await LMotion.Create(0f, 1f, 0.5f).BindToColorA(_img);
}

// Coroutine 호환 (점진 마이그레이션)
yield return LMotion.Create(0f, 1f, 0.5f).Bind(...).ToYieldInteraction();
```

### 3.6 데이터 지향 설계

LitMotion 은 Unity DOTS (Data-Oriented Tech Stack) 기반:
- C# Job System + Burst Compiler
- 데이터 SoA (Struct-of-Array) 배치
- 1000+ 동시 tween 시 효과 ↑↑

---

## 4. ROI 평가

### 4.1 정량 효과 (추정)

| 영역 | 현 GC 추정 | LitMotion GC | 개선 |
|---|---:|---:|---:|
| EnemyController hit (cap 300 환경) | ~5 KB/s | 0 | **5 KB/s 절감** |
| DamageNumberFloater | ~3 KB/s | 0 | **3 KB/s 절감** |
| CinematicEffects (critical hit) | ~1 KB/s | 0 | **1 KB/s 절감** |
| UI 영역 (FadeOverlay etc.) | ~0.5 KB/s | 0 | minimal |
| 합계 | **~10 KB/s** | **0** | **10 KB/s 절감** |

PIVOT v4 §7 GC <50KB/s 임계 대비:
- 현 마진: 80% (10/50 사용)
- LitMotion 후: 100% (0/50 사용)
- **절대 효과는 작음 (10KB/s 절감)**, 상대 effect 는 분명 (long-tail GC spike 회피)

### 4.2 frame time 효과

- LitMotion 5× DOTween — 다만 **현 코드는 DOTween X, IEnumerator 직접**
- IEnumerator vs LitMotion 직접 벤치 X (추정 1.5~2× 빠름)
- cap 300 환경 60fps 마진 강화

### 4.3 라이선스 효과

- DOTween 커스텀 라이선스 **회피 (현재 미사용이라 N/A)**
- LitMotion MIT — 명확 + 안전 ✅

### 4.4 비용

| 항목 | 비용 |
|---|---|
| 패키지 설치 | 5분 (manifest.json + 의존성 4종) |
| 기존 코드 마이그레이션 | **22 파일** × 평균 10분 = ~3.5h |
| 학습 곡선 | 1h (LMotion API 익히기) |
| 회귀 검증 | 2h (Play Mode 시각 검증) |
| 총 | **~6.5h** |

### 4.5 ROI 종합

| 지표 | 평가 |
|---|---|
| GC 절감 절대량 | △ 10KB/s (작음) |
| Frame time 마진 | ○ 1.5~2× 추정 |
| 라이선스 cleanup | ○ MIT 명확 |
| 학습/마이그레이션 비용 | △ 6.5h |
| 회귀 위험 | △ 시각 변동 가능 |
| 미래 확장성 | ◎ DOTS / Burst / Job System |

**판정**: **조건부 진행** — Profiler 실제 측정 후 GC > 30KB/s 확인 시 진행. 30KB/s 미만이면 IEnumerator 유지 (변경 ROI 미미).

---

## 5. 권장 측정 항목 (Profiler — 5016 진행 전 사전 작업)

### 5.1 측정 환경

- Unity Editor + Profiler attached
- iPhone 12 또는 desktop standalone build (모바일 영향 정확)
- cap 300 환경: CH 1 wave 5 (MiniBoss + 다수 적 + 처치 활발)

### 5.2 측정 메트릭

| 메트릭 | 목표 | 임계 |
|---|---|---|
| **GC Alloc/sec** | < 50KB | > 30KB → 5016 진행 |
| **CPU/frame** (Main Thread) | < 16.67ms (60fps) | > 14ms → 5016 진행 |
| **GC.Collect spike** | < 5ms | > 3ms → 5016 진행 |
| **Coroutine count** (peak) | - | > 100/sec → 5016 진행 |

### 5.3 측정 시나리오

1. Attic → CH 1 진입
2. wave 1~5 진행 (~3분)
3. wave 5 MiniBoss 처치 + 다수 hit 효과 발생
4. Profiler Memory module → GC alloc 누적 측정
5. CPU module → Main Thread frame time 분포

---

## 6. 5016 디자인 권장 방향

### 6.1 시나리오 분기

#### 시나리오 A: GC 마진 충분 (< 30KB/s)
- **5016 보류** (IEnumerator 유지)
- LitMotion 도입은 향후 추가 부담 시점에 재평가

#### 시나리오 B: GC 마진 약함 (30~50KB/s)
- **5016 부분 마이그레이션** — 핫 패스 (EnemyController + DamageNumberFloater + CinematicEffects) 만 LitMotion
- 22 파일 → 3 파일 (~1h 작업)
- IEnumerator wrapper + LitMotion 혼재 허용

#### 시나리오 C: GC 임계 초과 (> 50KB/s)
- **5016 전체 마이그레이션** — 22 파일 모두
- Sprint 분해: UI / VFX / Gameplay 영역별
- ~6.5h 작업 + 회귀 검증

### 6.2 권장 설계 (시나리오 B 기준 — 가장 가능성 높음)

#### Sprint 분해
| Sprint | 작업 | commit |
|---|---|---|
| 1 | LitMotion 패키지 + 의존성 4종 설치 + LMotionExtensions 헬퍼 (~50L) | 1 |
| 2 | EnemyController PunchRoutine + FlashRoutine 마이그레이션 | 1 |
| 3 | CinematicEffects HitPause + Shake + Zoom 마이그레이션 | 1 |
| 4 | DamageNumberFloater Lerp → LMotion | 1 |
| 5 | NUnit + ProjectValidator (LitMotion import 회귀 룰) + docs | 1 |
| **총** | **5 sprint** (시나리오 C 시 6 sprint) | |

#### 신규 헬퍼
- `Scripts/Core/LMotionExtensions.cs` (~50L) — DOTween-like wrapper
  - `MotionHandle DOFade(this CanvasGroup cg, float endValue, float duration)`
  - `MotionHandle DOMove(this Transform tr, Vector3 endValue, float duration)`
  - `MotionHandle DOPunchScale(this Transform tr, float strength, float duration)`

#### Validator 룰
- `LitMotion_NoIEnumeratorMigration` — 마이그레이션 후 IEnumerator + Lerp 패턴 재발 방지

---

## 7. 위험 + 완화

| 위험 | 완화 |
|---|---|
| Unity 6.3 LTS 호환성 미명시 | 패키지 설치 후 Editor 컴파일 검증 (5분) |
| Burst 의존성 추가 (~10MB+) | App 용량 임계 (150MB) 영향 검토 — Burst pre-built 활용 |
| 시각 회귀 (Lerp curve 차이) | Sprint 별 Play Mode 검증 + Ease.Linear 일치 |
| Coroutine 호환성 (yield return) | LitMotion `ToYieldInteraction()` API 활용 |

---

## 8. 결론 + 5016 PRD 진행 권장사항

### 8.1 결론

| 단계 | 권장 |
|---|---|
| **즉시 (사용자 액션)** | Profiler 측정 1회 (cap 300 환경 GC 메트릭) — ~10분 |
| **측정 결과 < 30KB/s** | 5016 **보류** (IEnumerator 유지) |
| **측정 결과 30~50KB/s** | **5016 부분 마이그레이션** (3 파일, 시나리오 B) |
| **측정 결과 > 50KB/s** | **5016 전체 마이그레이션** (22 파일, 시나리오 C) |

### 8.2 5016 디자인 단계 진입 조건

- Profiler 측정 결과 보고 (사용자 또는 자동)
- 결과 ≥ 30KB/s 시 → `/prd-pipeline 5016 --from generate`
- < 30KB/s 시 → 5016 작성 보류 (RESERVED 또는 archive)

### 8.3 즉시 가능한 fix (측정과 무관)

- CLAUDE.md §10 외부 라이브러리 — 이미 5015 RESERVED 사이클에서 fix 완료 ✅
- LitMotion 도입 후 §10 추가 행 (LitMotion MIT)

---

## 9. Sources

- [LitMotion GitHub](https://github.com/annulusgames/LitMotion)
- [LitMotion Installation](https://annulusgames.github.io/LitMotion/articles/en/installation.html)
- [LitMotion Design Philosophy](https://annulusgames.github.io/LitMotion/articles/en/design-philosophy.html)
- [TweenPerformance Benchmark](https://github.com/AnnulusGames/TweenPerformance)
- [LitMotion Medium Article](https://medium.com/@annulusgames/litmotion-fastest-tween-implementation-with-data-oriented-design-in-unity-07c9aa14d9dd)

## 10. 변경 이력
- 2026-05-04: 초기 작성 (사용자 요청 — 5016 신규 PRD 작성 전 사전조사).
