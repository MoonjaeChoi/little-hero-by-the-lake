# 5028 Discussion Topics

> Phase 2.2 · 합계 **3**

## D1. EquipmentPatternActivator/InstanceSyncer POCO vs 정적 (Medium)

EquipmentManager(MonoBehaviour) 가 component 를 어떻게 가져올지.

- **A** ★: POCO (instance) — facade 가 new + reference (5027 AudioBgmController 답습)
- **B**: 정적 service — facade 가 정적 위임 (5023 답습)
- **C**: MonoBehaviour component — 별도 GameObject

★ Claude 권장: A — EquipmentManager 가 EquipmentInstance 상태 (재고 인스턴스) 관리. POCO 가 facade 인스턴스에 결합. 정적 service 는 EquipmentManager 직접 reference 못함.

## D2. StarforceService facade 140L 추가 분리 (Medium)

facade 가 TryEnhance 응집 (~80L) + GetLevel/Shards (~40L) + Initialize/Reset (~20L) = 140L.

- **A** ★: 응집 유지 (140L, 200L 룰 통과) — TryEnhance 핵심 로직 분리 시 응집도 ↓
- **B**: TryEnhance 추가 분리 (TryEnhanceCore service) — facade 80L

★ Claude 권장: A — 5027 GachaRollService 200L 답습 패턴, 200L 룰 통과 + TryEnhance 응집 우선

## D3. Sprint 4 통합 (Low)

Validator 회귀 + changelog + CLAUDE.md (3 작업) 1 commit?

- **A**: 분리 (Validator 회귀 + docs 별도) — 단일 책임
- **B** ★: 통합 (PRD 원안) — 5022/5027 답습

★ Claude 권장: B — 작은 작업 통합

## 권장 답변

```
D1=A, D2=A, D3=B
```

또는 "권장 그대로 진행".
