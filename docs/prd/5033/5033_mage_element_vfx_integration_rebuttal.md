# 5033 mage_element_vfx_integration PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: 2026-05-06

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| §15.11 안티 | R1 `[SerializeField] static` 무효 | High | **Accepted** | 디자인 § 3.2 SerializeField 제거 → PRD § 2.2 와 통일 | ✅ |
| §15.11 안티 | R2 Magic Number `3f` Destroy hardcode | High | **Accepted** | `const SPAWN_LIFETIME_SECONDS = 3f` 분리 | ✅ |
| 디자인 옵션 | D1 prefab variant 생성 방법 (수동/Editor 도구/CopyAsset) | Medium | **Discuss** | discussion_topics → mediator | △ |
| 성능 | D2 GC 0 정착 시점 (Sprint 1+2 단순 vs VfxPool 즉시) | Medium | **Discuss** | discussion_topics → mediator | △ |
| 일관성 | D3 init 패턴 (Bootstrap vs EnsureLoaded lazy) | Medium | **Discuss** | discussion_topics → mediator (디자인 vs PRD 불일치 해결) | △ |
| Accept | A1 옵션 A 4 element 1 prefab 단순 매핑 | — | Confirmed | 변경 X | ✅ |
| Accept | A2 prefab variant 패턴 (Hovl 원본 보존) | — | Confirmed | 변경 X | ✅ |
| Accept | A3 3 Sprint 1h15m 균형 | — | Confirmed | 변경 X | ✅ |
| Accept | A4 검증 시나리오 4 충분 | — | Confirmed | 변경 X | ✅ |
| Accept | A5 위험 + 완화 5건 사전 명시 | — | Confirmed | 변경 X | ✅ |

---

## 2. 품질 메트릭

- 총 Discussion Points: **8** (Reject 2 + Discuss 3 + Accept 5)
- **Accepted (수정 반영)**: 2건 (25%) — R1, R2
- **Discuss (사용자 결정 필요)**: 3건 (37.5%) — D1, D2, D3
- **Confirmed (이미 PRD 반영)**: 5건 (62.5%) — A1~A5
- **Rejected**: 0건 (0%)
- **§15 정합률**: 8/10 (Reject 2 수용으로 9.5/10 회복 예상)

---

## 3. Detailed Responses

### 3.1 R1 — `[SerializeField] static` 안티 (§15.11) [High → Accepted]

**Original Criticism** (critique § 3 R1):
> 디자인 § 3.2 라인 77-81: `[SerializeField] private static GameObject _firePrefab;` — Unity SerializeField 는 instance 필드만 작동, **static 필드는 Inspector 노출 X / serialize X**. 디자인 ↔ Sprint PRD § 2.2 불일치.

- **Rebuttal**: **수용**. Unity 명세상 `[SerializeField]` 는 instance 필드 전용. static 에 붙여도 무효 → 코드 작동은 하지만 의미 없는 attribute (혼란 유발).
- **Proposed Action**:
  - 디자인 `docs/prd/5033_mage_element_vfx_integration_prd.md` § 3.2 라인 77 의 `[SerializeField]` 줄 제거.
  - 결과 코드:
    ```csharp
    private static GameObject _firePrefab;     // Resources.Load 로 lazy init
    private static GameObject _icePrefab;
    private static GameObject _lightningPrefab;
    private static GameObject _arcanePrefab;
    private static bool _initialized;
    ```
- **PRD Impact**: 디자인 § 3.2 코드 블록만 갱신. Sprint PRD § 2.2 는 이미 정답 (변경 X).
- **§15 인용**: §15.11 "private/protected 필드 `_camelCase`" + §15.4 "SerializeField + private 우선" — 단, **static 은 SerializeField 적용 X** (Unity 한계).

### 3.2 R2 — Magic Number `3f` Destroy hardcode (§15.11) [High → Accepted]

**Original Criticism** (critique § 3 R2):
> `Object.Destroy(instance, 3f)` 의 `3f` 의미 불명, hardcode → §15.11 "Magic Number 금지" 위반.

- **Rebuttal**: **수용**. `3f` 는 의미가 즉시 보이지 않고 튜닝 시 hardcode 변경 필요. const 또는 SerializeField (instance) 분리 필수.
- **Proposed Action**:
  - `ElementVfxBank` 클래스 상단에 const 추가:
    ```csharp
    private const float SPAWN_LIFETIME_SECONDS = 3f;
    ```
  - `Spawn` 메서드 내 `Destroy(instance, 3f)` → `Destroy(instance, SPAWN_LIFETIME_SECONDS)`.
  - 디자인 § 3.2 라인 102 + Sprint PRD § 3.1 라인 122 두 곳 모두 갱신.
