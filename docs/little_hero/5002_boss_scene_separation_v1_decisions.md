# 5002 Boss Scene Separation — Mediator Decisions

> **Phase 2.3 Mediator 산출** — discussion_topics 의 3 미결 항목에 대한 최종 결정.
> **모드**: ai-assisted (Claude 권장 일괄 채택)
> **사용자 결정**: "권장안 그대로" (2026-05-04)
> 결정: **D2=A, D3=A, D4=B**

## 결정 표

| ID | 토픽 | 결정 | 모드 | PRD v2 적용 |
|---|---|---|---|---|
| **D2** | ChapterCleared 표시 시점 | **A** — SaveData `pendingChapterClearedId` + Attic 1회 toast | ai-assisted (권장 채택) | Sprint 4 + AtticController 변경 |
| **D3** | 미니보스 씬 분리 후속 | **A** — 본 5002 스코프 외, 디자인 §6 후속 항목 명시 | ai-assisted (권장 채택) | 디자인 v1.md §6 1줄 추가 |
| **D4** | LoadSceneAsync 도입 시점 | **B** — 5002 sync 유지, 5004 별 최적화 sprint | ai-assisted (권장 채택) | PRD §6.1 위험 표 1줄 갱신 |

## 결정 상세

### D2 = A — SaveData 단일 진실원

**채택 이유**:
- 기존 `firstClearedChapters` 와 같은 SaveData 패턴 답습 → 일관성
- Attic 가 모든 보상·상태 표시의 단일 진실원 → §15.1 SRP 정합
- SaveData 마이그레이션은 빈 문자열 기본값으로 자동 (별 commit 불필요)
- 옵션 B/C 는 BossEncounter 씬 책임 늘어남 또는 D4 결합 필수 → 거부

**PRD v2 영향**:
- `MetaProgressionData` 에 `string pendingChapterClearedId = "";` 신규 필드 (Sprint 4 commit 안에 포함)
- Sprint 4 `BossEncounterService.CompleteChapterBoss` 에:
  ```csharp
  if (MetaProgression.Instance?.Data != null)
  {
      MetaProgression.Instance.Data.pendingChapterClearedId = chapter.id;
      SaveSystem.Save(MetaProgression.Instance.Data);
  }
  ```
- `AtticController.Start` 에 `HandlePendingChapterCleared()` 1회 호출 추가:
  ```csharp
  private void HandlePendingChapterCleared()
  {
      var data = MetaProgression.Instance?.Data;
      if (data == null || string.IsNullOrEmpty(data.pendingChapterClearedId)) return;
      var ch = ResolveChapterById(data.pendingChapterClearedId);
      if (ch != null && _chapterClearedPanel != null) _chapterClearedPanel.Show(ch);
      data.pendingChapterClearedId = "";
      SaveSystem.Save(data);
  }
  ```
- ChapterClearedPanel — Attic 씬에 prefab 인스턴스 추가 (이미 존재할 가능성 높음 — Sprint 4 wiring 검증)

### D3 = A — 본 스코프 외, 후속 명시

**채택 이유**:
- 5002 스코프 = "챕터 보스 씬 분리". 미니보스 분리는 별 의사결정 (UX 단조로움 위험 → 사용자 검토 필요)
- 5003 PRD 즉시 자동 생성은 사용자 의사 미확인 시 헛 작업

**PRD v2 영향**: 없음 (PRD 변동 0)

**디자인 v1.md 영향**: §6 위험 표 또는 별 §7 후속 항목에 1줄 추가:
```
- 5003 미니보스(5/10/15) 씬 분리 검토 (사용자 결정 시) — 옵션 D 비교 답습. 5002 종료 후 사용자 의사 확인.
```

### D4 = B — 5002 sync 유지, 5004 별 최적화 sprint

**채택 이유**:
- 5002 = 분리 자체. async + fade 는 모든 씬 전환에 일관 적용 권장 → 단일 sprint (5004) 로 통합 작업이 효율적
- Sprint 3 작업량 +20분 + 모든 씬 전환 4곳 동시 수정은 scope creep
- 5002 sync 전환 1회 (~1초) UX 부담은 짧고, 5004 에서 일괄 해소

**PRD v2 영향**:
- §6.1 위험 표 의 "LoadSceneAsync (향후)" → "LoadSceneAsync — 5004 별 최적화 sprint 일괄 적용"
- Sprint·commit 변동 X

## 변경 이력

- 2026-05-04: 사용자 권장안 일괄 채택 (Phase 2.3 mediator). D2=A, D3=A, D4=B.
