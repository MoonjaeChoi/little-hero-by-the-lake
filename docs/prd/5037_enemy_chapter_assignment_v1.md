# 5037 enemy_chapter_assignment_v1 — 챕터별 적 변형 + 반복 적용 시스템

> **목적**: 5035 IEnemyBehavior 인프라(Stationary/Charger/Splitter/Ranged/Bomber)를 실제 챕터 콘텐츠에 적용하되, 35 챕터 + Endgame 까지 반복적/점진적 확장이 용이한 데이터+도구 패턴을 정착.
>
> **배경**: 5035 PGE 6-Phase 결과 — 5 신규 IEnemyBehavior + 2 IDeathReaction + Factory + EnemyComposition/WaveComposition SO 클래스 모두 구현 완료. 단, **0/262 EnemyData SO 가 behaviorType 미적용** (모두 default Follow). 콘텐츠 wiring 미수행 → 시범 + 확장 패턴 정착 필요.

---

## 1. 핵심 결정 (사용자 선택 반영)

| 항목 | 결정 | 사유 |
|---|---|---|
| **시범 적용** | CH3/4/5 (시범 1차) | 사용자 요청 "시범적용을 하고" |
| **반복 적용** | 35 챕터 + Endgame 점진 확장 | 사용자 요청 "상위 챕터에 구현이 될 것을 감안" |
| **데이터 패턴** | 변형 EnemyData SO 5종 (archetype) + 챕터 wave 룰 | 5035 EnemyData.behaviorType 답습, ChapterData 구조 변경 최소 |
| **확장 도구** | Editor 시드 도구 (Tools/LittleHero/Seed Chapter Variants) | 35 챕터 일괄 적용 시 수작업 회피 |

---

## 2. 디자인 패턴 (3 옵션 비교)

### 옵션 A — **EnemyData 변형 SO + ChapterData.miniBossPool 답습** ⭐ 선택

**장점**:
- ChapterData 구조 변경 0 (기존 enemyPool/miniBossPool/bossEnemy 그대로)
- WaveManager 변경 0 (변형 SO 도 EnemyData)
- 5 archetype SO 1회 만들고 35 챕터 + Endgame reuse
- 점진 확장: 챕터마다 miniBossPool[]에 변형 SO 추가만 하면 됨

**단점**:
- 챕터별 wave 단위 미세 제어 불가 (wave 5/10/15 모두 miniBossPool 추첨)
- 5035 WaveComposition SO 미활용 (deferred)

### 옵션 B — **ChapterData.waves[20] WaveComposition[] 도입** (5035 T05 답습)

**장점**:
- wave 5/10/15/20 별 정확한 제어
- 5035 WaveComposition/EnemyComposition 즉시 활용
- 향후 보스 타임라인 / 페이즈 분기 확장 용이

**단점**:
- ChapterData 구조 변경 (waves[20] 신설)
- WaveManager.SpawnRoutine 큰 변경 (WaveComposition 인지)
- 35 챕터 × 20 wave = 700 SO 인스턴스 또는 큰 inline 배열 — 작업량 ↑

### 옵션 C — **Inspector 직접 wiring**

**장점**: 도구 0
**단점**: 35 챕터 × 5 wave 변형 = 175 SO 수작업, 일관성 ↓

**선택 → 옵션 A** (5037 본체 / 시범 + 반복 패턴 정착) + **옵션 B 후속** (5037.1+ cascade — 정확 제어 필요 시 단계 도입)

---

## 3. 5 Archetype 변형 SO 설계

### 3.1 명세 (5037 본체)

| 변형 SO | behaviorType | deathReactionType | base stat (기준) | 챕터 활용 |
|---|---|---|---|---|
| `Variant_Stationary_Sentry` | Stationary (1) | None | HP 30 / dmg 5 / speed 0 / armor 5 | wave 7~13 (지형 위협) |
| `Variant_Charger_Bull` | Charger (2) | None | HP 60 / dmg 8 / speed 2.5 / armor 0 | wave 13+ (CH 15+) |
| `Variant_Splitter_Bear` | Splitter (3) | Splitter | HP 40 / dmg 4 / speed 1.5 | wave 5/10 (CH 4+) |
| `Variant_Ranged_Archer` | Ranged (4) | None | HP 25 / dmg 6 / speed 1.2 | wave 8+ (CH 10+) |
| `Variant_Bomber_Mine` | Bomber (5) | Explode | HP 20 / dmg 8 / speed 1.8 | wave 15 (CH 5+) |

**경로**: `Assets/_Project/Data/Enemies/Variants/Variant_<Archetype>.asset`

**stat 적용**: ChapterData.enemyStatMultiplier (1.10^(N-1)) 가 자동 곱셈 → 챕터 N 의 변형은 base × 챕터 곡선

### 3.2 Splitter Child 자식

`Variant_Splitter_Bear.splitterChildData` = `Variant_Splitter_BearChild` (HP 12 / dmg 2 / speed 1.8) 별도 SO

→ 6번째 SO 추가 (자식 답습용, behaviorType=Follow)

---

## 4. 챕터 배치 룰 (반복 적용 패턴)

### 4.1 기본 룰 (CH 1~35)

