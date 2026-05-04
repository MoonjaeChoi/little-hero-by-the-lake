# Evaluator Checklist: 5009 — Chapter Overrides

## 자동 검증

### C1 컴파일 (회귀)
- [ ] 코드 변경 0건 (자산 only)
- [ ] refresh_unity 후 errors=0

### C2 Editor Test (회귀)
- [ ] 누적 NUnit 94 그대로 유지

### C3 ProjectValidator + Auditor
- [ ] `Tools > LittleHero > Validate Project` errors=0
- [ ] `Tools > LittleHero > Audit Enemy Sprite Quality`:
  - 210 chapter override PNG transparent 30~80% 통과
  - asymmetry ≤ 50
  - V2 archetype fallback OK
  - 손상 0건

### C5 SO 자산 metric
- [ ] 210 PNG 모두 정확한 path (CH{01~35}_{Small,Medium,Large,MiniA,MiniB,Boss}.png)
- [ ] 카테고리 톤 일관 (Meta/Nature/Animal/Civ/Fantasy)
- [ ] 시드 공식 정합 (base + (CH-1)*6)
- [ ] TextureImporter Sprite (Single), Pixels Per Unit 100
- [ ] EnemyData SO icon 변경 X

## 수동 검증 (사용자 Play Mode)

### S1.1 Meta 톤 (CH 1, 2, 3, 4, 5)
- [ ] CH 1 wave 1 — warm beige/brown 톤 일관
- [ ] CH 5 wave 1 — Meta 카테고리 동일 톤

### S1.2 Nature 톤 (CH 6~14 sample)
- [ ] CH 8 wave 1 — forest green 톤
- [ ] CH 14 wave 1 — 동일 톤

### S1.3 Animal 톤 (CH 15~20 sample)
- [ ] CH 18 wave 1 — warm orange/brown

### S1.4 Civ 톤 (CH 21~27 sample)
- [ ] CH 25 wave 1 — cool blue/gray paper craft

### S1.5 Fantasy 톤 (CH 28~35 sample)
- [ ] CH 32 wave 1 — soft lavender/pastel purple

### S2 Auditor 재실행
- [ ] 210 PNG healthy
- [ ] V2 fallback OK
- [ ] 손상 0건

### S3 SpriteAtlas + Profiler
- [ ] EnemyAtlas Pack Atlases 성공
- [ ] CH 1 wave 1 draw call <10/frame
- [ ] SRP Batcher 정상

### 5012 결합 검증 (사용자 추가 요청 trigger)
- [ ] CH 1 wave 1 일반 적 — 5단계 hue 색조 변형 visible
- [ ] Boss 원색 유지

## 비기능

- [ ] 60fps (PIVOT v4 §7)
- [ ] GC <50KB/s (자산 작업이라 코드 영향 X)
- [ ] App 용량 <150MB (PNG 5008 baseline 기준 ~250KB/PNG × 210 ≈ 52MB 추가 — Atlas 압축 후 영향 적음)

## 합격 판정

- 자동 모두 ✅ + S1 (5 카테고리 모두) + S2 ✅ → **최소 합격**
- 자동 모두 ✅ + S1 + S2 + S3 + 5012 결합 ✅ → **완전 합격**
- 자동 1개라도 ❌ → **FAIL** (Phase 5 수정)
