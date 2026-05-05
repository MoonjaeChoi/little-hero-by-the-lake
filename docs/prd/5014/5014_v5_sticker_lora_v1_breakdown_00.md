# 5014 — Breakdown 00 (개요)

| ID | 제목 | 의존성 | 시간 |
|---|---|---|---|
| T01 | caption 자동 생성 + 262 caption.txt | - | 45분 |
| T02 | 서버 환경 setup (sd-scripts) + 학습 SSH script + trigger | T01 | 30분 + 학습 2~4h |
| T03 | LoRA 결과 검증 (10 sample) | T02 | 30분 |
| T04 | workflow_layerdiffuse_lora.json + sprite_generation_rules v2 | T03 | 25분 |
| T05 | lora_training_guide.md + docs | T04 | 30분 |

## 의존성 그래프
```
T01 → T02 (서버) → T03 → T04 → T05
```

T02 학습 시간 = 서버 background. Claude Code 는 SSH script 작성 + trigger 만, 학습 자체는 사용자 monitoring.
