# T02: Nature CH 6~14 54 PNG

> **레이어**: Asset
> **의존성**: T01 완료 + 사용자 review PASS
> **예상 시간**: 50~70분 + review
> **PRD 출처**: 5009_chapter_overrides_v1_v2.md §2 Sprint 2

## 1. 작업 항목

### 1.1 대상

`Generated_v2/Enemies/Chapters/CH{06~14}_*.png` × 9 챕터 × 6 archetype = **54 PNG**

### 1.2 시드 매핑

| Chapter | 시드 base |
|---|---:|
| CH 6 | 33333 + 5*6 = 33363 |
| CH 7 | 33369 |
| CH 8 | 33375 |
| CH 9 | 33381 |
| CH 10 | 33387 |
| CH 11 | 33393 |
| CH 12 | 33399 |
| CH 13 | 33405 |
| CH 14 | 33411 |

### 1.3 Prompt 매핑

#### Nature 카테고리 prefix
```
friendly forest creature, lush green and earthy brown tones, peaceful nature atmosphere
```

#### Archetype 별
| Archetype | Form |
|---|---|
| Small | `tiny moss sprite, leafy details, dewdrops, gentle glow` |
| Medium | `friendly acorn faerie, autumn leaf wings, cute round face` |
| Large | `bramble guardian, thick vine arms, kind eyes through leafy mask` |
| MiniA | `forest watcher, owl-like features, mossy cloak, watchful` |
| MiniB | `gentle root spirit, wood and bark texture, peaceful elder posture` |
| Boss | `majestic forest king, antlers like branches, deep green presence, regal stature` |

## 2. 검증 기준

§3.6 metric (transparent 30~80%, asym ≤ 50) + 톤 일관 (forest green/brown).

## 3. commit 메시지

```
5009 Sprint 2 / T02 — Nature CH 6~14 chapter override 54 PNG (2/6)

[변경]
- Generated_v2/Enemies/Chapters/CH{06~14}_{Small,Medium,Large,MiniA,MiniB,Boss}.png × 54
- 시드 33333 + (CH-1)*6 (CH 6=33363 / CH 14=33411)
- prompt: friendly forest creature 6 archetype × 9 chapters

[검증]
- 54 PNG transparent 30~80%, asym ≤ 50
- 톤 일관 (forest green/brown)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```
