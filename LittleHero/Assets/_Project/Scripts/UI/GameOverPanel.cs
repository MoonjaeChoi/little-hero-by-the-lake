using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LittleHero
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _restartButton;
        [SerializeField] private string _hubSceneName = "Attic";

        private void OnEnable()
        {
            GameEvents.OnPlayerDied += HandlePlayerDied;
            if (_restartButton != null) _restartButton.onClick.AddListener(HandleRestart);
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied -= HandlePlayerDied;
            if (_restartButton != null) _restartButton.onClick.RemoveListener(HandleRestart);
        }

        private void Start()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void HandlePlayerDied()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            AudioManager.Instance?.PlayBgm("game_over_gentle", crossFadeSec: 0.8f);
        }

        private void HandleRestart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_hubSceneName);
        }
    }
}
