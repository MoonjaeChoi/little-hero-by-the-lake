# PRD Generator — Little Hero 디자인 + Sprint PRD 자동 작성

> **목적**: 사용자 요청을 받아 `docs/prd/` 폴더에 **디자인 문서 + Sprint PRD** 2 종을 자동 작성한다.
> Clayve의 prd-generator를 Unity 게임 개발 환경에 맞게 재구성.
>
> **저장 위치 + 명명 규칙 (2026-05-05 갱신 — 사용자 결정)**:
>
> ### 파일명 형식 (필수)
>
> **모든 prd-generator 산출물은 `<문서번호>_<내용>_prd.md` 형식 강제**.
>
> | 파일 | 위치 | 명명 |
> |---|---|---|
> | 디자인 (시스템 색인) | `docs/prd/` 루트 | `<문서번호>_<내용>_prd.md` |
> | Sprint PRD | `docs/prd/<문서번호>/` 하위 | `<문서번호>_<내용>_prd.md` (동일 파일명, 경로로 구분) |
>
> ### 명명 규칙
>
> - **문서번호**: 4자리 (예: `5008`, `5009`)
> - **내용**: snake_case 영문 — 시스템/feature 명칭만 (예: `enemy_visual_pipeline`, `attic_decomposition`, `litmotion_migration`)
> - **suffix**: `_prd.md` 고정
> - 🚫 **`_v1` / `_v2` 등 버전 표기 자제** (2026-05-05 사용자 결정 — 파일명 단순화). 버전 진화는 commit history + 문서번호로 추적. 새 버전이 필요하면 신규 문서번호 사용 (예: `5001_boss_system` → 후속은 `5040_boss_system_overhaul` 등).
>
> ### 예시
>
> ```
> ✅ 5008_enemy_visual_pipeline_prd.md
> ✅ 5009_mpb_hue_shift_prd.md
> ✅ 5016_litmotion_migration_prd.md
> ❌ 5008_enemy_visual_pipeline_v2_prd.md  (v2 표기 자제)
> ❌ 5009_mpb_hue_shift_v1_prd.md          (v1 표기 자제)
> ❌ 5008_enemy_visual_pipeline.md         (suffix 누락)
> ❌ enemy_visual_pipeline_prd.md          (번호 누락)
> ```
>
> ### 후속 산출 (다른 prd-* 스킬 — critique/rebuttal/decisions 등)
>
> - `<문서번호>_<내용>_critique.md`
> - `<문서번호>_<내용>_rebuttal.md`
> - `<문서번호>_<내용>_decisions.md`
> - `<문서번호>_<내용>_v2.md` (mediator 최종 — `v2` 는 PGE 단계 표기, 버전 표기 X)
> - `<문서번호>_<내용>_breakdown_NN.md`
> - `<문서번호>_<내용>_contract.md`
> - `<문서번호>_<내용>_evaluation.md`
>
> ### 마이그레이션 정책
>
> - 기존 5001~5015 파일은 **유지** (rename 시 commit churn 발생, history 보존 우선)
> - **5016+** 부터 신규 명명 규칙 적용 (`_v1` 표기 자제, snake_case 시스템명 만)
> - 신규 PRD 작성 시 자동 적용. 기존 파일 rename 은 사용자 명시 시에만 (선택)
> - changelog_v1.md / architecture_analysis_v1.md 등 일반 문서는 `docs/little_hero/` 그대로 유지 (PRD 외 일반 docs 의 v1 표기는 별도 규칙)

## 0. 단일 진실원

| 파일 | 역할 |
|---|---|
| `CLAUDE.md` §15 OOP 정책 | 신규 코드 작성 시 우선 (SOLID·SO·GC·100줄) |
| `docs/prd/<문서번호>_<내용>_prd.md` | 디자인 문서 (분석·옵션·결정) — 시스템 색인 (prd/ 루트) |
| `docs/prd/<문서번호>/<문서번호>_<내용>_prd.md` | Sprint PRD (작업 명세·검증 기준) — 동일 파일명, 경로로 구분 |
| `docs/little_hero/changelog_v1.md` | 변경 이력 (changelog-manager 자동, 위치 그대로) |

