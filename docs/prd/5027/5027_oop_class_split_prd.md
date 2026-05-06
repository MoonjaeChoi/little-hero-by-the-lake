# 5027 Sprint PRD — oop_class_split

> **PRD v1 (초안)** · 디자인: `docs/prd/5027_oop_class_split_prd.md`
> 옵션 B 채택 — Top 2 production service 분리

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | GachaService 476L → 5 service + facade ~50L | 1 | 60분 |
| 2 | AudioManager 454L → 3 component + facade ~80L | 1 | 60분 |
| 3 | NUnit 회귀 + Validator | 1 | 20분 |
| 4 | changelog + CLAUDE.md | 1 | 10분 |
| **합계** | | **4** | **2h 30m** |

---

## 2. Sprint 1 — GachaService 분리 (1 commit)

### 2.1 신규 파일 5

**위치**: `Scripts/Core/Gacha/`

| 파일 | 라인 | 책임 |
|---|---|---|
| `GachaResult.cs` | ~20 | DTO (instance/templateId/starforceShards/isRarePlusGuaranteed) |
| `GachaCanRollService.cs` | ~40 | `CanRollSingle(pool, currency)` / `CanRoll10x(pool, currency)` |
| `GachaProgressService.cs` | ~45 | `CalcSummonLevel(pool, totalCount)` / `GetNextLevelThreshold(pool, currentLevel)` |
| `GachaRollService.cs` | ~180 | `RollSingle` / `Roll10x` / `GrantSpecificWeapon` / `GrantWeaponByRarity` (핵심 로직) |
| `GachaTicketService.cs` | ~35 | `GetTickets` / `GrantTickets` / `SpendTickets` |

### 2.2 GachaService facade (~50L)

기존 시그니처 13 메서드 모두 보존 — 내부에서 분리된 service 호출 위임.

```csharp
public static class GachaService {
    public static bool IsInitialized => _initialized;
    public static void Initialize(SlotAffixPool[] slotPools, GlobalSubAffixPool globalPool) {
        GachaRollService.Initialize(slotPools, globalPool);
        _initialized = true;
    }
    public static void Reset() { GachaRollService.Reset(); GachaTicketService.Reset(); _initialized = false; }
    public static bool CanRollSingle(GachaPool p, GachaCurrency c) => GachaCanRollService.CanRollSingle(p, c);
    public static GachaResult RollSingle(GachaPool p, GachaCurrency c, System.Random rng = null) => GachaRollService.RollSingle(p, c, rng);
    // ... 나머지 메서드 위임
}
```

### 2.3 검증 기준

- 컴파일 errors=0
- 호출처 13 파일 시그니처 변경 0 (회귀 X)
- 기존 NUnit (StarforceService 10 + WeaponSystem 13 등) 회귀 0

#### Sprint 1 commit

```
5027 Sprint 1 — GachaService 476L → 5 service + facade ~50L (1/4)

[변경]
  + Core/Gacha/GachaResult.cs (~20L) — DTO 분리
  + Core/Gacha/GachaCanRollService.cs (~40L) — CanRoll1/CanRoll10
  + Core/Gacha/GachaProgressService.cs (~45L) — CalcSummonLevel + GetNextLevelThreshold
  + Core/Gacha/GachaRollService.cs (~180L) — RollSingle/Roll10x/Grant* (핵심)
  + Core/Gacha/GachaTicketService.cs (~35L) — Tickets
  - Core/GachaService.cs 476L → ~50L facade (시그니처 동일, 위임)

[검증]
  - 호출처 13 파일 시그니처 변경 0 (회귀 X)
  - NUnit 회귀 0
  - §15.1 SRP 5 service 단일 책임

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 3. Sprint 2 — AudioManager 분리 (1 commit)

### 3.1 신규 파일 3

**위치**: `Scripts/Core/Audio/`

| 파일 | 라인 | 책임 |
|---|---|---|
| `AudioSfxController.cs` | ~120 | SFX Play (type → clip cache, 절차적 합성) |
| `AudioBgmController.cs` | ~130 | BGM PlayBgm / StopBgm / CrossFade (AudioSource 2개 fade) |
| `AudioChapterBgmMap.cs` | ~80 | 정적 GetChapterBgm (35 챕터 매핑 dictionary) |

### 3.2 AudioManager facade (~80L)

```csharp
public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }
    private AudioSfxController _sfx;
    private AudioBgmController _bgm;
    void Awake() { Instance = this; _sfx = new AudioSfxController(...); _bgm = new AudioBgmController(...); }
    public void Play(SfxType type) => _sfx.Play(type);
    public void PlayBgm(string name, float crossFadeSec = 1.5f) => _bgm.PlayBgm(name, crossFadeSec);
    public void StopBgm(float fadeSec = 0.5f) => _bgm.StopBgm(fadeSec);
    public static string GetChapterBgm(int chapterN) => AudioChapterBgmMap.Get(chapterN);
}
```

### 3.3 검증 기준

- AudioManager.Instance 호출처 (~25 파일) 시그니처 변경 0
- AudioManager.GetChapterBgm 정적 호출처 호환 (위임)

#### Sprint 2 commit

```
5027 Sprint 2 — AudioManager 454L → 3 component + facade ~80L (2/4)

