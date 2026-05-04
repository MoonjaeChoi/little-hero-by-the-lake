# Sprint Contract: 5008 chapter_enemy_asset_quality_v1

> **PGE Planner — Phase 4 Contract**
> PRD: `5008_enemy_visual_pipeline_v2_v2.md` · breakdown: `5008_enemy_visual_pipeline_v2_breakdown_00.md`
> 생성일: 2026-05-04

---

## 1. C1 컴파일 기준

- **C1.1**: 신규 .cs 파일 (T02 EnemySpriteAuditor.cs) 컴파일 errors=0
- **C1.2**: 도메인 리로드 후 5~10초 내 안정화 (`editor_state.isCompiling == false`)
- **C1.3**: 6 commit (조건부 7) 모두 errors=0

**검증 방법**:
```
mcp__UnityMCP__refresh_unity (compile=request)
sleep 8  # 도메인 리로드
mcp__UnityMCP__read_console (errors)  # 0
```

> 본 PRD 의 코드 신규는 T02 1건만. T01/T03/T04/T05/T05.5 = 자산 작업 (스크립트 영향 X), T06 = docs.

---

## 2. C2 Editor Test 기준 (NUnit)

> 본 PRD 는 NUnit Test 신규 명시 X (decisions D1=C: Editor 정적 도구 + 자산 작업, NUnit 가치 낮음).
> 회귀 검증만 — 기존 81 NUnit (5007 까지 누적) 모두 PASS.

| Test 클래스 | 명세 | 위치 |
|---|---|---|
| (신규 없음) | — | — |
| **회귀 검증**: 누적 81 NUnit | 모두 PASS | `Tests/Editor/` 5 클래스 |

### 신규 NUnit (0)

(없음)

**검증**: Test Runner Edit Mode > Run All → 81/81 PASS (5007 baseline 회귀 X)

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings 변동 ±0 (5007 종료 시점 기준)
- **C3.3**: 신규 위반 없음 (§15.6 200L LINE_LIMIT 통과)
  - EnemySpriteAuditor.cs ~85L ✅ (100L 룰 + 200L 위반 0)

**검증 방법**: `Tools/LittleHero/Validate Project` 실행 → 콘솔 출력 비교

**Validator 룰 추가 명세**: 없음 (Auditor 자체가 자산 검증 도구 — Validator 룰 외부)

---

## 4. C4 Reflection 검증 (정적)

| Type | Method/Field |
|---|---|
| `LittleHero.Editor.EnemySpriteAuditor` (static class) | `AuditAll()` (정적 메서드) + `[MenuItem("Tools/LittleHero/Audit Enemy Sprite Quality")]` 어트리뷰트 |
| 회귀: 5007 의 7 Type (CinematicEffectsConfig, HitEffectResolver, ResolvedHitEffect 등) | 변경 X |

**검증 방법**:
```csharp
var t = System.Type.GetType("LittleHero.Editor.EnemySpriteAuditor, Assembly-CSharp-Editor");
Assert.NotNull(t);
var m = t.GetMethod("AuditAll", BindingFlags.Public | BindingFlags.Static);
Assert.NotNull(m);
```

---

## 5. C5 SO 검증

> 본 PRD 는 SO 신규 X. 자산 (PNG) + 스킬 + Editor 도구만.
> 회귀 검증: 기존 SO 412 (BossLootTable 4 / EnemyData 262 등) 모두 변경 X.

### 자산 검증 (Auditor 자동 생성 report 기반)

| 자산 그룹 | 위치 | 개수 | 검증 |
|---|---|---|---|
| Meta archetype | `Generated_v2/Enemies/Enemy_Meta_*.png` | 6 | T03 후 alpha transparent 30~70% |
| Nature archetype | `Generated_v2/Enemies/Enemy_Nature_*.png` | 6 | T04 후 alpha transparent 30~70% |
| Animal archetype | `Generated_v2/Enemies/Enemy_Animal_*.png` | 6 | T04 후 alpha transparent 30~70% |
| Civ archetype | `Generated_v2/Enemies/Enemy_Civ_*.png` | 6 | T05 후 alpha transparent 30~70% |
| Fantasy archetype | `Generated_v2/Enemies/Enemy_Fantasy_*.png` | 6 | T05 후 alpha transparent 30~70% |
| **(조건부 D2=C)** chapter override 8 | `Generated_v2/Enemies/Chapters/CH{14,22,25,30,33×2,34}_*.png` | 8 | T05.5 후 alpha transparent 30~70% (활성화 시만) |

**검증 방법**:
- `Tools/LittleHero/Audit Enemy Sprite Quality` 실행 → enemy_sprite_audit_report.md 자동 생성
- report 의 transparent ratio 항목 확인

---

## 6. C6 Play 검증 시나리오

### 시나리오 1: Meta 카테고리 일관성 (T03 PASS 후)

**사전 조건**:
- T03 commit 후 (Meta 6 archetype 재생성 완료)
- 새 SaveData 또는 기존 SaveData
- SpriteAtlas 재빌드 (T06 또는 Unity 자동)

**단계**:
1. Play Mode 진입 → Attic 씬
2. 챕터 1 진입 → Wave 1~5 적 sprite 시각 확인
3. 챕터 2~5 동일 카테고리 적 sprite 확인

**기대 결과**:
- 6 archetype (Small/Medium/Large/MiniA/MiniB/Boss) 모두 warm beige + village 톤 일관
- 두꺼운 흰 outline 일관
- 콘솔 에러 0건
- Profiler — draw call <10/frame (§7 PIVOT v4)

---

### 시나리오 2: Nature vs Animal 카테고리 차별성 (T04 PASS 후)

**사전 조건**:
- T04 commit 후 (Nature + Animal 12 archetype 재생성 완료)

