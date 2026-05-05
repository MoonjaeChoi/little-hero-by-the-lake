# T04: DamageNumberFloater + Boss attacks 5종 마이그레이션

> **레이어**: Logic (Floater = 핫 패스, Boss attacks = 콜드)
> **의존성**: T01
> **예상 시간**: 50분 / 1 commit
> **PRD 출처**: `5016_litmotion_migration_v2.md` §2 Sprint 4

## 1. 작업 항목

### 1.1 DamageNumberFloater.cs 수정 (핫 패스)

**파일**: `LittleHero/Assets/_Project/Scripts/Gameplay/Combat/DamageNumberFloater.cs`

**변경**:
- `Update()` 의 `Mathf.Lerp` 처리 → `LMotion.Create(start, end, duration).Bind(...)`
- `new Vector3` (line ~60-62) — start/end 1회 캡처 + Bind 내부 활용
- alpha fade — 별도 `LMotion`
- `Pool.Release(this)` — `WithOnComplete` 에서 호출

**예상 라인**: ~30L 변경

### 1.2 Boss attacks 5종 (콜드)

**파일**:
- `Scripts/Gameplay/Enemies/BossAttacks/BossVineAttack.cs`
- `Scripts/Gameplay/Enemies/BossAttacks/BossTornadoAttack.cs`
- `Scripts/Gameplay/Enemies/BossAttacks/BossSummonAttack.cs`
- `Scripts/Gameplay/Enemies/BossAttacks/BossSpreadAttack.cs`
- `Scripts/Gameplay/Enemies/BossAttacks/BossPulseAttack.cs`

**변경**: 각 파일 내 `Mathf.Lerp` / `Vector3.Lerp` / `Color.Lerp` 사용처 → `LMotion.Create + Bind`

**예상 라인**: 각 파일 ~5L 변경 / 5종 × 5L = ~25L

## 2. 검증 기준

### 2.1 컴파일
- errors=0, 5초 대기

### 2.2 Play Mode (수동)
- 적 hit 시 데미지 숫자 floater 출현
- 위로 떠오르며 fade 정상 (1.2K / 15K / 1.5M 압축 표시)
- Boss 공격 패턴 시각 동일 (vine 자라남 / tornado 회전 / spread / pulse)

### 2.3 회귀
- DamageNumberFloater Pool.Release 정확 (반환 누락 시 메모리 누수)
- Boss 공격 timing 정확 (Ease.Linear 일치)

## 3. commit 메시지

(PRD §2 Sprint 4 commit 메시지 그대로)

## 4. 관련 문서

- PRD §2 Sprint 4
- 디자인 §3.3

## 5. 알려진 위험

- DamageNumberFloater Pool.Release — `WithOnComplete` 정확 호출 검증 (빠진 경우 풀 고갈)
- Boss attacks Lerp 시각 차이 — `Ease.Linear` 명시 필수
