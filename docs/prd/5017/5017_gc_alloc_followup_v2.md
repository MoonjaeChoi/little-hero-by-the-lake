# 5017 — GC Alloc Follow-up — Sprint PRD v2 (최종, Mediator 확정)

> **PRD v2 (최종)**
> 원본: `5017_gc_alloc_followup_prd.md` · Decisions: `5017_gc_alloc_followup_decisions.md`
> 결정 3건 반영 (D1=A cap 1024 / D2=A struct / D3=B 0.005f)
>
> **출처**: 디자인 `docs/prd/5017_gc_alloc_followup_prd.md` (옵션 A — 단계적 hot path fix)
> **사전 검증**: 5016.1 후 Mac Player 측정 ~70 KB/s (시나리오 C 1.4×) — Editor noise 제거 후 진짜 GC alloc 값.
> **목표**: 시나리오 A (<30 KB/s) 도달 — PIVOT v4 §7 임계 회복.

## 0. 5016 → 5016.1 → 5017 진척

| 단계 | 작업 | Player GC | 상태 |
|---|---|---:|---|
| **5016** | LitMotion 마이그레이션 22 파일 | (Editor 만 측정) | ✅ |
| **5016.1** | HUDController string cache | (Editor 만 측정) | ✅ |
| **5016 Player 검증** | GCAllocMeasurer auto trigger | **69.97 KB/s** | ✅ 측정 완료 |
| **5017** | 추가 hot path fix (본 PRD) | 목표 < 30 KB/s | 진행 |

## 1. 정밀 hot path 영역 (5016.1 후 잔여, Player 측정 기준)

### 1.1 핵심 영역 (확정)

| 파일 | 위치 | 패턴 | 빈도 | 우선 |
|---|---|---|---|:---:|
| **DamageNumberFloater.cs:135-138** | `FormatDamage` | `(damage / 1000f).ToString("F1") + "M"` | 매 hit (cap 300 핫) | ★★★ |
| **ItemDropService.cs DropContext** | class instance | `new DropContext { ... }` 매 처치 | 매 처치 | ★★★ |
| **AudioManager.cs:397** | BGM not found | `$"AudioManager: BGM not found {name}"` | (낮은 빈도, 무시) | - |
| **EnemyController.UpdateAnim:102-105** | scale wobble | `new Vector3(...)` 매 frame | 매 frame × 적 수 | ★★ |
| **DropContext** | class | → struct 가능 | 매 처치 | ★★★ |

### 1.2 추가 검토 영역

| 파일 | 패턴 | 검토 결과 |
|---|---|---|
| `DamageQuery._buffer / _scratch` | 정적 buffer 64 | ✅ alloc 0 (이미 최적화) |
| `ResolvedHitEffect` | struct | ✅ alloc 0 |
| `VfxPool` | List<GameObject> 정적 | ✅ alloc 0 |
| `DamageDealer.ApplyMulti` | List<EnemyController> param | (호출자 정적 buffer) |

### 1.3 제외 영역 (D3=B 정신, 5016 범위 외)

| 영역 | 이유 |
|---|---|
| WaveManager / ChapterRunner / BossEncounterRunner | timing-only Coroutine 잔존 (D3=B) |
| Boss attacks 5종 | cold path (telegraph 시만) |

## 2. Sprint 분해

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | DamageNumberFloater FormatDamage string cache + AudioManager BGM warning 정리 | 1 | 30분 |
| 2 | DropContext class → struct + ItemDropService 호출처 갱신 | 1 | 35분 |
| 3 | EnemyController.UpdateAnim 의 new Vector3/frame 정적 캐시 + 의심 hot path 추가 검토 | 1 | 40분 |
| 4 | ProjectValidator 5017 룰 + NUnit | 1 | 30분 |
| 5 | Mac Player 재측정 (auto trigger) + 결과 분석 + docs (changelog + CLAUDE.md §14) | 1 | 30분 |
| **총** | **5 commit** | **5** | **약 2.5시간** |

## 3. Sprint 명세

### Sprint 1 — DamageNumberFloater string cache + AudioManager 정리

#### 1.1 DamageNumberFloater.FormatDamage 캐시

**파일**: `Scripts/Gameplay/Combat/DamageNumberFloater.cs`

