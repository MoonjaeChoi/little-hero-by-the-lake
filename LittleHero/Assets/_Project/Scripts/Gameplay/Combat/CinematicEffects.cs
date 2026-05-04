using System.Collections;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 크리티컬 시 hit pause + screen shake 글로벌 매니저 (cinematic_combat_v1.md §5).
    /// Cinemachine 의존 회피 — Camera.transform 직접 흔들기.
    /// RuntimeInitializeOnLoadMethod 자동 부트스트랩 (AudioManager 패턴 답습).
    /// </summary>
    public class CinematicEffects : MonoBehaviour
    {
        public static CinematicEffects Instance { get; private set; }

        private Coroutine _hitPauseCo;
        private Coroutine _shakeCo;

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
            if (_hitPauseCo != null) StopCoroutine(_hitPauseCo);
            _hitPauseCo = StartCoroutine(HitPauseRoutine(duration));
        }

        private IEnumerator HitPauseRoutine(float duration)
        {
            float prevScale = Time.timeScale;
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = prevScale > 0.06f ? prevScale : 1f;
            _hitPauseCo = null;
        }

        /// <summary>Screen shake — Camera.main.transform 흔들기. amplitude 0.1~0.5 권장.</summary>
        public void PlayScreenShake(float amplitude = 0.3f, float duration = 0.2f)
        {
            if (_shakeCo != null) StopCoroutine(_shakeCo);
            _shakeCo = StartCoroutine(ShakeRoutine(amplitude, duration));
        }

        private IEnumerator ShakeRoutine(float amplitude, float duration)
        {
            var cam = Camera.main;
            if (cam == null) { _shakeCo = null; yield break; }
            var t = cam.transform;
            Vector3 origin = t.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = 1f - (elapsed / duration);
                float ox = (Random.value - 0.5f) * 2f * amplitude * k;
                float oy = (Random.value - 0.5f) * 2f * amplitude * k;
                t.localPosition = origin + new Vector3(ox, oy, 0f);
                yield return null;
            }
            t.localPosition = origin;
            _shakeCo = null;
        }

        // 5007 T09 — Camera zoom (orthographicSize 변경, subtle 1.05× 권장)
        private Coroutine _zoomCo;

        /// <summary>5007 — 크리 시 subtle camera zoom (orthographicSize 변경). zoomMul 1.05 권장.</summary>
        public void PlayCameraZoom(float zoomMul = 1.05f, float upDuration = 0.05f, float downDuration = 0.15f)
        {
            if (_zoomCo != null) StopCoroutine(_zoomCo);
            _zoomCo = StartCoroutine(ZoomRoutine(zoomMul, upDuration, downDuration));
        }

        private IEnumerator ZoomRoutine(float zoomMul, float upDuration, float downDuration)
        {
            var cam = Camera.main;
            if (cam == null || !cam.orthographic) { _zoomCo = null; yield break; }
            float origin = cam.orthographicSize;
            float target = origin / Mathf.Max(zoomMul, 1.001f);  // zoomMul 클수록 size 작아짐 (확대)

            float t = 0f;
            while (t < upDuration)
            {
                t += Time.unscaledDeltaTime;
                cam.orthographicSize = Mathf.Lerp(origin, target, t / upDuration);
                yield return null;
            }
            t = 0f;
            while (t < downDuration)
            {
                t += Time.unscaledDeltaTime;
                cam.orthographicSize = Mathf.Lerp(target, origin, t / downDuration);
                yield return null;
            }
            cam.orthographicSize = origin;
            _zoomCo = null;
        }
    }
}
