# 5003 — Mini Boss Scene Separation v1 (예약)

> **상태**: 🟡 **RESERVED** — 디자인 + PRD 사전 작성 완료. **사용자 결정 시 Phase 2 (Critique) 부터 진행**.
> **목적**: 미니보스(5/10/15 wave) 처치를 별도 씬으로 분리할지 검토. 5002 옵션 D (디자인 §2) 답습.
> **번호**: 5003 / **선행**: 5002 boss_scene_separation_v1 (챕터 보스 BossEncounter 씬 분리, 옵션 C 채택)
> **트리거**: 5002 mediator decisions D3 — "미니보스 씬 분리 검토는 본 5002 스코프 외, 5003 후속 시스템화"

## 0. 결정 이력

| 날짜 | 결정 | 출처 |
|---|---|---|
| 2026-05-04 | 5002 옵션 C 채택 (챕터 보스만 분리) — 미니보스는 5003 후속 검토 | 5002_decisions.md (D3=A) |
| 2026-05-04 | **(예약)** 5003 디자인 + PRD 사전 작성 — 사용자 결정 대기 | 본 문서 |

## 1. 현재 시스템 분석

### 1.1 미니보스 흐름 (5002 종료 후)

| 위치 | 동작 |
|---|---|
| `BalanceConfig.miniBossWaveInterval = 5` | wave 5/10/15 마다 미니보스 등장 |
| `ChapterRunner.BuildWaveConfig` L157 | `isMiniB = waveNumber % 5 == 0 && wave < totalWaves` |
| `ChapterRunner.BuildWaveConfig` L164~165 | `_chapterData.miniBossPool` 에서 무작위 1 마리 |
| `WaveManager.SpawnRoutine` L76~80 | `config.miniBoss != null` → 미니보스 1마리 + 일반 적 절반 |
| `WaveManager.SpawnEnemy` L126 | `data.isMiniB \|\| data.isBoss` → 보스 spawn 위치 (player 주변) |
| `InfiniteTowerRunner` L85 | 무한 탑도 5층마다 미니보스 |

### 1.2 현재 UX 흐름

- 챕터 1 진입 → wave 1~4 (일반) → wave 5 (미니보스 + 일반 절반) → wave 6~9 (일반) → wave 10 (미니보스) → ... → wave 20 (BossEncounter 씬 전환, 5002)

미니보스는 일반 적과 함께 등장 (분리되지 않음).

### 1.3 SO 분포

- `EnemyData.bossTier == MiniBoss` SO 30개 (5001 마이그레이션 완료)
- `ChapterData.miniBossPool` 35 챕터 wiring
- `BossLootTable.MiniBoss` (Mini Tier ★50 + ✦1) — 5001 SO

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A** | 미니보스 5/10/15 모두 BossEncounter 씬 분리 (Mode.MiniBoss 추가) | 일관 — 모든 보스 cinematic 통일 | 4 transitions/챕터 (~4초). UX 단조로움 위험 |
| **B** ★ 후보 | 마지막 미니보스(15)만 분리 — wave 16~19 일반 적 후 보스 빌드업 효과 | 보스(20) 직전 긴장감 강화. 씬 전환 1회/챕터 (~1초) | 미니보스 5/10 은 분리 X (일관성 약함) |
| **C** | 신규 'MiniBossEncounter' 씬 생성 (간소화 — 미니보스 단독 spawn, 일반 적 X) | 짧은 cinematic. 챕터 BG 그대로 적용 가능 | 신규 씬 1 + Build Settings 등록. BossEncounter 와 중복 코드 위험 |
| **D** | BossEncounter 씬 재사용 (Mode.MiniBoss) + 미니보스 + 일반 적 모두 spawn | 기존 인프라 최대 재사용. WaveManager 흐름 그대로 | 미니보스 + 일반 적 동시 spawn → 씬 분리 의미 약화 |
| **E** ★ 권장 | **분리 X (현 상태 유지)** — 5002 옵션 C 그대로 | UX 단조로움 회피. 5002 종료 상태 그대로. 미니보스 = "양념" 의도 보존 | 5002 의 cinematic 통일성과 일관성 약간 손실 |

**★ 권장 = E (분리 X)** — UX 단조로움 위험 + 5002 의도 (미니보스 = 챕터 진행 중 양념) 보존. 본 시스템은 사용자 결정 시 옵션 B 또는 A 검토.

**★ 후보 = B (15만 분리)** — 사용자가 "분리 효과를 보고 싶다" 시 추천. 보스 빌드업 효과만 채택, UX 부담 최소.

## 3. 권장 설계 (옵션 B 채택 가정)

옵션 B 채택 시 신규/변경 파일:

| 종류 | 경로 | 책임 | 라인 예상 |
|---|---|---|---|
| 변경 | `Scripts/Core/BossEncounterContext.cs` | `Mode` enum 에 `MiniBoss` 추가 | +1 |
| 변경 | `Scripts/Core/BossEncounterRequest.cs` | DTO 그대로 (Mode 만 신규 값) | 변경 X |
| 변경 | `Scripts/Gameplay/Waves/BossEncounterRunner.cs` | `Mode.MiniBoss` 분기 + 보상 처리 (BossLootGrantService.Apply MiniBoss tier) | +20 |
| 변경 | `Scripts/Core/BossEncounterService.cs` | `CompleteMiniBoss(EnemyData boss, ChapterData chapter)` 신규 | +20 |
| 변경 | `Scripts/Gameplay/Waves/ChapterRunner.cs` | `wave == 15` 감지 + `BossEncounterLauncher.Launch(Mode.MiniBoss)` | +15 |
| 변경 (옵션) | `Scripts/Data/BalanceConfig.cs` | `bool separateLastMiniBoss = true` SerializeField (디자이너 토글) | +1 |

