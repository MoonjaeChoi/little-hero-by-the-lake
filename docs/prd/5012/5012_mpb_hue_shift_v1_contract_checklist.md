# Evaluator Checklist: 5012 — MPB Hue Shift v1

## 자동 검증 (스크립트 / MCP 자동화)

### C1 컴파일
- [ ] `mcp__UnityMCP__refresh_unity` 후 errors=0
- [ ] `editor_state.isCompiling == false` (5초 대기 후)

### C2 Editor Test (NUnit)
- [ ] Test Runner > Edit Mode > Run All → 6/6 PASS
- [ ] `EnemyHueVariationTests` 클래스 존재
- [ ] (자동) reflection 으로 6 메서드 명세 확인

### C3 ProjectValidator
- [ ] `Tools/LittleHero/Validate Project` 실행
- [ ] errors=0
- [ ] warnings 변동 ≤ 0
- [ ] 신규 룰 `EnemyController_HueVariation_Hook` 통과 (HueShift 호출 라인 존재)

### C4 Reflection 정적 검증
- [ ] `LittleHero.EnemyHueVariation` static class (abstract+sealed)
- [ ] `OFFSETS` public static readonly float[] (length=5)
- [ ] `VARIATION_COUNT` public const int (=5)
- [ ] `HueShift(Color, float)` public static → Color
- [ ] `VariationIndex(int, int)` public static → int
- [ ] `EnemyController.Initialize` 시그니처 (EnemyData, float=1, int=0) → void
- [ ] `EnemySpawner.Spawn` 시그니처 (EnemyData, int=0) → EnemyController

### C5 SO 검증
- [ ] EnemyData 51 보스 bossTier != None (5001 회귀 검사)
- [ ] (5012 신규 SO 없음 — N/A)

## 수동 검증 (사용자 Play Mode)

### C6.1 Wave 1~5 sprite hue 5단계 분포 (S1)
- [ ] CH 1 진입 → wave 1~5 진행
- [ ] 각 wave 의 일반 적 sprite 색상 시각 확인
- [ ] 5 hue 분포 (-20%, -10%, 0%, +10%, +20%) 인지 가능

### C6.2 Boss 원색 유지 (S2)
- [ ] wave 5 (MiniBoss) 또는 wave 20 (ChapterBoss) 출현
- [ ] Boss sprite 원색 (hue 적용 X) 확인

### C6.3 flash 후 hue 색상 유지 (S3, D3 검증)
- [ ] 일반 적 (variationIndex>0) hit
- [ ] flash 종료 후 \_baseColor 복귀
- [ ] hue 색상 유지 (white 로 복귀 X)

## 비기능 검증

- [ ] 기존 BossEncounterRunner / InfiniteTowerRunner Spawn 호출 회귀 X (default 0 호환)
- [ ] 5008 LayerDiffuse 30 archetype sprite 표시 정상
- [ ] 60fps 유지 (Profiler 권장 — HSV 변환 1회/spawn)

## 합격 판정

- 자동 검증 모두 ✅ + 수동 시나리오 S1, S2 ✅ → **최소 합격**
- 자동 검증 모두 ✅ + 수동 시나리오 S1, S2, S3 모두 ✅ → **완전 합격**
- 자동 검증 1개라도 ❌ → **FAIL** (Phase 5 수정)
