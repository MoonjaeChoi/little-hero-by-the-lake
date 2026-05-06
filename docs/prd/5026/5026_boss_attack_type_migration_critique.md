# 5026 PRD 리뷰

> Phase 2.1 Critique · 일시: 2026-05-06

## 1. 종합 점수: 9.2/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | §15.1 SRP 정적 헬퍼, §15.6 ~80L, §15.4 SO 우선 일부 (CategoryMap dictionary 는 SO 화 후속 검토) |
| 데이터 정합 | 10/10 | 96 분포 + BossTier + 카테고리 매핑 정확 |
| 마이그레이션 안전성 | 9/10 | 기존 wiring 보존 정합 명시. V1 11종 HashSet typo 위험 — NUnit 검증 |
| Sprint 분해 | 10/10 | 3 commit 명료, 의존성 순차, 시간 추정 합리 |
| 회귀 방지 | 9/10 | 시나리오 1+2 객관적, 시나리오 3 (Boss 행동) 선택적 |

## 2. Accept (5)

- ✅ **A1**: 옵션 A (Editor 메뉴 일괄) 채택 — 5001 마이그레이션 답습, NUnit 보장
- ✅ **A2**: CategoryMap 5 카테고리 매핑 — 5008/5019 정착 패턴 정합
- ✅ **A3**: V1 11종 HashSet 분리 — Meta 분류 (CH 1~3 소속 정합)
- ✅ **A4**: Hardcore CH36~50 (n-1)%35 매핑 — 같은 카테고리 유지
- ✅ **A5**: bossAttackType=None 만 갱신 + 기존 wiring 보존 — 안전 패턴

## 3. Reject (0)

(없음)

## 4. Discuss (3)

- 🟡 **D1 [Medium]**: V1 11종 분류 — V1 캐릭터가 모두 Meta(Summon)인지? RedHood 같은 캐릭터는 Animal 적합?
- 🟡 **D2 [Low]**: CategoryMap SO 화 (BalanceConfig.bossAttackCategoryMap) 여부 — 후속 디자이너 조정 vs 정적 const 단순화
- 🟡 **D3 [Low]**: Sprint 3 (changelog + CLAUDE.md) 별도 commit vs Sprint 2 통합 — 5021/5022 통합 사례 답습 가능

## 5. §15 정책 위반 점검

- §15.1 SRP: ✅ 정적 헬퍼만
- §15.4 SO 우선: △ CategoryMap 정적 const (D2 사용자 결정)
- §15.6 100줄 룰: ✅ ~80L
- §15.7 DI: ✅ 정적 헬퍼 (Initialize 불필요)
- §15.9 Test: ✅ NUnit 4 명시
- §15.11 안티 패턴: ✅ public 필드/Update/Instantiate 사용 X
- §15.12 Validator: ✅ 기존 룰 자동 검증 (96 → 0)

## 6. 권장 변경

- 즉시 변경: 없음 (PRD 그대로 진행 가능)
- 사용자 결정 후 변경: D1/D2/D3 mediator
