# 5007 damage_hit_effect_v2 Implementation Breakdown — 개요

> **Phase 3 산출** — v2 PRD 12 commit 분해
> 일시: 2026-05-04 · 총 12 태스크 / 12 commit / 5h50m

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 시간 |
|---|---|---|---|---|
| T01 | CinematicEffectsConfig SO + TierEffectProfile struct | Data | — | 30분 |
| T02 | Editor seed (Tools 메뉴 + asset 생성 in Resources/) | Editor | T01 | 30분 |
| T03 | HitEffectResolver 정적 + ResolvedHitEffect struct + cap 2.0 hardcode | Logic | T01 | 40분 |
| T04 | DamageSystem.DealDamage 시그니처 확장 + avgDamage 전달 (D2) | Logic | — | 25분 |
| T05 | DamageDealer + EquipmentManager.GetActiveWeaponCategory 인프라 (D1 분할 ①) | Logic | — | 20분 |
| T06 | 8 IAttackPattern 일괄 마이그레이션 (D1 분할 ②) | Logic | T05 | 30분 |
| T07 | EnemyController.OnHit reflow + Loader 정적 (D3) + Resolver 호출 | Logic | T01~T06 | 45분 |
| T08 | DamageNumberFloater 동적 fontSize + 적 prefab unsafe wiring 0 | UI | T01, T07 | 20분 |
| T09 | CinematicEffects.PlayCameraZoom 신규 + SO 화 | Logic | T01 | 30분 |
| T10 | DamageFloaterPool prewarm 300 + VfxPool 100 + monitoring | Core | T01 | 30분 |
| T11 | NUnit HitEffectV2Tests 8 + Validator 룰 (선택) | Test | T01~T10 | 45분 |
| T12 | docs (changelog + CLAUDE.md §11/§12/§14) | Docs | T11 | 25분 |

## 2. 의존성 그래프

```
T01 (Config SO) ──┬→ T02 (Editor seed)
                  ├→ T03 (Resolver) ────┐
                  ├→ T09 (CamZoom)      │
                  └→ T10 (Pool prewarm) │
                                        │
T04 (DealDamage signature) ─────────────┤
T05 (DamageDealer infra) ──→ T06 (8 IAttackPattern) ─┤
                                        ↓
                              T07 (EnemyController reflow) ──→ T08 (Floater fontSize)
                                        ↓
                              T11 (NUnit) ──→ T12 (docs)
```

**병렬 가능**:
- T01 후 — T02, T03, T09, T10 4개 동시 가능
- T04, T05 — T01 과 독립 (signature 변경만)
- T06 — T05 후 (DamageDealer 의 새 인자 사용)
- T07 — T01+T03+T04+T06 모두 완료 후
- T08 — T07 와 같은 사이클 가능 (별 commit)

**순차**:
- T11 → T12 (test pass 후 docs)

## 3. 4 레이어 분포

- **Data (SO)**: 1 (T01 CinematicEffectsConfig)
- **Logic (Combat/Equipment)**: 7 (T03~T10)
- **UI/Floater**: 1 (T08)
- **Editor (Seed)**: 1 (T02)
- **Test**: 1 (T11)
- **Docs**: 1 (T12)

## 4. CLAUDE.md §15 정합 사전 검증

| 태스크 | §15 정합 |
|---|---|
| T01 CinematicEffectsConfig ~80L | §15.4 SO 우선 ✅ / §15.6 ✅ |
| T03 HitEffectResolver ~70L 정적 | §15.1 SOLID-S ✅ / §15.7 DI ✅ / §15.11 struct (no boxing) ✅ |
| T04 DealDamage signature | optional 인자 (default null) — 호출처 자동 호환 |
| T05~T06 DamageDealer + 8 IAttackPattern | optional 인자 마이그레이션 — 점진 |
| T07 EnemyController reflow | hardcode 0 (모두 SO/resolved 참조) — §15.4 정합 ✅ |
| T09 CinematicEffects | Camera.main null 체크 + cache (정적 _camera) |
| T10 Pool prewarm | GC <50KB 보장 — §15.11 정합 ✅ |
| T11 NUnit 8 | §15.9 Test ✅ |

## 5. Unity MCP 안전 전략

- 한 사이클 = 1 태스크 + commit + 5~10s 도메인 리로드 대기
- T07 (EnemyController reflow) — 큰 변경, 사이클 분리 commit
- T11 NUnit 실행 — refresh + test runner

## 6. 변경 이력

- 2026-05-04: 초기 작성 (Phase 3, v2 PRD 12 commit 분해)
