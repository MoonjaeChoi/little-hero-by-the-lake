# 5011 — Endgame Boss Sprites Decisions

> **Phase 2.3 Mediator** · 모드: ai-assisted · 일시: 2026-05-05

## 1. 요약 표

| Priority | Item | Decision |
|---|---|---|
| Critical | D2 | **A+ — Prompt 강화 후 재생성** (인형 실사판 회피) |
| Medium | D3 | **A — GUID 보존** (5009 패턴) |

## 2. Detailed Decisions

### 2.1 D2 — 재생성 + Prompt 강화

- **사용자 시각 review 결과**: 기존 11 보스 sprite "인형 실사판" 톤 → V5 sticker 의도 미달성
- **Decision**: 재생성 진행 + prompt 강화 (인형/3D/실사 회피)
- **Rationale**: 단순 prompt (PRD 원안) 으로는 동일 톤 재발 가능 → V5 sticker 강조 + negative 강화 필수
- **Prompt 갱신**:
  - **Positive 추가**: `2D flat cartoon sticker illustration, hand-drawn V5 dynamic tone, vivid bold colors`
  - **Negative 추가**: `plush toy photography, plushie, soft toy, realistic doll, doll photography, toy photo, 3D render, octane render, photographic, 3D model, realistic`

### 2.2 D3 — GUID 보존 (5009 패턴)

- **Decision**: A — 같은 PNG 경로 덮어쓰기 + .meta GUID 보존
- **Rationale**: EnemyData SO icon path 변경 X → 11 SO 재wiring 0건

## 3. v2 변경 diff

- §1.1 위치 정정: `Generated_v2/Enemies/` → `Generated_v2/Bosses/`
- §3.1/§3.2 prompt 강화: V5 sticker + 인형/3D 회피
- 시드 보존: Daily 99000~99006 + Weekly 99100~99103
