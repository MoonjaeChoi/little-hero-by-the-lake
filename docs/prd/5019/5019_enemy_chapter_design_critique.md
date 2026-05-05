# 5019 PRD 리뷰

## 종합: 9.0/10 — Accept 7 / Reject 0 / Discuss 2

## Accept (7)
- ✅ 35 챕터 narrative-specific 적 디자인 (옵션 A)
- ✅ 5008 archetype baseline + 5009 Chapter override 폴더 정합 (GUID 보존)
- ✅ scripts/5019_chapter_narrative_batch.py (5009 답습)
- ✅ 시드 namespace 90000+ (5009 22222+ / 5010 11001+ / 5011 자체 / 5019 90000+ 충돌 X)
- ✅ Sprint 1 prompt 표 + 사용자 review (큰 변경 사전 검증)
- ✅ EnemySpriteAuditor 5008 baseline 자동 검증
- ✅ 5018 ASTC 6×6 자동 적용 (TextureCompressionConfigurer 1회)

## Discuss

### D1. ComfyUI 모드 — 5008 baseline vs 5014 LoRA

- 5008 baseline (검증된 23+ token prompt) — 안정 ✅
- 5014 V5 sticker LoRA (5~10 token, 일부 카테고리 효과 약함) — Boss/Medium 5/10 healthy
- **A (★권장)**: 5008 baseline (production 우선, lora_training_guide.md 권장)
- B: 5014 LoRA (prompt 간결 — 23+ → 5~10 token)

### D2. 사용자 review 시점

- A: Sprint 1 후 (prompt 표 review 만, sprite 미생성)
- **B (★권장)**: Sprint 4 후 (2차 batch 종료 후 sprite 시각 review — 부분 재생성 결정)
- C: Sprint 5 (최종 docs 전 — 너무 늦음)

## 권장 일괄: D1=A 5008 / D2=B Sprint 4 후

## 변경 이력
- 2026-05-06: 초기 작성
