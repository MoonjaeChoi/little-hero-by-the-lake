# 5033 mage_element_vfx_integration PRD Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.
> **권장 mode**: ai-assisted (Claude 추천 + 사용자 응답 대기)

---

## High Priority Items

(없음)

---

## Medium Priority Items

### D1. prefab variant 생성 방법

- **Original Criticism** (critique § 4 D1):
  PRD § 2.1 + 디자인 § 3.3 양쪽에 옵션 병기 — Sprint 1 시작 직전 결정 필요.

- **Current Position (PRD 그대로 진행 시 영향)**:
  두 옵션이 모두 § 2.1 에 남아 있어 구현 시점 결정 → Sprint 1 작업 흐름이 가변. 검증 재현성 ↓.

- **Discussion Points**:
  | 옵션 | 설명 | 장점 | 단점 |
  |---|---|---|---|
  | A | 사용자 Unity Editor 수동 (우클릭 → Create → Prefab Variant × 4) | 가시적 + 사용자 시각 검증 즉시 | 사용자 시간 소비 (~5분), 재현 어려움 |
  | **B (★권장)** | Claude Editor 도구 자동 — `Tools/LittleHero/Setup Element VFX Variants` (PrefabUtility.SaveAsPrefabAsset) | 결정론적 + 재현 가능 + 5033.1 (9 무기) cascade 시 재사용 | Editor 도구 1 신규 (~50L) |
  | C | `AssetDatabase.CopyAsset` 직접 — Sprint 1 작업 시 일회성 코드 | 빠름 (~10L) | Editor 메뉴 분리 X (재실행 어려움), Tools/LittleHero 패턴 위반 |

- **Proposed Resolution Path**:
  Claude 권장: **B**. CLAUDE.md §16 "Editor 메뉴 (Tools/LittleHero) — 콘텐츠 시드" 패턴 정합. 5033.1 cascade (9 무기 매핑) 시 재사용 가능. Sprint 1 작업량 30분 → 35분 정도 미세 증가.

- **PRD Impact**:
  - § 2.1 "생성 방법" 단일 옵션으로 정리 (사용자 결정 옵션만 남김).
  - 추가 코드 산출물: `Editor/SetupElementVfxVariants.cs` (Tools/LittleHero/Setup Element VFX Variants 메뉴).
  - Sprint 1 작업: prefab × 4 + Bootstrap + **Editor 도구 신규** (총 commit 1 개 유지).

---

### D2. GC 0 정착 시점

- **Original Criticism** (critique § 4 D2):
  Instantiate + Destroy → 적 ~4000/챕터 처치 시 GC 누적. PIVOT v4 §7 "GC <50KB/s" 목표 정합 여부 검토 필요.

- **Current Position (PRD 그대로 진행 시 영향)**:
  옵션 A (Sprint 1+2 단순 + 5033.1 후속 cascade) 가 PRD § 9 후속 backlog 에 명시되어 있으나, GC 즉시 정착할지 분리할지 결정 필요.

- **Discussion Points**:
  | 옵션 | 설명 | 장점 | 단점 |
  |---|---|---|---|
  | **A (★권장)** | Sprint 1+2 단순 Instantiate + Destroy → 5033.1 후속 cascade VfxPool | Sprint 단순 + 시각 검증 우선, 5033 ~1h15m 유지, gc_alloc_followup 마일스톤 정합 | 적 ~4000/챕터 처치 시 GC 누적 (~수백 KB) |
  | B | Sprint 1+2 부터 VfxPool.Register/Spawn API 추가 + element 등록 | GC 0 정착, 5029 VfxPool 답습 | 작업 시간 ↑ (~30분 추가), API 검증 필요, 5033 시각 검증 지연 |
  | C | A 진행 + Sprint 2 끝에 Profiler GC 측정 → 50KB/s 초과 시 자동 cascade | 데이터 기반 결정 | Sprint 일정 가변, 측정 환경 필요 |

- **Proposed Resolution Path**:
  Claude 권장: **A**. 5033 의 핵심 가치는 시각 통합 (placeholder → 실 prefab) — 사용자 시각 검증 우선. GC 우려는 5033.1 별도 sprint 로 분리하면 회수 용이. CLAUDE.md §11 `gc_alloc_followup` 마일스톤과도 정합 (5016.1 + 5017 누적 패턴 답습).

- **PRD Impact**:
  - § 9 후속 backlog 항목 명시 강화 (5033.1 VfxPool 정착).
  - Sprint 분해 변동 X.

---

### D3. init 패턴 — Bootstrap vs EnsureLoaded

- **Original Criticism** (critique § 4 D3):
  디자인 § 3.2 (`[RuntimeInitializeOnLoadMethod] Bootstrap`) ↔ PRD § 2.2 (`EnsureLoaded()` lazy) 불일치 — 디자인/PRD 통일 필요.

- **Current Position (PRD 그대로 진행 시 영향)**:
  Sprint 1 구현 시점에 어느 쪽 패턴 사용할지 모호. 디자인 따르면 모든 씬 진입 전 강제 로드, PRD 따르면 lazy.

- **Discussion Points**:
  | 옵션 | 설명 | 장점 | 단점 |
  |---|---|---|---|
  | A | `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] Bootstrap` (디자인 § 3.2 답습) | Scene 진입 전 1회 보장, 첫 호출 지연 X | 모든 씬에 강제 로드 (Attic 같은 마법사 비활성 씬도 4 prefab 로드) |
  | **B (★권장)** | `EnsureLoaded()` lazy + GetPrefab/Spawn 첫 호출 시 init (PRD § 2.2 답습) | 마법사 안 쓰는 씬은 로드 X, NUnit 테스트 시 명시 호출 가능 | 첫 호출 미세 지연 (Resources.Load 4건 ≈ 수 ms, 사용자 인지 X) |

- **Proposed Resolution Path**:
  Claude 권장: **B**. PRD § 2.2 가 정답 → 디자인 § 3.2 의 `Bootstrap` 메서드를 `EnsureLoaded()` 로 통일. NUnit 친화 + Attic 등 비마법사 씬 부담 X. Sprint 1 작업량 변동 X.

- **PRD Impact**:
  - 디자인 § 3.2 의 `[RuntimeInitializeOnLoadMethod]` Bootstrap 메서드 → `EnsureLoaded()` lazy 로 갱신.
  - Sprint PRD § 2.2 코드 그대로 (이미 정답).

---

## 요약

- **Total Items**: 3 (모두 Medium)
- **High**: 0 / **Medium**: 3 / **Low**: 0
- **Claude 권장**: D1=B (Editor 도구) / D2=A (Sprint cascade) / D3=B (EnsureLoaded lazy)
- **권장 mode**: **ai-assisted** — Claude 권장 응답 후 사용자 확인. 자연어 응답 (예 "권장 그대로", "D1=B, D2=A, D3=B") 모두 가능.
