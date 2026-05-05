# 5020 Starter Hero Visual — PRD Critique

> **검토 대상**:
> - `docs/prd/5020_starter_hero_visual_prd.md` (디자인)
> - `docs/prd/5020/5020_starter_hero_visual_prd.md` (Sprint PRD)
>
> **검토자**: Claude (prd-critiquer)
> **검토일**: 2026-05-06
> **검토 정책**: CLAUDE.md §15 OOP / §2 PG-12 / §7 성능 / §11~14 통합 + 정적 grep 검증

---

## 0. 결정 이력

| 일자 | 검토자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-critiquer) | 초기 — Reject 1 / Discuss 4 / Accept 8 / Doc 1 |

---

## 1. 종합 점수: 8.9/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9.5/10 | SaveDataMigration 정적 헬퍼 (§15.7 DI), SRP 분할 (3a/3b/3c), Test 8 신규 |
| PG-12 톤 | 10/10 | "young hero kid" + 따뜻한 색조, V5 sticker dynamic 정합 |
| 성능 | 9/10 | 6 PNG 추가지만 psb_ref Editor 자동 제외 — in-game 영향 0 |
| 시스템 통합 | 7.5/10 | ⚠️ **Attic.unity 하드코드 GameObject 이름, LimitedShop 첫 선택 플래그 누락 가능** |
| Sprint 분해 | 8.5/10 | 7 commit 적절. **Sprint 2 wired PNG 매핑 모호 (R1)** |

---

## 2. Accept 항목 (8)

- **A1** ✅ **SaveDataMigration 정적 헬퍼** — `Dictionary<string, string>` 매핑 + `Apply(SaveData)` 정적 메서드. CLAUDE.md §15.7 DI (Singleton 회피, CurrencyService 패턴 답습) 정합. ≤60L 명시.

- **A2** ✅ **D5-B PSB reference Editor/ 폴더 자동 빌드 제외** — Unity 의 `Editor/` 폴더 자동 룰 활용. 빌드 용량 영향 0. iOS 150MB 임계 안전.

- **A3** ✅ **Sprint 3 → 3a/3b/3c 단일 책임 분할** — rename / migration / config 로 분리. CLAUDE.md §15.1 SRP + §15 워크플로 "단일 책임 commit" 정합.

- **A4** ✅ **PG-12 톤** — "young hero kid" + 따뜻한 골드/핑크/보라 색조 + 결의에 찬 미소. CLAUDE.md §2 절대 원칙 준수. NEGATIVE prompt 에 "adult, teen, mature, sexy" 명시.

- **A5** ✅ **ASTC 6×6 + Crunched 자동 적용** — 5018 검증 답습. idle 3 PNG 만 in-game 모드. iOS 빌드 자동 최적화.

- **A6** ✅ **NUnit 8 신규** (CharacterDataIntegrityTests 4 + SaveDataMigrationTests 4) — CLAUDE.md §15.9 Test 정합. 정적 헬퍼 단위 테스트 우선.

- **A7** ✅ **changelog 형식** (5019 답습) — 사전조사 / 신규 산출 / 결과 / NUnit / Phase 4 영향 표기. 전 시스템 일관.

- **A8** ✅ **prompt 표 분리** → `5020_prompt_table.md` — 5019 답습. 6 prompt 가독성 ↑, retry 시 단일 정정 용이.

---

## 3. Reject 항목 (1)

### ❌ R1 — Sprint 2 wired PNG 매핑 모호 (필수 수정)

**위치**: Sprint PRD §3 (Sprint 2 출력 명세)

**문제**:
- 현재 `Generated_v2/Characters/` 에 6 PNG 가 공존:
  - `Char_DefaultBoy.png` (164KB, May 4)
  - `Char_DefaultBoy_idle.png` (624KB, May 1)
  - (BraveGirl/QuietFriend 동일 패턴)
- **어느 PNG 가 SO `portrait` 필드에 wired 인가?** — PRD 미명시
- Sprint 3a 의 `git rm` 6 파일이 wired PNG 까지 삭제할 위험

**근거 (정적 grep)**:
- SO portrait GUID: Char_DefaultBoy=`f6bedfb7…`, Char_BraveGirl=`f0823e95…`, Char_QuietFriend=`2c53e767…`
- 6 PNG `.meta` 의 GUID 와 매핑 확인 사전 필요

