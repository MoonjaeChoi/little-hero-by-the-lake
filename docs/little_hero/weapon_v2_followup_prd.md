# Weapon System v2 후속 PRD

> **출처**: `weapon_system_v2_prd.md` 12 commit 완료 후 알려진 후속 3건
> **목표**: 시각·시뮬·UX 잔여 작업 정리.

## 0. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| **A** | 신규 6 무기 icon 생성 (ComfyUI + Unity wiring) | 1 | 30분 |
| **B** | CurveSimulator 24 무기 가챠 분포 시뮬 | 1 | 30분 |
| **C** | AtticController 캐릭터 변경 시 무기 자동 unequip | 1 | 20분 |
| **총** | | **3** | **80분** |

## Sprint A — 신규 6 무기 icon (1 commit)

### A.1 ComfyUI batch + Unity wiring

**대상 무기**:
- `warrior_iron_sword` — 철검 (Sword)
- `warrior_steel_axe` — 강철 도끼 (Axe)
- `warrior_claymore` — 대검 (Sword)
- `warrior_giants_axe` — 거인 도끼 (Axe)
- `archer_eagle_bow` — 매눈 활 (Bow)
- `mage_glacier_rune` — 얼음 룬 (Tome)

**ComfyUI 설정** (Phase G2 P5 + Weapons batch 답습):
- 시드: 22222 (Seed 정책 정합)
- 톤: V5 sticker dynamic (이전 18 무기 동일)
- 모델: DreamShaperXL Turbo SFW
- IPAdapter: ref_weapons.png (weight 0.3 / end_at 0.3)
- 512×512, CFG 7, steps 20, euler_ancestral_karras

**Prompt 구조** (V5 base + 무기 form):
- iron_sword: "iron sword with leather grip, simple steel blade, classic warrior weapon"
- steel_axe: "steel axe with sharp blade, wooden handle, two-handed warrior weapon"
- claymore: "claymore great sword, large two-handed blade, ornate guard"
- giants_axe: "giant battle axe, massive double-edged blade, rune carvings on metal"
- eagle_bow: "elegant longbow with eagle feather string, archer weapon, golden tips"
- glacier_rune: "frozen ice rune tome, crystal pages, glowing blue runes, mage weapon"

**파이프라인**:
1. ComfyUI batch (서버 192.168.75.194, SSH 터널 8188)
2. isnet-anime segmentation (흰배경 alpha)
3. PNG 6 → `Assets/_Project/Art/Generated_v2/Weapons/<id>.png`
4. TextureImporter: Sprite Single + alphaIsTransparency true
5. EquipmentTemplate.icon 자동 wiring (SerializedObject API)

**검증**:
- 6 PNG 생성 + Unity Sprite 인식
- 6 EquipmentTemplate.icon != null
- 가챠 시 신규 무기 아이콘 표시

**완료 조건**:
- 24 무기 모두 icon wired
- InventoryGridCard 시각 검증

---

## Sprint B — CurveSimulator 24 무기 가챠 시뮬 (1 commit)

### B.1 직업별 가챠 시뮬 (1000회)

**파일**: `Editor/CurveSimulator.cs` (갱신 — 신규 메서드)

**메뉴**: `Tools > LittleHero > Weapon Gacha 24 Simulation`

**시뮬 항목**:
1. **각 직업 1000회 가챠** — 실제 GachaService.RollSingle 호출 (Mock)
2. **등급 분포 검증** — Lv 1~10 등급 분포 실제 결과 비교
3. **직업 필터 검증** — 본인 직업 외 결과 0건
4. **fallback 발생 빈도** — Warrior Magic 부재 → Common fallback 카운트

**시뮬 데이터 출력**:
```
=== Warrior 1000회 가챠 (Lv 5 분포) ===
결과 분포:
  Common 480 (목표 50%)
  Magic 250 (목표 25%)
  Rare 200 (목표 20%)
  Epic 70 (목표 5%)

직업 필터 — 모두 Warrior ✅

무기 분포:
  iron_sword 245 / impact_hammer 235 (Common)
  steel_axe 130 / claymore 120 (Magic)
  thunder_hammer 105 / katana 95 (Rare)
  giants_axe 35 / abyss_hammer 35 (Epic)
```

**simulation_report.md 갱신** (자동):
- §15 Weapon Gacha v2 검증 (신규)

**검증 기준**:
- 각 직업 1000회 모두 본인 직업 무기
- 등급 분포 ±5% 오차 내 일치
- 컴파일 에러 0

