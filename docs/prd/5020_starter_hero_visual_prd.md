# 5020 — Starter Hero Visual (디자인 문서)

> **문서번호**: 5020
> **시스템**: 시작 주인공 3종 V5 sticker 직업 정체성 재생성
> **작성일**: 2026-05-06
> **참조**: `docs/little_hero/character/character_animation_research_v1.md` (5019 후속 보고서)
> **선행**: 5008 enemy_visual_pipeline / 5018 texture_compression / 5019 enemy_chapter_design
> **CLAUDE.md 정합**: §15 OOP 정책 (자산 생성 + SO wiring 만, 코드 변경 ≤ 1)

---

## 1. 결정 이력

| 일자 | 사용자 발화 | 결정 |
|---|---|---|
| 2026-05-06 | "보고서를 기반으로 시작하는 주인공 캐릭터를 직업별 특성에 맞게 생성" | 5020 PRD 작성 착수 — 시작 캐릭터 3종(전사/궁수/마법사) V5 직업 정체성 재생성 |
| 2026-05-06 | "D1: 전사 시우, 궁수 가영, 마법사 연우 / D2-A / D3-B / D4-A / D5-B" | **확정**: D1 캐릭터 이름 부여, D2-A idle 1장, **D3-B id+파일명 직업 정합**, D4-A 코드 분리, **D5-B PSB reference 별도 출력** |

---

## 2. 현재 시스템 분석

### 2.1 자산 ↔ 데이터 정합성 (미스매치 발견)

| SO 파일 | id | displayName (옛 키즈) | className (stats_v2) | tintColor | STR/AGI/WIS | 정체성 |
|---|---|---|---|---|---|---|
| `Char_DefaultBoy.asset` | `default_boy` | 용감한 소년 | **전사** | (1.0, 0.85, 0.55) 따뜻한 골드 | 2.0 / 0.5 / 0.5 | Warrior |
| `Char_BraveGirl.asset` | `brave_girl` | 씩씩한 소녀 | **궁수** | (0.95, 0.65, 0.85) 핑크 | 0.5 / 2.0 / 0.5 | Archer |
| `Char_QuietFriend.asset` | `quiet_friend` | 조용한 친구 | **마법사** | (0.55, 0.85, 0.95) 청록 | 0.5 / 0.5 / 2.0 | Mage |

**핵심 미스매치**:
- `className` / multipliers 는 stats_v2 기준 직업 분화 완료 (PIVOT v2 후 갱신).
- `displayName` / portrait 는 PIVOT 이전 키즈 캐릭터 톤 그대로 — **검·활·지팡이가 보이지 않음**, 직업 정체성 시각 부재.
- portrait GUID 3개 (`f6bedfb7…` / `f0823e95…` / `2c53e767…`) — wiring 정상이지만 **그림 자체가 키즈 일러스트**.

### 2.2 현재 portrait 자산

`Assets/_Project/Art/Generated_v2/Characters/`:
```
Char_BraveGirl.png        (180KB, May 4, V5 기반 시도)
Char_BraveGirl_idle.png   (590KB, May 1, PIVOT 직후 1차)
Char_DefaultBoy.png       (164KB, May 4, V5 기반 시도)
Char_DefaultBoy_idle.png  (624KB, May 1, PIVOT 직후 1차)
Char_QuietFriend.png      (158KB, May 4, V5 기반 시도)
Char_QuietFriend_idle.png (858KB, May 1, PIVOT 직후 1차)
```

→ 실제 wiring 된 portrait 가 어느 PNG 인지 **GUID 매핑** Sprint 1 에서 확정 필요.

### 2.3 5019 까지의 학습 (재활용 가능)

- **V5 sticker baseline** 검증 완료 (5008): SDXL base 1.0 + 768×1024 + cfg 6 + Danbooru "solo, 1character" 태그.
- **LayerDiffuse RGBA workflow**: `.claude/skills/comfyui-image/assets/workflow_layerdiffuse.json` + `comfyui-image` 스킬.
- **Batch script 패턴**: `scripts/5019_chapter_narrative_batch.py` — Sprint 분해 + sub-batch + retry 패턴 답습 가능.
- **GUID 보존 정책** (5009/5010/5011/5019): PNG 만 갱신, `.meta` 건드리지 않음 → CharacterData wiring 자동 유지.
- **iOS Override ASTC 6×6 + Crunched** (5018): 신규 PNG 자동 적용 (TextureCompressionConfigurer).
- **EnemySpriteAuditor** (5008): 5008 baseline 품질 자동 검증 — 캐릭터용으로 변형 가능.

