# 5017 — Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 결정 필요 3 항목.

## Medium Priority Items

### D1. FormatDamage cache 메모리 누수 회피

- **위험**: cache cap 1024 + LRU 미적용. 무한 다른 damage 값 시 1024 stop 누적, 메모리 ~30 KB 상주.
- **옵션**:
  - **A (★권장)**: 현재 PRD 그대로 (cap 1024) — 30 KB 무시 가능
  - B: LRU 도입 (Dictionary + LinkedList) — 추가 코드 복잡도
  - C: cache 폐기, StringBuilder 만 (매 hit 1 alloc)
- **권장**: A — 30 KB 영구 비용 vs 매 hit alloc 절감 — 절감 가치 큼

### D2. DropContext class → struct 호환성

- **위험**: `ref` / field 저장 영역 컴파일 errors 가능. by-value 변경 lost.
- **옵션**:
  - **A (★권장)**: struct 변환 + 호출처 grep + 컴파일 검증
  - B: 부분 struct (복잡)
  - C: class + ObjectPool<DropContext> (재사용 패턴)
- **권장**: A — 단순한 변환 + 컴파일 검증으로 안전 보장. 위험 발견 시 C 폴백

### D3. EnemyController wobble 임계값

- **위험**: 0.001f 임계는 매 frame 변동 검출 (효과 미미)
- **옵션**:
  - A: 0.001f (현재 PRD)
  - **B (★권장)**: 0.005f (5배 큰 임계, 효과 ↑)
  - C: setter 매 frame 그대로 (T03 폐기)
- **권장**: B — 시각 차이 미미 + native interop 절감 효과 ↑

## 권장 일괄 (사용자 "권장안대로" 시)

| Item | Recommendation | Rationale |
|---|---|---|
| D1 | A cap 1024 | 30 KB 영구 비용 < 매 hit 절감 |
| D2 | A struct 변환 | 컴파일 검증 + 안전 |
| D3 | B 0.005f | 효과 ↑ 시각 동일 |
