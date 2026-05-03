# Little Hero by the Lake

> 호숫가 초등학교에 다니는 한 어린이.
> 복층 아파트 다락방에서 아빠의 어린 시절 동화책을 발견한다.
> 책장을 넘기자, 그림 속으로 빨려들어갔다 —
> 아빠도 한때 이 책으로 모험을 떠난 작은 영웅이었다는 걸, 아직은 모른 채.

## 1. 프로젝트 개요

| 항목 | 값 |
|---|---|
| 게임명 | Little Hero by the Lake |
| 장르 | Auto-Shooter Idle |
| 영감 | Vampire Survivors / Maplestory Idle (작은 적 무리·메타) + 지브리 (정서) |
| 진행 | Idle식 (20웨이브 / 챕터, 자동 진행 + 영구 인벤토리) |
| 플랫폼 | iOS 1차 → Android, macOS |
| 엔진 | Unity 6.3 LTS, URP 2D |
| 언어 | C# (.NET Standard 2.1) |
| 톤 | 카주얼 ARPG **PG-12** (잔인·피·끔찍 ❌, 만화풍 폭발 OK) |
| 시장 | 한국 / 일본 / 미국 — 메이플 키우기·뱀파이어 서바이버 톤 |
| 수익화 | 별빛 ⋆ 캐시 + 시즌 패스 + 가챠 (신화 풀) |

## 2. 세계관 + 절대 원칙

### 핵심 컨셉
- **현실 허브**: 호숫가 마을 — 초등학교, 호수, 다락방
- **모험 무대**: 다락방 동화책 → 그림 속으로 빨려들어감
- **이중 의미 "Little Hero"**: 주인공 = 어린 시절의 아빠
- **챕터 35**: 동화책 풍 + 마지막 페이지 (히든) 진엔딩

### PG-12 톤 (2026-05-02 PIVOT)
- ❌ 금지: 피·시체·잔해·비명·사실적 폭력·공포
- ✅ 허용: 만화풍 폭발·강한 색·임팩트·약한 긴장·도전
- 별 흩어짐 / 만화풍 KO / 풍선 폭발 등 처치 표현 자유
- 일반 게임 어휘 OK (DPS·치명타·드랍률·천장)

### 접근성
- 한 손 플레이 (가상 조이스틱 좌하단)
- 터치 타겟 44pt (밀도 시 32pt 허용)
- 색맹 친화 (색 외 형태 구분)

## 3. 폴더 구조

```
Assets/_Project/
├── Scripts/
│   ├── Core/             # GameManager, EventBus, ObjectPool, SaveSystem, PlayerFinder, AudioManager
│   ├── Core/Events/      # GameEventChannel<T> + Listener (Ryan Hipple SO 패턴)
│   ├── Data/             # ScriptableObject 클래스 (40+)
│   ├── Editor/           # 시드, ProjectValidator (OOP 정책 검증), CurveSimulator
│   ├── Gameplay/
│   │   ├── Player/       # PlayerController, Stats, Health, Progress
│   │   ├── Equipment/    # EquipmentManager, IAttackPattern (8), AffixRoller, EnhancementService
│   │   ├── Enemies/      # EnemyController, EnemyAI, AnimationManager, BossAttacks (7)
│   │   ├── Waves/        # WaveManager, ChapterRunner, InfiniteTowerRunner, BossEncounterRunner
│   │   ├── Combat/       # DamageSystem, DamageQuery, DamageDealer, CinematicEffects, DamageNumberFloater
│   │   └── Items/        # ItemDropService, MemoryPickup, MemorySpawner
│   ├── Hub/              # AtticController
│   ├── Meta/             # MetaProgression
│   └── UI/               # 30+ Panel (HUD/Inventory/Gacha/Endgame 등)
├── Tests/Editor/         # NUnit 단위 테스트 (DamageDealer/Starforce/Enhancement)
├── Data/                 # SO 인스턴스 400+
├── Prefabs/  Art/  Audio/  Resources/
└── Scenes/               # Attic / Chapter / InfiniteTower / BossEncounter
```

## 4. 아키텍처

### 3계층 분리
1. **Data Layer** — ScriptableObject (모든 밸런스/콘텐츠)
2. **Logic Layer** — MonoBehaviour Manager + EventBus
3. **UI Layer** — uGUI Panel/HUD (이벤트 구독, 데이터 수정 X)

