# T04: Sprint 3 — Attic.unity wiring (R4 + R3 D1=B)

> **레이어**: Data (scene)
> **의존성**: T02 (CharacterPool 인스턴스 GUID), T03 (MetaProgression._characterPool field)
> **예상 시간**: 60분 / 1 commit (D1=B cascade — WondrousMapPanel 검증 +30분)
> **PRD 출처**: v2 §4
> **★ 게이트**: WondrousMapPanel wiring 시각 review

---

## 1. 작업 항목

### 1.1 R4 wiring — MetaProgression._characterPool

**파일**: `Scenes/Attic.unity` (수정)

**작업**: MetaProgression GameObject 의 `_characterPool` field 에 CharacterPool_Default.asset GUID 매핑.

**방법 1 (권장)**: Unity Editor Inspector 에서 MetaProgression GameObject 선택 → Inspector → CharacterPool field 에 CharacterPool_Default 드래그

**방법 2 (scene YAML 직접)**:
1. CharacterPool_Default.asset.meta 의 GUID 추출
2. Attic.unity 의 MetaProgression component (m_EditorClassIdentifier 검색) 의 `_characterPool: {fileID: 0}` → `_characterPool: {fileID: 11400000, guid: <GUID>, type: 2}`

### 1.2 R3 fix — AtticChapterEntry 비활성화 (D1=B)

**작업 (D1=B mediator 결정)**:

#### 사전 조사
```bash
# WondrousMapPanel wiring 상태 검사
grep -B 1 -A 30 "WondrousMapPanel" LittleHero/Assets/_Project/Scenes/Attic.unity | head -50
# _chapters[] 와 _nodes[] 가 채워져 있는지 확인

# AtticChapterEntry 사용처 검사
grep -rn "AtticChapterEntry" LittleHero/Assets/_Project/Scripts/ 2>/dev/null
# 0건 또는 극소 = 비활성화 안전
```

#### Step 1: AtticChapterEntry GameObject 비활성화

**옵션 A (간단)**: Attic.unity 의 AtticChapterEntry GameObject 의 `m_IsActive: 1` → `m_IsActive: 0`

**옵션 B (Component 제거)**: Attic.unity 의 AtticChapterEntry MonoBehaviour 항목 자체 제거

→ **옵션 A 권장** (history 보존, 향후 복구 용이).

#### Step 2: WondrousMapPanel wiring 검증

**검사 항목**:
- `_chapters[]` (35 ChapterData 참조) — 35 모두 wiring 됐는지
- `_nodes[]` (35 WondrousMapNode 참조) — 35 모두 wiring 됐는지
- `_confirmModalRoot`, `_confirmStartButton` 등 모달 관련 field

**누락 발견 시**:
- 35 노드 모두 누락 → 5021.1 별도 fix backlog (시간 +60~120분)
- 일부 누락 → Inspector 또는 scene YAML 직접 보강 (Sprint 3 시간 내 처리)

### 1.3 사용자 시각 review (★ 게이트)

1. Attic Play ▶ → WondrousMapPanel 35 노드 표시 확인
2. 챕터 1 클릭 → SceneTransitionService.Transition("Chapter") 호출
3. Chapter 씬 로드 + portrait 정상 노출 (시우 sprite, 노란 원 X)

---

## 2. 검증 기준

### 2.1 git status 확인
- `Scenes/Attic.unity` `M` (modified, 1개)
- 합산: 1 M (scene 파일만)

### 2.2 Unity MCP 검증 (refresh_unity + 10s + read_console)
- 콘솔 errors=0 (scene 변경 후 reimport)
- MetaProgression Inspector 에서 _characterPool 미리보기 정상
- AtticChapterEntry GameObject 가 비활성화 (회색 표시)

### 2.3 Play 검증
- Attic Play → MetaProgression.Awake → Console "[MetaProgression] _characterPool wiring OK" (또는 Warning 없음)
- WondrousMapPanel 35 노드 표시
- 챕터 1 진입 → portrait 정상

---

## 3. commit 메시지

```
5021 Sprint 3 / T04 — Attic.unity wiring R4+R3 (D1=B mediator) (4/5)

[변경 — D1=B 결정]
  ~ Scenes/Attic.unity:
    - MetaProgression._characterPool → CharacterPool_Default GUID (R4 fix)
    - AtticChapterEntry GameObject m_IsActive: 1 → 0 (R3 fix, dead code 정리)
    - WondrousMapPanel _chapters[] / _nodes[] wiring 상태 검증 (필요 시 보강)

[검증]
  - Unity 콘솔 errors=0
  - Play 시 MetaProgression.Awake → SelectedCharacter 자동 set (Warning 없음)
  - WondrousMapPanel 35 노드 정상 (사용자 시각 review)
  - 챕터 1 진입 → portrait 정상 노출 (5020 C6 시나리오 2b 자동 PASS)

[5005 회귀 정리]
  - AtticChapterEntry 비활성화 — 5005 attic_decomposition_v1 dead code
  - WondrousMapPanel 가 production path 단일화

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- PRD v2 §4
- decisions D1=B (★ 핵심)
- 5020 patches 5020.1 (Attic 라벨) + 5020.2 (_buttons[] wiring) — scene 직접 편집 패턴 답습
- 5005 attic_decomposition_v1 (dead code 정리 cascade)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| WondrousMapPanel 35 노드 wiring 모두 누락 | Sprint 3 시간 60분 → 90~120분 (5021.1 backlog 가능) |
| AtticChapterEntry 비활성화 후 다른 코드 참조 | grep "AtticChapterEntry" 0 confirm — Sprint 시작 직후 |
| scene YAML 직접 편집 시 fileID 오타 → scene 손상 | `git diff` 검증 + Unity 콘솔 errors 확인 후 commit |
| Inspector 변경 vs YAML 직접 — 일관성 | 같은 commit 내 통일 (Inspector 권장 — UI 직관) |
