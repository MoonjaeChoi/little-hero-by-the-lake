# PRD Critiquer — Little Hero PRD 리뷰

> **목적**: PRD 작성 후 CLAUDE.md §15 OOP 정책 정합 + Unity 안티 패턴 + 게임 디자인 일관성 검증.
> Clayve prd-critiquer 의 Unity 게임 개발 특화 변형.
>
> **저장 위치 (5001/5002 정착 패턴)**:
> - 입력 PRD: `docs/prd/<번호>/<system>_v1_prd.md` (mediator 단계 v2 도 같은 폴더)
> - 산출 critique: `docs/prd/<번호>/<system>_v1_critique.md`
> - 번호는 시스템 이름 prefix 4자리 숫자 자동 추출 (예 `5003_X` → `5003`). 폴더 미존재 시 `mkdir -p` 자동 생성.
> - 일반 참조 문서 (changelog / architecture_analysis 등) 는 `docs/little_hero/` 그대로

## 0. 입력

- PRD 파일 경로 (예 `docs/prd/<번호>/<system>_v1_prd.md`)
- 또는 시스템 이름 (자동 매칭 — 번호 폴더 자동 탐색)

## 1. 워크플로 (4 단계)

### Step 1 — 컨텍스트 로드

병렬 Read:
- 대상 PRD + 디자인 문서 (`<system>_v1.md`)
- `CLAUDE.md` §1·§5·§15 (정책 핵심)
- `docs/little_hero/architecture_analysis_v1.md` (현재 시스템 평가)
- `docs/little_hero/changelog_v1.md` 마지막 5건 (직전 변경)

### Step 2 — 검증 항목 (5 카테고리)

#### 2.1 OOP §15 정합 (필수)

| 정책 | 검증 |
|---|---|
| §15.1 SRP | Sprint 분해에 1 클래스 = 1 책임 명시? |
| §15.1 OCP | switch 신규 시 Factory/Strategy 활용? |
| §15.4 SO 우선 | 신규 데이터는 SO? Magic Number 0? |
| §15.6 100줄 | 신규 클래스 100줄 이내? |
| §15.7 DI | 신규 클래스 Initialize 명시? Singleton 직접 참조 X? |
| §15.9 Test | 핵심 service 단위 테스트 명시? |
| §15.11 안티 | public 필드·Update in N·string concat 회피? |

#### 2.2 PG-12 톤 (CLAUDE.md §2)

- 잔인·피·끔찍 영역 X
- 처치 표현 — 별 흩어짐·만화풍 OK
- 게임 어휘 (DPS·치명타·드랍률 등) 적절

#### 2.3 성능 목표 (§7)

- 동시 적 cap 300 (PIVOT v4)
- GC <50KB/s
- 신규 시스템이 cap·GC 영향 검토?

#### 2.4 시스템 통합

- stats_v2 (3 코어) 정합?
- weapon_v2 (직업 분류) 정합?
- 가챠/한정 상점/도감 영향?
- SaveData 호환 (마이그레이션 명시)?

#### 2.5 Sprint 분해 적절성

- 각 Sprint 작업량 균형
- 의존성 그래프 명확
- commit 단위 의미 단위
- 검증 시나리오 충분 (3+ Play 검증)

### Step 3 — 결정 분류

각 발견 항목:
- **Accept ✅** — PRD 그대로 진행 가능
- **Reject ❌** — 필수 수정 (예 PG-12 위반, 100줄+ 신규)
- **Discuss 🟡** — 사용자 결정 필요 (옵션 변경 / 보류 / 단계 조정)

### Step 4 — 리뷰 보고서 작성

`docs/prd/<번호>/<system>_v1_critique.md`:

```markdown
# <system> PRD 리뷰

## 0. 결정 이력
- YYYY-MM-DD: <검토자> 리뷰

## 1. 종합 점수: N/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | ... |
| PG-12 톤 | 10/10 | ... |
| 성능 | 8/10 | ⚠️ <영역> |
| 시스템 통합 | 9/10 | ... |
| Sprint 분해 | 9/10 | ... |

## 2. Accept 항목 (N)
- ✅ ... (자세한 사유)

## 3. Reject 항목 (N)
- ❌ ... (필수 수정 + 대안)

## 4. Discuss 항목 (N)
- 🟡 ... (사용자 결정 필요 + 옵션 A/B/C)

## 5. CLAUDE.md §15 정책 위반 점검
- §15.X: ... (위반 시) / 통과 (위반 X)

## 6. 권장 변경
- 즉시 변경: ...
- 사용자 결정 후 변경: ...

## 7. 변경 이력
- YYYY-MM-DD: 초기 작성
```

## 2. 사용 패턴

| 사용자 발화 | 동작 |
|---|---|
| "PRD 리뷰" | 직전 PRD 자동 매칭 + 리뷰 |
| "<system> 검증" | 시스템 매칭 후 PRD + 디자인 리뷰 |
| "OOP 정합 검사" | §15 정책만 빠른 검증 |

## 3. 검증 자동화

### 3.1 100줄 룰 (정적 검증)

PRD 의 신규 클래스 명세:
- 메서드 수 × 평균 라인 수 = 추정 라인
- 100줄+ 시 분리 권장

### 3.2 SOLID-O switch 검출

PRD 텍스트에 "switch" 또는 "분기" 등장 시:
- Factory/Strategy 패턴 활용 권장

### 3.3 안티 패턴 텍스트 검색

PRD 에서 다음 키워드 발견 시 경고:
- "public float damage" → SerializeField + private 권장
- "Update in 1500" → 단일 Manager Update 권장
- "Instantiate in" → ObjectPool 권장
- "GameObject.Find" → Awake 1회 캐시 권장

## 4. 출력 보고

```
✅ <system> PRD 리뷰 완료
종합 점수: 9/10

Accept (3):
  - ✅ 옵션 B 채택 (메이플 식 직업 분류)
  - ✅ Sprint 의존성 그래프 명확
  - ✅ Test 13 NUnit 명시

Discuss (1):
  - 🟡 Sprint 4 UI 작업량 (60분) 큼 — 분리 권장 검토

Reject (0):
  (없음)

다음:
  - PRD 그대로 진행 → /prd-pipeline
  - Discuss 항목 사용자 결정 후 진행
  - critique.md: docs/prd/<번호>/<system>_v1_critique.md
```

## 5. 출처

- Clayve `prd-critiquer.md` 5 카테고리 패턴
- CLAUDE.md §15 OOP 정책
- `architecture_analysis_v1.md` 8/10 평가 패턴
