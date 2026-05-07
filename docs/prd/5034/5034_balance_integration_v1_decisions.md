# 5034 balance_integration_v1 PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 10건 결정 기록
> 모드: **ai-assisted** (Claude 권장 일괄 채택)
> 사용자 응답: "권장 그대로"
> 일시: 2026-05-07

## 1. 요약 표

| Priority | Item | Topic | Decision |
|---|---|---|---|
| Medium | D1 | 챕터 매트릭스 | **A** 균형 (1챕터당 +0.5⭐) |
| Medium | D2 | 슬롯 곡선 | **A** 유지 (⭐15=2배) |
| Medium | D3 | 무기 +N | **A** damage 곱셈 |
| Medium | D4 | 직업 시너지 | **B** 메이플 정합 (직업별 특수 스탯) ★ |
| Medium | D5 | DPS check | **B** 0.7× 도전 가능 |
| Medium | D6 | 캐시 진행 | **A** 50% 단축 |
| Medium | D7 | CurveSimulator | **A** 통합 |
| Medium | D8 | PlayerStats 가시성 | **B** Add/Remove 메서드 |
| Low | D9 | Sprint 4 분리 | **B** 2 commit (4a/4b) |
| Low | D10 | 5034.1 후속 | **B** 폐기 |

## 2. Detailed Decisions

### D1=A 챕터 매트릭스 균형
- BalanceConfig.recommendedLevelByChapter (N+9), recommendedWeaponEnhanceByChapter (~N×0.7), recommendedSlotStarforceByChapter (~N×0.6)
- 메이플 답습 점진적 진행

### D2=A 슬롯 곡선 유지
- StarforceMultiplierTable._baseMulPercent 5033.4 답습 그대로
- ⭐0=0% / ⭐15=100% / ⭐20=200%
- Sprint 2 작업: 검증만 (코드 변경 X)

### D3=A 무기 +N damage 곱셈
- 5033.4 답습 (DamageDealer.EnhanceDamage 의 Weapon 슬롯 multiplier 정합)
- 무기 +N 강화 시 attackConfig.damage 곱셈

### D4=B 직업 시너지 메이플 정합 ★
- PlayerStats 신규 3 필드: bossDamageBonus / critChanceBonus / aoeBonus
- EffectResolver 또는 새 ApplyClassBonus 로직:
  - 전사: STR +20%, 보스 데미지 +10%
  - 궁수: AGI +20%, 크리 확률 +10%
  - 마법사: WIS +20%, AOE 데미지 +15%

### D5=B DPS check 0.7×
- DPS_player ≥ DPS_target × 0.7 (도전 가능 영역)
- DpsCheckTests NUnit 검증

### D6=A 캐시 50% 단축
- 무료 35 챕터 ~30~40h, 캐시 50% 단축
- 별빛 ⋆ 캐시 → 가챠 + 마법의돌 조각

### D7=A CurveSimulator 통합
- 슬롯 + 무기 + affix 모두 통합 시뮬
- Sprint 4 (D9 결정 따라 4a/4b 분리)

### D8=B PlayerStats Add/Remove 메서드
- private float _bossDamageBonus
- public void AddBossDamageBonus(float v) / RemoveBossDamageBonus
- §15.2 / §15.7 / §15.11 정합

### D9=B Sprint 4 2 commit 분리
- Sprint 4a: CurveSimulator 슬롯 통합 (30분)
- Sprint 4b: CurveSimulator 무기 + affix 통합 (30분)
- Sprint 표 6 → 7 commit

### D10=B 5034.1 폐기
- V매트릭스 / 5차 스킬 (메이플 본판 답습) — 단일 캐릭터 게임 부적합
- 5034.2 시즌 패스 / 5034.3 직업 잠재 backlog 만 보존

## 3. 통계
- AI Recommendations Followed: 10/10 (100%)
- User Overrides: 0
- 모드: ai-assisted

## 4. v2_prd.md 변경 diff 요약

| 위치 | 변경 |
|---|---|
| Sprint 표 | 6 commit → 7 commit (Sprint 4 → 4a/4b) / 시간 ~3h 15m → ~3h 30m |
| §3 (Sprint 3) | PlayerStats 신규 필드 D8=B Add/Remove 메서드 패턴 명시 |
| §3.4 (직업 시너지) | D4=B 메이플 정합 수치 확정 (전사 STR+20%/보스+10% 등) |
| §5 Sprint 4 | 4a (슬롯 통합) + 4b (무기+affix 통합) 분리 |
| §9 후속 | 5034.1 V매트릭스 항목 삭제 (D10=B), 5034.2/5034.3 보존 |

## 5. 변경 이력
- 2026-05-07: 초기 작성 (Phase 2.3 Mediator). 권장 10/10 채택. v2 PRD 최종 확정.