### 이벤트 시스템
- **GameEvents** (정적 EventBus) — 게임플레이 핵심 (OnEnemyKilled / OnLevelUp / OnEquipmentChanged 등 20+)
- **GameEventChannel\<T\>** (SO Event Channel, Ryan Hipple) — UI/디자이너 wiring
- **C# event delegate** — 컴포넌트 내부 (Health.OnDamaged 등)

### 데미지 흐름 (Damage System v2)
- 충돌 폐기 — `DamageQuery.FindNearest/FindEnemiesInCircle` (Physics2D.OverlapCircleNonAlloc + Layer "Enemy")
- `DamageDealer.ApplyMulti/ApplySingle` — 메이플 풍 다중 보정 (1차 100% / 2차 80% / 3차 70% / 4차+ 60% floor)
- `DamageSystem.DealDamage` — Crit roll + ArmorIgnore + Lifesteal + Mythic + EnemyController.OnHit (Premium 효과)

## 5. 코딩 규칙

- **네이밍**: PascalCase 클래스/메서드, `_camelCase` private, UPPER_SNAKE 상수
- **MonoBehaviour 100줄 룰**: 100줄 넘으면 책임 분리 (§15.6 ProjectValidator 자동 검증)
- **Update 사용 최소화**: 단일 Manager Update + List (예 EnemyAnimationManager)
- **Instantiate 직접 호출 금지**: ObjectPool<T> / VfxPool / DamageFloaterPool
- **GC 최소화**: 매 프레임 new ❌, 정적 buffer 사전 할당
- **Magic Number 금지**: 모든 수치는 SO 또는 const
- **public 필드 최소화**: SerializeField + private 우선 (SO 필드는 예외 허용)

자세한 규칙 → §15 OOP 정책

## 6. 핵심 시스템

### 능력치 (stats_v2 + v2.5)
- **L100 cap** — 캐릭터 레벨. chunk 곡선 (L1~50 fast / L51~100 slow)
- **3 코어** — 힘 / 민첩 / 지혜 (Spirit 폐기, HP→힘, XP→지혜 흡수)
- **24 파생 스탯** — 데미지 3 / 크리 4 / 방어 5 / 공격 4 / 회복 3 / 메타 5
- **3 직업** — 전사(STR 2.0×) / 궁수(AGI 2.0×) / 마법사(WIS 2.0×) — 모두 무료, 첫 시작 강제 선택

### 장비 (weapon_v2)
- **12 슬롯** — 주 7 (Hat/Top/Bottom/Cloak/Gloves/Weapon/Shoes) + 장신구 5 (Earring/Glasses/Necklace/Ring/Pet)
- **8 attack pattern** — Projectile / Aura / Orbit / Rain / Trail / Pulse / Summon / Slash (IAttackPattern, EquipmentPatternFactory)
- **24 무기 + 직업별 분류** — Warrior 8 / Archer 8 / Mage 8 (가챠 풀)
  - WeaponCategory 11종 (Sword/Axe/Mace/Spear / Bow/Crossbow/Dagger/Boomerang / Staff/Wand/Tome / None)
  - 직업+카테고리 시너지 (WeaponClassBonus): Warrior+검·둔기 +Dmg 10% / Archer+활·단검 +AS 5% / Mage+지팡이·완드 +MagicDmg 10%
  - 잘못된 직업 무기 — EquipmentManager.EquipInstance 거부 + UI 회색
  - 가챠 결과 — 본인 직업만 (PickFromRarityPool 자동 필터)
- **장비 강화** — +N 무한 (stats_v2.5 cap 폐기). +0~+15 결정론 / +16~+25 RNG / +25+ P_FLOOR 30% 동결, 비용 1.5^N
- **슬롯 ⭐ 스타포스** — ⭐0~⭐25 곡선 + ⭐25+ 무한 (+5%/단계), 비용 1.4^N
- **Affix** — 주옵션 3 + 부옵션 3 라인 (EffectResolver + ApplyResolvedBonus 통합)
- **신화** — 4 종 (마일리지 200, 추억 4종 발견 후 발현)

### 가챠 + 한정 상점
- **18 무기 가챠** — 별빛 ⋆ 100/단판, 900/10연차. Lv1~10 등급 분포 곡선. 10연차 Rare+ 보장
- **무기 인스턴스** — `EquipmentInstance` (instanceId/templateId/enhanceLevel/affix 6/mythicId)
- **한정 상점** — 12 핫딜 (메이플 키우기 패턴): 무료 4 + 캐시 8, 트리거 6종 (소환레벨/챕터클리어/신규/보스/캐릭터)

