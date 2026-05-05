# T03: Sprint 3a — SO/PNG `git mv` rename + Selected_Characters_v1_legacy

> **레이어**: Data
> **의존성**: T02 (6 PNG 생성 + 사용자 OK)
> **예상 시간**: 25분 / 1 commit
> **PRD 출처**: v2 §차분 3 + §차분 9 (D-D=B Mediator)

---

## 1. 작업 항목

### 1.1 SO 파일 rename (3 쌍 = 6 git mv)

```bash
git mv LittleHero/Assets/_Project/Data/Characters/Char_DefaultBoy.asset       Char_HeroWarrior.asset
git mv LittleHero/Assets/_Project/Data/Characters/Char_DefaultBoy.asset.meta  Char_HeroWarrior.asset.meta
git mv LittleHero/Assets/_Project/Data/Characters/Char_BraveGirl.asset        Char_HeroArcher.asset
git mv LittleHero/Assets/_Project/Data/Characters/Char_BraveGirl.asset.meta   Char_HeroArcher.asset.meta
git mv LittleHero/Assets/_Project/Data/Characters/Char_QuietFriend.asset      Char_HeroMage.asset
git mv LittleHero/Assets/_Project/Data/Characters/Char_QuietFriend.asset.meta Char_HeroMage.asset.meta
```

### 1.2 PNG 파일 rename (T01 매핑에 따라 wired PNG 만 git mv, 비-wired 는 git rm)

**wired PNG (T01 GUID 매핑 결과 기준)**:
```bash
git mv Generated_v2/Characters/Char_DefaultBoy.png      Char_HeroWarrior.png
git mv Generated_v2/Characters/Char_DefaultBoy.png.meta Char_HeroWarrior.png.meta
git mv Generated_v2/Characters/Char_BraveGirl.png       Char_HeroArcher.png
git mv Generated_v2/Characters/Char_BraveGirl.png.meta  Char_HeroArcher.png.meta
git mv Generated_v2/Characters/Char_QuietFriend.png     Char_HeroMage.png
git mv Generated_v2/Characters/Char_QuietFriend.png.meta Char_HeroMage.png.meta
```

**비-wired PNG (T01 매핑에서 별도 식별)** — 일반적으로 `_idle` suffix:
```bash
git rm Generated_v2/Characters/Char_DefaultBoy_idle.png
git rm Generated_v2/Characters/Char_DefaultBoy_idle.png.meta
git rm Generated_v2/Characters/Char_BraveGirl_idle.png
git rm Generated_v2/Characters/Char_BraveGirl_idle.png.meta
git rm Generated_v2/Characters/Char_QuietFriend_idle.png
git rm Generated_v2/Characters/Char_QuietFriend_idle.png.meta
```

> 주의: T01 GUID 매핑에서 `_idle` 가 실제로 wired 였으면 위 git mv/rm 대상이 바뀜. 매핑 표 기준으로 결정.

### 1.3 D-D=B Mediator — Selected_Characters rename

```bash
git mv LittleHero/Assets/_Project/Art/References/Selected_Characters \
       LittleHero/Assets/_Project/Art/References/Selected_Characters_v1_legacy
```

→ 옛 키즈 reference 자료 명확 분리 (history 보존).

### 1.4 SO 필드 갱신 (rename 후 YAML 직접 또는 Inspector 편집)

3 SO 의 `id` 필드 변경:

| SO | id (변경 전) | id (변경 후) |
|---|---|---|
| `Char_HeroWarrior.asset` | `default_boy` | `hero_warrior` |
| `Char_HeroArcher.asset` | `brave_girl` | `hero_archer` |
| `Char_HeroMage.asset` | `quiet_friend` | `hero_mage` |

> portrait GUID 는 PNG 만 덮어쓰기 (T02) + git mv (PNG + .meta 동행) → 자동 보존, 변경 불필요.

---

## 2. 검증 기준

### 2.1 git status 확인
- [ ] 3 SO `R` (rename) + 3 SO.meta `R`
- [ ] wired PNG 3 `R` + 3 PNG.meta `R`
- [ ] 비-wired PNG 6 `D` (delete)
- [ ] Selected_Characters 폴더 `R` (rename to _v1_legacy)
- [ ] 합산: 6 R (SO) + 6 R (PNG) + 6 D (비-wired PNG) + 1 R (폴더)

### 2.2 Unity MCP 검증 (refresh_unity + 5~10s + read_console)
- [ ] 콘솔 errors=0 (도메인 리로드 후)
- [ ] CharacterData Inspector 에서 portrait 미리보기 정상 (신 PNG)
- [ ] AssetDatabase.LoadAssetAtPath("Assets/_Project/Data/Characters/Char_HeroWarrior.asset") 로드 OK

### 2.3 회귀
- [ ] 기존 Player Spawn / AtticCharacterSelector 정상 (id 변경 → SaveData 마이그레이션은 T04 에서)

---

## 3. commit 메시지

```
5020 Sprint 3a / T03 — D3-B SO/PNG rename + Selected_Characters_v1_legacy (3/7)

[변경]
  R Char_DefaultBoy.asset   → Char_HeroWarrior.asset (id: hero_warrior)
  R Char_BraveGirl.asset    → Char_HeroArcher.asset  (id: hero_archer)
  R Char_QuietFriend.asset  → Char_HeroMage.asset    (id: hero_mage)
  R Char_DefaultBoy.png     → Char_HeroWarrior.png   (wired, GUID 보존)
  R Char_BraveGirl.png      → Char_HeroArcher.png    (wired, GUID 보존)
  R Char_QuietFriend.png    → Char_HeroMage.png      (wired, GUID 보존)
  D Char_*_idle.png 3쌍 (비-wired, T01 매핑 결과)
  R References/Selected_Characters → Selected_Characters_v1_legacy (D-D=B 옛 자료 명확 분리)

[검증]
  - Unity 콘솔 errors=0 (도메인 리로드 후)
  - CharacterData portrait 미리보기 정상
  - GUID 보존 — wiring 자동 유지

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- T01 `5020_png_guid_map.md` (wired vs 비-wired 매핑 결정)
- v2 §차분 3 (Sprint 3a `git rm` → `git mv`)
- v2 §차분 9 (D-D=B mediator)
- 5009/5010/5019 GUID 보존 정책 답습

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| `git mv` 시 .meta 누락 → GUID 변경 | PNG + .meta 동행 명시 (위 명령 답습) |
| T01 매핑 오류 → wired vs 비-wired 잘못 분류 | T01 prelude 표 사전 검증 + Unity Inspector 미리보기 확인 |
| Unity 가 rename 인식 못함 (캐시 stale) | refresh_unity + Reimport 필요 시 수동 |
| Selected_Characters_v1_legacy 폴더 .meta 처리 | Unity 자동 .meta 생성 (folder rename) |
