# Little Hero — 구현 작업 계획 v1

> [← Little Hero 인덱스](./README.md)
>
> 10 설계 문서 + 시뮬레이터 완성 후 **구현 단계** 작업 단위·의존성·MVP 범위·일정 추정.

---

## 0. 현재 상태 (2026-04-30)

### 완료
- Phase 0/1/2/3 부분 완료 (CLAUDE.md §11~12)
  - 코어 골격: GameEvents, GameManager, ObjectPool, SaveSystem
  - 데이터 SO: 16 장비, 11 적, 3 챕터, 3 캐릭터, 4 추억, 3 메타 업그레이드
  - 게임플레이: Player + Enemy + Wave + 7 공격 패턴 + Combat
  - UI: HUD, Shop, Attic, GameOver/ChapterCleared, Diary, Bestiary, MetaUpgrade
- 10 설계 문서 (equipment / stats / affix_pool / item_drop / character_progression / chapter_curve / economy / endgame / monetization / ui)
- CurveSimulator (Editor 메뉴) + 자동 검증 리포트

### 미구현 (본 계획서가 다룸)
- L100 piecewise 레벨 곡선 + 4 코어 스탯 자동 증가
- 5층 아이템 아키텍처 (강화·옵션·신화)
- 6줄 affix (주옵션 3 + 부옵션 3) + 드랍 트리
- 4 화폐 (마법 결정 + 별빛 신규)
- 변환 모루 + 별빛 상점 + Unity IAP
- Endgame 5 모드
- 카드 6단 + 인벤토리 + 신기한 지도
- 챕터 4~35 + 콘텐츠

---

## 1. Phase 구분

| Phase | 목적 | 산출물 | MVP? |
|---|---|---|---|
| **A** | Foundation 확장 | PlayerStats·PlayerProgress·DamageCalc·CurrencyService | ✅ MVP |
| **B** | 데이터 마이그레이션 | EquipmentTemplate·Affix SO 35종·ChapterData 35개·BalanceConfig | ✅ MVP |
| **C** | 핵심 시스템 | AffixRoller·ItemDropService·EnhancementService·EffectResolver | ✅ MVP |
| **D** | UI | HUD·Attic·인벤토리·카드 6단·강화·변환 모루 | ✅ MVP (일부) |
| **E** | 과금 | 별빛·Unity IAP·시즌 패스 | Phase 2 |
| **F** | 콘텐츠 | 챕터 11~35·일일 보스·주간·무한·고난도 | Phase 2~3 |
| **G** | 출시 | 사운드·아트·한국어·iOS 빌드·심사 | 출시 직전 |

---

## 2. Phase A — Foundation (코어 시스템 확장)

> 모든 후속 작업의 기반. 데이터 모델 + 서비스 클래스. 의존성 적음.

| ID | 작업 | 참조 | 의존 | 복잡도 |
|---|---|---|---|---|
| **A1** | `PlayerStats` 확장 — 7 신규 필드 + 4 코어 + L100 통합 | stats_v1 §6 | — | 중 | ✅ 2026-04-30 |
| **A2** | `PlayerProgress` 신규 — XP 적립 + L100 piecewise + Save | character_progression §7 | A1 | 중 | ✅ 2026-05-01 |
| **A3** | `DamageCalculator` 신규 — 3 버킷 공식 + 방어 관통 | stats_v1 §6 | A1 | 간단 | ✅ 2026-05-01 |
| **A4** | `EnemyController` 챕터 곱셈 — 1.10^(N-1) 자동 적용 | chapter_curve §9.1 | — | 간단 | ✅ 2026-05-01 |
| **A5** | `CurrencyService` 정적 — 4 화폐 grant·calc | economy §9.3, monetization §7.3 | A6 | 간단 | ✅ 2026-04-30 |
| **A6** | `SaveData` 확장 — magicCrystals·starlight·level·xp·affix 등 | economy §9.2 + character_progression §7.4 + monetization §7.1 | — | 중 | ✅ 2026-04-30 (affix 관련은 B2와 함께) |
| **A7** | `GameEvents` 신규 이벤트 — OnLevelUp(int)·OnStarlightChanged·OnMagicCrystalChanged·OnAffixRolled | (각 시스템) | — | 간단 | ✅ 2026-04-30 (OnAffixRolled는 B2와 함께) |