## 1. 입력

사용자 발화에서 다음 추출:
- **문서번호**: 4자리 (자동 — 직전 PRD 번호 + 1, 또는 사용자 명시. 예 `5009`)
- **내용설명**: snake_case 영문 + 버전 표기 (예 `mpb_hue_shift_v1`, `weapon_system_v2`)
- **사용자 결정**: 옵션 X/Y/Z 채택 또는 미결정 (디자인 문서에서 옵션 제시)
- **참조 시스템**: 기존 `stats_v2_infinite.md` / `gacha_v1.md` 등

**최종 파일명**: `<문서번호>_<내용>_prd.md` (예: `5009_mpb_hue_shift_v1_prd.md`)

## 2. 워크플로 (3 단계)

### Step 1 — 분석 + 디자인 문서

`docs/prd/<문서번호>_<내용>_prd.md` 작성:
1. 결정 이력 (날짜·사용자 결정)
2. 현재 시스템 분석 (코드 grep + SO 분포 + 위반 사항)
3. 옵션 비교 표 (장점/단점/권장)
4. 권장 설계 (CLAUDE.md §15 정합)
5. 신규 파일 명세
6. 마이그레이션 단계 (Sprint 분해)
7. 알려진 위험 + 완화
8. 변경 이력
9. Sources (웹 조사 시)

### Step 2 — Sprint PRD 작성

`docs/prd/<문서번호>/<문서번호>_<내용>_prd.md` 작성:
1. 전체 개요 (Sprint × commit × 시간 표)
2. 각 Sprint 명세:
   - 작업 항목 (파일별)
   - 인풋 (참조 SO / 디자인 §)
   - 산출물 (코드 / SO / 문서)
   - 검증 기준
   - 완료 조건
   - commit 메시지 형식
3. 의존성 그래프
4. 완료 정의 (Definition of Done)
5. 검증 시나리오 (Play 검증)
6. 알려진 위험 + 완화

### Step 3 — 사용자 검토 요청

```
✅ 디자인 문서 완료: docs/prd/<문서번호>_<내용>_prd.md
✅ Sprint PRD 완료: docs/prd/<문서번호>/<문서번호>_<내용>_prd.md

[Sprint 요약]
  Sprint 1: <작업> (N commit, M분)
  Sprint 2: ...
  총 X commit, Y시간

결정 요청:
  - X commit 일괄 진행
  - Sprint 1만 먼저
  - PRD 추가 조정
```

## 3. CLAUDE.md §15 OOP 정책 정합 (필수 검증)

PRD 작성 시 다음 정책 자동 검증:

| 정책 | 검증 |
|---|---|
| §15.1 SOLID-S | 100줄+ 클래스 분리 명시 |
| §15.1 SOLID-O | switch → Strategy/Factory 변환 |
| §15.4 SO 우선 | 마법 숫자 → SO 또는 const |
| §15.5 EventBus | 정적 GameEvents 또는 SO Channel |
| §15.6 100줄 룰 | 신규 클래스 100줄 이내 권장 |
| §15.7 DI | Initialize 명시 주입 |
| §15.9 Test | 핵심 정적 service 단위 테스트 명시 |
| §15.11 안티 패턴 | public 필드·Update in 1500·string concat 회피 |
| §15.12 Validator | ProjectValidator OOP 룰 추가 권장 |

## 4. Sprint 분해 표준

각 Sprint 는 **commit 단위 작은 의미**:

| 유형 | 예시 |
|---|---|
| 인프라 (1~3 commit) | enum + interface + base 클래스 신규 |
| SO 마이그레이션 (1 commit) | Editor 메뉴 + N SO 일괄 갱신 |
| 검증 + Bonus (1~2 commit) | Manager 진입부 검증 + 정적 헬퍼 |
| UI (1~2 commit) | Panel + Card + 라벨 |
| Test + Validator (1 commit) | NUnit Test 5+ + ProjectValidator 룰 |
| 콘텐츠 (1 commit) | 신규 SO N 종 + 분포 검증 |

## 5. 디자인 문서 형식 (자세히)

### 5.1 §1 현재 시스템 분석

