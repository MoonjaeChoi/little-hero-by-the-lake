# InventorySelectionPanel v2 — 페이징 + 썸네일 + 옵션 표시

> **목표**: 가챠 누적 54+ 인스턴스를 사용자가 빠르게 식별·비교·장착할 수 있는 정식 UI.
> 1080×1920 portrait (모바일) 한 손 친화 + 페이지 네비게이션 + 썸네일 카드 + 옵션 6라인 상세.

## 0. 결정 이력

- 2026-05-03: 사용자 요청 — 페이징·썸네일·옵션 표시
- 본 문서: 상하 분할 (그리드 상단 + 상세 하단) 레이아웃

## 1. 화면 레이아웃 (1080×1920 portrait)

```
┌─ Title bar (높이 120) ─────────────────────────────────┐
│  📦 무기 — 보유 장비    페이지 1 / 6        [✕ 닫기]   │
├─ Grid 영역 (3×3 = 9 카드, 높이 1080) ───────────────────┤
│  ┌──────┐ ┌──────┐ ┌──────┐                              │
│  │ A 카드│ │ B 카드│ │ C 카드│   카드 320×320              │
│  └──────┘ └──────┘ └──────┘                              │
│  ┌──────┐ ┌──────┐ ┌──────┐                              │
│  │ D     │ │ E     │ │ F     │                            │
│  └──────┘ └──────┘ └──────┘                              │
│  ┌──────┐ ┌──────┐ ┌──────┐                              │
│  │ G     │ │ H     │ │ I     │                            │
│  └──────┘ └──────┘ └──────┘                              │
├─ Pagination (높이 100) ─────────────────────────────────┤
│         [◀ 이전]   1 / 6   [다음 ▶]                      │
├─ Detail 영역 (높이 580) ────────────────────────────────┤
│  ┌────┐ 빠른 단검  +5  영웅                              │
│  │아이│                                                   │
│  │콘  │ 주옵션:                                            │
│  │128 │  ▸ 공격력 +12.5%                                 │
│  └────┘  ▸ 치명타 확률 +5%                               │
│         ▸ 공격속도 +8%                                    │
│         부옵션:                                           │
│         ▸ 힘 +3                                            │
│         ▸ 이동속도 +0.5                                    │
│         ▸ 별가루 획득 +10%                                 │
│                                                            │
│  [🟢 장착]  [🔴 해제]                                    │
└────────────────────────────────────────────────────────────┘
```

## 2. 그리드 카드 디자인 (320×320)

```
┌─ Card (320×320, 등급 색 테두리) ──┐
│ ┌─ Highlight Border (선택 시) ──┐ │
│ │                                │ │
│ │  ┌─────────────────────────┐  │ │
│ │  │                         │  │ │
│ │  │   Icon (260×260)        │  │ │  ← 큰 썸네일
│ │  │   preserveAspect        │  │ │
│ │  │                         │  │ │
│ │  └─────────────────────────┘  │ │
│ │                                │ │
│ │  빠른 단검 (24pt 흰색)         │ │  ← 이름
│ │  +5 (20pt 골드, 상우)         │ │  ← 강화
│ │  영웅 (18pt 보라, 하우)        │ │  ← 등급
│ │                                │ │
│ │  ▶ 장착중 (녹, 좌하 배지)     │ │  ← 장착 라벨
│ │                                │ │
│ └────────────────────────────────┘ │
└────────────────────────────────────┘
```

## 3. 옵션 라인 표시 형식

### 3.1 한국어 라벨 매핑 (StatTarget)

| StatTarget | 한국어 |
|---|---|
| FinalDamage | 최종 데미지 |
| AttackDamage | 공격력 |
| CritDamage | 치명타 피해 |
| CritChance | 치명타 확률 |
| AttackSpeed | 공격속도 |
| ArmorPenetration | 방어 관통 |
| ExtraAttackTargets | 다중 타깃 |
| MaxHealth | 최대 체력 |
| Armor | 방어력 |
| DamageReduction | 피해 감소 |
| HpRegen | 체력 재생 |
| MoveSpeed | 이동속도 |
| Range | 사거리 |
| Strength | 힘 |
| Agility | 빠름 |
| Wisdom | 슬기 |
| Spirit | 마음 |
| AllStats | 올스탯 |
| XpGain | 경험치 획득 |
| DustGain | 별가루 획득 |
| DropRate | 드랍률 |
| CompanionDuration | 동료 지속 |

### 3.2 라인 포맷
- ADD op: `"▸ {label} +{value}"` (예: "▸ 힘 +3")
- MUL op: `"▸ {label} +{value}%"` (예: "▸ 공격력 +12.5%")
- 음수 → `"-"` (감소 affix 가 있다면)
- 잠금: `"🔒 ▸ ..."`