**단계**:
1. CH 6 (Nature 시작) 진입 → 적 sprite 확인
2. CH 14 (Nature 끝) 진입
3. CH 15 (Animal 시작) 진입 → 톤 차이 확인

**기대 결과**:
- Nature 6 PNG: 녹색·갈색 + leafy 모티프 일관
- Animal 6 PNG: 따뜻 오렌지·흰색 + 동물 모티프 일관
- Nature → Animal 전환 시 시각적 톤 차이 명확
- 콘솔 에러 0건

---

### 시나리오 3: Alpha 정확도 (모든 Sprint 후)

**사전 조건**:
- T03 + T04 + T05 (+ T05.5 조건부) commit 후
- 모든 30 archetype + (조건부 8 chapter override) 재생성 완료

**단계**:
1. CH 1 ~ CH 35 순차 진입 (또는 무작위 5 챕터 sample)
2. 각 챕터의 적 sprite 시각 확인

**기대 결과**:
- 회색 박스 / 흰 박스 / 가짜 alpha 0% 출현 0건 (5008 직전 사용자 보고 영구 해결)
- 모든 적 자연스럽게 표시 (chunky outline + cartoon shading)
- 콘솔 warning 0건 (sprite alpha 관련)

---

### 시나리오 4: V5 sticker tone 일관성 (T05 PASS 후)

**사전 조건**:
- T05 commit 후 (5 카테고리 × 6 archetype = 30 sprite 모두 재생성 완료)

**단계**:
1. Editor 메뉴 `Window > 2D > Sprite Atlas` 또는 Generated_v2/Enemies 폴더 직접 시각 검토
2. 30 sprite 일괄 비교

**기대 결과**:
- 두꺼운 흰 outline + 만화풍 + cartoon shading 모든 sprite 통일
- 5 카테고리 × 6 archetype = 30 sprite 의 V5 톤 일관성 사용자 확인
- 카테고리 간 톤 차이는 명확하나 sticker 형식은 통일

---

### 시나리오 5: SpriteAtlas 재빌드 + draw call (T06 PASS 후)

**사전 조건**:
- T06 commit 후 (SpriteAtlas 재빌드 완료)

**단계**:
1. Unity 빌드 (Editor 또는 iOS 빌드)
2. Play Mode 진입 → CH 1 진입
3. Profiler 확인 — Rendering > Batches

**기대 결과**:
- EnemyAtlas SpriteAtlas SRP Batcher 정상 작동
- draw call <10/frame (§7 PIVOT v4 cap)
- 빌드 시 atlas 누락 0
- 콘솔 에러 0건

---

### 시나리오 6: Auditor V2 archetype 폴백 검증 (T02 PASS 후, D2=C)

**사전 조건**:
- T02 commit 후 (EnemySpriteAuditor 신규)

**단계**:
1. Editor 메뉴 `Tools > LittleHero > Audit Enemy Sprite Quality` 실행
2. 콘솔 + `docs/little_hero/enemy_sprite_audit_report.md` 확인

**기대 결과**:
- report 자동 생성
- 손상 9 PNG (이전 식별 list) 자동 검출
- "V2 archetype fallback" 섹션에 "OK" 또는 "깨짐" 명시
- 임포트 설정 변동 0 (D1=C 복원 검증) — 임의 1 PNG의 isReadable 변경 X

**후속**:
- fallback OK → T05.5 미실행 (5008.1 anchor)
- fallback 깨짐 → 사용자 결정 후 T05.5 활성화

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0 (T02 + 회귀)
- [ ] Editor Test 81/81 PASS (5007 baseline 회귀 X)
- [ ] ProjectValidator warnings 변동 ±0
- [ ] 5007 cinematic effect 시스템 회귀 X
- [ ] 60fps 유지 (적 sprite 재생성, 텍스처 사이즈 동일 가정)
- [ ] EnemyAtlas SpriteAtlas SRP Batcher draw call <10/frame
- [ ] 빌드 (Unity → Build) 성공 + atlas 누락 0
- [ ] 메모리 ≤ 250MB (CLAUDE.md §7)

## 8. 합격 기준

**최소 합격**:
- C1 컴파일 ✅
- C2 회귀 81/81 ✅
- C3 Validator errors=0 ✅
- C4 Reflection EnemySpriteAuditor 통과 ✅
- C5 자산 검증 30 archetype transparent 30~70% ✅
- 시나리오 S1, S3, S6 PASS

**완전 합격**:
- 모든 카테고리 ✅
- 시나리오 S1~S6 모두 PASS
- 비기능 요구사항 모두 ✅
- 사용자 review (T03~T05) 모두 "OK" (또는 skip 시 D3=A+B 적용)

## 9. Trade-off (mediator 결정 기반)

(decisions.md §2 의 Trade-off 인용)

- **D1=C 결정** → AssetImporter.SetReadable + try/finally 복원 패턴. PNG byte 직접 파싱 (B) 의 메모리 효율 손실은 audit 1회 실행이라 무시 가능.
- **D2=C 결정** → Auditor V2 폴백 검증 후 결정. fallback OK 시 8 PNG 미처리 정당화 (5008.1 anchor 만). 깨짐 시 T05.5 강제 활성화로 회귀 방지.
- **D3=A+B 결정** → skip list docs + 5008.1 anchor. C (skip 0 강제) 의 30 archetype 완성 보장 손실 < 사용자 review 무한 반복 위험.
- **Rebuttal accepted**: SpriteAtlas 재빌드 명시 + Sprint 1 fallback 한계 명시 — 사용자 빌드 누락 + 기대치 정렬.

## 10. 변경 이력

- 2026-05-04: 초기 작성 (Phase 4 Contract). 6 카테고리 검증 + 6 Play 시나리오.
