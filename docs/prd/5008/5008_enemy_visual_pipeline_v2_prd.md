# 5008 — Enemy Visual Pipeline v2 — Sprint PRD

> **출처**: `docs/prd/5008_enemy_visual_pipeline_v2.md` (디자인) — 옵션 D1~D6 권장 일괄 채택 가정 (모두 A)
> **PGE Phase 2 review 가능성**: `/prd-pipeline 5008` 호출 시 critique → rebuttal → mediator → breakdown → contract → implement → evaluate 자동.

## 1. 전체 개요

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | EnemyData.hueShift 필드 + ApplyAppearance MPB 적용 | 1 | 40분 |
| 2 | EnemyHueShiftSeed Editor 메뉴 + 262 SO chapter 별 분배 | 1 | 30분 |
| 3 | EnemyVisibilityCuller 단일 manager + EnemyController hook | 1 | 50분 |
| 4 | comfyui-image 스킬 LayerDiffuse 워크플로 추가 | 1 | 35분 |
| 5 | 9 손상 PNG LayerDiffuse 재생성 + Unity 임포트 | 1 | 40분 (사용자 LayerDiffuse 노드 설치 의존) |
| 6 | NUnit EnemyVisualPipelineTests 5+ + Validator | 1 | 35분 |
| 7 | docs (changelog + CLAUDE.md §11/§12/§14) | 1 | 25분 |
| **총** | | **7** | **4시간 15분** |

## 2. Sprint 명세

### Sprint 1 — EnemyData hueShift + MPB 적용 (1 commit)

#### 1.1 EnemyData 필드 + EnemyController.ApplyAppearance MPB

**파일**:
- `Assets/_Project/Scripts/Data/EnemyData.cs` — `hueShift` float 필드 신규 (Range -1~1)
- `Assets/_Project/Scripts/Gameplay/Enemies/EnemyController.cs` — ApplyAppearance 에 MPB hue shift

**산출물**:
```csharp
// EnemyData.cs
[Header("5008 — 색상 변형")]
[Range(-1f, 1f)]
[Tooltip("0 = 원본. ±0.1 = 미세 변형. ±0.3 = 큰 변형. chapter 별 다양성 확보.")]
public float hueShift = 0f;

// EnemyController.cs — ApplyAppearance 변경
private static MaterialPropertyBlock _mpb;
private void ApplyAppearance(EnemyData data) {
    SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
    if (sr != null) {
        if (data.icon != null) sr.sprite = data.icon;
        Color final = ResolveTintColor(data);
        sr.color = final;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        sr.GetPropertyBlock(_mpb);
        _mpb.SetColor("_Color", final);
        sr.SetPropertyBlock(_mpb);
    }
    // ... 기존 scale 코드 보존
}

private static Color ResolveTintColor(EnemyData data) {
    if (data.icon == null) return data.tintColor;
    Color baseCol = Color.white;
    if (Mathf.Abs(data.hueShift) > 0.001f) {
        Color.RGBToHSV(baseCol, out float h, out float s, out float v);
        h = Mathf.Repeat(h + data.hueShift, 1f);
        s = Mathf.Max(s, 0.15f);
        baseCol = Color.HSVToRGB(h, s, v);
    }
    return baseCol;
}
```

**검증 기준**:
- 컴파일 errors=0
- §15.6 100L 내 (EnemyController 본체 +20L)
- SRP Batcher 호환 (Material 인스턴스화 X)
- hueShift = 0 (default) 시 기존 동작과 동일

#### Sprint 1 commit

