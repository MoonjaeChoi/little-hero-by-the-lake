# 5019 — Breakdown 개요

| ID | 작업 | 시간 |
|---|---|---:|
| T01 | 35×6=210 prompt 표 작성 (5019_prompt_table.md) | 60분 |
| T02 | scripts/5019_chapter_narrative_batch.py | 20분 |
| T03 | 1차 batch CH1~20 (Meta/Nature/Animal 120 sprite) | 50분 |
| T04 | 2차 batch CH21~35 (Civ/Fantasy 90 sprite) + Auditor | 40분 |
| T05 | review + 부분 재생성 + 5018 Texture 자동 + docs | 60분 |

## 의존성

```
T01 (prompt) ──┐
T02 (script) ──┴→ T03 (batch 1차) → T04 (batch 2차 + Auditor) → T05 (review + docs)
```

## §15 정합

- T01: prompt 표 markdown (코드 X)
- T02: 외부 Python script (5009/5010/5011 답습)
- T03/T04: ComfyUI batch (외부 자동화)
- T05: 5018 TextureCompressionConfigurer 자동 hook + docs

## 변경 이력
- 2026-05-06: 초기 작성
