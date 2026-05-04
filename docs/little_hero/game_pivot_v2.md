# Little Hero by the Lake — 게임 전개 시스템 PIVOT v2

> **2026-05-02 결정**: Brotato 식 (웨이브 사이 상점 + 런 내 빌드) → **Maplestory Idle 식** (랜덤 드롭 + 영구 인벤토리 + 메타 진행).
> 사용자가 시뮬 §13 retention 위험 + 한정 상점 + 가챠 시스템 도입 후 자연스러운 풀 idle 패턴 전환.

## 0. 결정 (2026-05-02 사용자 추천 안 채택)

| # | 항목 | 결정 |
|---|---|---|
| 1 | 별가루 ★ 정책 | **A 영구 단일** — 상점 페이즈 폐기 정합 |
| 2 | 상점 처리 | **archive** — 코드 보존, 비활성 |
| 3 | 드랍 빈도 | **공격 5%** — Enemy 5% / MiniBoss 30% / Boss 100% |
| 4 | 웨이브 사이 | **5초 휴식** — HP 회복 + 시각 break |
| 5 | V1 장비 16종 | **일반 드랍 풀** — 가챠 풀과 분리 (Common/Magic) |
| 6 | 디자인 문서 | **신규** — game_pivot_v2.md (본 문서) |

## 1. 변경 전·후 비교

| 항목 | Before (Brotato) | After (Idle) |
|---|---|---|
| **웨이브 사이** | 상점 페이즈 (장비 4 + 강화 2 선택) | 자동 5초 휴식 → 다음 웨이브 |
| **장비 획득** | 별가루 ★ 로 상점 구매 | 적·보스 랜덤 드롭 |
| **장비 영구성** | 챕터 클리어 시 리셋 | **모든 챕터 영구 보유** (인벤토리) |
| **별가루 ★** | 런 내 (사망/클리어 시 0) | **영구 자원** (메타 강화·변환·드랍 보너스) |
| **챕터 의미** | 독립 런 | **영구 진행 도구** (장비·재화·도감) |
| **빌드** | 매 챕터 새 빌드 | 누적 빌드 (가챠·드랍·강화) |

## 2. 데이터 모델 변경

### 2.1 별가루 ★ 영구화

**Before**: `GameManager._currentStarDust` (런 내 정수, 사망/클리어 시 0)
**After**: `MetaProgression.Data.starDust` (SaveData 영구 저장)

```csharp
// SaveData 신규 필드
public int starDust;  // 별가루 ★ (영구, PIVOT v2)

// CurrencyService — 정적 facade 통일
CurrencyService.GrantStarDust(int amount)   // ★ 적립
CurrencyService.SpendStarDust(int amount)   // ★ 차감
CurrencyService.StarDust => MetaProgression.Instance.Data.starDust;
```

**GameManager 변경**:
- `_currentStarDust` 필드 제거
- `AddStarDust(N)` → `MetaProgression.Instance.AddStarDust(N)` 위임
- 사망/클리어 hook 의 별가루 리셋 코드 제거

### 2.2 EquipmentInstance 영구화 (이미 B2 도입)

기존 `SaveData.equipmentInstances[]` 그대로 사용. 챕터 시작 시 인벤토리 로드 + 장착 상태 복원.

### 2.3 드랍 빈도

| 출처 | Before | After |
|---|---|---|
| Enemy | 0.5% | **5%** (10×) |
| MiniBoss | 8% | **30%** (3.75×) |
| Boss | 100% | 100% |
| BossRepeat | 30% | 30% |
| Shop | 100% | (Shop archive — 호출 X) |

## 3. 시스템 변경

### 3.1 상점 페이즈 폐기

**WaveManager 변경**:
```csharp
// Before
private void EndWave() {
    GameManager.OpenShop();  // 일시정지 + 상점 진입
}

// After
private IEnumerator EndWaveCoroutine() {
    GameEvents.RaiseWaveCleared(_currentWave);
    yield return new WaitForSeconds(5f);  // 5초 휴식 (HP 회복)
    StartNextWave();  // 자동 진행
}
```

**ShopController/ShopPanel**:
- 코드 archive (비활성). 다락방의 별빛 상점 / 가챠 / 슬롯 강화 / 변환 모루 / 마음 단련 → 다른 화면으로 분리됨.

### 3.2 5초 휴식 — HP 회복

웨이브 종료 시 `PlayerHealth.RestoreFull()` 또는 회복률 50% — 휴식 의미 부여.

### 3.3 EquipmentManager 의 attack pattern 활성화

**현재**: 장비 장착 시 `IAttackPattern` 활성. 가챠 무기·랜덤 드롭 모두 동일.
**변경 X**: 장비 영구화 후 인벤토리에서 사용자가 장착 → attack pattern 활성. 챕터 진입 시 자동 복원.

## 4. 챕터 진행 흐름

### 4.1 챕터 시작 (Before)
```
다락방 → 챕터 선택 → Chapter 씬 진입
PlayerStats 초기화 (베이스 스탯)
EquipmentManager 빈 상태
별가루 ★ = 0
웨이브 1 시작
```

