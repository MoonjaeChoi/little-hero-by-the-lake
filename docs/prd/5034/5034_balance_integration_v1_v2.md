# 5034 Sprint PRD — balance_integration_v1

> **PRD v2 (최종, Mediator 확정)**
> 원본: `5034_balance_integration_v1_prd.md` · Rebuttal: `5034_balance_integration_v1_rebuttal.md` · Decisions: `5034_balance_integration_v1_decisions.md`
> **결정**: D1=A, D2=A, D3=A, D4=B, D5=B, D6=A, D7=A, D8=B, D9=B, D10=B (권장 10/10 채택)
> 다음 단계: `/prd-breakdown` (Phase 3)

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | BalanceConfig + ChapterData 권장 ⭐/+N 매트릭스 (D1=A) | 1 | 30분 |
| 2 | StarforceMultiplierTable 곡선 (D2=A 유지 — 검증만) | 1 | 15분 |
| 3 | WeaponClassBonus 직업별 특수 스탯 (D4=B) + PlayerStats Add/Remove (D8=B) | 1 | 30분 |
| 4a | CurveSimulator 슬롯 통합 (D9=B 분리) | 1 | 30분 |
| 4b | CurveSimulator 무기 + affix 통합 (D9=B 분리) | 1 | 30분 |
| 5 | EnhanceModalPanel + SlotStarforceView UI 권장 ⭐ 표시 | 1 | 30분 |
| 6 | NUnit 5 + Validator 룰 + changelog + CLAUDE.md | 1 | 30분 |
| **합계** | | **7** | **~3h 15m** |

---

## 2. Sprint 1 — BalanceConfig + ChapterData (1 commit, 30분)

### 2.1 BalanceConfig 신규 필드 (D1=A 균형)

```csharp
[Header("5034 — 챕터별 권장 진행 매트릭스 (35 챕터)")]
public int[] recommendedLevelByChapter = new int[35];          // 챕터 N → N+9 (대략)
public int[] recommendedWeaponEnhanceByChapter = new int[35];  // 챕터 N → N×0.7 (대략)
public int[] recommendedSlotStarforceByChapter = new int[35];  // 챕터 N → N×0.6 (대략)
```

기본값 (D1=A 매트릭스):
- recommendedLevelByChapter: { 10, 11, 13, 15, 17, ..., 50 }
- recommendedWeaponEnhanceByChapter: { 0, 1, 3, 4, 5, 7, ..., 25 }
- recommendedSlotStarforceByChapter: { 0, 0, 1, 2, 3, 4, ..., 20 }

### 2.2 Editor 시드 메뉴
`Tools/LittleHero/Seed BalanceConfig Recommended Matrix` — 위 35×3 배열 일괄 채움 (D1=A 곡선).

### 2.3 검증
- BalanceConfig.asset 갱신
- 컴파일 errors=0

---

## 3. Sprint 2 — StarforceMultiplierTable 검증 (1 commit, 15분)

D2=A 유지 — _baseMulPercent 배열 변동 X. 검증만:
- 5033.4 답습 ⭐0=0% / ⭐15=100% / ⭐20=200%
- StarforceMultiplierTableTests NUnit 보존

---

## 4. Sprint 3 — WeaponClassBonus 직업별 특수 스탯 (1 commit, 30분, D4=B + D8=B)

### 4.1 PlayerStats 신규 필드 (D8=B Add/Remove 메서드)

```csharp
public class PlayerStats : MonoBehaviour {
    // 5034 Sprint 3 — 직업별 특수 스탯 (D4=B)
    private float _bossDamageBonus;
    private float _critChanceBonus;
    private float _aoeBonus;

    public float BossDamageBonus => _bossDamageBonus;
    public float CritChanceBonus => _critChanceBonus;
    public float AoeBonus => _aoeBonus;

    public void AddBossDamageBonus(float v) => _bossDamageBonus += v;
    public void RemoveBossDamageBonus(float v) => _bossDamageBonus -= v;
    public void AddCritChanceBonus(float v) => _critChanceBonus += v;
    public void RemoveCritChanceBonus(float v) => _critChanceBonus -= v;
    public void AddAoeBonus(float v) => _aoeBonus += v;
    public void RemoveAoeBonus(float v) => _aoeBonus -= v;
}
```

### 4.2 EffectResolver 또는 ApplyClassBonus (D4=B 메이플 정합)

```csharp
public static void ApplyClassBonus(PlayerStats stats, CharacterClass cls) {
    if (stats == null) return;
    switch (cls) {
        case CharacterClass.Warrior:
            stats.AddStrengthBonus(stats.Strength * 0.20f);
            stats.AddBossDamageBonus(0.10f);
            break;
        case CharacterClass.Archer:
            stats.AddAgilityBonus(stats.Agility * 0.20f);
            stats.AddCritChanceBonus(0.10f);
            break;
        case CharacterClass.Mage:
            stats.AddWisdomBonus(stats.Wisdom * 0.20f);
            stats.AddAoeBonus(0.15f);
            break;
    }
}
```

