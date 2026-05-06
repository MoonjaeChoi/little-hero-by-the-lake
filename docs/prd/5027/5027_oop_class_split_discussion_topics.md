# 5027 Discussion Topics

> Phase 2.2 · mediator 결정 필요. 합계 **3**

## D1. GachaRollService 180L 추가 분리 (Medium)

가챠 핵심 로직 (RollSingle/Roll10x/GrantSpecific/GrantByRarity) 응집 vs 추가 분리.

- **A** ★: 응집 유지 (180L, 200L 룰 통과) — 가챠 핵심 로직 응집도 우선
- **B**: GachaRollService + GachaGrantService 분리 (각 ~90L) — 100L 이내 정합
- **C**: Roll1/Roll10/Grant 3 분리 (각 ~60L) — 과분리 위험

★ Claude 권장: A — 가챠 핵심 로직 응집도 우선, 200L 룰 통과 (5022 SceneRule 192L 답습 패턴)

## D2. AudioBgmController MonoBehaviour vs POCO (Medium)

BGM cross-fade 는 AudioSource 2개 (MonoBehaviour 의존) — 분리 방식.

- **A** ★: facade(AudioManager) 가 AudioSource 보유, controller 는 POCO (reference 받음) — 5023 답습 (service 정적)
- **B**: AudioBgmController MonoBehaviour + 별도 GameObject — Singleton 분산
- **C**: facade 유지 + 내부 helper 만 분리 — facade 비대화

★ Claude 권장: A — 5023 service 답습, facade 가 MonoBehaviour 의존성 흡수, controller 는 POCO 단순화

## D3. Sprint 3+4 통합 (Low)

NUnit 회귀 (Sprint 3) + changelog (Sprint 4) 통합 → 3 commit?

- **A**: 분리 (PRD 원안 4 commit) — 단일 책임 commit
- **B** ★: 통합 — Sprint 3 = Validator 회귀 + docs (1 commit) → 3 commit 단축

★ Claude 권장: B — 5022 D1=B (Sprint 1+2 통합) 답습 패턴, 작업 시간 짧음 (20+10=30분)

## 권장 답변

```
D1=A, D2=A, D3=B
```

또는 "권장 그대로 진행".
