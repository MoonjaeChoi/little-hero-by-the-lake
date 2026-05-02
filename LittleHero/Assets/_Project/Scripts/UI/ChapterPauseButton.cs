using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// Chapter 씬 우상단 "🏠 다락방" 버튼 — 클릭 시 현재 wave session 저장 + 다락방 복귀.
    /// 다락방 → 같은 챕터 재진입 시 이전 wave 부터 재개 (ChapterRunner.Start 가 SaveData 검사).
    /// 사용자 요청 2026-05-03.
    /// </summary>
    public class ChapterPauseButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private string _atticSceneName = "Attic";

        private void OnEnable()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_button != null) _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (_button != null) _button.onClick.RemoveAllListeners();
        }

        private void HandleClick()
        {
            // 현재 챕터 session 저장 후 다락방 복귀
            var runner = FindFirstObjectByType<ChapterRunner>(FindObjectsInactive.Include);
            if (runner != null) runner.SaveSession();
            Time.timeScale = 1f;
            SceneManager.LoadScene(_atticSceneName);
        }
    }
}
