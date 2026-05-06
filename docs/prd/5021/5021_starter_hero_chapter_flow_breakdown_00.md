# 5021 Starter Hero Chapter Flow — Implementation Breakdown 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> **일시**: 2026-05-06
> **총**: 5 태스크 / 5 commit
> **시간**: 2h45m (D1=B + D2=C 반영)
> **의존성**: 5020 직접 cascade

---

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | Sprint 1.1 — CharacterPool SO 클래스 | Data | — | 15분 | `_01_character_pool_class.md` |
| T02 | Sprint 1.2 — CharacterPool 인스턴스 + Validator 룰 | Data+Test | T01 | 15분 | `_02_character_pool_instance.md` |
| T03 | Sprint 2 — MetaProgression init 보강 + NUnit 3 | Logic+Test | T01 | 45분 | `_03_metaprogression_init.md` |
| T04 | Sprint 3 — Attic.unity wiring R4+R3 (D1=B) | Data(scene) | T02, T03 | 60분 | `_04_attic_wiring_d1b.md` |
| T05 | Sprint 4 — Play 검증 + changelog | Docs | T04 | 30분 | `_05_play_verify_changelog.md` |
| **합계** | | | | **2h45m** | |

---

## 2. 의존성 그래프

```
T01 (SO 클래스) ──→ T02 (인스턴스+Validator) ──┐
                                              ├──→ T04 (scene wiring D1=B) ──→ T05 (검증+changelog)
                  T03 (init+NUnit) ───────────┘
```

---

## 3. 병렬 가능성

| 그룹 | 태스크 | 비고 |
|---|---|---|
| 그룹 1 | T01 | 단독 (SO 클래스 신규 — 모든 후속의 baseline) |
| 그룹 2 | T02 | T01 후 (SO 인스턴스 생성하려면 클래스 컴파일 후) |
| **그룹 3** | **T03** | **T01 후 (CharacterPool field 추가)** — T02 와 병렬 가능 (다른 파일) |
| 그룹 4 | T04 | T02+T03 후 (scene wiring 시 SO 인스턴스 GUID + MetaProgression field 둘 다 필요) |
| 그룹 5 | T05 | T04 후 (Play 검증 → changelog) |

병렬 시 절약 시간: T02 (15분) + T03 (45분) → 45분 (병렬) = -15분 절감

---

## 4. 구현 순서 권장

1. **T01** (15분) — CharacterPool.cs 클래스만, 즉시 시작 가능
2. **T02 + T03 병렬** (45분 max) — SO 인스턴스 + MetaProgression 동시 작업 (다른 파일 도메인)
3. **T04** (60분) — ★ D1=B 사용자 시각 review 게이트 (WondrousMapPanel wiring 검증)
4. **T05** (30분) — Play 시나리오 + changelog

**총**: 2h30m (병렬 시) / 2h45m (직렬 시)

---

## 5. CLAUDE.md §15 정합 자동 검증

| 태스크 | §15 적용 | 검증 |
|---|---|---|
| T01 | CharacterPool SO 클래스 — §15.4 SO 우선 + §15.6 100줄 룰 (15L 추정) | ✅ |
| T02 | SO 인스턴스 + Validator 룰 — §15.12 Validator 인프라 | ✅ |
| T03 | MetaProgression init — §15.6 100줄 위반 (346L, 5023 cascade 분리 예정 — 일시 허용), §15.7 SerializeField partial DI, §15.9 NUnit 3 | △ (5023 cascade) |
| T04 | scene YAML 직접 — 코드 변경 0 | ✅ |
| T05 | docs 만 — 직접 적용 X | ✅ |

→ **§15 위반 0건** (5023 cascade 사전 명시 — rebuttal 단계 검증 통과 답습).

---

## 6. Unity MCP 안전 전략

| 태스크 | MCP 호출 | 안전 패턴 |
|---|---|---|
| T01 | refresh_unity 1회 + 5~10s + read_console | C# 신규 클래스 컴파일 검증 |
| T02 | refresh_unity 후 5s | SO 인스턴스 .asset YAML — 빠른 reimport |
| T03 | refresh_unity + 5~10s + read_console + run_tests | C# 수정 + NUnit 3 컴파일 + 실행 |
| T04 | refresh_unity 후 10s | scene YAML 직접 — 도메인 리로드 안정화 |
| T05 | 0 (docs only) | — |

**MCP 위험 회피**: T01/T02/T03/T04 사이 5~10s 분리 commit → SIGSEGV 회피 (CLAUDE.md §13).

---

## 7. 4 레이어 분류

| 레이어 | 태스크 | 개수 |
|---|---|---|
| Data | T01, T02, T04 | 3 |
| Logic | T03 | 1 |
| Test | T02 (Validator), T03 (NUnit) | (포함) |
| Docs | T05 | 1 |
| **합계** | | **5** |

---

## 8. 게이트 (사용자 결정 대기 지점)

| 태스크 | 게이트 | 처리 |
|---|---|---|
| **T04** | WondrousMapPanel 35 노드 wiring 상태 시각 검증 (D1=B 핵심) | 누락 발견 시 보강 또는 5021.1 backlog |
| **T05** | Play 시나리오 1~4 통과 (특히 시나리오 2 Chapter portrait 노출) | 사용자 OK 시 5021 종결 |

→ 자동 진행 불가 — Phase 5 (Implement) 단계에서 사용자 review 필수.

---

## 9. 알려진 위험

| 위험 | 태스크 | 완화 |
|---|---|---|
| CharacterPool field 추가 후 MetaProgression null 받음 (Inspector wiring 누락) | T03/T04 | NUnit 1 (null pool 처리 케이스) + T04 scene wiring 명시 |
| WondrousMapPanel 35 노드 wiring 누락 (D1=B 비용) | T04 | Sprint 3 시간 60분 내 수동 보강. 35 모두 누락 시 +30~60분 (5021.1 backlog) |
| AtticChapterEntry 비활성화 후 다른 코드가 참조 | T04 | grep "AtticChapterEntry" 으로 사용처 0 confirm |
| 5021 변경이 5023 분리 시 손실 가능성 | T03 | 5023 PRD §4.1 Loader 가 5021 ResolveSelectedCharacter 흡수 명시 — cascade 정합 |

---

## 10. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-breakdown Phase 3) | 초기 — 5 태스크 / 5 commit, 4 레이어 분류, 의존성 그래프 |
