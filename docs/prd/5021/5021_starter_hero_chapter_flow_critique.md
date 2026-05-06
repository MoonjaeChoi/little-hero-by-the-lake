# 5021 PRD 리뷰 (Critique)

> **검토자**: Claude (prd-critiquer Phase 2.1)
> **대상**: `5021_starter_hero_chapter_flow_prd.md` (디자인 + Sprint)
> **일시**: 2026-05-06

## 0. 결정 이력

| 일자 | 검토자 | 검토 |
|---|---|---|
| 2026-05-06 | Claude | 초기 — Phase 2.1 critique. Attic.unity WondrousMapNode 10+ 발견 → R3 옵션 재검토 권장 |

---

## 1. 종합 점수: 8.7/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9.0/10 | CharacterPool §15.4 SO 정합. MetaProgression +15L 은 5023 cascade 후 분리 예정 (일시 허용) |
| PG-12 톤 | 10/10 | 데이터 path 만 — 영향 0 |
| 성능 | 10/10 | Awake 1회 호출 추가만 — 0 영향 |
| 시스템 통합 | 8.5/10 | SaveData 호환 5020 cascade 정합. 그러나 R3 옵션 (AtticChapterEntry vs WondrousMapPanel) 재검토 필요 |
| Sprint 분해 | 9.0/10 | 4 Sprint 균형, 의존성 직렬 명확. Sprint 1 의 2 commit 분배 약간 모호 |

---

## 2. Accept 항목 (5)

### 2.1 ✅ R4 fix 옵션 2 (CharacterPool SO 인덱스) 채택
- §15.4 SO 우선 정합
- Build 호환 (Resources/AssetDatabase 의존성 없음)
- 5020 GUID 보존 정책 답습

### 2.2 ✅ NUnit 3 명시 (MetaProgressionInitTests)
- §15.9 핵심 정적 service 우선 테스트
- 5020 마이그레이션 + 5021 init 통합 검증 (`ResolveCharacter_AfterMigration_DefaultBoy_ResolvesToHeroWarrior`)

### 2.3 ✅ ProjectValidator ValidateCharacterPool 룰 추가
- §15.12 정합 — 신규 SO 검증 인프라

### 2.4 ✅ Sprint 의존성 그래프 직렬
- Sprint 1 SO → Sprint 2 init → Sprint 3 scene → Sprint 4 검증
- 직렬 의존성 명확, 병렬 가능 0 (CharacterPool SO 가 모든 후속의 baseline)

### 2.5 ✅ 알려진 위험 명시 (4건)
- AtticChapterEntry GameObject 미존재 시 결정 보류
- CharacterPool 충돌 grep 검증 명시
- Singleton race idempotent 보장
- Build 모드 SerializeField 검증

---

## 3. Reject 항목 (0)

(없음 — Reject 사유 없음)

---

## 4. Discuss 항목 (2)

### 4.1 🟡 D1 — R3 옵션 재검토 (AtticChapterEntry vs WondrousMapPanel)

**배경**: critique 단계에서 grep 으로 Attic.unity 에 `WondrousMapNode` 10+ instance 발견 (실제로는 35 모두 존재 추정 — 10 까지 본 후 head). PRD §3.1 의 "사전 조사" 가 critique 단계에서 부분 완료됨.

**해석**: AtticChapterEntry 는 **5005 attic_decomposition_v1 시 분리됐으나 실제로는 dead code 가능성 높음**. Chapter 진입 production path 는 **WondrousMapPanel** (35 노드).

**옵션**:
- **A** (PRD 원안): AtticChapterEntry._buttons[] wiring 복원 (legacy path 살림)
  - 장점: 단순 — 5020.2 패턴 답습
  - 단점: dead code 살리는 의미 — WondrousMapPanel 와 path 이중화
- **B** ★ 권장 (신규): AtticChapterEntry MonoBehaviour 비활성화 또는 GameObject 제거 + WondrousMapPanel 동작 확인 + 필요 시 그쪽 wiring 보강
  - 장점: production path 단일화, dead code 정리
  - 단점: WondrousMapPanel 의 35 노드 wiring 상태 검증 추가 필요
- **C**: 둘 다 살림 (entry 1개 = AtticChapterEntry 빠른 진입, 35 챕터 = WondrousMapPanel)
  - 장점: UX 옵션 ↑
  - 단점: 중복 wiring 비용

**Discuss**: 사용자 결정 — A / B / C / 추가 옵션.

### 4.2 🟡 D2 — Sprint 1 의 2 commit 세분화

**배경**: PRD Sprint 1 표 에 "CharacterPool SO 인프라" 가 2 commit 으로 표기 (commit 1 + 2 = 1/5 + 2/5). 그러나 §2 본문에는 1 commit 메시지 형식만 제시.

**해석**: 2 commit 분리 의도였으나 명세 불일치.

**옵션**:
- **A** (수정): 2 commit 명확화 → 1) `CharacterPool.cs` 클래스만, 2) `CharacterPool_Default.asset` SO + ProjectValidator 룰
- **B**: 1 commit 으로 통합 (Sprint 1 = 1 commit, 합계 4 commit)
- **C**: PRD 그대로 (2 commit 의도 보존, commit 메시지 1번째 = SO 클래스 / 2번째 = SO 인스턴스+Validator)

**Discuss**: 권장 C — 작은 분리는 회수 용이.

---

## 5. CLAUDE.md §15 정책 위반 점검

| §15 항목 | 상태 |
|---|---|
| §15.1 SRP | ✅ CharacterPool 단일 책임 (id → CharacterData lookup) |
| §15.1 OCP | ✅ switch 신규 0 |
| §15.4 SO 우선 | ✅ CharacterPool SO + 인스턴스 |
| §15.6 100줄 | ⚠️ MetaProgression 331L → +15L = 346L (이미 위반 상태, 5023 cascade 분리 예정) |
| §15.7 DI | ⚠️ Singleton 잔존, SerializeField partial DI |
| §15.9 Test | ✅ NUnit 3 신규 명시 |
| §15.11 안티 | ✅ public 필드 0, Update X, ObjectPool 무관 |
| §15.12 Validator | ✅ ValidateCharacterPool 룰 추가 |

**§15.6 위반 평가**:
- MetaProgression.cs 가 5023 에서 분리될 예정이므로 일시 +15L 허용
- 5021 → 5023 cascade 정합. 5023 PRD §4.1 Loader 가 5021 의 ResolveSelectedCharacterFromSave 흡수 명시됨 ✅

---

## 6. 권장 변경

### 즉시 변경 (Reject 없음 → 0)

(없음)

### 사용자 결정 후 변경 (Discuss)

- **D1 — R3 옵션**: 사용자 결정 (A / B / C) → Sprint 3 작업 항목 재정의
- **D2 — Sprint 1 commit 분배**: A / B / C (권장 C) → Sprint 1 commit 메시지 명확화

---

## 7. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 작성 (Phase 2.1 critique). 점수 8.7/10. Accept 5 / Reject 0 / Discuss 2 |