[변경]
  + Core/Audio/AudioSfxController.cs (~120L) — SFX Play
  + Core/Audio/AudioBgmController.cs (~130L) — BGM CrossFade
  + Core/Audio/AudioChapterBgmMap.cs (~80L) — 35 챕터 정적 매핑
  - Core/AudioManager.cs 454L → ~80L facade

[검증]
  - AudioManager.Instance 호출처 (~25 파일) 시그니처 변경 0
  - 정적 GetChapterBgm 호환

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. Sprint 3 — NUnit 회귀 + Validator (1 commit)

### 4.1 NUnit 회귀 검증

기존 NUnit 158 PASS 검증 (코드 변경 0, 회귀 안전망).

신규 NUnit 0 (facade 패턴 — 시그니처 동일이라 추가 Test 불필요).

### 4.2 Validator 변동 확인

- OOPPolicyRule warnings 18 → 16 (-2: GachaService 476L + AudioManager 454L 200L 미만)
- 신규 5 service + 3 component 모두 200L 이내 ✅
- WeaponBossRule.WeaponSystem sentinel 1 변동 0

### 4.3 Sprint 3 commit

```
5027 Sprint 3 — NUnit 회귀 + Validator (3/4)

[검증]
  - 기존 NUnit 158 PASS (회귀 0)
  - OOPPolicyRule warnings 18 → 16 (-2)
  - 신규 8 파일 모두 200L 이내

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 5. Sprint 4 — changelog + CLAUDE.md (1 commit)

```
5027 Sprint 4 — changelog + CLAUDE.md §14 (4/4)

[변경]
  - changelog_v1.md +5027 항목
  - CLAUDE.md §14 직전 5건 갱신

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 6. 의존성 그래프

```
Sprint 1 (Gacha 분리) ─┐
                       ├─→ Sprint 3 (회귀 + Validator) → Sprint 4 (docs)
Sprint 2 (Audio 분리) ─┘
```

Sprint 1+2 병렬 가능 (서로 독립). 단 컴파일 안정성 위해 순차 권장.

## 7. 완료 정의 (DoD)

- [ ] GachaService 476L → ~50L facade
- [ ] 5 신규 Gacha service 모두 200L 이내
- [ ] AudioManager 454L → ~80L facade
- [ ] 3 신규 Audio component 모두 200L 이내
- [ ] 호출처 시그니처 변경 0
- [ ] NUnit 158 회귀 0
- [ ] Validator warnings 75 → 73 (-2)
- [ ] changelog + CLAUDE.md 갱신

## 8. 검증 시나리오 (Play 검증)

### 시나리오 1 — 가챠 회귀
Attic → 가챠 패널 → 단판/10연차 실행 → 결과 표시
**기대**: GachaService.RollSingle / Roll10x 동작 정상 (facade 위임)

### 시나리오 2 — 오디오 회귀
챕터 진입 → BGM 재생 / 적 처치 → SFX 재생 / 챕터 클리어 → BGM 페이드
**기대**: AudioManager.Play / PlayBgm 동작 정상 (facade 위임)

### 시나리오 3 — Validator 회귀
Tools/LittleHero/Validate Project → warnings 75 → 73 확인

## 9. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| facade 위임 시 정적 변수 분산 → 초기화 순서 의존 | GachaService.Initialize 가 모든 service Initialize 일괄 |
| AudioBgmController 가 AudioSource 2개 (cross-fade) → MonoBehaviour 의존 | facade 가 AudioSource 인스턴스 보유, controller 가 reference 받음 |
| GachaRollService 180L (100L 초과) | 가챠 핵심 로직 응집 우선, 200L 룰 통과 |
| 호출처 grep mismatch | 정적 시그니처 보존 + 컴파일 검증 |

## 10. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 옵션 B (Top 2, 4 sprint / 2h30m) |
