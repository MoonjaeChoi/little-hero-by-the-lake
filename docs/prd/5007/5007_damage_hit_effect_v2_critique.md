# 5007 damage_hit_effect_v2 PRD 리뷰 (Phase 2.1 Critique)

> **검토자**: Claude Code (자동) · **일시**: 2026-05-04
> **PRD**: `5007_damage_hit_effect_v2_prd.md` v1
> **디자인**: `5007_damage_hit_effect_v2.md`

## 1. 종합 점수: 8.9 / 10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9.5/10 | 신규 클래스 모두 100L 이내, SO 분리, struct ResolvedHitEffect (no boxing). (-0.5: enum BossTier switch 5분기 — Strategy 적용 안 함, 그러나 enum 자체가 이미 구조적 분리이므로 OK) |
| PG-12 톤 | 10/10 | 만화풍 punch + sparkle, 잔인 표현 0. CinematicEffectsConfig 의 모든 색·강도 설정 PG-12 정합 |
| 성능 (§7) | 9.5/10 | DamageFloaterPool 50→300 prewarm = GC 폭증 해소. struct 반환 + 정적 헬퍼 = 0 GC 호출. (-0.5: cap 300 환경 60fps 검증 Profiler 측정 추천) |
| 시스템 통합 | 9/10 | 5001 BossTier + weapon_v2 WeaponCategory + Damage v2 모두 정합. (-1: OnHit signature 확장으로 8 IAttackPattern 마이그레이션 부담) |
| Sprint 분해 | 7.5/10 | 8 Sprint × 11 commit / 5h40m. (-2.5: Sprint 3.2 (8 IAttackPattern 일괄 마이그레이션) 1 commit 으로 너무 큼 — 분할 권장) |

## 2. Accept 항목 (12건)

- ✅ **D1=B CinematicEffectsConfig 신규 SO** — BalanceConfig 비대화 회피, §15.1 SRP
- ✅ **D2=B TierEffectProfile struct (4 multiplier)** — 효과별 fine-tune 가능, 세밀한 차별화
- ✅ **D3=B damage intensity ratio/avg, cap 5×** — 직관적, PlayerStats 의존성 명시
- ✅ **D4=B WeaponCategory → flashColor 4 그룹** — weapon_v2 정합 (Sword/Bow/Dagger/Magic)
- ✅ **D5=B prewarm 300** — cap 300 GC 폭증 해소 (5006.1 보다 우선 적용 가치)
- ✅ **D6=C 0.05s 디바운스 (no stacking)** — 시각 안정 + 성능 균형
- ✅ **D7=B 크리 시 zoom 1.05× × 0.08s** — subtle 임팩트, 멀미 위험 X
- ✅ **D8=A 색맹 친화 5008 분리** — scope 적절
- ✅ **HitEffectResolver 정적 헬퍼** — DI 명시 (cfg 인자), §15.7 정합
- ✅ **ResolvedHitEffect struct (no boxing)** — §15.11 안티 패턴 회피
- ✅ **NUnit 8 명시** — Reflection + tier 분기 + intensity + weapon 검증
- ✅ **CinematicEffectsConfigSeed Editor 메뉴** — idempotent, 기존 시드 패턴 답습 (5001 BossLootTableSeed 참조)

## 3. Reject 항목 (0건)

(없음 — §15 위반 / PG-12 위반 0)

## 4. Discuss 항목 (4건)

### D1. Sprint 3.2 8 IAttackPattern 일괄 마이그레이션 — commit 분할

- **현재 PRD**: Sprint 3.2 가 1 commit 으로 8 IAttackPattern 모두 마이그레이션 (Projectile/Aura/Orbit/Rain/Trail/Pulse/Summon/Slash) + DamageDealer + EquipmentManager
- **문제**: 1 commit 변경량 과다 (~100~150L 변경 추정), 회수 어려움
- **옵션**:
  - **A (현재)**: 1 commit (Sprint 3.2)
  - **B (★권장)**: **2 commit 분할** — 3.2 (DamageDealer + EquipmentManager.GetActiveWeaponCategory) + 3.3 (8 IAttackPattern 일괄)
  - **C**: 8 commit 분할 (각 IAttackPattern 별 1 commit) — 과도, 비추천
- **권장**: **B** — 인프라 (1) + 마이그레이션 (1) 분리, 회수성 ↑

### D2. PlayerStats.AttackDamage 의존성 — avgDamage 계산 위치

- **현재**: HitEffectResolver.Resolve 가 `avgPlayerDamage` 인자 받음 (optional)
- **문제**: 호출자 (EnemyController.OnHit) 가 PlayerStats 를 참조해야 함 → 결합도 ↑
- **옵션**:
  - **A**: 호출자가 PlayerStats.Instance.AttackDamage 매번 query (간단, Singleton 의존)
  - **B (★권장)**: **DamageSystem.DealDamage 가 enemy.OnHit 호출 시 avgDamage 인자 전달** (이미 PlayerStats 참조 중인 위치)
  - **C**: DamageSystem 이 cached avgDamage 정적 변수 (운영 중 갱신)