**총 7 작업. 예상 4~5일.**

---

## 3. Phase B — 데이터 (SO 마이그레이션·생성)

> SO 인스턴스 일괄 생성 + 기존 데이터 재구조화. 에디터 스크립트 활용.

| ID | 작업 | 참조 | 의존 | 복잡도 |
|---|---|---|---|---|
| **B1** | `EquipmentTemplate` SO 클래스 정의 + 16종 마이그레이션 (기존 EquipmentData → Template) | equipment §4.1 + §4.4 | — | 중 | ✅ 2026-05-01 |
| **B2** | `EquipmentInstance` 직렬화 — 6줄 affix (주 3 + 부 3) | equipment §4.2 + item_drop §5.1 | B1 | 중 | ✅ 2026-05-01 (SaveData affix 필드 함께 도입) |
| **B3** | `AffixDefinition` SO 23종 일괄 생성 (Editor 스크립트) | affix_pool §2 | A1 | 중 | ✅ 2026-05-01 (실제 unique 22, 문서 오류) |
| **B4** | `SubAffixDefinition` SO 12종 일괄 생성 | item_drop §3.1 | A1 | 중 | ✅ 2026-05-01 |
| **B5** | `SlotAffixPool` SO 12종 (슬롯별 1차/2차/3차 매핑) | affix_pool §3 | B3 | 간단 | ✅ 2026-05-01 |
| **B6** | `GlobalSubAffixPool` SO 1종 (글로벌 부옵션 풀) | item_drop §3.2 | B4 | 간단 | ✅ 2026-05-01 |
| **B7** | `ChapterData` 35개 SO 일괄 생성 (`enemyStatMultiplier` 미리 계산) | chapter_curve §9.2 | — | 중 | ✅ 2026-05-01 |
| **B8** | `EndgameModeData` SO 4종 (daily/weekly/infinite/hardcore) | endgame §8.1 | — | 간단 | ✅ 2026-05-01 |
| **B9** | `BalanceConfig` 확장 — enhanceCostByLevel·rerollCost 등 | economy §9.1 | — | 간단 | ✅ 2026-05-01 (AnimationCurve는 B7와 함께) |
| **B10** | `MythicDefinition` SO 4종 (아빠의 마법) | equipment §3.7 | — | 간단 | ✅ 2026-05-01 |

**총 10 작업. 예상 5~7일 (대부분 에디터 스크립트로 일괄 생성).**

---

## 4. Phase C — 핵심 시스템 (게임플레이)

> 데이터 위에서 동작하는 비즈니스 로직.

| ID | 작업 | 참조 | 의존 | 복잡도 |
|---|---|---|---|---|
| **C1** | `AffixRoller` 정적 — 슬롯 풀 + 글로벌 풀 굴림 | affix_pool §7.3 + item_drop §5.4 | B3, B4, B5, B6 | 중 | ✅ 2026-05-01 |
| **C2** | `ItemDropService` — 4 드랍원 + 슬롯·등급·6줄 결정 트리 | item_drop §4 + §5.3 | C1, B1 | 복잡 | ✅ 2026-05-01 |
| **C3** | `EnhancementService` — +0~+25 결정론·천장·실패 로직 | equipment §3.3 + §3.4 | A5, A7, B9 | 복잡 |
| **C4** | `EffectResolver` — 캐릭터 + 인스턴스 + 신화 통합 stats 계산 | equipment §4.3 | A1, B1, B2 | 중 |
| **C5** | `ConversionForge` 정적 — 별가루·구슬 → 마법 결정 변환 | economy §11.4 | A5 | 간단 | ✅ 2026-05-01 |
| **C6** | `EndgameResetService` 정적 — 일일/주간 가용성 (UTC+9 기준) | endgame §8.3 | A6 | 간단 |
| **C7** | `MythicMileageService` — 마일리지 누적 + 발현 시도 | equipment §3.7 + monetization §4.4 | C3 | 중 |
| **C8** | `IAttackPattern` 갱신 — `ExtraAttackTargets`·`AttackSpeed` 통합 | stats_v1 §6 + ui_v1 §9 | A1 | 중 |
| **C9** | `EnemySpawner` 챕터 곱셈 통합 | chapter_curve §9.1 | A4, B7 | 간단 |