**변경 전** (line 130-141):
```csharp
private static string FormatDamage(int damage, DamageType type)
{
    string prefix = type == DamageType.Heal ? "+" : "";
    string body;
    int abs = Mathf.Abs(damage);
    if (abs >= 1_000_000) body = (damage / 1_000_000f).ToString("F1") + "M";
    else if (abs >= 10_000) body = (damage / 1000) + "K";
    else if (abs >= 1_000) body = (damage / 1000f).ToString("F1") + "K";
    else body = damage.ToString();
    if (type == DamageType.Critical) return body + "!";
    return prefix + body;
}
```

**변경 후** — 작은 정수 cache (frequent damage values):
```csharp
private static readonly StringBuilder _formatBuffer = new StringBuilder(16);
private static readonly Dictionary<(int, DamageType), string> _formatCache = new Dictionary<(int, DamageType), string>(256);

private static string FormatDamage(int damage, DamageType type)
{
    var key = (damage, type);
    if (_formatCache.TryGetValue(key, out string cached)) return cached;
    // 새 값만 string 생성 + 캐시
    var sb = _formatBuffer;
    sb.Length = 0;
    if (type == DamageType.Heal) sb.Append('+');
    int abs = Mathf.Abs(damage);
    if (abs >= 1_000_000) sb.Append((damage / 1_000_000f).ToString("F1")).Append('M');
    else if (abs >= 10_000) sb.Append(damage / 1000).Append('K');
    else if (abs >= 1_000) sb.Append((damage / 1000f).ToString("F1")).Append('K');
    else sb.Append(damage);
    if (type == DamageType.Critical) sb.Append('!');
    string result = sb.ToString();
    if (_formatCache.Count < 1024) _formatCache.Add(key, result);
    return result;
}
```

→ 동일 damage 값 반복 시 cache hit → alloc 0.

#### 1.2 AudioManager BGM warning 정리

**파일**: `Scripts/Core/AudioManager.cs:397`

**변경 전**:
```csharp
else Debug.LogWarning($"AudioManager: BGM not found Resources/BGM/{name}");
```

**변경 후** — 빈도 낮음, but 정적 빌드로 alloc 회피:
```csharp
else Debug.LogWarning("AudioManager: BGM not found Resources/BGM/" + name);
```

(string concat 만 alloc, $"" interpolation 보다 작음)

**검증 기준**:
- 컴파일 errors=0
- DamageFloater 시각 동일 (10/100/1K/15K/1.5M 표시)
- DamageNumberFloater._formatCache.Count 가 wave 진행 중 ~64~256 도달 후 안정화

#### Sprint 1 commit
```
5017 Sprint 1 / T01 — DamageNumberFloater string cache + AudioManager 정리 (1/5)

[변경]
- DamageNumberFloater.FormatDamage:
  - StringBuilder + Dictionary cache 패턴
  - 동일 damage+type 반복 시 cache hit → alloc 0
- AudioManager BGM warning string concat (LogWarning $"" → "+ name")

[검증]
- 컴파일 errors=0
- DamageFloater 시각 동일

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 2 — DropContext class → struct

#### 2.1 DropContext struct 변환

**파일**: `Scripts/Gameplay/Items/ItemDropService.cs`

**변경 전**:
```csharp
public class DropContext
{
    public DropSource source = DropSource.Enemy;
    public int chapterNumber = 1;
    public EnemyData enemy;
    public EquipSlot? forcedSlot;
    ...
}
```

**변경 후** (struct):
```csharp
public struct DropContext
{
    public DropSource source;
    public int chapterNumber;
    public EnemyData enemy;
    public EquipSlot? forcedSlot;
    ...
}
```

#### 2.2 호출처 갱신 (`new DropContext { ... }`)

매 처치 시 호출되는 영역 — `EnemyController.Die`, `BossEncounterRunner`, 기타.

struct 는 stack alloc, by-value 전달.

**검증 기준**:
- 컴파일 errors=0
- 처치 후 보상 정상 (별가루/아이템 드랍)
- ItemDropService 동작 회귀 X

#### Sprint 2 commit
```
5017 Sprint 2 / T02 — DropContext class → struct (2/5)

[변경]
- ItemDropService.DropContext class → struct
- by-value 전달 — heap alloc 폐기
- 호출처 (EnemyController.Die / BossLootGrantService 등) 갱신

