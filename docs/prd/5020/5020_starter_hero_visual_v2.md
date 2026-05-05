# 5020 Starter Hero Visual — PRD v2 (Mediator 확정)

> **PRD v2 (최종, Mediator 확정 — 2026-05-06)**
> 원본: `5020_starter_hero_visual_prd.md`
> Rebuttal: `5020_starter_hero_visual_rebuttal.md`
> Decisions: `5020_starter_hero_visual_decisions.md`
> Discussion Topics: `5020_starter_hero_visual_discussion_topics.md` (모두 결정 완료)
>
> **다음 단계**: `/prd-breakdown` (Phase 3) → `/prd-contract` (Phase 4) → 구현 (Phase 5) → `/prd-evaluate` (Phase 6)

> **본 v2 의 적용 변경 (Rebuttal Accepted 4건 + Mediator Decisions 2건)**:
> - **Rebuttal Accepted**:
>   - R1 (Sprint 2 wired PNG 매핑) — Sprint 1 prelude + Sprint 2/3a 명세 갱신
>   - I1 (NUnit 케이스 정리) — SaveDataMigrationTests Empty 제거
>   - D-A (Attic.unity GameObject 이름) — 변경 불필요 사실 결정 명시
>   - D-B (SaveData unlockedCharacterIds 매핑) — Sprint 3b SaveDataMigration +4줄, NUnit +1 (116→117)
> - **Mediator Decisions**:
>   - D-C=A (character_progression_v1.md 별도 PR 5020.1 후속) — Sprint PRD 변경 X, §10.1 후속 메모만 추가
>   - D-D=B (Selected_Characters → Selected_Characters_v1_legacy rename) — Sprint 3a `git mv` 1줄 + 5분

---

## 변경 차분 표 (원본 PRD vs v2)

> **참고**: 원본 PRD `docs/prd/5020/5020_starter_hero_visual_prd.md` 가 정본. 본 v2 는 **Rebuttal Accepted 차분만 명시** (mediator 단계 decision 반영 후 본문 통합).

### 차분 1 — Sprint 1 prelude 추가 (R1 + D-A)

**원본**: Sprint 1 = "Prompt 표 작성 (45분)"

**v2 갱신**: Sprint 1 = **"Prelude (사실 검증) + Prompt 표 작성 (60분, +15분)"**

**Sprint 1 Prelude (신규)**:
```bash
# Prelude Step 1 — wired PNG GUID 매핑 표 (R1)
grep "portrait:" LittleHero/Assets/_Project/Data/Characters/Char_*.asset \
  > docs/prd/5020/5020_png_guid_map.md
grep "guid:" LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_*.png.meta \
  >> docs/prd/5020/5020_png_guid_map.md
# 수동: SO portrait GUID ↔ PNG GUID 매핑 표 정리

# Prelude Step 2 — D-A 사실 결정 명시
# AtticCharacterSelector 직렬화 wiring → CharBtn_<id> GameObject 이름 변경 불필요
# (Attic.unity 씬 파일 변경 0)
```

### 차분 2 — Sprint 2 출력 명세 갱신 (R1)

**원본**: Sprint 2 출력 = `Char_HeroWarrior.png` 등 신 파일명

**v2 갱신**:
- wired PNG 와 동일 베이스명으로 덮어쓰기 → GUID 보존
  - 예: wired 가 `Char_DefaultBoy.png` 면 신 PNG 도 `Char_DefaultBoy.png` (덮어쓰기)
- Sprint 3a 에서 `git mv Char_DefaultBoy.png Char_HeroWarrior.png` (rename) — `.meta` 동행

### 차분 3 — Sprint 3a `git rm` → 일부 `git mv` (R1)

**원본**:
```bash
git rm Char_DefaultBoy.png
git rm Char_DefaultBoy_idle.png
# ...
```

**v2 갱신**:
```bash
# wired PNG (Sprint 2 에서 신 PNG 로 덮어쓴 것) → git mv 로 rename
git mv Char_DefaultBoy.png      Char_HeroWarrior.png
git mv Char_DefaultBoy.png.meta Char_HeroWarrior.png.meta
# (BraveGirl/QuietFriend 동일)

# 비-wired PNG (예 _idle suffix) → git rm
git rm Char_DefaultBoy_idle.png
git rm Char_DefaultBoy_idle.png.meta
# (BraveGirl/QuietFriend 동일)
```

> **사전 조건**: Sprint 1 prelude 의 GUID 매핑 표 결과로 wired vs 비-wired 식별 완료.

### 차분 4 — Sprint 3b SaveDataMigration +4줄 (D-B)

**원본** (Sprint PRD §5):
```csharp
public static SaveData Apply(SaveData data)
{
    if (data == null) return data;
    data.selectedCharacterId = MigrateCharacterId(data.selectedCharacterId);
    return data;
}
```

**v2 갱신**:
```csharp
public static SaveData Apply(SaveData data)
{
    if (data == null) return data;
    data.selectedCharacterId = MigrateCharacterId(data.selectedCharacterId);
    
    // D-B (rebuttal): unlockedCharacterIds[] 일괄 매핑
    if (data.unlockedCharacterIds != null)
        for (int i = 0; i < data.unlockedCharacterIds.Length; i++)
            data.unlockedCharacterIds[i] = MigrateCharacterId(data.unlockedCharacterIds[i]);
    
    return data;
}
```

→ SaveDataMigration.cs 라인 수 ≤60L → **≤64L** (§15.6 100줄 룰 여전히 준수).