### 2.4 character_progression_v1.md 와의 불일치 (별도 문서 갱신 필요 — 본 PRD 범위 외)

- character_progression_v1.md §4.6 "캐릭터 클래스별 비중" 표가 여전히 **4 코어** (힘/빠름/슬기/마음) + 옛 키즈명 사용.
- 본 PRD 는 자산 + SO portrait 만 다루고, 진행 문서 갱신은 **별도 후속 PR** 권장.

---

## 3. 옵션 비교 (사용자 결정 항목)

### 3.1 D1 — 캐릭터 이름 (★확정)

사용자 결정으로 3 캐릭터 고유명 부여:

| 직업 | 이름 | className | 정체성 톤 |
|---|---|---|---|
| 전사 | **시우** | 전사 | 부드러운 발음 + 결의에 찬 어린이, 따뜻한 골드 톤 |
| 궁수 | **가영** | 궁수 | 명랑 활발 어린이, 핑크 + 청록 보색 |
| 마법사 | **연우** | 마법사 | 차분 사색 어린이, 청록 + 보라 별 톤 |

**displayName 형식 변형** (Sprint 3 사용자 결정):
- 변형 A (간결): `시우` / `가영` / `연우` — className 라벨 별도 표시 시
- 변형 B (명확): `전사 시우` / `궁수 가영` / `마법사 연우` — 한 줄 표기 (★권장)

> 결정 이력: D1 옵션 A/B/C 비교는 폐기 — 사용자가 직접 직업 정체성 강조 + 고유명 부여 (D1-B 강화판) 를 채택.

### 3.2 D2 — 출력 frame 수

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| D2-A (★권장) | **idle portrait 1장** | 5009/5019 답습, GUID 보존 단순, 재 review 빠름 | walk/attack 시 같은 sprite |
| D2-B | idle + walk 2 frame | Phase A 절차적 bob 효과 강화 | 2× 작업, V5 일관성 어려움 |
| D2-C | 4 state set (idle/walk/attack/hit, 각 1장) | 향후 sprite sheet 호환 | 4× 작업, sprite sheet 작업은 5021+ 별도 |

### 3.3 D3 — SO 식별자 변경 범위 (★D3-B 확정)

| 옵션 | 설명 | 장점 | 단점 | 채택 |
|---|---|---|---|---|
| D3-A | id/파일명 유지, displayName 만 직업 톤 갱신 | SaveData 호환, GUID 보존 | 파일명 ↔ 직업 어긋남 | |
| **D3-B (★확정)** | **모든 식별자 직업명 정합** (`hero_warrior` / `hero_archer` / `hero_mage`) | **파일명 정합도 최고**, 시각·데이터·식별자 일관 | **SaveData 마이그레이션 필요** | ✅ |
| D3-C | 식별자·displayName 전부 옛 키즈 그대로 | 변경 0 | 직업 정체성 표현 못함 | |

**D3-B 채택의 영향**:

| 항목 | 변경 전 | 변경 후 |
|---|---|---|
| SO 파일명 | `Char_DefaultBoy.asset` | `Char_HeroWarrior.asset` |
| SO 파일명 | `Char_BraveGirl.asset` | `Char_HeroArcher.asset` |
| SO 파일명 | `Char_QuietFriend.asset` | `Char_HeroMage.asset` |
| `id` 필드 | `default_boy` / `brave_girl` / `quiet_friend` | `hero_warrior` / `hero_archer` / `hero_mage` |
| PNG 파일명 | `Char_DefaultBoy.png` 등 | `Char_HeroWarrior.png` 등 |
| `.meta` GUID | (보존) | (보존 — git mv 로 rename) |

**SaveData 마이그레이션 필요**:
- `MetaProgression` / `SaveSystem` 에 character ID lookup 시 `default_boy → hero_warrior` 매핑 헬퍼 추가 (1회성 migration on load)
- 또는 Phase 4 베타 사용자에게 "Reset SaveData" 안내 (베타 적은 가정 시)
- Sprint 3b (별도 commit) 에서 처리