### 웨이브 (Idle식, PIVOT v4 Cinematic Combat)
- 1 챕터 = 20 웨이브, 5/10/15 미니보스, 20 보스
- **동시 적 cap 300** (PIVOT v4) — 스파이크 시 ApplyMulti + Critical 시 slow-mo + screen shake
- 웨이브 간 5초 휴식 (HP 50% 회복) → 자동 다음 웨이브
- **챕터당 ~4,000 마리** / 마리당 ★ ×0.5 (합산 보존)

### 메타 + 화폐 4종
- **별가루 ★** (영구) — 적 처치, 강화·변환 모루 소비
- **무지개 구슬 ✦** (영구) — 챕터 첫 클리어, 마음 단련 소비
- **마법 결정 ◇** (영구) — 보스 드랍, +16+ 강화 전용
- **별빛 ⋆** (영구·캐시) — 일일 로그인·챕터 첫·캐시. 가챠/강화 가속
- 변환 모루: ★ 1,000 → ◇ 1 / ✦ 5 → ◇ 1
- 도감: 처치 적 자동 등록 + 챕터별 100% 시 별빛 50⋆

### Endgame (CH 35 클리어 후)
- **일일 보스** 7종 (요일 회전), KST 0시 리셋
- **주간 보스** 4종 (4주 사이클), 신화 발현 보장
- **무한 다락방** — 깊이 무제한 (×1.05^d 누적)
- **고난도 챕터** — CH 36~50 (CH 21~35 ×2 곱셈)

### 추억 시스템 (스토리 코어)
- 챕터 진행 중 일정 확률 MemoryPickup 등장 (사진/메모/스티커/낙서 4종)
- 발견 → MetaProgression 영구 저장 + 다락방 일기장
- 모두 모으면 마지막 페이지 (히든 챕터) 언락

### Premium 피격 (PIVOT v4 Cinematic)
- punch scale 1.4× (크리 1.7×) + flash 빨강/노랑 + VfxPool.SpawnHitStar
- 데미지 수치 floating (`DamageNumberFloater`, 1.2K/15K/1.5M 자동 압축)
- Critical 5% baseline + slow-mo 0.08s + screen shake (CinematicEffects)

## 7. 성능 목표 (PIVOT v4)

- iPhone 12 이상 60fps 유지
- 동시 적 **300** cap (cinematic_combat_v1)
- GC Allocation/sec **<50KB**
- GPU draw call **<10/frame** (EnemyAtlas SpriteAtlas SRP Batcher)
- 메모리 **250MB** 이하
- 앱 용량 **150MB** 이하

## 8. 작업 우선순위

> 충돌 시 다음 순서로 결정:

1. **OOP 정책 §15 준수** (SOLID / GC / 네이밍)
2. **성능** (Pool, GC 최소화)
3. **PG-12 톤** (비폭력, 따뜻한 정서)
4. **UI/UX** (한 손 플레이, 큰 터치 타겟)
5. **코드 유지보수성** (SO + Event 분리)
6. 새 기능

## 9. 작업 요청 가이드

- ✅ 한 번에 한 시스템만
- ✅ 데이터 우선: SO 정의 → Manager → UI
- ✅ 새 클래스는 폴더 위치 명시
- ✅ EventBus 이벤트 재사용 우선
- ❌ 한 함수에 여러 책임
- ❌ MonoBehaviour 직접 인스턴스화

## 10. 외부 라이브러리

| 용도 | 라이브러리 | 라이선스 |
|---|---|---|
| 애니메이션 | DOTween | 커스텀 |
| 입력 | Unity Input System | Unity |
| 카메라 | Cinemachine 3.1.6 | Unity |
| UI | uGUI Legacy Text (TMP X — URP 셰이더 깨짐) | Unity |
| 사운드 | Unity Audio (절차적 SFX 합성) | Unity |
| IAP | com.unity.purchasing 5.0.0 | Unity |
| 테스트 | Unity Test Framework (NUnit) | Unity |

## 11. 마일스톤

