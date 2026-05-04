# T04 — Nature + Animal (CH 6~20) 12 archetype 재생성 + 사용자 review

> **레이어**: 자산 (Generated_v2/Enemies)
> **의존성**: T03 PASS
> **예상 시간**: 45~75분 / 1 commit (review 사이클 포함)
> **PRD 출처**: `5008_enemy_visual_pipeline_v2_v2.md` §2 Sprint 4

## 1. 작업 항목

### 1.1 자산 재생성

**대상**:
- `Enemy_Nature_{Small,Medium,Large,MiniA,MiniB,Boss}.png` × 6 (시드 33333)
- `Enemy_Animal_{Small,Medium,Large,MiniA,MiniB,Boss}.png` × 6 (시드 44444)
- 합계 12 PNG

**프로세스**: T03 와 동일 — comfyui-image + LayerDiffuse + 카테고리 prompt + 시드. Nature/Animal 별도 batch (각 6 sprite).

**카테고리 톤**:
- **Nature** (CH 6~14): 녹색 + 갈색 + 잎사귀 모티프, "forest nature spirit"
- **Animal** (CH 15~20): 따뜻 오렌지 + 흰색, "cute domestic animal companion"

### 1.2 사용자 review

T03 와 동일 사이클 — 12 sprite batch 한 번에 review. skip 시 D3=A+B 정책.

## 2. 검증 기준

### 2.1 자산
- 12 PNG 모두 alpha transparent 30~70%
- Nature 6 PNG: 녹색·갈색 톤 일관
- Animal 6 PNG: 따뜻 오렌지 톤 일관
- Nature vs Animal 톤 차이 명확 (S2 검증 사전)

### 2.2 사용자 review
- "OK" → T05 진입
- skip 시 → T06 docs skip list (D3=A+B)

### 2.3 SaveData / EnemyData
- CH 6 ~ CH 20 Play Mode 진입 시 적 sprite 정상 표시

## 3. commit 메시지

```
5008 Sprint 4 / T04 — Nature + Animal 12 archetype 재생성 (4/6)

[변경]
- Enemy_Nature_*.png × 6 (시드 33333, forest spirit 톤)
- Enemy_Animal_*.png × 6 (시드 44444, warm animal 톤)
- LayerDiffuse + 카테고리별 prompt prefix

[검증]
- 12 PNG transparent 30~70%
- 카테고리 톤 일관 (Nature 녹색·갈색 / Animal 따뜻 동물)
- Nature vs Animal 톤 차이 명확
- 사용자 review PASS (또는 skip → D3=A+B 정책)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 4
- 디자인 §3.3 카테고리별 prompt template (Nature, Animal)
- decisions D3=A+B (skip 후속 정책)

## 5. 알려진 위험

- **Nature vs Animal 톤 혼동** → 시드 분리 (33333 vs 44444) + prompt prefix 차별화
- **12 sprite review 부담 ↑ vs T03 (6 sprite)** → 한 번에 12 review, 사용자 자율 분할 가능
- **iter cap 3 도달 시 일부 skip** → D3=A+B 적용
