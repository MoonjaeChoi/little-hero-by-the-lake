# 5014 — V5 sticker LoRA Decisions

> **Phase 2.3 Mediator** · ai-assisted · 2026-05-05

## 1. 결정

| Priority | Item | Decision |
|---|---|---|
| Critical | D1 | **B** — PRD 갱신 + 진행 |
| Medium | D2 | **A** — 459 PNG 전체 학습 데이터 |
| Medium | D3 | **B** — 서버 SSH automation (Kohya CLI) |

## 2. v2 변경 diff

- §1.1 데이터 풀: 66 → **459 PNG** (5009/5010/5011 진행 후 누적)
- §3.1 Sprint 2 = SSH automation script (서버 192.168.75.194 RTX 5070 + sd-scripts)
- caption 자동 생성 script (5009/5010/5011 batch 의 prompt 구조 활용)
- 학습 시간 추정 2~4h (459 sample, 1500 step)

## 3. 권장 진행 방식

- **Sprint 1**: caption 자동 생성 script + 459 PNG + caption 정리 (Claude Code, ~1h)
- **Sprint 2**: SSH script (Kohya 학습 trigger), 학습 자체는 서버 측 background, 사용자 trigger
- **Sprint 3+**: 학습 완료 후 결과 검증 + workflow + docs (Claude Code)

## 4. Trade-off

- D1=B 의 비용 (~5~7h + 학습 외주) ↔ 향후 sprite 작업 prompt 부담 ↓ + 톤 일관성 ↑
- D2=A 459 데이터 풍부함 vs caption 작성 시간 ↑ (자동 script 로 완화)
- D3=B SSH automation 재현성 ↑ vs 서버 setup 추가 (sd-scripts 설치 등)
