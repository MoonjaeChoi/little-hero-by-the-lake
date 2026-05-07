# 5034 Sprint PRD — balance_integration_v1

> **PRD v1 (초안)** · 디자인: `docs/prd/5034_balance_integration_v1.md`
> 권장 채택: D1=A / D2=A / D3=A / D4=B / D5=B / D6=A / D7=A (Mediator 단계 사용자 결정)

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | BalanceConfig + ChapterData 권장 ⭐/+N 매트릭스 추가 (Data 레이어) | 1 | 30분 |
| 2 | StarforceMultiplierTable 곡선 검토 + 정합 (D2=A 유지 시 변경 X) | 1 | 15분 |
| 3 | WeaponClassBonus 강화 — D4=B 직업별 특수 스탯 곡선 | 1 | 30분 |
| 4 | CurveSimulator 확장 — 슬롯 + 무기 + affix 통합 시뮬 (D7=A) | 1 | 1h |
| 5 | EnhanceModalPanel + SlotStarforceView UI 권장 ⭐ 표시 | 1 | 30분 |
| 6 | NUnit 5 + Validator 룰 + changelog + CLAUDE.md | 1 | 30분 |
| **합계** | | **6** | **~3h 15m** |

---

## 2. Sprint 1 — BalanceConfig + ChapterData 권장 매트릭스 (1 commit, 30분)

### 2.1 BalanceConfig 신규 필드

```csharp
[Header("5034 — 챕터별 권장 진행 매트릭스 (35 챕터)")]
public int[] recommendedLevelByChapter = new int[35];     // 챕터 N → 권장 Lv
public int[] recommendedWeaponEnhanceByChapter = new int[35];  // 챕터 N → 권장 무기 +N
public int[] recommendedSlotStarforceByChapter = new int[35];  // 챕터 N → 권장 슬롯 ⭐
```

기본값 (디자인 §3.1 답습):
- `recommendedLevelByChapter`: { 1, 3, 5, 8, 11, ..., 50 } (챕터 N → N+9 대략)
- `recommendedWeaponEnhanceByChapter`: { 0, 1, 2, 3, 4, 5, ..., 25 } (챕터 N → 약 N×0.7)
- `recommendedSlotStarforceByChapter`: { 0, 0, 1, 2, 3, ..., 20 } (챕터 N → 약 N×0.6)

### 2.2 Editor 시드 메뉴

`Tools/LittleHero/Seed BalanceConfig Recommended Matrix` — 위 35×3 배열 일괄 채움.

### 2.3 검증
- BalanceConfig.asset 갱신
- 컴파일 errors=0

#### Sprint 1 commit
```
5034 Sprint 1 — BalanceConfig 권장 매트릭스 (1/6)
+ Data/BalanceConfig.cs 3 신규 필드 + Editor seed 메뉴
+ chapter 1~35 권장 Lv / +N / ⭐ 매트릭스
```

---

## 3. Sprint 2 — StarforceMultiplierTable 곡선 (1 commit, 15분)

D2=A 결정 시 변경 X. D2=B 또는 C 결정 시 _baseMulPercent 배열 갱신.

기존 (D2=A):
```csharp
{ 0f, 5f, 10f, 15f, 22f, 30f, 36f, 42f, 48f, 54f, 60f,
  68f, 76f, 84f, 92f, 100f, 120f, 140f, 160f, 180f, 200f }
```

D2=B (강화) 시:
```csharp
{ 0f, 7f, 14f, 22f, 30f, 40f, 50f, 60f, 72f, 85f, 100f,
  115f, 130f, 145f, 160f, 175f, 200f, 230f, 260f, 290f, 300f }
```

#### Sprint 2 commit
```
5034 Sprint 2 — StarforceMultiplierTable 곡선 (D2=A 유지 또는 갱신) (2/6)
```

---

## 4. Sprint 3 — WeaponClassBonus 직업별 특수 스탯 (1 commit, 30분)

D4=B 결정 시 PlayerStats 또는 EffectResolver 에 직업별 스탯 보너스 추가:

```csharp
// EffectResolver 또는 PlayerStats
public static void ApplyClassBonus(PlayerStats stats, CharacterClass cls) {
    switch (cls) {
        case CharacterClass.Warrior:
            stats.AddStrengthBonus(stats.Strength * 0.20f);  // STR +20%
            stats.AddBossDamageBonus(0.10f);                 // 보스 데미지 +10%
            break;
        case CharacterClass.Archer:
            stats.AddAgilityBonus(stats.Agility * 0.20f);    // AGI +20%
            stats.AddCritChanceBonus(0.10f);                 // 크리 +10%
            break;
        case CharacterClass.Mage:
            stats.AddWisdomBonus(stats.Wisdom * 0.20f);      // WIS +20%
            stats.AddAOEBonus(0.15f);                        // 광역 +15%
            break;
    }
}
```

> PlayerStats 신규 필드 (`bossDamageBonus`, `critChanceBonus`, `aoeBonus`) 추가 가능.

