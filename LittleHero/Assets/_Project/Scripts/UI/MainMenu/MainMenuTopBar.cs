using UnityEngine;
using UnityEngine.UI;

namespace LittleHero.MainMenu
{
    /// <summary>
    /// 5040 T02 — 상단 화폐 4 칩 + 프로필 + 우편 (탕탕특공대 답습).
    /// AtticCurrencyView 답습 — MetaProgression event 구독.
    /// </summary>
    public class MainMenuTopBar : MonoBehaviour
    {
        [Header("프로필")]
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _nicknameText;

        [Header("화폐 4 칩")]
        [SerializeField] private Text _starDustText;     // ★ 별가루
        [SerializeField] private Text _rainbowOrbText;   // ✦ 무지개구슬
        [SerializeField] private Text _magicCrystalText; // ◇ 마법결정
        [SerializeField] private Text _starlightText;    // ⋆ 별빛

        [Header("알림")]
        [SerializeField] private Image _mailboxDot;
        [SerializeField] private Image _noticeDot;

        private void OnEnable()
        {
            GameEvents.OnStarDustChanged       += OnStarDustChanged;
            GameEvents.OnMagicCrystalChanged   += OnMagicCrystalChanged;
            GameEvents.OnStarlightChanged      += OnStarlightChanged;
            MetaProgression.OnStarFragmentsChanged += OnRainbowOrbChanged;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnStarDustChanged       -= OnStarDustChanged;
            GameEvents.OnMagicCrystalChanged   -= OnMagicCrystalChanged;
            GameEvents.OnStarlightChanged      -= OnStarlightChanged;
            MetaProgression.OnStarFragmentsChanged -= OnRainbowOrbChanged;
        }

        public void Refresh()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            if (_levelText != null) _levelText.text = $"Lv.{data.level}";
            if (_starDustText != null) _starDustText.text = ((int)(GameManager.Instance?.StarDust ?? 0)).ToString();
            if (_rainbowOrbText != null) _rainbowOrbText.text = data.starFragments.ToString();
            if (_magicCrystalText != null) _magicCrystalText.text = data.magicCrystals.ToString();
            if (_starlightText != null) _starlightText.text = data.starlight.ToString();
        }

        private void OnStarDustChanged(int v)        { if (_starDustText != null) _starDustText.text = v.ToString(); }
        private void OnMagicCrystalChanged(int v)    { if (_magicCrystalText != null) _magicCrystalText.text = v.ToString(); }
        private void OnStarlightChanged(int v)       { if (_starlightText != null) _starlightText.text = v.ToString(); }
        private void OnRainbowOrbChanged(int v)      { if (_rainbowOrbText != null) _rainbowOrbText.text = v.ToString(); }
    }
}
