# T06 — docs + SpriteAtlas 재빌드 + skip list (D3=A+B)

> **레이어**: docs
> **의존성**: T05 PASS (+ T05.5 활성화 시 그 이후)
> **예상 시간**: 25~30분 / 1 commit
> **PRD 출처**: `5008_enemy_visual_pipeline_v2_v2.md` §2 Sprint 6

## 1. 작업 항목

### 1.1 changelog 추가

**파일**: `docs/little_hero/changelog_v1.md`

**추가 항목**:
```markdown
- 2026-05-XX: **5008 chapter_enemy_asset_quality_v1** — 30 archetype LayerDiffuse 재생성 (Meta 6 + Nature 6 + Animal 6 + Civ 6 + Fantasy 6) + EnemySpriteAuditor (D1=C 복원 패턴 + D2=C V2 폴백 검증) + SpriteAtlas 재빌드 + skip list (D3=A+B). N commit. 후속 anchor — 5008.1 chapter overrides 210 (skip list 우선) / 5008.2 V1 11종 / 5008.3 보스 11종 / 5009 MPB hue shift / 5010 culling / 5011 LoRA.
```

### 1.2 CLAUDE.md §11 마일스톤

**추가 행**:
```markdown
| **chapter_enemy_asset_quality_v1** | 30 archetype LayerDiffuse + Auditor + skip list | ✅ 2026-05-XX |
```

### 1.3 CLAUDE.md §12 시스템 현황

**§12.활성 시스템 — Editor** 섹션에 추가:
```
EnemySpriteAuditor (5008, Tools/LittleHero/Audit Enemy Sprite Quality, D1=C 복원 패턴 + D2=C V2 폴백 검증)
```

**§12.아트 V5 sticker dynamic 톤** 섹션 갱신:
- 기존 "적 sprite 262" → "적 sprite 262 (5008 30 archetype LayerDiffuse 재생성, Meta/Nature/Animal/Civ/Fantasy 카테고리 시드 분배)"

### 1.4 CLAUDE.md §14 변경 이력 (직전 5건)

**추가 항목** (가장 위):
```markdown
- 2026-05-XX: **5008 chapter_enemy_asset_quality_v1** — 30 archetype LayerDiffuse 재생성 + EnemySpriteAuditor + skip list (D3=A+B). N commit. 후속 anchor 5008.1+/5009+ 명시.
```

**기존 5건 중 가장 오래된 1건 제거** (직전 5건 maintain).

### 1.5 SpriteAtlas 재빌드 (Rebuttal accepted)

**작업**:
1. Unity Editor 메뉴 `Window > 2D > Sprite Atlas` 진입
2. EnemyAtlas 선택
3. "Pack Preview" 또는 "Pack Atlases" 버튼으로 재빌드
4. (또는 Unity 자동 재빌드 — Project Settings 의 "Sprite Packer Mode" 설정 확인)
5. 빌드 후 Play Mode 에서 적 sprite draw call <10/frame 검증 (Profiler)

**docs**:
- T06 commit 메시지 + Definition of Done 에 SpriteAtlas 재빌드 단계 명시
- (선택) `docs/little_hero/changelog_v1.md` 에 SpriteAtlas 재빌드 가이드 한 줄 추가

### 1.6 skip list docs (D3=A+B)

**조건**: T03/T04/T05 에서 skip 발생 시.

**작업**:
- `docs/little_hero/changelog_v1.md` 5008 항목에 skip list 명시 (예: `skip: Enemy_Civ_Boss (시드 55555 iter 3 후 톤 불일치 — 5008.1 우선 처리 enroll)`)
- 5008.1 anchor (CLAUDE.md §13 또는 후속 PRD) 에 "skip list 우선 처리" 1줄 추가

**조건 미충족 시**: skip list 작성 X (자동 skip)

### 1.7 후속 anchor 명시

`docs/little_hero/changelog_v1.md` 또는 5008 디자인 문서 §7 에 후속 anchor 명시:
- **5008.1**: chapter overrides 210 PNG (+ skip list 우선 처리 — D3=A+B)
- **5008.2**: V1 11종 동화 적 (Chapter_01~03)
- **5008.3**: Daily 7 + Weekly 4 보스
- **5009**: MPB hue shift 다양성
- **5010**: EnemyVisibilityCuller (off-screen culling)
- **5011**: V5 sticker LoRA 훈련

## 2. 검증 기준

### 2.1 docs 정합성
- changelog_v1.md 5008 항목 존재 + skip list 명시 (있으면)
- CLAUDE.md §11/§12/§14 갱신 정합
- 5건 maintain (§14)

### 2.2 SpriteAtlas
- EnemyAtlas 재빌드 완료
- Play Mode 진입 후 Profiler — 적 sprite draw call <10/frame (§7 PIVOT v4 cap)
- 빌드 (Unity → Build) 시 atlas 누락 0

### 2.3 후속 anchor
- 5008.1+, 5009+ 모두 명시
- skip list (있으면) 5008.1 anchor 에 우선 처리 표시

### 2.4 ProjectValidator
- `Tools/LittleHero/Validate Project` 실행 → errors=0, warnings 변동 ±0

## 3. commit 메시지

```
5008 Sprint 6 / T06 — docs + SpriteAtlas 재빌드 + skip list (6/6)

[변경]
- docs/little_hero/changelog_v1.md — 5008 항목 + skip list (D3=A+B)
- CLAUDE.md §11/§12/§14 갱신
- 후속 anchor 명시 (5008.1: chapter overrides 210 + skip list 우선 / 5008.2 / 5008.3 / 5009 / 5010 / 5011)
- SpriteAtlas 재빌드 가이드 (사용자 빌드 시 누락 방지, Rebuttal accepted)

[검증]
- Phase 5 종결 (6/6 commit, T05.5 조건부)
- 30 archetype 모두 사용자 review PASS (또는 skip enroll)
- EnemyAtlas SpriteAtlas 재빌드 검증 (draw call <10/frame, §7)
- skip list docs 명시 (있으면)
- ProjectValidator errors=0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 6
- decisions D3=A+B (skip 후속 정책)
- Rebuttal accepted: SpriteAtlas 재빌드 명시

## 5. 알려진 위험

- **CLAUDE.md §14 5건 maintain 누락** → 명시적으로 직전 5건 직접 확인 필요
- **SpriteAtlas 재빌드 누락** → Unity 자동 재빌드 의존 시 누락 가능. 메뉴 강제 실행 권장
- **skip list 누락** → T03/T04/T05 review 시 skip 결정한 sprite list 보존 필수 (commit 메시지 또는 별도 메모)
- **후속 anchor 명시 누락** → 5008.1+ / 5009+ 일괄 명시