**필수 수정 (Sprint 2)**:
1. Sprint 2 시작 전: 3 SO 의 portrait GUID 와 6 PNG `.meta` GUID 매핑 표 생성 (Sprint 1 또는 Sprint 2 prelude)
2. Sprint 2 출력 파일명을 wired PNG 와 동일 베이스명으로 통일 (예 wired 가 `Char_DefaultBoy.png` 면 신 PNG 도 `Char_HeroWarrior.png` 로 git mv 후 덮어쓰기, `_idle` 는 별도 처리)
3. Sprint 3a `git rm` 명령에서 wired PNG 는 `git mv` (rename) 으로 처리, 비-wired 만 `git rm`

**대안 명세 추가**:
```
Sprint 1 prelude (또는 Sprint 2 step 0):
  python3 -c "import yaml ... " 로 SO portrait GUID 추출 + PNG GUID 매핑 표 생성
  → docs/prd/5020/5020_png_guid_map.md (sprite ↔ wired PNG 매핑)
```

---

## 4. Discuss 항목 (4 — 사용자 결정 필요)

### 🟡 D-A — Attic.unity 의 `CharBtn_<id>` GameObject 이름 변경

**발견 (정적 grep)**:
```
Attic.unity:910   m_Name: CharBtn_brave_girl
Attic.unity:11019 m_Name: CharBtn_default_boy
Attic.unity:58566 m_Name: CharBtn_quiet_friend
```

**의문**: GameObject 이름이 단순 라벨인가, 또는 AtticCharacterSelector / 다른 코드에서 `Find` / lookup key 로 사용되는가?

**옵션**:
- **D-A-1** (가장 안전, ★권장): AtticCharacterSelector + 관련 코드 grep 으로 lookup 사용 여부 확인 후 결정 — Sprint 1 prelude
- D-A-2: 변경 X (단순 라벨 가정) — 위험
- D-A-3: 일괄 변경 (`CharBtn_default_boy` → `CharBtn_hero_warrior` 등) — 씬 파일 직접 편집 (위험)

**위험**: GameObject lookup 코드가 있으면 변경 후 NRE. 사전 확인 필수.

### 🟡 D-B — SaveData 다른 character id 관련 필드 마이그레이션 범위

**발견 (정적 grep)**:
- `LimitedShopService.HandleCharacterFirstSelect(string characterId)` 호출 — 캐릭터 첫 선택 트리거. SaveData 에 "캐릭터별 첫 선택했음" 플래그 가능 (LimitedShopOffer 의 `triggerCharacterFirstSelect`)

**의문**: SaveDataMigration 이 `selectedCharacterId` 외에 다음 필드도 매핑해야 하는가?
- `unlockedCharacters` list (있다면)
- `firstSelectedCharacters` set (있다면 — LimitedShop 트리거 한도)
- 캐릭터별 stat, level, equipment 데이터 (id key 사용 시)

**옵션**:
- **D-B-1** (★권장): SaveData class 전체 필드 grep + character id 사용 모두 매핑 추가 (Sprint 3b 범위 확장 +10분)
- D-B-2: selectedCharacterId 만 매핑, 나머지는 사용자 reset 안내 (위험)
- D-B-3: SaveData version bump + 마이그레이션 로직 일괄 처리

### 🟡 D-C — character_progression_v1.md 갱신 범위

**현재 상태**: docs/little_hero/character_progression_v1.md 가 옛 4 코어 (힘/빠름/슬기/마음) + 옛 키즈명 (용감한 소년 등) 사용. PIVOT v2 stats_v2 이후 미갱신.

**의문**: 5020 안에 포함 vs 별도 PR?

**옵션**:
- D-C-1 (★권장): 별도 PR — 5020 은 자산·SO·코드만, 디자인 문서 갱신은 5020.1 후속
- D-C-2: 5020 Sprint 6 추가 (디자인 문서 갱신 commit) — 5020 8 commit 이 됨
- D-C-3: 무시 (5020 종결 후 다른 작업) — 디자인 문서 stale 상태 잔존

### 🟡 D-D — `References/Selected_Characters/` 옛 자료 처리

**현재 상태**: 옛 키즈 reference PNG 3장 보존됨 (`Char_BraveGirl_idle.png` 등).

**의문**: 새 캐릭터 (시우/가영/연우) 와 톤 불일치 — 미래 혼란?

**옵션**:
- **D-D-1** (★권장): 보존 — history 자료, 5020 범위 외, 향후 작업 시 참고
- D-D-2: `References/Selected_Characters_v1_legacy/` 로 rename (명확 분리)
- D-D-3: 삭제 (history 손실 위험)

