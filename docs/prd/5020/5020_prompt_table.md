# 5020 Prompt Table — Starter Hero V5 sticker (T01)

> **목적**: ComfyUI 6 PNG 생성 prompt 사전 정의.
> **출처**: 디자인 §4.1.1~4.1.3 + §4.2 (BASE) + §4.3 (NEGATIVE).
> **사용**: `scripts/5020_starter_hero_batch.py` (T02) 에서 본 표 기반 dispatch.

---

## 1. 6 Prompt 명세

| hero | 종류 | seed | 출력 경로 | 정체성 prompt |
|---|---|---|---|---|
| hero_warrior (시우) | idle | 50200 | `Art/Generated_v2/Characters/Char_DefaultBoy.png` (wired 베이스명, 덮어쓰기) | "young boy hero named Siu, light leather chest plate, cartoon shortsword in hand, brave determined smile, light brown hair" |
| hero_warrior (시우) | psb_ref | 50210 | `Art/References/Characters_PSB/Char_HeroWarrior_psb_ref.png` | "young boy hero T-pose, body parts clearly separated, head/torso/arms/legs visible, light leather plate, neutral expression" |
| hero_archer (가영) | idle | 50201 | `Art/Generated_v2/Characters/Char_BraveGirl.png` (wired 베이스명, 덮어쓰기) | "young girl hero named Gayoung, hooded cape with face visible, short cartoon bow drawn, focused expression, dark brown hair" |
| hero_archer (가영) | psb_ref | 50211 | `Art/References/Characters_PSB/Char_HeroArcher_psb_ref.png` | "young girl hero T-pose, hood pulled back showing face, body parts clearly separated, neutral expression" |
| hero_mage (연우) | idle | 50202 | `Art/Generated_v2/Characters/Char_QuietFriend.png` (wired 베이스명, 덮어쓰기) | "young child mage named Yeonwoo, star-pattern robe, small wooden staff with star tip, gentle smile, light hair" |
| hero_mage (연우) | psb_ref | 50212 | `Art/References/Characters_PSB/Char_HeroMage_psb_ref.png` | "young child mage T-pose, body parts clearly separated, star-pattern robe, neutral expression" |

> **wired 베이스명 규칙 (R1)**: idle PNG 출력 파일명을 wired PNG 와 동일하게 → PNG 만 덮어쓰기 → GUID 보존 → SO portrait wiring 자동 유지. T03 에서 신 파일명(`Char_HeroWarrior.png` 등) 으로 git mv.

---

## 2. BASE 공통 블록 (디자인 §4.2)

```
masterpiece, best quality, V5 sticker style, cute cartoon, transparent background,
three-quarter view, full body, arms visible, child proportions, 3-head tall,
soft pastel palette, clean line, vibrant colors, warm lighting, PG-12 friendly
```

---

## 3. NEGATIVE 공통 블록 (디자인 §4.3)

```
low quality, blurry, distorted, deformed, ugly, scary, dark, gore, blood,
adult, teen, mature, muscular, sexy, heavy plate armor, heavy weapon,
multiple subjects, twin, grid, sprite sheet, multiple poses,
realistic photo, 3d render, doll, plush figure, no weapon hidden behind body
```

---

## 4. 시드 namespace

| 종류 | seed 범위 |
|---|---|
| idle | 50200 ~ 50202 |
| psb_ref | 50210 ~ 50212 |

→ 5019 (90000~90209) / 5011 (12001~) / 5010 (11001~11011) 등 기존 시드 namespace 와 겹침 X.

---

## 5. ComfyUI 워크플로 (5019 답습)

- 모델: SDXL base 1.0 (5008 검증 baseline)
- 해상도: 768 × 1024 (RGBA, transparent)
- cfg: 6
- sampler: dpmpp_2m karras
- steps: 30~40
- LayerDiffuse: enabled (transparent background)

> 5014 V5 LoRA 는 prompt 단순화 가속 도구로 보조 가능 (선택).

---

## 6. 시각 review 체크리스트 (T02 게이트)

- [ ] 직업별 무기 식별 가능 (검·활·지팡이 명확)
- [ ] 어린이 비율 (3-head tall) 일관
- [ ] 단일 캐릭터 (multiple/twin 없음)
- [ ] PG-12 톤 (피·잔인 X, 따뜻한 어린이 친화)
- [ ] V5 sticker 톤 정합 (5008 baseline + sticker dynamic)
- [ ] psb_ref 3장: T-pose 또는 명확한 부위 분리
- [ ] **사용자 OK** ✅

---

## 7. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (T01 Prelude) | 초기 — 6 prompt + BASE/NEGATIVE + 시드 namespace |
