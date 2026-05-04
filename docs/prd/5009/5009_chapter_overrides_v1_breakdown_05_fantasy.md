# T05: Fantasy CH 28~35 48 PNG

> **레이어**: Asset
> **의존성**: T04 완료
> **예상 시간**: 45~65분 + review (가장 큰 batch)

## 1. 대상

`Generated_v2/Enemies/Chapters/CH{28~35}_*.png` × 8 × 6 = **48 PNG**

## 2. 시드 매핑

| Chapter | 시드 base |
|---|---:|
| CH 28 | 77777 + 27*6 = 77939 |
| CH 29 | 77945 |
| CH 30 | 77951 |
| CH 31 | 77957 |
| CH 32 | 77963 |
| CH 33 | 77969 |
| CH 34 | 77975 |
| CH 35 | 77981 |

## 3. Prompt (5008 사용자 보고 후 단순화 prefix)

#### Fantasy 카테고리 prefix
```
friendly fairy tale character, soft lavender and pastel purple tones, simple cute magical features
```

(5008 사용자 보고 "Fantasy 단어 너무 몰입, 통일성 부족" 후 단순화 prefix — abstract 회피)

| Archetype | Form |
|---|---|
| Small | `tiny friendly fairy, simple wings, sparkles, cute eyes` |
| Medium | `apprentice wizard, simple star hat, cheerful expression` |
| Large | `crystal creature, simple gem body, gentle glow, kind eyes` |
| MiniA | `young witch, simple broom and pointy hat, friendly smile` |
| MiniB | `elder wizard, soft beard, simple staff, wise gentle eyes` |
| Boss | `wizard king, regal robes with simple star pattern, kind majestic presence` |

## 4. commit 메시지

```
5009 Sprint 5 / T05 — Fantasy CH 28~35 chapter override 48 PNG (5/6)

[변경]
- Generated_v2/Enemies/Chapters/CH{28~35}_*.png × 48
- 시드 77777 + (CH-1)*6 (CH 28=77939 / CH 35=77981)
- prompt: friendly fairy tale character 6 archetype × 8 chapters
- 5008 사용자 보고 단순화 prefix 적용 (abstract 회피)

[검증]
- 48 PNG transparent 30~80%, asym ≤ 50
- 톤 일관 (soft lavender/pastel purple)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```