코드 grep 으로 자동 수집:
```bash
grep -rn "<관련 클래스>" Scripts/
find Data/ -name "*.asset" -exec grep -l "<필드>" {} \;
```

표 형식 출력:
- 현재 N 개 SO + M 라인 클래스
- 위반 사항 (100줄+ 클래스 / public 필드 / 직접 참조)

### 5.2 §2 옵션 비교 (필수)

```markdown
| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | ... | ... | ... |
| B (★권장) | ... | ... | ... |
| C | ... | ... | ... |
```

### 5.3 §3 권장 설계 (CLAUDE.md §15 정합)

- 신규 파일 명세 (위치 + 클래스명 + 메서드)
- enum / interface / 정적 헬퍼
- 기존 시스템 hook (메서드 갱신)

### 5.4 §4 Sprint 분해

각 Sprint:
- 작업 항목
- 산출물 (코드/SO/문서)
- 검증 기준
- 완료 조건

## 6. PRD 형식 (자세히)

### 6.1 전체 개요 표

```markdown
| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | <작업> | N | M분 |
| ...
| 총 | | X | Y시간 |
```

### 6.2 각 Sprint 세부

```markdown
### Sprint N — <제목> (N commit)

#### N.1 <작업>

**파일**: `<경로>`

**산출물**:
- 클래스명 (또는 enum/interface)
- 메서드/필드 명세

**검증 기준**:
- 컴파일 OK
- 기존 회귀 X
- 특정 동작 검증

**완료 조건**:
- ...

#### Sprint N commit

```
<제목> — <Sprint 명> (N/M)

[변경]
  ...

[검증]
  ...

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```
```

### 6.3 의존성 그래프

```
Sprint 1 → Sprint 2 → Sprint 3 ┐
                              ├→ Sprint 5
              Sprint 4 ────────┘
```

병렬 가능 표시.

### 6.4 검증 시나리오 (Play 검증)

각 시나리오:
1. 사용자 작업 단계
2. 기대 결과
3. 콘솔 로그 / SaveData 검증

## 7. 사용 패턴 (사용자 발화 → 동작)

| 사용자 발화 | 동작 |
|---|---|
| "X 시스템 OOP 재설계" | Step 1 (분석 + 디자인 문서) |
| "Sprint PRD 작성" | Step 2 (PRD 만, 디자인은 기존) |
| "옵션 X 권장 + PRD" | Step 1+2 (옵션 X 채택 + 디자인 + PRD) |
| "<문서번호>_<내용설명> 디자인 갱신" | 기존 디자인 문서 갱신 |

## 8. 자동 트리거

다음 사용자 발화 패턴 시 자동 호출 권장:
- "PRD 작성"
- "<시스템> 디자인 + Sprint 분해"
- "<feature> OOP 재설계"

## 9. 출력 형식 (보고)

```
✅ 디자인 문서: docs/prd/<문서번호>_<내용>_prd.md (X 라인)
✅ Sprint PRD: docs/prd/<문서번호>/<문서번호>_<내용>_prd.md (Y 라인)

[디자인 핵심]
  - 옵션 N 비교 (★권장: B)
  - 신규 파일 M 종
  - CLAUDE.md §15 정합 검증

[Sprint 요약]
  Sprint 1: <작업> (N commit, M분)
  ...
  총 X commit, Y시간

[검증 시나리오]
  1. <Play 검증>
  ...

다음 단계:
  - X commit 일괄 진행 → /prd-pipeline
  - Sprint 1 먼저 → /prd-pipeline --to=1
  - PRD 추가 조정 (사용자 명시)
```

## 10. 출처

- Clayve `prd-generator.md` 패턴
- CLAUDE.md §15 OOP 정책
- `docs/little_hero/` 일반 디자인 문서 (PRD 산출 외 참조용 — `stats_v2_infinite.md`, `weapon_system_v2.md` 등)
- `docs/prd/5001_*` / `docs/prd/5002_*` 정착 패턴 (디자인 1개 prd/ 루트 + 산출 다수 `<번호>/` 하위)
- `weapon_system_v2_prd.md` 형식 (Sprint × commit × 시간 표 + 의존성 그래프)
