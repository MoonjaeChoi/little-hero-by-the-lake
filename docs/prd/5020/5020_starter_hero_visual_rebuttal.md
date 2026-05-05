# 5020 Starter Hero Visual — PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: 2026-05-06
> **입력**: `5020_starter_hero_visual_critique.md` (8.9/10, R1 + Discuss 4 + Accept 8 + I1)
> **산출**: 본 rebuttal + `_v2.md` 초안 + `_discussion_topics.md`

---

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| 시스템 통합 | R1 — Sprint 2 wired PNG 매핑 모호 (자산 손실 위험) | High | **Accepted** | Sprint 1 prelude 에 GUID 매핑 표 작성 단계 추가 | ✅ |
| 문서 일관성 | I1 — NUnit 케이스 수 미스매치 (5 vs 4) | Low | **Accepted** | SaveDataMigrationTests 4 케이스 정리 (Empty 제거) | ✅ |
| 시스템 통합 | D-A — Attic.unity `CharBtn_<id>` GameObject 이름 변경 | Medium | **Accepted (사실 검증 완료)** | **변경 불필요** — AtticCharacterSelector grep 결과 GameObject 이름 사용 X (직렬화 array wiring) | ✅ |
| 시스템 통합 | D-B — SaveData 다른 character id 필드 마이그레이션 범위 | High | **Accepted (사실 검증 완료)** | SaveDataMigration 에 `unlockedCharacterIds[]` 매핑 추가 (Sprint 3b 4 줄 확장) | ✅ |
| 문서 동기화 | D-C — `character_progression_v1.md` 갱신 범위 (옛 4 코어 + 옛 키즈명) | Medium | **Discuss** | 사용자 결정 (별도 PR vs 5020 포함) → discussion_topics | △ |
| 자료 정리 | D-D — `References/Selected_Characters/` 옛 자료 처리 | Low | **Discuss** | 사용자 결정 (보존 vs rename vs 삭제) → discussion_topics | △ |
| OOP §15 | A1~A8 (8건 Accept) | — | **이미 Accepted** | 변경 X — PRD v1 그대로 | ✅ |

---

## 2. 품질 메트릭

| 항목 | 값 |
|---|---|
| 총 Discussion Points | 6 (R1 + I1 + D-A + D-B + D-C + D-D) |
| **Accepted** | **4건 (R1, I1, D-A 사실 결정, D-B 사실 결정)** — 67% |
| **Discuss** (사용자 결정) | **2건 (D-C, D-D)** — 33% |
| Rejected | 0건 |
| §15 정합률 | 100% (위반 0) |

> **사실 검증 자동 처리**: D-A / D-B 는 critique 단계의 "사용자 결정 필요" 표기였으나, rebuttal 단계 grep 으로 사실 확정 → Accepted 로 전환. mediator 부담 ↓.

---

## 3. Detailed Responses

### 3.1 [시스템 통합 — High] R1 — Sprint 2 wired PNG 매핑 모호

**Original Criticism** (critique §3 R1):
> 현재 Generated_v2/Characters/ 에 6 PNG 가 공존. 어느 PNG 가 SO portrait 필드에 wired 인가? PRD 미명시. Sprint 3a `git rm` 6 파일이 wired PNG 까지 삭제할 위험.

