using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5005 Sprint 5 — 다락방 패널 토글 (한 번에 하나만 활성, 출시 전 fix #1).
    /// AtticController 에서 추출 (§15.1 SRP). _managedPanels 비어있으면 Awake 에서 Canvas 자식 자동 탐색.
    /// </summary>
    public class AtticPanelToggleManager : MonoBehaviour
    {
        [Header("패널 토글 관리 (한 번에 하나만 활성, 출시 전 fix #1)")]
        [SerializeField] private GameObject[] _managedPanels;
        [SerializeField] private string[] _excludePanelNames = { "CharacterPanel" };

        private GameObject _lastActivePanel;

        private void Awake()
        {
            if (_managedPanels == null || _managedPanels.Length == 0) AutoDiscoverPanels();
        }

        private void AutoDiscoverPanels()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null) { _managedPanels = new GameObject[0]; return; }

            var list = new List<GameObject>();
            foreach (Transform child in canvas.transform)
            {
                string n = child.name;
                if (_excludePanelNames != null && System.Array.IndexOf(_excludePanelNames, n) >= 0) continue;
                if (n.EndsWith("Panel") || n.EndsWith("Modal") || n.EndsWith("Overlay"))
                    list.Add(child.gameObject);
            }
            _managedPanels = list.ToArray();
        }

        private void Update()
        {
            if (_managedPanels == null || _managedPanels.Length == 0) return;

            GameObject newlyActive = null;
            for (int i = 0; i < _managedPanels.Length; i++)
            {
                var p = _managedPanels[i];
                if (p != null && p.activeSelf && p != _lastActivePanel)
                {
                    newlyActive = p;
                    break;
                }
            }

            if (newlyActive != null)
            {
                for (int i = 0; i < _managedPanels.Length; i++)
                {
                    var p = _managedPanels[i];
                    if (p != null && p != newlyActive && p.activeSelf) p.SetActive(false);
                }
                _lastActivePanel = newlyActive;
            }
            else
            {
                bool any = false;
                for (int i = 0; i < _managedPanels.Length; i++)
                    if (_managedPanels[i] != null && _managedPanels[i].activeSelf) { any = true; break; }
                if (!any) _lastActivePanel = null;
            }
        }
    }
}
