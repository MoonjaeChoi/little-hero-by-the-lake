using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D1 — HUD: 체력 / 웨이브 / 별가루 ★ + XP 바 + 레벨업 0.8s 연출 (ui_v1 §4).
    /// 레벨업 시 0.8s 동안 XP 바 색상 강조 + 레벨 텍스트 punch.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Text _waveText;
        [SerializeField] private Text _starDustText;
        [SerializeField] private Text _healthText;

        // ✦ D1 — XP 바 + 레벨 (선택. 미설정 시 기존 동작 유지)
        [Header("D1 — 레벨/XP")]
        [SerializeField] private Slider _xpBar;
        [SerializeField] private Text _levelText;
        [SerializeField] private Image _xpFill;
        [SerializeField] private Color _xpNormalColor = new Color(0.55f, 0.78f, 0.95f, 1f);
        [SerializeField] private Color _xpLevelUpColor = new Color(1f, 0.92f, 0.4f, 1f);
        [SerializeField] private float _levelUpHighlightSeconds = 0.8f;

        private PlayerStats _playerStats;
        private Coroutine _levelUpFx;

        private void OnEnable()
        {
            GameEvents.OnPlayerHealthChanged += UpdateHealth;
            GameEvents.OnShopClosed += RefreshWave;
            GameEvents.OnStarDustChanged += UpdateStarDust;
            GameEvents.OnLevelUp += HandleLevelUp;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHealthChanged -= UpdateHealth;
            GameEvents.OnShopClosed -= RefreshWave;
            GameEvents.OnStarDustChanged -= UpdateStarDust;
            GameEvents.OnLevelUp -= HandleLevelUp;
        }

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                _playerStats = player.GetComponent<PlayerStats>();

            if (_xpFill != null) _xpFill.color = _xpNormalColor;

            RefreshWave();
            UpdateStarDust(0);
            RefreshXpBar();
        }

        private void Update()
        {
            // PlayerProgress 의 XP 는 적 처치 시마다 변하므로 매 프레임 갱신 (저비용).
            RefreshXpBar();
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
            _starDustText.text = $"별가루 ★ {amount}";
        }

        private void RefreshXpBar()
        {
            var p = PlayerProgress.Instance;
            if (p == null) return;
            if (_levelText != null) _levelText.text = $"Lv {p.Level}";
            if (_xpBar != null)
            {
                float ratio = p.NextLevelXp > 0 ? (float)p.Xp / p.NextLevelXp : 0f;
                _xpBar.value = Mathf.Clamp01(ratio);
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            if (_levelText != null) _levelText.text = $"Lv {newLevel}";
            if (_levelUpFx != null) StopCoroutine(_levelUpFx);
            if (gameObject.activeInHierarchy && _xpFill != null)
                _levelUpFx = StartCoroutine(LevelUpFx());
        }

        private IEnumerator LevelUpFx()
        {
            if (_xpFill == null) yield break;
            _xpFill.color = _xpLevelUpColor;
            yield return new WaitForSeconds(_levelUpHighlightSeconds);
            _xpFill.color = _xpNormalColor;
            _levelUpFx = null;
        }
    }
}
