# 5019 — Sprint Contract

## C1 컴파일
- 외부 script (Python) — Unity 영향 X
- TextureCompressionConfigurer 자동 적용 시 errors=0

## C2 NUnit
- 신규 NUnit 0 (sprite 영역, 코드 변경 없음)
- 누적 108 유지

## C3 Validator
- 5008 EnemySpriteAuditor 통과 (210 sprite healthy ≥ 80%)

## C4 Reflection
- 코드 영역 변경 0

## C5 SO 검증
- EnemyData SO 263 — icon path 변경 X (.meta GUID 보존)
- ChapterData V2 35 변경 X

## C6 Play 시나리오

### S1 — 5 챕터 spot check (T05 후)
- CH 1 (호숫가) Wave 1~5 → 호수 강아지 + 물고기 narrative 정합
- CH 22 (사탕 공장) → 사탕 인형 narrative 정합
- CH 28 (살아있는 정원) → 식물 정령 narrative 정합
- CH 33 (달의 뒷면) → 달 토끼 narrative 정합
- CH 35 (별과 별 사이) → 별 정령 narrative 정합

### S2 — Auditor 통과 (T04 후)
- 210 sprite healthy ≥ 80% (~170+)
- 시각 회귀 X (archetype baseline 30 변경 0)

## 합격 기준
- 자동 (C1~C5) ✅ + S2 healthy ≥ 80% ✅ → 최소
- + S1 사용자 시각 review 5 챕터 PASS → 완전

## 변경 이력
- 2026-05-06: 초기 작성
