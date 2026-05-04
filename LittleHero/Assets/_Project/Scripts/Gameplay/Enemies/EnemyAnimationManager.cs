using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v3 Phase 2 — 단일 Update 패턴 (game_pivot_v2.md GC 우려 해결).
    ///
    /// 1,000 적 × MonoBehaviour Update = Unity native↔managed callback 1,000번/frame.
    /// → EnemyAnimationManager 단일 Update 가 일괄 처리. 시각 동일, CPU 30~50% 감소.
    ///
    /// EnemyController.Initialize 시 Register, Die 시 Unregister.
    /// </summary>
    public class EnemyAnimationManager : MonoBehaviour
    {
        // 5013 — visibility throttle (D1=A 0.1s)
        private const float VIS_THROTTLE_SEC = 0.1f;
        private static float _lastVisCheck;

        private static EnemyAnimationManager _instance;
        private static readonly List<EnemyController> _active = new List<EnemyController>(1024);

        public static void Register(EnemyController e)
        {
            if (e == null) return;
            EnsureInstance();
            _active.Add(e);
        }

        public static void Unregister(EnemyController e)
        {
            if (e == null) return;
            // List.Remove O(n) but 적 사망 빈도 ≪ Update — 허용.
            _active.Remove(e);
        }

        private static void EnsureInstance()
        {
            if (_instance != null) return;
            var go = new GameObject("[EnemyAnimationManager]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<EnemyAnimationManager>();
        }

        private void Update()
        {
            float t = Time.time;
            // 5013 — 0.1s 간격 visibility 체크 (PIVOT v4 §7 cap 300 60fps 마진)
            bool checkVis = (Time.unscaledTime - _lastVisCheck) >= VIS_THROTTLE_SEC;
            if (checkVis) _lastVisCheck = Time.unscaledTime;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var e = _active[i];
                if (e == null) { _active.RemoveAt(i); continue; }
                if (checkVis)
                {
                    bool vis = EnemyVisibilityCuller.IsVisible(e.transform.position);
                    e.SetVisible(vis);
                }
                if (e.IsVisible) e.UpdateAnim(t);  // off-screen 적 anim skip
            }
        }
    }
}
