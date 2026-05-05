# Character Animation Research v1

> **작성일**: 2026-05-06
> **대상**: Little Hero by the Lake — 주인공·적·보스의 자연스러운 움직임 표현
> **범위**: 이미지 제작 단계부터 런타임 구현까지의 전체 파이프라인 비교 조사
> **현재 단계**: Phase 4 출시 진행 중 — 리스크 최소화하면서 시각 임팩트 우선

---

## 0. 한 줄 결론

현재 Little Hero는 **정적 단일 PNG + LitMotion 절차적 효과** 구조이며, **412장 V5 sticker를 버리지 않고** 그 위에 움직임을 입히는 **3 트랙 병행** 전략이 최적이다.

| 트랙 | 대상 | 이미지 작업 | 코드 작업 | 기간 | 우선순위 |
|---|---|---|---|---|---|
| **A. 절차적 강화** | 모든 캐릭터 (주인공·적·보스) | 0 (pivot만 변경) | LitMotion 5 모션 추가 | 2~5일 | **★ 즉시** |
| **B. 주인공 PSB Skeletal Rig** | 주인공 3 (전사·궁수·마법사) | V5 1장 → PSB 부위 분리 | PSD Importer + Animator | 1~2주 | 출시 전 |
| **C. 보스 Sprite Sheet** | 보스 11 (Daily 7 + Weekly 4) | 4 state × 4 frame = 176장 | Aseprite Importer 자동 | 3~4주 | 출시 후 |

---

## 1. 현재 상태 진단 (정확한 출발점)

| 항목 | 현재 |
|---|---|
| Player/Enemy 외형 | **단일 PNG 1장/캐릭터** (V5 sticker 768×1024) |
| Animator/AnimationClip | **0개** — 완전 부재 |
| 절차적 모션 | EnemyController **wobble (sin 4Hz) + LitMotion punch/flash** (5016) |
| Sprite import | spriteMode=Multiple 이지만 실제로는 단일 프레임만 사용 |
| 2D Animation 패키지 | **이미 설치** (`com.unity.2d.animation`, `com.unity.2d.psdimporter`) — **미사용** |
| flipX | velocity.x 부호로 좌우 반전만 |

**핵심**: 쓸 도구는 다 있는데 활용이 0인 상태. 추가 패키지 도입 거의 불필요.

### 1.1 현재 코드 위치

| 파일 | 핵심 |
|---|---|
| `Assets/_Project/Scripts/Gameplay/Player/PlayerController.cs:103` | `Rigidbody2D.linearVelocity` 직접 제어 |
| `Assets/_Project/Scripts/Gameplay/Player/PlayerController.cs:86` | `_sr.flipX = vx < 0f` |
| `Assets/_Project/Scripts/Gameplay/Enemies/EnemyController.cs:91~120` | `UpdateAnim` — sin 4Hz wobble + flipX |
| `Assets/_Project/Scripts/Gameplay/Enemies/EnemyController.cs:165~189` | `DoPunch`/`DoFlash` — LitMotion LSequence |
| `Assets/_Project/Scripts/Gameplay/Enemies/EnemyAnimationManager.cs` | 1,000+ 적 단일 Update 통합 (GC 0) |
| `Assets/_Project/Scripts/Data/CharacterData.cs:12` | `portrait` Sprite — 단일 idle |
| `Assets/_Project/Scripts/Data/EnemyData.cs:11` | `icon` Sprite — 단일 idle |

---

## 2. 2D 캐릭터 움직임 — 3 접근 방법 비교

| 방법 | 이미지 작업량 | 표현력 | GC/성능 | Little Hero 적합도 |
|---|---|---|---|---|
| **A. 절차적 (Procedural)** — 1장 + tween | **0 추가** (현재 자산 그대로) | ★★ (호흡·홉·기울기·squash) | 최상 (alloc 0) | **즉시 적용 ★★★★★** |
| **B. Frame-by-frame Sprite Sheet** | 캐릭터당 16~64장 | ★★★★ (자연스러움) | 중 (텍스처 메모리↑) | 보스만 ★★★★ |
| **C. Skeletal Rig (PSB)** | 캐릭터당 1 PSB (분리 레이어) | ★★★★★ (자유도 최상) | 상 (bone CPU↑) | 주인공 3 ★★★★ |

각 방법은 **이미지 제작 단계에서 다른 준비**를 요구한다.

