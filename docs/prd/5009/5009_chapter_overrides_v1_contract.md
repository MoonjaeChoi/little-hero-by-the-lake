# Sprint Contract: 5009 — Chapter Overrides 210 PNG

> **PGE Planner — Phase 4 Contract**
> PRD: 5009_chapter_overrides_v1_v2.md · breakdown: _breakdown_00.md
> 생성일: 2026-05-04 · RESERVED 해제 (5012/5013 종결 후 재진입)

---

## 1. C1 컴파일 기준

5009 는 자산 작업 — 코드 변경 없음. 컴파일 검증 N/A (회귀 X).

- **C1.1**: 기존 코드 변경 0건 — refresh 후 errors=0 (회귀 검증)

---

## 2. C2 Editor Test 기준 (NUnit)

5009 는 자산 작업 — 신규 NUnit 없음. 기존 94 NUnit 회귀 X 검증.

- **C2.1**: 누적 NUnit 94 그대로 유지

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings ≤ 기존
- **C3.3**: EnemySpriteAuditor (5008 D1=C/D2=C 도구) 재실행 결과:
  - 210 PNG 모두 transparent 30~80% 통과
  - asymmetry ≤ 50
  - V2 fallback OK (V2 archetype 미손상)
  - 손상 0건

**검증 방법**: `Tools > LittleHero > Validate Project` + `Tools > LittleHero > Audit Enemy Sprite Quality`

---

## 4. C4 Reflection 검증

5009 는 자산 작업 — 신규 Type 없음. N/A.

---

## 5. C5 SO 검증 (자산 검증 — sprite_generation_rules §3.6)

| 검증 항목 | 기준 |
|---|---|
| 210 chapter override PNG 존재 | Generated_v2/Enemies/Chapters/CH{01~35}_{Small,Medium,Large,MiniA,MiniB,Boss}.png |
| 각 PNG metric (Auditor) | transparent 30~80%, asym ≤ 50 |
| 카테고리 톤 일관 | Meta=warm beige/brown / Nature=forest green / Animal=warm orange/brown / Civ=cool blue/gray / Fantasy=lavender |
| 시드 공식 정합 | base + (CH-1)*6 |
| TextureImporter Sprite Single | Pixels Per Unit 100 |
| EnemyData SO icon 변경 X | chapter override path 유지 |

**검증 방법**: Auditor 자동 + 사용자 시각 review

---

## 6. C6 Play 검증 시나리오

### S1: 챕터별 시각 일관성

**사전 조건**: T01~T06 완료, SpriteAtlas 재빌드 됨

**단계**:
1. Play Mode 진입 → CH 1 wave 1 시작
2. 일반 적 sprite 출현 — Meta 카테고리 톤 (warm beige/brown) 확인
3. CH 8 wave 1 진입 → Nature 톤 (forest green) 확인
4. CH 18 wave 1 → Animal 톤 (warm orange/brown)
5. CH 25 wave 1 → Civ 톤 (cool blue/gray)
6. CH 32 wave 1 → Fantasy 톤 (lavender)

**기대 결과**:
- 각 챕터 sprite 카테고리 톤 일관
- 5008 archetype (Meta/Nature/Animal/Civ/Fantasy) 와 chapter override 시각 통일성
- 5012 hue shift 와 결합 시 visible 5단계 색조 변형 (multiplicative tint 가 saturation 있는 sprite 에 적용)

---

### S2: Auditor 재실행 손상 0

**단계**:
1. `Tools > LittleHero > Audit Enemy Sprite Quality` 실행
2. console output 확인

**기대 결과**:
- 210 chapter override PNG 모두 healthy
- V2 archetype fallback OK
- 손상 0건

---

### S3: SpriteAtlas + Profiler

**단계**:
1. EnemyAtlas SpriteAtlas Pack Atlases (수동 또는 자동)
2. Play Mode → CH 1 진입
3. Profiler 모니터링

**기대 결과**:
- draw call <10/frame (PIVOT v4 §7 cap)
- SRP Batcher 정상 (적 sprite 모두 같은 atlas)

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0 (회귀 X)
- [ ] Editor Test 94 그대로 (회귀 X)
- [ ] ProjectValidator warnings 변동 ≤ 0
- [ ] Auditor 손상 0
- [ ] PIVOT v4 §7 60fps + draw call <10/frame
- [ ] 5012 hue shift 와 결합 시 visible 효과 (saturation 있는 sprite 픽셀)

## 8. 합격 기준

**최소 합격**:
- C3 Validator + Auditor errors=0 ✅
- C5 자산 metric 통과 ✅
- 시나리오 S1 (일부 챕터) PASS

**완전 합격**:
- 모든 카테고리 ✅
- S1 + S2 + S3 모두 PASS
- 사용자 5 batch review PASS

## 9. Trade-off (mediator 결정 기반)

- D1=B → RESERVED 해제 (5012/5013 종결 후 진행) → 사용자 5012 visible 보고 trigger 로 즉시 재진입
- D2=A → Sprint 5 Fantasy 48 PNG 그대로 (분할 X) — 사용자 review 부담 ↑ 다만 batch 단위 안정

## 10. 변경 이력
- 2026-05-04: 초기 작성 (Phase 4 Contract, RESERVED 해제 후).
