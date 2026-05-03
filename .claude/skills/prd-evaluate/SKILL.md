# PRD Evaluate — Little Hero 구현 후 검증

> **목적**: PRD 의 Sprint 구현 완료 후 자동 검증. 컴파일 / Editor Test / ProjectValidator / Play 검증.
> Clayve prd-evaluate 의 Unity 게임 개발 특화 변형.

## 0. 입력

- PRD 파일 경로 (검증 기준 추출)
- 또는 직전 Sprint 의 commit hash

## 1. 워크플로 (5 단계)

### Step 1 — 컴파일 검증

```
mcp__UnityMCP__refresh_unity
mcp__UnityMCP__read_console (errors)
```

판정:
- ✅ errors=0 → 통과
- ❌ errors>0 → FAIL + 에러 list 출력 + 수정 지시

### Step 2 — Editor Test (NUnit) 실행

```
Window > General > Test Runner > Edit Mode > Run All
```

또는 reflection 으로 Test 클래스 검증:
- `Assembly-CSharp-Editor` 의 Test 클래스 존재 확인
- 신규 PRD 의 Test 명시 (예 WeaponSystemTests) 검증

판정:
- ✅ 모든 Test PASS → 통과
- ❌ FAIL Test 존재 → 수정 지시

### Step 3 — ProjectValidator 실행

```
mcp__UnityMCP__execute_menu_item: Tools/LittleHero/Validate Project
mcp__UnityMCP__read_console (warnings)
```

판정:
- ✅ errors=0, warnings ≤ 기존 → 통과
- ⚠️ 신규 warnings → PRD 위반 가능 (보고)
- ❌ errors>0 → FAIL

### Step 4 — Play 검증 시나리오 (PRD §6.4)

PRD 의 검증 시나리오를 사용자에게 제시:

```
[Play 검증 가이드]

시나리오 1: <제목>
1. <단계>
2. <기대 결과>
3. 콘솔 로그 확인: "<로그 내용>"

시나리오 2: ...

사용자가 Play 검증 후 결과 보고 부탁드립니다.
```

### Step 5 — 검증 보고서 작성

`docs/little_hero/<system>_v1_evaluation.md`:

```markdown
# <system> 구현 검증

## 0. 검증 일시
- YYYY-MM-DD HH:MM

## 1. 종합 결과: PASS / FAIL

## 2. 자동 검증

### 2.1 컴파일
- errors: 0 ✅
- warnings: N

### 2.2 Editor Test (NUnit)
- 통과: N/M
- 실패: list

### 2.3 ProjectValidator
- errors: 0
- warnings: N (이전 M → 변동 ±X)
- 신규 위반: list

## 3. 수동 Play 검증 (사용자 보고)
- 시나리오 1: PASS / FAIL (사용자 보고)
- 시나리오 2: ...

## 4. PRD 검증 기준 매트릭스

| Sprint | 검증 기준 | 결과 |
|---|---|---|
| 1 | <기준> | ✅ |
| 2 | ... | ✅ |
...

## 5. 알려진 문제 (있으면)
- ⚠️ <문제> + 임시 회피 / 후속 작업

## 6. 다음 단계
- ✅ PASS — changelog 등재 + 다음 PRD
- ❌ FAIL — 수정 사이클 (Sprint 재실행)

## 7. 변경 이력
```

## 2. 사용 패턴

| 사용자 발화 | 동작 |
|---|---|
| "구현 검증" | Step 1~5 자동 |
| "<system> 검증" | 시스템 매칭 후 검증 |
| "컴파일만" | Step 1 만 |
| "Test 실행" | Step 2 만 |
| "Validator" | Step 3 만 |

## 3. 자동 검증 도구

### 3.1 Reflection 검증

신규 클래스 / 메서드 존재:
```csharp
var t = System.Type.GetType("LittleHero.X, Assembly-CSharp");
return t != null && t.GetMethod("Y") != null;
```

### 3.2 SO 검증

신규 SO 의 필드 wiring:
```csharp
var so = AssetDatabase.LoadAssetAtPath<X>(path);
return so != null && so.field != null;
```

### 3.3 시뮬레이션 검증 (CurveSimulator)

PRD 명시 시:
- `Tools > LittleHero > Curve Simulator`
- `Tools > LittleHero > Weapon Gacha 24 Simulation`
- 결과 markdown 생성 + 비교

## 4. 출력 보고

```
═══════════════════════════════════════
<system> 구현 검증 결과
═══════════════════════════════════════

자동 검증:
  ✅ 컴파일: errors=0
  ✅ Editor Test: 13/13 PASS
  ⚠️ ProjectValidator: warnings 21 (이전 18, +3)

수동 검증 (Play):
  사용자 보고 부탁드립니다.

PRD 검증 기준:
  Sprint 1: ✅ enum 2종 + Template 확장 + IWeapon
  Sprint 2: ✅ 18 SO 마이그레이션 (Warrior 4 / Archer 7 / Mage 7)
  Sprint 3: ✅ EquipmentManager 검증 + WeaponClassBonus
  Sprint 4: ✅ UI 직업 표시
  Sprint 5: ✅ Test + Validator
  Sprint 6: ✅ Gacha 직업 필터
  Sprint 7: ✅ 24 무기 균형 (8/8/8)

종합: PASS

다음:
  - changelog_v1.md 등재 (changelog-manager 자동)
  - 다음 PRD: /prd-generator <next system>
  - 또는 Play 검증 시나리오 진행
═══════════════════════════════════════
```

## 5. FAIL 시 처리

판정 FAIL → PRD 의 Sprint 수정 지시:

```
❌ Sprint N 수정 필요

[문제]
  - <에러 또는 위반 사항>

[수정 지시]
  1. <파일>:<라인>
  2. <코드 변경>
  3. <재검증>

수정 후 다시 검증 → /prd-evaluate
```

## 6. 출처

- Clayve `prd-evaluate.md` 5 단계 패턴
- CLAUDE.md §15 정책
- ProjectValidator (Editor) — Unity 자동 점검
- Editor Test Runner — NUnit
