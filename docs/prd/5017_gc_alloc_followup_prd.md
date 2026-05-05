# 5017 — GC Alloc Follow-up (5016 후속) (디자인)

> **목적**: 5016 LitMotion 마이그레이션 후 GC alloc 측정 결과 시나리오 C (>50 KB/s) 잔존 → 추가 hot path 영역 식별 + fix.
> 5016.1 (HUDController string alloc cache) 이후 측정 효과 검증 + 잔여 영역 정리.
> **우선순위**: 높음 (PIVOT v4 §7 GC <50 KB/s 임계 회복).

## 0. 결정 이력

- 2026-05-05: 5016 LitMotion 마이그레이션 완료 (7 commit, ★0~20 cap 정합)
- 2026-05-05: GCAllocMeasurer 측정 결과 ~3,670 KB/s (시나리오 C ×73) — 사용자 보고
- 2026-05-05: HUDController string interpolation 매 frame alloc 발견 (Update → RefreshXpBar)
- 2026-05-05: **5016.1 — HUDController string cache fix 적용** (commit 76ec998)
- 2026-05-05: 5017 — 5016.1 후 잔여 hot path 분석 + 추가 fix (본 PRD)

## 1. 5016 → 5016.1 → 5017 진척

| 단계 | 작업 | GC 측정 (Editor) |
|---|---|---:|
| **5015** | RESERVED (DOTween 가정 무효) | - |
| **5015b** | 사전조사 (시나리오 C 측정) | >50 KB/s |
| **5016** | LitMotion 마이그레이션 22 파일 | ~3,670 KB/s (잔존) |
| **5016.1** | HUDController string cache | 측정 진행 중 |
| **5017** | 추가 hot path fix (본 PRD) | 목표 < 30 KB/s |

## 2. 의심 hot path 영역 (5016.1 후 잔여)

### 2.1 string interpolation 매 frame / 매 hit

| 영역 | 위치 | 빈도 | 5016.1 처리 |
|---|---|---|---|
| HUDController.Update | Lv {p.Level} 매 frame | 60fps | ✅ 5016.1 cache |
| HUDController.UpdateHealth | $"{cur} / {max}" 매 hit | hit 빈도 ↑ | ✅ 5016.1 cache |
| HUDController.RefreshWave | $"웨이브 {n}" | 낮은 빈도 | ✅ 5016.1 cache |
| HUDController.UpdateStarDust | $"별가루 ★ {n}" | drop 빈도 | ✅ 5016.1 cache |
| **DamageNumberFloater.Initialize** | $"{damage}!" 매 hit | hit 빈도 매우 ↑ | ⚠️ **확인 필요** |
| **EnhanceModalPanel / SlotStarforceView** | $"⭐ {lv}" 등 | 낮은 빈도 (UI 진입 시) | 무시 가능 |
| **WaveTransitionToast** | $"웨이브 {n} 완료!" | wave 전환 시 | 무시 가능 (LMotion bind) |

### 2.2 매 frame Update 의 GC alloc

| 영역 | 위치 | 검토 필요 |
|---|---|---|
| **EnemyAnimationManager.Update** | static List<>(1024) — alloc 0 | ✅ |
| **PlayerController** | Update / FixedUpdate | ⚠️ |
| **DamageQuery** | FindNearest / FindEnemiesInCircle | ⚠️ |
| **DamageDealer** | ApplyMulti | ⚠️ |
| **GameManager** | Update | ⚠️ |
| **PlayerStats** | UpdateStat | ⚠️ |

### 2.3 string concat / new[] / new List

| 영역 | 위치 | 빈도 |
|---|---|---|
| **AudioManager.Play** | string SFX 키 lookup | hit 빈도 ↑ |
| **VfxPool.SpawnHitStar** | new GameObject? | hit 빈도 ↑ |
| **DamageFloaterPool.Spawn** | string 또는 new[]? | hit 빈도 ↑ |
| **EnemySpawner** | new[] / List.Add | wave 진입 시 |

### 2.4 Coroutine 잔존 (D3=B 정신 — 5016 범위 외)

| 영역 | 잔존 이유 |
|---|---|
| WaveManager.SpawnRoutine | timing-only Sequencer (D3=B) |
| ChapterRunner.WaveRestCoroutine | timing-only (D3=B) |
| BossEncounterRunner | timing-only (D3=B) |