**완료 조건**:
- simulation_report.md 갱신
- 직업별 분포 표 출력

---

## Sprint C — AtticController 캐릭터 변경 시 무기 자동 unequip (1 commit)

### C.1 OnCharacterChanged hook + 무기 자동 해제

**파일**: `Scripts/Hub/AtticController.cs` + `Scripts/Meta/MetaProgression.cs` (갱신)

**흐름**:
```
사용자 → 다락방 캐릭터 선택 (예 전사 → 마법사)
   ↓
MetaProgression.SelectCharacter(newChar)
   ↓
신규: OnCharacterChanged event 발행 (CharacterClass 전달)
   ↓
EquipmentManager 가 구독 → 잘못된 직업 무기 자동 unequip
   ↓
SaveData 갱신 + EquipmentManager.UnequipInstance 호출
   ↓
AtticController 토스트: "전사 무기 N개 해제됨 (다른 직업 선택 시 사용 가능)"
```

**MetaProgression 신규**:
```csharp
public event System.Action<CharacterClass> OnCharacterClassChanged;

public void SelectCharacter(CharacterData ch) {
    var prevClass = ResolveClass(SelectedCharacter);
    var newClass = ResolveClass(ch);
    SelectedCharacter = ch;
    SaveSystem.Save(Data);
    OnCharacterChanged?.Invoke(ch);  // 기존
    if (prevClass != newClass) OnCharacterClassChanged?.Invoke(newClass);  // 신규
}
```

**EquipmentManager 신규 hook**:
```csharp
private void OnEnable() {
    if (MetaProgression.Instance != null)
        MetaProgression.Instance.OnCharacterClassChanged += HandleClassChanged;
}

private void HandleClassChanged(CharacterClass newClass) {
    var data = MetaProgression.Instance?.Data;
    if (data?.equipmentInstances == null) return;
    int unequipped = 0;
    foreach (var inst in data.equipmentInstances) {
        if (inst == null || !inst.isEquipped) continue;
        if (!_templateCache.TryGetValue(inst.templateId, out var tpl) || tpl == null) continue;
        if (tpl.category == WeaponCategory.None) continue;
        if (tpl.requiredClass != newClass) {
            inst.isEquipped = false;
            UnequipInstance(inst);
            unequipped++;
        }
    }
    if (unequipped > 0) {
        SaveSystem.Save(data);
        Debug.Log($"[EquipmentManager] 캐릭터 변경 — {unequipped}개 무기 자동 해제");
    }
}
```

**AtticController 토스트** (선택):
- 기존 `_selectedCharacterText` 활용 또는 신규 토스트 GameObject
- 1.5s 페이드 in/out — "전사 무기 N개 해제됨"

**검증 기준**:
- 전사 → 마법사 변경 시 전사 무기 모두 isEquipped=false
- SaveData 정확 갱신
- EquipmentManager.UnequipInstance 호출

**완료 조건**:
- Play 검증 — 캐릭터 변경 → 인벤토리 무기 슬롯 빈 상태

---

## 의존성 그래프

```
Sprint A (icon)        — 독립
Sprint B (시뮬)         — 독립
Sprint C (unequip)     — 독립

세 Sprint 모두 병렬 가능. 순서 자유.
```

권장 순서: **C → B → A** (UX 우선 → 검증 → 시각)

## 완료 정의

각 Sprint:
- ✅ 컴파일 에러 0
- ✅ 기능 검증 (Play 또는 Editor 메뉴)
- ✅ commit 메시지 형식
- ✅ changelog_v1.md 등재

전체:
- ✅ 24 무기 모두 icon wired
- ✅ simulation_report.md weapon_v2 §15 추가
- ✅ 캐릭터 변경 → 무기 자동 unequip 검증

## 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| ComfyUI 서버 응답 X | 사용자에게 보고 + 재시도 |
| 시뮬 1000회 × 3 직업 = 3000 GachaService 호출 | Mock SaveData 격리 (실제 SaveData 영향 X) |
| 캐릭터 변경 시 가챠 결과 변경 안내 누락 | AtticController 토스트로 명시 |

## 변경 이력
- 2026-05-03: 초기 작성 (3 Sprint 후속)

## 참조
- `weapon_system_v2_prd.md` — 본편 PRD (12 commit 완료)
- `weapon_balance_24_v1.md` — 신규 6 무기 명세
- `gacha_class_filter_v1.md` — 가챠 직업 필터
