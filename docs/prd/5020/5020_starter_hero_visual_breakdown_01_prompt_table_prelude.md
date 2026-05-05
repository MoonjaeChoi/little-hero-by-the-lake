# T01: Sprint 1 — Prelude + Prompt 표 작성

> **레이어**: Docs
> **의존성**: 없음 (시작 태스크)
> **예상 시간**: 60분 / 1 commit
> **PRD 출처**: v2 §차분 1 (Sprint 1 prelude 추가) + Sprint PRD §2

---

## 1. 작업 항목

### 1.1 Prelude Step 1 — wired PNG GUID 매핑 표 (R1 대응)

**산출**: `docs/prd/5020/5020_png_guid_map.md` (신규)

**작업**:
```bash
# SO portrait GUID 추출
grep "portrait:" LittleHero/Assets/_Project/Data/Characters/Char_*.asset

# PNG .meta GUID 추출
grep "guid:" LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_*.png.meta | head
```

**5020_png_guid_map.md 표 형식**:
| 캐릭터 | SO 파일 | SO portrait GUID | wired PNG 파일 | PNG GUID 매칭 |
|---|---|---|---|---|
| 전사 시우 | Char_DefaultBoy.asset | f6bedfb7… | Char_DefaultBoy.png 또는 Char_DefaultBoy_idle.png | (확인 후 기입) |
| 궁수 가영 | Char_BraveGirl.asset | f0823e95… | Char_BraveGirl.png 또는 Char_BraveGirl_idle.png | (확인 후 기입) |
| 마법사 연우 | Char_QuietFriend.asset | 2c53e767… | Char_QuietFriend.png 또는 Char_QuietFriend_idle.png | (확인 후 기입) |

→ T03 (git mv) 단계에서 wired PNG 만 rename, 비-wired 는 git rm.

### 1.2 Prelude Step 2 — D-A 사실 결정 명시

**작업**: `5020_png_guid_map.md` 에 한 섹션 추가:

```markdown
## D-A 사실 결정 (rebuttal 검증 완료)

- AtticCharacterSelector.cs (line 22~82) 가 직렬화된 `_buttons[]` 배열로 wiring
- `ResolveCharacterById(id)` 에서 `entry.character.id == id` 만 사용 — GameObject 이름 사용 X
- → **`CharBtn_default_boy` 등 GameObject 이름 변경 불필요** (Attic.unity 변경 0)
```

### 1.3 Prompt 표 작성 — 6 prompt

**산출**: `docs/prd/5020/5020_prompt_table.md` (신규)

**6 row 표** (디자인 §4.1.1~4.1.3 답습):

| hero | 종류 | seed | 출력 경로 | 정체성 prompt 요약 |
|---|---|---|---|---|
| hero_warrior (시우) | idle | 50200 | Generated_v2/Characters/Char_DefaultBoy.png (wired 베이스명) | "young boy hero named Siu, light leather chest plate, cartoon shortsword..." |
| hero_warrior (시우) | psb_ref | 50210 | References/Characters_PSB/Char_HeroWarrior_psb_ref.png | "young boy hero T-pose, body parts clearly separated..." |
| hero_archer (가영) | idle | 50201 | Generated_v2/Characters/Char_BraveGirl.png (wired 베이스명) | "young girl hero named Gayoung, hooded cape with face visible, short cartoon bow..." |
| hero_archer (가영) | psb_ref | 50211 | References/Characters_PSB/Char_HeroArcher_psb_ref.png | "young girl hero T-pose, hood pulled back, body parts clearly separated..." |
| hero_mage (연우) | idle | 50202 | Generated_v2/Characters/Char_QuietFriend.png (wired 베이스명) | "young child mage named Yeonwoo, star-pattern robe, small staff..." |
| hero_mage (연우) | psb_ref | 50212 | References/Characters_PSB/Char_HeroMage_psb_ref.png | "young child mage T-pose, body parts clearly separated..." |

> **wired 베이스명 규칙 (R1)**: idle PNG 출력 파일명을 T01 prelude 의 GUID 매핑에서 확인된 wired PNG 와 동일하게 설정 → PNG 만 덮어쓰기 → GUID 보존 → SO portrait wiring 자동 유지. T03 에서 신 파일명(`Char_HeroWarrior.png` 등) 으로 git mv.

**BASE / NEGATIVE 공통 블록** (디자인 §4.2 / §4.3 답습):
- BASE: V5 sticker tone + cute cartoon + transparent + three-quarter view + arms visible
- NEGATIVE: low quality + adult/teen/mature + heavy plate armor + multiple subjects + grid/sprite sheet

---

## 2. 검증 기준

### 2.1 산출 파일
- [ ] `docs/prd/5020/5020_png_guid_map.md` 작성 (3 SO ↔ wired PNG 매핑 명확)
- [ ] `docs/prd/5020/5020_prompt_table.md` 작성 (6 prompt + BASE/NEGATIVE)

### 2.2 정합성
- [ ] D-A 사실 결정 메모 명시 (CharBtn 변경 X)
- [ ] 시드 namespace 분리 (idle 50200~50202, psb_ref 50210~50212)
- [ ] 출력 경로 idle = wired 베이스명, psb_ref = References/Characters_PSB/

### 2.3 사용자 review (선택, 5분)
- [ ] prompt 표 quick review — 직업 정체성 + 어린이 이름 톤 정합

---

## 3. commit 메시지

```
5020 Sprint 1 / T01 — Starter Hero prelude + prompt table 6장 (1/7)

[변경]
  + docs/prd/5020/5020_png_guid_map.md (R1 — SO portrait ↔ wired PNG 매핑 표)
  + docs/prd/5020/5020_prompt_table.md (6 prompt: idle 3 + psb_ref 3)

[결정]
  - D1 (사용자): 전사 시우 / 궁수 가영 / 마법사 연우
  - D-A (rebuttal): CharBtn_<id> GameObject 이름 변경 불필요 (사실 검증)
  - D5-B (사용자): idle 3 + psb_ref 3 (Phase B PSB 출발 자료)

[검증]
  - 시드 namespace: idle 50200~50202, psb_ref 50210~50212
  - wired 베이스명 통일 (R1) → T03 git mv 시 GUID 보존

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- PRD v2 §차분 1 (Sprint 1 prelude)
- Decisions D-D (참고만 — T03 영향)
- Rebuttal R1 (wired PNG 매핑) + D-A (CharBtn 검증)
- 디자인 §4.1.1~4.1.3 (직업 prompt)

---

## 5. 알려진 위험

- 위험: `Char_*.png` 와 `Char_*_idle.png` 둘 다 wired 가능성 (이중 wiring) — 매핑 표에서 확인 후 결정
  - 완화: SO portrait GUID 1개만 wiring (다른 PNG 는 비-wired) — 정합성 확인
