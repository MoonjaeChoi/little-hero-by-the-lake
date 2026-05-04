# 5014 — V5 sticker LoRA 훈련 — Sprint PRD

> **출처**: `docs/prd/5014_v5_sticker_lora_v1_prd.md` (디자인) — 옵션 B (sd-scripts CLI)
> **범위**: 66 sprite 학습 + LoRA 모델 → ComfyUI 워크플로 통합

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | 학습 데이터 + caption 정리 (66 sprite) | 1 | 60분 |
| 2 | Kohya / sd-scripts config + 학습 실행 (서버) | 1 (또는 0 commit, 서버 측) | 2~4시간 (서버) |
| 3 | LoRA 결과 검증 (10 sample) | 1 | 30분 |
| 4 | workflow_layerdiffuse_lora.json + sprite_generation_rules 갱신 | 1 | 25분 |
| 5 | lora_training_guide.md + docs | 1 | 30분 |
| **총** | | **5** | **약 3~5h (학습 시간 포함)** |

## 2. Sprint 명세

### Sprint 1 — 학습 데이터 + caption 정리

**작업**:
- 66 sprite 수집 → `~/lora_training/v5_sticker/dataset/`
- 각 sprite caption .txt 파일 작성 (Kohya 형식)
- Caption 예: `v5 sticker tone, cute cartoon, chunky thick white outline, soft cartoon shading, single character, transparent background`
- augmentation: 좌우 반전 → 132 sample

**검증**: 132 (sprite + caption) 쌍 + 폴더 구조 검증

#### Sprint 1 commit
```
5014 Sprint 1 / T01 — LoRA 학습 데이터 + caption 정리 (1/5)

[변경]
- ~/lora_training/v5_sticker/dataset/ 66 sprite + caption + augmentation 132
- Kohya 형식 caption 표준화

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 2 — 학습 실행 (서버)

**서버 작업** (commit 분리 가능):
- Kohya / sd-scripts config — SDXL base 1.0, batch 1, 1000 step, lr 1e-4
- 학습 — 2~4시간 (RTX 5070 12GB)
- 출력: `~/lora_training/v5_sticker/output/v5_sticker_v1.safetensors` (~150MB)

**검증**: 학습 log 의 loss 수렴 + 출력 파일 존재

### Sprint 3 — LoRA 결과 검증

**작업**:
- ComfyUI custom_nodes 에 LoRA 파일 사본 (또는 lora 폴더)
- workflow 임시 — 10 sample 생성 (5 카테고리 × 2 archetype)
- 5008 baseline 대비 V5 톤 일관성 비교

**검증**: 10 sample 의 V5 sticker outline + cartoon shading 일관, 사용자 review

#### Sprint 3 commit
```
5014 Sprint 3 / T03 — LoRA 결과 검증 (3/5)

[변경]
- 10 sample 생성 (5 카테고리 × 2 archetype) — V5 LoRA 적용
- v5_sticker_v1.safetensors 검증

[검증]
- V5 톤 일관성 5008 baseline 동등 또는 ↑
- 사용자 review PASS

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 4 — workflow + rules 갱신

**파일**:
- `.claude/skills/comfyui-image/assets/workflow_layerdiffuse_lora.json` 신규 (LoraLoader 노드 추가)
- `.claude/skills/comfyui-image/references/sprite_generation_rules.md` v2 — LoRA 사용 prompt 단순화 가이드

**검증**: workflow 호출 + LoRA weight 0.7 적용 확인

#### Sprint 4 commit
```
5014 Sprint 4 / T04 — ComfyUI LoRA workflow + rules 갱신 (4/5)

[변경]
- assets/workflow_layerdiffuse_lora.json 신규 (LoraLoader + lora_name="v5_sticker_v1")
- references/sprite_generation_rules.md v2 — LoRA 모드 prompt 단순화

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 5 — docs

**작업**:
- `references/lora_training_guide.md` 신규 — 학습 절차 + caption 형식 + 재학습 가이드
- changelog + CLAUDE.md §11/§12/§14

#### Sprint 5 commit
```
5014 Sprint 5 / T05 — docs (5/5)

[변경]
- references/lora_training_guide.md 신규
- changelog + CLAUDE.md
- 후속 anchor 5015 LitMotion

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
T01 (데이터) → T02 (학습 서버) → T03 (검증) → T04 (워크플로) → T05 (docs)
```

## 4. 검증 시나리오

### S1 — LoRA sample 검증
1. 10 sample 생성 (시드 고정)
2. 5008 baseline 대비
- **검증**: V5 톤 outline + shading 일관, prompt 단순화 (10토큰 → 5토큰)

### S2 — 신규 sprite 작업 적용
1. 5008.x 후속 작업 (chapter override / V1 등) 시 LoRA workflow 사용
- **검증**: prompt 부담 ↓ + 결과 동등

## 5. Definition of Done

- [ ] 학습 완료 (loss 수렴)
- [ ] 10 sample V5 톤 일관성 검증
- [ ] workflow + rules 갱신
- [ ] lora_training_guide.md 신규
- [ ] changelog + CLAUDE.md

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 학습 시간 (2~4시간) | Sprint 2 commit 별도 서버 측, 파이프라인 막힘 X |
| LoRA over-fit | weight 0.7 default, augmentation 좌우 반전 |
| Kohya / sd-scripts 학습 실패 | log 확인 후 step 1500/2000 재시도 |
| 학습 후 결과 V5 톤 약함 | weight 0.85 ~ 1.0 시도 |

## 7. 변경 이력

- 2026-05-04: v1 작성.
