# T04: Civ CH 21~27 42 PNG

> **레이어**: Asset
> **의존성**: T03 완료
> **예상 시간**: 40~55분 + review

## 1. 대상

`Generated_v2/Enemies/Chapters/CH{21~27}_*.png` × 7 × 6 = **42 PNG**

## 2. 시드 매핑

| Chapter | 시드 base |
|---|---:|
| CH 21 | 55555 + 20*6 = 55675 |
| CH 22 | 55681 |
| CH 23 | 55687 |
| CH 24 | 55693 |
| CH 25 | 55699 |
| CH 26 | 55705 |
| CH 27 | 55711 |

## 3. Prompt

#### Civ 카테고리 prefix
```
friendly origami construct, cool blue and gray tones, paper craft atmosphere
```

| Archetype | Form |
|---|---|
| Small | `cute paper crane, folded wings, simple geometric shapes` |
| Medium | `cardboard robot toy, blocky charm, bright button eyes` |
| Large | `origami giant, large folded paper figure, sturdy posture` |
| MiniA | `paper knight, helmet and shield made of folded sheets, valiant` |
| MiniB | `cardboard wizard, hat and robe of crafted paper, intricate folds` |
| Boss | `pastel mech, gentle giant of crafted blocks, friendly LED eyes` |

## 4. commit 메시지

```
5009 Sprint 4 / T04 — Civ CH 21~27 chapter override 42 PNG (4/6)

[변경]
- Generated_v2/Enemies/Chapters/CH{21~27}_*.png × 42
- 시드 55555 + (CH-1)*6 (CH 21=55675 / CH 27=55711)
- prompt: friendly origami construct 6 archetype × 7 chapters

[검증]
- 42 PNG transparent 30~80%, asym ≤ 50
- 톤 일관 (cool blue/gray paper craft)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```
