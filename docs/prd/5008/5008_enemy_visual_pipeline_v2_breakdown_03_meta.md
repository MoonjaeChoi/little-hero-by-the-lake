# T03 — Meta (CH 1~5) 6 archetype 재생성 + 사용자 review

> **레이어**: 자산 (Generated_v2/Enemies)
> **의존성**: T01 (LayerDiffuse 워크플로) + T02 (Auditor)
> **예상 시간**: 30~60분 / 1 commit (review 사이클 포함)
> **PRD 출처**: `5008_enemy_visual_pipeline_v2_v2.md` §2 Sprint 3

## 1. 작업 항목

### 1.1 자산 재생성

**대상**: `LittleHero/Assets/_Project/Art/Generated_v2/Enemies/Enemy_Meta_{Small,Medium,Large,MiniA,MiniB,Boss}.png` (6 PNG)

**프로세스**:

1. **comfyui-image 스킬 호출** (T01 신규 워크플로):
   ```
   /comfyui-image \
     --workflow workflow_layerdiffuse.json \
     --prompt "<Meta 카테고리 prompt — enemy_sprite_prompts_v1.md 참조>" \
     --seed 22222 \
     --output Enemy_Meta_{archetype}.png
   ```
   6번 호출 (archetype 별, 시드 22222 통일)

2. **Unity 임포트** — Asset 폴더 drop → 자동 임포트 (Sprite 2D + transparent)

3. **Auditor 검증** (선택) — `Tools > LittleHero > Audit Enemy Sprite Quality` → 6 PNG transparent 30~70% 확인

4. **사용자 review**:
   - **만족** → T04 진입 (commit)
   - **불만족** → prompt 조정 + 재생성 (iteration 2)
   - **iter 2 불만족** → 재시도 (iteration 3)
   - **iter 3 불만족** → 사용자 결정 (다른 시드 / 다른 prompt / **skip**)
   - **skip 시 (D3=A+B)**: T06 docs skip list 추가 + 5009 anchor 우선 처리 표시

### 1.2 SpriteAtlas 영향

- EnemyAtlas SpriteAtlas 가 자동 또는 수동 재빌드 — T06 commit 에서 일괄 처리

## 2. 검증 기준

### 2.1 자산
- 6 PNG 모두 alpha transparent 30~70%
- 768×768 또는 1024×1024 (sticker 사이즈 호환)
- V5 sticker tone 일관: 두꺼운 흰 outline + warm beige + village 톤

### 2.2 사용자 review
- 사용자 응답 "OK" / "재시도" / "skip" 명시
- "OK" → T04 진입
- max 3 iteration 후 PASS X 시 → skip (D3=A+B 적용)

### 2.3 SaveData / EnemyData
- EnemyData SO sprite 참조 path 그대로 유지 (sprite override 만, 메타 변경 X)
- 1라운드 (CH 1) Play Mode 진입 시 적 sprite 정상 표시 (회귀 X)

## 3. commit 메시지

```
5008 Sprint 3 / T03 — Meta 6 archetype LayerDiffuse 재생성 (3/6)

[변경]
- Art/Generated_v2/Enemies/Enemy_Meta_{Small,Medium,Large,MiniA,MiniB,Boss}.png 재생성
- LayerDiffuse SDXL native transparency
- 시드 22222 (Meta 카테고리 통일)
- prompt: enemy_sprite_prompts_v1.md Meta 카테고리

[검증]
- 6 PNG transparent 30~70%
- V5 sticker tone 일관 (warm village 톤)
- 사용자 review PASS (또는 skip 시 T06 docs 자동 enroll)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 3
- 디자인 §3.3 카테고리별 prompt template (Meta)
- decisions D3=A+B (skip 후속 정책)

## 5. 알려진 위험

- **카테고리 톤 불만족** → 시드 22222 고정 + Meta prompt prefix 표준
- **6 sprite 일괄 review 부담** → batch 단위 명확화, 1회 review
- **iter 3 후 skip** → D3=A+B 정책 적용, T06 docs + 5009 anchor 자동 enroll
- **review 시간 길어짐** → iter cap 3, 사용자 빠른 결정 권장