---

## 5. CLAUDE.md §15 정책 위반 점검

| 정책 | 5020 적용 | 위반 | 조치 |
|---|---|---|---|
| §15.1 SRP | StarterHeroAuditor (audit only), SaveDataMigration (id mapping only) | ✅ 통과 | — |
| §15.1 OCP | switch/분기 신규 X (Dictionary lookup) | ✅ 통과 | — |
| §15.4 SO 우선 | CharacterData SO 만 수정, 마법 숫자 0 (id 매핑은 const Dictionary) | ✅ 통과 | — |
| §15.6 100줄 룰 | StarterHeroAuditor ≤80L, SaveDataMigration ≤60L | ✅ 통과 | — |
| §15.7 DI | SaveDataMigration 정적 헬퍼, SaveSystem hook 1 줄 | ✅ 통과 | — |
| §15.9 Test | NUnit 8 신규 (108 → 116) | ✅ 통과 | — |
| §15.11 안티 | public 필드 추가 X, Update 추가 X, GameObject.Find 추가 X | ✅ 통과 | — |
| §15.12 Validator | StarterHero_PortraitWired 룰 추가 | ✅ 통과 | — |

→ §15 위반 0건. **Reject 강제 사유 없음** (Reject 는 시스템 통합 차원 R1 1건만).

---

## 6. 문서 일관성 점검

### 📝 I1 — NUnit 케이스 수 본문 ↔ 합산 미스매치

**위치**: Sprint PRD §7.3 (SaveDataMigrationTests) 5 케이스 명시 vs §7.5 합산 8 (4+4) — **5 케이스이면 9 = NUnit 117 이어야 함**

**현재 본문** (§7.3):
1. MigrateCharacterId_DefaultBoy_ReturnsHeroWarrior
2. MigrateCharacterId_BraveGirl_ReturnsHeroArcher
3. MigrateCharacterId_QuietFriend_ReturnsHeroMage
4. MigrateCharacterId_AlreadyNew_ReturnsSame
5. MigrateCharacterId_Empty_ReturnsEmpty

→ 5 케이스. 그런데 §1 표 / §7.5 / §10 / §13 은 모두 NUnit 4 / 합산 116.

**수정**:
- 옵션 A: §7.3 → 4 케이스 (Empty 제거 또는 AlreadyNew 와 통합)
- 옵션 B: §7.5 / §1 / §10 / §13 → 9 케이스 / NUnit 117 로 정정
- ★권장: **옵션 A** — Empty 케이스는 정적 헬퍼라면 보장됨 (TryGetValue 룰), 굳이 검증 X

---

## 7. 권장 변경

### 즉시 변경 (Reject 1 + Doc 1)

1. **R1**: Sprint 2 prelude 또는 Sprint 1 끝에 "wired PNG GUID 매핑 표" 작성 단계 추가
2. **I1**: SaveDataMigrationTests 4 케이스로 정리 (Empty 케이스 제거)

### 사용자 결정 후 변경 (Discuss 4)

3. **D-A**: Sprint 1 prelude 에 AtticCharacterSelector lookup 사용 여부 확인 단계 추가 → 사용 시 Sprint 3 에 GameObject 이름 변경 commit 추가
4. **D-B**: SaveData class 전체 필드 grep → SaveDataMigration 매핑 범위 확장 (Sprint 3b)
5. **D-C**: character_progression_v1.md 갱신 별도 PR 결정 / 5020 포함 결정
6. **D-D**: References/Selected_Characters/ 처리 결정 (보존 / rename / 삭제)

---

## 8. 종합 판정

**5020 PRD 는 OOP §15 정책 ✅ + PG-12 톤 ✅ + 성능 ✅** — 전반적으로 견고.
**그러나 시스템 통합 (Attic.unity / LimitedShop / SaveData 다른 필드)** 검증 누락이 있어 Sprint 1 prelude / Sprint 3b 범위 확장 필요.
**Sprint 2 의 wired PNG 매핑 모호 (R1)** 는 자산 손실 위험이라 필수 수정.

→ **다음 단계**: Phase 2.2 Rebuttal — R1 자동 수용 + Discuss 4 항목 정리 + I1 자동 수정.

---

## 9. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-critiquer) | 초기 — 8.9/10, Reject 1 / Discuss 4 / Accept 8 / Doc 1 |