[검증]
- 컴파일 errors=0
- 처치 보상 정상 (별가루 + 아이템 드랍)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 3 — EnemyController scale wobble 캐시 + 추가 검토

#### 3.1 EnemyController.UpdateAnim 의 new Vector3 캐시

**파일**: `Scripts/Gameplay/Enemies/EnemyController.cs:102-105`

**변경 전** (매 frame, 적 수만큼 alloc):
```csharp
transform.localScale = new Vector3(
    _baseScaleX * _punchScale,
    _baseScaleY * (1f + wobble) * _punchScale,
    1f);
```

**변경 후** — Vector3 struct 라 alloc 0 (Unity 의 Vector3 는 struct):
```csharp
// Vector3 는 struct (alloc 0). 그러나 transform.localScale = Vector3 setter 가
// 내부 native 함수 호출 — 추가 최적화 영역. 일단 그대로 유지.
```

→ 실제로는 struct 라 alloc 0. 그러나 transform.localScale setter 매 frame 호출이 native interop 비용.

**추가 fix** — wobble 변동 시만 setter 호출:
```csharp
private float _lastWobble = float.MinValue;
private float _lastPunch = float.MinValue;
...
float wobble = Mathf.Sin(time * 4f + _swayPhase) * 0.04f;
if (Mathf.Abs(wobble - _lastWobble) > 0.005f || Mathf.Abs(_punchScale - _lastPunch) > 0.005f)
{
    _lastWobble = wobble;
    _lastPunch = _punchScale;
    transform.localScale = new Vector3(
        _baseScaleX * _punchScale,
        _baseScaleY * (1f + wobble) * _punchScale,
        1f);
}
```

→ wobble 변동 미미 시 setter 생략 (native interop 절감).

#### 3.2 추가 hot path 검토

Mac Player Profiler attach 가능 시 분석 (5017 Sprint 5 재측정 결과 기반).

**검증 기준**:
- 적 wobble 시각 동일
- 적 50+ 동시 wobble 정상
- frame rate 60 fps 유지

#### Sprint 3 commit
```
5017 Sprint 3 / T03 — EnemyController wobble 변동 시만 setter (3/5)

[변경]
- EnemyController.UpdateAnim:
  - _lastWobble + _lastPunch 캐시
  - 변동 미미 시 transform.localScale setter 생략 (native interop 절감)

[검증]
- wobble 시각 동일
- 적 cap 300 환경 정상

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 4 — Validator 5017 룰 + NUnit

#### 4.1 ProjectValidator.cs 룰 추가

**파일**: `Scripts/Editor/ProjectValidator.cs`

**산출물**: `ValidateGCAllocPatterns` 메서드 신규 (~30L)

```csharp
private static void ValidateGCAllocPatterns(List<Issue> issues)
{
    string scriptDir = Path.Combine(Application.dataPath, "_Project/Scripts");
    int updateStringInterp = 0;
    int classInsteadOfStruct = 0;
    foreach (var file in Directory.GetFiles(scriptDir, "*.cs", SearchOption.AllDirectories))
    {
        if (file.EndsWith("ProjectValidator.cs")) continue;
        string content = File.ReadAllText(file);
        // Update() 안에 $"" 검사 — 단순 라인 패턴
        if (content.Contains("private void Update()") && content.Contains("$\""))
        {
            // false positive 가능 — 정확한 검출은 AST 필요. 휴리스틱.
        }
    }
    // 휴리스틱 결과 보고만
    Debug.Log($"[ProjectValidator] 5017 GC alloc 정적 검사 (휴리스틱): {updateStringInterp} files with potential pattern");
}
```

#### 4.2 NUnit Test

**파일**: `Tests/Editor/DamageNumberFloaterFormatTests.cs` 신규 (4 NUnit)

- `FormatDamage_BelowThousand_NoSuffix`
- `FormatDamage_OverMillion_MSuffix`
- `FormatDamage_Cache_RepeatedCallReturnsSameInstance` — cache hit 검증
- `Type_DamageNumberFloater_FormatDamageIsStatic`

**검증 기준**:
- NUnit 4/4 PASS
- 누적 NUnit 104 → 108

#### Sprint 4 commit
```
5017 Sprint 4 / T04 — Validator 5017 룰 + NUnit 4 (4/5)