#### Sprint 3 commit
```
5034 Sprint 3 — WeaponClassBonus 직업별 특수 스탯 (D4=B) (3/6)
+ PlayerStats 3 신규 필드 + EffectResolver.ApplyClassBonus
```

---

## 5. Sprint 4 — CurveSimulator 확장 (1 commit, 1h)

D7=A 결정 시 — 기존 CurveSimulator 에 슬롯 + 무기 + affix 통합:

```csharp
public class CurveSimulator : EditorWindow {
    // ... 기존 챕터 N + 권장 Lv 매트릭스
    // 5034 신규
    private bool _includeSlotStarforce = true;
    private bool _includeWeaponEnhance = true;
    private bool _includeAffix = true;

    private void DrawDpsRow(int chapter, int lv) {
        var dps = CalculateDps(lv,
            slotMul: _includeSlotStarforce ? StarforceMultiplierTable.GetBaseMultiplierForLevel(_recommendedSlot[chapter-1]) : 0f,
            weaponMul: _includeWeaponEnhance ? GetWeaponEnhanceMul(_recommendedEnhance[chapter-1]) : 0f,
            affixMul: _includeAffix ? GetAvgAffixBonus(chapter) : 0f);
        var ttk = bossHp / dps;
        // ...
    }
}
```

#### Sprint 4 commit
```
5034 Sprint 4 — CurveSimulator 통합 시뮬 (슬롯 + 무기 + affix) (4/6)
```

---

## 6. Sprint 5 — UI 권장 ⭐ 표시 (1 commit, 30분)

`SlotStarforceView` 가 권장 ⭐ 단계 표시 (현재 챕터 기반):

```csharp
private void Refresh() {
    int level = StarforceService.GetLevel(_slot);
    int recommended = BalanceConfig.Instance.recommendedSlotStarforceByChapter[currentChapter - 1];
    _levelText.text = $"⭐ {level}";
    _recommendedText.text = level < recommended ? $"권장 ⭐ {recommended}" : "";
}
```

`StarforceEnhanceModal` 도 동일 표시 + 권장 미달 시 색상 강조.

#### Sprint 5 commit
```
5034 Sprint 5 — UI 권장 ⭐ 표시 (5/6)
```

---

## 7. Sprint 6 — NUnit + 검증 + docs (1 commit, 30분)

### 7.1 NUnit 5+
- `BalanceMatrixTests` — 35 챕터 권장 ⭐/+N 단조 증가
- `DpsCheckTests` — 챕터 N 권장 진행 시 DPS_player ≥ DPS_target × 0.7
- `WeaponClassBonusTests` — 직업별 특수 스탯 적용 검증
- `StarforceMultiplierTableTests` — ⭐0~⭐20 multiplier 단조 증가
- `CurveSimulatorTests` — 통합 시뮬 결과 정합

### 7.2 ProjectValidator 룰
- BalanceConfig 의 recommended* 배열 길이 35 검증

### 7.3 docs
- `changelog_v1.md` +5034
- `CLAUDE.md` §14 갱신 + §11 마일스톤

#### Sprint 6 commit
```
5034 Sprint 6 — NUnit 5 + Validator + changelog (6/6)
```

---

## 8. 의존성 그래프

```
Sprint 1 (BalanceConfig + ChapterData)
   ↓
Sprint 2 (StarforceMultiplierTable) — 병렬 가능 (Data 만)
   ↓
Sprint 3 (WeaponClassBonus) — 병렬 가능
   ↓
Sprint 4 (CurveSimulator) — Sprint 1+2+3 의존
   ↓
Sprint 5 (UI) — Sprint 1 의존
   ↓
Sprint 6 (NUnit + docs)
```

---

## 9. 완료 정의 (DoD)

- [ ] BalanceConfig.recommended* 3 배열 (길이 35) wiring
- [ ] StarforceMultiplierTable 곡선 (D2 결정 적용)
- [ ] PlayerStats 직업별 특수 스탯 필드 + EffectResolver 적용 (D4=B)
- [ ] CurveSimulator 통합 시뮬 작동
- [ ] SlotStarforceView 권장 ⭐ 표시
- [ ] NUnit 5+ PASS
- [ ] 컴파일 errors=0
- [ ] 사용자 Play Mode 검증 (시나리오 1 + 2 + 3)

---

## 10. 검증 시나리오

### 시나리오 1 — 챕터 10 권장 진행
- L20 / +8 / ⭐3
- DPS check: DPS_player ≥ DPS_target × 1.0
- 보스 TTK 30~40s

### 시나리오 2 — 챕터 20 도달 (튜토리얼 종료)
- L32 / +15 / ⭐10
- DPS check 통과 + 마법 결정 unlock

### 시나리오 3 — 챕터 35 클리어
- L50 / +25 / ⭐15~⭐20
- TTK 50~67s + 신화 발현

---

## 11. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-07 | 초기 — 6 Sprint / ~3h 15m / 12 NUnit |