```
5008 Sprint 1 / T01 — EnemyData.hueShift + EnemyController MPB 적용 (1/7)

[변경]
- Scripts/Data/EnemyData.cs — hueShift float 필드 (Range -1~1, default 0)
- Scripts/Gameplay/Enemies/EnemyController.cs
  - ApplyAppearance 에 MaterialPropertyBlock + Color.HSVToRGB 적용
  - ResolveTintColor 정적 헬퍼 (sticker 톤 ±channel HSV shift)
  - _mpb 정적 (no GC, SetPropertyBlock 만 사용 — SRP Batcher 호환)

[검증]
- 컴파일 errors=0
- 기존 동작 (hueShift=0) 회귀 X
- §15.4 SO + §15.7 DI + §15.11 GC 0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 2 — EnemyHueShiftSeed (1 commit)

#### 2.1 Editor 메뉴

**파일**: `Assets/_Project/Scripts/Editor/EnemyHueShiftSeed.cs`

**산출물**:
- 메뉴 `Tools > LittleHero > Seed Enemy HueShift (chapter 별 자동 분배)`
- 262 EnemyData SO 의 hueShift 자동 분배:
  - V1 11종 (Chapter_01_RedHood 등) → 0 (원본)
  - Meta (CH 1~5) → 0
  - Nature (CH 6~14) → 챕터 번호 × 0.015 (CH6=0.015, CH14=0.135)
  - Animal (CH 15~20) → -0.05 ~ 0.05 (랜덤 미세)
  - Civ (CH 21~27) → 0.1 ~ 0.2
  - Fantasy (CH 28~35) → -0.15 ~ -0.05 (시안/파랑 톤 강화)
  - Daily 7 / Weekly 4 → 0 (보스 원본 보존)

**검증 기준**:
- 메뉴 실행 시 262 SO 모두 hueShift 적용
- 멱등 (재실행 시 동일 결과)
- AssetDatabase.SaveAssets 후 변경 반영

#### Sprint 2 commit

```
5008 Sprint 2 / T02 — EnemyHueShiftSeed Editor 메뉴 + 262 SO 분배 (2/7)

[변경]
- Scripts/Editor/EnemyHueShiftSeed.cs 신규 (~50L)
  - 'Tools > LittleHero > Seed Enemy HueShift'
  - 262 EnemyData chapter 별 자동 분배 (V1=0 / Nature 점진 / Civ +0.1~0.2 / Fantasy -0.15~-0.05)
  - 멱등 (Editor 재실행 안전)

[검증]
- 컴파일 errors=0
- 30 archetype × 5 hue 변형 = 150 시각 ✅
- §15.4 SO 우선

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 3 — EnemyVisibilityCuller (1 commit)

#### 3.1 단일 manager + EnemyController hook

**파일**:
- `Assets/_Project/Scripts/Gameplay/Enemies/EnemyVisibilityCuller.cs` 신규 (~70L)
- `Assets/_Project/Scripts/Gameplay/Enemies/EnemyController.cs` — Initialize 시 Register, OnDisable 시 Unregister

**산출물**:
- 단일 LateUpdate (Camera.main.WorldToViewportPoint 기반)
- _marginViewport = 0.1f (SerializeField, 디자이너 튜닝)
- _renderers List<SpriteRenderer> (capacity 1024, GC 0)
- visible toggle 변경 시만 sr.enabled 갱신

**검증 기준**:
- 컴파일 errors=0
- 화면 밖 적 sr.enabled = false
- 화면 진입 시 즉시 enabled = true
- LateUpdate (모든 적 update 후 한 번)

#### Sprint 3 commit

```
5008 Sprint 3 / T03 — EnemyVisibilityCuller 단일 manager + 등록 hook (3/7)

[변경]
- Scripts/Gameplay/Enemies/EnemyVisibilityCuller.cs 신규 (~70L)
  - Camera viewport rect 기반 SpriteRenderer.enabled 토글
  - _marginViewport SerializeField (default 0.1, ±10% 여유)
  - 단일 LateUpdate + List<SpriteRenderer> (1024 capacity, GC 0)
  - Register/Unregister 정적 API
- Scripts/Gameplay/Enemies/EnemyController.cs
  - Initialize 시 Register, OnDisable 시 Unregister hook

[검증]
- 컴파일 errors=0
- 화면 밖 적 enabled=false (GPU 절감)
- §15.6 70L / §15.11 GC 0 / §15.7 정적

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 4 — comfyui-image LayerDiffuse 워크플로 (1 commit)

#### 4.1 LayerDiffuse 워크플로 + SKILL.md 가이드

**파일**:
- `.claude/skills/comfyui-image/assets/workflow_layerdiffuse.json` 신규
- `.claude/skills/comfyui-image/SKILL.md` 가이드 추가
- `.claude/skills/comfyui-image/references/comfyui_api.md` LayerDiffuse 노드 명세

**산출물**:
- DreamShaperXL Turbo + ComfyUI-layerdiffuse 노드 통합 워크플로
- SDXL native transparency 생성 (RGBA 직접 출력)
- `--workflow assets/workflow_layerdiffuse.json` 사용 시 alpha 정확도 100%

**검증 기준**:
- comfyui-image 스킬 호출 시 `--workflow workflow_layerdiffuse.json` 인자 동작
- 사용자 ComfyUI 에 huchenlei/ComfyUI-layerdiffuse 설치 사전 조건 (SKILL.md 명시)

#### Sprint 4 commit

```
5008 Sprint 4 / T04 — comfyui-image 스킬 LayerDiffuse 워크플로 추가 (4/7)

