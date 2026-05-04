# 5008 Chapter Enemy Asset Quality v1 — Implementation Breakdown 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> 일시: 2026-05-04 · 총 6 태스크 + 1 조건부 / 6~7 commit
> 출처: `5008_enemy_visual_pipeline_v2_v2.md` (mediator 최종)

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | LayerDiffuse 워크플로 + 카테고리 prompt 가이드 + fallback 한계 | 스킬 (외부) | — | 50분 | `_01_layerdiffuse.md` |
| T02 | EnemySpriteAuditor (D1=C 복원 패턴) + V2 폴백 검증 (D2=C) | Logic (Editor) | — | 45분 | `_02_auditor.md` |
| T03 | Meta 6 archetype 재생성 (시드 22222) + review | 자산 | T01, T02 | 30~60분 | `_03_meta.md` |
| T04 | Nature + Animal 12 archetype 재생성 + review | 자산 | T03 PASS | 45~75분 | `_04_nature_animal.md` |
| T05 | Civ + Fantasy 12 archetype 재생성 + review | 자산 | T04 PASS | 45~75분 | `_05_civ_fantasy.md` |
| T05.5 | (조건부) 손상 8 chapter override 흡수 | 자산 | T02 결과 fallback 깨짐 | 0~30분 | `_05_5_chapter_override.md` |
| T06 | docs (changelog + CLAUDE.md §11/§12/§14) + SpriteAtlas 재빌드 + skip list (D3=A+B) | docs | T05 PASS (+T05.5) | 25~30분 | `_06_docs.md` |

## 2. 의존성 그래프

```
T01 (LayerDiffuse 스킬) ──┐
                          ├──→ T03 (Meta) ──→ T04 (Nature+Animal) ──→ T05 (Civ+Fantasy) ─┬─→ T06 (docs+atlas+skip)
T02 (Auditor + V2 폴백 검증) ┘                                                            │
                            (T02 결과 fallback 깨짐) ──→ T05.5 (chapter override) ────────┘
```

## 3. 병렬 가능성

- **병렬 그룹 1**: T01 + T02 (서로 독립 — 스킬 외부 vs Editor 내부)
- **순차 그룹**: T03 → T04 → T05 (사용자 review 사이클, 카테고리별)
- **조건부**: T05.5 (T02 V2 폴백 검증 결과 = 깨짐 시만 활성화)
- **종결**: T06 (T05 PASS 후, T05.5 활성화 시 그 이후)

## 4. 구현 순서 권장

1. **T01 + T02 병렬** (~95분)
2. **T03** (Meta, ~45분 평균)
3. **T04** (Nature+Animal, ~60분 평균)
4. **T05** (Civ+Fantasy, ~60분 평균)
5. **(조건부) T05.5** (chapter override, ~30분, T02 결과에 따라)
6. **T06** (docs, ~30분)

총 약 **4~6시간** (사용자 review 시간 포함, T05.5 조건부)

## 5. CLAUDE.md §15 정합 자동 검증

| 태스크 | §15 정책 | 정합 여부 |
|---|---|---|
| T01 | §15.4 SO 우선 (prompt = MD 문서) | ✅ |
| T02 | §15.6 100L (~80L 추정) | ✅ |
| T02 | §15.7 DI (Editor 정적 도구, 정당) | △ |
| T02 | §15.12 Validator (Auditor 가 Validator 역할) | ✅ |
| T03~T05 | 자산 작업 — OOP 직접 영향 X | — |
| T05.5 | (조건부) 자산 작업 | — |
| T06 | docs — 영향 X | — |

## 6. Unity MCP 안전 전략

- **T01**: 스킬 파일 (스크립트 X), MCP 영향 없음
- **T02**: EnemySpriteAuditor.cs 신규 → `mcp__UnityMCP__refresh_unity` 후 5~10초 대기 → `read_console` errors=0 확인
- **T03~T05**: 자산 임포트 (sprite reimport) → 임포트 후 5초 대기 + AssetDatabase.Refresh
- **T05.5**: T03~T05 와 동일
- **T06**: docs 만, MCP 영향 없음

## 7. PRD v2 차이점 (v1 → v2)

- **Sprint 5.5 추가** (D2=C 조건부) — Auditor 결과에 따라
- **Sprint 6 확장** (D3=A+B) — skip list docs + 5009 anchor 우선
- **Sprint 6 추가** (Rebuttal accepted) — SpriteAtlas 재빌드
- **Sprint 1 SKILL.md 가이드 강화** (Rebuttal accepted) — fallback 한계 명시
- **Sprint 2 구현 방식 명시** (D1=C) — try/finally 복원 패턴

## 8. 변경 이력

- 2026-05-04: 초기 작성 (Phase 3 Breakdown). 6 태스크 + 1 조건부 = 7 문서.