**총 9 작업. 예상 8~10일.**

---

## 5. Phase D — UI (모바일)

> 사용자 노출. 일부 MVP, 일부 Phase 2.

### MVP 필수

| ID | 작업 | 참조 | 의존 | 복잡도 |
|---|---|---|---|---|
| **D1** | `HUDController` 갱신 — XP 바·체력·별가루 + 레벨업 0.8s 연출 | ui_v1 §4 | A2, A7 | 중 |
| **D2** | `AtticController` 갱신 — 4 화폐 + endgame 메뉴 (placeholder) | ui_v1 §6 | A5, A7 | 중 |
| **D3** | `WondrousMapPanel` 신규 — 35 챕터 선형 지도 + 진입 모달 | ui_v1 §7 | B7 | 복잡 |
| **D4** | `InventoryPanel` 신규 — 12 슬롯 그리드 + 캐릭터 풀바디 + 잠금 | ui_v1 §8 | B1, B2 | 복잡 |
| **D5** | `EquipmentCardPanel` 신규 — 6단 풀스크린 (정체성·스탯·강화·주·부·신화) | ui_v1 §9 | B1, B2, C1, C3, C4 | 복잡 |
| **D6** | `EnhanceModalPanel` 신규 — +0~+25 + 마법 결정 + 천장 게이지 | ui_v1 §9.3 + economy §10.3 | C3, A5 | 중 |
| **D7** | `ConversionForgePanel` 신규 — 변환 모루 UI | economy §11.3 | C5 | 간단 |
| **D8** | `CharacterInfoPanel` 신규 — 4 코어 + 파생 스탯 풀스크린 | ui_v1 §11.2 | A1, A2 | 중 |

### Phase 2

| ID | 작업 | 참조 | 의존 |
|---|---|---|---|
| **D9** | `StarlightShopPanel` — 별빛 상점 (마법 결정 직구매·강화 보장 등) | monetization §8 | E1 |
| **D10** | `EndgameMenuPanel` 풀 — 일일/주간/무한/고난도 진입 | endgame §9 | C6, B8 |
| **D11** | `MythicEffectOverlay` — 신화 발현 무지개 연출 | equipment §3.7 + ui_v1 §13 | C7 |
| **D12** | `RecommendedLevelWarningModal` — 챕터 진입 시 권장 미달 경고 | ui_v1 §7.2 | D3 |

**MVP 8 작업 + Phase 2 4 작업. MVP 예상 10~14일.**

---

## 6. Phase E — 메타·과금

> Phase 2 진입. Unity IAP + 별빛 인프라.

| ID | 작업 | 참조 | 의존 | 복잡도 |
|---|---|---|---|---|
| **E1** | Unity IAP 패키지 통합 + 5 패키지 등록 | monetization §7.4 | A5 | 복잡 |
| **E2** | `IAPService` + App Store Connect 설정 | monetization §7.4 | E1 | 중 |
| **E3** | 별빛 무료 출처 핸들러 — 일일 로그인 + 챕터 첫 클리어 + 추억 + 도감 100% | monetization §3.1 | A5, A7 | 중 |
| **E4** | `SeasonPassService` — 무료 선 + 프리미엄 선 (출시 후 3개월차) | monetization §3.3 | E1 | 복잡 |
| **E5** | 영수증 검증 (서버리스 Unity IAP 자체 검증) | monetization §7.4 | E2 | 중 |

**총 5 작업. 예상 7~10일.**

---

## 7. Phase F — 콘텐츠

> 디자인 + 데이터 입력. 가장 시간 많이 듦.

### Phase 2 — 메인 스토리 확장

| ID | 작업 | 참조 | 의존 | 비고 |
|---|---|---|---|---|
| **F1** | 챕터 4~10 콘텐츠 (적·보스·환경·BGM) | chapter_v2 §3 | B7 | 7개 챕터 |
| **F2** | 챕터 11~20 콘텐츠 | chapter_v2 §3 | F1 | 10개 챕터 |
| **F3** | 챕터 21~35 콘텐츠 | chapter_v2 §3 | F2 | 15개 챕터 |

### Phase 3 — Endgame 콘텐츠