```
CH N % 5 == 0 (CH 5/10/15/20/25/30/35) → wave 15 miniBossPool += Bomber (자폭)
CH N % 5 == 4 (CH 4/9/14/19/24/29/34)  → wave 5  miniBossPool += Splitter (분열)
CH N % 5 == 3 (CH 3/8/13/18/23/28/33)  → wave 10 miniBossPool += Stationary (포대)
CH N >= 10                              → enemyPool += Ranged   (소수 mix)
CH N >= 15                              → enemyPool += Charger  (소수 mix)
```

### 4.2 시범 적용 (CH 3/4/5) — 5037 본체

- **CH3 — Alice**: wave 10 miniBossPool 에 `Variant_Stationary_Sentry` 추가 → 카드 군대 사이 포대
- **CH4 — Aladdin**: wave 5 miniBossPool 에 `Variant_Splitter_Bear` 추가 → 사막 곰 분열
- **CH5 — TBD**: wave 15 miniBossPool 에 `Variant_Bomber_Mine` 추가 → 지뢰

### 4.3 점진 확장 (5037.1+ cascade)

- 5037.1: CH6~15 적용 (Ranged 도입 CH10+)
- 5037.2: CH16~25 적용 (Charger 도입 CH15+)
- 5037.3: CH26~35 적용 + Endgame Daily/Weekly Boss reuse

---

## 5. Editor 시드 도구

### 5.1 명세

**메뉴**: `Tools/LittleHero/Seed Chapter Enemy Variants`

**동작**:
1. `Assets/_Project/Data/Enemies/Variants/` 폴더 생성 (없으면)
2. 6 변형 SO 생성/갱신 (Stationary/Charger/Splitter+Child/Ranged/Bomber)
   - 기존 SO 가 있으면 stat 갱신만 (GUID 보존)
3. CH3/4/5 ChapterData 의 miniBossPool[] 에 룰 기반 추가
   - 중복 검사 (이미 추가된 SO 면 skip)
4. `AssetDatabase.SaveAssets() + Refresh()`
5. 콘솔 로그: 갱신 SO N개 / 챕터 wiring M개

### 5.2 확장 모드 (5037.1+)

같은 메뉴 + 별도 옵션 (예 `Seed All 35 Chapters`) — 35 챕터 룰 일괄 적용. 5037 본체에서는 CH3/4/5 만, 후속에서 확장.

### 5.3 Validator 룰 추가

`ProjectValidator` 에 신규 룰:
- `EnemyDataVariantConsistency` — splitterChildData != null when behaviorType==Splitter 검사
- `EnemyDataProjectileConsistency` — projectilePrefab != null when behaviorType ∈ {Stationary, Ranged} 검사

---

## 6. WaveManager 통합 (변경 0)

옵션 A 채택으로 WaveManager 변경 없음. 변형 SO 가 그냥 EnemyData → 기존 SpawnEnemy 동작.

단, EnemyController.Initialize 가 5035 T06 에서 IEnemyBehavior Factory 호출 → 변형 SO 의 behaviorType 자동 적용 ✅

---

## 7. PG-12 톤 검증

- **Bomber 자폭**: 만화풍 폭발 + 별 흩어짐 (5035 ExplodeReaction 답습) ✅
- **Splitter 분열**: 작은 곰 3마리 (귀여움) ✅
- **Stationary 포대**: 발사체 (위협) ✅
- **Ranged Archer**: 활 (PG-12) ✅
- **Charger Bull**: 돌진 (만화풍) ✅

PG-12 정합 ✅

---

## 8. 성능 영향 (PIVOT v4 cap 300)

- 5 신규 SO 인스턴스: 메모리 영향 무시 가능 (~5KB)
- Bomber Splitter 분열: 1 → 4 적 (Splitter 사망 시) → cap 영향 ↑
  - 완화: Splitter SO 의 spawn 빈도 ↓ (miniBossPool 1마리만, wave 5)
- Stationary 발사체: 5035 ProjectilePool reuse → GC 0
- Charger telegraph: VfxPool.SpawnWarning reuse

---

## 9. 검증 시나리오 (Phase 6 C6)

| # | 시나리오 | 기대 결과 |
|---|---|---|
| 1 | CH3 wave 10 진입 | Stationary 변형 mini-boss 출현 + 발사체 1.5s 주기 |
| 2 | CH4 wave 5 진입 → 처치 | Splitter 변형 mini-boss → 사망 시 자식 3마리 분열 |
| 3 | CH5 wave 15 진입 → bomberRadius 진입 | Bomber 변형 → 1s telegraph + 자폭 + 별 흩어짐 |

---

## 10. 후속 작업 (Backlog)

- 5037.1: CH6~15 wiring (Ranged 도입)
- 5037.2: CH16~25 wiring (Charger 도입)
- 5037.3: CH26~35 + Endgame reuse
- 5038: ChapterData.waves[20] WaveComposition 통합 (옵션 B 본격 채택)
- 5039: Variant SO 의 sprite/tint 차별화 (현재 base SO icon reuse)

---

## 11. 변경 이력

- 2026-05-07: 초기 작성 (Phase 1 Generate)
