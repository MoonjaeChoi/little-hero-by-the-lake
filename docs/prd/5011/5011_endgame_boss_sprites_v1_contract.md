# 5011 Sprint Contract

> Phase 4 — 검증 기준 (D2=A+ / D3=A)

## C1 컴파일 — N/A (자산 작업, 코드 변경 0건)

## C2 Editor Test — N/A (NUnit 변경 X, 누적 그대로 유지)

## C3 ProjectValidator + Auditor

- `Tools > LittleHero > Validate Project` errors=0 (회귀)
- `Tools > LittleHero > Audit Enemy Sprite Quality`:
  - DailyBoss 7/7 + WeeklyBoss 4/4 healthy
  - transparent 30~80%
  - asym ≤ 50

## C4 Reflection — N/A

## C5 SO + 자산 metric

- Bosses/DailyBoss_{00~06}.png 7개 + WeeklyBoss_{00~03}.png 4개 모두 존재 + 갱신 (mtime 신규)
- 각 PNG: 768×1024, transparent 30~80%, rgb_std ≥ 30
- EnemyData SO 11개 icon GUID 보존 (변경 X)
- 시드 정합 (99000~99006 + 99100~99103)

## C6 Play 검증 시나리오

### S1 — Daily 보스 요일 톤 차별
1. Endgame 메뉴 → Daily 보스 진입 (월~일 sample)
2. 7 보스 시각 확인
- **검증**: 인형 실사판 회피 + V5 cartoon 톤 + 요일별 색상 차별

### S2 — Weekly 보스 cycle 톤
1. Weekly 보스 진입 (Light/Shadow/Life/Cosmic sample)
- **검증**: 4 cycle 차별성 + epic boss + PG-12

### S3 — Auditor
1. Tools/LittleHero/Audit Enemy Sprite Quality
- **검증**: 11 sprite healthy

## 합격 기준

- C3 Validator + Auditor PASS
- C5 11 PNG metric healthy
- C6 사용자 시각 review — "인형 실사판" 회피 확인