### 4.3 직업 변경 시 차감
- HandleClassChanged 시 RemoveBossDamage / RemoveCritChance / RemoveAoe 차감 후 새 직업 ApplyClassBonus.

---

## 5. Sprint 4a — CurveSimulator 슬롯 통합 (1 commit, 30분, D9=B 분리)

`Editor/CurveSimulator.cs` 확장:
- `_includeSlotStarforce` 토글 추가
- `CalculateDps` 가 BalanceConfig.recommendedSlotStarforceByChapter[N-1] 의 multiplier 곱셈 반영

---

## 6. Sprint 4b — CurveSimulator 무기 + affix 통합 (1 commit, 30분, D9=B 분리)

`Editor/CurveSimulator.cs` 확장:
- `_includeWeaponEnhance` + `_includeAffix` 토글
- `CalculateDps` 가 무기 +N (recommendedWeaponEnhance) + affix 평균 보너스 (티어별) 곱셈 반영

---

## 7. Sprint 5 — UI 권장 ⭐ 표시 (1 commit, 30분)

- `SlotStarforceView` 가 BalanceConfig.recommendedSlotStarforceByChapter 기반 권장 ⭐ 표시
- `StarforceEnhanceModal` 도 동일 + 권장 미달 시 색상 강조 (5033.0g UI 패턴 답습)

---

## 8. Sprint 6 — NUnit + 검증 + docs (1 commit, 30분)

### 8.1 NUnit 5+
- `BalanceMatrixTests` (3 신규 — recommended* 배열 길이 35 + 단조 증가)
- `DpsCheckTests` (D5=B 0.7× 검증, 챕터 1/10/20/35)
- `WeaponClassBonusTests` (D4=B 3 직업 ApplyClassBonus 검증)
- `StarforceMultiplierTableTests` (D2=A 보존, 단조 증가)
- `CurveSimulatorTests` (D7=A 통합 시뮬 정합)

### 8.2 ProjectValidator 룰
- BalanceConfig.recommended* 배열 길이 35 검증

### 8.3 docs
- `changelog_v1.md` +5034
- `CLAUDE.md` §14 갱신 + §11 마일스톤

---

## 9. 의존성 그래프

```
Sprint 1 (BalanceConfig) → Sprint 2 (Starforce 검증, 병렬) → Sprint 3 (WeaponClassBonus, 병렬)
                                ↓
                         Sprint 4a (Curve 슬롯)
                                ↓
                         Sprint 4b (Curve 무기+affix)
                                ↓
                         Sprint 5 (UI)
                                ↓
                         Sprint 6 (NUnit + docs)
```

---

## 10. 완료 정의 (DoD)

- [ ] BalanceConfig.recommended* 3 배열 (길이 35) wiring (D1=A)
- [ ] StarforceMultiplierTable 곡선 D2=A 유지 (변경 X)
- [ ] PlayerStats 3 신규 필드 + Add/Remove 메서드 (D8=B)
- [ ] EffectResolver.ApplyClassBonus 직업별 특수 스탯 (D4=B)
- [ ] CurveSimulator 통합 시뮬 (D7=A, Sprint 4a/4b)
- [ ] SlotStarforceView 권장 ⭐ 표시
- [ ] NUnit 5+ PASS
- [ ] 컴파일 errors=0
- [ ] 사용자 Play Mode 검증 (시나리오 1+2+3)

---

## 11. 검증 시나리오

### 시나리오 1 — 챕터 10 권장 진행
- L20 / +8 / ⭐3 (D1=A 매트릭스 정합)
- DPS_player ≥ DPS_target × 0.7 (D5=B)
- 보스 TTK 30~40s

### 시나리오 2 — 챕터 20 도달 (튜토리얼 종료)
- L32 / +15 / ⭐10
- DPS check 통과 + 마법 결정 unlock

### 시나리오 3 — 챕터 35 클리어 (엔드콘텐츠 진입)
- L50 / +25 / ⭐15~⭐20
- TTK 50~67s + 신화 발현

---

## 12. 후속 backlog (D10=B)

- ~~5034.1 V매트릭스 / 5차 스킬~~ — D10=B 폐기 (단일 캐릭터 게임 부적합)
- 5034.2 시즌 패스 통합 — 5026.x 시즌 backlog 와 통합 (보존)
- 5034.3 직업별 잠재 — 신화 4종 (5023) 정합 검토 (보존)

---

## 13. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-07 | v1 초기 — 6 Sprint / ~3h 15m |
| 2026-05-07 | **v2 최종 (Mediator)** — 권장 10/10 채택. Sprint 4 → 4a/4b 분리 (7 commit). D4=B 메이플 정합 + D8=B Add/Remove 메서드. |