| Phase | 핵심 산출물 | 상태 |
|---|---|---|
| 0 환경 | Unity + Xcode + iOS 빌드 | ✅ 2026-04-29 |
| 1 프로토타입 | 다락방 → 1챕터 3웨이브 | ✅ 2026-04-30 |
| 2 확장 | 1챕터 20웨이브 + 장비 12종 + 적 6종 | ✅ 2026-04-30 |
| 3 콘텐츠 | 챕터 3권 + 캐릭터 3 + 메타 + 추억 | ✅ 2026-04-30 |
| **PIVOT** | 키즈 → PG-12 카주얼 ARPG (가챠/스타포스/시즌 패스) | ✅ 2026-05-02 |
| **stats_v2** | 3 코어 + 무한 강화 + 메이플 키우기 능력치 | ✅ 2026-05-03 |
| **Damage v2** | 충돌 폐기 + DamageQuery + 다중 타깃 보정 | ✅ 2026-05-03 |
| **OOP §15** | 정책 + Editor Test + Validator | ✅ 2026-05-03 |
| **weapon_v2** | 직업별 분류 + 24 무기 균형 + 가챠 필터 | ✅ 2026-05-03 |
| **boss_system_v2** | 4 Tier + IBossPhase State + BossPatternFactory + BossLootTable 4 (PGE 6-Phase 첫 시연) | ✅ 2026-05-04 |
| **boss_scene_separation_v1** | 챕터 보스 별 씬 + Launcher DTO + BackgroundApplier (5002) | ✅ 2026-05-04 |
| **mini_boss_scene_separation_v1** | 미니보스 wave 15 별 씬 분리 (5003 옵션 B, BalanceConfig 토글) | ✅ 2026-05-04 |
| 4 출시 | TestFlight + App Store | 🔄 진행 중 |

## 12. 시스템 현황 (간략)

> 자세한 작업 이력은 → [docs/little_hero/changelog_v1.md](docs/little_hero/changelog_v1.md)

### 활성 시스템 (요약)
- **Core**: GameManager / MetaProgression / SaveSystem / ObjectPool / VfxPool / AudioManager / IAPService / SeasonPassService / CurrencyService / ConversionForge / EndgameResetService / StarlightGrantService / LimitedShopService / BossEncounterService / BossLootGrantService / GachaService / StarforceService / PlayerFinder / **BossEncounterLauncher (5002)** / **BackgroundApplier (5002)**
- **Data SO 클래스**: EquipmentTemplate / EquipmentInstance / AffixDefinition / SubAffixDefinition / SlotAffixPool / GlobalSubAffixPool / MythicDefinition / GachaPool / EnemyData / ChapterData / CharacterData / MemoryData / BalanceConfig / EndgameModeData / MetaUpgradeData / LimitedShopOffer / **BossLootTable** (5001)
- **Gameplay**: PlayerStats v2 (3 코어 + 24 파생) / PlayerController / PlayerProgress / PlayerHealth / EnemyController + AI + AnimationManager / WaveManager / ChapterRunner / InfiniteTowerRunner / BossEncounterRunner / EquipmentManager + PatternFactory / 8 IAttackPattern / 7 IBossAttackPattern + **BossPatternFactory** / **IBossPhase + BossPhase1Default + BossPhase2Aggressive + BossPhaseManager (5001 State 패턴)** / DamageSystem + Query + Dealer / EnhancementService + AffixRoller + EffectResolver + MythicMileageService / ItemDropService / CinematicEffects + DamageNumberFloater
- **UI**: HUDController / WaveTransitionToast / BossHealthBar / GameOverPanel / ChapterClearedPanel / AtticController + 14 진입 버튼 / InventoryPanel + InventoryModel + InventorySelectionPanel v2 (페이징 + 썸네일 + 옵션) + InventoryGridCard + InventoryDetailView / EquipmentCardPanel + Row + EnhanceModalPanel / WondrousMapPanel + Node + RecommendedLevelWarningModal / GachaPanel + ResultPanel + StarforceEnhanceModal + SlotStarforcePanel + View / EndgameMenuPanel / ConversionForgePanel / MemoryEffectOverlay / CharacterInfoPanel / LimitedShopPanel + ItemView / StarlightShopPanel
- **Tests/Editor**: 56 NUnit Test (EnhancementService 6 + DamageDealer 7 + StarforceService 10 + WeaponSystem 13 + BossSystem 10 + **BossSceneSeparation 10** (5002+5003))
- **씬**: Attic / Chapter / InfiniteTower / BossEncounter

### SO 인스턴스 (400+)
- 적 262 (35 챕터 고유 210 + V1 11 + 카테고리 archetype 30 + Daily/Weekly 보스 11)
- 챕터 50 (V2 35 + Hardcore 36~50 = 15)
- Equipment 16 + Weapon Template 18 = 34
- Affix 22 + SubAffix 12 = 34
- SlotAffixPool 12 + GlobalPool 1
- Mythic 4 / EndgameMode 4 / MetaUpgrade 3 / LimitedShop 12 / Memory 4
- **BossLootTable 4** (5001 — Mini/Chapter/Daily/Weekly)

