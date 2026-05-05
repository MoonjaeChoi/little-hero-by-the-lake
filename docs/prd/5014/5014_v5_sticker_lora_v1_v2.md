# 5014 — V5 sticker LoRA v2 (최종)

> Mediator 확정 (D1=B 진행 + D2=A 459 PNG + D3=B SSH automation)

## 1. 데이터 풀 (D2=A)

**459 PNG** (Generated_v2/Enemies + Bosses):
| 카테고리 | 개수 | 위치 |
|---|---:|---|
| V2 archetype (5 cat × 6 arch) | 30 | Enemies/Enemy_*.png |
| V1 동화 | 11 | Enemies/V1/*.png |
| Chapter override 35 × 6 | 210 | Enemies/Chapters/*.png |
| Daily 7 + Weekly 4 | 11 | Bosses/*.png |
| 기타 (UI/Memory 등) | 197 | Generated_v2 외 영역 |
| **학습 대상 (적/보스)** | **262** | Enemies + Bosses |

> 효율 위해 학습 대상은 적/보스 262 (UI/Memory 제외).

## 2. Sprint 분해

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | caption 자동 생성 script + 262 caption .txt | 1 | 45분 (script ~30분 + 검증 15분) |
| 2 | 서버 환경 setup (sd-scripts 설치) + 학습 SSH script + 학습 trigger | 1 | 30분 + 학습 2~4h (서버) |
| 3 | LoRA 결과 검증 (10 sample) | 1 | 30분 |
| 4 | workflow_layerdiffuse_lora.json + sprite_generation_rules v2 | 1 | 25분 |
| 5 | lora_training_guide.md + docs | 1 | 30분 |
| **총** | | **5** | **~4~6h** |

## 3. Caption 자동 생성 (D2=A 풀 활용)

scripts/5014_caption_generator.py — 5009/5010/5011 batch script 의 prompt 매핑 활용:

```python
# 각 sprite 별 caption 형식
caption = f"v5 sticker tone, cute cartoon, chunky thick white outline, soft cartoon shading, single character, transparent background, {category_prefix}, {archetype_form}"
```

5009/5010/5011 batch 의 prefix + form 그대로 caption.txt 로 저장.

## 4. 서버 SSH automation (D3=B)

### 4.1 서버 환경 setup (Sprint 2 첫 작업)

```bash
ssh pro301@192.168.75.194
mkdir -p ~/lora_training/v5_sticker/{dataset,output,config}

# sd-scripts 설치 (한 번만)
git clone https://github.com/kohya-ss/sd-scripts.git
cd sd-scripts
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
pip install xformers
pip install torch==2.1.0+cu121 torchvision==0.16.0+cu121 --index-url https://download.pytorch.org/whl/cu121
```

### 4.2 학습 데이터 업로드

```bash
rsync -av LittleHero/Assets/_Project/Art/Generated_v2/Enemies/ pro301@192.168.75.194:~/lora_training/v5_sticker/dataset/
rsync -av LittleHero/Assets/_Project/Art/Generated_v2/Bosses/ pro301@192.168.75.194:~/lora_training/v5_sticker/dataset/
# caption .txt 파일들도 같은 폴더에 (Kohya 형식 — image.png + image.txt 1:1)
```

### 4.3 학습 config + 실행

`config/v5_sticker.toml`:
```toml
[general]
enable_bucket = true
[[datasets]]
[[datasets.subsets]]
image_dir = "/home/pro301/lora_training/v5_sticker/dataset"
caption_extension = ".txt"
num_repeats = 5
```

학습 명령:
```bash
accelerate launch --num_cpu_threads_per_process=2 sdxl_train_network.py \
  --pretrained_model_name_or_path=sd_xl_base_1.0.safetensors \
  --dataset_config=config/v5_sticker.toml \
  --output_dir=output \
  --output_name=v5_sticker_v1 \
  --save_model_as=safetensors \
  --network_module=networks.lora \
  --network_dim=32 --network_alpha=16 \
  --learning_rate=1e-4 \
  --max_train_steps=1500 \
  --train_batch_size=1 \
  --mixed_precision=fp16 \
  --gradient_checkpointing
```

## 5. 검증 시나리오

### S1 — LoRA 10 sample 검증
- 5 카테고리 × 2 archetype = 10 sample
- 5008 baseline 대비 V5 톤 일관성

### S2 — Prompt 단순화
- 신규 sprite 작업 시 prompt 23+ → 5~10 token 단순화 가능 검증

### S3 — Over-fit 검증
- weight 0.7 / 0.85 / 1.0 비교

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 서버 sd-scripts 설치 시간 | Sprint 2 첫 작업 — 한 번만 |
| 학습 시간 2~4h | background — Sprint 2 commit 후 사용자 trigger 또는 SSH 백그라운드 실행 |
| caption 자동 생성 정확도 | 5009/5010/5011 batch script 의 검증된 prompt 활용 |
| Over-fit | weight 0.7 default + augmentation 검토 (Sprint 3 후) |

## 7. 변경 이력

- 2026-05-04: v1 작성
- 2026-05-05: v2 (D1=B / D2=A 459→262 학습 대상 / D3=B SSH automation)
