# Profiler GC Alloc 분석 가이드 (5016/5017 검증용)

> **목적**: GCAllocMeasurer 가 잡은 KB/s 의 **정확한 callstack 식별** + fix 영역 정밀 결정.
> **사용**: 5016/5017 검증 시 사용자 직접 진행. 결과 보고 → Claude 분석.

## 1. Unity Profiler 진입

1. Unity Editor → **Window → Analysis → Profiler** (Ctrl+7 / Cmd+7)
2. Profiler 창 열림 — 6 모듈 (CPU / Rendering / Memory / Audio / Video / Physics)
3. **CPU Usage** 모듈 선택

## 2. 측정 단계

### 2.1 사전 준비

1. Unity Editor → File → Open Scene → **Attic.unity**
2. Profiler 창 → ⚪ Record 버튼 (빨간 ●로 변경)
3. ▶ Play 버튼 (Editor)
4. Attic 화면 → 캐릭터 1 선택 → 챕터 1 진입
5. Chapter 씬 wave 1 ~ 5 진행 (~30s, 적 cap 300 도달)

### 2.2 GC Alloc Hot Frame 식별

Profiler CPU Usage:
- **Hierarchy** 탭 (시간 순 메서드 callstack)
- **GC Alloc** 컬럼 정렬 (큰 값 → 작은 값)
- 매 frame 의 GC Alloc 합계 표시 (frame 별 그래프)

상단 GC Alloc 큰 영역:
- `string.Concat` / `string.Format` / `String.CreateFromChar` — string interpolation
- `System.Collections.Generic.List<T>.Add` / `new T[]` — 컬렉션 alloc
- `Boxing` — value type → object 변환
- `IEnumerator.MoveNext` — Coroutine state machine
- `Closure / Lambda` — 람다 캡처

## 3. callstack 추출 + 보고

### 3.1 결과 수집

1. 측정 30초 ~ 60초 진행
2. ⏸ Pause (Profiler) — frame 별 분석 가능
3. 가장 GC 큰 frame 클릭 → Hierarchy 의 callstack 자세히
4. 상위 5 영역 메서드명 + GC Alloc 값 메모

### 3.2 보고 형식

```
[Profiler 분석 결과]
프레임 평균 GC Alloc: X KB/frame
Peak frame: Y KB

상위 5 hot path:
1. UnityEngine.UI.Text.set_text → X.X KB (호출 N회)
2. System.String.Format → Y.Y KB
3. ...

가장 큰 의심 메서드:
  파일: <FullClass.Method>
  라인: <approximate>
  GC Alloc: X.X KB
```

## 4. Deep Profile (선택)

기본 Profiler 는 Top-level 만. **Deep Profile** 활성화 시 매 메서드 callstack:

1. Profiler 창 → **Edit → Project Settings → Editor → Profiler**
2. **Deep Profile** 체크박스 활성화
3. 측정 재실행 — 정확한 callstack
4. ⚠️ 30~50% 성능 저하 (측정 시 정상)

## 5. 자주 발견되는 GC Alloc 영역 (Little Hero)

| 패턴 | 의심 영역 | 5016/5017 처리 |
|---|---|---|
| `Text.set_text` 매 frame | UI 패널 (HUD / Floater) | 5016.1 cache |
| `String.Format` 매 hit | DamageNumberFloater / SFX key | 5017 후속 |
| `new Vector3` in loop | DamageNumberFloater shake (5016 T04 fix) | ✅ |
| `Mathf.Lerp` cumulative | Coroutine state machine | ✅ 5016 |
| `GameObject.FindWithTag` 매 frame | PlayerController / EnemyAI | 5017 후속 |
| `Linq Where/OrderBy` | DamageQuery (정적 buffer 사용 — OK) | ✅ |

## 6. 결과에 따른 Claude 분석

사용자가 Profiler 결과 보고 시 Claude 분석:

| 보고 패턴 | 분석 + Action |
|---|---|
| `Text.set_text` 큰 alloc | UI cache 패턴 fix (HUDController 답습) |
| `String.Format` 큰 alloc | string cache 또는 cache StringBuilder |
| `new[]` / `new List` | 정적 buffer 마이그레이션 |
| `Coroutine.MoveNext` | LMotion 마이그레이션 (D3=B 정신 외) |
| `GameObject.Find*` | Awake 1회 캐시 + PlayerFinder 패턴 |

## 7. 변경 이력

- 2026-05-05: 초기 작성 (5016/5017 검증용)
