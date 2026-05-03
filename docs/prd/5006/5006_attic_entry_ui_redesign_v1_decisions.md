# 5006 attic_entry_ui_redesign_v1 — Mediator Decisions (Phase 2.3)

> **사용자 결정**: 2026-05-04 "권장안 그대로" — Claude 권장안 5건 일괄 채택
> **Mode**: ai-assisted (기본값)
> **출처**: `5006_attic_entry_ui_redesign_v1_discussion_topics.md`

## 1. 결정 요약

| # | 주제 | 결정 | 근거 |
|---|---|---|---|
| **D1** | 사용자 수동 PNG 14종 외부 의존 | **B**: Sprint 4 분리 → 5006.1 후속 | 본 PRD 즉시 진행, PNG 도착 변수 흡수 |
| **D2** | Sprint 1.2 tint 1차 중복 | **C**: tint 영구 (PNG 임포트 = 5006.1) | D1=B 결합, 작업 중복 X |
| **D3** | EndgameMenuPanel 협업 | **C**: Panel.OnEnable 자체 검증 | 결합도 0, 가장 단순 |
| **D4** | PanelToggleManager 자동 탐색 | **A**: 4 그룹 이름 규칙 회피 | 변경 0 (확인 차원) |
| **D5** | AtticUIIconImporter 9-slice | **B**: 버튼 border=12, 화폐=0 prefix 분기 | Unity 표준 (5006.1 시점) |

## 2. PRD v2 적용 변경

### 2.1 Sprint 분해 변경

**v1 (5 Sprint × 10 commit / 4시간 40분)**
```
Sprint 1: Layout + tint (1차)            [2 commit]
Sprint 2: PanelEntryView + wiring         [2 commit]
Sprint 3: CharInfo/Forge/Endgame 보강     [2 commit]
Sprint 4: PNG 임포트 + Importer           [2 commit]  ← D1=B 로 분리
Sprint 5: NUnit                           [1 commit]
Sprint 6: docs                            [1 commit]
```

**v2 (5 Sprint × 8 commit / 3시간 50분, Sprint 4 → 5006.1)**
```
Sprint 1: AtticGroupLayoutPresenter + tint (영구, D2=C)  [2 commit]
Sprint 2: AtticPanelEntryView + 10 panel wiring          [2 commit]
Sprint 3: CharInfo/Forge OnEnable + EndgameMenuPanel.OnEnable 자체 검증 (D3=C) [2 commit]
Sprint 4 (구 5): NUnit AtticEntryUITests 6                                       [1 commit]
Sprint 5 (구 6): docs (changelog + CLAUDE.md §11/§12/§14)                        [1 commit]
```

**5006.1 후속 사이클 (별도, PNG 도착 후)**
```
Sprint 1: AtticUIIconImporter (9-slice 분기, D5=B)       [1 commit]
Sprint 2: 14 PNG 임포트 + Sprite 교체 + tint 복원         [1 commit]
```

### 2.2 EndgameMenuPanel 명세 (D3=C 채택)

```csharp
// Scripts/UI/EndgameMenuPanel.cs — Sprint 3 추가
private void OnEnable()
{
    // 5006 D3 — Panel 자체 검증 (CH 35 클리어 + 추억 4종 발견 둘 중 하나 만족)
    long mask = MetaProgression.Instance?.Data?.firstClearedChapters ?? 0L;
    const long target35 = (1L << 35) - 1;
    bool allChapters = (mask & target35) == target35;
    bool allMemories = MetaProgression.Instance != null && MetaProgression.Instance.AllMemoriesDiscovered();

    if (!allChapters && !allMemories)
    {
        gameObject.SetActive(false);
        Debug.Log("[EndgameMenuPanel] CH 35 클리어 후 도전 가능");
        return;
    }
    // 기존 OnEnable 로직 (모드 표시 등)
}
```

### 2.3 4 그룹 이름 규칙 (D4=A 채택)

```
Group1Combat       (좌상)
Group2Equipment    (좌하)
Group3Shop         (우상)
Group4Record       (우하)
```

> Panel/Modal/Overlay 접미어 X → AtticPanelToggleManager 자동 탐색 영향 0

### 2.4 5006.1 보존

본 PRD (5006) 종결 후 사용자 PNG 도착 시 `/prd-pipeline 5006.1` 또는 `/prd-generator` 로 후속 사이클 진입. 5006.1 명세는 본 PRD v2 §1 후속 PRD 표 참조.

## 3. v2 PRD 변경 (요약)

- §1 Sprint 표: 6 → 5 Sprint, 10 → 8 commit, 4h40m → 3h50m
- §2.4 → §2.3 (Sprint 4 PNG 임포트 → 5006.1 분리 anchor)
- §3 의존성 그래프: Sprint 1 ─→ Sprint 2 ─→ Sprint 3 ─→ Sprint 4 (NUnit) ─→ Sprint 5 (docs)
- §4 검증 시나리오: S3 (V5 톤) 제거 (5006.1 검증)
- §5 Definition of Done: PNG 임포트 항목 제거 (5006.1 이전)

## 4. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.3 Mediator, 사용자 "권장안 그대로", 5건 일괄)