### 3.3 색상
- 주옵션 (mainAffixes): **보라** (#A66BC9)
- 부옵션 (subAffixes): **청록** (#4FC3D7)
- 신화 (mythicId): **무지개 그라데이션 또는 골드**

## 4. 페이지 네비게이션

- 페이지당 **9 카드** (3×3)
- `totalPages = ceil(matches.Count / 9)`
- `[◀ 이전]` / `1 / 6` / `[다음 ▶]` 버튼
- 첫 페이지: 이전 버튼 disabled
- 마지막 페이지: 다음 버튼 disabled
- 페이지 변경 시: `_currentPage` 갱신 + 카드 재배치 (같은 9 GameObject 재사용)

## 5. 클래스 구조

### 5.1 InventorySelectionPanel.cs (갱신)
```
SerializeField:
  - _gridContainer: Transform (3×3 GridLayoutGroup)
  - _cardPrefab: GameObject (InventoryGridCard 템플릿)
  - _prevButton / _nextButton: Button
  - _pageText: Text ("1 / 6")
  - _detailView: InventoryDetailView
  - _equipButton / _unequipButton: Button
  - _closeButton: Button
  - _titleText: Text
  - _emptyText: Text
  - _templateLookup: EquipmentTemplate[]
  - _inventoryPanel: InventoryPanel

State:
  - _currentSlot: EquipSlot
  - _matches: List<(EquipmentInstance, EquipmentTemplate)>
  - _currentPage: int (0-based)
  - _selected: EquipmentInstance / EquipmentTemplate
  - _cards: List<InventoryGridCard> (9 재사용)

Methods:
  - Show(EquipSlot slot)
  - Refresh() — 매칭 collect + 정렬
  - DrawPage(int page) — _matches[page*9 .. page*9+9] 카드 채움
  - GoPage(int delta)
  - SelectCard(EquipmentInstance, EquipmentTemplate)
  - EquipSelected() / UnequipSelected()
  - Close()
```

### 5.2 InventoryGridCard.cs (신규)
```
SerializeField:
  - _icon: Image (260×260)
  - _nameText: Text (24pt)
  - _enhanceText: Text (20pt 골드)
  - _rarityText: Text (18pt)
  - _equippedBadge: GameObject (녹 라벨)
  - _highlightBg: Image (선택 강조 — 등급 색)
  - _border: Image (등급별 테두리)
  - _button: Button

API:
  - Bind(EquipmentInstance, EquipmentTemplate, isEquipped)
  - SetSelected(bool)
  - OnClicked: Action<EquipmentInstance, EquipmentTemplate>
```

### 5.3 InventoryDetailView.cs (신규)
```
SerializeField:
  - _icon: Image (256×256)
  - _nameText: Text (32pt)
  - _enhanceRarityText: Text (24pt — "+5 영웅")
  - _mainAffixContainer: Transform (VerticalLayoutGroup)
  - _subAffixContainer: Transform
  - _mainAffixRowPrefab: GameObject (Text row)
  - _subAffixRowPrefab: GameObject
  - _mythicLabel: Text (있을 때만)

API:
  - Show(EquipmentInstance, EquipmentTemplate)
  - Clear()
  - 라인 표시: ApplyAffixLine(line, container, prefab, color)
```

### 5.4 StatTargetLabel.cs (신규 정적 헬퍼)
```
public static class StatTargetLabel {
  public static string Korean(string targetName);
  public static string Format(AffixLine line);  // "▸ 공격력 +12.5%"
}
```

## 6. 색상 팔레트

| 영역 | 색 |
|---|---|
| 카드 BG | RGBA(0.1, 0.1, 0.15, 0.85) |
| 카드 등급 테두리 Common | 흰 |
| Magic | 청 (#67ADFF) |
| Rare | 녹 (#4DD968) |
| Epic | 보 (#B266F2) |
| Legendary | 골드 (#FFD700) |
| Mythic | 분홍 (#FF6BB5) |
| 주옵션 텍스트 | 보라 |
| 부옵션 텍스트 | 청록 |
| 강화 +N | 골드 |
| 장착중 라벨 | 녹 |
| 선택 highlight | 등급 색 alpha 0.3 |

## 7. 워크플로 (사용자 시점)

1. 다락방 → 인벤토리 → Weapon 슬롯 클릭
2. SelectionPanel v2 진입 — 페이지 1, 9 카드 표시
3. **카드 클릭** → 하단 Detail 패널 갱신 (옵션 6라인 표시)
4. **다른 페이지** → [◀] / [▶] 버튼
5. **장착** → [🟢 장착] 버튼 → isEquipped 토글 + Save
6. 장착중인 무기는 카드에 "▶ 장착중" 라벨 + 등급 골드 테두리
7. **닫기** → InventoryPanel 갱신 (Weapon 슬롯에 장착 무기 아이콘 표시)

## 8. 페이지 네비게이션 알고리즘

```csharp
public void GoPage(int delta) {
    int newPage = _currentPage + delta;
    int total = (_matches.Count + 8) / 9;  // ceil
    if (newPage < 0 || newPage >= total) return;
    _currentPage = newPage;
    DrawPage(newPage);
    UpdatePageText();
    UpdateNavButtons(total);
}

private void DrawPage(int page) {
    int start = page * 9;
    for (int i = 0; i < 9; i++) {
        int idx = start + i;
        if (idx < _matches.Count) {
            _cards[i].gameObject.SetActive(true);
            _cards[i].Bind(_matches[idx].Item1, _matches[idx].Item2,
                           _matches[idx].Item1.isEquipped);
        } else {
            _cards[i].gameObject.SetActive(false);
        }
    }
}
```

## 9. 변경 이력
- 2026-05-03: 초기 작성 (InventorySelectionPanel v2)

## 10. 다음 단계
1. 사이클 A — 신규 클래스 4종 (StatTargetLabel + GridCard + DetailView + Panel v2)
2. 사이클 B — Attic.unity 자동 wiring (그리드 + 9 카드 prefab + Detail + 페이징 버튼)
3. 사이클 C — Play 검증 (사용자 보고)
