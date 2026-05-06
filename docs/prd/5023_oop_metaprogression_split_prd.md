# 5023 — OOP MetaProgression Split

> **목적**: MetaProgression.cs 331L → 책임별 분리. 5021 init 보강 후 init 책임 일관성.
> **scope**: 코드 리팩토링만 — 동작 변경 0 (게임플레이 영향 0).
> **의존성**: 5021 (ResolveSelectedCharacterFromSave hook 정합).

---

## 1. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | 5020 evaluation 시 MetaProgression init 책임 한계 발견 + §15.1 SRP 위반 (다중 책임) → 5023 분리 |

---

## 2. 현재 시스템 분석

### 2.1 MetaProgression.cs 구조 (331L)

**책임 다중**:
| 책임 | 라인 (~) |
|---|---|
| Singleton + Awake bootstrap | 50 |
| BootstrapItemDropService (Editor only) | 40 |
| SelectCharacter / OnCharacterChanged | 25 |
| SelectChapter / activeChapterId | 15 |
| Currency operations (StarFragment / StarDust / Magic Crystal / Starlight) | 60 |
| Equipment instances (unlock / lock / mythic mileage) | 50 |
| Endgame (lastDailyBoss / lastWeeklyBoss / infiniteDepth / hardcore) | 30 |
| Save trigger 분산 | 20 (각 작업 끝에 SaveSystem.Save 산재) |
| ResolveCharacterClass + 기타 helper | 41 |

**§15.1 SRP 위반**: 7+ 책임 단일 클래스.

### 2.2 영향 범위

- 5021 작업 후 Awake 가 추가 보강 → 더 비대 (~345L 예상)
- 신규 화폐/시스템 추가 시 MetaProgression 이 점점 더 비대
- 단위 테스트 어려움 (전체 Singleton mock 필요)

---

## 3. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | partial class 분리 | 단순, namespace 동일 | §15.6 효과 작음 (컴파일 단위 동일) |
| B (★권장) | 책임별 정적 service + MetaProgression orchestrator | 명확 SRP, 각 service 독립 테스트 | 파일 수 +6, MetaProgression.Instance 패턴 변동 |
| C | DI 기반 IMetaProgression 인터페이스 + service 합성 | OOP 모범 | 과도한 추상화 |

**권장 옵션 B** — Service Locator 패턴 (CLAUDE.md §15.8). 5005 attic_decomposition 답습.

---

## 4. 권장 설계

### 4.1 분리 구조

```
Meta/
├── MetaProgression.cs                  (Singleton + Awake + Data prop, ~80L)
├── MetaProgressionLoader.cs            (Load + Migration apply + ResolveSelectedCharacter, ~50L)
├── MetaProgressionBootstrapper.cs      (IAP + ItemDrop + StarlightGrant, ~70L)
├── MetaProgressionCharacter.cs         (SelectCharacter + OnCharacterChanged + ResolveCharacterClass, ~50L)
├── MetaProgressionChapter.cs           (SelectChapter + activeChapterId, ~30L)
├── MetaProgressionCurrency.cs          (StarFragment + StarDust + MagicCrystal + Starlight, ~80L)
├── MetaProgressionEquipment.cs         (UnlockEquipment + LockEquipment + MythicMileage, ~70L)
├── MetaProgressionEndgame.cs           (DailyBoss + WeeklyBoss + InfiniteDepth + Hardcore, ~50L)
```

각 service 정적 클래스 (Singleton instance 의 Data 참조):

```csharp
public static class MetaProgressionCurrency
{
    public static void AddStarFragment(int amount)
    {
        if (MetaProgression.Instance == null) return;
        MetaProgression.Instance.Data.starFragments += amount;
        SaveSystem.Save(MetaProgression.Instance.Data);
        GameEvents.RaiseStarFragmentChanged(MetaProgression.Instance.Data.starFragments);
    }
    // ...
}
```

### 4.2 MetaProgression.Instance 호출 패턴 보존

기존 코드:
```csharp
MetaProgression.Instance.AddStarFragment(10);
```

→ 유지 (deprecated proxy):
```csharp
public class MetaProgression : MonoBehaviour
{
    [System.Obsolete("Use MetaProgressionCurrency.AddStarFragment")]
    public void AddStarFragment(int amount) => MetaProgressionCurrency.AddStarFragment(amount);
}
```

→ 또는 즉시 마이그레이션 (호출처 일괄 수정 — 약 30 호출처).

### 4.3 CLAUDE.md §15 정합

| 정책 | 검증 |
|---|---|
| §15.1 SRP | ✅ 각 service 단일 책임 |
| §15.6 100줄 | ✅ 모든 service 100L 미만 |
| §15.7 DI | △ Singleton 잔존 (Currency 등은 Initialize 명시 X) — 점진 |
| §15.8 Service Locator | ✅ 정적 service 패턴 |
| §15.9 Test | ✅ 각 service 단위 테스트 가능 |

---

## 5. 마이그레이션 단계

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Loader + Bootstrapper 분리 (5021 ResolveSelectedCharacter 수용) | 1 | 30분 |
| 2 | Character + Chapter service 분리 + 호출처 일괄 마이그레이션 | 1 | 30분 |
| 3 | Currency + Equipment service 분리 + 호출처 마이그레이션 | 1 | 45분 |
| 4 | Endgame service 분리 + NUnit 회귀 + 결과 비교 | 1 | 30분 |
| **합계** | | **4** | **2h15m** |

---

## 6. 알려진 위험

| 위험 | 완화 |
|---|---|
| 호출처 30+ 마이그레이션 누락 → 컴파일 에러 | grep 후 일괄 sed 또는 IDE refactor (rename), Sprint 별 분리 |
| Singleton race (Awake 순서) | MetaProgression Awake 가 모든 service init 보장 |
| Save 트리거 산재 → 누락 시 SaveData 미보존 | service 별 Save 명시 (변경 후 SaveSystem.Save 호출) |
| OnCharacterChanged event 구독자 (LimitedShopService 등) 동작 보장 | event delegate 그대로 보존, 이동 후 hook 검증 NUnit |

---

## 7. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-generator) | 초기 — 5020 init 한계 + 7+ 책임 발견 → 5023 분리, 5021 의존 |
