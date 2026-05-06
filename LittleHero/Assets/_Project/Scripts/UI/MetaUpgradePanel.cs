using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public class MetaUpgradePanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _rowsParent;
        [SerializeField] private GameObject _rowPrefab;
        [SerializeField] private Text _starFragmentsText;

        private readonly List<MetaUpgradeRow> _rows = new List<MetaUpgradeRow>();

        private void OnEnable()
        {
            if (_openButton  != null) _openButton.onClick.AddListener(Open);
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (MetaProgression.Instance != null)
            {
                MetaProgression.OnStarFragmentsChanged += HandleStarChanged;
                MetaProgression.OnUpgradeChanged       += HandleUpgradeChanged;
            }
        }

        private void OnDisable()
        {
            if (_openButton  != null) _openButton.onClick.RemoveListener(Open);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Close);
            if (MetaProgression.Instance != null)
            {
                MetaProgression.OnStarFragmentsChanged -= HandleStarChanged;
                MetaProgression.OnUpgradeChanged       -= HandleUpgradeChanged;
            }
        }

        private void Start()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void Open()
        {
            Rebuild();
            UpdateStars(MetaProgression.Instance != null ? MetaProgression.Instance.StarFragments : 0);
            if (_panelRoot != null) _panelRoot.SetActive(true);
        }

        private void Close()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void Rebuild()
        {
            if (MetaProgression.Instance == null) return;
            var pool = MetaProgression.Instance.UpgradePool;
            if (pool == null || pool.upgrades == null || _rowPrefab == null || _rowsParent == null) return;

            while (_rows.Count < pool.upgrades.Length)
            {
                GameObject go = Instantiate(_rowPrefab, _rowsParent);
                _rows.Add(go.GetComponent<MetaUpgradeRow>());
            }
            for (int i = 0; i < _rows.Count; i++)
            {
                bool active = i < pool.upgrades.Length;
                _rows[i].gameObject.SetActive(active);
                if (active) _rows[i].Bind(pool.upgrades[i]);
            }
        }

        private void HandleStarChanged(int amount)
        {
            UpdateStars(amount);
            for (int i = 0; i < _rows.Count; i++)
                _rows[i].Refresh();
        }

        private void HandleUpgradeChanged(MetaUpgradeData _)
        {
            for (int i = 0; i < _rows.Count; i++)
                _rows[i].Refresh();
        }

        private void UpdateStars(int amount)
        {
            if (_starFragmentsText != null)
                _starFragmentsText.text = $"무지개 구슬 ✦ {amount}";
        }
    }
}