- **Rebuttal**: **수용** (자산 손실 = 출시 직전 critical risk).
- **근거**: CLAUDE.md §13 "안전 패턴 — 한 사이클 분리 commit", §15.10 "사용자 결정 우선" — Sprint 3a 의 `git rm` 명령이 명확 사전 검증 없이 실행되면 portrait wiring 끊김.
- **Proposed Action**:
  - **Sprint 1 prelude (Sprint 1 의 첫 단계)** 에 GUID 매핑 표 작성 추가:
    ```bash
    # 3 SO 의 portrait GUID 추출
    grep "portrait:" LittleHero/Assets/_Project/Data/Characters/Char_*.asset
    # 6 PNG .meta 의 GUID 와 매핑
    grep "guid:" LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_*.png.meta
    # → docs/prd/5020/5020_png_guid_map.md 생성
    ```
  - Sprint 2 출력 파일명을 wired PNG 와 동일 베이스명으로 통일 (예 wired 가 `Char_DefaultBoy.png` 이면 신 PNG 도 동일 GUID 보존 위해 `Char_DefaultBoy.png` 덮어쓰기 후 Sprint 3a 에서 `git mv` 로 rename).
  - Sprint 3a 의 `git rm` 명령 수정: wired PNG 는 `git mv` (rename + GUID 보존), 비-wired (`_idle` suffix) 만 `git rm`.
- **PRD Impact**:
  - Sprint PRD §2 (Sprint 1) 에 prelude step 추가
  - Sprint PRD §3 (Sprint 2) 출력 명세 갱신 (wired 베이스명 통일)
  - Sprint PRD §4 (Sprint 3a) `git rm` → 일부 `git mv` 로 정정
- **Little Hero Context**: 5009/5010/5011/5019 GUID 보존 정책 답습. PNG 만 덮어쓰기 = `.meta` 보존 = wiring 자동 유지.

### 3.2 [문서 일관성 — Low] I1 — NUnit 케이스 수 미스매치

**Original Criticism** (critique §6 I1):
> §7.3 SaveDataMigrationTests 5 케이스 명시 vs §7.5 합산 8 (4+4) — 5 케이스이면 9 = NUnit 117 이어야 함.

- **Rebuttal**: **수용** (옵션 A — Empty 케이스 제거).
- **근거**: 정적 헬퍼 `MigrateCharacterId` 의 첫 라인 `if (string.IsNullOrEmpty(oldId)) return oldId;` 가 보장하므로 Empty 케이스 단위 테스트 잉여. CLAUDE.md §15.9 "핵심 정적 service 우선 테스트" 정합 — 핵심 매핑 4 케이스만.
- **Proposed Action**: SaveDataMigrationTests 4 케이스 (3 매핑 + AlreadyNew passthrough). `MigrateCharacterId_Empty_ReturnsEmpty` 삭제.
- **PRD Impact**: Sprint PRD §7.3 5번 항목 제거. §1·§7.5·§10·§13 NUnit 합산 116 (이미 정합) — 변경 X.

### 3.3 [시스템 통합 — Medium] D-A — Attic.unity GameObject 이름 변경

**Original Criticism** (critique §4 D-A):
> Attic.unity 에 CharBtn_default_boy 등 GameObject 이름 하드코드. AtticCharacterSelector 에서 lookup key 사용 여부 불확실.

- **Rebuttal**: **사실 검증 완료 → Accepted (변경 불필요)**.
- **근거 (정적 grep, AtticCharacterSelector.cs:22~82)**:
  ```csharp
  [SerializeField] private CharacterButton[] _buttons;  // 직렬화 array
  
  private CharacterData ResolveCharacterById(string id)
  {
      foreach (var entry in _buttons)
          if (entry?.character != null && entry.character.id == id) return entry.character;  // CharacterData.id 만 사용
  }
  ```
  → GameObject 이름 사용 X (배열 wiring 만). `CharBtn_default_boy` 등은 Unity 에디터 가독성용 라벨.
- **Proposed Action**:
  - **Sprint 변경 X** — Attic.unity 씬 파일 변경 0
  - 다만 사용자가 "정합성" 차원에서 GameObject 이름 변경 원할 수 있음 → **Sprint 1 prelude 에 사실 명시 + 사용자 1회 확인** ("CharBtn 이름 변경하시겠습니까? 권장: 변경 X")
- **PRD Impact**: Sprint PRD §2 (Sprint 1) 에 D-A 사실 결정 명시 ("변경 불필요 — AtticCharacterSelector 직렬화 wiring").

