# Gacha Class Filter v1 — 가챠 직업 제한 설계

> **목표**: 가챠 결과 = 본인 직업 무기만. 다른 직업 무기 폐기. weapon_system_v2 정합.
> **OOP 정책 (CLAUDE.md §15)**: SOLID-O / Strategy / SO 우선.

## 0. 결정 이력
- 2026-05-03: 사용자 요청 — 가챠도 직업별 제한 결과

## 1. 현재 가챠 분석

### 1.1 GachaService 흐름
```
RollSingle(pool, currency)
  → 등급 결정 (소환 레벨 곡선)
  → RollOne(pool, rng, forcedRarity?)
  → 등급별 풀에서 균등 추출
  → BuildAndStoreInstance (affix 6 + history)
  → SaveData.equipmentInstances append
```

### 1.2 직업×등급 매트릭스 (현재 18 무기)

| | Common | Magic | Rare | Epic | 합 |
|---|---|---|---|---|---|
| **Warrior** | 1 (impact_hammer) | 0 ⚠️ | 2 (thunder + katana) | 1 (abyss) | 4 |
| **Archer** | 3 (quick/fan/spin) | 2 (pierce/phantom) | 1 (boomerang) | 1 (storm_bow) | 7 |
| **Mage** | 2 (rune/firebottle) | 2 (frost/spirit_pet) | 1 (meteor) | 2 (annihilation/time_spirit) | 7 |

⚠️ **Warrior Magic 등급 부재** — fallback 또는 신규 무기 추가 필요.

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A 풀 분리 | 직업별 GachaPool 3 SO | 명확 | SO 3배, 관리 복잡 |
| **B 런타임 필터** | 단일 풀 + RollOne 시 직업 매칭 | 단순, SO 보존 | Warrior Magic 부재 fallback |
| C 변환 | 다른 직업 → 본인 직업 자동 변환 | 가챠 결과 100% 사용 가능 | 복잡, 의도 모호 |
| D 가챠권 | 무기 소환권만 직업 한정 | 화폐 분리 | UX 분기 |

## 3. 권장 — **옵션 B** (단일 풀 + 런타임 필터)

### 3.1 설계 변경

**GachaService.RollSingle 시그니처 확장**:
- 기존: `RollSingle(GachaPool pool, GachaCurrency currency)`
- 신규: `RollSingle(GachaPool pool, GachaCurrency currency, CharacterClass cls)`

**RollOne 흐름**:
```
1. 등급 결정 (소환 레벨 분포 — 변경 X)
2. pool.GetTemplatesByRarityAndClass(rarity, cls) → 매칭 list
3. list 비어있으면 → rarity-1 재시도 (Warrior Magic 부재 fallback)
4. 여전히 비어있으면 → 같은 직업 어떤 등급이든 (안전 fallback)
5. list 에서 균등 추출
```

### 3.2 GachaPool SO 확장

기존 `GachaPool.commonPool / magicPool / rarePool / epicPool` (등급별 EquipmentTemplate[]) 유지.

신규 헬퍼 메서드 (런타임 필터):
```
public List<EquipmentTemplate> GetTemplatesByRarityAndClass(Rarity r, CharacterClass cls);
```

### 3.3 PlayerClass Resolver

`GachaService.ResolvePlayerClass()` (정적):
- `MetaProgression.Instance.SelectedCharacter.className` → CharacterClass enum
- 미선택 시 default Warrior

### 3.4 LimitedShopOffer 통합

`LimitedShopOffer.guaranteedRarity` 보상 시:
- 동일하게 직업 필터 적용
- `GachaService.GrantWeaponByRarity(pool, rarity, cls)` 호출

## 4. 시너지 (weapon_system_v2 + gacha_class_filter)

```
사용자 캐릭터 = 마법사 (quiet_friend)
  ↓
가챠 진입 → ResolvePlayerClass() = Mage
  ↓
RollSingle(pool, Starlight, Mage)
  ↓
등급 결정 (Lv 2 분포)
  ↓
GetTemplatesByRarityAndClass(Common, Mage) = [rune_staff, firebottle]
  ↓
균등 추출 → rune_staff
  ↓
BuildAndStoreInstance + isEquipped=false
  ↓
인벤토리 → Weapon 슬롯 → Mage 무기만 표시
  ↓
시너지: WIS 2.0× + Staff Magic 시너지 +10%
```

## 5. UI 영향

### 5.1 GachaPanel
- 진입 시 player class 표시 ("현재: 마법사 — 가챠는 마법사 무기만")
- 직업 라벨 (등급 색 + 직업 색)

### 5.2 가챠 결과
- 결과 카드에 직업 라벨 (자동 본인 직업)
- "직업 제한" 안내 (재차 강조)

### 5.3 직업 변경 안내
- 다락방 캐릭터 변경 시 → "가챠 결과 직업이 변경됩니다" 토스트

## 6. SaveData 호환

기존 SaveData.equipmentInstances 의 무기 인스턴스 (다른 직업 포함) — 그대로 유지. 본 설계는 **신규 가챠** 만 영향.

기존 잘못된 직업 무기는 인벤토리에서 회색 표시 (weapon_system_v2 §5.1).

## 7. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Warrior Magic 부재 → 가챠 무한 fallback | rarity-1 1회만 시도, 그 후 같은 직업 random |
| 직업 변경 시 가챠 결과 분배 변경 | 토스트 안내 + 가챠 진입 시 명확 표시 |
| 기존 다른 직업 인스턴스 사용 불가 | 인벤토리 회색 + "다른 직업 변경 시 사용" 안내 |
| 신규 무기 추가 시 직업×등급 균형 | weapon_system_v2 의 ProjectValidator OOP 룰 검증 |

## 8. 향후 — 18 → 24 무기 확장 (Warrior Magic 보강)

신규 6 무기 추가 시 균형 매트릭스 (제안):
| | Common | Magic | Rare | Epic | 합 |
|---|---|---|---|---|---|
| Warrior | 2 | **2** | 2 | 2 | 8 |
| Archer | 2 | 2 | 2 | 2 | 8 |
| Mage | 2 | 2 | 2 | 2 | 8 |

각 직업 8 무기, 등급 균등 2:2:2:2 — **균형 가챠 풀**. 별도 Sprint.

## 9. 변경 이력
- 2026-05-03: 초기 작성 (옵션 B 권장 — 런타임 필터)

## 10. 참조
- `weapon_system_v2.md` — 직업·카테고리 분류
- `weapon_system_v2_prd.md` — 5 Sprint PRD
- `gacha_v1.md` — 18 무기 가챠 풀
- `CLAUDE.md` §15 OOP 정책
