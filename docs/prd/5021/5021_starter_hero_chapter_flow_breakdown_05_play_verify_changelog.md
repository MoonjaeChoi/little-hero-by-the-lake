# T05: Sprint 4 — Play 검증 + changelog

> **레이어**: Docs
> **의존성**: T04 (모든 wiring 적용 후)
> **예상 시간**: 30분 / 1 commit
> **PRD 출처**: v2 §5

---

## 1. 작업 항목

### 1.1 Play 검증 시나리오 4종 (사용자 보고)

**시나리오 1**: Attic 진입 + SelectedCharacter 자동 resolve
1. save.json: `selectedCharacterId="hero_warrior"` (5020 결과 또는 5021 마이그레이션 후)
2. Attic Play ▶
3. **기대**:
   - Console: `[MetaProgression] ItemDrop bootstrap` 정상
   - `[MetaProgression] _characterPool wiring OK` 또는 Warning 없음
   - AtticCharacterSelector.Start 가 SelectedCharacter 미null 받음 → "선택: 전사 시우" 라벨 즉시 표시

**시나리오 2**: Chapter 직접 Play (5020 C6 2b 검증)
1. save.json hero_warrior 보존 + Attic Play 한번 거친 후 Stop
2. **새로운 Editor 세션** (또는 Domain Reload) → Chapter.unity 직접 Open Scene → Play ▶
3. **기대**:
   - MetaProgression.Awake → Data 로드 + ResolveSelectedCharacterFromSave → SelectedCharacter set
   - PlayerController.Start 시 character != null → portrait 정상 노출
   - **노란 둥근 원 → 시우 sprite** (5020 C6 시나리오 2b 자동 PASS)

**시나리오 3**: WondrousMapPanel 챕터 진입 (D1=B 검증)
1. Attic Play → WondrousMapPanel 35 노드 표시
2. 챕터 1 노드 클릭 → SceneTransitionService.Transition("Chapter")
3. **기대**:
   - Chapter 씬 로드 (LoadSceneAsync + fade)
   - PlayerController spawn + portrait 정상

**시나리오 4**: 5020 마이그레이션 + 5021 init 통합
1. save.json 옛 형식 sed 주입:
   ```bash
   SAVE="$HOME/Library/Application Support/Bandibul/LittleHero/save.json"
   sed -i '' 's|"selectedCharacterId": "hero_warrior"|"selectedCharacterId": "default_boy"|' "$SAVE"
   ```
2. Attic Play → 1초 대기 → Stop
3. save.json 다시 확인:
   ```bash
   grep selectedCharacterId "$SAVE"
   # 기대: "selectedCharacterId": "hero_warrior" (5020 마이그레이션 + 5021 init 정상)
   ```
4. **기대**: NRE 0 + 자동 변환 + Attic 에서 시우 선택 상태로 노출

### 1.2 changelog 추가

**경로**: `docs/little_hero/changelog_v1.md` 최상단

**항목** (5020 형식 답습):

```markdown
- 2026-05-06: **5021 starter_hero_chapter_flow** — 5005 attic_decomposition_v1 회귀 R3+R4 fix → 5020 C6 시나리오 2b 완성 (D1=B + D2=C, 5 commit + Phase 1~4 docs). **사전조사**: 5020 evaluation 에서 발견된 (R3) AtticChapterEntry._buttons[] 빈 배열 + (R4) MetaProgression.Awake 가 selectedCharacterId → SelectedCharacter resolve 미지원 → Chapter 직접 진입 시 portrait 미적용. **PGE 6-Phase 적용**: Critique 8.7/10 → Rebuttal Discuss 2 → Mediator D1=B (★ AtticChapterEntry 비활성화 + WondrousMapPanel 단일화) + D2=C (Sprint 1 commit 2개 명시). **신규 산출**: docs/prd/5021/ + scripts (없음 — scene 직접 편집) + Data/CharacterPool.cs (~17L SO 인덱스) + Data/Characters/CharacterPool_Default.asset + Tests/Editor/MetaProgressionInitTests.cs (NUnit 3 — 117→120) + ProjectValidator ValidateCharacterPool 룰. **결과**: AtticChapterEntry GameObject 비활성화 (5005 dead code 정리), WondrousMapPanel 35 노드 production path 단일화, MetaProgression Awake 보강 (+15L, 5023 cascade 분리 예정). 시나리오 4종 PASS — Attic 진입 자동 resolve / Chapter 직접 Play portrait 정상 / WondrousMapPanel 챕터 진입 / 5020 마이그레이션 + 5021 init 통합. 5020 C6 시나리오 2b 자동 PASS.
```