---

## 3. 이미지 제작 단계의 "움직임 고려" — 방법별 워크플로우

### 3-A. 절차적 — 이미지 준비 거의 변경 없음

- 현재 V5 sticker 그대로 사용 가능
- **단 하나의 권장 변경**: pivot을 `(0.5, 0.5)`에서 **`(0.5, 0.1)`** (발 밑) 으로
  - 이유: landing squash, jump anticipation 적용 시 발이 기준점이 되어야 자연스러움
- 어깨/머리에 약간의 여백 (호흡 wobble 시 잘리지 않도록) — 현재 768×1024 캔버스로 충분

### 3-B. Frame Sprite Sheet — 여러 포즈 미리 생성

**핵심 원칙**: 첫 프롬프트부터 "동일 캐릭터의 다른 포즈"를 기획

ComfyUI 다중 포즈 생성 워크플로우:

```
Step 1: 첫 프레임으로 Reference Image 생성 (V5 sticker 그대로)
Step 2: 동일 시드 + IP-Adapter (얼굴/의상 reference)
Step 3: ControlNet OpenPose — 포즈 N개 (idle/walk1/walk2/attack1/attack2/hit)
Step 4: 배경 제거 (rembg) → Aseprite/Photoshop에서 sprite sheet 정렬
Step 5: Unity Aseprite Importer (Unity 6 native) → 자동 AnimationClip 생성
```

