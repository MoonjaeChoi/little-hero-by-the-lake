# 5026 PRD Discussion Topics

> Phase 2.2 · mediator 결정 필요. 합계 **3**

## D1. V1 11종 분류 (Medium)

V1 fairytale 11종 (BeanGiant/Bear/BigWolf/CardSoldier/CloudGiant/Firefly/Pumpkin/QueenHearts/Rabbit/StarCloud/Basic) 모두 Meta(Summon)?

- **A**: 모두 Meta(Summon) — V1 = CH1~3 (Meta) 소속 정합
- **B** ★: 동물형(Bear/BigWolf/Rabbit) → Animal(Charge), 식물/사물형(BeanGiant/CloudGiant/Pumpkin) → Nature(Vine), 카드(QueenHearts/CardSoldier) → Civ(Pulse), Star/Firefly/Basic → Meta(Summon)
- **C**: 모두 Charge fallback (단순화)

★ Claude 권장: B — 캐릭터 시각 정합 우선 (5008 V1 sprite 분류 답습)

## D2. CategoryMap SO 화 (Low)

CategoryMap (5 카테고리 → BossAttackType) 정적 const vs SO?

- **A** ★: 정적 const Dictionary (PRD 원안) — 단순화, 5026 scope 최소
- **B**: BalanceConfig.bossAttackCategoryMap 추가 — 디자이너 후속 조정 가능

★ Claude 권장: A — 후속 5026.1 분리 가능, 5026 scope 최소 (5022 D2=A SerializeField 패턴 답습)

## D3. Sprint 3 통합 (Low)

Sprint 3 (changelog + CLAUDE.md) 를 Sprint 2 와 통합?

- **A** ★: 분리 (PRD 원안 — 3 commit) — 단일 책임 commit
- **B**: 통합 Sprint 2+3 = 2 commit — 작업 시간 짧음

★ Claude 권장: A — 단일 책임 원칙 (changelog 갱신은 별도 의미 단위)

## 권장 답변

```
D1=B, D2=A, D3=A
```

또는 "권장 그대로 진행".
