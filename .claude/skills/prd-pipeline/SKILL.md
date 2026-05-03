# PRD Pipeline — Little Hero 5 단계 자동 파이프라인

> **목적**: 사용자 요청 → 디자인 → PRD → 리뷰 → 구현 → 검증 자동.
> Clayve prd-pipeline 의 Unity 게임 개발 특화 변형 (PGE 패턴).

## 0. 5 단계 파이프라인

```
사용자 요청
   ↓
[Phase 1] Generator   → docs/little_hero/<system>_v1.md + _v1_prd.md
   ↓
[Phase 2] Critiquer   → <system>_v1_critique.md (Accept/Reject/Discuss)
   ↓ 사용자 Discuss 결정
[Phase 3] Mediator    → 결정 통합 + PRD 수정
   ↓
[Phase 4] Implement   → 코드 + SO + Sprint commit (12 commit 등)
   ↓
[Phase 5] Evaluate    → 컴파일 + Test + Validator + 보고
   ↓ FAIL 시
   └── 수정 → Phase 4 재진입
```

## 1. 입력

```bash
# 사용자 발화 패턴
"<system> 시스템 OOP 재설계"        # Phase 1~5 자동
"<system> Sprint N+M+...개 일괄 진행"  # Phase 4 직행 (PRD 있을 때)
"<system> 검증"                      # Phase 5 만
```

## 2. Phase 별 동작

### Phase 1 — Generator (prd-generator 스킬 호출)

**스킵 조건**:
- `docs/little_hero/<system>_v1_prd.md` 이미 존재 → 사용자 확인 후 스킵

**산출**:
- `<system>_v1.md` (디자인)
- `<system>_v1_prd.md` (Sprint PRD)

### Phase 2 — Critiquer (prd-critiquer 스킬 호출)

**스킵 조건**:
- `<system>_v1_critique.md` 이미 존재 + 변경 X → 스킵
- `--skip-review` 플래그

**산출**:
- `<system>_v1_critique.md` (5 카테고리 리뷰)

**사용자 인터랙션**:
- Discuss 항목 N개 발견 → 사용자 결정 요청
- Accept만 또는 Reject 0건 → 자동 진행

### Phase 3 — Mediator (사용자 결정 통합)

Discuss 항목별 사용자 결정:
- "Sprint 4 분리" → PRD 갱신 (1 commit → 2 commit)
- "옵션 변경" → 디자인 + PRD 수정
- "그대로 진행" → 변경 X

PRD 수정 시 `<system>_v1_prd.md` 갱신 + commit.

### Phase 4 — Implement (Sprint 일괄 진행)

PRD 의 Sprint 순차 실행:
1. Sprint N.M 작업 (코드 / SO / 문서)
2. 컴파일 검증 (read_console errors)
3. commit (PRD 의 commit 메시지 형식)
4. 다음 Sprint 진입
5. 모든 Sprint 완료

**의존성 그래프 따라 병렬 가능 표시**.

### Phase 5 — Evaluate (prd-evaluate 스킬 호출)

자동 검증:
- 컴파일 (errors=0)
- Editor Test (PASS)
- ProjectValidator (warnings 변동)

수동 검증 (사용자):
- Play 검증 시나리오 제시

판정 FAIL 시 → Phase 4 수정 사이클 (최대 2회).

## 3. changelog 자동 등재

각 Sprint 완료 시 `changelog-manager` 스킬 자동 호출:
- 직전 commit 분석
- 시스템 도입 / 사용자 결정 / 큰 fix 인지 판단
- `changelog_v1.md` 항목 추가
- CLAUDE.md §14 5건 maintain

전체 PRD 완료 시 — 묶음 changelog 항목 (예 "weapon_v2 후속 3 Sprint").

## 4. CLAUDE.md §11 마일스톤 갱신 (선택)

큰 시스템 도입 시:
- §11 마일스톤 행 추가 (예 "weapon_v2 — 직업별 분류 + 24 균형 ✅ 2026-05-03")

## 5. 사용 패턴 (사용자 발화)

| 발화 | Phase 진행 |
|---|---|
| "<system> OOP 재설계" | 1 → 2 → 3 → 4 → 5 |
| "<system> 디자인 + PRD" | 1 만 |
| "<system> PRD 리뷰" | 2 만 |
| "<system> Sprint X+Y 일괄" | 4 만 (PRD 있을 때) |
| "<system> 검증" | 5 만 |
| "<system> 후속 Sprint" | 1 (후속 PRD) → 4 → 5 |
| "/prd-pipeline <system>" | 1 → 2 → 3 → 4 → 5 |

## 6. 출력 보고

```
═══════════════════════════════════════
<system> Pipeline Complete
═══════════════════════════════════════

Phase 1 — Generator:
  ✓ 디자인: <system>_v1.md (X 라인)
  ✓ PRD: <system>_v1_prd.md (Y 라인, Z Sprint)

Phase 2 — Critiquer:
  ✓ 종합 점수: N/10
  ✓ Accept M / Reject 0 / Discuss N

Phase 3 — Mediator (해당 시):
  ✓ Discuss 결정 N건 → PRD 갱신

Phase 4 — Implement:
  ✓ Sprint 1: ✅
  ✓ Sprint 2: ✅
  ...
  ✓ commit X 생성 / Y시간

Phase 5 — Evaluate:
  ✓ 컴파일: errors=0
  ✓ Editor Test: M/M PASS
  ✓ ProjectValidator: warnings 변동 ±N
  ✓ Play 검증 시나리오: 사용자 보고 대기

종합: PASS

다음:
  - 사용자 Play 검증
  - changelog 등재 완료
  - /prd-generator <next system>
═══════════════════════════════════════
```

## 7. 에러 처리

| 상황 | 동작 |
|---|---|
| PRD 파일 없음 | Phase 1 자동 호출 |
| 디자인 + PRD 이미 존재 | 사용자 확인 후 스킵 또는 갱신 |
| Discuss 항목 미해결 | Phase 4 진입 안 함, 사용자 결정 대기 |
| 컴파일 에러 | Phase 4 수정 시도 (최대 2회), 그래도 실패 시 보고 |
| Test FAIL | Phase 5 수정 지시 → Phase 4 재진입 |
| Validator 신규 위반 | 보고 + 사용자 결정 (수정 / 보류) |

## 8. 원칙

- **사용자 결정 우선** — Discuss 항목 명시적 결정 후 진행
- **단일 책임 commit** — 한 commit = 한 의미 단위 (Sprint 분해)
- **OOP §15 정합** — 자동 검증 (Critiquer + Validator)
- **회귀 방지** — Editor Test + Play 검증 필수
- **CLAUDE.md changelog** — 핵심만 (5건 maintain), 자세한 이력은 changelog_v1.md
- **Unity MCP 안전 패턴** — refresh 사이 5~10s 대기, 도메인 리로드 회피

## 9. 4 스킬 통합

```
prd-generator   → Phase 1
prd-critiquer   → Phase 2
prd-evaluate    → Phase 5
prd-pipeline    → 본 스킬 (전체 통합)
changelog-manager → 자동 등재 (모든 Phase 후)
little-hero-dev   → Phase 4 작업 진행 (기존)
```

## 10. 출처

- Clayve `prd-pipeline.md` 5 단계 패턴
- CLAUDE.md §15 OOP 정책 + §14 changelog 분리
- 직전 PRD 진행 사례 (`weapon_system_v2_prd` 12 commit + `weapon_v2_followup_prd` 3 commit)
- `prd-generator` / `prd-critiquer` / `prd-evaluate` 스킬 (본 PRD 와 동시 작성)
