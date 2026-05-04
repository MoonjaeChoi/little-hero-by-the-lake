# 5008 — Enemy Visual Pipeline v2 (적 시각 파이프라인 v2)

> **목적**: 적 캐릭터 자산 시각 품질·다양성·성능 동시 개선. 심층 조사 기반 즉시 적용 가능한 3 개선 (MPB hue shift + Off-screen culling + LayerDiffuse 재생성).
> **연계**: 5007 damage_hit_effect_v2 (Resolver 인프라) / 137-262 EnemyData SO / 262 PNG (V5 sticker tone) / Vampire Survivors 류 cap 300 환경
> **출처**: 2026-05-04 Unity 적 캐릭터 자산 최신 기법 심층 조사 (18 references) — LayerDiffuse + MaterialPropertyBlock + Sprite Atlas V2 + Off-screen culling

## 0. 결정 이력

- 2026-05-04: 사용자 발화 "PRD 5008_enemy_visual_pipeline_v2 작성" → 본 디자인 문서 작성
- 사용자 보고: 1라운드 적 캐릭터 일부 흰 박스 / 주인공 미표시 → 23 PNG alpha 손상 발견 → 14개 flood-fill 처리 + 9개 ComfyUI 재생성
- 심층 조사 결과: ① LayerDiffuse 가 근본 해결 ② MPB hue shift = 30 archetype × 5 hue → 150 변형 ③ Off-screen culling 미적용

## 1. 현재 시스템 분석

### 1.1 적 시각 자산 인프라

| 항목 | 현황 | 평가 |
|---|---|---|
| EnemyData SO | 262 인스턴스 (35 챕터 × 6 + V1 11 + Animal 30 + Daily 7 + Weekly 4) | ✅ 충분 |
| Generated_v2/Enemies PNG | 251 + V1 11 = 262 | ✅ |
| EnemyAtlas.spriteatlas | 1개 존재 | ✅ 활용도 낮음 (자동 packing 미설정) |
| ApplyAppearance (EnemyController:178) | `sr.color = data.tintColor` 적용 | ✅ MPB 의 자연 확장점 |
| EnemyAnimationManager | 단일 Update 1024 capacity | ✅ §15.6 모범 |
| Off-screen culling | ❌ 없음 | 화면 밖 적도 sr.enabled=true |
| Material 인스턴스화 | X (sr.color 만 변경) | ✅ SRP Batcher 호환 |

### 1.2 PIVOT v4 cinematic 통합 (5007 완료)

- HitEffectResolver 가 BossTier × WeaponCategory × intensity 분기
- CinematicEffectsConfig.asset 에서 25+ 필드 SO 화
- 본 5008 은 이 인프라 위에 **시각적 다양성 + alpha 정확도 + 성능 cull** 추가

### 1.3 위반·한계

| 항목 | 상태 | 영향 |
|---|---|---|
| Alpha 손상 9 PNG (회색 gradient + 흰 캐릭터) | ❌ flood-fill 분리 불가 | 적 1라운드 미표시 — 사용자 보고 |
| 30 archetype 재사용 | △ tintColor 만 변형 | Hue shift 미사용 — 색상 다양성 ↓ |
| 화면 밖 적 SpriteRenderer | △ enabled 항상 true | GPU draw call cap 300 환경 일부 낭비 |
| EnemyAtlas 자동 packing 비활성 | △ asset 1 존재하나 효과 미확인 | draw call 검증 필요 |
| Idle PNG only (애니메이션 X) | △ PunchRoutine + sr.color 만 | sticker 정적 — Vampire Survivors 류는 2-6 frame 표준 |

## 2. 옵션 비교

### 2.1 D1 — 본 PRD 범위

| 옵션 | 포함 사항 | commit | 시간 |
|---|---|---|---|
| **A (★권장)** | **MPB hue + Off-screen culling + LayerDiffuse 9 PNG 재생성** (심층 조사 즉시 권장 3) | 7 | 4h |
| B | A + LoRA 훈련 가이드 | 9 | 6h |
| C | A + IPAdapter idle/run/attack 자동화 | 12 | 9h |
| D | A + Sprite Atlas 자동 packing 설정 | 8 | 5h |

> 권장 **A** — ROI 최고 + 사용자 즉시 체감. LoRA / IPAdapter / Atlas analyzer 는 5008.1+ 후속.

### 2.2 D2 — Hue shift 구현 방식

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A (★권장)** | **MaterialPropertyBlock + Color.HSVToRGB(hueShift, ...)** | SRP Batcher 호환, GC 0, sticker 톤 보존 | 채도/명도 미변경 |
| B | Shader Graph 2D HSV shift uniform | 풀 색공간 변환, 채도 조정 가능 | URP 2D shader 추가 작업 |
| C | Sprite 사전 처리 (Editor 메뉴) | 원본 보존 | atlas 폭증 (262 × 5 = 1310 sprite) |

> 권장 **A** — 현재 `sr.color = data.tintColor` 패턴의 자연 확장. SetPropertyBlock 만 사용.

