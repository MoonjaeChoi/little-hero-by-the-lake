using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D1 — HUD: 체력 / 웨이브 / 별가루 ★ + XP 바 + 레벨업 0.8s 연출 (ui_v1 §4).
    /// 레벨업 시 0.8s 동안 XP 바 색상 강조 + 레벨 텍스트 punch.
    /// 5016.1 — string interpolation cache (매 frame alloc 0).
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
        // 5016 T05 — Coroutine → MotionHandle (state machine alloc 0)
        private MotionHandle _levelUpFx;

        // 5016.1 — 매 frame string interpolation 회피 (변동 시만 갱신)
        private int _cachedLevel = -1;
        private int _cachedHealth = -1;
        private int _cachedHealthMax = -1;
        private int _cachedWave = -1;
        private int _cachedStarDust = -1;

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
            // 5016.1 — XP bar 의 value 만 매 frame (string alloc X). Level 변동 시만 string 갱신.
            RefreshXpBar();
        }

        private void UpdateHealth(int currentHealth)
        {
            float maxHealth = _playerStats != null
                ? _playerStats.MaxHealth
                : GameManager.Instance.Balance.baseMaxHealth;

            if (_healthBar != null)
                _healthBar.value = currentHealth / maxHealth;

            // 5016.1 — string alloc 회피 (현재/최대 변동 시만)
            if (_healthText != null)
            {
                int curRounded = currentHealth;
                int maxRounded = Mathf.RoundToInt(maxHealth);
                if (curRounded != _cachedHealth || maxRounded != _cachedHealthMax)
                {
                    _cachedHealth = curRounded;
                    _cachedHealthMax = maxRounded;
                    _healthText.text = $"{curRounded} / {maxRounded}";
                }
            }
        }

        private void RefreshWave()
        {
            if (_waveText == null) return;
            int current = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 0;
            // 5016.1 — string alloc 회피 (wave 변동 시만)
            if (current != _cachedWave)
            {
                _cachedWave = current;
                _waveText.text = $"웨이브 {current}";
            }
        }

        private void UpdateStarDust(int amount)
        {
            if (_starDustText == null) return;
            // 5016.1 — string alloc 회피 (별가루 변동 시만)
            if (amount != _cachedStarDust)
            {
                _cachedStarDust = amount;
                _starDustText.text = $"별가루 ★ {amount}";
            }
        }

        private void RefreshXpBar()
        {
            var p = PlayerProgress.Instance;
            if (p == null) return;
            // 5016.1 — Level string 은 변동 시만 갱신 (매 frame alloc 회피)
            if (_levelText != null && p.Level != _cachedLevel)
            {
                _cachedLevel = p.Level;
                _levelText.text = $"Lv {p.Level}";
            }
            // XP bar value 는 매 frame 갱신 (struct value, alloc 0)
            if (_xpBar != null)
            {
                float ratio = p.NextLevelXp > 0 ? (float)p.Xp / p.NextLevelXp : 0f;
                _xpBar.value = Mathf.Clamp01(ratio);
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            // 5016.1 — Level 캐시 무효화 (string alloc 1회 — 레벨업 시점)
            if (_levelText != null && newLevel != _cachedLevel)
            {
                _cachedLevel = newLevel;
                _levelText.text = $"Lv {newLevel}";
            }
            if (_levelUpFx.IsActive()) _levelUpFx.Cancel();
            if (gameObject.activeInHierarchy && _xpFill != null)
            {
                _xpFill.color = _xpLevelUpColor;
                // LMotion 으로 highlight 유지 후 normal 색 복귀
                _levelUpFx = LMotion.Create(0f, 1f, _levelUpHighlightSeconds)
                    .WithOnComplete(() => { if (_xpFill != null) _xpFill.color = _xpNormalColor; })
                    .RunWithoutBinding();
            }
        }
    }
}
