# 5009 Chapter Overrides v1 — PRD 리뷰

## 0. 결정 이력
- 2026-05-04: Claude Code (자동) 리뷰 — Phase 2.1 Critique
- 입력: `5009_chapter_overrides_v1_prd.md` (디자인) + `5009/5009_chapter_overrides_v1_prd.md` (PRD)
- 컨텍스트: CLAUDE.md §15 + 5008 sprite_generation_rules.md + PhaseFContentSeed V2 시스템

## 1. 종합 점수: 8.6 / 10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 10/10 | 신규 코드 0건 (자산 작업 + Auditor 재실행) |
| PG-12 톤 | 10/10 | sprite_generation_rules baseline 재사용, 5008 검증 |
| 성능 | 9/10 | SpriteAtlas 재빌드 영향 명시 부족 (-1) |
| 시스템 통합 | 7/10 | **chapter override 가 V2 시스템 미사용 — ROI 의문 (-3)** |
| Sprint 분해 | 8/10 | Sprint 5 (Fantasy 48 PNG) 가장 큼 + 시드 offset 공식 검증 필요 (-2) |

## 2. Accept 항목 (5)

- ✅ **옵션 B (5 카테고리 batch)** — review 단위 명확, 5008 패턴 답습. 디자인 §2 잘 정리
- ✅ **sprite_generation_rules baseline 재사용** — 5008 검증된 768×1024 portrait + cfg 6 + 17종 grid negative + Danbooru solo 태그 그대로. 메모리 sprite_generation_v1.md 의 사용자 승인 baseline
- ✅ **시드 카테고리별 통일 + chapter offset 패턴** — 결정론적 재현 가능 (Meta 22222 base / Nature 33333 / ...)
- ✅ **Auditor 사후 검증 (5008 도구 재활용)** — Sprint 6 의 V2 fallback 검증 + 손상 0 확인 자동화
- ✅ **skip list 정책 (5008 D3=A+B 답습)** — 사용자 review iter 3 후 skip 가능, 5009.x 후속 anchor 일관

## 3. Reject 항목 (0)

(없음 — 자산 작업 + 5008 baseline 재사용, OOP/PG-12 영향 X)

## 4. Discuss 항목 (3)

### 🟡 D1. Chapter Override 의 V2 시스템 미사용 — ROI 의문

**현재 PRD**: 디자인 §1.2 가 "V2 시스템 (PhaseFContentSeed) 미사용 — archetype 30 만 사용 / Chapter override 는 V1 호환성 + 챕터별 시각적 개성 용 (현재 미사용)" 명시.

**문제**: 210 PNG 재생성에 ~3.5~5h 투자하지만 즉시 활용처 X. 5008 EnemySpriteAuditor V2 fallback 검증 결과 = OK (이미 통과).

**옵션**:
- **A. 진행 (legacy 호환 + 미래 활용)** — chapter override 가 향후 PhaseFContentSeed 변경 시 활용 가능, 또는 도감/추억 회고 sprite. 디자인 일관성 ↑
- **B. 우선순위 ↓ (5012/5013/5015 후순위)** — 코드 작업 (성능/다양성) 이 즉시 게임플레이 영향 ↑. 5009 는 5012/5013/5015 종결 후 진행
- **C. 보류 (사용자 회귀 보고 시에만 즉시)** — 5008 baseline 으로 즉시 1~2 PNG 재생성 가능. 5009 미실행

**Claude 권장**: **B (우선순위 ↓)** — 5008 종결 후 즉시 활용처 X, 5012/5013/5015 코드 작업이 더 큰 ROI. 5009 는 후순위 또는 5008.x 후속 사이클로.

---

### 🟡 D2. Sprint 5 (Fantasy 48 PNG) 분할 검토

**현재 PRD**: Sprint 5 = Fantasy 8 챕터 × 6 = 48 PNG, 한 번 사용자 review.

**문제**: 다른 Sprint (30/54/36/42) 보다 큼은 아님 (Sprint 2 Nature 54 PNG 가장 큼 — 본 PRD 의 분할 위험 명시도 Sprint 5 만 거론). 사용자 review 부담 → 48 PNG 한 번에 결정 어려움.

