# 5012 — MPB Hue Shift PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 자동 적용
> **검토자**: Claude Code · 일시: 2026-05-04

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| 통합 | D1 Hue shift 대상 (tintColor vs SpriteRenderer.color) | High | Discuss | discussion_topics 분리 | △ 사용자 결정 |
| §15.4 | D2 OFFSETS const → SO | Medium | Discuss | discussion_topics 분리 | △ 사용자 결정 |
| 통합 | D3 \_baseColor 캐시 회귀 | High | Discuss | discussion_topics 분리 | △ 사용자 결정 |
| Sprint | T03 갱신 대상 모호 (WaveManager vs EnemySpawner) | Medium | Accepted | v2 명시 (WaveManager.SpawnEnemy 진입점) | ✅ |

## 2. 품질 메트릭

- 총 Discussion Points: 4
- Accepted: 1 (25%)
- Rejected: 0
- Discuss (사용자 결정): 3 (75%)
- §15 정합률: 100% (위반 0건)

## 3. Detailed Responses

### 3.1 [통합 — High] D1 Hue shift 대상

**Original Criticism**: PRD §3.2 `_spriteRenderer.color = HueShift(data.tintColor, ...)` — 5008 sprite 가 `icon != null` 라 tintColor=white → HueShift 효과 0.

- **Status**: Discuss → discussion_topics 이동
- **이유**: 디자인 의도 (sprite 픽셀 다양화 vs placeholder 원형 호환) 사용자 결정
- **Current Position**: PRD 그대로면 시각 다양성 효과 0
- **Resolution Path**: mediator 단계 옵션 A/B/C 중 결정

### 3.2 [§15.4 — Medium] D2 OFFSETS SO 화

**Original Criticism**: const → SO 권장 (디자이너 후속 조정).

- **Status**: Discuss
- **이유**: 단순성 vs 디자이너 친화 trade-off — 사용자 결정
- **Current Position**: const 유지 시 코드 수정 필요. SO 화 시 +5L wiring.
- **Resolution Path**: ai-assisted 권장 — const 유지 (5단계 고정, 변경 빈도 낮음, §15.4 예외 허용)

### 3.3 [통합 — High] D3 \_baseColor 회귀

**Original Criticism**: flash 효과 \_baseColor 복귀가 hue shift 풀림.

- **Status**: Discuss → discussion_topics 이동
- **이유**: 회귀 방지 시점 결정 (Initialize 순서)
- **Current Position**: PRD 명시 X — 구현 시 임의 결정 위험
- **Resolution Path**: 옵션 B (\_baseColor 갱신 후 hue 적용) 강력 권장 — flash 후 hue 색상 유지

### 3.4 [Sprint — Medium] T03 갱신 대상

**Original Criticism**: WaveManager vs EnemySpawner 어느 쪽 갱신?

- **Rebuttal**: Accepted — 코드 grep 결과 명확
- **Proposed Action**: WaveManager.SpawnEnemy → EnemySpawner.Spawn(data, variationIndex) 시그니처 변경. variationIndex 계산은 WaveManager 가 currentWave + spawn count 보유.
- **PRD Impact**: §3.3 + Sprint 3 작업 항목 명시 (EnemySpawner.Spawn 시그니처 변경)
- **Little Hero Context**: 5012 가 EnemySpawner.cs (~70L 추정 영향) + WaveManager.cs 함께 수정. 시그니처 호환은 default param `variationIndex = 0` 으로 회귀 방지.

## 4. PRD v2 적용 변경 요약

(Accepted 1건 만 v2 초안 반영, Discuss 3건은 mediator 후 최종 확정)

- §3.3 EnemySpawner.Spawn 시그니처 명시 + WaveManager 의 variationIndex 계산 명시
- Sprint 3 commit 메시지 EnemySpawner.cs 명시

## 5. 변경 이력
- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal).