- **PRD Impact**:
  - Sprint PRD § 3.1 코드 블록 갱신 (라인 116-124).
  - 디자인 § 3.2 코드 블록 갱신 (라인 91-103).
  - 5033.1 후속 cascade 시 element 별 차등 lifetime (Fire 2.5s / Ice 4s / Lightning 1.5s / Arcane 3.5s) 도 const → SO 분리로 자연 진화.
- **§15 인용**: §15.4 "Magic Number 금지: 모든 수치는 SO 또는 const" + §15.11 안티 패턴 표.

### 3.3 D1 — prefab variant 생성 방법 [Medium → Discuss]

**Original Criticism** (critique § 4 D1):
> PRD § 2.1 + 디자인 § 3.3 양쪽에 두 옵션 병기 — 사용자 수동 vs Editor 도구 자동 vs AssetDatabase.CopyAsset 일회성. 결정 필요.

- **Status**: **Discuss → discussion_topics.md**
- **이유**: 디자인 옵션 — 작업 결정론성 vs 사용자 시간 vs 재사용성 trade-off. 사용자 결정 사안.
- **Current Position (PRD 그대로 진행 시)**: 두 옵션이 § 2.1 에 모두 명시되어 있어 구현 시점에 어느 쪽으로 갈지 모호 → Sprint 1 시작 직전 결정 필요.
- **Resolution Path**: mediator 에서 사용자 응답 (예 "D1=B Editor 도구") 반영 → v2 PRD 의 § 2.1 를 단일 옵션으로 정리.

### 3.4 D2 — GC 0 정착 시점 [Medium → Discuss]

**Original Criticism** (critique § 4 D2):
> Sprint 1+2 단순 Instantiate + Destroy → GC 발생 (적 ~4000/챕터). 5033.1 후속 cascade 명시되어 있으나 즉시 정착 vs 후속 분리 결정 필요.

- **Status**: **Discuss → discussion_topics.md**
- **이유**: 일정 vs 성능 trade-off. PIVOT v4 §7 GC <50KB/s 목표 정합 여부.
- **Current Position (PRD 그대로 진행 시)**: 옵션 A (Sprint 1+2 단순 + 5033.1 후속) — 시각 통합 우선, GC 누적 5033.1 분리.
- **Resolution Path**: mediator 에서 결정 (A 권장).

### 3.5 D3 — init 패턴 (Bootstrap vs EnsureLoaded) [Medium → Discuss]

**Original Criticism** (critique § 4 D3):
> 디자인 § 3.2 는 `[RuntimeInitializeOnLoadMethod] Bootstrap`, PRD § 2.2 는 `EnsureLoaded()` lazy — 일관성 보정 필요.

- **Status**: **Discuss → discussion_topics.md**
- **이유**: 둘 다 §15.7 DI 정합 (명시 init). Scene 부담 vs 첫 호출 미세 지연 trade-off.
- **Current Position (PRD 그대로 진행 시)**: PRD § 2.2 가 정답 (`EnsureLoaded()` lazy) — 디자인 § 3.2 의 `Bootstrap` 은 갱신 필요.
- **Resolution Path**: mediator 결정 (B EnsureLoaded 권장 — Test 친화 + 비마법사 씬 부담 X).

### 3.6 A1~A5 — Confirmed (이미 PRD 반영)

| Item | Confirmation |
|---|---|
| A1 옵션 A 4 element 1 prefab | PRD § 1 명시, 변경 X |
| A2 prefab variant 패턴 | PRD § 2.1 + 디자인 § 3.3 명시, 변경 X |
| A3 3 Sprint 1h15m 균형 | PRD § 1 표 명시, 변경 X |
| A4 검증 시나리오 4 | PRD § 7 명시, 변경 X |
| A5 위험 + 완화 5건 | PRD § 8 + 디자인 § 5 명시, 변경 X |

---

## 4. PRD v2 적용 변경 요약

(R1, R2 만 반영. D1/D2/D3 는 mediator 결정 후 v2 최종에 반영.)

| 변경 위치 | 변경 내용 |
|---|---|
| 디자인 § 3.2 라인 77 | `[SerializeField]` 제거 (R1) |
| 디자인 § 3.2 라인 102 | `Object.Destroy(instance, 3f)` → `Object.Destroy(instance, SPAWN_LIFETIME_SECONDS)` (R2) |
| 디자인 § 3.2 클래스 상단 | `private const float SPAWN_LIFETIME_SECONDS = 3f;` 추가 (R2) |
| Sprint PRD § 2.2 클래스 상단 | `private const float SPAWN_LIFETIME_SECONDS = 3f;` 추가 (R2) |
| Sprint PRD § 3.1 라인 122 | `Object.Destroy(instance, 3f)` → `Object.Destroy(instance, SPAWN_LIFETIME_SECONDS)` (R2) |

---

## 5. 변경 이력
- 2026-05-06: 초기 작성 (Phase 2.2 Rebuttal). Reject 2 자동 수용 + Discuss 3 분리.
