# Part 4. 옵션 라이브러리 (세트 · 신화)

> [← Part 3](./part_3_economy.md) · [README](../README.md) · [다음: Part 5 →](./part_5_ui_ux.md)
>
> ✦ `direction_v2.md` 2번 반영. **캐릭터 베이스 스탯**도 `Effect` 리졸버의 입력에 포함된다.

---

## 1. 핵심 결정

출처가 캐릭터 레벨이든 베이스든 신화든 세트든, 산출되는 결과물은 모두 동일한 `Effect` 데이터 레코드다. 형태가 같으면 리졸버는 출처를 신경쓰지 않고 **한 가지 알고리즘**으로 처리할 수 있다.

---

## 2. 설계 원칙

### 효과 통합 데이터 모델

```ts
type Effect = {
  type: 'STAT_MOD' | 'SKILL_MOD' | 'TRIGGER' | 'REPLACE'
  target: string         // 'attack', 'skill_1.cooldown', 'crit_chance'
  op: 'ADD' | 'MUL' | 'OVERRIDE'
  value: number
  condition?: string     // 'when_hp_below_50', 'on_crit'
}
```

### 리졸버 알고리즘

0. ✦ **캐릭터 베이스 스탯 입력** (Part 1b의 `base_stats` — 레벨업으로 결정된 max_health/attack/등). 이 위에 장비 효과가 쌓인다.
1. 장착된 모든 아이템에서 효과 수집 (베이스, 옵션, 신화)
2. 세트 멤버십 카운트 → 충족된 threshold의 세트 효과 추가 (디아블로식 누적 적용)
3. `target`별로 그룹핑 — 캐릭터 베이스 스탯도 같은 `target` 도메인 (`max_health`, `attack` 등)
4. `op` 우선순위로 적용: **ADD → MUL → OVERRIDE**
5. 최종 스탯 산출 → 캐시 갱신

### 운영 핸들 정리

| 작업 | 방법 |
|---|---|
| 신규 신화 추가 | `mythic_def`에 row 1줄 |
| 시즌 메타 회전 | `season_retired` 필드 갱신 |
| 밸런스 패치 | `effects` 배열의 숫자 수정 |
| 이벤트 가중치 조정 | `rarity_weight` 임시 변경 |
| A/B 테스트 | 풀 쿼리에 `user_segment` 조건 추가 |

`archetype_tag`가 운영의 가장 강력한 핸들. 시즌마다 archetype별 분포를 의도적으로 설계해 빌드 다양성을 자연스럽게 굴리고, 메타가 한쪽으로 쏠리면 해당 archetype의 `rarity_weight`만 일괄 조정해 식힌다.

---

## 3. 구체적 사양

### 세트 정의 구조

```ts
type SetDefinition = {
  id: string                    // 'dragon_kings_set'
  name: string                  // '용왕의 유산'
  member_base_ids: string[]     // 이 세트에 속하는 아이템 base_id
  bonuses: {
    threshold: number           // 2, 4, 6
    effects: Effect[]
  }[]
  season_introduced: number
  season_retired: number | null
}
```

세트 멤버십을 아이템 마스터에 박지 말고 **세트 정의 쪽에서 멤버를 나열**한다. 운영 중 세트 구성 조정이 한 곳에서 끝난다.

### 신화 옵션 정의 구조

```ts
type MythicDefinition = {
  id: string
  name: string
  flavor_text: string
  effects: Effect[]
  rarity_weight: number         // 풀 내 가중치
  slot_restriction: SlotType[] | null
  archetype_tag: string         // 'burst_dps', 'sustain_tank' 등
  season_introduced: number
  season_retired: number | null
  is_legacy_protected: true     // 항상 true
}
```

### 드롭 풀 vs 효과 적용 분리

이미 발현된 신화 옵션은 시즌 풀에서 빠져도 영원히 작동해야 한다. 두 쿼리는 명확히 분리한다.

```sql
-- 드롭 풀 (이번 시즌 굴릴 수 있는 것)
SELECT * FROM mythic_def
WHERE season_introduced <= :current_season
  AND (season_retired IS NULL OR season_retired > :current_season);

-- 효과 적용 (이미 발현된 것은 시즌 무관)
SELECT * FROM mythic_def WHERE id = :item.mythic_id;
```

---

## 4. 미해결 질문

- archetype 분류 체계 (5~8종 권장: burst, sustain, control, mobility, support, tank, hybrid 등)
- 시즌별 신화 추가 개수 (3~5개 권장)
- 출시 시 보유할 세트 종수 (10~15종 권장)
- 출시 시 보유할 신화 옵션 수 (50~80종 권장)
- 효과 평가 엔진의 정확한 우선순위 처리 (조건부 효과의 평가 시점 등)

---

## 5. 다음 단계

- archetype 5~8종 정의 + 각 archetype의 컨셉 문서
- 출시 시 세트 카탈로그 (10~15종, 각 세트의 컨셉과 piece 구성)
- 출시 시 신화 옵션 카탈로그 (50~80종, 효과 + flavor text)
- 효과 평가 엔진 프로토타입 + 단위 테스트
- 시뮬레이션: 임의 빌드 100개 자동 생성하여 밸런스 분포 분석
