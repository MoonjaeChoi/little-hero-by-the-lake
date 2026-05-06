# 5033 mage_element_vfx_integration PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 3건 결정 기록
> 모드: **ai-assisted** (Claude 권장 일괄 채택)
> 사용자 응답: "권장 그대로"
> 일시: 2026-05-06

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| Medium | D1 | prefab variant 생성 방법 | **B — Claude Editor 도구** | Done |
| Medium | D2 | GC 0 정착 시점 | **A — Sprint 1+2 단순 + 5033.1 후속 cascade** | Done |
| Medium | D3 | init 패턴 | **B — EnsureLoaded lazy** | Done |

---

## 2. Detailed Decisions

### 2.1 D1 — prefab variant 생성 방법

- **Decision**: **B (Claude Editor 도구 자동)**
- **Mode**: ai-assisted (Claude 권장 + 사용자 일괄 채택)
- **Rationale**:
  - CLAUDE.md §16 "Editor 메뉴 (Tools/LittleHero) — 콘텐츠 시드" 패턴 정합.
  - 결정론적 + 재현 가능 — Sprint 1 재실행 시 동일 결과.
  - 5033.1 후속 cascade (9 무기 매핑) 시 동일 도구 재사용 가능.
- **PRD Impact**:
  - § 2.1 "생성 방법" — 단일 옵션 (Editor 도구) 으로 정리.
  - **신규 산출물**: `Editor/SetupElementVfxVariants.cs` (~50L, `Tools/LittleHero/Setup Element VFX Variants` 메뉴).
  - Sprint 1 작업: prefab × 4 + Bootstrap + Editor 도구 신규 (총 commit 1 개 유지).
  - Sprint 1 시간: 30분 → ~35분 (미세 증가).
- **Trade-off**: Editor 도구 1 신규 (약 50L) — 일회성 사용자 수동 (옵션 A) 대비 작업량 +5분.

### 2.2 D2 — GC 0 정착 시점

- **Decision**: **A (Sprint 1+2 단순 + 5033.1 후속 cascade)**
- **Mode**: ai-assisted (Claude 권장 + 사용자 일괄 채택)
- **Rationale**:
  - 5033 의 핵심 가치는 **시각 통합** (placeholder → 실 prefab). 사용자 시각 검증 우선.
  - CLAUDE.md §11 `gc_alloc_followup` 마일스톤 답습 — 5016.1 + 5017 누적 패턴 (시각/기능 우선 → GC 후속 분리).
  - 회수 용이 — 5033.1 별도 sprint 분리하면 commit 단위 의미 명확.
- **PRD Impact**:
  - Sprint 분해 변동 X (~1h15m 유지).
  - § 9 후속 backlog 항목 5033.1 강화 (VfxPool Register/Spawn API).
  - § 8 위험 표 GC 항목 — "5033.1 후속 cascade" 명시.
- **Trade-off**: 적 ~4000/챕터 처치 시 GC 누적 (~수백 KB) 발생. 단, PIVOT v4 §7 GC <50KB/s 목표는 챕터 평균이므로 5033.1 까지 임시 허용 가능.

### 2.3 D3 — init 패턴 (Bootstrap vs EnsureLoaded)

- **Decision**: **B (EnsureLoaded lazy)**
- **Mode**: ai-assisted (Claude 권장 + 사용자 일괄 채택)
- **Rationale**:
  - PRD § 2.2 가 정답 — 디자인 § 3.2 의 `Bootstrap` 메서드를 `EnsureLoaded()` 로 통일.
  - NUnit 테스트 친화 — 명시 호출 가능 (`[RuntimeInitializeOnLoadMethod]` 은 Test 환경에서 자동 호출 X).
  - 마법사 안 쓰는 씬 (Attic, BossEncounter — non-mage 캐릭터) 은 Resources.Load 부담 X.
- **PRD Impact**:
  - 디자인 `docs/prd/5033_mage_element_vfx_integration_prd.md` § 3.2 의 `[RuntimeInitializeOnLoadMethod] Bootstrap` → `EnsureLoaded()` lazy 로 갱신.
  - Sprint PRD § 2.2 코드 그대로 (이미 정답).
  - Sprint 1 작업량 변동 X.
- **Trade-off**: 첫 호출 시 Resources.Load 4건 미세 지연 (≈ 수 ms, 사용자 인지 X) — Bootstrap 옵션 A 의 사전 로드 vs lazy.

---

## 3. 통계

- **Total Items**: 3
- **AI Recommendations Followed**: 3 / 3 (100%)
- **User Overrides**: 0
- **모드**: ai-assisted

---

## 4. v2_prd.md 변경 diff 요약

(rebuttal 단계 초안 → mediator 최종 의 변경사항)

| 위치 | 변경 |
|---|---|
| § 2.1 "생성 방법" | 3 옵션 병기 → 단일 옵션 (Editor 도구) 명시 |
| § 2.1 추가 | 신규 산출물 `Editor/SetupElementVfxVariants.cs` 명세 추가 |
| § 8 위험 표 GC 행 | "5033.1 후속 cascade — VfxPool Register/Spawn 정착, D2 결정 사안" → "5033.1 후속 cascade (D2=A 결정)" |
| § 9 5033.1 backlog | "D2=A 결정 시 정식 진행" → "5033.1 정식 진행 (VfxPool Register/Spawn API)" |
| § 10 미결 사항 | 항목 3 → "Mediator 결정 완료 (D1=B, D2=A, D3=B)" 로 갱신 |
| § 11 변경 이력 | "v2 mediator 확정 (D1=B, D2=A, D3=B)" 추가 |
| 상단 | "PRD v2 초안" → "PRD v2 (최종, Mediator 확정)" |

**별도 디자인 문서 갱신 (Phase 5 구현 시 일괄)**:

| 위치 | 변경 |
|---|---|
| 디자인 § 3.2 라인 77 | `[SerializeField]` 제거 (R1) |
| 디자인 § 3.2 클래스 상단 | `private const float SPAWN_LIFETIME_SECONDS = 3f;` 추가 (R2) |
| 디자인 § 3.2 메서드 | `Bootstrap()` → `EnsureLoaded()` lazy (D3=B) |
| 디자인 § 3.2 라인 102 | `Object.Destroy(instance, 3f)` → `SPAWN_LIFETIME_SECONDS` (R2) |
| 디자인 § 3.3 | "권장: prefab Variant" → "Editor 도구 `Tools/LittleHero/Setup Element VFX Variants` 자동 생성" (D1=B) |

---

## 5. 변경 이력
- 2026-05-06: 초기 작성 (Phase 2.3 Mediator). ai-assisted 권장 3/3 채택. v2 PRD 최종 확정.