### 차분 5 — Sprint 4 NUnit 정정 (I1 + D-B)

**원본** (§7.3): SaveDataMigrationTests 5 케이스 (DefaultBoy/BraveGirl/QuietFriend/AlreadyNew/Empty)

**v2 갱신**: SaveDataMigrationTests 5 케이스
1. `MigrateCharacterId_DefaultBoy_ReturnsHeroWarrior`
2. `MigrateCharacterId_BraveGirl_ReturnsHeroArcher`
3. `MigrateCharacterId_QuietFriend_ReturnsHeroMage`
4. `MigrateCharacterId_AlreadyNew_ReturnsSame`
5. **`MigrateUnlockedCharacterIds_ArrayMapping_Works` (D-B 신규)**

> **제거**: `MigrateCharacterId_Empty_ReturnsEmpty` (I1 — TryGetValue 룰 보장)
> **추가**: `MigrateUnlockedCharacterIds_ArrayMapping_Works` (D-B — array 매핑 검증)

### 차분 6 — NUnit 합산 117 (D-B)

**원본**: §1·§7.5·§10·§13 모두 NUnit 108 → 116

**v2 갱신**: NUnit 108 → **117** (CharacterDataIntegrityTests 4 + SaveDataMigrationTests 5 = 9 신규)

### 차분 7 — Sprint 3b 시간 +5분 (D-B)

**원본**: Sprint 3b = 30분

**v2 갱신**: Sprint 3b = **35분** (4줄 추가 + 단위 테스트 케이스 1개 — Sprint 4 와 합산 시간 영향)

### 차분 8 — 변경 이력 항목 추가

**v2 갱신** (Sprint PRD §13):
```markdown
| 2026-05-06 | Claude (prd-rebuttal Phase 2.2) | **v2 초안**: R1 (wired PNG 매핑) + I1 (NUnit 정리) + D-A (CharBtn 변경 X 사실) + D-B (unlockedCharacterIds 매핑) Accepted. Discuss 2건 (D-C, D-D) mediator 대기 |
| 2026-05-06 | Claude (prd-mediator Phase 2.3, ai-assisted) | **v2 확정**: D-C=A (별도 PR), D-D=B (Selected_Characters_v1_legacy rename, 사용자 오버라이드). Sprint 3a +1줄 +5분, 합산 7 commit 유지 |
```

### 차분 9 — Sprint 3a `git mv` +1줄 (D-D=B Mediator)

**원본 (rebuttal v2 초안)** Sprint 3a `git mv` 명령:
```bash
git mv Char_DefaultBoy.png      Char_HeroWarrior.png
git mv Char_DefaultBoy.png.meta Char_HeroWarrior.png.meta
# (BraveGirl/QuietFriend 동일)
```

**v2 갱신 (mediator 최종)**:
```bash
git mv Char_DefaultBoy.png      Char_HeroWarrior.png
git mv Char_DefaultBoy.png.meta Char_HeroWarrior.png.meta
# (BraveGirl/QuietFriend 동일)

# D-D=B (mediator 결정): Selected_Characters 폴더 명확 분리
git mv LittleHero/Assets/_Project/Art/References/Selected_Characters \
       LittleHero/Assets/_Project/Art/References/Selected_Characters_v1_legacy
```

**Sprint 3a commit 메시지 추가** (한 줄):
```
[변경]
  ...
  R References/Selected_Characters → Selected_Characters_v1_legacy (D-D=B 옛 자료 명확 분리)
```

**Sprint 3a 시간**: 20분 → **25분** (+5분, rename 작업)
**합산 commit**: 7 (변경 X — Sprint 3a 안에 통합)
**합산 시간**: 4~5h → **4h05m~5h05m**

### 차분 10 — Sprint PRD §10 후속 PRD 메모 (D-C=A Mediator)

**원본** §10 후속 PRD:
```
- 5021 (제안): Phase A 절차적 모션 코드
- 5022 (출시 후): Phase B 주인공 PSB skeletal rig
```

**v2 갱신 (mediator 최종)**:
```
- 5020.1 (제안, D-C=A 결정): character_progression_v1.md stats_v2 정합 갱신
  - 옛 4 코어 (힘/빠름/슬기/마음) → 3 코어 (힘/민첩/지혜)
  - 옛 키즈명 (용감한 소년 등) → 신 직업 정합 (전사 시우 등)
- 5021 (제안): Phase A 절차적 모션 코드
- 5022 (출시 후): Phase B 주인공 PSB skeletal rig
```

---

## 미결 항목 (mediator 결정 대기)

✅ **모두 결정 완료** — `5020_starter_hero_visual_decisions.md` 참조

| 항목 | 결정 | 영향 |
|---|---|---|
| **D-C** character_progression_v1.md 갱신 | **A — 별도 PR (5020.1 후속)** | Sprint PRD 변경 X, §10 후속 메모만 |
| **D-D** References/Selected_Characters/ 처리 | **B — rename to `_v1_legacy`** | Sprint 3a +1줄 +5분 |

---

## 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-rebuttal) | v2 초안 — Rebuttal Accepted 4건 (R1/I1/D-A/D-B) 차분 명시 |
| 2026-05-06 | Claude (prd-mediator, ai-assisted) | **v2 확정** — D-C=A (별도 PR) / D-D=B (rename, 사용자 오버라이드). Sprint 3a +1줄 +5분, 합산 7 commit 유지 |
