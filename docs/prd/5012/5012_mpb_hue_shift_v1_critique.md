# 5012 — MPB Hue Shift PRD 리뷰

## 0. 결정 이력
- 2026-05-04: prd-critiquer 자동 리뷰 (Phase 2.1)

## 1. 종합 점수: 8.4 / 10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | 정적 헬퍼 + 100L 통과. OFFSETS magic number 일부 (Discuss) |
| PG-12 톤 | 10/10 | 색상 변형 — 무관 |
| 성능 (PIVOT v4 §7) | 9/10 | SpriteRenderer.color 1회 (Initialize), cap 300 무관. Mathf.Sin/Cos 1회 |
| 시스템 통합 | 7/10 | ⚠️ EnemyController:196 hue 적용 위치 모호 + flash _baseColor 회귀 위험 (Discuss) |
| Sprint 분해 | 8/10 | T01→T05 순차 명확. T03 갱신 대상 (WaveManager vs EnemySpawner) 모호 (Discuss) |

## 2. Accept 항목 (5)

- ✅ **EnemyHueVariation 정적 헬퍼** — §15.1 SRP / §15.7 DI 정합 (생성자 X, 정적 메서드만)
- ✅ **HSV 변환 + 5 OFFSETS** — §15.6 100L 통과 (~70L)
- ✅ **Boss 예외 처리** — BossTier != None 시 variationIndex=0 (시각 일관성 유지) — 사용자 디자인 의도 정합
- ✅ **NUnit 6** — HueShift / VariationIndex / OFFSETS 단위 테스트 (§15.9)
- ✅ **GC 0** — Color struct, Mathf 정적, no LINQ (§15.11)

## 3. Reject 항목 (0)
- (없음 — §15 위반 또는 PG-12 위반 없음)

## 4. Discuss 항목 (3)

### 🟡 D1. Hue shift 대상 — `data.tintColor` vs `SpriteRenderer.color` tint

**Critical**. PRD §3.2 는 `_spriteRenderer.color = HueShift(data.tintColor, OFFSETS[i])` 명시.

문제:
- `EnemyData.tintColor = Color.white` (default — 5008 sprite 가 자체 색상을 가지는 PNG 라 tintColor 무관)
- 현 EnemyController.cs:196 — `sr.color = data.icon != null ? Color.white : data.tintColor;` (icon 있으면 white tint, 없으면 placeholder 원형 tintColor)
- 5008 LayerDiffuse RGBA sprite 30 archetype 모두 `data.icon != null` → tintColor 무시 → **HueShift 효과 0**

**옵션**:
- A: PRD 그대로 (`data.tintColor` hue shift) — 5008 sprite 영향 X (tintColor 가 white 라 white 의 hue shift 도 white)
- B (★권장): **SpriteRenderer.color tint** 직접 hue offset (Color.white → HueShift(Color.white, offset) 결과 색상으로 multiplicative tint). 5008 sprite 픽셀에 곱연산 → 색상 다양화 가능. EnemyController:196 라인 갱신.
- C: Custom shader (디자인 옵션 A 였던 것) — URP 호환성 부담

### 🟡 D2. OFFSETS const → SO 화

**Medium**. PRD §3.1 `OFFSETS = { -0.20, -0.10, 0.0, +0.10, +0.20 }` const.

§15.4 SO 우선 정책상 디자이너 후속 조정 가능성:
- A: const 유지 (단순, 변경 시 코드 수정)
- B (★권장): `BalanceConfig` 에 `enemyHueOffsets float[5]` 추가 (디자이너 Inspector 조정)
- C: `EnemyVariationConfig` 신규 SO (over-engineered)

### 🟡 D3. EnemyController.\_baseColor 캐시 회귀

**High** (발견 시점에서 즉시 검토 필요).

EnemyController.cs:68 `_baseColor = _sr != null ? _sr.color : Color.white;` — flash 효과 (line 174~184) 가 `_baseColor` 로 복귀.

PRD 가 hue shift 를 Initialize 후 적용 시 `_baseColor` 캐시 시점:
- 옵션 A: hue shift 전 (현재 코드) — flash 후 white 로 복귀 (hue shift 풀림)
- 옵션 B (★권장): hue shift 후 (\_baseColor 갱신 후 hue 적용) — flash 후 hue 색상 유지
- 옵션 C: \_baseColor 자체를 hue shift 결과로 캐시

PRD 명시 X — 구현 시 회귀 위험. PRD §3.2 "기존 시스템 hook" 에 명시 필수.

## 5. CLAUDE.md §15 정책 위반 점검

- §15.1 SRP — EnemyHueVariation 단일 책임 ✅
- §15.4 SO 우선 — OFFSETS const → SO 권장 (Discuss D2)
- §15.6 100L — ~70L ✅
- §15.7 DI — 정적 헬퍼, Singleton 직접 참조 X ✅
- §15.9 Test — 6 NUnit 명시 ✅
- §15.11 안티 패턴 — Color struct (no boxing), Mathf 정적, public 필드 X ✅

## 6. 권장 변경

**즉시 변경 (Accept)**:
- (없음 — 디자인은 견고. 구현 시 정합)

**사용자 결정 후 변경 (Discuss)**:
- D1: 옵션 B 채택 시 PRD §3.2 hue shift 적용 라인 수정 (`SpriteRenderer.color` 직접)
- D2: 옵션 B 채택 시 BalanceConfig 필드 추가 + Sprint 1 작업 항목 +SO wiring
- D3: 옵션 B 채택 시 PRD §3.2 \_baseColor 갱신 명시

## 7. 변경 이력
- 2026-05-04: 초기 작성 (Phase 2.1 Critique).
