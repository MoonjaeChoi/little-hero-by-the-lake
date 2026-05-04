# T05 — Profiler 측정 + docs

> **레이어**: docs
> **의존성**: T01~T04
> **예상 시간**: 25분 / 1 commit
> **PRD 출처**: `5013_enemy_visibility_culler_v1_v2.md` §2 Sprint 5

## 1. 작업 항목

### 1.1 Profiler 측정 (Rebuttal §3.2 accepted)

**환경**: Unity Editor Play Mode (또는 iPhone 12 빌드 시)

**시나리오**: cap 300 wave 환경
- test scene: `Tests/Editor/Scenes/StressTest_300Enemies.unity` (없으면 신규)
- 또는 wave 30+ chapter (CH 1 wave 30 = ~150~200 적)

**도구**:
- Unity Profiler (CPU + Rendering 탭)
- Xcode Instruments (iOS 빌드 시)

**측정 항목**:
- frame time (ms)
- GPU draw call (Rendering 탭)
- CPU Update 호출 빈도 (CPU 탭, EnemyController.Update)
- GC allocation/sec

**비교**: cull 도입 전후 frame 평균 (1분 측정)

**기대 (디자인 §1.3)**:
- GPU draw call **-50%**
- CPU Update **-30%**
- frame time < 16ms (60fps)
- GC <50KB/s

### 1.2 측정 보고서 (선택)

**경로**: `docs/little_hero/profiler_5013_visibility_cull.md` (신규)

cull 전/후 비교 표 + 스크린샷 (해당 시).

### 1.3 docs 갱신

#### `docs/little_hero/changelog_v1.md` 5013 항목 추가

```markdown
- 2026-05-XX: **5013 enemy_visibility_culler_v1** — 화면 외 적 SpriteRenderer/Animator/Manager cull (D1=A 0.1s throttle / D2=A 1.5x margin, 5 commit, NUnit 81→87). PIVOT v4 §7 cap 300 60fps 마진 강화. EnemyVisibilityCuller 정적 헬퍼 + EnemyAnimationManager HashSet O(1) + ProjectValidator EnemyController_Visibility_Check_Present 룰 + SceneTransition InvalidateCache 자동 hook. Profiler 결과 GPU draw call -X% / CPU Update -Y% / frame time Z ms.
```

#### `CLAUDE.md` §11 마일스톤 추가

```markdown
| **enemy_visibility_culler_v1** | Off-screen sprite/animator cull (5013, 60fps 마진 + GC 0) | ✅ 2026-05-XX |
```

#### `CLAUDE.md` §12 시스템 현황 갱신

- **Tests/Editor**: NUnit 81 → 87 (6 EnemyVisibilityCullerTests 추가)
- **Gameplay → Enemies** 에 EnemyVisibilityCuller 정적 헬퍼 명시

#### `CLAUDE.md` §14 직전 5건 회전

- 5013 추가 (가장 위)
- 가장 오래된 1건 제거 (5005 또는 5004 — 직전 5건 maintain)

## 2. 검증 기준

### 2.1 docs 정합성
- changelog 5013 항목 존재
- CLAUDE.md §11/§12/§14 갱신 정합

### 2.2 Profiler 결과
- 60fps 유지 (300 적 환경 frame time < 16ms)
- GPU draw call 절감 (cull 도입 전 대비)
- GC <50KB/s (PIVOT v4 §7)

### 2.3 ProjectValidator 정합
- `Tools/LittleHero/Validate Project` 실행 → errors=0, warnings 변동 ±0

## 3. commit 메시지

```
5013 Sprint 5 / T05 — Profiler 측정 + docs (5/5) — 5013 종결

[변경]
- docs/little_hero/changelog_v1.md — 5013 항목 추가
- CLAUDE.md §11 마일스톤 + §12 시스템 현황 + §14 직전 5건 회전
- (선택) docs/little_hero/profiler_5013_visibility_cull.md — 측정 보고서

[Profiler 결과]
- GPU draw call -X% (cull 200 적, cap 300 환경)
- CPU Update -Y% (off-screen 적 Update skip)
- frame time Z ms (60fps 마진 ↑)
- GC allocation/sec <50KB (§15.11 정합)

[5013 누적 5 commit]
- T01 EnemyVisibilityCuller 정적 헬퍼 + Scene hook
- T02 EnemyController.Update visibility hook
- T03 EnemyAnimationManager HashSet
- T04 NUnit 6 + Validator 룰
- T05 Profiler + docs

[5013 종결]
- Phase 1~5 완료, Phase 6 Evaluate 진입 가능
- iPhone 12 60fps 유지 마진 ↑

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 5
- Rebuttal §3.2 (Profiler 절차 구체화)
- CLAUDE.md §7 PIVOT v4 cap 300 / §15.11 GC 50KB

## 5. 알려진 위험

- **Profiler 측정 환경 차이** — Editor / iPhone 12 frame time 다를 수 있음. 실제 빌드 검증 권장
- **GC 50KB/s 측정** — Unity Profiler 의 GC.Alloc 항목 1분 평균
- **CPU Update -30% 추정 오차** — 실측 결과 ±10% 변동 가능. 절대 수치 X 비교만 의미
- **Profiler 보고서 작성 부담** — 선택 사항, changelog 만 필수
