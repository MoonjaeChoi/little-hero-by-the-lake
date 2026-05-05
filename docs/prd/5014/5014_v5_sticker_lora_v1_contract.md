# 5014 Sprint Contract

> Phase 4 — D1=B / D2=A / D3=B 검증 기준

## C1 컴파일 — N/A (학습/자산 작업)

## C2 Editor Test — N/A

## C3 ProjectValidator — N/A (스킬/자산 외부 영역)

## C4 Reflection — N/A

## C5 자산 + 학습 산출물

- 262 caption .txt 파일 (각 PNG 와 1:1 매칭, Kohya 형식)
- `~/lora_training/v5_sticker/output/v5_sticker_v1.safetensors` (~150MB, 서버)
- `.claude/skills/comfyui-image/assets/workflow_layerdiffuse_lora.json` 신규
- `.claude/skills/comfyui-image/references/lora_training_guide.md` 신규
- `references/sprite_generation_rules.md` v2 (LoRA 모드 prompt 단순화)

## C6 Play 검증 시나리오

### S1 — LoRA 10 sample 검증
1. `workflow_layerdiffuse_lora.json` 로 5 카테고리 × 2 archetype = 10 sample 생성
2. 5008 baseline 대비 비교
- **검증**: V5 톤 outline + cartoon shading 일관 (사용자 review)

### S2 — Prompt 단순화 검증
1. 기존 5008 prompt (23+ 토큰) 와 LoRA prompt (5~10 토큰) 비교
- **검증**: 결과 동등 또는 ↑ + prompt 단순화

### S3 — Over-fit 검증
1. LoRA weight 0.7 / 0.85 / 1.0 sample
- **검증**: 0.7 = baseline 추천, 0.85 = 강한 V5 톤

## 합격 기준

- T01 caption 262/262 ✓
- T02 학습 loss 수렴 + LoRA .safetensors 생성
- T03 사용자 review V5 톤 일관성 PASS
- T04 workflow + rules 갱신
- T05 lora_training_guide.md + docs