### 아트 V5 sticker dynamic 톤 (PIVOT 후 통일)
- 캐릭터 portrait 3 + 적 sprite 262 + 보스 11 + 무기 icon 18 + 장비 icon 16 + 챕터 BG 35 + ground tile 35 + UI Attic 4 + 슬롯 placeholder 12 + 화폐 4
- 누적 V5 PNG ~401 + Mythic 4 + Memory 4 + Meta 3 = 412

## 13. 다음 작업 후보

### 우선순위 (최근 갱신)
1. 🟢 **OOP §15 적용 확장** — 100줄+ 클래스 분리 (CurveSimulator/GachaService/AudioManager/AtticController/MetaProgression 등 10개)
2. 🟢 **DI 점진** — 신규 클래스 Initialize 명시 주입 (§15 정책)
3. 🟢 **Editor Test 확장** — DamageQuery / EquipmentManager / Player 등
4. 🟡 **G3 한국어 검수** — UI 어휘 정합 (남은 영역)
5. 🟢 **G4 iOS 빌드 + TestFlight** — Apple Developer 인증 + 베타 배포
6. 🟢 **F1~F3 적 디자인 패스** — 30 archetype → 챕터별 고유 (디자인 작업)
7. ✅ **F4/F5 보스 씬 분리** — 5002 boss_scene_separation_v1 완료 (2026-05-04, 옵션 C)
8. ✅ **5003 미니보스 씬 분리** — 옵션 B 채택 (wave 15만 분리, BalanceConfig 토글) 완료 (2026-05-04, 5 commit)
9. 🟢 **5004 LoadSceneAsync + fade-in 통합 sprint** — 모든 씬 전환 일관 적용.
10. 🟢 **AtticController 책임 분리** — 415L → 200L 권장 (§15.6).

### 빠른 시작 명령
- `리틀히어로 구현 계속` → 자동 선정
- `구현 상태 점검` → Phase 진척 확인
- `<태스크 ID> 시작` → 직행

### 알려진 제약
- **Unity MCP 비정상 종료** (Mono SIGSEGV 가능) — 다중 execute_code + refresh_unity 연쇄 시 도메인 리로드 위험. 안전 패턴: 스크립트 편집 후 5~10초 대기 / 씬 저장 후 10초+ 대기 / 한 사이클 분리 commit

### Phase 3 vs 실제
§11 Phase 3 "🔄" 는 사실상 종료. **Phase 4 (출시)** 진입 단계.

## 14. 변경 이력

> **위치 변경 (2026-05-03)**: 변경 이력은 `docs/little_hero/changelog_v1.md` 로 분리.
> CLAUDE.md 는 핵심만 유지 (changelog-manager 스킬 운영).
> 신규 항목은 changelog_v1.md 에 추가.

### 최근 핵심 결정 (직전 5건)
- 2026-05-04: **5003 mini_boss_scene_separation_v1** — 미니보스 wave 15 분리 (옵션 B 재진입 채택, BalanceConfig 토글, 5 commit, NUnit 54→56). PGE 첫 RESERVED 해제 사례
- 2026-05-04: **5002 boss_scene_separation_v1** — 챕터 보스 BossEncounter 씬 분리 + Launcher DTO + BackgroundApplier (옵션 C, 10 commit, NUnit 46→54)
- 2026-05-04: **5001 boss_system_v2** — BossTier 4 + IBossPhase State + Factory + LootTable 4 (PGE 6-Phase 풀파이프라인 첫 시연, 10 commit, NUnit 36→46)
- 2026-05-04: **PGE 6-Phase 풀세트 도입** — 4 신규 스킬 (rebuttal/mediator/breakdown/contract) + pipeline/evaluate 개편 (Anthropic Planner-Generator-Evaluator)
- 2026-05-03: **weapon_v2** — 무기 OOP 재설계 (직업별 분류 + 24 균형 + 가챠 필터, 7 commit)

자세한 이력은 → [docs/little_hero/changelog_v1.md](docs/little_hero/changelog_v1.md)


## 15. OOP 개발 방법론 정책 (필수 준수)

> **출처**: `architecture_analysis_v1.md` (Unity OOP + Claude Code 평가) + 웹 조사 4 쿼리 (2026-05-03)
> **우선순위**: 본 정책은 §1~14 보다 **상위** — 신규 코드 작성·기존 코드 수정 시 본 정책 우선 적용
> **위반 발견 시**: 즉시 보고 + fix (또는 사용자 승인 후 후속 사이클)

