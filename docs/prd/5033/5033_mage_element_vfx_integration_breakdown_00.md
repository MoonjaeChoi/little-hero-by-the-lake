# 5033 mage_element_vfx_integration Implementation Breakdown — 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> 일시: 2026-05-06 · 총 3 태스크 / 3 commit / ~1h 20m
> PRD 출처: `5033_mage_element_vfx_integration_v2.md` (mediator 확정)

---

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| **T01** | Editor 도구 + 4 prefab variant + ElementVfxBank Bootstrap | Editor + Resources + Logic | - | ~35분 | `_01_editor_tool_variants_bootstrap.md` |
| **T02** | ElementVfxBank.Spawn + ElementResolver hook | Logic | T01 | ~30분 | `_02_spawn_and_hook.md` |
| **T03** | ElementVfxBankTests NUnit + changelog + CLAUDE.md | Test + Docs | T02 | ~15분 | `_03_tests_and_docs.md` |
| **합계** | | | | **~1h 20m** | **3 파일** |

각 태스크 = 1 commit. PRD v2 의 Sprint 와 1:1 매핑 (Sprint 1 = T01, Sprint 2 = T02, Sprint 3 = T03).

---

## 2. 의존성 그래프

```
T01 (Editor 도구 + 4 prefab + ElementVfxBank.EnsureLoaded/GetPrefab)
   ↓
T02 (ElementVfxBank.Spawn + ElementResolver hook)
   ↓
T03 (NUnit + changelog + CLAUDE.md)
```

**순차 실행** — T01 → T02 → T03. 병렬 가능성 X (T02 는 T01 의 prefab + GetPrefab 의존, T03 는 T02 의 Spawn 의존).

---

## 3. 4 레이어 분류

| 레이어 | 태스크 | 비고 |
|---|---|---|
| **Editor** | T01 (SetupElementVfxVariants.cs) | Tools/LittleHero 메뉴 (~50L) |
| **Resources** | T01 (4 prefab variant) | Editor 도구 자동 생성 |
| **Logic** | T01 (ElementVfxBank Bootstrap) + T02 (Spawn + ElementResolver hook) | 정적 service, ~70L |
| **Test** | T03 (ElementVfxBankTests NUnit 2) | 선택 — Resources Load + None 가드 |
| **Docs** | T03 (changelog + CLAUDE.md §14) | 변경 이력 |

UI 레이어 없음 (5033 은 시각 효과 통합만 — UI 변경 X).

---

## 4. 구현 순서 권장

순차 (의존성 고정):

1. **T01** (~35분) — Editor 도구 신규 + 메뉴 실행으로 4 prefab 생성 + ElementVfxBank.cs Bootstrap (const + EnsureLoaded + GetPrefab)
2. **컴파일 확인** — `mcp__UnityMCP__refresh_unity` + 5~10초 대기 + `read_console` errors=0
3. **commit 1**
4. **T02** (~30분) — ElementVfxBank.Spawn 메서드 추가 + ElementResolver.ApplyElementEffect hook
5. **컴파일 확인** — refresh + read_console
6. **commit 2**
7. **T03** (~15분) — ElementVfxBankTests NUnit 2 + changelog_v1.md + CLAUDE.md §14
8. **컴파일 + Editor Test 확인** — Test Runner 또는 reflection 으로 PASS 확인
9. **commit 3**

---

## 5. CLAUDE.md §15 정합 사전 검증

| 태스크 | §15 정합 | 비고 |
|---|---|---|
| T01 SetupElementVfxVariants | §15.6 100줄 ✅ (~50L), §15.7 정적 도구 (DI 무관) | Editor 메뉴 패턴 (CLAUDE.md §16) |
| T01 ElementVfxBank | §15.6 100줄 ✅ (~70L), §15.4 const (R2), §15.7 EnsureLoaded lazy (D3=B) | R1 SerializeField static 제거 |
| T02 ElementResolver hook | 기존 클래스 1 메서드 호출 추가 — §15 변동 X | 5029 시그니처 보존 (NUnit 회귀 0) |
| T03 ElementVfxBankTests | §15.9 Test 정합 (NUnit 2 추가) | NUnit 178 → 180 |

전체 §15 정합 ✅ — Reject 2 (R1+R2) Phase 2.2 에서 자동 수용 완료.

---

## 6. Unity MCP 안전 전략 (CLAUDE.md §13)

각 태스크 사이 도메인 리로드 회피:

| 태스크 후 | 안전 패턴 |
|---|---|
| T01 commit 후 | `refresh_unity` + 10초 대기 (Editor 메뉴 신규 + 4 prefab 추가 + Logic 클래스 갱신 — domain reload 큼) |
| T02 commit 후 | `refresh_unity` + 5초 대기 (ElementResolver 1 메서드 호출 추가만 — 작은 변경) |
| T03 commit 후 | `refresh_unity` + 5초 대기 (NUnit 2 신규 + docs) |

**위험**: T01 의 Editor 메뉴 실행 (`Tools/LittleHero/Setup Element VFX Variants`) 은 반드시 commit 전에 완료 — prefab 4 생성 후 Asset Database refresh.

---

## 7. 검증 우선순위 (Phase 4 Contract 사전 미리보기)

| 카테고리 | T01 | T02 | T03 |
|---|---|---|---|
| C1 컴파일 errors=0 | ✅ | ✅ | ✅ |
| C2 Editor Test | N/A | N/A | NUnit 178→180 |
| C3 ProjectValidator | warnings 변동 0 | warnings 변동 0 | warnings 변동 0 |
| C4 Reflection (Type/Method) | ElementVfxBank.EnsureLoaded/GetPrefab/SPAWN_LIFETIME_SECONDS | ElementVfxBank.Spawn / ElementResolver.ApplyElementEffect (5029 보존) | ElementVfxBankTests Type 존재 |
| C5 SO 검증 | 4 prefab Resources/VFX/Element/ 존재 | N/A | N/A |
| C6 Play 시나리오 | 시나리오 1 Resources Load | 시나리오 2 마법사 element 시각 + 시나리오 3 URP 호환 + 시나리오 4 PG-12 톤 | (T02 종료 시 검증) |

상세는 Phase 4 contract.md 에서 확정.

---

## 8. 알려진 위험 (PRD § 8 답습)

| 위험 | 대응 태스크 |
|---|---|
| Hovl prefab URP 2D 호환 (분홍/검정 표시) | T02 commit 후 사용자 Play Mode 검증 → 미합격 시 `Edit > Render Pipeline > Universal RP > Convert URP Materials` |
| Resources prefab variant 부서짐 | T01 prefab variant 사용 — Hovl 원본 변경 X (외부 에셋 안전) |
| Instantiate + Destroy GC 누적 (~수백 KB) | D2=A 결정 — 5033.1 후속 cascade VfxPool Register/Spawn |
| PG-12 톤 — Red energy explosion 강렬 | 사용자 Play Mode 검증 → 미합격 시 prefab 교체 (4 GameObject ref 만) |
| SetupElementVfxVariants Editor 메뉴 실행 누락 | T01 작업 흐름에 메뉴 실행 단계 명시 + 검증 |

---

## 9. 변경 이력
- 2026-05-06: 초기 작성 (Phase 3 Breakdown). 3 태스크 / 3 commit / ~1h 20m.
