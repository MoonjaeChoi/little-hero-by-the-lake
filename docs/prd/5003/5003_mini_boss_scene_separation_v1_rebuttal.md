# 5003 Mini Boss Scene Separation — PRD Rebuttal

> **Phase 2.2 Rebuttal** / 일시: 2026-05-04
> 입력: `5003_*_critique.md` (8/10, Accept 6 / Reject 0 / Discuss 3)

## 1. 요약 표

| ID | Category | Criticism | Priority | Status | §15 Aligned |
|---|---|---|---|---|---|
| D1 | 디자인 옵션 | 권장 E (분리 X) vs PRD 채택 B (wave 15) 불일치 | **High** | **Discuss** (사용자 결정 핵심) | △ |
| D2 | docs scope | §11 마일스톤 등재 여부 | Low | **Discuss** | △ |
| D3 | OOP §15.4 | 두 boolean 토글 → 단일 enum 통합 | Medium | **Accepted** (자동 §15 강화) | ✅ |

## 2. 품질 메트릭

- 총 Discussion Points: 3
- **Accepted**: 1 (33%) — D3 (§15 강화)
- **Discuss**: 2 (67%) — D1 (핵심), D2 (사소)
- **Rejected**: 0
- §15 정합률: 100%

## 3. Detailed Responses

### 3.1 D3 — 두 boolean 토글 → 단일 enum (Medium / **Accepted**)

**Original Criticism**: `separateLastMiniBoss` / `separateAllMiniBosses` 두 boolean 의 잘못된 조합 위험.

**Rebuttal**: **자동 수용** (§15.4 SO 우선 + §15.1 SRP 강화).

**Proposed Action**: 단일 enum 으로 통합:
```csharp
public enum MiniBossSeparationMode
{
    None,       // 분리 X (현 5002 상태)
    LastOnly,   // wave 15 만 분리 (옵션 B)
    All,        // 5/10/15 모두 분리 (옵션 A)
}

[Header("5003 — 미니보스 씬 분리 (옵션 토글)")]
public MiniBossSeparationMode miniBossSeparation = MiniBossSeparationMode.None;
```

**PRD Impact (v2)**:
- Sprint 1 작업: `separateLastMiniBoss` / `separateAllMiniBosses` → `MiniBossSeparationMode miniBossSeparation` 단일 SerializeField
- Sprint 3 ChapterRunner 분기:
  ```csharp
  bool isMiniBossWave = wave > 0 && wave % balance.miniBossWaveInterval == 0 && wave < _chapterData.totalWaves;
  bool shouldSeparate = isMiniBossWave && (
      balance.miniBossSeparation == MiniBossSeparationMode.All
      || (balance.miniBossSeparation == MiniBossSeparationMode.LastOnly && wave == _chapterData.totalWaves - balance.miniBossWaveInterval)
  );
  ```
- 검증 시나리오 S2 (옵션 A) / S3 (옵션 E) 토글 입력 단순화 (boolean 두 개 → enum 1개)

**Little Hero Context**: 5002 `BossEncounterContext.Mode` enum 패턴 답습. SerializeField + Inspector 노출.

---

### 3.2 D1 — 디자인 권장 vs PRD 채택 (High / **Discuss**)

**Status**: discussion_topics.md 로 이동.

**이유**: **핵심 디자인 의사결정** — 사용자 권한. 옵션 변경 시 PRD 전면 재작성 가능 (옵션 E 채택 시 PRD 폐기).

**Current Position (PRD v1)**: 옵션 B (wave 15만 분리) 가정. 디자인 §2 권장은 옵션 E (분리 X).

**Resolution Path**: mediator 단계 사용자 결정. 옵션 4종 (E/B/A/D) 중 선택.

---

### 3.3 D2 — §11 마일스톤 등재 여부 (Low / **Discuss**)

**Status**: discussion_topics.md 로 이동.

**이유**: 사소한 docs 정합 결정 (D1 채택 결과에 따라 자동 결정 — 옵션 E 시 폐기).

**Resolution Path**: D1 결정 후 자동 진행 (옵션 E → 폐기 / B/A/D → 등재).

## 4. PRD v2 적용 변경 요약

| ID | 변경 위치 | 변경 |
|---|---|---|
| D3 | Sprint 1 산출물 | 두 boolean → 단일 `MiniBossSeparationMode` enum |
| D3 | Sprint 3 ChapterRunner | enum 비교 분기 |
| D3 | 검증 시나리오 S2/S3 | enum 값 토글 (boolean 조합 X) |

D1/D2 는 v2 미반영 (mediator 후 최종).

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal). Accepted 1 / Discuss 2 / Rejected 0
