# 5027 oop_class_split — 디자인 문서

> **목적**: 200줄+ 클래스 18개 중 Top 2 production service 분리 — §15.6 100줄 룰 점진 정합
> **상태**: PRD (디자인 + Sprint)
> **시작**: 2026-05-06

## 0. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | 5026 evaluation 후 잔여 75 warnings 분석 — OOP 18 (200줄+ 파일) source 가 차지 |

## 1. 현재 시스템 분석

### 1.1 200줄+ 클래스 18개 분포

| 레이어 | 개수 | 파일 (Top) |
|---|---|---|
| Editor | 3 | CurveSimulator 970L / PhaseFContentSeed 212L / LimitedShopSeed 203L |
| Core (Service) | 5 | **GachaService 476L** / **AudioManager 454L** / LimitedShopService 303L / StarforceService 255L / StarlightGrantService 214L |
| Gameplay | 5 | EquipmentManager 326L / EnemyController 284L / PlayerStats 248L / ItemDropService 232L / ChapterRunner 220L |
| UI | 5 | EquipmentCardPanel 230L / InventorySelectionPanel 224L / GachaResultPanel 213L / GachaPanel 204L / EndgameMenuPanel 203L |

### 1.2 Top 2 책임 분석

#### GachaService 476L (Core)

7 책임 그룹:
1. Initialize / Reset (21L)
2. CanRoll1 / CanRoll10 (~30L)
3. CalcSummonLevel / GetNextLevelThreshold (~40L)
4. RollSingle / Roll10x (~120L 핵심)
5. GrantSpecificWeapon / GrantWeaponByRarity (~80L)
6. Tickets (GetTickets / Grant / Spend) (~30L)
7. GachaResult DTO class (~15L)

#### AudioManager 454L (Core)

3 책임 그룹:
1. SFX (Play, PlayInternal, type → clip cache) (~150L)
2. BGM (PlayBgm, StopBgm, CrossFade) (~150L)
3. ChapterBgm (정적 GetChapterBgm — 35 챕터 매핑) (~80L)
4. Singleton + Init (~70L)

### 1.3 위반 영향

- ProjectValidator OOP warnings: **18 / 75** (24%)
- §15.6 100줄 권장 위반 — 신규 책임 추가 시 가독성·유지보수 ↓
- 단위 테스트 작성 시 단일 책임 그룹 isolation 어려움

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | Top 5 production 모두 분리 (GachaService + AudioManager + EquipmentManager + LimitedShopService + StarforceService) | warnings 18 → ~13, 큰 진전 | 5 sprint × 1h = ~5h, 회귀 위험 ↑ |
| **B (★권장)** | Top 2 분리 — GachaService + AudioManager | 가장 무거운 2 처리, 답습 패턴 (5022/5023) | 후속 5027.1/.2 필요 |
| C | GachaService 만 | 1 sprint, 빠른 시연 | 효과 미미 (warnings 18 → 17) |
| D | UI Panel 분리 우선 (EquipmentCardPanel/InventorySelectionPanel) | UI MVC 정착 | service 분리 효과 우선 |

★ Claude 권장: B — 5022 (Validator 11 Rule) + 5023 (MetaProgression 6 service) 답습. Production service 우선, UI/Editor 후속.

## 3. 권장 설계 (옵션 B)

### 3.1 GachaService 분리 (5 service + 1 DTO)

| 신규 파일 | 책임 | 라인 |
|---|---|---|
| `Core/Gacha/GachaResult.cs` | DTO 분리 | ~20L |
| `Core/Gacha/GachaCanRollService.cs` | CanRoll1/CanRoll10 | ~40L |
| `Core/Gacha/GachaProgressService.cs` | CalcSummonLevel + GetNextLevelThreshold | ~45L |
| `Core/Gacha/GachaRollService.cs` | RollSingle / Roll10x / Grant* | ~180L (핵심) |
| `Core/Gacha/GachaTicketService.cs` | Get/Grant/Spend Tickets | ~35L |
| **`Core/GachaService.cs`** (얇은 facade 유지) | Initialize + Reset + 호출처 호환 | **~50L** |

핵심 — 호출처 13 파일 (UI Panel + Bootstrapper) 변경 최소화 위해 **GachaService facade 유지**. 내부 위임만.

### 3.2 AudioManager 분리 (3 component)

| 신규 파일 | 책임 | 라인 |
|---|---|---|
| `Core/Audio/AudioSfxController.cs` | SFX Play (type → clip cache) | ~120L |
| `Core/Audio/AudioBgmController.cs` | BGM PlayBgm / StopBgm / CrossFade | ~130L |
| `Core/Audio/AudioChapterBgmMap.cs` | 정적 GetChapterBgm (35 챕터) | ~80L |
| **`Core/AudioManager.cs`** (얇은 facade) | Singleton + Init + 위임 | **~80L** |

### 3.3 §15 정합

- §15.1 SRP: ✅ service 별 단일 책임
- §15.4 SO 우선: 영향 0 (기존 SO 유지)
- §15.6 100L: 모든 신규 클래스 100L 이내 (Roll 180L 만 권장 초과 — 가챠 핵심 로직)
- §15.7 DI: GachaService.Initialize 가 모든 service 에 전파
- §15.9 Test: 기존 NUnit 회귀 0 (facade 유지로 호출처 시그니처 동일)

## 4. Sprint 분해

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | GachaService 분리 (5 service + facade) | 1 | 60분 |
| 2 | AudioManager 분리 (3 component + facade) | 1 | 60분 |
| 3 | NUnit 회귀 검증 + Validator | 1 | 20분 |
| 4 | changelog + CLAUDE.md | 1 | 10분 |
| **합계** | | **4** | **2h 30m** |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 호출처 13 파일 시그니처 변경 → 회귀 | facade 유지 (GachaService / AudioManager 정적 메서드 시그니처 동일) |
| GachaService 정적 → service 정적 의존성 init order | GachaService.Initialize 시 모든 service Initialize 일괄 호출 |
| AudioManager Singleton + DontDestroyOnLoad → component 분리 회귀 | facade 가 component 인스턴스 유지 + 위임 |
| GachaRollService 180L (100L 초과) | 가챠 핵심 로직 — 추가 분리 시 응집도 ↓ (200L 룰 통과) |

## 6. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 옵션 B 권장 (Top 2, 4 sprint / 2h30m) |

## 7. Sources

- 5026 evaluation Validator C3 warnings 75 분석
- 5022 ProjectValidator 666L → 11 Rule 분리 패턴
- 5023 MetaProgression 346L → 6 service 분리 패턴
- §15.1 SRP + §15.6 100줄 룰
