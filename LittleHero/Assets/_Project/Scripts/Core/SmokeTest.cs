using System.Collections;
using System.IO;
using UnityEngine;

namespace LittleHero
{
    // Play 모드 시작 시 자동으로 N초 시점에 스크린샷·상태 덤프.
    // 사용: Chapter 씬의 빈 GO에 부착, 검증 시 _enabledOnStart=true로 토글.
    public class SmokeTest : MonoBehaviour
    {
        [SerializeField] private bool _enabledOnStart = false;
        [SerializeField] private float[] _captureSeconds = { 1f, 5f, 15f, 30f };
        [SerializeField] private string _outputFolder = "SmokeTest";

        private void Start()
        {
            if (!_enabledOnStart) return;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            string runId = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string folder = Path.Combine(Application.persistentDataPath, _outputFolder, runId);
            Directory.CreateDirectory(folder);
            Debug.Log($"[SmokeTest] Run start. id={runId} folder={folder} captures={_captureSeconds.Length}");

            float lastT = 0f;
            for (int i = 0; i < _captureSeconds.Length; i++)
            {
                float wait = _captureSeconds[i] - lastT;
                if (wait > 0f) yield return new WaitForSeconds(wait);
                lastT = _captureSeconds[i];
                Snapshot(i, folder);
            }
            Debug.Log($"[SmokeTest] Run done. id={runId}");
        }

        private void Snapshot(int index, string folder)
        {
            int wave = GameManager.Instance != null ? GameManager.Instance.CurrentWave : -1;
            int dust = GameManager.Instance != null ? GameManager.Instance.StarDust : -1;
            string state = GameManager.Instance != null ? GameManager.Instance.CurrentState.ToString() : "?";

            GameObject player = GameObject.FindWithTag("Player");
            PlayerHealth ph = player != null ? player.GetComponent<PlayerHealth>() : null;
            float hp = ph != null ? ph.CurrentHealth : -1f;
            string playerPos = player != null ? player.transform.position.ToString("F2") : "<null>";
            bool isDead = ph != null && ph.IsDead;

            WaveManager wm = Object.FindFirstObjectByType<WaveManager>();
            int activeEnemies = wm != null ? wm.ActiveEnemyCount : -1;

            string camPos = Camera.main != null ? Camera.main.transform.position.ToString("F2") : "<null>";

            Debug.Log($"[SmokeTest] T={_captureSeconds[index]}s state={state} wave={wave} dust={dust} hp={hp} dead={isDead} player={playerPos} cam={camPos} enemies={activeEnemies}");

            string fname = Path.Combine(folder, $"snap_{index:D2}_t{_captureSeconds[index]:F0}s.png");
            ScreenCapture.CaptureScreenshot(fname);
        }
    }
}
