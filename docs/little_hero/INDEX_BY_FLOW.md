# Little Hero — 게임플레이 흐름 인덱스 (토론용)

> [← README](./README.md) · [시스템별 인덱스](./INDEX_BY_SYSTEM.md) · [전체 토론](./DISCUSSION.md)
>
> 11 시스템을 **사용자 게임플레이 흐름순** 으로 재구성. 신규 사용자가 게임을 시작해 주간 보스까지 도달하는 동선을 따라 각 시스템이 어디서 등장하는지 추적.

---

## 0. 사용자 동선 한눈에

```
앱 실행 → 다락방 → 캐릭터 선택 → 신기한 지도 → 챕터 진입 → 전투 → 드랍 → 클리어
   ↓                                                                  ↓
   ▼                                              ┌───────────────────┘
[메타]                                            ▼
일기장·도감·마음 단련·인벤토리·카드 ←─────  보상 적립 + XP 레벨업
   ↑                                              │
   └─── L100 + CH35 첫 클리어 ──────────────→ Endgame 메뉴
                                                 ↓
                              일일 보스 / 주간 보스 / 무한 다락방 / 고난도
```

---

## 1. 첫 실행 — 캐릭터·다락방

### 1.1 첫 실행 시 발생
- `Awake` 부트스트랩 — `MetaProgression.Instance` 자동 생성
- `SaveSystem.Load` — `save.json` (없으면 새 SaveData)
- `AudioManager.Bootstrap` — 5 SFX (절차합성) + BGM 캐시 준비
- `PlayerProgress.Bootstrap` — Level 1 (또는 저장된 값)

### 1.2 다락방 (Attic.unity)
- BGM `attic_nostalgic` (Joe Hisaishi 풍 잔잔)
- 캐릭터 선택 3종 (`Char_DefaultBoy / BraveGirl / QuietFriend`)
- 4 화폐 표시 (★ / ✦ / ◇ / ⋆)

**관련 시스템**:
- 캐릭터 → [`character_progression_v1.md`](./character_progression_v1.md) §1·§2 (XP 곡선·레벨업)
- 화폐 → [`economy_v1.md`](./economy_v1.md) §1·§2 (4 화폐 정의)
- UI → [`ui_v1.md`](./ui_v1.md) §6 (다락방 레이아웃)

### 1.3 메타 메뉴 6개
| 버튼 | 기능 | 관련 문서 |
|---|---|---|
| 일기장 열기 (DiaryPanel) | 추억 4종 발견 진척 | (CLAUDE.md §6 — 추억 시스템) |
| 마음 단련 (MetaUpgradePanel) | 무지개 구슬 ✦ 영구 강화 | [`economy_v1.md`](./economy_v1.md) §3 |
| 도감 (BestiaryPanel) | 처치한 적 도감 | (CLAUDE.md §6 — 도감) |
| 변환 모루 (ConversionForgePanel) | 별가루↔무지개 구슬 → 마법 결정 | [`economy_v1.md`](./economy_v1.md) §11.4 |
| 별빛 상점 (StarlightShopPanel) | IAP + 별빛 → 마법 결정 | [`monetization_v1.md`](./monetization_v1.md) §8 |
| 보스 도전 (EndgameMenuPanel) | 일일·주간·무한·고난도 | [`endgame_v1.md`](./endgame_v1.md) §3~§6 |
| 신기한 지도 (WondrousMapPanel) | 35 챕터 진입 | [`chapter_structure_v2.md`](./chapter_structure_v2.md) |
| 인벤토리 (InventoryPanel) | 12 슬롯 + 장비 카드 | [`equipment_v1.md`](./equipment_v1.md) + [`ui_v1.md`](./ui_v1.md) §8·§9 |

---

## 2. 챕터 진입 — 신기한 지도

### 2.1 신기한 지도
- 35 노드 그리드 (5×7) + coverArt 썸네일
- 잠금 규칙: CH N 해방 = CH(N-1) 첫 클리어 (`firstClearedChapters` 비트)
- 노드 클릭 → 진입 모달 (제목·설명·보상)
- 권장 레벨 미달 시 **D12 RecommendedLevelWarningModal** 표시

**관련 시스템**:
- 챕터 구조 → [`chapter_structure_v2.md`](./chapter_structure_v2.md) §3 (35 챕터 카탈로그)
- 권장 레벨 → [`character_progression_v1.md`](./character_progression_v1.md) §6 + UI 매트릭스

### 2.2 챕터 보상 (모달 표시)
- `보상: 무지개 구슬 ✦ N / 마법 결정 ◇ N / 별빛 ⋆ N (첫 클리어)`
- 공식: ✦ N×1 / ◇ 1+N/3 / ⋆ N×2 ([`economy_v1.md`](./economy_v1.md) §3)

