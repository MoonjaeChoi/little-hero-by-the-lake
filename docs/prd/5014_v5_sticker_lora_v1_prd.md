# 5014 — V5 sticker LoRA 훈련 v1 (디자인)

> **목적**: V5 sticker dynamic 톤을 LoRA 로 학습 → SDXL base + V5_LoRA 로 생성 시 톤 일관성 ↑ + prompt 부담 ↓.
> **우선순위**: 자동화 영역 (PRD 외부 작업, 서버 + Kohya_ss).

## 0. 결정 이력

- 2026-05-04: 5008 종결 시 후속 anchor 등록 (이전 5011 → 재부여 5014)

## 1. 현재 시스템 분석

### 1.1 V5 sticker tone 현황

- Generated_v2/Enemies 30 archetype + V1 11 + Daily 7 + Weekly 4 + UI 14 = **66 sprite** 가 V5 톤
- 5008 sprite_generation_rules baseline 적용 — 일관성 ↑ but prompt 23+ 토큰 매번 적용 (long prompt)

### 1.2 LoRA 학습 효과

- LoRA 모델 (~150MB) 학습 시 V5 톤 = SDXL 가중치에 합성
- 사용 시: `--lora v5_sticker.safetensors:0.7` 정도 weight + 짧은 prompt
- prompt 부담 ↓ ("V5 sticker tone" 한 토큰으로 충분)
- 톤 일관성 ↑ (학습 sample 의 평균)

### 1.3 학습 환경

- 서버: 192.168.75.194 (RTX 5070 12GB) — Kohya_ss 또는 sd-scripts
- 학습 데이터: 66 sprite + 캡션 (5008 의 prompt 재사용)
- 학습 시간 — RTX 5070 12GB 기준 추정 **2~4시간** (1000 step / batch 1)

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | Kohya_ss SDXL LoRA (UI 친화) | 검증된 워크플로 | UI 의존 |
| **B (★권장)** | **sd-scripts CLI 학습 (스크립트 자동화)** | 재현 가능, 자동화 | CLI 익숙도 필요 |
| C | LyCORIS / DoRA 변형 | 더 작은 모델 | 호환성 검증 필요 |

> 권장 **B** — 재현 + 자동화.

## 3. 권장 설계

### 3.1 학습 파이프라인

1. 학습 데이터 정리 (66 sprite + caption) → `~/lora_training/v5_sticker/dataset/`
2. Kohya/sd-scripts 학습 config (SDXL base 1.0, batch 1, 1000 step, learning rate 1e-4)
3. 학습 → `~/lora_training/v5_sticker/output/v5_sticker_v1.safetensors` (~150MB)
4. ComfyUI custom_nodes/ComfyUI-layerdiffuse 에 LoraLoader 노드 추가
5. workflow_layerdiffuse.json 에 LoraLoader 노드 wiring + lora_name="v5_sticker_v1"

### 3.2 신규 파일 (스킬 추가)

- `.claude/skills/comfyui-image/assets/workflow_layerdiffuse_lora.json` — LoraLoader 추가 워크플로
- `.claude/skills/comfyui-image/references/lora_training_guide.md` — 학습 절차 + caption 형식

### 3.3 sprite_generation_rules.md 갱신

- §1 검증 환경 — LoRA 사용 시 prompt 단순화 (V5 sticker tone, character ...)
- LoRA weight 0.7 권장 (over-fit 방지)

## 4. Sprint 분해

| Sprint | 작업 | commit |
|---|---|---|
| 1 | 학습 데이터 정리 + caption 작성 (66 sprite) | 1 |
| 2 | Kohya_ss / sd-scripts 학습 config + 학습 실행 | 1 (서버) |
| 3 | LoRA 결과 검증 (10 sample 생성, V5 톤 비교) | 1 |
| 4 | ComfyUI workflow_layerdiffuse_lora.json 신규 + sprite_generation_rules 갱신 | 1 |
| 5 | docs (lora_training_guide.md + changelog) | 1 |
| **총** | | **5** |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 학습 데이터 부족 (66 sprite) | augmentation (좌우 반전 + crop) → 200+ sample |
| Over-fit (특정 캐릭터에 강함) | LoRA weight 0.7 default + 다양한 caption |
| Kohya_ss 의 ComfyUI 통합 | LoraLoader 노드 = 표준 ComfyUI, 호환성 ✓ |
| RTX 5070 12GB 학습 시간 | 1000 step 충분, 추가 시 fine-tune |

## 6. 변경 이력

- 2026-05-04: v1 작성.