### 15.1 SOLID 5 원칙 (Unity 적용)

| 원칙 | Unity 적용 |
|---|---|
| **S — 단일 책임** | 클래스당 1 책임. PlayerInput / Health / WeaponHandler 분리 (Player.cs 모놀리식 ❌) |
| **O — 개방-폐쇄** | switch 폐기, Strategy/Factory 활용 (예 IAttackPattern 8 구현체) |
| **L — 리스코프** | 인터페이스 구현체 모두 호환 (계약 위반 X) |
| **I — 인터페이스 분리** | 작은 interface (IAttackPattern + IBossAttackPattern 분리) |
| **D — 의존성 역전** | Initialize(stats, config) 명시 주입 / Singleton.Instance 직접 참조 최소 |

### 15.2 네이밍 컨벤션 (Unity 공식 + Little Hero)

| 종류 | 규칙 | 예시 |
|---|---|---|
| 클래스/인터페이스/메서드 | PascalCase | `PlayerStats`, `IAttackPattern`, `RecalculateStats()` |
| public 필드 (가능한 회피) | camelCase | `public float damage;` (SO 필드 한정 허용) |
| private/protected 필드 | `_camelCase` | `private float _attackDamage;` |
| 상수 | UPPER_SNAKE | `MAX_ENHANCE_LEVEL` |
| 정적 readonly | UPPER_SNAKE | `static readonly int[] _shardCost` |
| Boolean | 동사 접두사 | `isDead`, `hasAttacked`, `canEquip` |
| 약어 | 사용 X (수학 제외) | `playerHealth` ✅ / `pHp` ❌ |

**SerializeField 패턴**:
```csharp
[SerializeField] private float _attackDamage;  // ✅ private + SerializeField
public float AttackDamage => _attackDamage;     // ✅ readonly property
public float damage;                             // ❌ public 필드 회피
```

### 15.3 GC 최소화 패턴 (모바일 핵심)

| 패턴 | 적용 |
|---|---|
| **ObjectPool 필수** | Instantiate/Destroy in Update ❌. ObjectPool<T> + VfxPool + DamageFloaterPool 활용 |
| **String 회피** | `$"{x}"` in Update ❌. StringBuilder 또는 StringBuffer 캐시 |
| **LINQ 회피** | 성능 critical 영역 ❌. for/foreach 사용 |
| **Boxing 회피** | `int → object` ❌. 제네릭 List<T> 사용 |
| **Update() 최소화** | 1500+ 객체 매 프레임 callback ❌. 단일 Manager Update + List 일괄 처리 (예 EnemyAnimationManager) |
| **사전 buffer** | `new Collider2D[]` in loop ❌. 정적 buffer 64 (DamageQuery._buffer) |
| **Resources.Load 캐시** | 매 호출 로드 ❌. 1회 + Dictionary cache |

### 15.4 ScriptableObject 우선 (Data-Driven Design)

- **모든 밸런스/콘텐츠** → ScriptableObject (BalanceConfig / EnemyData / EquipmentTemplate / AffixDefinition)
- **마법 숫자 금지** — 코드 hardcode ❌, SO 필드 또는 const ✅
- **Designer 친화** — Inspector 편집 + Prefab 변형
- **메모리 효율** — 1회 로드 + 다수 컴포넌트 공유

### 15.5 이벤트 아키텍처

| 종류 | 사용처 |
|---|---|
| **정적 EventBus (GameEvents)** | 게임플레이 핵심 (OnEnemyKilled / OnLevelUp 등) — 빠른 라우팅 |
| **SO Event Channel** | UI/디자이너 wiring (VoidEventChannel + GameEventListener) — 코드 X |
| **C# event delegate** | 컴포넌트 내부 (Health.OnDamaged 등) — 결합도 낮음 |
| **MonoBehaviour 직접 참조** | ❌ 회피 (높은 결합도) |

### 15.6 Component 패턴 + 100줄 룰

**100줄 룰** (CLAUDE.md §5 기존):
- MonoBehaviour 100줄 넘으면 책임 분리
- 위반 시 — Model/View/Presenter 분리 또는 Component 추출

**Component 분리 예시** (모범 — EnemyController):
```
EnemyController     (Initialize + 외형 적용)     ~50 라인
EnemyAI             (이동 AI)                    ~60 라인
EnemyAnimationMgr  (단일 Update 매니저)         ~40 라인
```

### 15.7 Dependency Injection (Lightweight)

