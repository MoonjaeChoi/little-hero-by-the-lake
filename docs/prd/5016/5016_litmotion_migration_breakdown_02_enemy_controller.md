# T02: EnemyController PunchRoutine + FlashRoutine 마이그레이션

> **레이어**: Logic (핫 패스)
> **의존성**: T01
> **예상 시간**: 40분 / 1 commit
> **PRD 출처**: `5016_litmotion_migration_v2.md` §2 Sprint 2

## 1. 작업 항목

### 1.1 EnemyController.cs 수정

**파일**: `LittleHero/Assets/_Project/Scripts/Gameplay/Enemies/EnemyController.cs`

**변경 1 — PunchRoutine → DoPunch**:
- `IEnumerator PunchRoutine` (line ~151~169) 폐기
- `MotionHandle _punchHandle` 필드 신규
- `DoPunch(targetScale, expandTime, returnTime)` 메서드 신규 — `LSequence.Create().Append(...).Append(...).Run()`
- `_punchHandle.IsActive() && Cancel()` 가드

**변경 2 — FlashRoutine → DoFlash**:
- `IEnumerator FlashRoutine` (line ~171~186) 폐기
- `MotionHandle _flashHandle` 필드 신규
- `DoFlash(flashColor, holdTime, returnTime)` 메서드 — `LMotion.Create(0f, 1f, returnTime).WithDelay(holdTime).Bind(k => _sr.color = Color.Lerp(flashColor, _baseColor, k))`
- 5012 D3=B 정합 — `_baseColor` 복귀 명시

**변경 3 — OnHit 호출 갱신**:
- `StartCoroutine(_punchCo)` → `DoPunch(...)`
- `StartCoroutine(_flashCo)` → `DoFlash(...)`
- `_punchCo` / `_flashCo` 필드 폐기

**예상 라인**: ~30L 추가 / ~40L 삭제 (순감 ~10L)

## 2. 검증 기준

### 2.1 컴파일
- errors=0, 5초 대기 (도메인 리로드)

### 2.2 Play Mode (수동)
- CH 1 wave 1 → 적 hit
- punch scale 1.4× (크리 1.7×) 시각 동일
- flash 색상 적용 + _baseColor 복귀 정상 (5012.1 hue 색상 유지)

### 2.3 회귀
- 기존 5012 hue tint 동작 유지
- 기존 5013 visibility culler 동작 유지

## 3. commit 메시지

(PRD §2 Sprint 2 commit 메시지 그대로)

## 4. 관련 문서

- PRD §2 Sprint 2
- 디자인 §3.3 EnemyController.PunchRoutine 마이그레이션 예시

## 5. 알려진 위험

- punch scale 1.4× 정확 — `LSequence.Append × 2` 의 timing 검증 필수
- flash 후 _baseColor 복귀 — 5012 D3=B Color.Lerp 키 정확 검증
