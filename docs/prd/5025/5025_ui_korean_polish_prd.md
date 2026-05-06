# 5025 Sprint PRD — UI Korean Polish

> 디자인: `docs/prd/5025_ui_korean_polish_prd.md`
> Sprint: 4 / commit: 4 / 시간: ~2h30m
> 의존성: 없음 (독립).

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | KoreanLabelAuditor Editor 도구 | 1 | 30분 |
| 2 | 우선순위 1~2 (Attic + HUD) | 1 | 30분 |
| 3 | 우선순위 3~6 (Map + Inventory + Equipment + Gacha) | 1 | 60분 |
| 4 | 우선순위 7~9 (Endgame/Forge/Memory) + 폰트 점검 | 1 | 30분 |
| **합계** | | **4** | **2h30m** |

---

## 2. Sprint 1 — KoreanLabelAuditor

### 2.1 신규 Editor 도구

**파일**: `Editor/KoreanLabelAuditor.cs` (신규, ~80L)
- `[MenuItem("Tools/LittleHero/Audit Korean Labels")]`
- 검사 대상: ChapterData / EquipmentTemplate / AffixDefinition / SubAffix / MythicDefinition / EndgameModeData / MetaUpgradeData / LimitedShopOffer / MemoryData / CharacterData
- 영문 검출: `Regex.IsMatch(text, @"[a-zA-Z]")` 또는 한글 비율 < 50%
- 보고서: `docs/little_hero/korean_label_audit_report.md` 자동 생성

### 2.2 보고서 형식

```markdown
# Korean Label Audit Report

생성: 2026-MM-DD

## 영문 잔존 (변경 필요 / 유지 필요 결정)

| SO 카테고리 | path | 필드 | 현재값 | 권장 |
|---|---|---|---|---|
| ChapterData | Chapter01.asset | displayName | "Lake Village" | "호숫가 마을" |
| ... | | | | |

## 한글/영문 혼재 (의도된 표기 검토)

| SO | path | 필드 | 현재값 | 결정 |
|---|---|---|---|---|
| AffixDefinition | affix_crit_chance.asset | displayName | "치명타 확률" | OK |
| ... | | | | |
```

### Sprint 1 commit
```
5025 Sprint 1 / T01 — KoreanLabelAuditor Editor 도구 (1/4)

[변경]
  + Editor/KoreanLabelAuditor.cs (~80L)
    - Tools/LittleHero/Audit Korean Labels 메뉴
    - 10 SO 카테고리 영문 검출 + 보고서 생성

[검증]
  - 컴파일 errors=0
  - 실행 시 docs/little_hero/korean_label_audit_report.md 생성
  - 영문 잔존 N건 식별 → Sprint 2~4 작업 baseline
```

---

## 3. Sprint 2 — 우선순위 1~2 (Attic + HUD)

### 3.1 Attic 영역 추가 검수

5020.1 fix 후 추가 영역:
- AtticGroupLayoutPresenter 의 4 그룹 라벨
- AtticPanelEntryView 진입 버튼 라벨
- AtticEndgameMenuView 4 mode 진입 라벨

검수 → spot fix.

### 3.2 HUD 영역

`HUDController.cs` 또는 Scene 의 HUD Text 컴포넌트:
- Level "Lv. 5" → "레벨 5"
- Wave "Wave 12" → "웨이브 12"
- Health "HP 50/100" → "체력 50/100"
- Currency 4종 (★/✦/◇/⋆) — 아이콘만 OK

### Sprint 2 commit
```
5025 Sprint 2 / T02 — Attic + HUD 한국어 spot fix (2/4)

[변경]
  ~ Attic.unity 또는 HUDController.cs (영문 라벨 → 한글)
  ~ 라벨 N건

[검증]
  - 사용자 Play 시각 — 모든 Attic + HUD 라벨 한국어
  - 폰트 잘림 0건
```

---

## 4. Sprint 3 — 우선순위 3~6 (Map/Inventory/Equipment/Gacha)

### 4.1 WondrousMapPanel 35 챕터

KoreanLabelAuditor 보고서 기반 ChapterData displayName/description 일괄 갱신.

### 4.2 InventoryPanel + EquipmentCardPanel + GachaPanel

각 Panel 의 라벨:
- "Inventory" → "인벤토리"
- "Slot Empty" → "빈 슬롯"
- "Enhance" → "강화"
- "Summon" → "소환"
- "Stardust ★" → "별가루 ★"

Inspector 직접 편집 또는 .prefab YAML 직접.

### Sprint 3 commit
```
5025 Sprint 3 / T03 — Map/Inventory/Equipment/Gacha 한국어 (3/4)

[변경]
  ~ ChapterData 35 displayName 영문 잔존 fix
  ~ InventoryPanel.prefab + EquipmentCardPanel.prefab + GachaPanel.prefab Text 영문 → 한글

[검증]
  - KoreanLabelAuditor 재실행: 영문 잔존 0
  - 사용자 Play 시각 검증 — 4 Panel 모두 한국어
```

---

## 5. Sprint 4 — 우선순위 7~9 + 폰트 점검

### 5.1 Endgame/Forge/Memory

남은 Panel 의 라벨:
- EndgameMenuPanel 4 mode
- ConversionForgePanel "Forge" → "변환 모루"
- LimitedShopPanel + StarlightShopPanel
- MemoryEffectOverlay + CharacterInfoPanel

### 5.2 폰트 잘림 시각 검사

사용자 Play 시 다음 사례 보고 받음 후 fix:
- 한글 자모 부분 잘림
- maxWidth 좁아 줄바꿈 어색
- font size 너무 작음

### 5.3 KoreanLabelAuditor 최종 검사

```bash
# 영문 잔존 0건 확인
grep "영문 잔존" docs/little_hero/korean_label_audit_report.md
# 기대: 0건
```

### Sprint 4 commit
```
5025 Sprint 4 / T04 — 잔여 영역 + 폰트 점검 (4/4) — 5025 종결

[변경]
  ~ EndgameMenuPanel + ConversionForgePanel + LimitedShopPanel + StarlightShopPanel + MemoryEffectOverlay + CharacterInfoPanel
  ~ 폰트 잘림 spot fix N건

[검증]
  - KoreanLabelAuditor: 영문 잔존 0
  - 사용자 Play 시각 — 모든 UI 한국어 + 폰트 정상
  - changelog + CLAUDE.md
```

---

## 6. 의존성 그래프

```
Sprint 1 (Auditor) → Sprint 2 (Attic+HUD) → Sprint 3 (Map+Inv+Equip+Gacha) → Sprint 4 (잔여+폰트)
```

직렬. Sprint 1 의 보고서가 Sprint 2~4 baseline.

---

## 7. 완료 정의

- [ ] KoreanLabelAuditor 실행 시 영문 잔존 0건
- [ ] 사용자 Play 시각 — 모든 UI 한국어
- [ ] 폰트 잘림 spot fix 완료 (사용자 보고 0건)
- [ ] C2 NUnit 회귀 0
- [ ] `/prd-evaluate 5025` PASS

---

## 8. 알려진 위험

| 위험 | 완화 |
|---|---|
| 의도된 영문 (예 affix targetName 내부 enum) → 변경 시 코드 영향 | KoreanLabelAuditor 가 displayName 만 검사 (targetName 제외) |
| 폰트 잘림 fix 시 Layout 깨짐 | Game 뷰 시각 검증 후 commit |
| SO YAML 직접 편집 시 인코딩 (Unicode escape) | 5020 패턴 답습 (Edit tool 한글 → YAML escape 자동) |

---

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 4 Sprint / 4 commit / 2h30m |