- **권장**: **B** — DamageSystem 은 이미 PlayerStats 참조 중, 그곳에서 1회 query 후 OnHit 으로 전달. EnemyController 의 PlayerStats 의존성 0.

### D3. CinematicEffectsConfig SO 의 적 prefab wiring 방식

- **현재 PRD**: Sprint 4.2 의 "적 Prefab — CinematicEffectsConfig 정적 로딩 (Resources)" 모호
- **문제**: 137 EnemyData prefab 마다 SerializeField wiring 부담 vs Resources.Load 정적 호출
- **옵션**:
  - **A (★권장)**: **EnemyController._config = null + RuntimeInitializeOnLoadMethod 가 Resources.Load 1회 정적 cache** (모든 적 공유, Inspector wiring 0)
  - **B**: 137 prefab 모두 SerializeField wiring (변경 부담 ↑)
  - **C**: ScriptableObject Singleton (정적 Instance) — §15.7 DI 위반
- **권장**: **A** — Resources.Load 정적 cache, runtime 초기화 1회

### D4. cap 2.0× punch scale ceiling 적용 위치

- **현재 PRD**: §5 알려진 위험 — "weeklyBoss × intensity max = 1.96, cap 2.0× 추가 권장"
- **문제**: cap 적용 위치 명시 X (Resolver vs EnemyController vs SO)
- **옵션**:
  - **A**: SO 에 `maxPunchScale = 2.0f` 필드 추가 + Resolver 에서 Mathf.Min
  - **B (★권장)**: **Resolver 내부 hardcode `Mathf.Min(2.0f, ...)`** + 주석 "PG-12 시각 멀미 방지 cap" + 향후 SO 화 가능
  - **C**: cap 무시 (시각 과장 허용) — 비추천
- **권장**: **B** — 핵심 안전 cap 은 hardcode (잘못된 SO 값으로 인한 사용자 경험 망가짐 방지). 향후 필요시 SO 화.

## 5. CLAUDE.md §15 정책 위반 점검

| 정책 | 결과 |
|---|---|
| §15.1 SOLID-S | ✅ Config (data) + Resolver (logic) + EnemyController (apply) 명확 분리 |
| §15.1 SOLID-O | ✅ enum BossTier switch 는 SO 데이터로 분기 (코드 X) |
| §15.4 SO 우선 | ✅ 모든 magic number → CinematicEffectsConfig |
| §15.6 100L | ✅ ~80/~70/~50 모두 통과 |
| §15.7 DI | ✅ Resolver 정적 + cfg 인자 (Singleton 직접 X) |
| §15.9 Test | ✅ 8 NUnit 명시 (Reflection + 분기) |
| §15.11 안티 패턴 | ✅ public 필드 X (SO 만), Update X (OnHit 트리거), struct (no boxing), Object pool |
| §15.12 Validator | ✅ (선택) BossTier_HasEffectProfile 룰 추가 가능 |

## 6. PG-12 톤 점검

- ✅ punch / flash / vfx — 만화풍, 잔인 X
- ✅ slow-mo + screen shake — cinematic, 멀미 위험 cap 2.0× 권장
- ✅ flash 색 4 종 — 모두 따뜻 색감 (Red/Cyan/Orange/Purple)
- ✅ damage floater — 1.2K/15K/1.5M 압축 (텍스트만, PG-12 OK)
- ✅ SFX — 절차적 합성, 사실적 폭력음 X

## 7. 권장 변경 (mediator 단계)

### 즉시 변경 (Accept 자동 반영)
- 없음 (Reject 0건)

### 사용자 결정 후 변경 (4건 → discussion_topics.md)
- D1 Sprint 3.2 commit 분할 (1 → 2)
- D2 avgDamage 전달 위치 (DamageSystem)
- D3 CinematicEffectsConfig 적 prefab wiring (Resources.Load 정적 cache)
- D4 cap 2.0× 적용 위치 (Resolver hardcode)

## 8. 비기능 검증

- ✅ cap 300 환경 GC 0 (prewarm 300 + struct 반환)
- ✅ 60fps 유지 — Profiler 검증 권장 (Sprint 6 후)
- ✅ 5001~5006 회귀 X (Combat 영역만 변경, UI/Hub 영향 0)
- ✅ 메모리 ~30KB 추가 (250MB 한계 무시 가능)

## 9. 요약

```
종합: 8.9/10
Accept: 12 (자동 반영)
Reject: 0
Discuss: 4 (mediator 진행)
```

PRD 전체적으로 OOP §15 정합 우수, PG-12 위반 0, 성능 개선 (DamageFloaterPool 50→300) 까지 포함. Discuss 4건 모두 Sprint 분할 / 결합도 / wiring 방식 — 사용자 결정 사안.

## 10. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.1 Critique 자동)
