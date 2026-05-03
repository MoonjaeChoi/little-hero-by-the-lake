# 5001 Boss System v2 — PRD 리뷰

> **Phase 2 Critiquer** — prd-critiquer 스킬 적용 (CLAUDE.md §15 OOP + 5 카테고리)
> **검토자**: Claude Code (자동) / **일시**: 2026-05-03

## 1. 종합 점수: **8.6 / 10**

| 카테고리 | 점수 | 비고 |
|---|---|---|
| **§1 OOP §15 정합** | 9.5/10 | SOLID 5 원칙 모두 충족 |
| **§2 PG-12 톤** | 10/10 | 위반 영역 X |
| **§3 성능 (cap 300/GC 50KB)** | 8/10 | Phase 전환 GC 점검 필요 |
| **§4 시스템 통합** | 9/10 | 기존 Boolean 호환 명시 ✅ |
| **§5 Sprint 분해 적절성** | 7/10 | Sprint 7+8 통합 권장 |

## 2. Accept 항목 (8) ✅

### A1. BossTier enum 4 분리 (★ 핵심)
- 현재 `isMiniB` / `isBoss` boolean 한계 명확히 진단
- 4 Tier (MiniBoss / ChapterBoss / DailyBoss / WeeklyBoss) 메이플 식 명료
- BossTier.None 으로 비-보스 호환 유지

### A2. IBossPhase State 패턴 ★
- 4 메서드 (OnEnter/Tick/OnExit/ShouldTransitionNext) — ISP 정합
- BossPhase1Default + BossPhase2Aggressive 구체 구현
- HP 50% 기본 임계값 + EnemyData 에서 SerializeField 가능

### A3. BossPatternFactory 분리 (SOLID-O)
- EquipmentPatternFactory 패턴 답습 — 일관성 ↑
- EnemyController.CreateBossAttack switch 추출 — 신규 패턴 추가 시 1 곳만 수정

### A4. BossLootTable SO 4 인스턴스 (§15.4)
- ScriptableObject 우선 정합
- Tier별 보상 명확 (메이플 식 차별화)
- mythicChance 1.0 (WeeklyBoss) 메이플 키우기 보장 패턴 정합

### A5. EnemyData 호환 유지
- 기존 isMiniB/isBoss boolean 잔존 가능 (bossTier=None 처리)
- 27 사용처 — 점진 마이그레이션

### A6. Sprint 의존성 그래프 명확
- Sprint 1 → 2 → 3+4 (병렬) → 5 → 6
- 의존성 명시 + 병렬 가능 표시

### A7. 검증 시나리오 3 구체적
- MiniBoss / ChapterBoss Phase 전환 / WeeklyBoss 신화 보장
- 콘솔 로그 검증 명시

### A8. 51 Boss SO 자동 마이그레이션 룰
- isMiniB/isBoss + 경로 기반 자동 분류
- WeaponMigrationSeed 패턴 답습 — 검증 가능

## 3. Discuss 항목 (3) 🟡

### D1. Sprint 7 + 8 통합 권장
**현재 PRD**:
- Sprint 7 — changelog 등재 (1 commit)
- Sprint 8 — CLAUDE.md §6 + §11 갱신 (1 commit)

**제안**: Sprint 7 통합 (1 commit) — changelog + CLAUDE.md 동시 갱신.
- 이유: 두 작업 모두 docs 변경, 의미 단위 동일
- 결과: 9 commit → **8 commit**, 시간 30분 → 20분

**사용자 결정 요청**: 통합 OK / 분리 유지

### D2. Phase 2 1.3× 속도 밸런스
**현재**: BossPhase2Aggressive 가 1.3× 속도 + 텔레그라프 단축

**위험**: 사용자 체감 너무 빠를 수 있음 (PIVOT v4 cap 300 환경 + Premium 피격 effects 고려).

**대안**:
- 옵션 A: 1.3× 유지 (PRD 그대로)
- 옵션 B: 1.2× (보다 안전)
- 옵션 C: SerializeField (EnemyData 에서 보스별 조정 가능)

**사용자 결정 요청**: 1.2 / 1.3 / SerializeField 분리

### D3. MiniBoss Phase 2 X 일관성
**현재 PRD**: MiniBoss 는 Phase 1 만 (Phase 2 X)

**일관성 검토**:
- ChapterBoss/DailyBoss/WeeklyBoss = Phase 2 지원
- MiniBoss = Phase 1 만

**대안**:
- 옵션 A: PRD 그대로 (MiniBoss 단순)
- 옵션 B: MiniBoss 도 Phase 2 (HP 30% 시) — 더 도전적 게임플레이

**사용자 결정 요청**: A 그대로 / B MiniBoss Phase 2 도입

## 4. Reject 항목 (0) ❌

(없음)

## 5. CLAUDE.md §15 정책 위반 점검

### 5.1 §15.1 SOLID
- ✅ SRP: BossPhase / Factory / LootTable 분리
- ✅ OCP: switch → Factory (EquipmentPatternFactory 패턴)
- ✅ LSP: IBossPhase 모든 구현체 호환
- ✅ ISP: IBossPhase 4 메서드 (작은 인터페이스)
- ✅ DIP: BossPhaseManager 정적 헬퍼 (Singleton 직접 참조 X)

### 5.2 §15.4 ScriptableObject 우선
- ✅ BossLootTable SO 4 인스턴스
- ✅ EnemyData 필드 추가 (Inspector wiring)
- ✅ 마법 숫자 X (BossPhase 임계값 0.5f → SerializeField)