---

## 3. 챕터 플레이 (Chapter.unity)

### 3.1 진입 시
- `ChapterRunner.Start` → 카테고리 BGM crossfade (`ch_meta/nature/animal/civ/fantasy_a/b`)
- `EnemySpawner.SetActiveChapter` — `enemyStatMultiplier = 1.10^(N-1)` 적용
- `TestEquip` (디버그) — `Equip_StarWand` 자동 장착 (현재 wiring)

### 3.2 20 웨이브 자동 진행
| 웨이브 | 적 종류 | 길이 |
|---|---|---|
| 1, 2, 3, 4 | 일반 (Small/Medium/Large) | 30~60s |
| **5, 10, 15** | + **미니보스** (MiniA / MiniB) | — |
| 6~9, 11~14, 16~19 | 일반 + 가속 | — |
| **20** | **챕터 보스** | — |

### 3.3 전투 — 자동 슈팅
- `EquipmentManager` 가 장착 장비의 `IAttackPattern` 활성화
- 7 패턴: Projectile / Aura / Orbit / Rain / Trail / Pulse / Summon
- 데미지 계산 — `DamageCalculator` (3 버킷 + 방어 관통)
- 적 처치 → "퐁!" SFX + 별 흩어짐 VFX (CLAUDE.md §2)

**관련 시스템**:
- 공격 패턴 → [`equipment_v1.md`](./equipment_v1.md) §2 (7 패턴)
- 데미지 공식 → [`stats_v1.md`](./stats_v1.md) §6 (3 버킷)
- 적 곡선 → [`chapter_curve_v1.md`](./chapter_curve_v1.md) §2~§4

### 3.4 보스전 (웨이브 20)
- **🆕 보스 체력바** 화면 상단 자동 활성 (UI 카탈로그 추가)
- BGM crossfade `boss_chapter`
- HP × 21.6 ratio + 챕터 곱셈 적용

---

## 4. 드랍·아이템 시스템

### 4.1 드랍 결정 트리
1. **확률 굴림** — Enemy 0.5% / Mini 8% / Boss 100% / Shop 100%
2. **슬롯 선택** — CH ≤ 10 → 주 7 / CH 11+ → 12 균등
3. **등급** — chapter 묶음 분포 + Mini/Boss +1 가중
4. **템플릿** — slot+rarity 매칭
5. **AffixRoller** — 주옵션 3 + 부옵션 3 굴림

**관련 시스템**:
- 드랍 트리 → [`item_drop_v1.md`](./item_drop_v1.md) §4
- 주옵션 풀 → [`affix_pool_v1.md`](./affix_pool_v1.md) §3 (슬롯별 풀)

### 4.2 EquipmentInstance 생성
- `EquipmentTemplate` (마스터) + 인스턴스별 affix·강화 레벨
- `SaveData.equipmentInstances` 영구 저장
- 신화 마일리지 +1 (드랍/처치마다)

**관련 시스템**:
- 인스턴스 모델 → [`equipment_v1.md`](./equipment_v1.md) §4 (5층 아키텍처)
- 신화 발현 → [`equipment_v1.md`](./equipment_v1.md) §3.7 + [`monetization_v1.md`](./monetization_v1.md) §4.4

---

## 5. 상점 페이즈 (웨이브 사이)

### 5.1 6칸 상점 슬롯
- 장비 4 + 강화 2
- 별가루 ★ 로 구매·새로고침
- 인벤토리 드래그&드롭 슬롯 교체

**관련 시스템**:
- 상점 풀 → [`item_drop_v1.md`](./item_drop_v1.md) §5 (DropSource.Shop)
- 가격 → [`affix_pool_v1.md`](./affix_pool_v1.md) §4.2 (등급별 priceByRarity)

---

## 6. 챕터 클리어 → 보상 적립

### 6.1 보스 처치 시점
- **챕터 클리어 패널** — `무지개 구슬 ✦ +N` (G3 1차 변경, 옛 별 조각)
- `firstClearedChapters` 비트 set (해당 챕터)
- **별빛 ⋆ 첫 클리어 보상** — N×2⋆ (`StarlightGrantService.TryGrantChapterFirstClear`)
- **마법 결정 ◇** + **무지개 구슬 ✦** 적립

### 6.2 XP 적립
- 적 처치 누적 + 챕터 클리어 N×20 (50% 컷, 시뮬 §13 보정)
- 추억 발견 시 +20 XP
- 레벨업 → `OnLevelUp` 이벤트 → 스탯 자동 재계산