### 2.3 D3 — Off-screen culling 구현

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A (★권장)** | **단일 manager (EnemyVisibilityCuller) Update + Camera viewport rect** | EnemyAnimationManager 패턴 답습, GC 0 | margin 계산 필요 |
| B | OnBecameVisible / OnBecameInvisible callback | 자동 callback | 1500+ object 매 프레임 callback 비용 |
| C | LayerCullDistance | 단순 | 거리 기반만 (viewport 정밀도 X) |

### 2.4 D4 — hueShift 필드 위치

| 옵션 | 설명 | 장점 |
|---|---|---|
| **A (★권장)** | **EnemyData.hueShift float (-1~1, 0=원본)** | 137-262 SO 마이그레이션, Inspector 직접 편집 |
| B | 별도 EnemyVariationConfig SO | 분리 명확 |
| C | ChapterData.tintHueShift (chapter 단위) | chapter 별 톤 통일 |

### 2.5 D5 — LayerDiffuse 통합 위치

| 옵션 | 설명 |
|---|---|
| **A (★권장)** | **comfyui-image 스킬 (.claude/skills/comfyui-image)** 에 LayerDiffuse 워크플로 추가 |
| B | 별도 스킬 신규 |
| C | 외부 사용자 작업 (스킬 변경 X) |

### 2.6 D6 — 9 PNG 재생성 시점

| 옵션 | 설명 |
|---|---|
| **A (★권장)** | **본 PRD Sprint 5 에 포함** — Phase 5 commit |
| B | 5008.1 후속 (LoRA 훈련 후 동시) |
| C | 사용자 직접 재생성 (PRD 외) |

## 3. 권장 설계

### 3.1 신규 파일 (3개)

| 파일 | 책임 | 라인 |
|---|---|---|
| `Scripts/Gameplay/Enemies/EnemyVisibilityCuller.cs` | 단일 Update — Camera viewport 외 적 SpriteRenderer.enabled 자동 토글 | ~70 |
| `Scripts/Editor/EnemyHueShiftSeed.cs` | Editor 메뉴 — chapter 별 hueShift 자동 분배 (CH 1~5 동일 / CH 6~14 변형 등) | ~50 |
| `.claude/skills/comfyui-image/assets/workflow_layerdiffuse.json` | LayerDiffuse 워크플로 (transparent native 생성) | (JSON) |

### 3.2 수정 파일 (3개)

| 파일 | 변경 |
|---|---|
| `Scripts/Data/EnemyData.cs` | `hueShift` float 필드 신규 (range -1~1, default 0) |
| `Scripts/Gameplay/Enemies/EnemyController.cs` | ApplyAppearance 에 MaterialPropertyBlock 적용 + hueShift 반영 |
| `.claude/skills/comfyui-image/SKILL.md` | LayerDiffuse 워크플로 사용 가이드 추가 |

### 3.3 데이터 구조

```csharp
// EnemyData.cs 추가 필드
[Header("5008 — 색상 변형 (MPB hue shift)")]
[Range(-1f, 1f)]
[Tooltip("0 = 원본 색. ±0.05 = 미세 변형, ±0.2 = 큰 변형. 같은 sprite 의 chapter 별 다양성 확보.")]
public float hueShift = 0f;
```

```csharp
// EnemyController.ApplyAppearance — MPB 패턴
private static MaterialPropertyBlock _mpb;
private void ApplyAppearance(EnemyData data) {
    SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
    if (sr != null) {
        if (data.icon != null) sr.sprite = data.icon;

        // 5008 — MPB hue shift (SRP Batcher 호환, GC 0)
        Color final = ResolveTintColor(data);
        sr.color = final;  // 기존 패턴 (sticker tintColor 호환)

        // 추가: MPB 로 _Color uniform 별도 set 하여 향후 shader hue shift 도 지원
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        sr.GetPropertyBlock(_mpb);
        _mpb.SetColor("_Color", final);
        sr.SetPropertyBlock(_mpb);
    }
    // ... 기존 scale 코드
}

private static Color ResolveTintColor(EnemyData data) {
    if (data.icon == null) return data.tintColor;  // placeholder 시 tintColor

    // hueShift 적용 (HSV 변환)
    Color baseCol = Color.white;  // sprite 가 있으면 white 베이스
    if (Mathf.Abs(data.hueShift) > 0.001f) {
        Color.RGBToHSV(baseCol, out float h, out float s, out float v);
        h = Mathf.Repeat(h + data.hueShift, 1f);
        s = Mathf.Max(s, 0.15f);  // 채도 0 회피 (백색 유지)
        baseCol = Color.HSVToRGB(h, s, v);
    }
    return baseCol;
}
```

