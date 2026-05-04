using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5005 Sprint 1 — 다락방 4 화폐 표시 (StarFragments / MagicCrystal / Starlight / StarforceShards).
    /// AtticController 에서 추출 (§15.1 SRP). MetaProgression / GameEvents 정적 이벤트 구독.
    /// </summary>
    public class AtticCurrencyView : MonoBehaviour
    {
        [SerializeField] private Text _starFragmentsText;
        [SerializeField] private Text _magicCrystalText;
        [SerializeField] private Text _starlightText;
        [SerializeField] private Text _starforceShardsHud;

        private void OnEnable()
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.OnStarFragmentsChanged += UpdateStarFragments;
            GameEvents.OnMagicCrystalChanged += UpdateMagicCrystals;
            GameEvents.OnStarlightChanged += UpdateStarlight;
            GameEvents.OnStarforceShardsChanged += UpdateStarforceShards;
        }

        private void OnDisable()
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.OnStarFragmentsChanged -= UpdateStarFragments;
            GameEvents.OnMagicCrystalChanged -= UpdateMagicCrystals;
            GameEvents.OnStarlightChanged -= UpdateStarlight;
            GameEvents.OnStarforceShardsChanged -= UpdateStarforceShards;
        }

        private void Start()
        {
            UpdateStarFragments(MetaProgression.Instance != null ? MetaProgression.Instance.StarFragments : 0);
            UpdateMagicCrystals(CurrencyService.MagicCrystals);
            UpdateStarlight(CurrencyService.Starlight);
            UpdateStarforceShards(StarforceService.Shards);
        }

        private void UpdateStarFragments(int n) { if (_starFragmentsText != null) _starFragmentsText.text = $"무지개 구슬 ✦ {n}"; }
        private void UpdateMagicCrystals(int n) { if (_magicCrystalText != null) _magicCrystalText.text = $"마법 결정 ◇ {n}"; }
        private void UpdateStarlight(int n) { if (_starlightText != null) _starlightText.text = $"별빛 ⋆ {n}"; }
        private void UpdateStarforceShards(int n) { if (_starforceShardsHud != null) _starforceShardsHud.text = $"마법의돌 조각 ◈ {n}"; }
    }
}
