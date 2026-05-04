# Evaluator Checklist: 5008 chapter_enemy_asset_quality_v1

## 자동 검증 (스크립트 / MCP 자동화)

### C1 컴파일

- [ ] `mcp__UnityMCP__refresh_unity` 후 errors=0
- [ ] `editor_state.isCompiling == false` (5~10초 후)
- [ ] T02 EnemySpriteAuditor.cs 컴파일 errors=0
- [ ] 6 commit (조건부 7) 모두 errors=0

### C2 Editor Test (NUnit)

- [ ] Test Runner > Edit Mode > Run All → 81/81 PASS (5007 baseline 회귀 X)
- [ ] 신규 Test 0건 (Editor 정적 도구 + 자산, NUnit 가치 낮음)

### C3 ProjectValidator

- [ ] `Tools/LittleHero/Validate Project` 실행
- [ ] errors=0
- [ ] warnings 변동 ±0 (5007 baseline)
- [ ] 신규 위반 0건 (EnemySpriteAuditor ~85L, 100L 룰 통과)

### C4 Reflection 정적 검증

- [ ] `LittleHero.Editor.EnemySpriteAuditor` (static class) 존재 (Assembly-CSharp-Editor)
- [ ] `AuditAll()` 정적 public 메서드 존재
- [ ] `[MenuItem("Tools/LittleHero/Audit Enemy Sprite Quality")]` 어트리뷰트 존재
- [ ] 5007 7 Type 회귀 X (CinematicEffectsConfig, HitEffectResolver 등)

### C5 SO + 자산 검증

- [ ] (SO 신규 0 — 본 PRD 는 자산만)
- [ ] **자산 검증** (Auditor 자동 report 기반):
  - [ ] Enemy_Meta_*.png × 6 transparent 30~70% (T03)
  - [ ] Enemy_Nature_*.png × 6 transparent 30~70% (T04)
  - [ ] Enemy_Animal_*.png × 6 transparent 30~70% (T04)
  - [ ] Enemy_Civ_*.png × 6 transparent 30~70% (T05)
  - [ ] Enemy_Fantasy_*.png × 6 transparent 30~70% (T05)
  - [ ] (조건부 D2=C) Chapters/CH{14,22,25,30,33×2,34}_*.png × 8 transparent 30~70% (T05.5)
- [ ] 회귀: 기존 SO 412 (BossLootTable 4 / EnemyData 262 / Equipment 16 / 등) 변경 X

## 수동 검증 (사용자 Play Mode)

### C6.1 Meta 카테고리 일관성 (T03 PASS 후)

- [ ] 시나리오 1 단계 1~3 진행 (CH 1~5)
- [ ] 6 archetype 모두 warm beige + village 톤 일관 확인
- [ ] 두꺼운 흰 outline 일관
- [ ] 콘솔 에러 0건

### C6.2 Nature vs Animal 카테고리 차별성 (T04 PASS 후)

- [ ] 시나리오 2 단계 1~3 진행 (CH 6 / CH 14 / CH 15)
- [ ] Nature 녹색·갈색 일관 + Animal 따뜻 오렌지 일관
- [ ] 카테고리 전환 시 시각적 차이 명확
- [ ] 콘솔 에러 0건

### C6.3 Alpha 정확도 (모든 Sprint 후)

- [ ] 시나리오 3 단계 1~2 진행 (CH 1~35 sample)
- [ ] 회색 박스 / 흰 박스 / 가짜 alpha 출현 0건
- [ ] 모든 적 자연스럽게 표시 (5008 직전 사용자 보고 영구 해결)
- [ ] 콘솔 warning 0건

### C6.4 V5 sticker tone 일관성 (T05 PASS 후)

- [ ] 시나리오 4 단계 1~2 진행
- [ ] 30 sprite 일괄 비교
- [ ] 두꺼운 흰 outline + 만화풍 + cartoon shading 통일
- [ ] 5 카테고리 톤 차이 명확하나 sticker 형식 통일

### C6.5 SpriteAtlas 재빌드 + draw call (T06 PASS 후)

- [ ] 시나리오 5 단계 1~3 진행 (Unity 빌드 + Profiler)
- [ ] EnemyAtlas SpriteAtlas SRP Batcher 정상
- [ ] draw call <10/frame 확인
- [ ] 빌드 atlas 누락 0
- [ ] 콘솔 에러 0건

### C6.6 Auditor V2 archetype 폴백 검증 (T02 PASS 후, D2=C)

- [ ] 시나리오 6 단계 1~2 진행 (Tools > LittleHero > Audit Enemy Sprite Quality)
- [ ] enemy_sprite_audit_report.md 자동 생성
- [ ] 손상 9 PNG 자동 검출 (이전 식별 list 매칭)
- [ ] "V2 archetype fallback: OK / 깨짐" 명시
- [ ] 임포트 설정 변동 0 (D1=C 복원 검증)
- [ ] **후속 결정**:
  - fallback OK → T05.5 미실행
  - fallback 깨짐 → 사용자 결정 → T05.5 활성화

## 비기능 검증

- [ ] 5007 cinematic effect 시스템 회귀 X (CinematicEffectsConfig + HitEffectResolver 정상)
- [ ] 60fps 유지 (적 sprite 텍스처 사이즈 동일 가정)
- [ ] EnemyAtlas SpriteAtlas SRP Batcher draw call <10/frame
- [ ] 메모리 ≤ 250MB (Profiler 확인)
- [ ] 빌드 (Unity → Build) 성공

## 사용자 자산 review 체크 (D3=A+B)

### Sprint 별 사용자 review 결과 기록

- [ ] T03 Meta 6 sprite — 응답: ☐ OK / ☐ 재시도 (iter 2/3) / ☐ skip (5008.1 enroll)
- [ ] T04 Nature 6 + Animal 6 — 응답: ☐ OK / ☐ 재시도 / ☐ skip
- [ ] T05 Civ 6 + Fantasy 6 — 응답: ☐ OK / ☐ 재시도 / ☐ skip
- [ ] (조건부) T05.5 chapter override 8 — 응답: ☐ OK / ☐ 재시도 / ☐ skip

### Skip List (있으면 T06 docs 자동 enroll)

- skip 발생 시 list 보존 + 5008.1 anchor 우선 처리 표시

## 합격 판정

- 자동 검증 모두 ✅ (C1~C5) + 수동 시나리오 S1, S3, S6 PASS → **최소 합격**
- 자동 검증 모두 ✅ + 수동 시나리오 S1~S6 모두 PASS + 사용자 review 모두 "OK" 또는 skip 명시 → **완전 합격**
- 자동 검증 1개라도 ❌ → **FAIL** (Phase 5 수정)
- 사용자 review iter 3 후 다수 skip 발생 시 → 부분 합격 (5008.1 우선 처리 anchor 활성화)
