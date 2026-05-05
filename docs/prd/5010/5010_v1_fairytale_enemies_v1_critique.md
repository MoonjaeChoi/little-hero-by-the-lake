# 5010 — V1 Fairytale Enemies PRD 리뷰

## 0. 결정 이력
- 2026-05-05: prd-critiquer 자동 리뷰 (Phase 2.1)

## 1. 종합 점수: 6.5 / 10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 10/10 | 자산 작업 |
| PG-12 톤 | 9/10 | "no scary, friendly" 명시 ✅ |
| 성능 | 10/10 | 11 PNG, 영향 X |
| **시스템 통합** | **3/10** | 🚨 PRD §1.1 명단 불일치 (3 캐릭터 명단 잘못) |
| Sprint 분해 | 7/10 | 4 sprint 적정 |

## 2. 🚨 발견 — PRD §1.1 명단 정정 필수

### 2.1 PRD 명단 vs 실제 파일

PRD §1.1 명시 (잘못됨):
- Enemy_RedHood ❌ (실제 파일 X)
- Enemy_GoldenGoose ❌ (실제 파일 X)
- Enemy_Cheshire ❌ (실제 파일 X)
- Enemy_HeartQueen ❌ (실제 X — `Enemy_QueenHearts.png`)
- Enemy_WhiteRabbit ❌ (실제 X — `Enemy_Rabbit.png`)

**실제 V1 11 PNG**:
| 파일 | 캐릭터 | 동화 |
|---|---|---|
| Enemy_Basic | Slime | 공통 |
| Enemy_BeanGiant | Bean Giant | Beanstalk |
| Enemy_Bear | Bear | 공통 |
| Enemy_BigWolf | Big Wolf | Red Hood |
| Enemy_CardSoldier | Card Soldier | Alice |
| Enemy_CloudGiant | Cloud Giant | Beanstalk |
| Enemy_Firefly | Firefly | Beanstalk/공통 |
| Enemy_Pumpkin | Pumpkin | Halloween/공통 |
| Enemy_QueenHearts | Queen of Hearts | Alice |
| Enemy_Rabbit | Rabbit (White Rabbit) | Alice |
| Enemy_StarCloud | Star Cloud | Beanstalk |

### 2.2 기존 sprite 상태

11/11 metric **healthy** (모두 transparent 30~80% + rgb_std ≥ 25):

| 파일 | trans% | rgb_std | size | components |
|---|---:|---:|---|---:|
| Enemy_Basic | 57.9% | 80.9 | 512×512 | 1 |
| Enemy_BeanGiant | 66.8% | 85.8 | 512×512 | 1 |
| Enemy_Bear | 58.1% | 39.2 | 512×512 | 1 |
| Enemy_BigWolf | 56.9% | 104.0 | 512×512 | 1 |
| Enemy_CardSoldier | 66.9% | 94.7 | 512×512 | 1 |
| Enemy_CloudGiant | 34.9% | 71.2 | 512×512 | 27 |
| Enemy_Firefly | 75.3% | 31.1 | 512×512 | 87 |
| Enemy_Pumpkin | 71.6% | 68.0 | 512×512 | 12 |
| Enemy_QueenHearts | 62.0% | 109.3 | 512×512 | 1 |
| Enemy_Rabbit | 56.7% | 98.0 | 512×512 | 1 |
| Enemy_StarCloud | 46.6% | 69.1 | 512×512 | 24 |

**핵심 차이점** vs 5008 baseline (768×1024):
- 모든 V1 = **512×512** (legacy 사이즈)
- 일부 components 분산 (CloudGiant/Firefly/Pumpkin/StarCloud) — multi-element 또는 노이즈

### 2.3 재생성 ROI

**positive**:
- 5008 baseline (768×1024) 통일 → 일관 시각
- 동화 IP 명확화 (현 V1 일부 ambiguous)
- components 분산 정리

**negative**:
- 11 PNG 모두 healthy 상태 — 사용자 시각 review 없이 재생성 회귀 위험
- V1 = legacy/도감 회고 용도 (V2 시스템 미사용) — ROI 낮음

## 3. Accept 항목 (3)

- ✅ Sprint 분해 (4 sprint) 적정
- ✅ 옵션 B (3 batch — 동화별) 검수 단위 명확
- ✅ "no scary, friendly" PG-12 강조

## 4. Reject 항목 (0)

## 5. Discuss 항목 (3)

### 🔴 D1. PRD §1.1 명단 정정 (Critical)

**옵션**:
- A (★권장): PRD 명단 즉시 정정 (RedHood/GoldenGoose/Cheshire 제외, CloudGiant/Firefly/Pumpkin 포함)
- B: 명단 그대로 진행 (PRD 부정확 잔존 — 위험)

### 🟡 D2. 재생성 ROI (Medium)

기존 11 healthy. 재생성 정당화 약함.

**옵션**:
- A: 5011 D2=A+ 패턴 답습 — 사용자 시각 review "마음에 들지 않음" 시 재생성. 즉시 진행 보류.
- **B (★권장)**: 사이즈 통일 (512 → 768×1024) + 동화 IP 강조 prompt 로 재생성 (시각 일관성 ↑)
- C: 5010 RESERVED (V2 시스템 미사용 — ROI 낮음)

### 🟡 D3. components 분산 처리 (Medium)

CloudGiant 27 / Firefly 87 / Pumpkin 12 / StarCloud 24 — 분산 element 다수.

**옵션**:
- A (★권장): 재생성 시 자연스레 정리 (5009 prompt 단일 character 강조)
- B: 5011.4 connected components cleanup 후처리

## 6. 권장 변경

**즉시 (Accept)**:
- §1.1 명단 정정

**사용자 결정 (Discuss)**:
- D1: 정정 (즉시)
- D2: 재생성 (B 권장)
- D3: prompt 단일 character 강조 (A)

## 7. 변경 이력
- 2026-05-05: 초기 작성 (Phase 2.1 Critique).
