# 5015 — DOTween → LitMotion 마이그레이션 — Sprint PRD

> **출처**: `docs/prd/5015_litmotion_migration_v1_prd.md` (디자인) — 옵션 B (일괄)

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | LitMotion 패키지 설치 + LitMotionExtensions 헬퍼 | 1 | 30분 |
| 2 | UI Panel 영역 (Toast / GameOver / ChapterCleared) | 1 | 40분 |
| 3 | Currency UI + 기타 Panel 마이그레이션 | 1 | 40분 |
| 4 | VFX 영역 (HitEffect / DamageNumberFloater) | 1 | 35분 |
| 5 | DOTween 패키지 제거 + ProjectValidator 룰 | 1 | 25분 |
| 6 | NUnit + docs | 1 | 30분 |
| **총** | | **6** | **약 3.5시간** |

## 2. Sprint 명세

### Sprint 1 — LitMotion 설치 + 헬퍼

**작업**:
- Package Manager → LitMotion 설치 (또는 manifest.json `com.annulusgames.lit-motion`)
- `Scripts/Core/LitMotionExtensions.cs` (~50L) — DOTween-like API wrapper
  - `DOMove`, `DOFade`, `DOScale` etc. extension methods → LMotion.Create + Bind

**검증**: 컴파일 errors=0, LitMotion namespace 정상 참조

#### Sprint 1 commit
```
5015 Sprint 1 / T01 — LitMotion 패키지 + 헬퍼 (1/6)

[변경]
- Packages/manifest.json — com.annulusgames.lit-motion 추가
- Scripts/Core/LitMotionExtensions.cs 신규 (~50L) — DOTween-like API wrapper

[검증]
- 컴파일 errors=0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 2 — UI Panel 영역

**대상** (예상):
- WaveTransitionToast / GameOverPanel / ChapterClearedPanel
- DOFade / DOScale → LitMotion 또는 wrapper

**검증**: 컴파일 + Play Mode 진입 시 fade-in 정상

#### Sprint 2 commit
```
5015 Sprint 2 / T02 — UI Panel 마이그레이션 (2/6)

[변경]
- WaveTransitionToast / GameOverPanel / ChapterClearedPanel — DOTween → LitMotion (or wrapper)

[검증]
- Play Mode fade-in 정상

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 3 — Currency UI + 기타 Panel

**대상**:
- AtticCurrencyView (4 화폐 fade)
- HUDController / WondrousMapPanel / GachaResultPanel etc.

#### Sprint 3 commit
```
5015 Sprint 3 / T03 — Currency UI + 기타 Panel (3/6)

[변경]
- AtticCurrencyView 4 화폐 fade — LitMotion
- HUDController / WondrousMapPanel / GachaResultPanel — LitMotion

[검증]
- 화폐 fade 정상

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 4 — VFX 영역

**대상**:
- CinematicEffects (punch scale 1.4× / 1.7×)
- DamageNumberFloater (floating + fade)
- VfxPool.SpawnHitStar (alpha + scale)

#### Sprint 4 commit
```
5015 Sprint 4 / T04 — VFX 영역 (4/6)

[변경]
- CinematicEffects punch scale — LitMotion
- DamageNumberFloater 1.2K/15K/1.5M 압축 + fade — LitMotion
- VfxPool.SpawnHitStar — LitMotion

[검증]
- VFX 시각 정상 (punch scale + floater + star)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 5 — DOTween 제거 + Validator

**작업**:
- Packages/manifest.json — DOTween 제거 (또는 Demigiant.DOTween)
- ProjectValidator 룰 — `using DG.Tweening;` 검출 시 경고

#### Sprint 5 commit
```
5015 Sprint 5 / T05 — DOTween 제거 + ProjectValidator (5/6)

[변경]
- Packages/manifest.json — DOTween 의존성 제거
- ProjectValidator DOTween_Import_Removed 룰 (using DG.Tweening 검출 시 경고)

[검증]
- 컴파일 errors=0 (모든 DOTween call site 마이그레이션 완료)
- Validator warnings 0건

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 6 — NUnit + docs

**Test**:
- `Tests/Editor/LitMotionExtensionsTests.cs` — wrapper 동등성 검증 (5+ NUnit)

**docs**:
- changelog + CLAUDE.md §10 (외부 라이브러리 갱신) + §11/§12/§14

#### Sprint 6 commit
```
5015 Sprint 6 / T06 — NUnit + docs (6/6)

[변경]
- Tests/Editor/LitMotionExtensionsTests.cs (5 NUnit)
- changelog + CLAUDE.md §10 / §11/§12/§14

[검증]
- 5/5 PASS, 누적 NUnit 91→96
- CLAUDE.md §10 외부 라이브러리 — DOTween 제거 / LitMotion 추가

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
T01 (인프라) → T02 (UI) → T03 (Currency) → T04 (VFX) → T05 (제거) → T06 (Test+docs)
```

T02/T03/T04 병렬 가능 (영역 독립). 다만 컴파일 통합 위해 순차 권장.

## 4. 검증 시나리오

### S1 — UI 애니메이션
1. Attic → 캐릭터 선택 → CH 1 진입 → ChapterCleared 토스트
- **검증**: 모든 UI fade/scale 정상

### S2 — VFX 시각
1. Play Mode wave 1 → 적 처치
- **검증**: punch scale 1.4× + DamageNumberFloater + HitStar 정상

### S3 — Profiler GC
1. cap 300 환경 진입
- **검증**: GC allocation/sec < 50KB (PIVOT v4 §7), DOTween 대비 ~5× 절감

## 5. Definition of Done

- [ ] 6 commit + 컴파일 errors=0
- [ ] DOTween 의존성 제거
- [ ] LitMotion 모든 call site 적용
- [ ] NUnit 5/5 PASS (누적 96)
- [ ] Profiler GC <50KB/s

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Sprint 5 DOTween 제거 시 잔여 import | Validator 룰 자동 검출 |
| LitMotion API 학습 곡선 | LitMotionExtensions wrapper |
| Burst Compiler 의존성 | LitMotion v2+ Unity 6.3 호환 검증 |

## 7. 변경 이력

- 2026-05-04: v1 작성.