### 3.4 [시스템 통합 — High] D-B — SaveData 다른 character id 필드 마이그레이션

**Original Criticism** (critique §4 D-B):
> SaveDataMigration 이 selectedCharacterId 외 다른 필드 (unlockedCharacters list 등) 도 매핑해야?

- **Rebuttal**: **사실 검증 완료 → Accepted (1 필드 추가)**.
- **근거 (정적 grep, SaveSystem.cs:14~78 SaveData 전수 조사)**:
  - **character ID 필드 발견**:
    1. `selectedCharacterId` (line 17) ✅ PRD §3b 명시
    2. **`unlockedCharacterIds[]` (line 18) ⚠️ PRD 누락 — 매핑 추가 필수**
  - **무관 필드** (다른 ID 도메인): unlockedEquipmentIds, discoveredMemoryIds, defeatedEnemyIds, equipmentInstances, permanentlyLockedInstanceIds, limitedOfferTriggers, purchasedLimitedOfferIds, activeChapterId, pendingChapterClearedId
- **Proposed Action**:
  - SaveDataMigration.Apply 에 `unlockedCharacterIds[]` 일괄 매핑 추가 (4줄):
    ```csharp
    if (data.unlockedCharacterIds != null)
        for (int i = 0; i < data.unlockedCharacterIds.Length; i++)
            data.unlockedCharacterIds[i] = MigrateCharacterId(data.unlockedCharacterIds[i]);
    ```
  - SaveDataMigrationTests 에 `MigrateUnlockedCharacterIds_ArrayMapping_Works` 1 케이스 추가 (NUnit 4 → 5)
- **PRD Impact**:
  - Sprint PRD §5 (Sprint 3b SaveDataMigration.cs) 본문 갱신 — Apply 메서드 +4줄
  - SaveDataMigration ≤60L → 64L (여전히 §15.6 100줄 룰 준수)
  - §1 합산 NUnit 116 → 117

### 3.5 [문서 동기화 — Medium] D-C — character_progression_v1.md 갱신

→ **Discuss** (사용자 결정 → discussion_topics §D-C)

### 3.6 [자료 정리 — Low] D-D — References/Selected_Characters/ 처리

→ **Discuss** (사용자 결정 → discussion_topics §D-D)

---

## 4. PRD v2 적용 변경 요약 (Accepted 4건 반영)

수용 항목만 v2 PRD 초안 (`5020_starter_hero_visual_v2.md`) 에 반영:

| 영역 | 변경 |
|---|---|
| **Sprint 1 prelude** | + GUID 매핑 표 작성 (`5020_png_guid_map.md`) — R1 |
| **Sprint 1 prelude** | + D-A 사실 결정 명시 ("CharBtn 이름 변경 X") — D-A |
| **Sprint 2 출력 명세** | wired 베이스명 통일 (예 `Char_DefaultBoy.png` 덮어쓰기) — R1 |
| **Sprint 3a `git rm`** | wired PNG 는 `git mv` 로 정정, 비-wired 만 `git rm` — R1 |
| **Sprint 3b SaveDataMigration** | + `unlockedCharacterIds[]` 매핑 4줄 — D-B |
| **Sprint 3b 라인 수** | ≤60L → ≤64L (§15.6 준수) |
| **Sprint 4 NUnit** | SaveDataMigrationTests 4 케이스 (Empty 제거) + 1 케이스 (UnlockedCharacterIds 매핑) = **5 케이스** — I1 + D-B |
| **§1 합산 NUnit** | 116 → **117** |
| **§7.3 SaveDataMigrationTests** | 5 케이스 → 4 + 1 = 5 케이스 (Empty 제거 + UnlockedCharacterIds 추가) |

> Discuss 2건 (D-C / D-D) 은 v2 미반영, mediator 후 최종 확정.

---

## 5. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-rebuttal) | 초기 — Accepted 4 / Discuss 2 / Rejected 0. D-A/D-B 사실 검증으로 Discuss → Accept 전환 |
