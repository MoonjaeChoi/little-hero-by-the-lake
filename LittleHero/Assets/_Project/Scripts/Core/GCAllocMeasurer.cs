using System.Collections;
using Unity.Profiling;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LittleHero
{
    /// <summary>
    /// 5015b 사전조사 — Profiler GC Alloc/sec 자동 측정 도구.
    /// Play Mode 진입 후 메뉴 실행 → 60초 측정 + 콘솔 KB/s 출력.
    ///
    /// 사용 순서:
    /// 1. Play Mode 진입 (Attic → CH 1 wave 5 권장)
    /// 2. Tools/LittleHero/Measure GC Alloc (60s) 메뉴
    /// 3. 측정 동안 cap 300 환경 진행 (MiniBoss + 다수 hit)
    /// 4. 60초 후 콘솔 결과 확인
    ///
    /// 임계 (5015b 디자인 분기):
    ///   - GC < 30 KB/s → 5015b 보류 (IEnumerator 마진 충분)
    ///   - 30~50 KB/s → 5015b 부분 (3 핫 패스 파일)
    ///   - > 50 KB/s → 5015b 전체 (22 파일)
    ///
    /// MonoBehaviour 라 runtime 자산 (Scripts/Core/) 이 됨. MenuItem 은 #if UNITY_EDITOR.
    /// </summary>
    public class GCAllocMeasurer : MonoBehaviour
    {
        private const float MEASURE_DURATION = 60f;

#if UNITY_EDITOR
        [MenuItem("Tools/LittleHero/Measure GC Alloc (60s)")]
        public static void StartMeasure()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[GCAllocMeasurer] Play Mode 진입 후 실행 필요. (Attic → CH 1 wave 5 권장)");
                return;
            }
            SpawnAndMeasure();
        }
#endif

        // 5017 X — Player 자동 trigger. Chapter 씬 진입 시 자동 측정 (Profiler attach 의존성 X).
        // Player.log 에 KB/s 결과 출력 — Editor 외부 측정 가능.
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStartInPlayer()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
            Debug.Log("[GCAllocMeasurer] Player 자동 측정 활성 — Chapter 씬 진입 시 5초 후 측정 시작 (60s)");
        }

        private static void OnSceneChanged(UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
        {
            if (newScene.name != "Chapter") return;
            // 한 번만 측정 — 중복 방지
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
            SpawnAndMeasureDelayed(5f);
        }

        private static void SpawnAndMeasureDelayed(float delay)
        {
            var go = new GameObject("[GCAllocMeasurer-Auto]");
            DontDestroyOnLoad(go);
            var measurer = go.AddComponent<GCAllocMeasurer>();
            measurer.StartCoroutine(measurer.MeasureDelayed(delay));
        }

        private IEnumerator MeasureDelayed(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            yield return Measure();
        }
#endif

        private static void SpawnAndMeasure()
        {
            var go = new GameObject("[GCAllocMeasurer]");
            DontDestroyOnLoad(go);
            var measurer = go.AddComponent<GCAllocMeasurer>();
            measurer.StartCoroutine(measurer.Measure());
        }

        private IEnumerator Measure()
        {
            // Profiler.Recorder API (Unity 2021+) — 정확한 frame 별 GC Alloc 추적
            var gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            // 안정화 대기 (1초)
            yield return new WaitForSecondsRealtime(1f);
            if (!gcRecorder.Valid)
            {
                Debug.LogError("[GCAllocMeasurer] ProfilerRecorder invalid — Unity 2021+ 필요");
                Destroy(gameObject);
                yield break;
            }

            long accumGc = 0;
            long peakFrame = 0;
            int frameCount = 0;
            float elapsed = 0f;

            Debug.Log($"[GCAllocMeasurer] 측정 시작 — {MEASURE_DURATION:F0}초 / cap 300 환경 + 다수 hit 권장");

            while (elapsed < MEASURE_DURATION)
            {
                long frameAlloc = gcRecorder.LastValue;
                accumGc += frameAlloc;
                if (frameAlloc > peakFrame) peakFrame = frameAlloc;
                frameCount++;
                elapsed += Time.unscaledDeltaTime;

                // 매 10초 진행 상황 로그
                if (frameCount % 600 == 0)
                {
                    float curKbps = accumGc / 1024f / elapsed;
                    Debug.Log($"[GCAllocMeasurer] {elapsed:F0}s 경과 — 누적 {accumGc / 1024f:F1} KB / {curKbps:F2} KB/s");
                }
                yield return null;
            }

            gcRecorder.Dispose();

            float kbPerSec = accumGc / 1024f / elapsed;
            float peakKb = peakFrame / 1024f;
            float avgFrameKb = (accumGc / 1024f) / frameCount;

            Debug.Log("=== [GCAllocMeasurer] 측정 결과 ===");
            Debug.Log($"  측정 시간: {elapsed:F1}초 / frame {frameCount}");
            Debug.Log($"  누적 GC Alloc: {accumGc / 1024f:F1} KB ({accumGc} bytes)");
            Debug.Log($"  GC Alloc/sec: {kbPerSec:F2} KB/s");
            Debug.Log($"  Average per frame: {avgFrameKb * 1024f:F0} bytes/frame");
            Debug.Log($"  Peak frame: {peakKb:F2} KB");
            Debug.Log("=== 5015b 시나리오 분기 ===");
            if (kbPerSec < 30f)
                Debug.Log($"  → 시나리오 A (< 30 KB/s) — **5015b 보류** (IEnumerator 마진 충분)");
            else if (kbPerSec < 50f)
                Debug.Log($"  → 시나리오 B (30~50 KB/s) — **5015b 부분 마이그레이션** (3 핫 패스 파일)");
            else
                Debug.Log($"  → 시나리오 C (> 50 KB/s) — **5015b 전체 마이그레이션** (22 파일, 6 sprint)");

            Destroy(gameObject);
        }
    }
}