| ID | 작업 | 참조 | 의존 | 비고 |
|---|---|---|---|---|
| **F4** | 일일 보스 7종 (요일별 회전) | endgame §4 | C6, B8 | |
| **F5** | 주간 보스 4종 (4주 사이클) + 신화 발현 보장 | endgame §5 | C7, F4 | |
| **F6** | 무한 모드 (끝없는 다락방) | endgame §6 | C6 | |
| **F7** | 고난도 챕터 36~50 (어둠 모드) | endgame §3 | F3 | 15개 챕터 |
| **F8** | 신화 옵션 4종 SO + 발현 효과 | equipment §3.7 + endgame §5.4 | C7, B10 | |

**메인 스토리 예상 30~45일 / Endgame 예상 25~35일.**

---

## 8. Phase G — 출시 준비

| ID | 작업 | 참조 | 의존 | 복잡도 |
|---|---|---|---|---|
| **G1** | 사운드 16종 + BGM 6종 확보·통합 | ui_v1 §13 | — | 복잡 |
| **G2** | 아트 스프라이트 (단색 더미 → 실 에셋) | CLAUDE.md §13 | — | 매우 복잡 |
| **G3** | 한국어 텍스트 검수 (어린이 어휘 확인) | CLAUDE.md §2 | F3 | 중 |
| **G4** | iOS 빌드 + TestFlight 배포 | CLAUDE.md §11 | — | 중 |
| **G5** | App Store 심사 + 4+ 등급 인증 | monetization §10 | G4 | 중 |
| **G6** | 출시 후 3개월 라이브 운영 (시즌 1) | endgame + monetization | E4 | — |

**총 6 작업. 예상 14~21일 (이슈 대응 별도).**

---

## 9. 의존성 그래프 (요약)

```
A6 (SaveData)
  └─→ A5 (CurrencyService) ──→ C5 (변환 모루) ──→ D7
  └─→ A2 (PlayerProgress) ──→ D1 (HUD)
  
A1 (PlayerStats) ──→ A3 (DamageCalc)
                 ──→ B3 (Affix SO) ──→ C1 (AffixRoller) ──→ C2 (ItemDrop)
                                                          └─→ D5 (장비 카드)
B1 (EquipmentTemplate) ──→ B2 (Instance) ──→ D4 (인벤토리)
                                          └─→ C4 (EffectResolver)
B7 (ChapterData 35) ──→ A4 + C9 (EnemySpawner) ──→ D3 (지도)
B9 (BalanceConfig) ──→ C3 (Enhancement) ──→ D6 (강화 모달)

E1 (Unity IAP) ──→ E2 (IAPService) ──→ D9 (별빛 상점)
                                    └─→ E5 (영수증 검증)
```

---

## 10. MVP 범위 (1차 출시)

> **목표**: 챕터 1~10 + 4 화폐 + 변환 모루 + L100 곡선 + 핵심 UI. 과금·endgame 제외.

### MVP 포함

- **Phase A 전체** (A1~A7) — Foundation
- **Phase B 8/10** (B1~B9, B10 신화는 Phase 2)
- **Phase C 6/9** (C1~C5, C9 — C6 endgame은 Phase 2, C7 신화 Phase 2, C8 패턴 갱신)
- **Phase D 8 MVP** (D1~D8)
- **Phase F 1** (F1 — 챕터 4~10)
- 사운드 8 + BGM 3 (Phase G 일부)
- 단색 더미 아트 유지

### MVP 제외 (Phase 2~3)

- Endgame (일일·주간·무한·고난도) — Phase 3
- 별빛 ⋆ + Unity IAP — Phase 2
- 신화 옵션 발현 + 무지개 연출 — Phase 2
- 챕터 11~35 — Phase 2
- 시즌 패스 — 출시 후 3개월차
- 한 손 가로 모드 — 출시 후

### MVP 예상 일정

- Phase A: 4~5일
- Phase B (1~9): 5~7일
- Phase C (1~5, 9): 6~8일
- Phase D (1~8): 10~14일
- Phase F1 (챕터 4~10): 7~10일
- Phase G 일부: 5~7일
- **MVP 합계: 약 6~8주 (1.5~2개월)** + 검수·QA

---

## 11. 우선순위 (제안)

### 즉시 시작 (Sprint 1, 1주) ✅ 2026-05-01 완료

