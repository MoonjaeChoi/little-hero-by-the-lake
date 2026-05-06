# Sprint Contract: 5032

## C1 컴파일

errors=0 + CS0618 (Physics2D + FindObjectsSortMode) + CS0414 (_bossEncounterSceneName) warning 0

## C2 NUnit

171 회귀 0 (DamageQueryTests 6 시그니처 보존)

## C3 Validator

OOPPolicyRule warnings 변동 ≤ 0

## C4 Reflection

- DamageQuery: ContactFilter2D 정적 + EnsureFilter
- ProjectileAttack: 동일 패턴
- SceneRule: FindObjectsInactive.Exclude

## C5 SO 검증

영향 0

## C6 Play 시나리오

### 시나리오 1 — 컴파일
errors=0 + warning 4건 (CS0618 ×3 + CS0414 ×1) 모두 해소

### 시나리오 2 — Play Mode 데미지
CH1 진입 → 적 처치 → 콘솔 오류 0 + GC 변화 0

### 시나리오 3 — Validator
Tools/LittleHero/Validate Project → SceneRule (Cinemachine) 동작 정상

## 합격 기준

최소: C1+C3+C4 + 시나리오 1
완전: 모든 + 시나리오 2+3
