# 5027 PRD 리뷰

> Phase 2.1 Critique · 일시: 2026-05-06

## 1. 종합 점수: 8.7/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | §15.1 SRP 명확, §15.6 100L 신규 8 파일 모두 정합, GachaRollService 180L 만 권장 초과 |
| 회귀 안전성 | 9/10 | facade 패턴 — 호출처 시그니처 변경 0 |
| Sprint 분해 | 8/10 | 4 commit 합리. Sprint 1+2 작업량 큼 (60분 each) — 분리 적절 |
| 단위 테스트 | 7/10 | 신규 Test 0 — facade 위임이라 회귀 검증으로 충분하지만, RollService 핵심 로직은 단위 Test 후속 권장 |
| 회피 효과 | 8/10 | warnings 18 → 16 (-2). 효과 작음 — Top 5 분리가 더 효율 |

## 2. Accept (4)

- ✅ A1: facade 패턴 — 호출처 13 파일 시그니처 보존 (회귀 안전)
- ✅ A2: GachaService 5 책임 분리 (Result/CanRoll/Progress/Roll/Ticket) 명확
- ✅ A3: AudioManager 3 component 분리 (Sfx/Bgm/ChapterBgmMap) 명확
- ✅ A4: 5022/5023 답습 패턴 (Top 우선 + 후속 분리)

## 3. Reject (0)

(없음)

## 4. Discuss (3)

- 🟡 **D1 [Medium]**: GachaRollService 180L 권장 초과 — 추가 분리 (Roll1/Roll10x/Grant 분할) vs 응집 유지?
- 🟡 **D2 [Medium]**: AudioBgmController MonoBehaviour 의존 (AudioSource 2개 cross-fade) — facade 가 controller 구조 유지 vs 별도 GameObject component 분리?
- 🟡 **D3 [Low]**: Sprint 3 (NUnit 회귀) 와 Sprint 4 (docs) 통합 → 3 commit 으로 단축?

## 5. §15 정책 점검

- §15.1 SRP: ✅ 8 신규 파일 단일 책임
- §15.6 100L: △ GachaRollService 180L (D1)
- §15.7 DI: ✅ Initialize 명시 주입 패턴 유지
- §15.9 Test: △ 신규 Test 0 (facade 회귀로 충분, 후속 5027.1 권장)
- §15.11 안티 패턴: ✅ public 필드/Update/Instantiate 사용 X

## 6. 권장 변경

- 즉시: 없음
- 사용자 결정: D1/D2/D3 mediator