**관련 시스템**:
- XP 공식 → [`character_progression_v1.md`](./character_progression_v1.md) §3·§5
- 보상 매트릭스 → [`economy_v1.md`](./economy_v1.md) §3

---

## 7. 다락방 복귀 — 메타 진행

### 7.1 인벤토리 → 장비 카드 6단
| 단 | 내용 |
|---|---|
| 1 | 정체성 (이름·아이콘·강화 레벨·history) |
| 2 | 베이스 스탯 (template.baseStats) |
| 3 | 강화 진입 버튼 (→ EnhanceModal) |
| 4 | 주옵션 3줄 (보라 배경, 라인별 잠금 + 리롤) |
| 5 | 부옵션 3줄 (청록 배경) |
| 6 | 신화 영역 (마일리지 + 발현 시 displayName) |

**관련 시스템**:
- UI → [`ui_v1.md`](./ui_v1.md) §9 (카드 6단)
- 강화 → [`equipment_v1.md`](./equipment_v1.md) §3.3 (+0~+25 + 천장)

### 7.2 마음 단련 (메타 강화)
- 3종 (HpBoost / DamageBoost / DustGain)
- 무지개 구슬 ✦ 으로 영구 강화 (max 5)

### 7.3 변환 모루
- 별가루 1000 ★ → 마법 결정 1 ◇
- 무지개 구슬 5 ✦ → 마법 결정 1 ◇

**관련**: [`economy_v1.md`](./economy_v1.md) §11.4

### 7.4 일기장 (추억)
- 4 추억 (DadPhoto / DadMemo / DadSticker / DadDoodle)
- 모두 발견 시 신화 발현 활성 ([`equipment_v1.md`](./equipment_v1.md) §3.7)

---

## 8. Endgame (L51+ + CH35 클리어 후)

### 8.1 보스 도전 메뉴 활성 조건
- `firstClearedChapters` 35 비트 ON 또는 추억 4종 모두 발견
- (현재 OR 조건, AtticController.AllChaptersFirstCleared)

### 8.2 4 모드
| 모드 | cooldown | 보상 |
|---|---|---|
| 일일 보스 | KST 0시 | 별빛 + 마법 결정 |
| 주간 보스 | 월요일 KST 0시 | + **신화 발현 보장** |
| 무한 다락방 | 무제한 | 깊이별 |
| 고난도 챕터 (CH 36~50) | 챕터별 1회 | ×2.0 곱셈 |

**관련 시스템**:
- Endgame → [`endgame_v1.md`](./endgame_v1.md) §3~§6
- 신화 발현 → [`equipment_v1.md`](./equipment_v1.md) §3.7 + 주간 보스 GuaranteeRoll

### 8.3 BossEncounter 씬
- 보스 1마리 spawn → 처치 → 자동 보상 적용 → 2초 대기 → Attic 복귀
- 신화 발현 시 다락방 복귀 후 🌈 무지개 Overlay 자동 재생

---

## 9. 시즌 패스 (출시 3개월차)

### 9.1 시즌 시스템
- 4단계 (100/250/500/1000 pt)
- free 보상 + premium 보상 (별빛 IAP)
- `SeasonPassService.RotateSeason`

**관련**: [`monetization_v1.md`](./monetization_v1.md) §3.3 + Phase E4

---

## 10. 흐름별 미해결 토론 포인트

| 흐름 단계 | 토론 포인트 |
|---|---|
| 첫 실행 | 첫 다락방에서 사용자가 무엇 먼저 봐야? (튜토리얼 없음 시 onboarding) |
| 챕터 선택 | 권장 레벨 미달 시 경고 모달 — 사용자가 무시 빈도 높으면? |
| 챕터 플레이 | 자동 슈팅이라 사용자 행동 적음 — 능동성 어디서? |
| 드랍 | 매 챕터 4~6 인스턴스 드랍 — 인벤토리 12 슬롯 빠르게 가득 (정리 UI 필요?) |
| 보스 처치 | 챕터 클리어 패널 — 보상 + 다락방 복귀 자동 (사용자 자발 진행 vs 자동) |
| 메타 | 7개 메뉴 (일기장/마음 단련/도감/변환 모루/별빛 상점/보스 도전/신기한 지도/인벤토리) — 너무 많지 않은지 |
| Endgame | L51+ 진입 시 사용자가 어디로 가야? (UI 가이드) |
| 시즌 패스 | 출시 직후 보다는 3개월차 도입 — 그 사이 retention 핵심 메커니즘 |

전체 흐름 토론 → [`DISCUSSION.md`](./DISCUSSION.md)
