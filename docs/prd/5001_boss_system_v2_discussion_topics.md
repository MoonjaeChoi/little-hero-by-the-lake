# 5001 Boss System v2 — Discussion Topics

> **Phase 2.2 산출** — Rebuttal 단계에서 분리된 미결 항목 (사용자 결정 필요)
> **다음**: Phase 2.3 Mediator 에서 결정 → decisions.md + v2_prd.md 최종

## High Priority Items

(없음)

## Medium Priority Items

### D1. Sprint 7+8 통합 vs 분리 유지

- **Original Criticism (critique.md §3 D1)**: Sprint 7+8 통합 권장. Sprint 7 = changelog (1 commit) + Sprint 8 = CLAUDE.md §6/§11 (1 commit). 통합 시 9 → 8 commit, 시간 30분 → 20분.

- **Current Position (PRD 그대로)**: Sprint 7 + Sprint 8 분리 (총 9 commit, 2시간 50분)

- **Discussion Points**:
  - **옵션 A: 통합** (Sprint 7 = changelog + CLAUDE.md, 1 commit)
    - 장점: commit 수 감소 (9→8), 두 docs 변경 의미 단위 동일
    - 단점: 추적성 약간 감소 (어떤 commit 이 어떤 docs 변경인지 모호)
  - **옵션 B: 분리 유지** (PRD 원안, 2 commit)
    - 장점: 단일 책임 commit (changelog 와 CLAUDE.md 의 변경 분리 가능)
    - 단점: commit 2건이 모두 작은 docs 변경 (10분씩) — 시간 낭비

- **Proposed Resolution Path (Claude 권장)**: **옵션 A (통합)**
  - 근거: 두 작업 모두 docs 변경 + 작업량 < 30분 → CLAUDE.md §15.10 워크플로 정책 (한 commit = 한 의미 단위). docs 갱신 = 한 의미 단위.
  - 회수성 손실 최소: changelog 와 CLAUDE.md 둘 다 git diff 로 추적 가능.

- **PRD Impact (옵션 A 시)**:
  - §0 Sprint 표: 8행 → 7행
  - Sprint 7 = changelog_v1.md + CLAUDE.md §6/§11 (1 commit, 20분)
  - 총 commit: 9 → **8**
  - 총 시간: 2h50m → **2h30m**

---

### D2. Phase 2 1.3× 속도 밸런스

- **Original Criticism (critique.md §3 D2)**: BossPhase2Aggressive 의 1.3× 속도 + 텔레그라프 단축. 사용자 체감 너무 빠를 위험 (PIVOT v4 cap 300 환경 + Premium 피격 effects 고려).

- **Current Position (PRD 그대로)**: 1.3× 하드코드 (`Scripts/Gameplay/Enemies/BossPhases/BossPhase2Aggressive.cs`)

- **Discussion Points**:
  - **옵션 A: 1.3× 유지** (PRD 원안)
    - 장점: 도전적 게임플레이, 메이플 식 보스 Phase 2 강도 표현
    - 단점: cap 300 환경 + cinematic effects 동시 발생 시 사용자 체감 너무 빠를 수 있음. 후속 조정 시 재컴파일 필요.
  - **옵션 B: 1.2× (보다 안전)**
    - 장점: 사용자 체감 안전 마진 확보 (10% 더 느림)
    - 단점: Phase 2 차별화 약함 (1.0× 와 큰 차이 없을 수 있음)
  - **옵션 C: SerializeField (EnemyData 또는 BossPhase2Aggressive 에서 보스별 조정)**
    - 장점: §15.4 SO 우선 정합. 디자이너 (사용자) 후속 조정 가능. CurveSimulator 후속 시뮬 + 보스별 차별 (예 ChapterBoss 1.3× / WeeklyBoss 1.5×)
    - 단점: 약간의 추가 복잡도 (필드 1개)

- **Proposed Resolution Path (Claude 권장)**: **옵션 C (SerializeField)**
  - 근거: CLAUDE.md §15.4 SO 우선 + §15.11 안티 패턴 (Magic Number 회피). 디자이너 후속 조정의 핵심 패턴 (`weapon_v2` 직업 보너스 SO 와 동일).
  - 기본값: `phase2SpeedMultiplier = 1.3f` (PRD 원안 유지) → 후속 조정 가능.

- **PRD Impact (옵션 C 시)**:
  - §3.3 BossPhase2Aggressive: `[SerializeField] private float _speedMultiplier = 1.3f;`
  - §3.2 EnemyData: `phase2SpeedMultiplier` 필드 추가 (선택 — per-boss 오버라이드)
  - Sprint 1.2: 클래스 명세 갱신 (5분)
  - 총 변동: 미미

---

### D3. MiniBoss Phase 2 일관성

- **Original Criticism (critique.md §3 D3)**: ChapterBoss/DailyBoss/WeeklyBoss 는 Phase 2 지원. MiniBoss 는 Phase 1 만 (Phase 2 X). 일관성 검토.

- **Current Position (PRD §5.2)**: MiniBoss Phase 2 X (단일 phase)

- **Discussion Points**:
  - **옵션 A: PRD 그대로 (MiniBoss 단순)**
    - 장점: 미니보스 = 5/10/15 웨이브 짧은 등장. Phase 2 도입 시 게임 흐름 늘어남.
    - 단점: 시스템 일관성 약함 (Tier 별 다른 메커니즘)
  - **옵션 B: MiniBoss 도 Phase 2 도입 (HP 30% 시)**
    - 장점: 일관성 ↑, 도전적 게임플레이. Phase 임계값 차별 (Mini 30% / Chapter 50%) 가능
    - 단점: PRD 51 Boss SO 마이그레이션 시 phase2Pattern 추가 필요 (작업량 증가). 미니보스 짧은 등장 → Phase 2 진입 전 처치될 수 있음

- **Proposed Resolution Path (Claude 권장)**: **옵션 A (PRD 그대로)**
  - 근거: 미니보스 = 5/10/15 웨이브 등장 평균 5초 처치 → Phase 2 진입 시간 부족. 단순성 우선. 후속 사이클에서 사용자 데이터 기반 도입 가능.
  - 일관성 손실 완화: PRD §3.3 의 IBossPhase 인터페이스가 모든 Tier 대응 가능 → 후속 도입 시 코드 변경 X (SO 만 갱신).

- **PRD Impact (옵션 A 시)**:
  - 변동 X (PRD 원안 그대로)
  - §3.2 EnemyData.phase2Pattern: ChapterBoss+ 만 wired (MiniBoss = None)
  - §5.2 Phase 2 Pattern 매핑: MiniBoss 행 X

## 요약

- Total Items: **3**
- High: 0 / Medium: **3** / Low: 0
- 권장 mode: **ai-assisted** (Claude 권장 + 사용자 응답 대기)
- Claude 권장 (3건):
  - D1: 옵션 A (통합)
  - D2: 옵션 C (SerializeField)
  - D3: 옵션 A (PRD 그대로)

## 다음 단계

```bash
# ai-assisted (권장)
/prd-mediator --dis 5001_boss_system_v2_discussion_topics.md

# 사용자 인라인 결정 입력 (예시)
"권장 그대로 진행"               # 일괄 채택
"D1=A, D2=C, D3=A"               # 컴팩트
"D1 통합, D2 SerializeField, D3 그대로"  # 자연어
```

## 변경 이력
- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal 산출)
