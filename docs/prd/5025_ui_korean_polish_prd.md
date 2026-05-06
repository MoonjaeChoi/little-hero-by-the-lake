# 5025 — UI Korean Polish

> **목적**: G3 한국어 검수 + Legacy Text 폰트 잘림 spot fix.
> **scope**: UI 텍스트 / SO displayName / 폰트 잘림 영역. 동작 변경 0.

---

## 1. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | CLAUDE.md §13 #4 "G3 한국어 검수" 미완료 → 5025 분리 |

---

## 2. 현재 시스템 분석

### 2.1 한국어 텍스트 분포

**SO 인스턴스 (400+)**:
- ChapterData 35: displayName / description
- EquipmentTemplate 16 + 가챠 18: displayName / flavorText
- AffixDefinition 22 + SubAffix 12: displayName
- MythicDefinition 4 / EndgameModeData 4 / MetaUpgradeData 3 / LimitedShopOffer 12: 각각 한국어 라벨
- MemoryData 4 / CharacterData 3 (5020 갱신 완료): displayName

**UI Panel (30+)**:
- 버튼 라벨 / 헤더 / 툴팁 (Legacy Text 컴포넌트)
- 5020 에서 Attic 3 라벨 fix 완료 (5020.1)
- 다른 Panel 의 한국어 잔존 검수 미실시

**Legacy Text 폰트 잘림 (CLAUDE.md §10)**:
- TMP X (URP 셰이더 깨짐 — Unity 6) → Legacy Text 사용
- 한글 폰트 max width 또는 line spacing 문제 가능

### 2.2 알려진 문제 (5020 진행 중 발견)

- Attic 캐릭터 라벨 "용감한 소년" → "전사 시우" 갱신 (5020.1 완료)
- WondrousMapPanel 35 챕터 displayName 일부 영문 잔존 가능성
- HUD wave 라벨 "Wave N" → 한글 "웨이브 N" 통일 검토
- DamageNumberFloater "1.2K" / "15K" 압축 — 한국어 영향 X (숫자만)

### 2.3 영향 범위

- **출시 사용자 첫인상**: 한국 시장 1차 — UI 영문 혼재 시 어색
- **검수 비용**: 1~3시간 (단순 spot fix)

---

## 3. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | 전체 SO + Panel 한국어 검수 (체계적) | 완성도 ↑ | 시간 3시간+ |
| B (★권장) | 사용자 시각 보고 + spot fix (반응형) | 효율, 우선순위 명확 | 누락 가능성 |
| C | 자동 grep + Editor 도구 검출 | 자동화 | 도구 작성 비용 |

**권장 옵션 B** — 사용자 Play 시 어색한 영역 보고 → spot fix. 1~3시간 효율.

---

## 4. 권장 설계

### 4.1 검수 영역 (우선순위)

| # | 영역 | 우선순위 | 예상 fix |
|---|---|---|---|
| 1 | Attic 진입 (5006 4 그룹 + AtticPanelEntryView) | 🔴 High (사용자 첫 화면) | spot |
| 2 | HUD (Level/Wave/Health/Currency 라벨) | 🔴 High | spot |
| 3 | WondrousMapPanel 35 챕터 노드 (displayName/description) | 🟡 Medium | bulk |
| 4 | InventoryPanel + Selection Panel v2 (12 슬롯 + 페이징) | 🟡 Medium | spot |
| 5 | EquipmentCardPanel + EnhanceModalPanel | 🟡 Medium | spot |
| 6 | GachaPanel + ResultPanel + StarforceModalPanel | 🟡 Medium | spot |
| 7 | EndgameMenuPanel + 4 mode (Daily/Weekly/Infinite/Hardcore) | 🟢 Low | spot |
| 8 | ConversionForgePanel + LimitedShopPanel + StarlightShopPanel | 🟢 Low | spot |
| 9 | MemoryEffectOverlay + CharacterInfoPanel | 🟢 Low | spot |

### 4.2 폰트 잘림 검사

**케이스**:
- maxWidth 작아 줄바꿈 (예 "전사" 두 글자만 보임)
- line spacing 좁아 한글 자모 잘림
- font fallback 깨짐 (특수 문자)

**해결**:
- Text 컴포넌트의 `Horizontal Overflow: Wrap` / `Vertical Overflow: Truncate` 조정
- RectTransform width 늘림
- font size 줄임 (최후)

### 4.3 SO bulk 갱신 — Editor 도구 (선택)

35 챕터 displayName 처럼 다수일 경우:

```csharp
// Editor/Tools/KoreanLabelAuditor.cs (신규)
[MenuItem("Tools/LittleHero/Audit Korean Labels")]
public static void Run()
{
    var chapters = AssetDatabase.FindAssets("t:ChapterData")
        .Select(g => AssetDatabase.LoadAssetAtPath<ChapterData>(AssetDatabase.GUIDToAssetPath(g)));
    int englishCount = 0;
    foreach (var ch in chapters)
        if (ContainsEnglish(ch.displayName)) { Debug.LogWarning($"{ch.id}: {ch.displayName}"); englishCount++; }
    Debug.Log($"Korean Audit: {englishCount} 영문 잔존");
}
```

### 4.4 CLAUDE.md §15 정합

- §15.4 SO 우선 (한국어 displayName 모두 SO 필드)
- 하드코딩 라벨 (예 5020.1 fix 후 발견된 scene 라벨) 도 SO 바인딩으로 점진 전환 권장

---

## 5. 마이그레이션 단계

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | KoreanLabelAuditor Editor 도구 + 자동 검출 | 1 | 30분 |
| 2 | 우선순위 1~2 (Attic + HUD) spot fix | 1 | 30분 |
| 3 | 우선순위 3~6 (Map + Inventory + Equipment + Gacha) bulk + spot | 1 | 60분 |
| 4 | 우선순위 7~9 (Endgame + Forge + Memory) + 폰트 잘림 점검 | 1 | 30분 |
| **합계** | | **4** | **2h30m** |

---

## 6. 알려진 위험

| 위험 | 완화 |
|---|---|
| 영문 의도적 (예 "Lv. 50" 표기) → 한글 변경 시 의도 변경 | KoreanLabelAuditor 보고 후 사용자 결정 (변경 / 유지) |
| 폰트 잘림 fix 시 Layout 깨짐 | RectTransform 변경 후 Game 뷰 시각 검증 |
| SO bulk 갱신 시 GUID 보존 (5020 답습) | YAML 직접 편집 또는 Editor 도구 — Inspector 갱신 |
| 한글 자모 인코딩 (Unity .asset YAML \uXXXX 자동) | 5020 패턴 답습 (Edit tool 한글 입력 → YAML 자동 escape) |

---

## 7. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-generator) | 초기 — CLAUDE.md §13 #4 backlog → 5025 분리 |
