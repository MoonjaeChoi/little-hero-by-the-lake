# T01: EnemyHueVariation 정적 헬퍼

> **레이어**: Logic
> **의존성**: 없음
> **예상 시간**: 25분 / 1 commit
> **PRD 출처**: 5012_mpb_hue_shift_v1_v2.md §2 Sprint 1

## 1. 작업 항목

### 1.1 신규 파일

**경로**: `LittleHero/Assets/_Project/Scripts/Gameplay/Enemies/EnemyHueVariation.cs`

**클래스 명세** (~70L):
```csharp
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5012 — Enemy sprite 5단계 hue 변형 정적 헬퍼.
    /// 5008 LayerDiffuse RGBA sprite 30 archetype 시각 다양화 (30 → 150).
    /// SpriteRenderer.color multiplicative tint (D1=B). HSV 변환 GC 0.
    /// Boss 예외: variationIndex=0 강제 (EnemyController 가 처리).
    /// </summary>
    public static class EnemyHueVariation
    {
        public static readonly float[] OFFSETS = { -0.20f, -0.10f, 0.0f, +0.10f, +0.20f };
        public const int VARIATION_COUNT = 5;
        private const int SPREAD_PRIME = 7;

        public static Color HueShift(Color rgb, float hueOffset)
        {
            Color.RGBToHSV(rgb, out float h, out float s, out float v);
            h = (h + hueOffset + 1f) % 1f;
            var shifted = Color.HSVToRGB(h, s, v);
            shifted.a = rgb.a;
            return shifted;
        }

        public static int VariationIndex(int waveNumber, int spawnOrder)
        {
            int idx = ((waveNumber - 1) * SPREAD_PRIME + spawnOrder) % VARIATION_COUNT;
            return idx < 0 ? idx + VARIATION_COUNT : idx;
        }
    }
}
```

**예상 라인**: ~35 (100줄 이내 ✅)

## 2. 검증 기준

### 2.1 컴파일
- refresh + 5초 대기 + read_console errors=0

### 2.2 단위 검증 (T04 에서 NUnit)
- T01 단계에서는 컴파일만 검증 (NUnit 은 T04)

### 2.3 ProjectValidator
- 신규 warnings 0 (~35L 정합)

## 3. commit 메시지

```
5012 Sprint 1 / T01 — EnemyHueVariation 정적 헬퍼 (1/5)

[변경]
- Scripts/Gameplay/Enemies/EnemyHueVariation.cs 신규 (~35L)
- HueShift(Color rgb, float hueOffset) HSV 변환
- 5 단계 OFFSETS const + VariationIndex(wave, order) — 7 prime spread

[검증]
- 컴파일 errors=0
- §15.6 100L 통과 (35L)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 알려진 위험
- HSV 변환 wrap-around (음수 hueOffset) — `(h + offset + 1f) % 1f` 로 해결
- VariationIndex 음수 wave/order — `idx < 0 ? idx + COUNT : idx` 로 해결