### 1.3 CLAUDE.md §11 마일스톤 갱신

```markdown
| **starter_hero_chapter_flow** | 5005 R3+R4 회귀 fix → 5020 C6 시나리오 2b 완성 (5021, 5 commit, AtticChapterEntry 비활성화 + MetaProgression init 보강 + NUnit 117→120, GUID 보존) | ✅ 2026-05-06 |
```

### 1.4 CLAUDE.md §14 직전 5건 갱신

5021 추가, 가장 오래된 항목 (5014 v5_sticker_lora) 제거.

---

## 2. 검증 기준

### 2.1 사용자 시나리오 보고
- [ ] 시나리오 1 PASS (Attic 진입 자동 resolve)
- [ ] 시나리오 2 PASS (Chapter 직접 Play portrait 정상 — **5020 C6 2b 자동 PASS**)
- [ ] 시나리오 3 PASS (WondrousMapPanel 챕터 진입)
- [ ] 시나리오 4 PASS (5020+5021 마이그레이션 통합)

### 2.2 changelog
- [ ] `docs/little_hero/changelog_v1.md` 최상단에 5021 항목 추가
- [ ] CLAUDE.md §14 5건 maintain

### 2.3 CLAUDE.md §11 마일스톤
- [ ] starter_hero_chapter_flow 행 추가

---

## 3. commit 메시지

```
5021 Sprint 4 / T05 — Play 검증 + changelog (5/5) — 5021 종결

[변경]
  ~ docs/little_hero/changelog_v1.md (5021 항목 추가, 5020 형식 답습)
  ~ CLAUDE.md §11 마일스톤 (starter_hero_chapter_flow 행)
  ~ CLAUDE.md §14 직전 5건 (5021 추가, 5014 제거)

[검증]
  - Play 시나리오 1~4 사용자 보고 PASS
  - 5020 C6 시나리오 2b 자동 PASS (D1=B WondrousMapPanel 단일화 cascade)
  - NUnit 117 → 120 회귀 0
  - C3 Validator errors=0 (5021 ValidateCharacterPool 룰 PASS)

[종합]
  - PGE 6-Phase Phase 1~5 완수 (Phase 6 Evaluate 다음)
  - 5005 회귀 정리 (R3 dead code 제거 + R4 init 책임 보강)
  - 5023 cascade 정합 (MetaProgression 분리 예정)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- PRD v2 §5
- 5020 evaluation §5 (5005 R3+R4 backlog 등재)
- 5023 PRD (MetaProgression cascade 분리)
- changelog-manager 스킬 (자동 등재 시 활용)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| Play 시나리오 2 (Chapter 직접 Play) 실패 | T03 NUnit 3 사전 검증 → 자동 차단. 실패 시 MetaProgression Awake 순서 재검토 |
| 시나리오 3 WondrousMapPanel 35 노드 누락 | T04 게이트에서 검증 — 누락 발견 시 5021.1 backlog |
| changelog 5건 maintain 초과 | changelog-manager 자동 정리 또는 수동 |
| CLAUDE.md §11 마일스톤 행 추가 시 표 정합 | 기존 행 형식 답습 (이름 + 설명 + ✅ 일자) |
