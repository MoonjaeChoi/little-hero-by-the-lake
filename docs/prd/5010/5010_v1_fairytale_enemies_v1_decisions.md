# 5010 — V1 Fairytale Enemies Decisions

> **Phase 2.3 Mediator** · ai-assisted · 2026-05-05 · "권장안대로"

## 1. 결정

| Priority | Item | Decision |
|---|---|---|
| Critical | D1 | **A** — PRD §1.1 명단 즉시 정정 |
| Medium | D2 | **B** — 사이즈 통일 (512→768×1024) + 5008 baseline 재생성 |
| Medium | D3 | **A** — 5009 prompt 단일 character 강조로 자연 정리 |

## 2. v2 변경 diff

- §1.1 명단 정정 (실제 11 파일 기준)
- §3 prompt: 5008 baseline (sd_xl_base + 768×1024 + cfg 6 + steps 25 + LayerDiffuse RGBA) + 5009 단일 character 강조
- 시드 namespace: 11001~11011 (V1 전용, 5009/5011 시드와 충돌 X)
- Sprint 분해: 11 PNG 일괄 (1 commit) + docs (1 commit) — 최단 처리 (V1 legacy ROI 낮음)
