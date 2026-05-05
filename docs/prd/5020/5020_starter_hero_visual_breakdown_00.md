# 5020 Starter Hero Visual — Implementation Breakdown 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> **일시**: 2026-05-06
> **총**: 7 태스크 / 7 commit
> **시간**: 4h05m~5h05m (서버 시간 + 사용자 review iteration 제외)

---

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | Sprint 1 — Prelude (GUID 매핑 + D-A 검증) + Prompt 표 작성 | Docs | — | 60분 | `_01_prompt_table_prelude.md` |
| T02 | Sprint 2 — ComfyUI 배치 스크립트 + 6 PNG 생성 | Asset | T01 | 90~150분 | `_02_comfyui_batch_6png.md` |
| T03 | Sprint 3a — SO/PNG `git mv` rename + Selected_Characters_v1_legacy | Data | T02 | 25분 | `_03_git_mv_rename.md` |
| T04 | Sprint 3b — SaveDataMigration 헬퍼 + SaveSystem hook | Logic | T03 | 35분 | `_04_savedata_migration.md` |
| T05 | Sprint 3c — Sprite pivot + displayName + PSB ref Editor/ 이동 | Data | T03 | 20분 | `_05_pivot_displayname_psb.md` |
| T06 | Sprint 4 — StarterHeroAuditor + NUnit 9 + Validator 룰 | Test | T04, T05 | 40분 | `_06_auditor_tests_validator.md` |
| T07 | Sprint 5 — Play 검증 + changelog | Docs | T06 | 20분 | `_07_play_verify_changelog.md` |

---

## 2. 의존성 그래프

```
T01 (Prelude + Prompt)
  ↓
T02 (ComfyUI 6 PNG + 사용자 review iteration ★ 게이트)
  ↓
T03 (git mv rename — wired PNG GUID 보존)
  ├──→ T04 (SaveDataMigration — Logic)  ┐
  └──→ T05 (Pivot + displayName — Data) ┴──→ T06 (Auditor + NUnit + Validator)
                                                ↓
                                              T07 (Play 검증 + changelog)
```

---

## 3. 병렬 가능성

| 그룹 | 태스크 | 비고 |
|---|---|---|
| 그룹 1 | T01 | 단독 (시작) |
| 그룹 2 | T02 | T01 후 (★ 사용자 review 게이트) |
| 그룹 3 | T03 | T02 후 (Unity 도메인 리로드 → 5~10s 대기) |
| **그룹 4** | **T04 + T05 병렬** | **T03 후 동시 가능** (다른 파일 도메인) |
| 그룹 5 | T06 | T04+T05 후 (Test가 둘 다 의존) |
| 그룹 6 | T07 | T06 후 (Play 검증 → changelog) |

병렬 시 절약 시간: T04(35분) + T05(20분) → 35분 (병렬) = -20분 절감

---

## 4. 구현 순서 권장

1. **T01** (60분) — docs 작업, 즉시 시작 가능
2. **T02** (90~150분) — **★ ComfyUI 서버 + 사용자 시각 review iteration 게이트** (가장 긴 단일 태스크)
3. **T03** (25분) — `git mv` 일괄, Unity 콘솔 검증
4. **T04 + T05 병렬** (35분 max) — 코드 + 자산 동시 작업
5. **T06** (40분) — NUnit 9 신규, Validator 룰 추가
6. **T07** (20분) — Play 검증 후 changelog

**총**: 4h05m~5h05m (병렬 시) / 4h25m~5h25m (직렬 시)

---

## 5. CLAUDE.md §15 정합 자동 검증

| 태스크 | §15 적용 | 검증 |
|---|---|---|
| T01 | docs 만 — 직접 적용 X | ✅ |
| T02 | Python 5020_starter_hero_batch.py — Python 직접 적용 X (5019 답습) | ✅ |
| T03 | Data 마이그레이션 — `git mv` GUID 보존 (5009/5010/5019 답습) | ✅ |
| T04 | SaveDataMigration ≤64L (§15.6 100줄), 정적 헬퍼 (§15.7 DI) | ✅ |
| T05 | sprite .meta + SO YAML 만 수정 — 코드 변경 0 | ✅ |
| T06 | StarterHeroAuditor ≤80L (§15.6), NUnit 9 신규 (§15.9), Validator 룰 (§15.12) | ✅ |
| T07 | docs 만 — 직접 적용 X | ✅ |

→ **§15 위반 0건** (rebuttal 단계 검증 통과 답습).

---

## 6. Unity MCP 안전 전략

| 태스크 | MCP 호출 | 안전 패턴 |
|---|---|---|
| T01 | 0 (docs only) | — |
| T02 | comfyui-image 스킬 (ComfyUI 서버 호출 — Unity MCP 무관) | ComfyUI 서버 SSH 터널, Unity MCP 무관 |
| T03 | refresh_unity 1회 후 5~10s 대기 | git mv 후 Unity 도메인 리로드 안정화 |
| T04 | refresh_unity + 5~10s + read_console errors 0 | C# 신규 클래스 (SaveDataMigration) 컴파일 검증 |
| T05 | refresh_unity 후 5s | sprite .meta + SO YAML 만 (코드 X) — 빠른 reimport |
| T06 | refresh_unity + 10s + run_tests (NUnit) | Test 어셈블리 컴파일 + 9 케이스 PASS |
| T07 | 0 (docs only) | — |

**MCP 위험 회피**: T03/T04/T05/T06 사이 5~10s 분리 commit → SIGSEGV 회피 (CLAUDE.md §13).

---

## 7. 4 레이어 분류 (Little Hero 컨텍스트)

| 레이어 | 태스크 | 개수 |
|---|---|---|
| Docs | T01, T07 | 2 |
| Asset | T02 | 1 |
| Data | T03, T05 | 2 |
| Logic | T04 | 1 |
| Test | T06 | 1 |
| **합계** | | **7** |

---

## 8. 게이트 (사용자 결정 대기 지점)

| 태스크 | 게이트 | 처리 |
|---|---|---|
| **T02** | ComfyUI 6 PNG 시각 review (직업 정체성 + 어린이 이름 톤 정합) | 사용자 OK 시 T03 진행 |
| **T07** | Play 시나리오 1~3 통과 (특히 SaveData 마이그레이션) | 사용자 OK 시 5020 종결 |

→ 자동 진행 불가 — Phase 5 (Implement) 단계에서 사용자 review 필수.

---

## 9. 알려진 위험

| 위험 | 태스크 | 완화 |
|---|---|---|
| ComfyUI 비율/직업 정체성 흔들림 | T02 | seed 분리 (50200~50202 idle / 50210~50212 psb_ref) + retry |
| `git mv` 후 .meta 누락 → GUID 변경 → wiring NRE | T03 | T01 의 GUID 매핑 표 사전 검증 + Unity 콘솔 에러 0 |
| 구 SaveData 마이그레이션 누락 | T04 | unlockedCharacterIds[] 매핑 추가 (rebuttal D-B) + NUnit 5 |
| sprite pivot 변경 후 캐릭터 발 위치 어긋남 | T05/T07 | T07 Play 시나리오 2 검증 |
| NUnit 9 컴파일 실패 (Test 어셈블리) | T06 | refresh_unity + 10s + read_console |

---

## 10. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-breakdown Phase 3) | 초기 — 7 태스크 / 7 commit, 4 레이어 분류, 의존성 그래프 |
