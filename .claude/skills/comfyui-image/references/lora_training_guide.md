# LoRA Training Guide — V5 sticker (5014 baseline)

> **출처**: 5014 PGE 6-Phase. RTX 5070 12GB Blackwell sm_120 + sd-scripts + Python 3.12 + torch 2.8.0+cu128 검증.
> **목적**: V5 sticker 톤 LoRA 재학습 또는 추가 sprite 학습 시 절차 reference.

## 1. 환경 setup (한 번만)

### 1.1 서버 환경

| 항목 | 값 |
|---|---|
| 서버 | `pro301@192.168.75.194` |
| GPU | RTX 5070 12GB (Blackwell sm_120) |
| Python | 3.12 (3.9 는 sd-scripts PEP 604 syntax 호환 X) |
| torch | 2.8.0+cu128 (sm_120 호환 stable wheel) |
| sd-scripts | https://github.com/kohya-ss/sd-scripts.git |

### 1.2 sd-scripts 설치

```bash
ssh pro301@192.168.75.194
git clone https://github.com/kohya-ss/sd-scripts.git ~/sd-scripts
cd ~/sd-scripts
python3.12 -m venv venv
source venv/bin/activate
pip install --upgrade pip
pip install torch==2.8.0 torchvision --index-url https://download.pytorch.org/whl/cu128
pip install -r requirements.txt
pip install bitsandbytes
```

### 1.3 accelerate config

```bash
mkdir -p ~/.cache/huggingface/accelerate
cat > ~/.cache/huggingface/accelerate/default_config.yaml <<'YAML'
compute_environment: LOCAL_MACHINE
distributed_type: 'NO'
mixed_precision: fp16
num_processes: 1
num_machines: 1
gpu_ids: '0'
use_cpu: false
YAML
```

## 2. 학습 데이터 준비

### 2.1 디렉토리 구조

```
~/lora_training/v5_sticker/
├── dataset/                  # PNG + .txt caption (1:1 매칭)
│   ├── Enemy_Meta_Boss.png
│   ├── Enemy_Meta_Boss.txt   # caption
│   ├── Chapters/             # 하위 폴더 (Kohya multi-subset)
│   ├── V1/
│   └── Bosses/
├── config/v5_sticker.toml    # dataset config
├── output/                   # LoRA .safetensors 출력
└── train.sh                  # 학습 trigger
```

### 2.2 Caption 자동 생성

`scripts/5014_caption_generator.py` (5009/5010/5011 batch script 의 prompt 매핑 활용):

```bash
python3 scripts/5014_caption_generator.py
# 결과: 262 caption.txt 자동 생성 (Kohya 형식, PNG 와 1:1)
```

Caption 형식:
```
v5 sticker tone, cute cartoon, chunky thick white outline,
soft cartoon shading, single character, transparent background,
{카테고리 prefix}, {archetype/캐릭터 form}
```

### 2.3 데이터 업로드

```bash
rsync -av --include='*.png' --include='*.txt' --include='*/' --exclude='*' \
  LittleHero/Assets/_Project/Art/Generated_v2/Enemies/ \
  pro301@192.168.75.194:~/lora_training/v5_sticker/dataset/

rsync -av --include='*.png' --include='*.txt' --exclude='*' \
  LittleHero/Assets/_Project/Art/Generated_v2/Bosses/ \
  pro301@192.168.75.194:~/lora_training/v5_sticker/dataset/Bosses/
```

## 3. dataset config (Kohya multi-subset)

`~/lora_training/v5_sticker/config/v5_sticker.toml`:

```toml
[general]
enable_bucket = true
shuffle_caption = true

[[datasets]]
resolution = [768, 1024]
batch_size = 1

[[datasets.subsets]]
image_dir = '/home/pro301/lora_training/v5_sticker/dataset'
caption_extension = '.txt'
num_repeats = 5

[[datasets.subsets]]
image_dir = '/home/pro301/lora_training/v5_sticker/dataset/Chapters'
caption_extension = '.txt'
num_repeats = 1

[[datasets.subsets]]
image_dir = '/home/pro301/lora_training/v5_sticker/dataset/V1'
caption_extension = '.txt'
num_repeats = 5

[[datasets.subsets]]
image_dir = '/home/pro301/lora_training/v5_sticker/dataset/Bosses'
caption_extension = '.txt'
num_repeats = 5
```

> **multi-subset 필수** — Kohya 가 단일 image_dir 의 하위 폴더 미인식. 각 폴더 별도 subset.
> **num_repeats** — chapter 210 은 1 (총 210), 나머지는 5 (30+11+11 × 5 = 260) → epoch 당 470 sample.

## 4. 학습 trigger

`scripts/5014_train_lora.sh` (서버 측 실행):

