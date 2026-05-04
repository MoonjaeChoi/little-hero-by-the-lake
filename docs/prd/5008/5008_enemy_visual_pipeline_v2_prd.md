# 5008 — Chapter Enemy Asset Quality v1 — Sprint PRD

> **출처**: `docs/prd/5008_enemy_visual_pipeline_v2.md` (디자인, 범위 축소 판) — 옵션 D1=B / D2=A / D3=B / D4=A / D5=A / D6=A
> **범위**: 30 archetype 베이스 sprite 재생성 + LayerDiffuse 워크플로 + 사용자 review 사이클. **MPB / culling / LoRA / Chapter 별 210 sprite 등은 5008.1+ / 5009+ 별도 사이클**.

## 1. 전체 개요

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | comfyui-image LayerDiffuse 워크플로 + 카테고리 prompt 가이드 (`enemy_sprite_prompts_v1.md`) | 1 | 50분 |
| 2 | EnemySpriteAuditor Editor 메뉴 + 현재 262 PNG 품질 report | 1 | 40분 |
| 3 | Meta (CH 1~5) 6 archetype 재생성 → 사용자 review (max 3 iteration) | 1 | 30~60분 |
| 4 | Nature (CH 6~14) 6 + Animal (CH 15~20) 6 = 12 archetype 재생성 → review | 1 | 45~75분 |
| 5 | Civ (CH 21~27) 6 + Fantasy (CH 28~35) 6 = 12 archetype 재생성 → review | 1 | 45~75분 |
| 6 | docs (changelog + CLAUDE.md §11/§12/§14) | 1 | 25분 |
| **총** | | **6** | **약 4~6시간 (review 시간 포함)** |

## 2. Sprint 명세

### Sprint 1 — LayerDiffuse + 카테고리 prompt 가이드 (1 commit)

#### 1.1 comfyui-image 스킬 LayerDiffuse 워크플로

**파일**:
- `.claude/skills/comfyui-image/assets/workflow_layerdiffuse.json` 신규
- `.claude/skills/comfyui-image/SKILL.md` 가이드 추가
- `.claude/skills/comfyui-image/references/comfyui_api.md` LayerDiffuse 노드 명세
- `docs/little_hero/enemy_sprite_prompts_v1.md` 신규 (5 × 6 = 30 prompt template)

**산출물**:
- DreamShaperXL Turbo + ComfyUI-layerdiffuse (huchenlei) 워크플로 (RGBA native 출력)
- 사용자 ComfyUI 에 `huchenlei/ComfyUI-layerdiffuse` Custom Node 설치 사전 조건
- 5 카테고리 × 6 archetype = 30 prompt 매핑 문서 (Meta/Nature/Animal/Civ/Fantasy × Small/Medium/Large/MiniA/MiniB/Boss)

**검증 기준**:
- comfyui-image 스킬 `--workflow workflow_layerdiffuse.json` 인자 동작
- LayerDiffuse 노드 미설치 시 fallback (기존 워크플로 사용)
- 사용자 review: 1 sprite 테스트 후 alpha transparent ratio 30~70%

#### Sprint 1 commit

```
5008 Sprint 1 / T01 — comfyui-image LayerDiffuse 워크플로 + 카테고리 prompt 가이드 (1/6)

[변경]
- .claude/skills/comfyui-image/assets/workflow_layerdiffuse.json 신규
- .claude/skills/comfyui-image/SKILL.md / references/comfyui_api.md 가이드
- docs/little_hero/enemy_sprite_prompts_v1.md 신규 (5 카테고리 × 6 archetype = 30 prompt)
- 사전 조건: huchenlei/ComfyUI-layerdiffuse Custom Node 설치

[검증]
- 스킬 정상 동작 (기존 fallback)
- 1 sprite 테스트 alpha 정상

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 2 — EnemySpriteAuditor + 품질 report (1 commit)

#### 2.1 Editor 메뉴 + 자동 검증

**파일**: `Scripts/Editor/EnemySpriteAuditor.cs`

**산출물**:
- 메뉴 `Tools > LittleHero > Audit Enemy Sprite Quality`
- 모든 PNG (262) 의 alpha transparent ratio 검사 (Pillow 알고리즘 C# 구현 또는 Texture2D.GetPixels32 사용)
- 카테고리별 통계 + 손상 (transparent < 5% or > 95%) PNG 자동 list
- `docs/little_hero/enemy_sprite_audit_report.md` 자동 생성 (현재 상태 snapshot)

**검증 기준**:
- 컴파일 errors=0
- 메뉴 실행 시 report 생성
- 손상 9 PNG 자동 검출 (이미 알려진 list 와 일치)

#### Sprint 2 commit

```
5008 Sprint 2 / T02 — EnemySpriteAuditor Editor 메뉴 + 품질 report (2/6)

