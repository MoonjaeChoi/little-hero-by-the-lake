# 5011 — Endgame Boss Sprites PRD 리뷰

## 0. 결정 이력
- 2026-05-05: prd-critiquer 자동 리뷰 (Phase 2.1)

## 1. 종합 점수: 7.0 / 10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 10/10 | 자산 작업 — 코드 변경 X |
| PG-12 톤 | 9/10 | "no menace, friendly powerful" 명시 ✅ |
| 성능 (PIVOT v4 §7) | 10/10 | 11 PNG 자산만 — 영향 X |
| **시스템 통합** | **5/10** | ⚠️ PRD §1.1 위치 부정확 (Enemies/ → Bosses/) |
| Sprint 분해 | 8/10 | 3 sprint 적정. 다만 ROI 평가 약함 |

## 2. 🚨 발견 — PRD §1 정확성

### 2.1 위치 불일치

**PRD §1.1**:
```
Generated_v2/Enemies/DailyBoss_{00~06}.png
Generated_v2/Enemies/WeeklyBoss_{00~03}.png
```

**실제**:
```
Generated_v2/Bosses/DailyBoss_{00~06}.png  ← Bosses/ 폴더
Generated_v2/Bosses/WeeklyBoss_{00~03}.png
```

PRD 의 위치 명세 잘못됨. Sprint commit 메시지의 경로 표기도 정정 필요.

### 2.2 기존 sprite 상태

기존 11 PNG 모두 **healthy** (자동 metric 측정 결과):

| sprite | trans% | rgb_std | 판정 |
|---|---:|---:|---|
| DailyBoss_00 (월) | 53.2% | 52.3 | ✅ |
| DailyBoss_01 (화) | 47.3% | 47.2 | ✅ |
| DailyBoss_02 (수) | 62.0% | 51.6 | ✅ |
| DailyBoss_03 (목) | 58.8% | 53.0 | ✅ |
| DailyBoss_04 (금) | 43.6% | 47.9 | ✅ |
| DailyBoss_05 (토) | 51.5% | 48.5 | ✅ |
| DailyBoss_06 (일) | 56.0% | 53.5 | ✅ |
| WeeklyBoss_00 (Light) | 56.9% | 55.0 | ✅ |
| WeeklyBoss_01 (Shadow) | 46.2% | 54.7 | ✅ |
| WeeklyBoss_02 (Life) | 45.7% | 64.4 | ✅ |
| WeeklyBoss_03 (Cosmic) | 47.1% | 57.3 | ✅ |

**모두 768×1024 (5008 baseline 사이즈) + healthy 30~80% + rgb_std ≥ 25.**

즉 PRD 의 "재생성" 정당화가 약함:
- 기존 sprite 손상 X
- 5008 baseline 사이즈 정합
- 사용자 시각 review X 상태에서 "재생성" 만으로는 ROI 약함

### 2.3 갱신 가능 ROI

기존 sprite 생성 시점 prompt 추정 — 5008 baseline 적용 전 또는 단순 prompt. PRD §3 의 신규 prompt (요일/cycle 톤 prefix + epic boss + royal pose + filling frame) 적용 시 효과:
- ✅ 요일별 7 톤 차별성 ↑ (블루/레드/그린/퍼플/핑크/옐로/골드)
- ✅ Weekly 4 cycle 차별성 ↑ (Light/Shadow/Life/Cosmic)
- ✅ "epic boss + royal pose" — 기존 sprite 가 일반 monster 형태일 가능성 (사용자 검토 필요)
- ✅ PG-12 강화 ("no menace, friendly powerful")

다만 시각 회귀 위험:
- 기존 sprite 사용자 만족도 X 데이터 (없음)
- 갱신 시 EnemyData SO icon GUID 보존 (PNG 만 갱신)

## 3. Accept 항목 (3)

- ✅ Sprint 분해 적정 (3 sprint × commit, 약 1.5h)
- ✅ 옵션 B (Daily/Weekly batch 분리) review 명확
- ✅ PG-12 톤 보존 prompt ("no menace, friendly powerful, royal")

## 4. Reject 항목 (0)
- (PG-12 / §15 위반 없음)

## 5. Discuss 항목 (3)

### 🟡 D1. PRD §1 위치 정정 (Critical)

**옵션**:
- A (★권장): PRD §1 의 위치 표기 즉시 정정 (`Enemies/` → `Bosses/`) + Sprint commit 메시지 경로 표기 정정
- B: 보류 (PRD 부정확 잔존)

### 🟡 D2. 재생성 ROI — 사용자 시각 review 필요

기존 11 sprite 모두 metric healthy. "재생성" 정당화는 시각 review 필요.

**옵션**:
- A: PRD 그대로 진행 (5008 baseline + 신규 prompt = 시각 차별성 강화 가정)
- B (★권장): 사용자 시각 review 먼저 — 기존 sprite 만족도 확인 후 결정
- C: 부분 재생성 — 사용자 review 후 일부 (월/Light 등 representative 만)

### 🟡 D3. EnemyData SO icon wiring 영향

PNG 만 갱신 시 .meta GUID 보존 → EnemyData SO icon path 변경 X (5009 패턴). 다만 새 sprite 가 너무 다른 스타일이면 시각 회귀 가능.

**옵션**:
- A (★권장): GUID 보존 + 같은 PNG 경로 덮어쓰기 (SO 변경 X)
- B: 기존 PNG 백업 후 재생성 — revert 안전성 ↑

## 6. CLAUDE.md §15 정책 위반 점검

자산 작업 — 코드 변경 X. §15 무관.

## 7. 권장 변경

**즉시 변경 (Accept)**:
- PRD §1.1 위치 정정 (Bosses/ 명시)

**사용자 결정 후 변경 (Discuss)**:
- D1: 위치 정정 (즉시 권장)
- D2: 재생성 ROI — 사용자 시각 review 먼저
- D3: GUID 보존 정책 명시

## 8. 변경 이력
- 2026-05-05: 초기 작성 (Phase 2.1 Critique).
