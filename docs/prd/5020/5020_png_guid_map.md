# 5020 PNG GUID Mapping (T01 Prelude — R1)

> **목적**: SO portrait GUID ↔ wired PNG ↔ 비-wired PNG 매핑 사전 검증.
> **출처**: `grep "portrait:" Char_*.asset` + `grep "guid:" Char_*.png.meta` (2026-05-06).
> **R1 답습**: PNG 파일 덮어쓰기 + git mv + .meta 동행 → GUID 보존 → SO portrait wiring 자동 유지.

---

## 1. SO portrait ↔ wired PNG 매칭

| 캐릭터 | SO 파일 | SO portrait GUID | wired PNG 파일 | PNG GUID | 매칭 |
|---|---|---|---|---|---|
| 전사 시우 (구 default_boy) | `Char_DefaultBoy.asset` | `f6bedfb7ffbaf4ecdacb710ea3562ef3` | `Generated_v2/Characters/Char_DefaultBoy.png` | `f6bedfb7ffbaf4ecdacb710ea3562ef3` | ✅ |
| 궁수 가영 (구 brave_girl) | `Char_BraveGirl.asset` | `f0823e952db484eca9b38e86a15850ea` | `Generated_v2/Characters/Char_BraveGirl.png` | `f0823e952db484eca9b38e86a15850ea` | ✅ |
| 마법사 연우 (구 quiet_friend) | `Char_QuietFriend.asset` | `2c53e76751ee447c192630f0c9028063` | `Generated_v2/Characters/Char_QuietFriend.png` | `2c53e76751ee447c192630f0c9028063` | ✅ |

→ **wired = `Char_*.png`** (no `_idle` suffix). T02 ComfyUI 출력 베이스명 = wired PNG 명.

---

## 2. 비-wired PNG (T03 git rm 대상)

| 비-wired 파일 | GUID | 처리 |
|---|---|---|
| `Generated_v2/Characters/Char_DefaultBoy_idle.png` | `fe52f52c9e5934c1a9247d7ea1f26de1` | git rm (T03) |
| `Generated_v2/Characters/Char_BraveGirl_idle.png` | `4f260c916db4d46f293af8a4c1f6830d` | git rm (T03) |
| `Generated_v2/Characters/Char_QuietFriend_idle.png` | `edd537bdaffa14610bd7596c65d2ef93` | git rm (T03) |

→ 어떤 SO portrait 도 위 GUID 참조 X (사실 검증 완료, 2026-05-06).

---

## 3. T03 git mv / git rm 명세

```bash
# wired PNG rename (GUID 보존)
git mv Generated_v2/Characters/Char_DefaultBoy.png      Char_HeroWarrior.png
git mv Generated_v2/Characters/Char_DefaultBoy.png.meta Char_HeroWarrior.png.meta
git mv Generated_v2/Characters/Char_BraveGirl.png       Char_HeroArcher.png
git mv Generated_v2/Characters/Char_BraveGirl.png.meta  Char_HeroArcher.png.meta
git mv Generated_v2/Characters/Char_QuietFriend.png     Char_HeroMage.png
git mv Generated_v2/Characters/Char_QuietFriend.png.meta Char_HeroMage.png.meta

# 비-wired PNG 삭제
git rm Generated_v2/Characters/Char_DefaultBoy_idle.png
git rm Generated_v2/Characters/Char_DefaultBoy_idle.png.meta
git rm Generated_v2/Characters/Char_BraveGirl_idle.png
git rm Generated_v2/Characters/Char_BraveGirl_idle.png.meta
git rm Generated_v2/Characters/Char_QuietFriend_idle.png
git rm Generated_v2/Characters/Char_QuietFriend_idle.png.meta

# SO 파일 rename
git mv Data/Characters/Char_DefaultBoy.asset   Char_HeroWarrior.asset
git mv Data/Characters/Char_DefaultBoy.asset.meta   Char_HeroWarrior.asset.meta
git mv Data/Characters/Char_BraveGirl.asset    Char_HeroArcher.asset
git mv Data/Characters/Char_BraveGirl.asset.meta    Char_HeroArcher.asset.meta
git mv Data/Characters/Char_QuietFriend.asset  Char_HeroMage.asset
git mv Data/Characters/Char_QuietFriend.asset.meta  Char_HeroMage.asset.meta

# Selected_Characters → _v1_legacy (D-D=B mediator)
git mv References/Selected_Characters References/Selected_Characters_v1_legacy
```

---

## 4. D-A 사실 결정 (rebuttal 검증 완료)

- `Hub/AtticCharacterSelector.cs:22~82` 가 직렬화된 `_buttons[]` 배열로 wiring
- `ResolveCharacterById(id)` 에서 `entry.character.id == id` 만 사용 — GameObject 이름 사용 X
- → **`CharBtn_default_boy` 등 GameObject 이름 변경 불필요** (Attic.unity 변경 0)

**검증** (2026-05-06):
```
$ grep -n "ResolveCharacterById\|_buttons\|character.id" Hub/AtticCharacterSelector.cs
22:        [SerializeField] private CharacterButton[] _buttons;
30:            if (_buttons != null)
32:                for (int i = 0; i < _buttons.Length; i++)
34:                    var entry = _buttons[i];
46:            if (_buttons != null)
47:                foreach (var entry in _buttons)
58:                saved = ResolveCharacterById(id);
76:        private CharacterData ResolveCharacterById(string id)
78:            if (string.IsNullOrEmpty(id) || _buttons == null) return null;
79:            foreach (var entry in _buttons)
```

→ Attic.unity wiring 무영향. T03 SO id 변경만으로 자동 정합.

---

## 5. C# 코드 직접 참조 검색 (사실 검증 — D-A 보강)

```
$ grep -rn "default_boy\|brave_girl\|quiet_friend" LittleHero/Assets/_Project/Scripts/
(결과: 없음)
```

→ 코드 hardcode 0건. SaveData `selectedCharacterId` 만 string id 보존 → D3-B SaveDataMigration 으로 처리 (T04).

---

## 6. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (T01 Prelude) | 초기 — 3 SO ↔ wired PNG GUID 매핑 명확, 비-wired 6 PNG 식별, D-A 사실 검증 |
