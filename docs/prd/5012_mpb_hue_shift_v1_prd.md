# 5012 — MaterialPropertyBlock Hue Shift 다양성 v1 (디자인)

> **목적**: 30 archetype sprite 를 런타임 MaterialPropertyBlock(MPB) hue shift 로 5단계 변형 → 30 → 150 시각 변형. 자산 추가 0 + GPU 비용 미미 + SRP Batcher 호환.
> **우선순위**: 보통 (chapter 별 시각 다양성 ↑, 사용자 retention 영향).

## 0. 결정 이력

- 2026-05-04: 5008 종결 시 후속 anchor 등록 (이전 5009 → 재부여 5012)

## 1. 현재 시스템 분석

### 1.1 적 sprite 분포

- 30 archetype × 1 색상 = 30 변형
- Chapter 1~35 가 같은 archetype 재사용 → 같은 색상 반복 (시각 단조로움)

### 1.2 MaterialPropertyBlock 패턴 (Unity)

- SpriteRenderer 의 material 인스턴스 생성 회피 (메모리 + draw call 절감)
- `renderer.SetPropertyBlock(mpb)` — per-instance 색상 변동, 같은 material 공유
- SRP Batcher 호환 (PIVOT v4 §7 draw call <10/frame 보존)

### 1.3 SpriteRenderer 와 MPB

- Unity Sprite shader 의 `_Color` 또는 custom `_HueShift` property
- 기본 Sprite-Default shader 는 _Color tint 만 — hue shift 는 custom shader 필요
- 또는 **HSV color shift** 를 PreLoad 에서 SpriteRenderer 의 color 직접 적용 (간단)

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | Custom shader (HueShift property) | 완전한 hue rotation | 셰이더 작성 + URP 호환성 |
| **B (★권장)** | **HSV → SpriteRenderer.color 변환 (런타임 hue offset)** | 단순, GC 0, 셰이더 X | 일부 sprite 색상 변동 X (white 등) |
| C | 텍스쳐 사전 생성 (asset 30 → 150 PNG) | 정확 | 자산 5× ↑, 메모리 ↑ |

> 권장 **B** — 단순 + 성능 + 셰이더 의존 0.

## 3. 권장 설계

### 3.1 신규 파일

- `Scripts/Gameplay/Enemies/EnemyHueVariation.cs` (~70L)
  - `static Color HueShift(Color baseColor, float hueOffset)` 정적 헬퍼
  - HSV 변환 + hue offset (-0.5 ~ +0.5 = 360도 회전)
  - 5단계 hue offsets: { -0.20f, -0.10f, 0.0f, +0.10f, +0.20f }

### 3.2 EnemyController hook

- `EnemyController.Initialize(EnemyData data, int variationIndex)` 추가
- `_spriteRenderer.color = EnemyHueVariation.HueShift(data.tintColor, OFFSETS[variationIndex])`
- variationIndex = `wave_number % 5` (wave 1, 6, 11... 같은 hue, 다양성 ↑)

### 3.3 EnemySpawner / WaveManager 갱신

- spawn 시 variationIndex 계산 (현재 wave + spawn order)
- Initialize 호출 시 전달

### 3.4 Boss 예외

- BossTier != None 시 variationIndex = 0 (원색 유지) — 보스 시각 일관성

## 4. Sprint 분해

| Sprint | 작업 | commit |
|---|---|---|
| 1 | EnemyHueVariation 정적 헬퍼 + HSV 변환 + Test | 1 |
| 2 | EnemyController.Initialize variationIndex 추가 + spawn hook | 1 |
| 3 | WaveManager / EnemySpawner variationIndex 계산 + 전달 | 1 |
| 4 | Editor Test (HueShift NUnit 5+) + ProjectValidator 룰 | 1 |
| 5 | docs (changelog + CLAUDE.md) | 1 |
| **총** | | **5** |

## 5. CLAUDE.md §15 정합

- §15.1 SRP — EnemyHueVariation 단일 책임 ✓
- §15.4 SO 우선 — variation offsets const 정의 (최소)
- §15.6 100L — ~70L ✓
- §15.7 DI — 정적 헬퍼 ✓
- §15.9 Test — NUnit 5+ 명시
- §15.11 GC — Color struct (no boxing)

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| white sprite 의 hue shift 효과 X | 디자인 시 white 영역 보존 + saturation only 변형 옵션 |
| Boss tier 시각 약화 | BossTier != None 시 variation X (예외 처리) |
| 5단계 변형 부족 (30 → 150) — 사용자 인지 어려움 | 추후 7단계 확장 고려 (5012.1) |

## 7. 변경 이력

- 2026-05-04: v1 작성.