### 3.4 D4 — Phase A 절차적 모션 코드 포함 여부

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| D4-A (★권장) | **5020 = 이미지 + wiring 만, Phase A 모션은 별도 5021** | CLAUDE.md §15.1 SRP 준수, 5020 작은 단위 | 모션 효과는 별도 PRD 후 |
| D4-B | 5020 에 Phase A 모션 5종 통합 | 한 PR 로 끝 | 책임 혼재, review 부담 ↑ |

### 3.5 D5 — Phase B (PSB rig) 사전 준비 여부 (★D5-B 확정)

| 옵션 | 설명 | 장점 | 단점 | 채택 |
|---|---|---|---|---|
| D5-A | PSB 호환 prompt 만 적용 — 추가 출력물 X | Phase B SAM segmentation 약간 쉬워짐 | PSB rig 시점에 별도 reference 또 필요 | |
| **D5-B (★확정)** | **정면 idle + 부위 분리용 reference 별도 출력** | **Phase B 출발 자산 풍부**, T-pose 또는 부위별 spread reference 보유 | **2× ComfyUI 출력 (3 → 6 장)**, References/Characters_PSB/ 폴더 신규 | ✅ |

**D5-B 채택의 영향**:
- ComfyUI 출력 6장: 3 hero × {idle, psb_ref}
- 시드 namespace: `50200~50202` (idle) + `50210~50212` (psb_ref) — 분리하여 retry 시 충돌 방지
- 출력 위치:
  - `Assets/_Project/Art/Generated_v2/Characters/Char_<Name>.png` (정면 idle, in-game 사용)
  - `Assets/_Project/Art/References/Characters_PSB/Char_<Name>_psb_ref.png` (PSB rig 출발 자료, in-game 미사용)
- PSB reference prompt 차이: T-pose 또는 부위 spread + 단색 배경 + 정면 + 무기 분리 표시
- Sprint 2 시간: 60~90분 → 90~150분 (6장 + retry)

---

## 4. 확정 설계 (옵션 D1 명명 / D2-A / D3-B / D4-A / D5-B)

### 4.1 직업별 정체성 prompt 명세

#### 4.1.1 전사 시우 (Warrior — `hero_warrior`, 구 Char_DefaultBoy)

| 항목 | 값 |
|---|---|
| **이름** | **시우** |
| **컨셉** | 꼬마 전사 시우, 호숫가 마을의 용감한 소년이 다락방 동화책에서 받은 검을 든 모습 |
| **무기** | 가벼운 한손검 (만화풍, 어린이 키 1/3 길이) |
| **방어구** | 가죽 흉갑 + 천 망토 (어깨 가벼운 보강), 풀 플레이트 ❌ |
| **색조** | 따뜻한 골드 + 갈색 (`tintColor` 1.0, 0.85, 0.55 정합) |
| **포즈** | 정면 three-quarter view, 검을 어깨 옆에 가볍게, 두 발 약간 벌림 |
| **표정** | 결의에 찬 미소, 어린이 친화 |
| **idle prompt** | "young boy hero named Siu, light leather chest plate, cartoon shortsword held at shoulder, brave smile, golden brown tones, three-quarter front view, both arms visible, no heavy cloak occluding body" |
| **psb_ref prompt** | "young boy hero T-pose, light leather chest plate, arms straight to sides, body parts clearly separated, no occlusion, plain transparent background, frontal view, sword and cape held visible separately" |

#### 4.1.2 궁수 가영 (Archer — `hero_archer`, 구 Char_BraveGirl)

| 항목 | 값 |
|---|---|
| **이름** | **가영** |
| **컨셉** | 꼬마 궁수 가영, 숲을 좋아하는 씩씩한 소녀가 활을 든 모습 |
| **무기** | 짧은 활 (어린이 키 절반), 화살통 등 뒤 |
| **방어구** | 가벼운 후드 망토 (얼굴 그림자 X), 무릎 보호대 |
| **색조** | 핑크 + 청록 보색 (`tintColor` 0.95, 0.65, 0.85 정합 + 부 청록 액센트) |
| **포즈** | 정면 three-quarter view, 활을 옆으로 들고 시위 X, 한쪽 발 앞 |
| **표정** | 자신감 있는 눈빛 |
| **idle prompt** | "young girl hero named Gayoung, hooded cape with face visible, short cartoon bow at side, quiver on back, pink and teal accents, three-quarter front view, no heavy cloak occluding torso" |
| **psb_ref prompt** | "young girl hero T-pose, hood pulled back, arms straight to sides, body parts clearly separated, bow and quiver held visible separately, plain transparent background, frontal view" |

