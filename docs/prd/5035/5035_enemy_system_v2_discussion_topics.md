# 5035 enemy_system_v2 Discussion Topics

> Phase 2.2 산출 — mediator 결정 8건. 권장 mode: ai-assisted

## Medium Priority (8)

### D1. 1차 도입 적 특성 수
- A 5종 (Stationary+Charger+Splitter+Ranged+Bomber) ★권장
- B 3종만 (Stationary+Splitter+Charger)
- C 10종 모두 (Healer/Summoner/Armored/Knockback Resist/Ghostly 추가)

### D2. StationaryBehavior 동작
- A 단순 정지 (발사체 X)
- **B 주기적 발사체 (Ranged 통합 동작)** ★권장
- C 발사체 + 회복 (Healer 통합)

### D3. SplitterReaction 분열 수
- A 2마리 / **B 3마리 (탕탕 답습)** ★권장 / C 5마리

### D4. BomberBehavior 폭발 반경
- A 1.0u / **B 1.5u** ★권장 / C 2.0u

### D5. 기존 EnemyAI 마이그레이션
- **A FollowBehavior 전환 + EnemyAI 삭제 (깔끔)** ★권장
- B 둘 다 보존 (호환성)

### D6. WaveComposition vs ChapterData 통합
- **A WaveComposition 신규 SO + ChapterData 참조 (재사용)** ★권장
- B ChapterData 직접 (단순)

### D7. 적용 챕터 범위
- **A CH1~5 시범 (점진 + 회귀 위험 ↓)** ★권장
- B 35 챕터 모두 / C 신규 보너스 모드

### D8. NUnit 범위
- A 8 / **B 12 (각 behavior + reaction + factory)** ★권장 / C 15+

## 요약
- Total: 8 (Medium)
- Claude 권장: D1=A, D2=B, D3=B, D4=B, D5=A, D6=A, D7=A, D8=B