[변경]
- .claude/skills/comfyui-image/assets/workflow_layerdiffuse.json 신규
  - DreamShaperXL Turbo + LayerDiffuse 노드 통합 (transparent native)
  - SDXL RGBA 직접 출력, rembg 후처리 불필요
- .claude/skills/comfyui-image/SKILL.md
  - LayerDiffuse 워크플로 사용 가이드
  - 사전 조건: huchenlei/ComfyUI-layerdiffuse Custom Node 설치

[검증]
- comfyui-image 스킬 정상 동작 (기존 워크플로 회귀 X)
- LayerDiffuse 노드 미설치 시 fallback (기존 워크플로 사용)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 5 — 9 PNG LayerDiffuse 재생성 (1 commit)

#### 5.1 손상 9 PNG 재생성

**파일**:
- `Assets/_Project/Art/Generated_v2/Enemies/{9개 list}` 재생성
- 사용자가 huchenlei/ComfyUI-layerdiffuse 설치 후 재생성

**대상 9개**:
- Enemy_Nature_Medium / V1/Enemy_StarCloud
- CH14_Small / CH22_Medium / CH25_Small / CH30_Boss / CH33_Medium / CH33_MiniB / CH34_Small

**산출물**:
- LayerDiffuse 워크플로 사용 → alpha 정확 PNG (transparent ratio 30~70%)
- Unity 자동 임포트 + 검증

**검증 기준**:
- 9 PNG 모두 transparent ratio 30~70% (이전 0% → 정상화)
- Pillow 픽셀 분석 통과
- Unity Play Mode 진입 시 캐릭터 자연스럽게 표시 (회색 박스 X)

#### Sprint 5 commit

```
5008 Sprint 5 / T05 — 9 PNG LayerDiffuse 재생성 + Unity 임포트 (5/7)

[변경]
- Art/Generated_v2/Enemies/ 9개 PNG (alpha 손상 → 정상)
  - Enemy_Nature_Medium / V1/Enemy_StarCloud
  - CH14_Small / CH22_Medium / CH25_Small / CH30_Boss
  - CH33_Medium / CH33_MiniB / CH34_Small
- LayerDiffuse SDXL RGBA native 생성 (이전 ComfyUI Turbo 의 alpha 미생성 한계 영구 해결)

[검증]
- transparent ratio 30~70% (Pillow 분석)
- Unity 자동 임포트 + Sprite import 정상
- Play Mode 시 회색 박스 X (사용자 1차 보고 영구 해결)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 6 — NUnit + Validator (1 commit)

#### 6.1 EnemyVisualPipelineTests

**파일**: `Assets/_Project/Tests/Editor/EnemyVisualPipelineTests.cs`

**산출물 (5+ NUnit)**:
- `EnemyData_HasHueShift_Field` (Reflection)
- `EnemyVisibilityCuller_Type_Exists`
- `EnemyController_HasMaterialPropertyBlock_Static` (Reflection — _mpb private static field)
- `EnemyData_Default_HueShift_IsZero` (default 값 검증)
- `Resolver_HueShift_ZeroReturnsWhite` (정적 헬퍼 검증, optional)

**검증 기준**:
- 5/5 PASS (누적 NUnit 81 → 86)

#### Sprint 6 commit

```
5008 Sprint 6 / T06 — EnemyVisualPipelineTests 5 NUnit (6/7)

[변경]
- Tests/Editor/EnemyVisualPipelineTests.cs 신규
  - EnemyData_HasHueShift_Field
  - EnemyVisibilityCuller_Type_Exists
  - EnemyController_HasMaterialPropertyBlock_Static
  - EnemyData_Default_HueShift_IsZero
  - Resolver_HueShift_ZeroReturnsWhite

[검증]
- Test Runner 5/5 PASS
- 누적 NUnit 81 → 86

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 7 — docs (1 commit)

**작업**:
- `docs/little_hero/changelog_v1.md` — 5008 항목
- `CLAUDE.md §11` — 마일스톤 row
- `CLAUDE.md §12` — Gameplay 시스템 + Tests 81→86
- `CLAUDE.md §14` — 최근 5건 회전

#### Sprint 7 commit

