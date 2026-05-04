# 5011 — Breakdown 00 (개요)

| ID | 제목 | 의존성 | 시간 |
|---|---|---|---|
| T01 | Daily 7 (DailyBoss_00~06, 시드 99000+) | - | 35~50분 |
| T02 | Weekly 4 (WeeklyBoss_00~03, 시드 99100+) | T01 | 25~35분 |
| T03 | Auditor + docs | T02 | 15분 |

## 의존성 그래프
```
T01 → T02 → T03
```

## 시드 매핑

Daily: `99000 + index` (00=99000, 06=99006)
Weekly: `99100 + index` (00=99100, 03=99103)

## ComfyUI 설정 (5008 baseline)

- workflow_layerdiffuse.json
- 768×1024 portrait, cfg 6, steps 25, euler/normal

## GUID 보존 (D3=A)

기존 PNG 덮어쓰기 — .meta GUID 변경 X
