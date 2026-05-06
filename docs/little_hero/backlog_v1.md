# Little Hero — 다음 작업 Backlog (v1)

> **운영 규칙**:
> - System 번호 (5021~) 으로 분리. PGE 6-Phase 또는 약식 cycle 적용.
> - 완료 시 → `changelog_v1.md` 로 이동 + 본 파일에서 ✅ 마크.
> - 신규 발견 시 본 파일 끝에 추가, 우선순위 재정렬.
> - CLAUDE.md §13 은 본 파일의 요약 view (3~5건만 maintain).

---

## 우선순위 (2026-05-06 갱신)

| # | 시스템 | 우선순위 | scope | 예상 | 의존성 | 권장 cycle |
|---|---|---|---|---|---|---|
| **5021** | starter_hero_chapter_flow | 🔴 Critical | 5005 R3+R4 fix → 5020 C6 시나리오 2b 완성 | 1~2h | 5020 | 약식 (Generate→Implement→Evaluate) |
| **5022** | oop_validator_split | 🟠 High | ProjectValidator 650L → 책임별 분리 (룰 자기위반 해소) | 2~3h | - | 약식 |
| **5023** | oop_metaprogression_split | 🟠 High | MetaProgression 330L → 책임 분리 (5020 init 한계 정합 보강) | 1~2h | 5021 | 약식 |
| **5024** | editor_test_expansion | 🟡 Medium | NUnit 117 → 140+ (회귀 안전망 강화) | 4~6h | - | 약식 (4 Sprint commit) |
| **5025** | ui_korean_polish | 🟡 Medium | G3 한국어 검수 + Legacy Text 폰트 잘림 spot fix | 1~3h | - | 약식 |
| **5026** | ios_build_testflight | 🔴 Critical (출시) | G4 Apple Developer 인증 + Xcode archive + TestFlight + 5018 App Size 실측 | 4~8h | 5021, 5025 | 풀세트 (PGE 6-Phase) |
| **5027** | oop_lowprio_split | 🟢 Low | GachaService / AudioManager / CurveSimulator 등 잔여 100줄+ 클래스 분리 | 3~5h | - | 약식 |
| **5028** | content_finetune | 🟢 Low (출시 후) | F1~F6 콘텐츠 review + 5019 healthy 비율 미달 시 retry sprint | TBD | 5026 | 약식 (review 후 결정) |

---

## 5021 — starter_hero_chapter_flow 🔴 Critical

> **5020 직접 cascade. C6 시나리오 2b 미완성 해결.**

### 배경
5020 evaluation 에서 발견된 5005 attic_decomposition_v1 회귀 2건. C6 시나리오 2b (Chapter portrait 노출) 가 PASS 못 한 근본 원인.

### scope
- **R3 fix**: AtticChapterEntry._buttons[] 빈 배열 → wiring 또는 WondrousMapPanel production path 정합 검증 후 결정 (둘 중 어느 게 정식인지 사용자 결정 필요)
- **R4 fix**: MetaProgression.Awake 가 selectedCharacterId → SelectedCharacter resolve 자동화 (또는 다른 init 책임 분담 정책 결정)

### 산출 (예상)
- `Hub/AtticChapterEntry.cs` 또는 `UI/WondrousMapPanel.cs` 점검
- `Meta/MetaProgression.cs` Awake 보강 (~5L 추가)
- Attic.unity 또는 Chapter 진입 path scene wiring 복원
- NUnit 1~2 (Chapter 진입 직접 path smoke test)

### 검증 (Contract C6)
- 시나리오 1: Attic 시우 클릭 → Chapter 진입 (어떤 path 든) → portrait 정상 노출
- 시나리오 2: Chapter.unity 직접 Play → save.json hero_warrior 자동 resolve → portrait 노출
- 시나리오 3: 회귀 — 5020 NUnit 117/117 유지

### 권장 발화
```
/prd-pipeline 5021
또는
5021 starter_hero_chapter_flow 시작
```

---

## 5022 — oop_validator_split 🟠 High

> **OOP §15.6 100줄 룰 자기위반 해소.**

### 배경
ProjectValidator.cs 가 650L — §15.6 100줄 룰 위반. 본인이 룰을 검사하면서 본인은 위반하는 모순. 그리고 매 C3 검증 시 OOP warning 21 카운트의 1번에 자기 자신 등재.

### scope
ProjectValidator → 카테고리별 분리:
- `ProjectValidator.cs` (orchestrator, ~100L 목표) — Run 메서드 + Issue/Severity struct + Print
- `ProjectValidator/CharactersRule.cs` — ValidateCharacters
- `ProjectValidator/MemoryRule.cs` — ValidateMemoryPool
- `ProjectValidator/MetaUpgradeRule.cs` — ValidateMetaUpgradePool
- `ProjectValidator/EnemyRule.cs` — ValidateEnemyPool
- `ProjectValidator/SceneRule.cs` — ValidateActiveScene + Build / Chapter 등
- `ProjectValidator/OOPPolicyRule.cs` — ValidateOOPPolicy + Validate WeaponSystem 등
- `ProjectValidator/RecentSystemRules.cs` — 5012/5013/5016/5017/5020 룰 모음