```
5008 Sprint 7 / T07 — docs (changelog + CLAUDE.md §11/§12/§14) (7/7)

[변경]
- docs/little_hero/changelog_v1.md — 5008 enemy_visual_pipeline_v2 항목 (7 commit)
- CLAUDE.md §11 — 마일스톤 row
- CLAUDE.md §12 — EnemyVisibilityCuller / hueShift / LayerDiffuse + Tests 81→86
- CLAUDE.md §14 — 최근 5건 회전 (5008 등재)

[검증]
- Phase 5 종결 (7/7 commit)
- 사용자 1차 보고 (alpha 손상) 영구 해결
- 30 archetype × 5 hue = 150 변형
- Off-screen culling — cap 300 환경 GPU 절감

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
Sprint 1 (hueShift + MPB) ───→ Sprint 2 (Seed)
Sprint 3 (Culler) ─── (병렬, EnemyController hook 만 추가)
Sprint 4 (LayerDiffuse 스킬) ───→ Sprint 5 (9 PNG 재생성)
                                                ↓
                              Sprint 6 (NUnit) ←─ Sprint 1, 3 의존
                                  ↓
                              Sprint 7 (docs)
```

**병렬 가능**: Sprint 1+3+4 (각 독립)
**순차**: Sprint 2 (Sprint 1 후), Sprint 5 (Sprint 4 후), Sprint 6 (1+3 후), Sprint 7 (마지막)

## 4. 검증 시나리오 (Play 검증)

### S1 — Hue 다양성

1. CH 6 진입 (Nature, hueShift +0.09)
2. CH 14 진입 (Nature, hueShift +0.135)
3. CH 22 진입 (Civ, hueShift +0.15)
- **검증**: 같은 sprite 임에도 chapter 별 색상 차이 명확

### S2 — Off-screen culling

1. 적 cap 300 환경 wave 진입
2. Profiler 또는 Stats 창에서 SetPass calls / draw calls 측정
- **검증**: 화면 밖 적 sr.enabled = false → GPU 비용 절감

### S3 — Alpha 정확도 (사용자 1차 보고)

1. CH 14 / CH 22 / CH 25 / CH 30 / CH 33 / CH 34 챕터 wave 진입
2. 적 시각 확인
- **검증**: 회색 박스 / 흰 박스 X, 캐릭터만 자연스럽게 표시

### S4 — Cap 300 GC <50KB

1. cap 300 wave 진행
2. Profiler GC Allocation/sec 측정
- **검증**: <50KB/s (CLAUDE.md §7), 60fps 유지

## 5. Definition of Done

- [ ] Sprint 1~7 모두 commit (7 commit)
- [ ] EnemyData.hueShift Range(-1~1) + ApplyAppearance MPB
- [ ] EnemyVisibilityCuller (~70L) + EnemyController hook
- [ ] EnemyHueShiftSeed Editor 메뉴 + 262 SO 분배
- [ ] comfyui-image LayerDiffuse 워크플로 + SKILL.md 가이드
- [ ] 9 PNG LayerDiffuse 재생성 (사용자 LayerDiffuse 노드 설치 의존)
- [ ] NUnit EnemyVisualPipelineTests 5+ PASS (누적 81→86)
- [ ] CLAUDE.md §11/§12/§14 + changelog 등재
- [ ] Play 시나리오 S1~S4 모두 PASS
- [ ] §15 OOP 정합 (SRP Batcher 호환, GC 0)

## 6. 알려진 위험 + 완화

| 위험 | Sprint | 완화 |
|---|---|---|
| MPB SetPropertyBlock SRP Batcher 깨질 우려 | 1 | Material 인스턴스화 X 만 사용 (조사 결과 호환) |
| 262 SO hueShift 누락 | 2 | EnemyHueShiftSeed 멱등 + 검증 룰 |
| Camera.main null (씬 전환) | 3 | _cam null check + 재시도 |
| LayerDiffuse 노드 설치 의존 (사용자) | 4, 5 | SKILL.md 가이드 + huchenlei/ComfyUI-layerdiffuse URL 명시 |
| hueShift ±1 극값 톤 깨짐 | 1 | Range -0.3~0.3 권장, EnemyHueShiftSeed default ±0.15 이내 |
| NUnit 81 → 86 (HitEffectV2 무관) | 6 | 5007 회귀 검사 NUnit 추가 (EnemyController_5007_RegressionPass) |
| sticker V5 톤 일관성 손실 | 1, 2 | hueShift ±0.05 미세 변형만 default, 사용자 Inspector 조정 |

## 7. 변경 이력

- 2026-05-04: 초기 작성 (Phase 1, 옵션 D1=A/D2=A/D3=A/D4=A/D5=A/D6=A 권장)
