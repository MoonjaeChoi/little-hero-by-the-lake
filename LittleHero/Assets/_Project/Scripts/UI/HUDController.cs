using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Text _waveText;
        [SerializeField] private Text _starDustText;
        [SerializeField] private Text _healthText;

        private PlayerStats _playerStats;

        private void OnEnable()
        {
            GameEvents.OnPlayerHealthChanged += UpdateHealth;
            GameEvents.OnShopClosed += RefreshWave;
            GameEvents.OnStarDustChanged += UpdateStarDust;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHealthChanged -= UpdateHealth;
            GameEvents.OnShopClosed -= RefreshWave;
            GameEvents.OnStarDustChanged -= UpdateStarDust;
        }

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                _playerStats = player.GetComponent<PlayerStats>();

            RefreshWave();
            UpdateStarDust(0);
        }

        private void UpdateHealth(int currentHealth)
        {
            float maxHealth = _playerStats != null
                ? _playerStats.MaxHealth
                : GameManager.Instance.Balance.baseMaxHealth;

            if (_healthBar != null)
                _healthBar.value = currentHealth / maxHealth;

            if (_healthText != null)
                _healthText.text = $"{currentHealth} / {Mathf.RoundToInt(maxHealth)}";
        }

        private void RefreshWave()
        {
            if (_waveText == null) return;
            int current = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 0;
            _waveText.text = $"웨이브 {current}";
        }

        private void UpdateStarDust(int amount)
        {
            if (_starDustText == null) return;
            _starDustText.text = $"★ {amount}";
        }
    }
}