#### 4.1.3 마법사 연우 (Mage — `hero_mage`, 구 Char_QuietFriend)

| 항목 | 값 |
|---|---|
| **이름** | **연우** |
| **컨셉** | 꼬마 마법사 연우, 동화책 속 별빛을 다루는 조용한 친구 |
| **무기** | 작은 지팡이 (별 모양 끝) 또는 별빛 구슬 |
| **방어구** | 별·달 무늬 로브 (어린이 사이즈, 무겁지 않게) + 끝이 살짝 굽은 마법사 모자 |
| **색조** | 청록 + 보라 (`tintColor` 0.55, 0.85, 0.95 정합 + 보라 별 액센트) |
| **포즈** | 정면 three-quarter view, 지팡이를 옆에 들고 한 손에 별빛 |
| **표정** | 차분한 미소, 큰 눈 |
| **idle prompt** | "young child mage named Yeonwoo, star-pattern robe, small cartoon staff with star tip held at side, slight wizard hat, teal and lavender tones, three-quarter front view, robe not heavy enough to occlude limbs" |
| **psb_ref prompt** | "young child mage T-pose, star-pattern robe shortened, arms straight to sides, body parts clearly separated, staff and hat shown separately, plain transparent background, frontal view" |

### 4.2 공통 prompt (BASE)

5008 baseline + 5019 답습 + character 특화:

```
v5 sticker tone, cute cartoon, chunky thick white outline,
soft cartoon shading, single character, transparent background,
three-quarter view, single character centered in frame,
fully transparent background, isolated subject,
solo, 1character, alone,
young child hero, kid proportions (3-head tall),
arms visible, body parts not occluded by cape
```

### 4.3 NEGATIVE prompt

5019 답습 + character 특화:

```
low quality, blurry, watermark, photorealistic, dark, scary, blood, gore,
text, logo, signature,
multiple subjects, multiple characters, twins, duplicate, copies,
two characters, three characters, four characters,
grid, frame, border, comic panel, sprite sheet, character sheet,
2x2 grid, 4 panels, multi-panel, collage, mosaic, isometric grid, tile,
adult, teen, mature, sexy,
heavy plate armor, full helmet covering face, oversized weapon,
profile view, back view, t-pose
```

### 4.4 신규 파일 명세

| 파일 | 종류 | 위치 |
|---|---|---|
| `scripts/5020_starter_hero_batch.py` | Python (5019 답습, 6 PNG 일괄) | repo root scripts |
| `docs/prd/5020/5020_prompt_table.md` | Prompt 표 (Sprint 1 산출, 6 prompt) | docs/prd/5020/ |
| `Assets/_Project/Art/Generated_v2/Characters/Char_HeroWarrior.png` | 신규 (구 DefaultBoy.png 대체) | git mv + 갱신 |
| `Assets/_Project/Art/Generated_v2/Characters/Char_HeroArcher.png` | 신규 (구 BraveGirl.png 대체) | git mv + 갱신 |
| `Assets/_Project/Art/Generated_v2/Characters/Char_HeroMage.png` | 신규 (구 QuietFriend.png 대체) | git mv + 갱신 |
| `Assets/_Project/Art/References/Characters_PSB/Char_HeroWarrior_psb_ref.png` | 신규 (D5-B PSB reference) | 신규 폴더 |
| `Assets/_Project/Art/References/Characters_PSB/Char_HeroArcher_psb_ref.png` | 신규 | 동 |
| `Assets/_Project/Art/References/Characters_PSB/Char_HeroMage_psb_ref.png` | 신규 | 동 |
| `Assets/_Project/Data/Characters/Char_HeroWarrior.asset` | 신규 (구 DefaultBoy.asset 대체) | git mv + 필드 갱신 |
| `Assets/_Project/Data/Characters/Char_HeroArcher.asset` | 신규 (구 BraveGirl.asset 대체) | git mv + 필드 갱신 |
| `Assets/_Project/Data/Characters/Char_HeroMage.asset` | 신규 (구 QuietFriend.asset 대체) | git mv + 필드 갱신 |
| `Assets/_Project/Scripts/Core/SaveDataMigration.cs` | 신규 (D3-B migration 헬퍼, ≤ 60L) | Core/ |
| `Assets/_Project/Scripts/Editor/StarterHeroAuditor.cs` | Editor 도구 | 5008 EnemySpriteAuditor 답습 |
| `Assets/_Project/Tests/Editor/CharacterDataIntegrityTests.cs` | NUnit | className ↔ multiplier 정합 + id 정합 |
| `Assets/_Project/Tests/Editor/SaveDataMigrationTests.cs` | NUnit | 구 id → 신 id 매핑 검증 |

