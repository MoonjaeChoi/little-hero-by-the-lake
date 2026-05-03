# 5003 Mini Boss Scene Separation Implementation Breakdown — 개요

> **Phase 3 산출** — PRD v2 (옵션 B 재진입 채택) → 구현 태스크 분해
> 일시: 2026-05-04 (재진입) · 총 5 태스크 / 5 commit / 약 1시간 40분
> 입력: `5003_*_v2_prd.md` (D1=B, D2=A 재결정 반영)

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | MiniBossSeparationMode enum + BalanceConfig 토글 + Mode.MiniBoss enum | Data + Logic | - | 15분 | _01_*.md |
| T02 | BossEncounterRunner Mode.MiniBoss 분기 + BossEncounterService.CompleteMiniBoss | Logic (Core+Waves) | T01 | 25분 | _02_*.md |
| T03 | ChapterRunner wave==15 분기 → Launcher.Launch(MiniBoss) + Chapter 씬 복귀 | Logic (Waves) | T01, T02 | 25분 | _03_*.md |
| T04 | BossSceneSeparationTests 갱신 (NUnit 8 → 10) | Test | T01~T03 | 20분 | _04_*.md |
| T05 | docs + changelog + CLAUDE.md §11/§12/§14 갱신 | Docs | T01~T04 | 15분 | _05_*.md |

## 2. 의존성 그래프

```
T01 (enum + Mode + Toggle) ──┬─→ T02 (Runner+Service) ──┐
                             │                           ├─→ T04 (Test) → T05 (docs)
                             └─→ T03 (ChapterRunner) ────┘
```

T02 ↔ T03 병렬 가능 (T01 완료 후).

## 3. 구현 순서 권장 (단일 작업자)

```
Step 1 (15분):  T01 enum + BalanceConfig + Mode.MiniBoss
Step 2 (25분):  T02 BossEncounterRunner.MiniBoss + CompleteMiniBoss
Step 3 (25분):  T03 ChapterRunner wave==15 분기
Step 4 (20분):  T04 NUnit 2개 추가
Step 5 (15분):  T05 docs + changelog + CLAUDE.md
총: 약 1시간 40분
```

## 4. CLAUDE.md §15 정합 검증

| 태스크 | §15 정합 |
|---|---|
| T01 | §15.4 SO 우선 (enum + SerializeField, Magic Number 회피) ✅ |
| T02 | §15.1 SRP (CompleteMiniBoss 단일 책임) / §15.6 Runner 145L → ~165L ✅ |
| T03 | §15.6 ChapterRunner ~175L → ~195L (200L 경계) ⚠️ |
| T04 | §15.9 NUnit 추가 ✅ |
| T05 | 정합 영향 X |

## 5. Unity MCP 안전 전략

| 태스크 | 위험 | 완화 |
|---|---|---|
| T01 | 낮음 | refresh + 5초 대기 |
| **T02** | **중간** (다중 파일) | refresh + 10초 대기 |
| **T03** | **중간** (ChapterRunner 분기) | refresh + 10초 대기 |
| T04 | 낮음 (Test) | Test Runner 검증 |
| T05 | 낮음 (docs) | 검증 불필요 |

## 6. 검증 체크리스트 (Phase 6 Contract 입력)

- C1 컴파일: 모든 commit 후 errors=0
- C2 Editor Test: 8 → 10 (BossSceneSeparation 신규 2)
- C3 Validator: warnings 변동 0 (5002 룰 그대로)
- C4 Reflection: MiniBossSeparationMode enum / Mode.MiniBoss / CompleteMiniBoss
- C5 SO: BalanceConfig.miniBossSeparation 필드 마이그레이션 (default None)
- C6 Play: S1 (옵션 B 정상 흐름) / S2 (옵션 A 토글) / S3 (옵션 None 회귀) / S4 (5002 회귀)

## 7. 변경 이력

- 2026-05-04: 초기 작성 (Phase 3 Breakdown 재진입). 5 태스크 분해.
