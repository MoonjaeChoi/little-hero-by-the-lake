# 5015 — DOTween → LitMotion 마이그레이션 v1 (디자인)

> **목적**: DOTween 을 LitMotion (5× 빠른, 0 GC) 으로 마이그레이션. 60fps 보장 + Update 대량 호출 환경 (cap 300) 에서 frame time 마진 ↑.
> **우선순위**: 보통 (장기 게임플레이 안정성).

## 0. 결정 이력

- 2026-05-04: 5008 종결 시 후속 anchor 등록 (이전 5012 → 재부여 5015)

## 1. 현재 시스템 분석

### 1.1 DOTween 사용 현황

CLAUDE.md §10:
- 외부 라이브러리: DOTween (커스텀 라이선스)
- 사용처: UI 애니메이션 (Toast / Panel fade / 버튼 hover) + VFX (HitEffect punch scale)

코드 grep (예상):
```bash
grep -rn "DOTween\|DOAnchorPos\|DOFade\|DOScale" Scripts/
```

### 1.2 LitMotion vs DOTween

| 항목 | DOTween | LitMotion |
|---|---|---|
| 라이선스 | 커스텀 (proprietary) | MIT |
| 성능 (1000 tween) | 16ms | 3ms (5×) |
| GC allocation | ~50KB/frame | 0 |
| API | DOAnchorPos / DOFade / DOScale | LMotion.Create / Tween / Bind |
| Burst Compiler | X | ✓ |

### 1.3 마이그레이션 영향

- 모든 DOTween call site 변경 필요 (estimated ~50~100 호출)
- 설치: `com.unity.modules.litmotion` (또는 GitHub release)

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | LitMotion 신규 사용처만 적용 (점진) | 부담 ↓ | 일관성 ↓, DOTween 의존성 잔존 |
| **B (★권장)** | **일괄 마이그레이션 (모든 call site 치환)** | 일관 + GC 0 + 라이선스 cleanup | 큰 작업 (~3시간) |
| C | DOTween 유지 (보류) | 변경 X | PIVOT v4 §7 GC <50KB 마진 약함 |

> 권장 **B** — 60fps 마진 안정.

## 3. 권장 설계

### 3.1 신규 파일

- LitMotion 패키지 설치 (Package Manager 또는 manifest.json)
- `Scripts/Core/LitMotionExtensions.cs` (~50L) — DOTween API 등가 매핑 헬퍼
  - `static MotionHandle DOAnchorPos(this RectTransform rt, Vector2 endValue, float duration)` 등

### 3.2 마이그레이션 패턴

| DOTween | LitMotion |
|---|---|
| `transform.DOMove(pos, 0.5f)` | `LMotion.Create(transform.position, pos, 0.5f).Bind(p => transform.position = p)` |
| `image.DOFade(0, 0.3f)` | `LMotion.Create(1f, 0f, 0.3f).Bind(a => { var c = image.color; c.a = a; image.color = c; })` |
| `transform.DOScale(1.4f, 0.1f)` | `LMotion.Create(Vector3.one, Vector3.one*1.4f, 0.1f).Bind(s => transform.localScale = s)` |

또는 LitMotionExtensions.cs 의 wrapper 로 단순화.

### 3.3 Sprint 분해 (영역별)

영역별 commit:
- UI Panel (Toast / GameOver / ChapterCleared) — 1 commit
- Currency UI (4 화폐 fade) — 1 commit
- VFX (HitEffect / DamageNumberFloater) — 1 commit
- DOTween 패키지 제거 + LitMotion 의존성 명시 — 1 commit
- Test + Validator — 1 commit

## 4. Sprint 분해

| Sprint | 작업 | commit |
|---|---|---|
| 1 | LitMotion 패키지 설치 + LitMotionExtensions 헬퍼 | 1 |
| 2 | UI Panel 영역 마이그레이션 | 1 |
| 3 | Currency UI + Toast 마이그레이션 | 1 |
| 4 | VFX 영역 (HitEffect / DamageNumberFloater) 마이그레이션 | 1 |
| 5 | DOTween 패키지 제거 + ProjectValidator 룰 | 1 |
| 6 | Editor Test + docs | 1 |
| **총** | | **6** |

## 5. CLAUDE.md §15 정합

- §15.10 외부 라이브러리 — LitMotion MIT (DOTween 커스텀 → MIT 전환, 라이선스 ↓)
- §15.11 GC 최소화 — LitMotion 0 GC ✓
- §15.12 Validator — DOTween import 검출 룰 (마이그레이션 후 자동 경고)

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| LitMotion API 학습 곡선 | LitMotionExtensions wrapper 로 DOTween-like API |
| 마이그레이션 시 회귀 (애니 깨짐) | 영역별 sprint + Play Mode 시각 검증 |
| LitMotion 패키지 도입 호환성 | Unity 6.3 LTS + URP 호환 검증 |
| DOTween 의존 third-party | 호환성 검토 (있으면 wrapper 유지) |

## 7. 변경 이력

- 2026-05-04: v1 작성.