### 4.2 챕터 시작 (After)
```
다락방 → 챕터 선택 → Chapter 씬 진입
PlayerStats 초기화 (베이스 + 메타 강화 + 장착 장비 보너스)
EquipmentManager — SaveData.equipmentInstances.isEquipped 자동 복원
별가루 ★ = SaveData.starDust 그대로 (영구)
웨이브 1 시작
```

### 4.3 웨이브 진행 (After)
```
웨이브 시작 → 적 spawn → 자동 공격 → 처치 시 별가루 + 장비 5% 드랍
적 모두 처치 → 웨이브 클리어 이벤트
5초 휴식 (HP 50% 회복) → 다음 웨이브
20 웨이브 후 보스 spawn → 보스 처치 → 챕터 클리어
```

### 4.4 챕터 클리어 (After)
```
보스 처치 → ChapterClearedPanel 표시
보상: 무지개 구슬 ✦ + 마법 결정 ◇ + 별빛 ⋆ + 별가루 ★ (영구)
인벤토리 보존 — 모든 드롭/가챠 장비 그대로
다락방 복귀 → 다음 챕터 진입
```

## 5. UI 변경

### 5.1 폐기/비활성
- `ShopPanel` — Chapter 씬 비활성
- `ShopSlotView` — 미사용

### 5.2 변경
- `HUDController` — 별가루 ★ 라벨 "런 내" → "영구"
- `ChapterClearedPanel` — 별가루 ★ 보상 표시 추가

### 5.3 강화 (기존 진입 강조)
- 다락방 `InventoryPanel` — 챕터 진입 전 장비 장착 권장
- `EquipmentCardPanel` — 강화·리롤 (이미 D5/D6 구현)

## 6. 스토리·텍스트 정합

### 6.1 CLAUDE.md §2 갱신 항목
- "Brotato식 (20웨이브 / 챕터, 웨이브 사이 상점)" → "Idle식 (20웨이브 / 챕터, 자동 진행 + 랜덤 드롭)"
- 진행 방식 라벨

### 6.2 narrative
- "아빠 동화책 한 권 = 챕터 = 독립 모험" → "아빠 동화책 = 캐릭터의 영구 성장 일지"
- 35 챕터 = 누적된 이야기 (메이플 키우기 패턴 정합)

## 7. 시뮬 영향 (별도 사이클)

| 시뮬 § | Before | After |
|---|---|---|
| §2 챕터 수익 | 별가루 1회성 (35회 누적 표시) | 별가루 영구 누적 (M 챕터 N회 클리어 시) |
| §6 화폐 밸런스 | 별가루 vs 메타 강화 vs 변환 모루 | 별가루 vs 메타 강화 vs 변환 모루 + 영구 누적 |
| §17 신규 — 인벤토리 누적 | — | 챕터 N 클리어 후 평균 장비 수 |

## 8. 위험·완화

| 위험 | 완화 |
|---|---|
| Brotato 매력 상실 (웨이브 사이 빌드 결정) | 가챠/마법의돌/리롤 의 메타 빌드 결정으로 보완 |
| 챕터 진행 동기 약화 (장비 영구) | 챕터 권장 레벨/스탯 게이팅 강화 (D12 RecommendedLevelWarningModal 활용) |
| 별가루 사용처 X | 메타 강화 가격 인플레이션 + 변환 모루 + 신규 메타 업그레이드 |
| 시뮬 §2~16 재구성 | 별도 사이클 (Phase 4) |

## 9. Phase 분해 (5 Phase, 7~12일)

| Phase | 작업 | 시간 |
|---|---|---|
| 1 | 코어 시스템 변경 (5 sub-task) | 3~5일 |
| 2 | UI 변경 (ShopPanel archive + HUD + Inventory 강조) | 1~2일 |
| 3 | 스토리·텍스트 정합 (CLAUDE.md §2 + 챕터 description) | 1일 |
| 4 | 시뮬·밸런스 재구성 | 2~3일 |
| 5 | 검수·commit | 0.5일 |

## 10. Phase 1 — 코어 시스템 변경 (본 사이클)

### 1.1 별가루 ★ 영구화
- SaveData.starDust 신규 필드
- CurrencyService.GrantStarDust/SpendStarDust 영구 분기
- GameManager 의 _currentStarDust 필드 제거 → MetaProgression 위임

### 1.2 상점 페이즈 폐기
- WaveManager 웨이브 종료 → 5초 휴식 코루틴
- GameManager.OpenShop / CloseShop 비활성 (메서드 보존, 호출 제거)
- ShopPanel SetActive(false) 강제 (Chapter 씬)

### 1.3 드랍 확률 상향
- ItemDropService.RollDropChance: Enemy 0.005→0.05, MiniBoss 0.08→0.30

### 1.4 V1 장비 16종 일반 드랍 풀
- EquipmentTemplate 16종 (V2 마이그레이션) — 가챠 풀 18종과 분리 추적
- ItemDropService 의 PickTemplate 에서 가챠 무기 풀 (EquipSlot.Weapon) 제외 (이미 C6 적용)

### 1.5 5초 휴식 + HP 회복
- WaveManager 코루틴: WaitForSeconds(5f) + PlayerHealth.RestoreFraction(0.5f)

## 11. 다음 단계

본 사이클 완료 후 Phase 2~5 단계별 진행. 각 Phase 사용자 검수 + commit.
