# T03: Animal CH 15~20 36 PNG

> **레이어**: Asset
> **의존성**: T02 완료
> **예상 시간**: 35~50분 + review

## 1. 대상

`Generated_v2/Enemies/Chapters/CH{15~20}_*.png` × 6 × 6 = **36 PNG**

## 2. 시드 매핑

| Chapter | 시드 base |
|---|---:|
| CH 15 | 44444 + 14*6 = 44528 |
| CH 16 | 44534 |
| CH 17 | 44540 |
| CH 18 | 44546 |
| CH 19 | 44552 |
| CH 20 | 44558 |

## 3. Prompt

#### Animal 카테고리 prefix
```
friendly animal companion, warm orange and brown tones, cute pet atmosphere
```

| Archetype | Form |
|---|---|
| Small | `playful kitten, fluffy tail, big curious eyes` |
| Medium | `cheerful puppy, floppy ears, wagging tail` |
| Large | `golden retriever, friendly grin, soft golden fur` |
| MiniA | `tabby cat, watchful pose, sleek fur pattern` |
| MiniB | `loyal sheepdog, fluffy white and brown coat, gentle eyes` |
| Boss | `majestic lion, golden mane, kind regal expression` |

## 4. commit 메시지

```
5009 Sprint 3 / T03 — Animal CH 15~20 chapter override 36 PNG (3/6)

[변경]
- Generated_v2/Enemies/Chapters/CH{15~20}_*.png × 36
- 시드 44444 + (CH-1)*6 (CH 15=44528 / CH 20=44558)
- prompt: friendly animal companion 6 archetype × 6 chapters

[검증]
- 36 PNG transparent 30~80%, asym ≤ 50
- 톤 일관 (warm orange/brown)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```
