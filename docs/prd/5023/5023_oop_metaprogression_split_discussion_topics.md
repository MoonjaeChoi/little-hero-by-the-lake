# 5023 PRD Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 결정 필요.
> 합계: **3** (High 1, Medium 2)

---

## High Priority Items

### D1. 호출처 마이그레이션 전략 (30+)

- **옵션 A** (PRD 원안 옵션 B): 즉시 마이그레이션 — 호출처 일괄
  - 장점: 깔끔, deprecated 0
  - 단점: 30+ 한꺼번에, 누락 위험
- **옵션 B**: deprecated proxy 보존 — MetaProgression 이 service 호출 중계
  - 장점: 점진 마이그레이션, 위험 ↓
  - 단점: deprecated 코드 잔존 (5023.1 cleanup)
- **옵션 C** ★ Claude 권장: 즉시 + Sprint 별 분리 (Sprint 2 = Character/Chapter, Sprint 3 = Currency/Equipment, Sprint 4 = Endgame)
  - 장점: 깔끔 + 위험 격리

---

## Medium Priority Items

### D2. SaveSystem.Save 호출 위치

- **옵션 A** (PRD 원안): 각 service 메서드가 직접 호출 (분산)
  - 장점: 즉시 영속, 명시적
  - 단점: 누락 위험
- **옵션 B**: dirty flag + MetaProgression.Save() 중앙 호출
  - 장점: 누락 0
  - 단점: 명시적 Save() 호출 필요 (실수)
- **옵션 C** ★ Claude 권장: PRD 원안 + ProjectValidator 룰 (service Save 호출 검증)
  - 장점: 즉시 영속 + 자동 회귀 안전망

### D3. OnCharacterChanged event 위치

- **옵션 A** (PRD 원안): service 클래스로 이동 — `MetaProgressionCharacter.OnCharacterChanged`
  - 장점: 책임 일관
  - 단점: 구독자 호출처 변경
- **옵션 B** ★ Claude 권장: MetaProgression 정적 event 보존, service 가 발행
  - 장점: 구독자 변경 0 (instance → 정적만)
  - 단점: 책임 분산 (정의 vs 발행)
- **옵션 C**: GameEvents 정적 EventBus 로 이동
  - 장점: §15.5 일관 패턴
  - 단점: 구독자 일괄 마이그레이션

---

## 요약

- Total: **3** (High 1, Medium 2)
- 권장 답변: **D1=C, D2=C, D3=B** — 위험 분산 + 호환성 우선

---

## mediator 결정 입력

```
"권장 그대로 진행"     ← D1=C, D2=C, D3=B 일괄
"D1=A, D2=A, D3=A"     ← 모두 PRD 원안
"D1=B, D2=A, D3=B"     ← 부분
```