```csharp
// ❌ Singleton 직접 참조 (결합도 ↑)
public void Start() {
    var stats = GameManager.Instance.PlayerStats;
}

// ✅ Initialize 명시 주입 (테스트 가능)
public void Initialize(PlayerStats stats, BalanceConfig config) {
    _stats = stats;
    _config = config;
}

// ✅ Constructor 주입 (정적 service 권장)
public class GachaService {
    public static void Initialize(SlotAffixPool[] pools, GlobalSubAffixPool global) {...}
}
```

**Zenject IoC**: 도입 ❌ (모바일 GC 우려 + 학습 비용)

### 15.8 패턴 우선순위

| 패턴 | Little Hero 적용 |
|---|---|
| **Strategy** | ✅ IAttackPattern 8 / IBossAttackPattern 7 |
| **Object Pool** | ✅ ObjectPool<T> + VfxPool + DamageFloaterPool |
| **Service Locator (정적)** | ✅ CurrencyService / GachaService / DamageQuery |
| **Singleton** | △ GameManager / MetaProgression / AudioManager (DontDestroyOnLoad 만) |
| **Factory** | △ CreatePattern switch — 향후 SO Registry 전환 |
| **MVC/MVP** | △ InventoryModel 분리 시작 — 향후 다른 Panel 도 |
| **Observer (EventBus)** | ✅ GameEvents 정적 + SO Event Channel 인프라 |
| **State** | △ 게임 상태 enum 만 (PlayerState / GameState 미도입) |

### 15.9 단위 테스트 패턴 (향후 도입)

- Unity Test Framework (NUnit) + Editor Test
- ScriptableObject Mock — `ScriptableObject.CreateInstance<>()`
- 핵심 정적 service (DamageQuery / DamageDealer / EnhancementService) 우선 테스트
- MonoBehaviour 는 Logic 분리 (Component → Model 위임)
- **PIVOT v4 Cinematic Combat 시뮬** — `Editor/CurveSimulator.cs` 패턴 답습

### 15.10 Claude Code 워크플로 정책

1. **디자인 문서 → 코드 → 검증** 3단계 (한 commit = 한 의미 단위)
2. **CLAUDE.md** 핵심만 + changelog 분리 (changelog-manager 스킬)
3. **MCP 자동화** — 씬 wiring / SO 생성 / Reflection 검증
4. **단일 책임 commit** — 회수 용이
5. **사용자 결정 우선** — 옵션 X/Y/Z 채택 시 디자인 문서 + commit 반드시 명시

### 15.11 안티 패턴 (절대 금지)

| 안티 패턴 | 이유 |
|---|---|
| `public float damage;` (캡슐화 X) | SerializeField + private 사용 |
| `Player.cs` 500+ 라인 | 100줄 룰 + Component 분리 |
| `Update()` in 1500 객체 | 단일 Manager Update + List |
| `Instantiate()` in Update | ObjectPool 필수 |
| `new List<T>()` in Update | 정적 buffer 또는 사전 할당 |
| `string + string` in Update | StringBuilder + 캐시 |
| `LINQ.Where(...)` in critical | for/foreach 직접 |
| `GameObject.Find()` in Update | Awake 1회 캐시 |
| switch 9 분기 (Factory 미사용) | Strategy + SO Registry |
| Singleton 직접 참조 | Initialize 명시 주입 |

### 15.12 정책 검증 (Editor 자동화)

`Editor/ProjectValidator.cs` 룰 추가 권장:
- public 필드 검사 (SO 제외)
- 100줄+ 클래스 경고
- Update() 멀티플 검사 (단일 Manager 권장)
- Resources.Load in Update 검사
- string concat in Update 정적 분석

**현재 적용 상태** (Phase 1+2 완료 후, 2026-05-03):
- ✅ Object Pool (10+ 영역)
- ✅ ScriptableObject 우선 (400+ SO)
- ✅ Strategy 8+7 패턴
- ✅ Factory (EquipmentPatternFactory 분리)
- ✅ EventChannel 인프라 (GameEventChannel\<T\> + Listener)
- ✅ PlayerFinder 정적 cache (GameObject.Find 폐기)
- ✅ ProjectValidator OOP 룰 (200줄+ 자동 경고)
- ✅ Editor Test 23 NUnit (EnhancementService/DamageDealer/StarforceService)
- △ MVC/MVP (InventoryModel 시작 — 다른 Panel 후속)
- △ DI 명시 주입 (신규 클래스 §15 정책, 기존 Singleton 점진 전환)
- △ 100줄+ 클래스 10개 (CurveSimulator/GachaService 등 — 후속 Sprint)

