using LitMotion;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 크리티컬 시 hit pause + screen shake 글로벌 매니저 (cinematic_combat_v1.md §5).
    /// Cinemachine 의존 회피 — Camera.transform 직접 흔들기.
    /// RuntimeInitializeOnLoadMethod 자동 부트스트랩 (AudioManager 패턴 답습).
    /// 5016 T03 — IEnumerator + Lerp → LitMotion (state machine alloc 0 + LMotion.Shake new Vector3 alloc 폐기)
    /// </summary>
    public class CinematicEffects : MonoBehaviour
    {
        public static CinematicEffects Instance { get; private set; }

        // 5016 T03 — Coroutine → MotionHandle (state machine alloc 0)
        private MotionHandle _hitPauseHandle;
        private MotionHandle _shakeHandle;
        private MotionHandle _zoomHandle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("CinematicEffects");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<CinematicEffects>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>Hit pause — Time.timeScale 0.05 → 1.0, 실시간 duration 초.</summary>
        public void PlayHitPause(float duration = 0.08f)
        {
            if (_hitPauseHandle.IsActive()) _hitPauseHandle.Cancel();
            float prevScale = Time.timeScale > 0.06f ? Time.timeScale : 1f;
            Time.timeScale = 0.05f;
            // Use LMotion as a timer with WithOnComplete to restore timeScale.
            _hitPauseHandle = LMotion.Create(0f, 1f, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() => Time.timeScale = prevScale)
                .WithOnCancel(() => Time.timeScale = prevScale)
                .RunWithoutBinding();
        }

        /// <summary>Screen shake — Camera.main.transform 흔들기. amplitude 0.1~0.5 권장.</summary>
        public void PlayScreenShake(float amplitude = 0.3f, float duration = 0.2f)
        {
            var cam = Camera.main;
            if (cam == null) return;
            var t = cam.transform;
            Vector3 origin = t.localPosition;
            if (_shakeHandle.IsActive()) _shakeHandle.Cancel();
            // LMotion.Shake.Create 의 strength = Vector3 (각 축 amplitude). 0 alloc.
            _shakeHandle = LMotion.Shake.Create(Vector3.zero, new Vector3(amplitude, amplitude, 0f), duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() => t.localPosition = origin)
                .WithOnCancel(() => t.localPosition = origin)
                .Bind(t, (offset, target) => target.localPosition = origin + offset);
        }

        /// <summary>5007 — 크리 시 subtle camera zoom (orthographicSize 변경). zoomMul 1.05 권장.</summary>
        public void PlayCameraZoom(float zoomMul = 1.05f, float upDuration = 0.05f, float downDuration = 0.15f)
        {
            var cam = Camera.main;
            if (cam == null || !cam.orthographic) return;
            float origin = cam.orthographicSize;
            float target = origin / Mathf.Max(zoomMul, 1.001f);

            if (_zoomHandle.IsActive()) _zoomHandle.Cancel();
            _zoomHandle = LSequence.Create()
                .Append(LMotion.Create(origin, target, upDuration).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale).Bind(cam, (s, c) => c.orthographicSize = s))
                .Append(LMotion.Create(target, origin, downDuration).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale).Bind(cam, (s, c) => c.orthographicSize = s))
                .Run();
        }
    }
}