### 4.5 SO 갱신 (D3-B — id + 파일명 + displayName)

| 구 SO 파일 | 신 SO 파일 | 구 id | 신 id | 신 displayName |
|---|---|---|---|---|
| `Char_DefaultBoy.asset` | `Char_HeroWarrior.asset` | `default_boy` | `hero_warrior` | **전사 시우** (변형 B) |
| `Char_BraveGirl.asset` | `Char_HeroArcher.asset` | `brave_girl` | `hero_archer` | **궁수 가영** |
| `Char_QuietFriend.asset` | `Char_HeroMage.asset` | `quiet_friend` | `hero_mage` | **마법사 연우** |

> Sprint 3a 에서 `git mv` 로 파일 rename — `.meta` 동행 → GUID 보존 → CharacterData 참조 자동 유지
> Sprint 3b 에서 SaveDataMigration 헬퍼 + SaveSystem 진입부 1회성 매핑 적용

### 4.6 sprite import 설정 (보고서 §3-A 권장)

| 항목 | 변경 전 | 변경 후 (5020) |
|---|---|---|
| Pivot | (0.5, 0.5) Center | **(0.5, 0.1) Bottom Center** (in-game idle 만, psb_ref 는 미적용) |
| Filter Mode | Bilinear | Bilinear (유지) |
| iOS Override | (없음) | **ASTC 6×6 + Crunched** (5018 자동, idle 만) |
| Read/Write | true | false (Player 모드 GC 최소화) |

→ Pivot 변경은 **landing/jump squash, anticipation** 같은 Phase A 모션이 발 기준으로 자연스러워지기 위함.
→ `References/Characters_PSB/` 의 psb_ref PNG 는 **in-game 미사용** (Sprite import 설정 변경 X, ASTC override X — 출시 빌드 제외 가능)

---

## 5. Sprint 분해 (간략, D3-B + D5-B 반영)

> 자세한 Sprint 명세는 `docs/prd/5020/5020_starter_hero_visual_prd.md` 참조.

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Prompt 표 설계 — 6 prompt (idle 3 + psb_ref 3) | 1 | 45분 |
| 2 | ComfyUI 배치 생성 — 6 PNG (idle 3 in Generated_v2 + psb_ref 3 in References) | 1 | 90~150분 (서버 + retry) |
| **3a** | **SO + PNG 파일 rename (`git mv`) — D3-B 식별자 정합** | 1 | 20분 |
| **3b** | **SaveDataMigration 헬퍼 + SaveSystem hook (D3-B 마이그레이션)** | 1 | 30분 |
| 3c | Sprite import (pivot 발 밑) + displayName 갱신 + 직업명 정합 검증 | 1 | 20분 |
| 4 | StarterHeroAuditor + CharacterDataIntegrityTests + SaveDataMigrationTests | 1 | 40분 |
| 5 | Play 검증 (Attic CharacterSelector + SaveData 마이그레이션) + changelog 추가 | 1 | 20분 |
| **합계** | | **7** | **4~5시간** (서버 시간 제외) |

> D3-B / D5-B 반영으로 5 commit → 7 commit, 2.5~3시간 → 4~5시간 (Sprint 3 분할 + Sprint 2 시간 ≈ 2배)

---

## 6. 알려진 위험 + 완화 (D3-B + D5-B 반영)