[변경]
- Scripts/Editor/EnemySpriteAuditor.cs 신규 (~80L)
- docs/little_hero/enemy_sprite_audit_report.md 자동 생성 (현재 262 PNG 품질 snapshot)

[검증]
- 컴파일 errors=0
- 손상 9 PNG 자동 검출 (이전 식별 list 와 매칭)
- §15.6 100L / §15.12 Validator 룰 역할

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 3 — Meta (CH 1~5) 6 archetype 재생성 (1 commit, review 사이클)

#### 3.1 6 sprite 재생성

**대상**: `Generated_v2/Enemies/Enemy_Meta_{Small,Medium,Large,MiniA,MiniB,Boss}.png`

**프로세스**:
1. comfyui-image 스킬 + LayerDiffuse 워크플로 + Meta prompt template
2. 시드 22222 (Meta 카테고리 통일)
3. 6 sprite batch 생성 (~30초)
4. 사용자 review:
   - 만족 → Sprint 4 진입
   - 불만족 → prompt 조정 + 재생성 (iteration 2/3)
   - max 3 iteration 내 PASS 못 하면 사용자 결정 (다른 시드 / 다른 prompt / skip)

**검증 기준**:
- 6 PNG 모두 alpha transparent 30~70%
- V5 sticker tone 일관 (warm beige + 두꺼운 outline)
- 사용자 "OK" 응답

#### Sprint 3 commit

```
5008 Sprint 3 / T03 — Meta 6 archetype LayerDiffuse 재생성 (3/6)

[변경]
- Art/Generated_v2/Enemies/Enemy_Meta_{Small,Medium,Large,MiniA,MiniB,Boss}.png 재생성
- LayerDiffuse SDXL native transparency
- 시드 22222 (Meta 카테고리 통일)

[검증]
- 6 PNG transparent 30~70%
- V5 sticker tone 일관 (warm village 톤)
- 사용자 review PASS

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 4 — Nature + Animal (CH 6~20) 12 archetype (1 commit)

**대상**:
- `Enemy_Nature_{6}` 6 PNG (시드 33333)
- `Enemy_Animal_{6}` 6 PNG (시드 44444)

**프로세스**: Sprint 3 와 동일, 카테고리별 시드 고정

**검증 기준**: 12 PNG 모두 PASS, 사용자 review 통과

#### Sprint 4 commit

```
5008 Sprint 4 / T04 — Nature + Animal 12 archetype 재생성 (4/6)

[변경]
- Enemy_Nature_*.png × 6 (시드 33333)
- Enemy_Animal_*.png × 6 (시드 44444)
- LayerDiffuse + 카테고리별 prompt prefix

[검증]
- 12 PNG transparent 30~70%
- 카테고리 톤 일관 (Nature 녹색·갈색 / Animal 따뜻 동물)
- 사용자 review PASS

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 5 — Civ + Fantasy (CH 21~35) 12 archetype (1 commit)

**대상**:
- `Enemy_Civ_{6}` 6 PNG (시드 55555)
- `Enemy_Fantasy_{6}` 6 PNG (시드 66666)

#### Sprint 5 commit

```
5008 Sprint 5 / T05 — Civ + Fantasy 12 archetype 재생성 (5/6)

[변경]
- Enemy_Civ_*.png × 6 (시드 55555)
- Enemy_Fantasy_*.png × 6 (시드 66666)

[검증]
- 12 PNG transparent 30~70%
- 카테고리 톤 일관 (Civ 핑크·인공물 / Fantasy 보라·sparkle)
- 사용자 review PASS

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 6 — docs (1 commit)

**작업**:
- `docs/little_hero/changelog_v1.md` — 5008 항목
- `CLAUDE.md §11` — 마일스톤 row
- `CLAUDE.md §12` — UI 시스템 갱신 (LayerDiffuse + Auditor)
- `CLAUDE.md §14` — 최근 5건 회전

#### Sprint 6 commit

```
5008 Sprint 6 / T06 — docs (changelog + CLAUDE.md §11/§12/§14) (6/6)