```bash
#!/bin/bash
set -e
cd ~/sd-scripts
source venv/bin/activate

OUTPUT_DIR=/home/pro301/lora_training/v5_sticker/output
CONFIG=/home/pro301/lora_training/v5_sticker/config/v5_sticker.toml
CKPT=/home/pro301/ai-tools/ComfyUI/models/checkpoints/sd_xl_base_1.0.safetensors

mkdir -p "$OUTPUT_DIR"

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
  --max_train_steps=3000 \
  --train_batch_size=1 \
  --mixed_precision=fp16 \
  --save_precision=fp16 \
  --gradient_checkpointing \
  --seed=42 \
  --logging_dir="$OUTPUT_DIR/log" \
  --save_every_n_epochs=2 \
  --no_half_vae
```

### 4.1 GPU 점유 회피

학습 전 **ComfyUI 종료 필수** (RTX 5070 12GB → ComfyUI 6.99GB 사용 중 시 OOM):

```bash
ssh pro301@192.168.75.194 "tmux kill-session -t comfyui; pkill -KILL -f 'main.py --listen'"
```

### 4.2 학습 background trigger

```bash
ssh pro301@192.168.75.194 "cd ~ && nohup bash ~/lora_training/v5_sticker/train.sh > ~/lora_training/v5_sticker/train.log 2>&1 & disown"
```

### 4.3 학습 모니터링

```bash
ssh pro301@192.168.75.194 "tail -c 3000 ~/lora_training/v5_sticker/train.log | tr '\\\\r' '\\\\n' | tail -3"
```

학습 진행 표시:
```
steps:  52%|█████▏    | 1570/3000 [26:22<24:01, 1.01s/it, avr_loss=0.0685]
```

## 5. 학습 시간 추정

| max_train_steps | 시간 (RTX 5070) | sample/epoch | 비고 |
|---:|---|---|---|
| 1500 | ~28분 | 150 (archetype 30 only) | v1 — 데이터 부족 |
| 2000 | ~33분 | 470 (multi-subset 262) | v2 |
| **3000** | **~50분** | **470 (multi-subset 262)** | **v3 권장** |

### 5.1 학습 결과 비교 (5014 검증)

| 학습 | weight | healthy 10 sample |
|---|---|---:|
| v1 (1500 step / archetype 30) | 0.7 | 3/10 |
| v2 (2000 step / multi-subset) | 0.7 | 5/10 |
| **v3 (3000 step / multi-subset)** | **0.7** | **5/10** |
| v3 (3000 step / multi-subset) | 0.85 | 5/10 |

→ **v2 = v3 동등** (학습 sample 풍부 시 2000 step 충분). 3000 step 은 over-train 위험.

## 6. ComfyUI 사용

### 6.1 LoRA 파일 배치

```bash
ssh pro301@192.168.75.194 "
  cp ~/lora_training/v5_sticker/output/v5_sticker_v1.safetensors ~/ai-tools/ComfyUI/models/loras/
  tmux new-session -d -s comfyui 'cd ~/ai-tools/ComfyUI && source venv/bin/activate && python main.py --listen 0.0.0.0 --port 8188'
"
```

### 6.2 워크플로

`assets/workflow_layerdiffuse_lora.json` — LoraLoader (id 12) 추가:
- model: CheckpointLoader → LoraLoader → LayerDiffuseApply → KSampler
- clip: CheckpointLoader → LoraLoader → CLIPTextEncode

### 6.3 prompt 단순화

기존 5008 baseline 23+ token → LoRA 모드 5~10 token (sprite_generation_rules.md §9.2 참조).

## 7. 알려진 이슈

| 이슈 | 해결 |
|---|---|
| OOM (CUDA out of memory) | ComfyUI 종료 필수 (12GB GPU 단일) |
| Python 3.9 → sd-scripts PEP 604 syntax 오류 | Python 3.12 venv 재생성 |
| sm_120 (Blackwell) 비호환 | torch 2.8.0+cu128 stable 사용 |
| Small 형태 frame 작음 | 학습 sample 자체가 small frame 답습 — 추론 시 "large character filling frame" prompt 추가 |
| Fantasy 카테고리 약함 | 5008 baseline 의 lavender 자체가 약함 — LoRA 도입 효과 작음 |

## 8. 재학습 체크리스트

신규 sprite 추가 후 재학습 시:

- [ ] 신규 PNG + caption.txt 생성 (`scripts/5014_caption_generator.py` 갱신)
- [ ] 서버 dataset/ 동기화 (rsync)
- [ ] dataset config 의 num_repeats 검토 (학습 균형)
- [ ] ComfyUI 종료 (GPU 해제)
- [ ] 이전 LoRA backup (overwrite 회피)
- [ ] 학습 trigger (`train.sh`)
- [ ] 학습 완료 후 LoRA 교체 + ComfyUI 재시작
- [ ] 10 sample 검증 (healthy 비율 + 사용자 시각 review)
- [ ] sprite_generation_rules.md §9 갱신 (필요 시)

## 9. 변경 이력

- 2026-05-05: 초기 작성 (5014 PGE 6-Phase 검증).
