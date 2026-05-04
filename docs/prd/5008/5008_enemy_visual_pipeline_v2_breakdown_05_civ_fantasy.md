# T05 — Civ + Fantasy (CH 21~35) 12 archetype 재생성 + 사용자 review

> **레이어**: 자산 (Generated_v2/Enemies)
> **의존성**: T04 PASS
> **예상 시간**: 45~75분 / 1 commit (review 사이클 포함)
> **PRD 출처**: `5008_enemy_visual_pipeline_v2_v2.md` §2 Sprint 5

## 1. 작업 항목

### 1.1 자산 재생성

**대상**:
- `Enemy_Civ_{Small,Medium,Large,MiniA,MiniB,Boss}.png` × 6 (시드 55555)
- `Enemy_Fantasy_{Small,Medium,Large,MiniA,MiniB,Boss}.png` × 6 (시드 66666)
- 합계 12 PNG

**프로세스**: T04 와 동일.

**카테고리 톤**:
- **Civ** (CH 21~27): 핑크 + pastel + 종이/판지, "urban paper or cardboard creature"
- **Fantasy** (CH 28~35): 보라 + indigo + sparkles, "magical mystical being"

### 1.2 사용자 review

12 sprite batch 한 번에 review. skip 시 D3=A+B 정책.

## 2. 검증 기준

### 2.1 자산
- 12 PNG 모두 alpha transparent 30~70%
- Civ 6 PNG: 핑크·pastel·종이 톤 일관
- Fantasy 6 PNG: 보라·indigo·sparkle 톤 일관
- Civ vs Fantasy 톤 차이 명확

### 2.2 사용자 review
- "OK" → T05.5 (조건부 D2=C 활성화 시) 또는 T06 진입
- skip 시 → T06 docs skip list (D3=A+B)

### 2.3 SaveData / EnemyData
- CH 21 ~ CH 35 Play Mode 진입 시 적 sprite 정상 표시

## 3. commit 메시지

```
5008 Sprint 5 / T05 — Civ + Fantasy 12 archetype 재생성 (5/6)

[변경]
- Enemy_Civ_*.png × 6 (시드 55555, urban paper 톤)
- Enemy_Fantasy_*.png × 6 (시드 66666, magical sparkle 톤)
- LayerDiffuse + 카테고리별 prompt prefix

[검증]
- 12 PNG transparent 30~70%
- 카테고리 톤 일관 (Civ 핑크·인공물 / Fantasy 보라·sparkle)
- 사용자 review PASS (또는 skip → D3=A+B 정책)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 5
- 디자인 §3.3 카테고리별 prompt template (Civ, Fantasy)
- decisions D3=A+B (skip 후속 정책)

## 5. 알려진 위험

- **Civ 카테고리 정체성 약함** ("urban paper") → prompt prefix 명확화 (cardboard / pastel pink 강조)
- **Fantasy sparkle 처리** → LayerDiffuse 가 sparkle 효과 alpha 보존하는지 확인. 안 되면 prompt 조정 (sparkle 강도 ↓)
- **iter cap 3 도달 시 일부 skip** → D3=A+B 적용