**옵션**:
- **A. 그대로 1 sprint** — 5 batch 일관, 6 commit 유지
- **B. Sprint 2 (Nature) 분할 + Sprint 5 분할** — Nature CH 6~10 / CH 11~14 + Fantasy CH 28~31 / CH 32~35 = 8 sprint 총 9 commit. review 부담 분산
- **C. 가장 큰 1개만 분할** — Nature 54 = Sprint 2A (CH 6~10, 30 PNG) + Sprint 2B (CH 11~14, 24 PNG)

**Claude 권장**: **A (그대로)** — 5008 검증된 batch 단위가 이미 안정. 48 PNG 한 번에 review 도 카테고리 톤 일관 검증에 효율 ↑.

---

### 🟡 D3. 시드 base + chapter offset 공식 검증

**현재 PRD**: 디자인 §3.2 "예 CH 1 시드 22222 / CH 2 시드 22228 / ... CH 5 시드 22252".

**문제**: chapter*6 offset 이라면 CH 5 = 22222 + (5-1)*6 = 22222 + 24 = **22246**. PRD 의 22252 = 22222+30 = chapter*7.5? 또는 CH 5 = 22222 + 5*6 = 22252 (1-indexed offset).

**옵션**:
- **A. CH N = 22222 + N*6** — CH 1=22228 / CH 2=22234 / CH 3=22240 / CH 4=22246 / CH 5=22252. 본 PRD 의 22252 매칭. 다만 CH 1 시드가 22222 와 다름 (22228) — Meta archetype 의 시드 22222 와 충돌
- **B. CH N = 22222 + (N-1)*6** — CH 1=22222 / CH 2=22228 / CH 3=22234 / CH 4=22240 / CH 5=22246. archetype 시드 22222 + CH 1 일치, 사용자 직관적
- **C. CH N = 22222 + (N-1)*7** — CH 1=22222 / CH 2=22229 / ... CH 5=22250. 22252 와 다름

**Claude 권장**: **B ((N-1)*6)** — archetype 시드 22222 와 CH 1 일치. PRD 의 22252 (CH 5 위치) 는 typo 추정. CH N = 22222 + (N-1)*6 으로 수정.

## 5. CLAUDE.md §15 정책 위반 점검

| 정책 | 결과 | 비고 |
|---|---|---|
| §15.1 SRP | ✅ | 신규 코드 0 |
| §15.4 SO 우선 | ✅ | prompt = MD 문서 (5008 baseline) |
| §15.6 100L | ✅ | 신규 클래스 0 |
| §15.7 DI | ✅ | Editor 도구 재실행만 |
| §15.9 Test | △ | NUnit 변동 X (자산 작업) — 정당 |
| §15.11 안티 | ✅ | public 필드 X / Update X |
| §15.12 Validator | ✅ | EnemySpriteAuditor 재실행 (5008 정착) |

## 6. 권장 변경

### 즉시 변경 (Reject 없음)

(없음)

### 사용자 결정 후 변경 (3 Discuss)

- **D1**: ROI 검토 — 5009 진행 (A) / 후순위 (B) / 보류 (C). Claude 권장 B
- **D2**: Sprint 분할 — 그대로 (A) / Nature+Fantasy 분할 (B) / 큰 것만 (C). Claude 권장 A
- **D3**: 시드 offset 공식 — (N-1)*6 권장 (PRD 의 22252 = CH 5 정합)

## 7. 추가 관찰 (정보)

### 7.1 SpriteAtlas 재빌드

210 PNG 갱신 시 EnemyAtlas 재빌드 필수 (5008 §6 Definition of Done 패턴). Sprint 6 docs 에 명시 필요.

### 7.2 chapter override 활용 시나리오

V2 시스템에서 chapter override 직접 사용하려면:
- PhaseFContentSeed.SelectPoolForChapter() 변경 — chapter 별 EnemyData.icon 직접 wiring
- 또는 EnemyController 가 chapter 컨텍스트에서 sprite 교체

5009 작업 후 활용 sprint 별도 (5009.1) 필요.

### 7.3 V1 11 (5010) 과의 통합

5009 + 5010 둘 다 V2 미사용 자산. V1 sprite (RedHood/Beanstalk/Alice) 와 chapter override 통합 활용 = 향후 PRD (5009.1?) 에서 wiring 가능.

## 8. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.1 Critique). Accept 5 / Reject 0 / Discuss 3.