## 16. 개발 명령어

### Unity 환경
- Unity **6000.4.4f1** (LTS), 프로젝트 루트: `LittleHero/` (저장소 루트가 아님)
- 패키지: URP / Cinemachine 3.1.6 / Test Framework / IAP 5.0.0
- 솔루션: `LittleHero/LittleHero.slnx` (Assembly-CSharp / Assembly-CSharp-Editor / LittleHero.Tests.Editor)

### Editor 메뉴 (Tools/LittleHero) — 콘텐츠 시드
SO 인스턴스 400+는 코드 시드로 생성. 신규 시작 / 재시드 시 사용:
- `Seed 35 Chapter SOs` — 챕터 35 + 적 262
- `Seed BalanceConfig Curves` — 곡선 재계산
- `Seed Affix SOs` / `Seed Pool SOs` / `Seed Mythic SOs` / `Seed Gacha Pool`
- `Seed Boss Loot Tables (4)` — 5001 boss_system_v2
- `Seed Limited Shop` / `Seed Endgame Modes` / `Seed Phase F Content`
- `Migrate Equipment Templates` / `Migrate Boss Tier (5001)` — 기존 SO 변환
- `Validate Project` — §15.12 OOP 정책 검사 (200줄+ / public 필드 / Update 멀티플)
- `Curve Simulator` / `Weapon Gacha 24 Simulation` — 밸런스 시뮬
- `Reset SaveData (디스크 save.json 삭제)` / `Show SaveData Path`
- `Reset Boss Cooldown` / `Set Endgame Menu Active (firstClearedChapters 35비트 ON)`

### 테스트 (NUnit 46)
- **Editor**: Window > General > Test Runner > **EditMode** 탭 → Run All
- **단일 테스트**: 트리 노드 선택 후 Run Selected
- **배치 모드 (전체)**:
  ```bash
  /Applications/Unity/Hub/Editor/6000.4.4f1/Unity.app/Contents/MacOS/Unity \
    -batchmode -projectPath "$(pwd)/LittleHero" \
    -runTests -testPlatform EditMode \
    -testResults "$(pwd)/LittleHero/test-results.xml" -quit
  ```
- **단일 클래스**: 위 명령에 `-testFilter "BossSystemTests"` 추가
- **단일 메서드**: `-testFilter "BossSystemTests.BossPhaseManager_Transitions_AtHpThreshold"`
- 테스트 어셈블리: `LittleHero.Tests.Editor`, 위치: `Assets/_Project/Tests/Editor/`
- 현재 5 클래스: BossSystemTests(10) / DamageDealerTests(7) / EnhancementServiceTests(6) / StarforceServiceTests(10) / WeaponSystemTests(13)

### iOS 빌드
- Editor: File > Build Settings > iOS > Build → Xcode workspace 생성
- Xcode: `xcodebuild -workspace LittleHero/Builds/iOS/Unity-iPhone.xcworkspace -scheme Unity-iPhone -configuration Release`
- TestFlight 배포는 Phase 4 진행 중 (§11)

### Unity MCP (주요 개발 도구)
이 저장소는 Claude Code + Unity MCP 로 개발됩니다:
- `mcpforunity://instances` — 활성 Unity 세션 확인
- `set_active_instance` — 다중 세션 시 라우팅 고정
- `manage_script` — 스크립트 편집 → 직후 `read_console` 로 컴파일 검증
- `editor_state.isCompiling` polling — 도메인 리로드 종료 확인
- **§13 알려진 제약 재확인**: 다중 `execute_code` + `refresh_unity` 연쇄 시 Mono SIGSEGV 위험. 안전 패턴 — 스크립트 편집 후 5~10초 대기, 씬 저장 후 10초+ 대기, 한 사이클 분리 commit.

### PGE 6-Phase 파이프라인 (시스템 단위 작업 워크플로)
`/prd-pipeline` 으로 시스템 디자인부터 검증까지 자동:
1. `prd-generator` → 디자인 PRD
2. `prd-critiquer` → `prd-rebuttal` → `prd-mediator` (3단 리뷰)
3. `prd-breakdown` → 구현 태스크 분해
4. `prd-contract` → C1~C6 검증 기준 사전 정의
5. 구현 (`little-hero-dev` 스킬)
6. `prd-evaluate` → Sprint Contract 검증, FAIL 시 5번으로 재진입 (max 2회)

5001 boss_system_v2 가 첫 풀 시연 사례 (`docs/prd/5001_*.md` 참조).

