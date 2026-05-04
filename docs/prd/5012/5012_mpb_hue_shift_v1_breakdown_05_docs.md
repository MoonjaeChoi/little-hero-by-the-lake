# T05: docs (changelog + CLAUDE.md)

> **레이어**: Docs
> **의존성**: T04
> **예상 시간**: 15분 / 1 commit
> **PRD 출처**: 5012_mpb_hue_shift_v1_v2.md §2 Sprint 5

## 1. 작업 항목

### 1.1 changelog_v1.md 등재

**경로**: `docs/little_hero/changelog_v1.md`

신규 항목 (최상단 추가):
```markdown
## 5012 — mpb_hue_shift_v1 (D1=B / D2=A / D3=B)

- 일시: 2026-05-04
- 변경:
  - EnemyHueVariation 정적 헬퍼 신규 (~35L)
  - EnemyController.Initialize +variationIndex (default 0)
  - ApplyVisual: BossTier=None + variationIndex>0 시 HueShift 적용 (D1=B SpriteRenderer.color multiplicative tint)
  - _baseColor hue 결과 캐시 (D3=B flash 후 hue 색상 유지)
  - EnemySpawner.Spawn(data, variationIndex=0) 시그니처 + WaveManager.SpawnEnemy 7 prime spread
  - NUnit 6 + ProjectValidator 룰
- 누적 NUnit 87 → 93
- 5 commit (T01~T05)
- 출처: docs/prd/5012/5012_mpb_hue_shift_v1_v2.md
```

### 1.2 CLAUDE.md §11 마일스톤

`mpb_hue_shift_v1` 행 추가:
```markdown
| **mpb_hue_shift_v1** | EnemyHueVariation 5단계 hue 변형 + EnemyController hook + WaveManager 7 prime spread (5012, NUnit 87→93) | ✅ 2026-05-04 |
```

### 1.3 CLAUDE.md §12 시스템 현황

Tests/Editor 갱신:
- 87 → 93 NUnit
- (신규 EnemyHueVariation 6) 추가

### 1.4 CLAUDE.md §14 직전 5건 회전

5012 항목 최상단 추가, 가장 오래된 항목 (5005) 제거:
```markdown
- 2026-05-04: **5012 mpb_hue_shift_v1** — Enemy sprite 5단계 hue 변형 (D1=B SpriteRenderer.color tint / D2=A const / D3=B _baseColor 갱신, 5 commit, NUnit 87→93). EnemyHueVariation 정적 헬퍼 + EnemyController.ApplyVisual hook + WaveManager 7 prime spread. 5008 LayerDiffuse RGBA 30 archetype 시각 다양화 (30 → 150 변형)
- 2026-05-04: **5013 enemy_visibility_culler_v1** — ...
- ...
```

## 2. 검증 기준

### 2.1 markdown lint
- 컴파일 의존 X (docs only)

### 2.2 changelog 형식
- 5008 / 5013 형식 준수

## 3. commit 메시지

```
5012 Sprint 5 / T05 — docs (5/5) — 5012 종결

[변경]
- changelog_v1.md 5012 항목 추가
- CLAUDE.md §11 마일스톤 + §12 시스템 현황 + §14 직전 5건 회전

[5012 누적 5 commit]
- T01 EnemyHueVariation 정적 헬퍼
- T02 EnemyController hook + _baseColor 갱신
- T03 EnemySpawner + WaveManager variation 전달
- T04 NUnit 6 + Validator 룰
- T05 docs

[검증]
- D1=B / D2=A / D3=B Contract 정합
- NUnit 87→93
- 5008 LayerDiffuse 30 archetype 시각 다양화 (30 → 150)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 알려진 위험
- §14 5건 회전 — 가장 오래된 항목 제거 시 commit churn (정착 패턴 — OK)
