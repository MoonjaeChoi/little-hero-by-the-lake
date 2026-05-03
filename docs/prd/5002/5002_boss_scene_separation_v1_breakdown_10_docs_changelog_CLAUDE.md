# T10: docs 통합 — changelog_v1.md + CLAUDE.md §12/§13/§14

> **레이어**: Docs
> **의존성**: T01~T09 (모든 구현/검증 완료)
> **예상 시간**: 15분 / 1 commit (5002 Sprint 8, 10/10)
> **PRD 출처**: `5002_*_v2_prd.md` Sprint 8

## 1. 작업 항목

### 1.1 changelog_v1.md 등재

**경로**: `docs/little_hero/changelog_v1.md`

**추가 항목** (changelog-manager 스킬 또는 직접 추가):
```markdown
- 2026-05-04: **5002 boss_scene_separation_v1** — 챕터 보스 BossEncounter 씬 분리 (옵션 C). 8 Sprint 10 commit. BossEncounterRequest DTO + IBossEncounterLauncher (§15.7 DI 진입점) + BackgroundApplier 정적 헬퍼 (D1) + SaveData pendingChapterClearedId + AtticController toast (D2) + ChapterRunner wave==totalWaves Launcher 전환. NUnit 46→53+. ProjectValidator BossEncounterContext.Set 외부 호출 룰 신규.
```

### 1.2 CLAUDE.md §12 시스템 현황 갱신

**경로**: `CLAUDE.md`

**§12 활성 시스템 (Core)** 행에 추가:
```
... / BossEncounterService / BossLootGrantService / GachaService / StarforceService / PlayerFinder / **BossEncounterLauncher (5002)**
```

**§12 Data SO 클래스** — 변경 없음 (코드만 신규)

**§12 Tests/Editor**:
```
**Tests/Editor**: 53+ NUnit Test (EnhancementService 6 + DamageDealer 7 + StarforceService 10 + WeaponSystem 13 + BossSystem 10 + **BossSceneSeparation 7+** (5002))
```

### 1.3 CLAUDE.md §13 다음 작업 후보 갱신

**§13 우선순위** — "F4/F5 보스 씬 분리" 완료 표시:
```
~~7. 🟢 F4/F5 보스 씬 분리 — BossEncounterService 완성, 별도 보스 전투 씬~~ ✅ 2026-05-04 (5002 완료)
```

**신규 후속 항목 추가**:
```
- 🟢 5003 미니보스(5/10/15) 씬 분리 검토 (사용자 결정 시) — 옵션 D 답습. UX 단조로움 위험 검토.
- 🟢 5004 LoadSceneAsync + fade-in 통합 sprint — 모든 씬 전환에 일관 적용.
- 🟢 AtticController 책임 분리 — 393L → 415L (§15.6 200L+ 분리 권장).
```

### 1.4 CLAUDE.md §14 최근 핵심 결정 갱신

**§14 직전 5건** (회전):
```
- 2026-05-04: **5002 boss_scene_separation_v1** — 챕터 보스 BossEncounter 씬 분리 + Launcher DTO + BackgroundApplier (옵션 C, D1+D5 자동 수용 + D2/D3/D4 mediator 결정, 10 commit)
- 2026-05-04: **5001 boss_system_v2** — BossTier 4 + IBossPhase State + Factory + LootTable 4 (PGE 6-Phase 풀파이프라인 첫 시연, 10 commit, NUnit 36→46)
- 2026-05-04: **PGE 6-Phase 풀세트 도입** — 4 신규 스킬 (rebuttal/mediator/breakdown/contract) + pipeline/evaluate 개편
- 2026-05-03: **weapon_v2** — 무기 OOP 재설계 (직업별 분류 + 24 균형 + 가챠 필터, 7 commit)
- 2026-05-03: **CLAUDE.md §15 OOP 정책** — 12 섹션 (필수 준수, §1~14 보다 상위)
```

가장 오래된 1건 (`stats_v2_infinite`) 은 회전으로 제거.

### 1.5 CLAUDE.md §11 마일스톤 갱신 (선택)

큰 시스템 도입이므로 §11 표 에 행 추가:
```
| **boss_scene_separation_v1** | 챕터 보스 별 씬 + Launcher DTO + BackgroundApplier | ✅ 2026-05-04 |
```

§11 의 5001 다음 행에 삽입.

## 2. 검증 기준

### 2.1 컴파일
- N/A (docs 만)

### 2.2 Editor Test
- N/A

### 2.3 ProjectValidator
- N/A

### 2.4 회귀
- changelog_v1.md 형식 일관 (5001 패턴 답습)
- CLAUDE.md 라인 수 변화 합리 범위 (수십 라인 증가 OK)
- 마크다운 lint X (헤더 계층 / 표 정렬)

## 3. commit 메시지

```
5002 Sprint 8 — docs 통합 + changelog + CLAUDE.md §11/§12/§13/§14 (10/10)

[변경]
- docs/little_hero/changelog_v1.md — 5002 항목 등재
- CLAUDE.md §11 마일스톤 — boss_scene_separation_v1 행 추가
- CLAUDE.md §12 활성 시스템 (BossEncounterLauncher 추가) / §12 Tests 46→53+
- CLAUDE.md §13 — F4/F5 보스 씬 분리 완료 표시 + 5003/5004/AtticController 분리 후속 항목
- CLAUDE.md §14 — 최근 핵심 결정 5건 회전 (5002 추가)

[검증]
- 마크다운 lint X
- 라인 수 합리 범위
- 5001 changelog 패턴 일관

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §Sprint 8
- changelog_v1.md (기존 패턴)
- CLAUDE.md §11/§12/§13/§14

## 5. 알려진 위험

- §14 직전 5건 회전 — 가장 오래된 항목 1건 제거 시 잘못된 항목 제거 위험. 회전 전 5건 직접 확인.
- §13 후속 항목 추가 — 우선순위 번호 매기기 일관성 검토 (현 1~6).
- changelog-manager 스킬 자동 호출 vs 직접 추가 — 둘 중 1개로 통일.