| 위험 | 영향 | 완화 |
|---|---|---|
| ComfyUI consistency — 어린이 비율 (3-head tall) 흔들림 | 직업 차별화 약화 | seed namespace 50200~50202(idle) + 50210~50212(psb_ref) 고정 + IPAdapter 옵션 |
| V5 sticker LoRA(5014) 적용 시 prompt 단순화로 직업 표현 약화 | 정체성 부족 | 5020 은 5008 baseline production 우선 (5014 메모 답습) |
| ASTC 6×6 alpha edge 손실 | sticker 외곽 흰 outline 흐림 | 5018 검증 완료 (chapter sprite 동일 문제 없음) |
| character_progression_v1.md (옛 4 코어 + 옛 키즈명) 미갱신 | 디자인 문서 ↔ 자산 추가 미스매치 | 5020 범위 외 — 별도 문서 갱신 PR (제안 only) |
| **D3-B `git mv` 후 `.meta` 동행 실패** | **GUID 변경 → CharacterData portrait wiring 끊김 → Player Spawn NRE** | Sprint 3a 에 `git mv Char_DefaultBoy.png Char_HeroWarrior.png && git mv Char_DefaultBoy.png.meta Char_HeroWarrior.png.meta` 명시. SO 도 동일. Sprint 3a commit 후 Unity 콘솔 에러 0 검증 |
| **D3-B id 변경으로 기존 베타 SaveData 캐릭터 선택 정보 끊김** | 사용자 캐릭터 reset 됨 | Sprint 3b SaveDataMigration 헬퍼 — 로드 시 1회성 매핑 (`default_boy → hero_warrior` 등). SaveData version bump |
| **D5-B PSB reference PNG 가 빌드 포함되어 용량 증가** | iOS 빌드 +1.5~2 MB | `References/Characters_PSB/` 폴더에 Editor-only `.gitignore` 또는 빌드 제외 설정. 또는 `Editor` 폴더로 이동 (Unity 자동 제외) |
| **Sprint 3a~3c 분할로 commit 트리 복잡화** | review 부담 ↑ | 3a (rename) / 3b (migration) / 3c (config) 단일 책임 commit 메시지로 명확화 |

---

## 7. CLAUDE.md §15 정합 검증

| 정책 | 5020 적용 |
|---|---|
| §15.1 SOLID-S | StarterHeroAuditor 단일 책임 (audit 만). SaveDataMigration 단일 책임 (id 매핑 만) |
| §15.4 SO 우선 | CharacterData SO 만 수정 (코드 변경: SaveDataMigration ≤ 60L + SaveSystem hook ≤ 5L) |
| §15.6 100줄 룰 | StarterHeroAuditor ≤ 80L, SaveDataMigration ≤ 60L (5008 EnemySpriteAuditor 답습) |
| §15.7 DI | SaveDataMigration 정적 헬퍼 (CurrencyService 패턴 답습) |
| §15.9 Test | CharacterDataIntegrityTests 4 + SaveDataMigrationTests 3 = NUnit 108 → 115 |
| §15.11 안티 패턴 | public 필드 추가 X, Update 추가 X, string concat in Update X |
| §15.12 Validator | StarterHero_PortraitWired 룰 추가 (3 SO portrait null 검사) |

---

## 8. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-generator) | 초안 — 옵션 D1~D5 비교, ★ 권장 D1-B/D2-A/D3-A/D4-A/D5-A |
| 2026-05-06 | Claude (prd-generator, 사용자 결정 반영) | **확정**: D1=시우/가영/연우 / D2-A / **D3-B** / D4-A / **D5-B**. Sprint 3 → 3a/3b/3c 분할, 6 PNG 출력, SaveDataMigration 헬퍼 신규. 5 commit → 7 commit |

---

## 9. Sources

- 보고서: `docs/little_hero/character/character_animation_research_v1.md` (2026-05-06)
- 5008 baseline: `~/.claude/projects/.../memory/sprite_generation_v1.md`
- 5009 batch 패턴: `scripts/5009_sprint_batch.py`
- 5019 narrative-specific 답습: `scripts/5019_chapter_narrative_batch.py` + `docs/prd/5019/5019_prompt_table.md`
- 5018 texture compression: `docs/prd/5018_texture_compression_prd.md`
- stats_v2: `docs/little_hero/stats_v2_infinite.md`
