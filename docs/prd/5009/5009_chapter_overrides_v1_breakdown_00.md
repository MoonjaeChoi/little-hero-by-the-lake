# 5009 — Chapter Overrides Implementation Breakdown — 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해 (RESERVED 해제, 5012/5013 종결 후 재진입)
> 일시: 2026-05-04 · 총 6 태스크 / 6 commit / 210 PNG 재생성

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | Meta CH 1~5 30 PNG (22222+(CH-1)*6) | Asset | - | 30~45분 + review | _01_meta.md |
| T02 | Nature CH 6~14 54 PNG (33333+...) | Asset | - | 50~70분 + review | _02_nature.md |
| T03 | Animal CH 15~20 36 PNG (44444+...) | Asset | - | 35~50분 + review | _03_animal.md |
| T04 | Civ CH 21~27 42 PNG (55555+...) | Asset | - | 40~55분 + review | _04_civ.md |
| T05 | Fantasy CH 28~35 48 PNG (77777+...) | Asset | - | 45~65분 + review | _05_fantasy.md |
| T06 | Auditor + SpriteAtlas + docs | Test+Docs | T01~T05 | 25분 | _06_auditor_atlas_docs.md |

## 2. 의존성 그래프

```
T01 (Meta) → T02 (Nature) → T03 (Animal) → T04 (Civ) → T05 (Fantasy) → T06 (Auditor+Atlas+docs)
```

순차 — 사용자 review 사이클 의존 (각 batch 후 review 결과 따라 다음 진행).

## 3. 병렬 가능성

- 카테고리간 prompt 톤 독립 → 이론상 병렬 가능
- 그러나 ComfyUI 서버 단일 GPU 라 순차 큐잉 효율적
- 사용자 review 도 순차 권장 (한 카테고리씩 톤 검증)

## 4. 구현 순서 권장

순차 (T01 → T02 → T03 → T04 → T05 → T06) — 약 3.5~5h.

**중간 정지점** 권장: 각 Sprint 후 사용자 review → 톤 OK 확인 후 다음 Sprint 진행.

## 5. CLAUDE.md §15 정합

- §15.4 SO 우선 — prompt = sprite_generation_rules.md 참조 ✅
- §15.6 자산 작업 (코드 변경 X) — 100L 룰 무관 ✅
- §15.12 Auditor 재활용 (Validator 역할) ✅

## 6. ComfyUI 서버 안전 전략

- 사이클 = 1 sprite generation + image fetch + Unity import + meta wiring
- 한 sprite ~10~15s GPU 시간 (sd_xl_base 1.0 + LayerDiffuse decode)
- 30~50 PNG batch 당 ~10~20분 + 사용자 review
- 서버 OOM 위험 — sprite_generation_rules §7 auto-retry

## 7. 카테고리 prompt 매핑 (5008 baseline 그대로)

| 카테고리 | 시드 base | prefix | 톤 |
|---|---:|---|---|
| Meta | 22222 | `friendly mushroom child / village kid / burly man / village guardian / elder / patriarch` | 따뜻한 베이지·갈색 |
| Nature | 33333 | `moss sprite / acorn faerie / bramble guardian / forest watcher / root spirit / forest king` | 싱그러운 녹색·갈색 |
| Animal | 44444 | `kitten / puppy / golden retriever / tabby cat / sheepdog / lion` | 따뜻한 갈색·주황 |
| Civ | 55555 | `paper crane / cardboard robot / origami giant / paper knight / cardboard wizard / pastel mech` | 차가운 푸른·회색 |
| Fantasy | 77777 | `friendly fairy tale character (lavender pastel)` × 6 archetype | 부드러운 연보라 |

각 챕터의 시드 = `base + (CH-1)*6` (Rebuttal accepted 정정).

## 8. 변경 이력
- 2026-05-04: 초기 작성 (Phase 3 Breakdown).