### 5.3 §15.6 100줄 룰
- ✅ 신규 클래스 명세 모두 100줄 이내 추정
  - IBossPhase: 4 메서드 (15 라인)
  - BossPhase1Default: 30 라인
  - BossPhase2Aggressive: 40 라인
  - BossPatternFactory: 20 라인
  - BossLootTable: 25 라인
  - BossPhaseManager: 30 라인

### 5.4 §15.7 Lightweight DI
- ✅ Initialize 명시 주입 (IBossPhase.OnEnter(EnemyController))
- ✅ Singleton 직접 참조 회피 (BossPhaseManager 정적)

### 5.5 §15.9 Test
- ✅ BossSystemTests 10 명시 (Sprint 6)
- ✅ Factory + Phase + LootTable 단위 테스트

### 5.6 §15.11 안티 패턴
- ✅ public 필드 X (SerializeField + private 권장)
- ✅ Update in 1500 X (단일 IBossPhase Tick)
- ✅ Instantiate in Update X (BossLootTable 보상은 EnemyController.Die 시점)

### 5.7 §15.12 Validator
- ✅ ProjectValidator 신규 룰 명시 (51 SO bossTier 검증)

## 6. PG-12 톤 검증

- ✅ Phase 2 "Aggressive" — 만화풍 강한 임팩트 (잔인·피 X)
- ✅ 보스 사망 시 LootTable 보상 (별 흩어짐 그대로)
- ✅ 신화 보장 — 무지개 fade-in (비폭력 톤)
- ✅ 텔레그라프 단축 — 도전적 (PG-12 적합)

## 7. 성능 검증 (PIVOT v4 cap 300)

### 7.1 GC 영향
- ✅ IBossPhase 인스턴스 — 보스 1개당 1 (GC 영향 X)
- ✅ BossLootTable SO — 메모리 1회 로드
- ⚠️ Phase 전환 시 OnExit + OnEnter 호출 — 일시 GC 가능 (점검 권장)

### 7.2 frame budget
- ✅ 단일 보스 (cap 300 환경에서 1) → IBossPhase.Tick 비용 미미
- ✅ Premium 피격 효과와 결합 — frame budget 내

### 7.3 SaveData
- ✅ BossLootTable 보상 → SaveData 갱신 (배치 batching, MetaProgression.Save)
- ✅ Phase 상태는 SaveData 저장 X (런타임만)

## 8. 시스템 통합 검증

### 8.1 stats_v2 정합
- ✅ 보스 데미지는 PIVOT v4 단일 공식 (보스/일반 분기 X) 그대로
- ✅ BossPhase2Aggressive 의 1.3× 속도는 attack interval 만 영향 (데미지 X)

### 8.2 weapon_v2 정합
- ✅ 보스 처치 시 WeaponClassBonus 영향 X
- ✅ BossLootTable 의 EquipmentInstance 보상 — gacha_class_filter 와 분리 (보스 보상은 모든 직업 OK)

### 8.3 가챠 / 한정 상점 정합
- ✅ BossEncounterService.GuaranteeMythicForEquipped — bossTier=WeeklyBoss 매칭
- ✅ LimitedShopService.HandleDailyBossClear — bossTier 활용 가능

### 8.4 Endgame 시스템
- ✅ EndgameResetService — bossTier 명시적 분기 가능 (Daily/Weekly)
- ✅ 무한 다락방 — 보스 풀 (BossEncounterContext) 그대로

## 9. Sprint 분해 적절성 (7/10)

### 9.1 작업량 균형
| Sprint | commit | 시간 | 평가 |
|---|---|---|---|
| 1 | 3 | 45분 | ✅ 적절 |
| 2 | 1 | 20분 | ✅ |
| 3 | 2 | 30분 | ✅ |
| 4 | 1 | 30분 | ✅ |
| 5 | 1 | 15분 | ✅ |
| 6 | 1 | 30분 | ✅ |
| 7 | 1 | 10분 | ⚠️ Sprint 8 통합 권장 |
| 8 | 1 | 10분 | ⚠️ |

### 9.2 의존성
- ✅ Sprint 1 → 2 → 3 → 4 → 5 → 6 → 7+8
- ✅ 병렬 가능: Sprint 3 + 4

### 9.3 Discuss 결정 필요
- D1 (Sprint 7+8 통합 → 8 commit)

## 10. 권장 변경

### 즉시 변경 (사용자 결정 X 가능)
- (없음)

### 사용자 결정 후 변경
- **D1**: Sprint 7+8 통합 → 8 commit, 시간 2시간 50분 → 2시간 30분
- **D2**: Phase 2 속도 1.3× → 1.2× 또는 SerializeField
- **D3**: MiniBoss Phase 2 도입 여부

## 11. 결론

PRD **8.6/10** — 진행 가능 수준.

### 주요 강점
- BossTier enum 4 Tier 분리 명료
- IBossPhase State 패턴 OOP 정합
- BossLootTable SO + Factory 추출 — CLAUDE.md §15 모두 통과
- 51 Boss SO 자동 마이그레이션 룰 명확
- 검증 시나리오 3 구체적

### 권장 다음 단계
- **Discuss 3 항목 결정 후 Phase 4 (Implement)**
- 또는 PRD 그대로 진행 (D1만 사용자 확인)

## 12. 변경 이력
- 2026-05-03: 초기 작성 (Phase 2 Critiquer)

## 13. 다음 단계 (prd-pipeline)

```
[현재] Phase 2 ✅ — critique.md 산출
[다음] Phase 3 Mediator — Discuss D1/D2/D3 사용자 결정 통합
       Phase 4 Implement — 8~9 commit 구현
       Phase 5 Evaluate — 컴파일 + Test + Play
```
