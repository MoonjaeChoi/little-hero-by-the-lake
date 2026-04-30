using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public class BestiaryPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _slotsParent;
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Text _countText;

        private readonly List<BestiarySlot> _slots = new List<BestiarySlot>();

        private void OnEnable()
        {
            if (_openButton  != null) _openButton.onClick.AddListener(Open);
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.OnEnemyDefeated += HandleDefeated;
        }

        private void OnDisable()
        {
            if (_openButton  != null) _openButton.onClick.RemoveListener(Open);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Close);
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.OnEnemyDefeated -= HandleDefeated;
        }

        private void Start()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void HandleDefeated(EnemyData _) => Rebuild();

        private void Open()
        {
            Rebuild();
            if (_panelRoot != null) _panelRoot.SetActive(true);
        }

        private void Close()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void Rebuild()
        {
            if (MetaProgression.Instance == null) return;
            var pool = MetaProgression.Instance.EnemyPool;
            if (pool == null || pool.enemies == null || _slotPrefab == null || _slotsParent == null) return;

            while (_slots.Count < pool.enemies.Length)
            {
                GameObject go = Instantiate(_slotPrefab, _slotsParent);
                _slots.Add(go.GetComponent<BestiarySlot>());
            }
            for (int i = 0; i < _slots.Count; i++)
            {
                bool active = i < pool.enemies.Length;
                _slots[i].gameObject.SetActive(active);
                if (!active) continue;
                EnemyData e = pool.enemies[i];
                bool found = MetaProgression.Instance.IsEnemyDefeated(e.id);
                _slots[i].Bind(e, found);
            }

            if (_countText != null)
                _countText.text = $"{MetaProgression.Instance.EnemyDefeatedCount} / {MetaProgression.Instance.EnemyTotalCount} 발견";
        }
    }
}