[변경]
- docs/little_hero/changelog_v1.md — 5008 항목
- CLAUDE.md §11/§12/§14 갱신
- 후속 anchor 명시 (5008.1: chapter overrides 210 / 5008.2: V1 / 5008.3: 보스 / 5009: MPB / 5010: culling / 5011: LoRA)

[검증]
- Phase 5 종결 (6/6 commit)
- 30 archetype 모두 사용자 review PASS

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
Sprint 1 (스킬) ──┐
                  ├──→ Sprint 3 (Meta) ──→ Sprint 4 (Nature+Animal) ──→ Sprint 5 (Civ+Fantasy) ──→ Sprint 6 (docs)
Sprint 2 (Auditor) ┘
```

**병렬 가능**: Sprint 1 + Sprint 2 (독립)
**순차**: Sprint 3 → 4 → 5 (사용자 review 사이클)

## 4. 검증 시나리오 (Play 검증)

### S1 — Meta 카테고리 일관성

1. CH 1 ~ CH 5 진입
2. 적 sprite 시각 확인
- **검증**: 6 archetype (Small/Medium/Large/MiniA/MiniB/Boss) 모두 warm beige + village 톤 일관

### S2 — Nature vs Animal 카테고리 차별성

1. CH 6 (Nature) → CH 14 (Nature 끝) → CH 15 (Animal 시작)
2. 카테고리 전환 시 톤 차이 명확
- **검증**: Nature 녹색·갈색 vs Animal 따뜻 오렌지 톤 차이

### S3 — Alpha 정확도

1. CH 1~35 진행
2. 회색 박스 / 흰 박스 / 가짜 alpha 0
- **검증**: 모든 적 자연스럽게 표시 (5008 직전 사용자 보고 영구 해결)

### S4 — V5 sticker tone 일관성

1. 5 카테고리 × 6 archetype = 30 sprite 시각 검토
- **검증**: 두꺼운 흰 outline + 만화풍 + cartoon shading 모든 sprite 통일

## 5. Definition of Done

- [ ] Sprint 1~6 모두 commit (6 commit)
- [ ] LayerDiffuse 워크플로 추가 + 사용자 노드 설치 가이드
- [ ] enemy_sprite_prompts_v1.md (30 매핑)
- [ ] EnemySpriteAuditor Editor 메뉴 + 자동 report
- [ ] 30 archetype 재생성 + 사용자 review 모두 PASS
- [ ] CLAUDE.md §11/§12/§14 + changelog
- [ ] Play 시나리오 S1~S4 통과
- [ ] 후속 anchor 5008.1+, 5009+ 명시

## 6. 알려진 위험 + 완화

| 위험 | Sprint | 완화 |
|---|---|---|
| LayerDiffuse 노드 설치 (사용자 책임) | 1 | SKILL.md URL 명시 + fallback workflow |
| 카테고리 톤 불만족 → max 3 iteration | 3~5 | 시드 고정 + prompt prefix 표준 |
| 사용자 review 시간 길어짐 | 3~5 | 카테고리별 batch 6 sprite 한 번 review |
| 손상 9 PNG 중 30 archetype 외 8 PNG | 본 PRD 외 | 5008.1 후속 (Chapter 별 210 PNG 포함) |
| Chapter 별 sprite (210 PNG) 미처리 | 본 PRD 외 | V2 시스템 = 30 archetype 만 사용 (PhaseFContentSeed 결과). 210 chapter override = 레거시. 별도 사이클 |
| 사용자 만족도 측정 | 3~5 | "OK" / "재시도" / "skip" 응답 명시 |

## 7. 변경 이력

- 2026-05-04: v1 작성 (사용자 범위 축소 결정 — 30 archetype + LayerDiffuse + 사용자 review 사이클).