```csharp
// EnemyVisibilityCuller.cs (단일 manager)
public class EnemyVisibilityCuller : MonoBehaviour {
    private static readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>(1024);
    private Camera _cam;

    [SerializeField] private float _marginViewport = 0.1f;  // viewport ±10% 여유

    public static void Register(SpriteRenderer sr) { if (sr != null && !_renderers.Contains(sr)) _renderers.Add(sr); }
    public static void Unregister(SpriteRenderer sr) { _renderers.Remove(sr); }

    private void Awake() => _cam = Camera.main;
    private void LateUpdate() {
        if (_cam == null) return;
        for (int i = _renderers.Count - 1; i >= 0; i--) {
            var sr = _renderers[i];
            if (sr == null) { _renderers.RemoveAt(i); continue; }
            Vector3 vp = _cam.WorldToViewportPoint(sr.transform.position);
            bool visible = vp.z > 0
                && vp.x > -_marginViewport && vp.x < 1f + _marginViewport
                && vp.y > -_marginViewport && vp.y < 1f + _marginViewport;
            if (sr.enabled != visible) sr.enabled = visible;
        }
    }
}
```

### 3.4 §15 OOP 정합

| 정책 | 검증 |
|---|---|
| §15.1 SOLID-S | EnemyController (적용) + Culler (가시성) + HueShiftSeed (시드) 분리 ✅ |
| §15.4 SO 우선 | hueShift = SerializeField, 모든 magic number SO ✅ |
| §15.6 100L | 신규 모두 100L 이내 (~70 / ~50 / 워크플로 JSON) ✅ |
| §15.7 DI | Culler 정적 Register/Unregister + Camera.main cache ✅ |
| §15.11 안티 패턴 | MaterialPropertyBlock 정적 (no GC), 단일 Update Manager ✅ |
| §15.12 Validator | (선택) MPB 누락 검사 추가 가능 |

## 4. Sprint 분해 (개요)

자세한 Sprint × commit × 시간 표는 PRD 참조.

| Sprint | 작업 | 의존성 |
|---|---|---|
| 1 | EnemyData.hueShift 필드 + ApplyAppearance MPB 적용 | — |
| 2 | EnemyHueShiftSeed Editor 메뉴 + 262 SO chapter 별 분배 | 1 |
| 3 | EnemyVisibilityCuller 단일 manager + EnemyController 등록 | — |
| 4 | comfyui-image 스킬 LayerDiffuse 워크플로 추가 | — |
| 5 | 9 손상 PNG LayerDiffuse 재생성 + Unity 임포트 | 4 |
| 6 | NUnit EnemyVisualPipelineTests 5+ + Validator | 1~5 |
| 7 | docs (changelog + CLAUDE.md §11/§12/§14) | 모두 |

## 5. 알려진 위험 + 완화

| 위험 | 영향 | 완화 |
|---|---|---|
| MPB SetPropertyBlock 가 SRP Batcher 깨뜨릴 우려 | draw call ↑ | Material 인스턴스화 X 만 사용 (조사 결과 batching 호환) |
| 262 SO hueShift 마이그레이션 누락 | 일부 적 default (0) | Editor 메뉴 chapter 별 자동 분배 + 검증 룰 |
| Camera.main null (씬 전환 중) | culling 일시 비활성 | _cam null check + 다음 frame 재시도 |
| LayerDiffuse 노드 설치 사용자 책임 | 9 PNG 재생성 차단 가능 | comfyui-image SKILL.md 가이드 |
| hueShift ±1 극값 시 톤 깨짐 | Visual quality ↓ | Range -0.3~0.3 권장 (SerializeField Range attribute 그대로) |
| sticker 톤 일관성 손실 | V5 톤 깨짐 | EnemyHueShiftSeed 가 ±0.1 미세 변형만 default |

## 6. Definition of Done

- [ ] EnemyData.hueShift 필드 신규 + ApplyAppearance MPB 적용 (~30L 변경)
- [ ] EnemyVisibilityCuller 단일 manager (~70L) + 등록·해제 hook
- [ ] EnemyHueShiftSeed Editor 메뉴 + 262 SO 자동 분배
- [ ] comfyui-image 스킬 LayerDiffuse 워크플로 추가 + SKILL.md 가이드
- [ ] 9 손상 PNG LayerDiffuse 재생성 + Unity 임포트 + 검증
- [ ] NUnit EnemyVisualPipelineTests 5+ PASS (Reflection + 정적 검증)
- [ ] ProjectValidator 신규 위반 0
- [ ] CLAUDE.md §11/§12/§14 갱신 + changelog 등재
- [ ] Play 시나리오 4 통과 (S1 hue 다양성 / S2 culling / S3 alpha 정확 / S4 cap 300 GC)

## 7. 변경 이력

- 2026-05-04: 초기 작성 (Phase 1 Generate). 옵션 D1=A / D2=A / D3=A / D4=A / D5=A / D6=A 권장.

## 8. 출처

- 심층 조사 보고서 (2026-05-04, 18 references)
- ComfyUI-layerdiffuse (huchenlei, 2024) — SDXL native transparency
- ComfyUI-RMBG (1038lab, 2025) — 2025 표준 BG removal
- LitMotion (annulusgames) — DOTween 5× + GC 0 (장기 옵션)
- NSprites (Antoshidza) — DOTS sprite (장기 옵션)
- Apatero ComfyUI LoRA Training Guide 2026 (장기 옵션)
- 5007 damage_hit_effect_v2 (HitEffectResolver 패턴 답습)
- CLAUDE.md §7 cap 300 + GC <50KB / §15 OOP 정책
