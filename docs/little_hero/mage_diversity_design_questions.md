# 마법사 다양한 공격 패턴 — 설계 결정 가이드

> **목적**: 마법사 직업 공격 패턴 다양화를 위한 PRD 작성 **전** 결정 항목 정리
> **상태**: 결정 보류 (사용자 후속 검토)
> **작성**: 2026-05-06
> **차후 anchor**: 5029_mage_diversity (예상 PRD 번호)

이 시스템은 데이터 모델·UI·VFX·밸런스 영향이 큰 작업이라 **PRD 작성 전 사용자 결정 6개**가 먼저 필요합니다.

## 현재 시스템 (선결 분석)

| 항목 | 값 |
|---|---|
| 직업 | 3종 (전사 STR / 궁수 AGI / 마법사 WIS 2.0×) |
| 마법사 무기 (weapon_v2) | 8종 — Staff(지팡이) / Wand(완드) / Tome(완본) |
| 공통 attack pattern | 8 IAttackPattern (Projectile/Aura/Orbit/Rain/Trail/Pulse/Summon/Slash) |
| Element 시스템 | **없음** (속성 미존재) |
| 마법사 시너지 | Mage + Staff/Wand/Tome → +MagicDmg 10% |

→ 현재 마법사도 전사/궁수와 **같은 8 pattern** 공유. "다양화"의 정확한 의미를 좁혀야 합니다.

---

## ❶ 결정 1 — 다양화의 의미 (방향성)

| 옵션 | 설명 | 작업량 |
|---|---|---|
| **A** | 신규 attack pattern 추가 (8 → 12+) — Beam/Chain/Vortex/Meteor 등 | 中 |
| **B** | **Element 시스템** 도입 (Fire/Ice/Lightning/Arcane) — 같은 pattern 도 element 별 다른 동작 | 大 |
| **C** | **마법사 sub-class** (Pyromancer/Cryomancer/Storm/Arcane) — 직업 분기 | 매우 大 |
| **D** | **콤보/Chain 시스템** — 연속 시전 시 보너스 (메이플 패턴) | 中 |
| **E** | A+B 통합 (신규 pattern × element) | 매우 大 |

**핵심 질문**:
- "다양화" = **수직** (강도) 인가 **수평** (선택지) 인가?
- 메이플 키우기 / 뱀파이어 서바이버 어느 모델?

---

## ❷ 결정 2 — Element 시스템 도입 여부

만약 ❶에서 B/C/E 채택 시:

| 옵션 | 설명 |
|---|---|
| Element 4종 (Fire/Ice/Lightning/Arcane) | Ultimate Elemental Magic VFX Pack 정합 ★ |
| Element 5종 (+ Nature/Earth) | 5008 archetype 카테고리 정합 |
| Element X (Element 미도입, 시각 차이만) | 단순화, 기존 시스템 호환 |

**파급 효과**:
- ✅ 적 약점/저항 (Fire vs Ice 등) — 전략적 깊이
- ⚠️ EnemyData SO 262 모두 element resistance 추가 마이그레이션 필요
- ⚠️ Gacha 결과 element 표시 (UI 변경)

---

## ❸ 결정 3 — 대상 범위

| 옵션 | 설명 |
|---|---|
| **A** | **마법사만** — Sprint 작게, 전사/궁수 후속 |
| **B** | **3 직업 모두** 동시 — 직업 정체성 균형 |
| **C** | 마법사 우선 + 전사/궁수 후속 PRD |

전사/궁수도 다양화 필요한가? 아니면 **마법사가 가장 단조롭게 느껴지는가**?

---

## ❹ 결정 4 — VFX 전략

| 옵션 | 설명 | 비용 |
|---|---|---|
| **A** | Ultimate Elemental Magic VFX Pack ($28) 통합 | 빠름, 미려 |
| **B** | 자체 절차적 합성 (5007 HitEffectResolver 답습) | 무료, PG-12 톤 정밀 제어 |
| **C** | 무료 에셋 (Unity Particle Pack) + 자체 합성 혼합 | 중간 |
| **D** | 기존 VfxPool 만 재활용 (시각 차이 X) | 가장 빠름, 효과 약함 |

PG-12 톤 검수 — VFX Pack 의 사실적 Fire/Lightning 톤이 정합 우려.

---

## ❺ 결정 5 — 데이터 모델

| 결정 | 옵션 |
|---|---|
| Element enum 위치 | EnemyData / EquipmentTemplate / 둘 다 |
| Pattern × Element 조합 | Cartesian product (8×4=32) vs Strategy (Pattern 이 Element 받음) |
| SO 인스턴스 추가 | 신규 element 별 VFX prefab SO N 종 |
| 적 저항 | EnemyData.elementResistance[4] 추가 |

---

## ❻ 결정 6 — 밸런스 + 진행성

| 결정 | 옵션 |
|---|---|
| Element 잠금 해제 | 챕터 진행 (CH5 Fire / CH10 Ice 등) vs 시작부터 모두 |
| 가챠 영향 | 무기 가챠에 element 추가 vs Elemental Stone (별도 아이템) |
| 마법사 무기 8종 → element 매핑? | 기존 무기 element 부여 vs 신규 무기 추가 |

---

## 권장 시작 순서

```
Step 1 — ❶ 방향성 결정 (A/B/C/D/E 중)
   ↓
Step 2 — ❸ 대상 범위 (마법사만 vs 3 직업)
   ↓
Step 3 — ❷ Element 시스템 (B/C/E 채택 시)
   ↓
Step 4 — ❹ VFX 전략 (Pack 구매 vs 자체)
   ↓
Step 5 — ❺ 데이터 모델 + ❻ 밸런스
   ↓
Step 6 — /prd-generator 5029 (디자인 PRD 작성)
   ↓
Step 7 — /prd-pipeline 5029 (Phase 2~6)
```

---

## Claude 권장 프리셋

리스크 vs 효과 균형 권장:

```
❶ 방향성: B (Element 시스템) — "다양화" 핵심
❷ Element: 4종 (Fire/Ice/Lightning/Arcane) — VFX Pack 정합
❸ 범위: A (마법사만) — Sprint 작게, 전사/궁수 5030 후속
❹ VFX: B (자체 절차적) → A (Pack) 옵션 — PG-12 검수 후 결정
❺ 데이터: Element enum + Strategy 패턴 (Pattern 이 Element 받음)
❻ 밸런스: 챕터 진행 잠금 해제 + 무기 element 부여
```

---

## 후속 진행 시 입력 형식

결정 시:
```
방향성 = A / B / C / D / E
범위 = 마법사만 / 3 직업
VFX = Pack / 자체 / 혼합
Element = 4종 / 5종 / X
```

또는 **"권장 프리셋대로"** 발화 → 즉시 5029 PRD 작성 시작.

---

## 참조 문서

- `docs/little_hero/weapon_system_v2.md` — 직업 분류 + 24 무기
- `docs/prd/5001_boss_system.md` — IBossAttackPattern 7종 패턴 (Strategy)
- `docs/prd/5026_boss_attack_type_migration_prd.md` — 5 카테고리 매핑 (Animal=Charge / Civ=Pulse 등) → element 시스템 답습 가능
- Ultimate Elemental Magic VFX Pack: `assetstore.unity.com/packages/vfx/particles/spells/ultimate-elemental-magic-vfx-pack-286603`

## 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 작성 — 사용자 결정 보류, 차후 5029 PRD anchor |