**한계**: 게임 sprite는 "exaggerated proportions, occluding costumes" 때문에 일반 OpenPose 추정이 어렵다. ([Sprite Sheet Diffusion 논문 2024](https://arxiv.org/html/2412.03685v2))

권장 pose set (액션 슈터 기준):

| 상태 | frame 수 | 비고 |
|---|---|---|
| idle | 1~2 | 눈 깜빡임 정도 |
| walk | 4 | 좌우 양다리 교차 |
| attack | 3 | anticipation → 임팩트 → recovery |
| hit/death | 2 | — |
| **합계** | **12~16 frame/캐릭터** | |

**Unity 6 Aseprite Importer 의 가장 큰 이점**: `.aseprite` 파일을 그대로 import하면 frame tag가 AnimationClip으로 자동 변환. 수동 슬라이싱·키프레임 작업 0.

### 3-C. Skeletal Rig (PSB) — 부위별 레이어로 분리

**핵심 원칙**: 한 장의 멀쩡한 캐릭터를 → 머리/몸통/팔(2)/다리(2)/꼬리/장비 별도 레이어

#### 경로 1: AI 생성 후 분리 (V5 sticker 412장 활용 — **권장**)

```
1. ComfyUI에서 V5 portrait 생성 (현재 그대로)
2. Segment Anything (SAM) / rembg / Photoshop Object Selection
   → 부위별 마스크 추출
3. Photoshop Generative Fill / inpaint
   → 가려진 부분 복원 (예: 팔 뒤쪽 몸통)
4. PSB로 저장 (각 부위 = 별도 레이어, T-pose 권장이지만 정면 idle 도 가능)
5. Unity PSD Importer:
   - Texture Type = Sprite (2D and UI)
   - Sprite Mode = Multiple
   - Mosaic = ON (각 레이어를 별도 sprite로)
   - Character Rig = ON (Prefab 자동 생성)
   - Use Layer Grouping = ON
6. Sprite Editor → Skinning Editor:
   - Auto-Bone (Unity 6 에서 sprite silhouette 기반 자동 추천)
   - 수동 weight 보정 (관절 부근)
7. Animator Controller로 idle/walk/attack 클립 작성
   (혹은 LitMotion으로 bone Transform 직접 트윈)
```

#### 경로 2: 처음부터 부위별 생성

- ComfyUI에서 "T-pose" "split body parts" 프롬프트
- V5 sticker 톤과 충돌, 결과 일관성 낮음
- **권장 X** — 경로 1이 V5 sticker 자산을 살리면서 안전

---

## 4. Little Hero 맞춤 단계별 권장안

> Phase 4 출시 진행 중 → 리스크 최소화하면서 가장 큰 시각 임팩트 우선

### 4.1 Phase A — 절차적 강화 (즉시, 2~5일, 0 리스크) **★ 최우선 ★**

현재 LitMotion(5016) 인프라 위에 **5 종 절차적 모션** 추가:

| 모션 | 트리거 | 구현 패턴 |
|---|---|---|
| **Idle bob** | velocity ≈ 0 일 때 | 무한 loop, scale Y ±0.05, 1.6초 yoyo, InOutSine |
| **Walk bob** | velocity > 0 | bob 진폭/속도 = velocity 비례 (현재 wobble과 합성) |
| **Lean tilt** | 가속 시작/멈춤 | Z회전 ±5° (관성 표현) |
| **Anticipation squash** | 공격 직전 100ms | scale Y 1→0.85, X 1→1.1 |
| **Landing squash** | 보스 등장·점프 후 | scale Y 1→0.7→1.05→1 (3-step LSequence) |

**효과**:
- 이미지 변경 0, 코드만 수정
- PlayerController/EnemyController 각 ~30~50라인 추가
- pivot만 발 밑 `(0.5, 0.1)`으로 변경하면 squash가 훨씬 자연스러움 (sprite import 일괄 변경)

### 4.2 Phase B — 주인공 3종 PSB Skeletal Rig (출시 전 권장, 1~2주)

**왜 주인공만**: 사용자가 가장 오래 보는 캐릭터. 적은 1.5초 만에 죽어 무관.

워크플로우:

1. CharacterData 의 portrait 3장 (전사/궁수/마법사) 만 작업
2. SAM/Photoshop으로 부위 분리 → PSB
3. Unity PSD Importer + Skinning Editor (Auto-Bone Unity 6.x)
4. AnimationClip: idle / run / attack / hit / death (5 클립 × 3 캐릭터 = 15 클립)
5. Animator Controller — Blend Tree (Speed parameter)
6. PlayerController flipX 로직 → bone scale.x 반전으로 교체

**고급 팁**: bone에 직접 LitMotion 트윈 → AnimationClip 0개로도 절차적 idle/walk 가능. AnimationClip은 attack/hit 등 키프레임 필요한 것만 작성.

### 4.3 Phase C — 보스 11종 sprite sheet (장기, 출시 후)

- 11 boss × 4 state(idle/attack/hit/death) × 4 frame = 176 frame
- Aseprite 수작업 또는 ComfyUI ControlNet OpenPose + IPAdapter (consistency)
- Unity 6 Aseprite Importer로 자동 import → AnimationClip
- 일반 적 262종은 Phase A 절차적으로 충분 (1.5초 만에 죽으므로)

---

## 5. 즉시 적용 가능한 코드 예시 (Phase A)

> PlayerController/EnemyController에 들어갈 패턴. 기존 5016 LitMotion 인프라 그대로 활용. GC 0.

### 5.1 Idle bob — velocity 0일 때 호흡

```csharp
private MotionHandle _idleBobHandle;

private void StartIdleBob() {
    _idleBobHandle = LMotion.Create(-0.04f, 0.04f, 1.6f)
        .WithEase(Ease.InOutSine)
        .WithLoops(-1, LoopType.Yoyo)
        .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
        .BindToLocalPositionY(_sr.transform);
}

private void StopIdleBob() => _idleBobHandle.TryCancel();
```

### 5.2 Anticipation squash — 공격 직전

```csharp
public void PlayAnticipation() {
    LSequence.Create()
        .Append(LMotion.Create(Vector3.one, new Vector3(1.1f, 0.85f, 1f), 0.10f)
            .WithEase(Ease.OutQuad).BindToLocalScale(_sr.transform))
        .Append(LMotion.Create(new Vector3(1.1f, 0.85f, 1f), Vector3.one, 0.08f)
            .WithEase(Ease.OutBack).BindToLocalScale(_sr.transform))
        .Run();
}
```

### 5.3 Landing squash — 보스 등장·점프 후

```csharp
public void PlayLandingSquash() {
    LSequence.Create()
        .Append(LMotion.Create(Vector3.one, new Vector3(1.2f, 0.7f, 1f), 0.08f)
            .WithEase(Ease.OutQuad).BindToLocalScale(_sr.transform))
        .Append(LMotion.Create(new Vector3(1.2f, 0.7f, 1f), new Vector3(0.95f, 1.05f, 1f), 0.10f)
            .WithEase(Ease.OutQuad).BindToLocalScale(_sr.transform))
        .Append(LMotion.Create(new Vector3(0.95f, 1.05f, 1f), Vector3.one, 0.12f)
            .WithEase(Ease.OutBack).BindToLocalScale(_sr.transform))
        .Run();
}
```

**참고**: 이 패턴 5종을 PlayerController 에 넣으면 **이미지 0장 추가 없이** 캐릭터가 살아있는 느낌이 즉시 생긴다. 메이플 키우기/뱀파이어 서바이버류 인디 게임 다수가 이 수준에서 멈춤 — 절차적만으로 "충분히 자연스러움"을 달성.

---

## 6. AI Sprite → Skeletal Rig 파이프라인 (Phase B 상세)

V5 sticker 한 장을 PSB로 변환하는 구체 단계:

```
Input:  Assets/_Project/Art/Characters/Warrior_v5.png  (768×1024)
Output: Assets/_Project/Art/Characters/Warrior_v5.psb  (8~12 layer)
```

### 6.1 Photoshop 작업

1. Photoshop 열기
2. Object Selection Tool 또는 SAM 플러그인
   - 부위별 선택: head / torso / arm_left / arm_right / leg_left / leg_right / weapon / cloak
3. 각 선택 → 새 레이어로 복사 (Cmd+J)
4. 가려진 부분 (예: 팔 뒤쪽 몸통) → Generative Fill 또는 Clone Stamp
5. T-pose 변형이 어렵다면 정면 idle 자세 그대로도 가능 (Unity Skinning Editor에서 bone deform 가능)
6. `.psb` 로 저장 (PSD 가 아닌 PSB — Unity PSD Importer 권장 포맷)

### 6.2 Unity Import 설정

`Assets/_Project/Art/Characters/` 에 import 후 Inspector:

| 항목 | 값 |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Multiple |
| Mosaic | ✓ |
| Character Rig | ✓ |
| Use Layer Grouping | ✓ |
| Pivot | Bottom Center (발 밑) |

### 6.3 Skinning Editor 작업

1. **Auto Geometry** → 자동 mesh 생성
2. **Bone Tool** → 골반에서 시작, 각 부위로 뻗어나가는 bone 생성
   - Unity 6 의 Auto-Bone 으로 silhouette 기반 자동 추천 가능
3. **Auto Weights** → 자동 weight
4. **Weight Brush** → 관절 부근 수동 보정

→ Unity 자동으로 **SpriteSkin 컴포넌트 부착된 Prefab** 생성. AnimationClip 또는 LitMotion으로 bone Transform 트윈하면 끝.

---

## 7. 비용·일정·결정 매트릭스

| Phase | 작업량 | 시각 효과 | 리스크 | 권장 시점 |
|---|---|---|---|---|
| A 절차적 | 2~5일 (코드만) | ★★ → ★★★ | 최저 (LitMotion 검증 완료) | **즉시** |
| B 주인공 PSB | 1~2주 (작업+rig+테스트) | ★★★★ | 중 (PSD Importer 학습) | 출시 전 |
| C 보스 sheet | 3~4주 (이미지 다량) | ★★★★ | 중 (AI consistency 어려움) | 출시 후 |

### 7.1 권장 순서 결정 근거

- 사용자가 가장 오래 보는 = 주인공 (Attic + 챕터 모두) → Phase B 가성비 높음
- 적 262종은 Phase A 만으로 충분 (수명 짧음)
- Aseprite Importer는 무료·Unity 6 native 지만 V5 sticker 톤과 픽셀 아트 톤이 다름 → 보스만 한정 적용

---

## 8. 외부 도구 비교 — Spine vs Unity 2D Animation vs DragonBones

| 도구 | 비용 | Unity 통합 | 모바일 | Little Hero 결론 |
|---|---|---|---|---|
| Spine | $69~$399 | Spine Runtime 필요 | 우수 (런타임 최적화) | 과투자 — 주인공 3개에 $69도 비쌈 |
| **Unity 2D Animation** | **무료** | **네이티브** | 양호 | **★ 추천** — 이미 설치됨 |
| DragonBones | 무료 | 별도 SDK | 매우 가벼움 | 모바일 앱 강점이지만 게임 RTL 약함 |

**결론**: Unity 2D Animation 으로 충분. Spine은 4컷 이상 풀 컷씬 필요할 때만. 외부 도구 신규 도입 0.

---

## 9. 12 Animation 원칙 중 게임 핵심 5개

Disney 12 원칙 중 2D 게임 절차적 모션에 직결되는 5개:

| 원칙 | Little Hero 적용 |
|---|---|
| **Squash and Stretch** | 공격 anticipation, landing — Phase A 5.2/5.3 |
| **Anticipation** | 공격 직전 100ms 압축 — Phase A 5.2 |
| **Follow Through / Overlapping Action** | 망토·꼬리·머리카락이 본체보다 한 박자 늦게 따라옴 (별도 bone + 가벼운 spring 트윈) |
| **Slow In / Slow Out (Easing)** | 모든 LitMotion 트윈에 Ease.InOutSine / Ease.OutBack 활용 |
| **Secondary Motion** | idle bob, breathing, eye blink — Phase A 5.1 |

---

## 10. 다음 액션 (제안)

1. **Phase A 즉시 시작 의사 결정**
   - 사용자 승인 시 PlayerController/EnemyController 에 5 절차적 모션 패치
   - sprite pivot 일괄 `(0.5, 0.1)` 변경
   - 별도 Sprint 로 commit
2. **Phase B 는 출시 전 별도 PRD 작성 권장**
   - `/prd-pipeline character_skeletal_rig_v1`
   - V5 sticker → PSB 워크플로우 검증
   - 주인공 3 시범 후 적용 결정
3. **Phase C 는 출시 후 보스 11종에 한정**
   - Aseprite Importer 또는 ComfyUI 다중 포즈 PoC 후 결정

---

## 11. 출처

### Unity 2D Animation 공식
- [Preparing character artwork — Unity 2D Animation 6.0.7](https://docs.unity3d.com/Packages/com.unity.2d.animation@6.0/manual/PreparingArtwork.html)
- [Preparing and importing artwork — Unity 2D Animation 9.1.3](https://docs.unity3d.com/Packages/com.unity.2d.animation@9.1/manual/PreparingArtwork.html)
- [Rigging a character imported with the PSD Importer — 2D Animation 9.0.4](https://docs.unity3d.com/Packages/com.unity.2d.animation@9.0/manual/ex-psd-importer.html)
- [How to speed up 2D art workflows with 2D PSD Importer — Unity Blog](https://blog.unity.com/technology/how-to-speed-up-2d-art-workflows-with-2d-psd-importer)
- [Unity 6 Aseprite Importer manual](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.2d.aseprite.html)

### 애니메이션 이론
- [The fundamentals of modern 2D Animation Theory using Unity Tools — MY.GAMES](https://medium.com/my-games-company/the-fundamentals-of-modern-2d-animation-theory-using-unity-tools-0c7fe80758ff)
- [How to Create Fluid Character Movements Using Unity's 2D Animation Package — Morphic Studio](https://www.themorphicstudio.com/how-to-create-fluid-character-movements-using-unitys-2d-animation-package/)
- [12 Principles for Game Animation — Chris Totten](https://totter87.medium.com/12-principles-for-game-animation-a9137ef44345)
- [Simulating Breathing and Idle Motion Procedurally — Palos Publishing](https://palospublishing.com/simulating-breathing-and-idle-motion-procedurally/)
- [Squash and Stretch in Animation — Fox Render Farm](https://www.foxrenderfarm.com/news/squash-and-stretch-in-animation/)

### AI Sprite 생성
- [Sprite Sheet Diffusion: Generate Game Character for Animation (arXiv 2024)](https://arxiv.org/html/2412.03685v2)
- [Quick OpenPose Character Concept Sheet Creator — OpenArt](https://openart.ai/workflows/lord_lethris/quick-openpose-character-concept-sheet-creator-v12/F0Cz1ZX0xU7UebTQFyPs)
- [Create a consistent character animation sprite using ComfyUI — TawusGames](https://tawusgames.itch.io/ai-gen-sprite-tutorial)
- [2D Asset Generation: AI for Game Development #4 — Hugging Face](https://huggingface.co/blog/ml-for-games-4)

### 도구 비교
- [Spine vs Unity 2D Animation — RetroStyle Games](https://retrostylegames.com/blog/unity-2d-animation-vs-spine/)
- [Spine vs DragonBones Pro — Slant 2026](https://www.slant.co/versus/1900/15725/~spine_vs_dragonbones-pro)

---

## 12. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (조사) | 초안 작성 — Phase 4 출시 단계 진단 + 3 트랙 권장 |
