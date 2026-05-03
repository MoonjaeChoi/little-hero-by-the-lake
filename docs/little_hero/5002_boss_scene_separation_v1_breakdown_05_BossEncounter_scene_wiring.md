# T05: BossEncounter.unity Background SpriteRenderer wiring

> **레이어**: Editor (Scene)
> **의존성**: T04 (BossEncounterRunner._backgroundRenderer SerializeField)
> **예상 시간**: 10분 / 1 commit (5002 Sprint 3.2, 5/10)
> **PRD 출처**: `5002_*_v2_prd.md` Sprint 3.2

## 1. 작업 항목

### 1.1 씬 변경

**경로**: `LittleHero/Assets/_Project/Scenes/BossEncounter.unity`

**Unity Editor 작업**:
1. BossEncounter 씬 열기
2. Hierarchy 에 `Background` GameObject 추가 (또는 기존 placeholder 활용)
3. `SpriteRenderer` 컴포넌트 추가
   - sprite: null (Runtime 에 BackgroundApplier.Apply 가 설정)
   - sortingOrder: -100
   - drawMode: Tiled
   - tileMode: Continuous
   - size: (300, 300)
   - color: white
4. BossEncounterRunner 의 `_backgroundRenderer` SerializeField 에 Background.SpriteRenderer 드래그
5. AudioManager / VfxPool / DamageFloaterPool — DontDestroyOnLoad 객체 — 별도 wiring 불필요 확인
6. 씬 저장 (`Ctrl/Cmd + S`)

### 1.2 검증 wiring

- Inspector 에서 `_backgroundRenderer` 가 Background.SpriteRenderer 를 참조하는지 확인
- BossEncounterRunner 의 다른 SerializeField (`_atticSceneName`, `_returnDelay`, `_battleDuration`) 회귀 X

## 2. 검증 기준

### 2.1 컴파일
- 씬만 변경 (코드 변경 X) → 컴파일 영향 없음

### 2.2 Editor Test
- N/A (T09 reflection 검증)

### 2.3 ProjectValidator
- 씬 wiring 변경 — Validator 영향 없음

### 2.4 Play Mode 회귀
- Editor Play Mode → BossEncounter 씬 직접 진입 (또는 EndgameMenuPanel → Daily 클릭)
- Daily 보스 처치 흐름 회귀 X (BG sprite null 인 채로 정상 동작)
- ChapterBoss 분기는 T07 활성 후 검증

## 3. commit 메시지

```
5002 Sprint 3.2 — BossEncounter.unity Background SpriteRenderer wiring (5/10)

[변경]
- Scenes/BossEncounter.unity — Background GameObject + SpriteRenderer
- BossEncounterRunner._backgroundRenderer Inspector 드래그

[검증]
- Play Mode — Daily 보스 BG null 안전 (no-op)
- ChapterBoss 모드는 T07 wiring 후 검증

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §Sprint 3.2
- 디자인 §3.6 챕터 BG 적용

## 5. 알려진 위험

- **씬 저장 후 SIGSEGV 위험** — 씬 저장 후 **10초+ 대기** 필수
- DontDestroyOnLoad 객체 (GameManager / AudioManager / VfxPool / DamageFloaterPool) — BossEncounter 씬에 별도 prefab wiring 필요 시 Inspector 검토
- Inspector 드래그 누락 시 ChapterBoss 분기에서 BackgroundApplier no-op (안전하지만 검증 시 BG 미적용 보고)