옵션 A 채택 시: ChapterRunner 의 `wave == 15` 분기를 `wave % 5 == 0 && wave < totalWaves` 로 확장. 미니보스 5/10/15 모두 분리.

옵션 C 채택 시: 신규 씬 `Scenes/MiniBossEncounter.unity` + `MiniBossEncounterRunner.cs` (BossEncounterRunner 와 유사) + Build Settings 등록.

## 4. CLAUDE.md §15 정합 검증

| 정책 | 옵션 B 기준 |
|---|---|
| §15.1 SOLID-S | BossEncounterContext.Mode 단일 책임 유지 |
| §15.1 SOLID-O | switch 4분기 (Daily/Weekly/ChapterBoss/MiniBoss) — 허용 범위 |
| §15.4 SO 우선 | `BalanceConfig.separateLastMiniBoss` SerializeField 토글 (Magic Number 회피) |
| §15.6 100줄 | BossEncounterRunner 145L → ~165L (200L 경계 내) |
| §15.7 DI | 기존 Launcher 재사용 (변경 X) |
| §15.9 Test | BossSceneSeparationTests 에 `Mode_Enum_Has_MiniBoss` + `CompleteMiniBoss_Null_NoOp` 추가 (NUnit 8 → 10) |
| §15.12 Validator | 기존 룰 그대로 (BossEncounterContext 외부 직접 R/W 0건) |

## 5. Sprint 분해 (옵션 B 가정)

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Mode.MiniBoss + BalanceConfig.separateLastMiniBoss 토글 | 1 | 15분 |
| 2 | BossEncounterRunner Mode.MiniBoss 분기 + BossEncounterService.CompleteMiniBoss | 1 | 25분 |
| 3 | ChapterRunner wave==15 분기 → Launcher.Launch(MiniBoss) | 1 | 25분 |
| 4 | NUnit 2개 추가 + BossSceneSeparationTests 갱신 (8 → 10) | 1 | 20분 |
| 5 | docs + changelog + CLAUDE.md §12/§14 갱신 | 1 | 15분 |
| **총** | | **5** | **1시간 40분** |

옵션 A: Sprint 3 의 `wave==15` → `wave % 5 == 0` 변경. 시간 변동 X. 4 transitions 검증 시나리오 추가.
옵션 C: Sprint 3 추가 (신규 씬 wiring + MiniBossEncounterRunner.cs ~80L). 시간 +60분.

자세한 명세는 → [5003/5003_mini_boss_scene_separation_v1_prd.md](5003/5003_mini_boss_scene_separation_v1_prd.md)

## 6. 알려진 위험 + 완화

| 위험 | 옵션 | 완화 |
|---|---|---|
| 씬 전환 잦은 (UX 단조로움) | A (4회) | LoadSceneAsync + fade 0.3s (5004) |
| 보스 빌드업 효과 약화 | E | 옵션 B 채택 시 wave 15→20 빌드업 강조 |
| 신규 씬 prefab 중복 (MiniBossEncounter) | C | BossEncounter 씬 prefab 복제, 변경부 최소 |
| 일반 적 spawn 누락 (옵션 B/A) | B/A | WaveManager.SpawnRoutine 에서 미니보스 + 일반 적 절반 그대로 spawn (boss 분리와 다름) |
| 5002 회귀 (Daily/Weekly/ChapterBoss) | 모두 | NUnit 8/8 회귀 검증 — Phase 6 자동 |
| MCP SIGSEGV | 모두 | 한 commit 단위 분리, 씬 변경 후 10초+ 대기 (5002 패턴) |

## 7. 후속 시스템화 결정 트리

```
사용자 결정 시점
  ↓
"5003 진행" / "옵션 B 채택" / "분리 검토"
  ↓ /prd-pipeline 5003 (옵션 X) → Phase 2~6 자동
  
또는 "5003 보류" / "분리 X 결정"
  ↓ 옵션 E (현 상태) — 본 PRD 폐기 또는 v1 그대로 보존
```

## 8. 변경 이력

- 2026-05-04: v1 초안 작성 (예약 / 사용자 결정 대기). 5002 mediator D3 후속 시스템화.

## 9. Sources

- `docs/prd/5002_boss_scene_separation_v1.md` (옵션 D 비교 + §7 후속 항목)
- `docs/prd/5002/5002_*_decisions.md` (D3=A 결정 — 5003 후속)
- `docs/prd/5002/5002_*_evaluation.md` §8 다음 단계 — "5003 미니보스 씬 분리 검토 (사용자 결정 시)"
- 코드: `ChapterRunner.BuildWaveConfig` L157~165 / `WaveManager.SpawnRoutine` L70~88 / `BalanceConfig.miniBossWaveInterval`
