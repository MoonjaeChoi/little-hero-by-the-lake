# Player 빌드 측정 가이드 (5016 D2=A + 5017)

> **목적**: Editor noise 제거 + 정확한 GC alloc 측정 + App 용량 임계 검증.
> **사용**: 5016 D2=A 시나리오 + 5017 후속 검증.

## 1. Mac Player 빌드

### 1.1 빌드 실행

1. Unity Editor → File → Build Settings (Cmd+Shift+B)
2. Platform: **PC, Mac & Linux Standalone** → Target Platform: **macOS**
3. **Build** 클릭
4. 출력 경로: `LittleHero/Builds/Mac/` (생성 권장)
5. 빌드 시간: **5~10분** (첫 빌드 — Burst AOT 컴파일 포함)

### 1.2 빌드 후 용량 측정

```bash
du -sh /Users/memmem/git/little-hero-by-the-lake/LittleHero/Builds/Mac/*.app
```

**기대 결과**: < **150 MB** (CLAUDE.md §7 임계)

| 결과 | Action |
|---|---|
| < 150 MB ✅ | D2=A 시나리오 통과 |
| > 150 MB ❌ | 5015c 신규 PRD (Burst BasicMode 또는 DOTween/UniTask 폴백) |

## 2. iOS Player 빌드 (App Store 제출 검증)

### 2.1 Xcode 프로젝트 생성

1. Build Settings → Platform: **iOS**
2. **Build** 클릭 → `LittleHero/Builds/iOS/Unity-iPhone.xcworkspace` 생성
3. Xcode 자동 열림

### 2.2 IPA 빌드

```bash
xcodebuild -workspace LittleHero/Builds/iOS/Unity-iPhone.xcworkspace \
  -scheme Unity-iPhone -configuration Release \
  -archivePath build/LittleHero.xcarchive archive
```

### 2.3 IPA 용량 측정

```bash
du -sh build/LittleHero.xcarchive
```

App Store 임계: **150 MB** (다운로드 셀룰러 회피 한계).

## 3. Player 모드 GC 측정 (정확한 데이터)

GCAllocMeasurer 는 Player 빌드에서도 작동 (Editor menu 만 사라짐). 하지만 직접 호출 X.

### 3.1 Player 빌드용 진입점 추가 (선택)

`GCAllocMeasurer.cs` 의 `StartMeasure()` 가 `#if UNITY_EDITOR` 블록 — Player 빌드 시 호출 X.

해결: Player Mode 자동 트리거 추가 (선택):

```csharp
#if !UNITY_EDITOR
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
private static void AutoStartInPlayer()
{
    // Chapter 진입 후 60초 측정 자동 시작
    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Chapter") return;
    StartMeasure();
}
#endif
```

→ 5017 후속 사이클에서 추가 가능.

### 3.2 Profiler 연결 (Editor → Player)

**더 정확한 측정**: Editor Profiler 를 Player 에 연결.

1. Player 실행 (Mac Standalone)
2. Editor Profiler → 우측 상단 "Editor" 드롭다운 → "Mac Standalone <hostname>" 선택
3. 빨간 ● Record
4. 60초 측정 → frame 별 GC alloc

→ **Editor noise 없음, 빌드 최적화 (IL2CPP) 적용된 측정**.

## 4. 측정 결과 비교 (Editor vs Player)

| 측정 환경 | GC Alloc/sec | 비고 |
|---|---:|---|
| **Editor (Profiler off)** | ~3,670 KB/s | ProfilerRecorder + Editor noise |
| **Editor + Profiler attached** | ~? KB/s | 추가 Profiler overhead |
| **Player (IL2CPP)** | ~? KB/s | **실제 사용자 환경** ← 신뢰 |
| **Player + Profiler attached** | ~? KB/s | TestFlight 환경 권장 |

→ 5017 핵심 가치: **Player 측정** 결과가 시나리오 A (<30 KB/s) 도달.

## 5. App 용량 분석 (Build Report)

Player 빌드 후:

1. **Window → Analysis → Build Report** (Unity 2021+)
2. 용량 분포 확인:
   - Scripts (Burst native): ~10~20 MB
   - Assets (sprite + sound): ~30~50 MB
   - Unity Engine: ~50 MB
   - 합계: < 150 MB 검증

| 컴포넌트 | 임계 | 5016 후 |
|---|---:|---|
| Scripts (Burst) | < 30 MB | 측정 |
| Sprite + Sound | < 70 MB | 측정 |
| Engine | < 60 MB | 변경 없음 |
| **합계** | **< 150 MB** | **D2=A 핵심** |

## 6. 결과 보고 형식

```
[Player 빌드 측정]
플랫폼: Mac (또는 iOS)
빌드 시간: X분
App 용량: Y MB
기준 (150 MB): ✅ / ❌

[Player GC Alloc (60s)]
GC Alloc/sec: X.XX KB/s
시나리오: A (<30) / B (30~50) / C (>50)

[비교]
Editor: 3,670 KB/s → Player: X KB/s (Editor noise 절감 Y%)
```

## 7. 변경 이력

- 2026-05-05: 초기 작성 (5016 D2=A + 5017 검증용)