→ 5017 범위 검토 (필요 시 마이그레이션, 그러나 GC 효과 미미 추정).

## 3. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A (★권장)** | **단계적 fix — DamageNumberFloater + AudioManager + DamageQuery + Spawner** | 가장 핫한 hit path 영역 우선 | 4~5 sprint |
| B | 전체 hot path 검토 (10+ 영역) | 완전 cleanup | 시간 큼, scope creep 위험 |
| C | 5016.1 후 측정 결과 보고 무엇이 시나리오 A 도달이면 5017 폐기 | 작업 X | 측정 비신뢰성 (Editor noise) |
| D | Player 빌드 후 측정 (D2=A 결합) | 정확한 측정 | 빌드 시간 큼 |

> 권장 **A** — 핫 패스 우선 fix + 측정 결과 따라 후속 sprint 추가/축소.

## 4. 권장 설계 (CLAUDE.md §15 정합)

### 4.1 string alloc 패턴

```csharp
// ❌ 매 frame alloc
text.text = $"{cur} / {max}";

// ✅ cache + 변동 시만
private int _cachedCur, _cachedMax;
if (cur != _cachedCur || max != _cachedMax) {
    _cachedCur = cur; _cachedMax = max;
    text.text = $"{cur} / {max}";
}
```

### 4.2 매 frame Update 의 LINQ / new[]

```csharp
// ❌ Update 매 frame
var nearest = enemies.OrderBy(e => Vector2.Distance(e.pos, p)).First();

// ✅ for + 정적 buffer
EnemyController nearest = null;
float minDist = float.MaxValue;
for (int i = 0; i < _activeBuffer.Count; i++) {
    float d = ...;
    if (d < minDist) { minDist = d; nearest = _activeBuffer[i]; }
}
```

### 4.3 ProjectValidator 룰 추가

```csharp
[5017] ValidateGCAllocPatterns
- Update() 의 string interpolation $"" 패턴 검출 → 경고
- Update() 의 new[] / new List<> 패턴 검출 → 경고 (정적 buffer 권장)
- LINQ Where/OrderBy in hot path → 경고
```

## 5. Sprint 분해

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | DamageNumberFloater + AudioManager hit path string cache | 1 | 30분 |
| 2 | DamageQuery / DamageDealer / EnemySpawner Update 핫 패스 검토 | 1 | 40분 |
| 3 | VfxPool / DamageFloaterPool 매 hit alloc 검토 | 1 | 30분 |
| 4 | ProjectValidator 5017 룰 + NUnit | 1 | 30분 |
| 5 | 사용자 재측정 (Attic→챕터, GCAllocMeasurer) + docs (changelog + CLAUDE.md) | 1 | 25분 |
| **총** | **~18 영역 검토 + 5 commit** | **5** | **약 2.5시간** |

## 6. 검증 시나리오 (Play 검증)

### S1 — DamageNumberFloater + AudioManager (Sprint 1 후)
1. CH 1 wave 1 적 hit 다수
2. 데미지 floater 시각 동일
3. 콘솔 로그 ↓ (string alloc 추정 차이)

### S2 — DamageQuery / DamageDealer (Sprint 2 후)
1. cap 300 환경 적 spawn
2. 60fps 유지

### S3 — GC 재측정 (Sprint 5 후) ⭐ 핵심
1. Attic 진입 → 캐릭터 + 챕터 1 선택
2. Chapter 진입 wave 1~5 진행 (적 cap 300)
3. Tools/LittleHero/Measure GC Alloc (60s) 실행
4. **시나리오 A (< 30 KB/s) 도달** ← 5017 성공

## 7. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 시나리오 B 도달 (30~50) | 추가 sprint (5017.1) |
| 시나리오 C 잔존 | Player 빌드 측정 (D 옵션) — Editor noise 제거 후 검증 |
| 시각 회귀 (string text 미갱신) | 캐시 무효화 누락 검증 (level 변경 시 캐시 invalidate) |
| Validator 신규 위반 | 기존 hot path 회피 — 점진적 fix |

## 8. 변경 이력

- 2026-05-05: 초기 작성 (5016 후속, 사용자 측정 시나리오 C 잔존 기반).
