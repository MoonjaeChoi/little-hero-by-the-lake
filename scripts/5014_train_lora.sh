#!/bin/bash
# 5014 V5 sticker LoRA 학습 trigger script (서버 측 실행).
#
# 사용:
#   ssh pro301@192.168.75.194 'bash -s' < scripts/5014_train_lora.sh
#
# 또는 로컬 → 원격 SSH:
#   ssh pro301@192.168.75.194 "cd ~ && nohup bash -lc '<이 script 내용>' > ~/lora_training/v5_sticker/train.log 2>&1 &"
#
# 학습 시간 추정: 2~4h (RTX 5070 12GB, 262 PNG × 5 repeats = 1310 step, batch 1, lr 1e-4)

set -e
cd ~/sd-scripts
source venv/bin/activate

OUTPUT_DIR=/home/pro301/lora_training/v5_sticker/output
CONFIG=/home/pro301/lora_training/v5_sticker/config/v5_sticker.toml
CKPT=/home/pro301/ai-tools/ComfyUI/models/checkpoints/sd_xl_base_1.0.safetensors

mkdir -p "$OUTPUT_DIR"

echo "=== 5014 V5 sticker LoRA 학습 시작 — $(date) ==="
echo "  GPU: $(nvidia-smi --query-gpu=name,memory.total --format=csv,noheader | head -1)"
echo "  모델: $CKPT"
echo "  데이터: 262 PNG + 262 caption"
echo "  config: $CONFIG"
echo "  output: $OUTPUT_DIR/v5_sticker_v1.safetensors"

accelerate launch --num_cpu_threads_per_process=2 sdxl_train_network.py \
  --pretrained_model_name_or_path="$CKPT" \
  --dataset_config="$CONFIG" \
  --output_dir="$OUTPUT_DIR" \
  --output_name=v5_sticker_v1 \
  --save_model_as=safetensors \
  --network_module=networks.lora \
  --network_dim=32 --network_alpha=16 \
  --learning_rate=1e-4 \
  --optimizer_type=AdamW8bit \
  --lr_scheduler=cosine \
  --max_train_steps=1500 \
  --train_batch_size=1 \
  --mixed_precision=fp16 \
  --save_precision=fp16 \
  --gradient_checkpointing \
  --seed=42 \
  --logging_dir="$OUTPUT_DIR/log" \
  --save_every_n_epochs=2 \
  --no_half_vae

echo "=== 학습 종료 — $(date) ==="
ls -lh "$OUTPUT_DIR/"