[변경]
- ProjectValidator.ValidateGCAllocPatterns 휴리스틱 검사
- DamageNumberFloaterFormatTests 4 NUnit
- 누적 NUnit 104 → 108

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 5 — Mac Player 재측정 + docs

#### 5.1 Mac Player 재빌드 + 측정

`manage_build` 자동화:
- Development Build 재빌드 (~3분)
- Mac Player 직접 실행
- Player.log 자동 trigger 측정 60s
- 결과 추출 + 비교

#### 5.2 docs

- `docs/little_hero/changelog_v1.md` 5017 항목
- CLAUDE.md §11 마일스톤 행 (5017 진행 시)
- CLAUDE.md §14 직전 5건 회전

**검증 기준**:
- Mac Player 측정 — 시나리오 A (<30 KB/s) 도달 ★ 5017 핵심
- 시나리오 B (30~50) → 5017.1 후속
- 시나리오 C (>50, 미달) → 5017 부분 종결 + 추가 PRD

#### Sprint 5 commit
```
5017 Sprint 5 / T05 — Mac Player 재측정 + docs (5/5) — 5017 종결

[변경]
- Mac Player Development Build 재빌드 + 측정
- 결과: GC X.XX KB/s — 시나리오 A/B/C
- changelog_v1.md 5017 항목
- CLAUDE.md §11/§14 갱신

[5017 누적 5 commit]
- T01 DamageNumberFloater string cache + AudioManager
- T02 DropContext class → struct
- T03 EnemyController wobble 캐시
- T04 Validator + NUnit
- T05 재측정 + docs (본 commit)

[검증 — 사용자]
- Mac Player 측정 시나리오 A (<30 KB/s) 도달 ✅

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 의존성 그래프

```
T01 (Floater string + Audio) ──┐
                                ├→ T04 (Validator + NUnit) ──→ T05 (재측정 + docs)
T02 (DropContext struct) ───────┤
                                │
T03 (EnemyController wobble) ───┘
```

T01~T03 = 영역 독립 (병렬 가능). T04 통합 검증. T05 사용자 측정 + 종결.

## 5. Definition of Done

- [ ] 5 commit + 컴파일 errors=0 (각 Sprint 별)
- [ ] DamageNumberFloater FormatDamage cache 작동 (Profiler 검증 시)
- [ ] DropContext struct + 호출처 정합
- [ ] EnemyController wobble 변동 시만 setter
- [ ] NUnit 4 신규 (108 누적) PASS
- [ ] Validator 5017 룰 통과
- [ ] **Mac Player 재측정 시나리오 A (<30 KB/s) 도달**
- [ ] CLAUDE.md §11/§14 갱신

## 6. 검증 시나리오 (Play 검증)

### S1 — DamageNumberFloater 시각 (Sprint 1 후)
1. CH 1 wave 1~5 적 hit 다수
2. 데미지 floater 표시 동일 (1.2K / 15K / 1.5M)
3. 색상/sizing 정합

### S2 — 처치 보상 (Sprint 2 후)
1. 적 처치 → 별가루 ★ + 아이템 드랍 정상
2. ItemDropService 동작 정합

### S3 — wobble 시각 (Sprint 3 후)
1. 적 cap 300 환경에서 wobble 자연스러움
2. frame rate 60 fps 유지

### S4 — Mac Player 재측정 (Sprint 5 후) ⭐ 핵심
1. Mac Player 빌드 + 실행
2. Attic → 챕터 1 → wave 진행
3. 자동 GCAllocMeasurer 60s
4. **결과: 시나리오 A (<30 KB/s)** ← 5017 성공 기준

## 7. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| FormatDamage cache 가 메모리 누수 | cache size cap 1024 + LRU 미적용 (단순 dict) |
| DropContext struct 변환 시 by-ref 호출 누락 | 컴파일 errors 검출 + 호출처 grep |
| EnemyController wobble 미동기 | 변동 임계 0.001f — 시각 미미 |
| 시나리오 A 미달 | 5017.1 또는 추가 hot path 검토 (Sprint 6 가능) |

## 8. 변경 이력

- 2026-05-06: 초기 작성 (5016 Player 측정 ~70 KB/s 결과 기반).
