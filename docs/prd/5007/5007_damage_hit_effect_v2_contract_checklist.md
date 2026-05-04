# Evaluator Checklist: 5007 damage_hit_effect_v2

## 자동 검증

### C1 컴파일
- [ ] refresh_unity → errors=0
- [ ] isCompiling=false

### C2 Editor Test
- [ ] HitEffectV2Tests 8/8 PASS
- [ ] 누적 NUnit 73 → 81

### C3 Validator
- [ ] errors=0
- [ ] §15.6 LINE_LIMIT 200 통과 (신규 모두 ≤80L)

### C4 Reflection
- [ ] CinematicEffectsConfig SO Type
- [ ] TierEffectProfile struct
- [ ] HitEffectResolver static + Resolve
- [ ] ResolvedHitEffect struct
- [ ] CinematicEffectsConfigLoader static + Get/Bootstrap
- [ ] DamageSystem.DealDamage 시그니처 확장
- [ ] EnemyController.OnHit 시그니처 확장 (avgDamage)
- [ ] CinematicEffects.PlayCameraZoom 메서드

### C5 SO
- [ ] Resources/Combat/CinematicEffectsConfig.asset 존재
- [ ] 5 BossTier profile default 값 검증
- [ ] 25+ SerializeField 모두 적용

## 수동 검증 (사용자 Play)

### C6.1 적 티어 차별화 (S1)
- [ ] 일반 적 punch ~1.4×
- [ ] 미니보스 punch ~1.54×
- [ ] 챕터보스 punch ~1.75×

### C6.2 데미지 강도 (S2)
- [ ] 강화 +0 일반 데미지: 약한 효과
- [ ] 강화 +15 큰 데미지: 강한 효과 (intensity 1.0)

### C6.3 무기 색 (S3)
- [ ] Sword Red / Bow Cyan / Staff Purple

### C6.4 Pool 부족 해결 (S4)
- [ ] cap 300 환경 GC <50KB
- [ ] Debug.LogWarning 0건

## 비기능
- [ ] 60fps 유지
- [ ] 5001~5006 회귀 X

## 합격 판정
- 자동 모두 ✅ + S1, S2 ✅ → 최소 합격
- 자동 모두 ✅ + S1~S4 모두 ✅ → 완전 합격
