# 5006 attic_entry_ui_redesign_v1 Implementation Breakdown — 개요

> **Phase 3 산출** — v2 PRD → 8 commit 태스크 분해
> 일시: 2026-05-04 · 총 8 태스크 / 8 commit / 3시간 50분

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | AtticGroupLayoutPresenter 신규 | UI/Hub | — | 30분 | _01 |
| T02 | 13 버튼 4 그룹 재배치 + tint 영구 | UI/Scene | T01 | 30분 | _02 |
| T03 | AtticPanelEntryView 신규 | UI/Hub | — | 30분 | _03 |
| T04 | Attic.unity 10 panel wiring | UI/Scene | T03 | 30분 | _04 |
| T05 | CharacterInfoPanel/ConversionForgePanel OnEnable 점검 | UI | T04 | 20분 | _05 |
| T06 | EndgameMenuPanel.OnEnable 자체 검증 (D3=C) | UI | T04 | 25분 | _06 |
| T07 | AtticEntryUITests 6 NUnit | Test | T01~T06 | 40분 | _07 |
| T08 | docs (changelog + CLAUDE.md §11/§12/§14) | Docs | T07 | 25분 | _08 |

## 2. 의존성 그래프

```
T01 (Layout) ──→ T02 (재배치 + tint)              ┐
T03 (PanelEntryView) ──→ T04 (10 wiring) ──→ T05  ├→ T07 (Test) ──→ T08 (docs)
                                              ──→ T06 (EndgameMenu)
```

**병렬 가능**:
- T01 + T03 (스크립트 신규, 컴파일 의존 X)
- T05 + T06 (각 Panel 독립)

**순차**:
- T01 → T02 (RectTransform 가 컴포넌트 존재 후)
- T03 → T04 (스크립트 후 wiring)
- T07 → T08 (Test 통과 후 docs)

## 3. 4 레이어 분포

- **Data (SO)**: 0 (5006 은 SO 신규 X)
- **Logic (Hub)**: 2 (AtticGroupLayoutPresenter / AtticPanelEntryView)
- **UI (Scene + Panel)**: 4 (T02 재배치 / T04 wiring / T05 / T06)
- **Test**: 1 (T07 6 NUnit)
- **Docs**: 1 (T08)

## 4. CLAUDE.md §15 정합 사전 검증

| 태스크 | §15 정합 |
|---|---|
| T01 AtticGroupLayoutPresenter ~50L | §15.6 ✅ / §15.7 SerializeField ✅ |
| T02 Inspector 만 (코드 X) | 정합 검증 X (Scene yaml) |
| T03 AtticPanelEntryView ~80L | §15.6 ✅ / §15.1 SRP ✅ / §15.7 ✅ |
| T04 Inspector 10 wiring | 정합 검증 X |
| T05 CharInfo/Forge — OnEnable 만 | §15.5 EventBus 또는 단순 SetActive (영향 미미) |
| T06 EndgameMenuPanel.OnEnable +5L | §15.6 통과 (기존 Panel ~180L → ~185L 임계 — 점검) |
| T07 AtticEntryUITests | §15.9 NUnit ✅ |

> T06 은 EndgameMenuPanel 기존 라인 점검 필요. 현재 ~180L 추정 (+5L = 185L, 200L 안전).

## 5. Unity MCP 안전 전략

- 한 사이클 = 1 태스크 + commit + 5~10s 도메인 리로드 대기
- Scene 저장 후 10초+ 대기 (T02/T04 후)
- 한 commit = 한 의미 단위

**위험 사이클**:
- T02 (13 버튼 parent 변경) — 다수 GameObject 이동 → 1회 Scene 저장 후 충분 대기 권장
- T04 (10 entry wiring) — Inspector 다수 드래그 → 사용자 직접 작업 또는 MCP manage_components.set_property 일괄

## 6. 변경 이력

- 2026-05-04: 초기 작성 (Phase 3, v2 PRD 8 commit 분해)