### 검증
- C3 ProjectValidator 실행 결과 errors=0, warnings 변동 0
- §15.6 룰 자기위반 0 (분리된 파일 모두 100L 미만)

---

## 5023 — oop_metaprogression_split 🟠 High

> **5020 작업 중 발견된 init 책임 한계 정합 보강.**

### 배경
- MetaProgression.cs ~330L
- Awake 가 SaveData 만 로드, SelectedCharacter 미resolve → 5020 C6 시나리오 2b FAIL 원인
- §15.1 SRP — 다중 책임 (SaveData / IAP / ItemDrop / SelectCharacter / SelectChapter / Currency / Mythic)

### scope
- `MetaProgression.cs` (orchestrator, 100L 미만) — Singleton + Awake + Data prop
- `MetaProgressionLoader.cs` — Load + Migration + SelectedCharacter init resolve
- `MetaProgressionBootstrapper.cs` — IAP / ItemDrop / StarlightGrant bootstrap
- `MetaProgressionCharacter.cs` — SelectCharacter / OnCharacterChanged
- `MetaProgressionChapter.cs` — SelectChapter / activeChapterId 등

### 검증
- C2 NUnit 17+ (각 책임 분리 검증)
- C6: Chapter 직접 Play → portrait 자동 적용 (5021 cascade 해소)

---

## 5024 — editor_test_expansion 🟡 Medium

> **NUnit 117 → 140+ 회귀 안전망 강화.**

### scope (4 Sprint)
| Sprint | 영역 | 신규 NUnit |
|---|---|---|
| 1 | DamageQuery (정적 service) | 5~7 |
| 2 | EquipmentManager (slot 관리) | 6~8 |
| 3 | PlayerStats (recalc 곡선 + L100 piecewise) | 5~7 |
| 4 | ItemDropService (드랍 분포 + Affix roll) | 8~10 |

### 검증
- 각 sprint 후 새 NUnit PASS + 기존 회귀 0
- 누적: 117 → ~140

---

## 5025 — ui_korean_polish 🟡 Medium

> **G3 한국어 검수 + Legacy Text 폰트 잘림 spot fix.**

### scope
- 35 챕터 displayName / description 한국어 검수
- 가챠 / 강화 / 변환 모루 라벨 어휘 정합
- HUD / WaveTransitionToast / GameOverPanel 폰트 잘림 검사
- 일부 영문 잔존 (예 affix targetName) — 사용자에게 노출 X 인지 확인 후 정책 결정

### 검증
- 사용자 시각 review (스크린샷 또는 Play 시연)
- C3 ProjectValidator warnings 변동 0

---

## 5026 — ios_build_testflight 🔴 Critical (출시)

> **Phase 4 출시 진입.**

### scope
- Apple Developer 인증서 (사용자 행정 작업 — Apple Developer Program $99/yr)
- Xcode archive + thinning + app thinning report
- App Store Connect 업로드
- TestFlight 베타 배포 (Internal → External)
- **5018 App Size 실측** (현재 raw 794 MB, App Store 임계 150 MB 검증)
- IAP sandbox 테스트 (별빛 패키지 + 시즌 패스)
- iOS 14+ 호환성 + iPhone 12 60fps 검증

### 검증 (Contract)
- IPA 임계 150 MB 이내
- TestFlight 설치 + 5분 플레이 + crash 0
- IAP 영수증 검증 PASS

---

## 5027 — oop_lowprio_split 🟢 Low

> **잔여 100줄+ 클래스 분리 (출시 후도 가능).**

### 후보
- GachaService.cs
- AudioManager.cs
- CurveSimulator.cs (Editor 도구 — 우선순위 낮음)
- 그외 5022 검사 시 발견된 잔여

### 검증
- C3 ProjectValidator OOP warning 카운트 < 5

---

## 5028 — content_finetune 🟢 Low (출시 후)

> **5019 review 결과 + F1~F6 콘텐츠 fine-tune.**

### scope
- 5019 chapter narrative sprite 시각 review (healthy 비율 측정)
- 비율 미달 챕터 retry sprint (chapter prompt 보강)
- F4/F5 보스 추가 패턴 (BossPatternFactory 확장)
- F6 endgame 보상 밸런스 fine-tune

### 검증
- 사용자 시각 review + 일일/주간 보스 1주 플레이 데이터

---

## 변경 이력

| 일자 | 변동 |
|---|---|
| 2026-05-06 | 초기 backlog 생성 — 5021~5028, 5020 evaluation 결과 기반 |