1. **A1 PlayerStats 확장** — 모든 후속의 기반
2. **A6 SaveData 확장** — 모든 데이터 보존
3. **A7 GameEvents 신규 이벤트** — 통신 기반
4. **A5 CurrencyService** — 화폐 통합
5. **A2 PlayerProgress** — XP·레벨업
6. **A3 DamageCalculator** — 공격 정확도
7. **A4 EnemyController 곱셈** — 챕터 스케일링

### Sprint 2 (1주) — 데이터 풀

8. **B3, B4 Affix SO 35종 일괄 생성** — 가장 데이터 많음
9. **B5, B6 풀 SO** — 굴림 가능
10. **B1, B2 EquipmentTemplate + Instance** — 6줄 affix 구조
11. **B9 BalanceConfig 확장** — 강화 비용 곡선

### Sprint 3 (1.5주) — 시스템

12. **C1 AffixRoller** — 굴림 동작
13. **C2 ItemDropService** — 드랍 트리
14. **C5 ConversionForge** — 변환 모루
15. **C3 EnhancementService** — 강화
16. **C4 EffectResolver** — 통합 stats

### Sprint 4 (2주) — UI

17. **D1 HUD 갱신**
18. **D2 Attic 갱신**
19. **D7 변환 모루 UI**
20. **D6 강화 모달**
21. **D5 장비 카드 6단**
22. **D4 인벤토리 12 슬롯**

### Sprint 5 (1주) — 챕터 + 검수

23. **B7 ChapterData 35개 SO** — endgame 제외 4~10만 활성
24. **F1 챕터 4~10 콘텐츠 (튜닝·BGM·테스트)**
25. **G3 한국어 검수**
26. **TestFlight 빌드 + 베타 테스트**

→ **총 6주 = MVP 출시 가능 시점**

---

## 12. 단일 작업 표준 작업 흐름

> 각 task 진행 시 따를 절차.

```
1. 참조 문서 §섹션 재확인
2. 의존 task 완료 검증 (TaskList 확인)
3. 데이터 / 클래스 / SO 작성 (Unity MCP 활용)
4. 컴파일 검증 (read_console)
5. 단위 테스트 또는 SmokeTest 추가
6. ProjectValidator 갱신 (필요 시)
7. CurveSimulator 결과 변경 검증 (수치 영향 시)
8. CLAUDE.md §12 추가 (구현 완료 표기)
9. git commit (의미 단위)
```

---

## 13. 위험·미해결

- **데이터 마이그레이션 위험**: 기존 16 EquipmentData → EquipmentTemplate 변환 시 SO GUID 보존 검증 필요
- **시뮬 가정 ↔ 실제 게임플레이 차이**: 시뮬은 챕터당 적 1,000마리 가정 — 실 측정 후 보정
- **Unity IAP**: 결제 정책 변경 위험 (App Store 정책 동향 모니터)
- **Affix 시뮬**: 현재 평균값 기반, RNG 분산 ±20% 미반영 — 추가 시뮬 권장
- **iOS 4+ 등급 심사**: 별빛 상점이 가챠로 오해받지 않도록 명시 필요
- **메인 스토리 75 챕터 콘텐츠**: 35 + 15 hardcore = 50 챕터 디자인·튜닝 작업 — 최대 3개월 콘텐츠 작업
- **시뮬 §10 경고**: TTK 6개 챕터 ⚠️ + 저챕터 XP 과잉 — 콘텐츠 입력 후 실측 보정

---

## 14. 다음 단계 (즉시)

1. **본 계획서 검토 + 우선순위 확정 (사용자)**
2. **Sprint 1 착수: A1 PlayerStats 확장** ← Phase 0 작업 시작 지점
3. **Sprint 1 종료 시 CurveSimulator 재실행 + 정합 검증**
4. **MVP 6주 일정 vs 외부 마일스톤 (TestFlight·App Store 심사) 동조**

---

## 15. 출처

- 모든 설계 문서 (equipment_v1·stats_v1·affix_pool_v1·item_drop_v1·character_progression_v1·chapter_curve_v1·economy_v1·endgame_v1·monetization_v1·ui_v1)
- CurveSimulator (`Tools > LittleHero > Curve Simulator`)
- CLAUDE.md §11 마일스톤 + §12 Phase 1+2 구현 완료 + §13 Phase 3 작업
