# 5014 — V5 sticker LoRA PRD 리뷰

## 0. 결정 이력
- 2026-05-05: prd-critiquer (Phase 2.1)

## 1. 종합 점수: 5.5 / 10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 10/10 | 자산/외부 작업 |
| PG-12 톤 | 10/10 | 무관 |
| 성능 | 10/10 | 외부 학습, 게임 영향 X |
| **시스템 통합** | **3/10** | 🚨 PRD §1.1 데이터 풀 outdated (66 → 459 sprite) |
| **Sprint 분해** | **4/10** | 🚨 Sprint 2 (학습) Claude Code 자동화 어려움, 외부 의존 |

## 2. 🚨 Critical 발견

### 2.1 학습 데이터 풀 outdated

**PRD §1.1**: "66 sprite" 명시
**실제 현황** (2026-05-05):
- 5008 archetype 30 + V1 11 + Daily 7 + Weekly 4 + Chapter override 210 (5009) + 기타 = **459 PNG** (Generated_v2/Enemies + Bosses 합)
- 5009/5010/5011 진행 후 학습 데이터 풀 약 **7배 증가**

ROI 영향:
- 데이터 풀 ↑ → 학습 sample 풍부 → over-fit 위험 ↓
- 다만 caption 작성 시간도 459건 (~3~4h 추가)

### 2.2 Sprint 2 (학습) Claude Code 자동화 어려움

**Claude Code 제약**:
- GPU 필요 (RTX 5070 12GB) — 서버 측 작업
- Kohya_ss UI 또는 sd-scripts CLI 직접 실행 — 사용자 또는 외부 자동화 도구 의존
- 학습 시간 2~4h — Claude Code 의 background task 가능하지만 ComfyUI 와 별개 환경

**Sprint 2 처리 옵션**:
- A: Sprint 2 = 사용자 외주 (Claude Code Phase 5 미진행)
- B: 서버 측 SSH command + script — Kohya 자동화
- C: 사용자 manual 학습 후 결과 파일 commit

### 2.3 ROI 평가 약함

**현 상태**:
- 5008 sprite_generation_rules (사용자 "아주 만족" baseline) 이미 안정
- 5009/5010/5011 모두 5008 baseline 으로 healthy 자산 생성 (실증)
- LoRA 도입 시 prompt 단순화 효과는 있으나 **결과 품질 변동 위험**

**ROI**:
| 항목 | 평가 |
|---|---|
| Prompt 단순화 (23+ → 5~10 token) | ○ 작은 효과 |
| 톤 일관성 ↑ | △ 5008 baseline 이미 안정 |
| 학습 비용 (~3~5h + 데이터 정리) | ⚠️ 큼 |
| Over-fit 위험 (특정 캐릭터 재현) | ⚠️ |
| 후속 sprite 작업 도움 | ✓ 신규 작업 시 prompt 부담 ↓ |

## 3. Accept 항목 (1)

- ✅ Sprint 분해 5 단계 합리적

## 4. Reject 항목 (0)

## 5. Discuss 항목 (3)

### 🔴 D1. 5014 진행 여부 (Critical)

**옵션**:
- A: **5014 RESERVED** — 5008 baseline 안정 + ROI 약함. 향후 신규 sprite 작업 누적 시 재진입
- B: **PRD 갱신 + 진행** — 데이터 풀 459 갱신 + Sprint 2 외주 명시
- C: **부분 진행** — Sprint 1 (데이터 정리) + Sprint 4~5 (인프라만, 학습 future)

### 🟡 D2. 학습 데이터 풀 (Medium)

**옵션**:
- A: 459 PNG 전체 — 학습 풍부, caption 작성 ~3~4h
- B: 30 archetype + 11 V1 + 11 boss = 52 PNG — V2 시스템 archetype 만 (5009 chapter override 제외)
- C: 5008 검증 30 archetype 만 — 가장 안정 baseline

### 🟡 D3. Sprint 2 학습 처리 (Medium)

**옵션**:
- A (★권장): 사용자 외주 — Sprint 2 = 사용자 manual 학습 후 결과 commit. Sprint 1 + 3+ 만 Claude Code 진행
- B: 서버 SSH automation — Kohya CLI script + 학습 trigger
- C: 학습 보류 — Sprint 1 (데이터) + Sprint 4 (workflow 인프라) 만, 학습 자체 미진행

## 6. 권장 변경

**즉시 (Accept)**:
- (없음 — 모두 사용자 결정 사안)

**Discuss 결정 후**:
- D1: 진행 여부
- D2: 학습 데이터 풀 범위
- D3: 학습 처리 방식

## 7. 변경 이력
- 2026-05-05: 초기 작성 (Phase 2.1 Critique).
