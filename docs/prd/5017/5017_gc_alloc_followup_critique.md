# 5017 GC Alloc Follow-up PRD 리뷰

## 0. 결정 이력
- 2026-05-06: Claude Code (자동) Phase 2.1 Critique

## 1. 종합 점수: 8.8/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | struct 변환 + cache 패턴 정합. Validator 휴리스틱은 false positive 위험 |
| PG-12 톤 | 10/10 | 무관 영역 |
| 성능 | 9.5/10 | Player 측정 70→<30 KB/s 목표, hot path 정밀 식별 |
| 시스템 통합 | 8.5/10 | DropContext struct 변환은 호출처 검증 위험 |
| Sprint 분해 | 8/10 | T03 wobble cache 효과 검증 어려움, 5017.1 가능성 ↑ |

종합 평가: **PRD 품질 양호**. Mac Player 측정 결과 (~70 KB/s) 기반 정밀 hot path 식별. Discuss 3건 사용자 결정 대기.

## 2. Accept 항목 (8)

- ✅ **DamageNumberFloater.FormatDamage cache** — `Dictionary<(int, DamageType), string>` + StringBuilder 정적 + cap 1024 누수 회피
- ✅ **AudioManager BGM warning string concat** — $"" → "+ name" (작은 절감)
- ✅ **DropContext class → struct** — heap alloc 폐기, by-value 전달
- ✅ **EnemyController wobble 변동 임계 setter** — native interop 절감
- ✅ **5 Sprint 분해** — T01~T03 영역 독립 + T04 검증 + T05 측정/종결
- ✅ **NUnit 4 신규** (108 누적)
- ✅ **GCAllocMeasurer auto trigger 활용** — Sprint 5 자동 측정
- ✅ **CLAUDE.md §15 정합** — §15.1 SRP / §15.4 SO 우선 / §15.7 DI

## 3. Reject 항목 (0)

(없음 — §15 위반 / PG-12 톤 위반 / 성능 회귀 0건)

## 4. Discuss 항목 (3)

### D1. FormatDamage cache 의 장기 메모리 누수

- **현재 PRD**: `_formatCache` Dictionary, cap 1024 (LRU 미적용)
- **위험**: 무한 다른 damage 값 (cap 300 환경 매 hit 다른 값) 발생 시 cache 가 1024 채워진 후 stop 누적. 그러나 stop 시점 저장된 1024 값이 **항상 메모리 상주**
- **옵션**:
  - **A (★권장)**: 현재 PRD 그대로 (cap 1024, ~30 KB 메모리 — 무시 가능)
  - **B**: LRU 캐시 (System.Collections.Generic.Dictionary + LinkedList) — alloc 추가 가능
  - **C**: cache 폐기, StringBuilder 재사용만 (매 hit 1 string alloc — 부분 절감)

### D2. DropContext class → struct 호출처 안전성

- **현재 PRD**: 호출처 grep 으로 `new DropContext { ... }` 모두 갱신
- **위험**: `out DropContext` / `ref DropContext` / `DropContext` field 가 다른 곳에 저장되어 있으면 컴파일 변동 가능. struct 가 by-value 전달되어 변경 사항 lost 가능
- **옵션**:
  - **A (★권장)**: struct 변환 + 호출처 grep + 컴파일 errors 검증으로 안전 보장
  - **B**: 일부 영역 (DropContext field 저장 영역) 만 struct 유지 — 복잡
  - **C**: class 그대로, ObjectPool<DropContext> 도입 (재사용)

### D3. EnemyController wobble 임계 0.001f

- **현재 PRD**: `Mathf.Abs(wobble - _lastWobble) > 0.001f` 시만 setter
- **위험**: `_lastWobble` cache 매 frame 갱신 시 사실상 모든 frame setter 호출 (wobble 매 frame 미세 변동). 효과 작음
- **옵션**:
  - **A**: 0.001f (현재 PRD)
  - **B (★권장)**: 0.005f (5배 큰 임계) — 시각 차이 미미 + 효과 ↑
  - **C**: setter 호출 매 frame 그대로 (T03 폐기)

## 5. CLAUDE.md §15 정책 위반 점검

| 정책 | 검증 결과 |
|---|---|
| §15.1 SRP | ✅ FormatDamage = 단일 책임 cache + format / DropContext = 단일 데이터 컨텍스트 |
| §15.4 SO 우선 | ✅ 무관 (코드 영역) |
| §15.6 100L | ✅ FormatDamage 메서드 ~25L |
| §15.7 DI | ✅ 정적 cache (singleton 직접 참조 X) |
| §15.10 외부 라이브러리 | ✅ 무관 |
| §15.11 GC 0 | ✅ struct + cache 패턴 |
| §15.12 Validator | ✅ ValidateGCAllocPatterns 휴리스틱 룰 신규 |

→ 모든 §15 정책 정합 ✅

## 6. PIVOT v4 §7 성능 목표 정합

| 목표 | 5016.1 후 | 5017 후 기대 |
|---|---:|---:|
| GC Alloc <50KB/s | ❌ 70 KB/s (Player) | ✅ <30 KB/s 도달 (시나리오 A) |
| 60fps | ✅ | ✅ |
| 적 cap 300 | ✅ | ✅ |

## 7. 권장 변경

**즉시 변경**: 없음 (모든 Accept 항목 그대로 진행 가능)

**사용자 결정 후 변경**:
- D1 결정 → FormatDamage cache 정책
- D2 결정 → DropContext 변환 방식
- D3 결정 → wobble 임계값 (0.001 / 0.005 / 0)

## 8. 변경 이력
- 2026-05-06: 초기 작성 (Phase 2.1 Critique)
