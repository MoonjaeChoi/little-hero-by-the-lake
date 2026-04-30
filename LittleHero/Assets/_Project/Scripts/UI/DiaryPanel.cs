using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public class DiaryPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _slotsParent;
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Text _countText;

        private readonly List<DiarySlot> _slots = new List<DiarySlot>();

        private void OnEnable()
        {
            if (_openButton  != null) _openButton.onClick.AddListener(Open);
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            GameEvents.OnMemoryDiscovered += HandleDiscovered;
        }

        private void OnDisable()
        {
            if (_openButton  != null) _openButton.onClick.RemoveListener(Open);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Close);
            GameEvents.OnMemoryDiscovered -= HandleDiscovered;
        }

        private void Start()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
            Rebuild();
        }

        private void HandleDiscovered(MemoryData _) => Rebuild();

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
            var pool = MetaProgression.Instance.MemoryPool;
            if (pool == null || pool.memories == null || _slotPrefab == null || _slotsParent == null) return;

            while (_slots.Count < pool.memories.Length)
            {
                GameObject go = Instantiate(_slotPrefab, _slotsParent);
                _slots.Add(go.GetComponent<DiarySlot>());
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                bool active = i < pool.memories.Length;
                _slots[i].gameObject.SetActive(active);
                if (!active) continue;
                MemoryData m = pool.memories[i];
                bool found = MetaProgression.Instance.IsMemoryDiscovered(m.id);
                _slots[i].Bind(m, found);
            }

            if (_countText != null)
                _countText.text = $"{MetaProgression.Instance.MemoryDiscoveredCount} / {MetaProgression.Instance.MemoryTotalCount} 발견";
        }
    }
}
