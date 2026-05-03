using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LittleHero
{
    public class AtticController : MonoBehaviour
    {
        // 5005 Sprint 1 — 4 화폐 표시는 AtticCurrencyView 로 추출
        // 5005 Sprint 2 — 캐릭터 선택은 AtticCharacterSelector 로 추출
        // 5005 Sprint 3 — 챕터 진입 + 5002 D2 toast 는 AtticChapterEntry 로 추출 (ChapterButton 클래스 + _chapterButtons + _chapterSceneName + HandleStart + HandlePendingChapterCleared + ResolveChapterById)

        // 5005 Sprint 4 — Endgame 메뉴는 AtticEndgameMenuView 로 추출 (4 SerializeField + Refresh + AllChaptersFirstCleared + FormatHm/FormatDh)

        // 출시 전 fix #1 — 한 번에 하나의 패널만 활성화 보장
        // _managedPanels 가 비어있으면 Awake 에서 Canvas 자식 중 Panel/Modal/Overlay 접미어 자동 탐색.
        // _excludePanelNames 에 명시된 이름은 제외 (CharacterPanel 같은 다락방 본체 UI).
        [Header("패널 토글 관리 (한 번에 하나만 활성, 출시 전 fix #1)")]
        [SerializeField] private GameObject[] _managedPanels;
        [SerializeField] private string[] _excludePanelNames = { "CharacterPanel" };

        // ✦ PIVOT D-가챠 — 진입 버튼 (gacha_v1.md §5)
        [Header("PIVOT D-가챠 — 진입 버튼")]
        [SerializeField] private Button _gachaButton;
        [SerializeField] private GachaPanel _gachaPanel;
        [SerializeField] private Button _starforceButton;
        [SerializeField] private SlotStarforcePanel _starforcePanel;
        // _starforceShardsHud → AtticCurrencyView 로 이동

        // ✦ PIVOT 한정 상점 (limited_shop_v1.md §7.3)
        [Header("PIVOT 한정 상점")]
        [SerializeField] private Button _limitedShopButton;
        [SerializeField] private LimitedShopPanel _limitedShopPanel;
        [SerializeField] private Text _limitedShopBadgeText;  // 활성 핫딜 N개 뱃지

        private GameObject _lastActivePanel;

        private void Awake()
        {
            if (_managedPanels == null || _managedPanels.Length == 0) AutoDiscoverPanels();
        }

        private void AutoDiscoverPanels()
        {
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
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
            // 출시 전 fix #1 — 다른 패널이 활성화되면 이전 패널 자동 비활성화 (한 번에 하나만)
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

        private void OnEnable()
        {
            // 5005 Sprint 1 — 4 화폐 이벤트 구독은 AtticCurrencyView 가 담당
            // 5005 Sprint 2 — OnCharacterChanged 구독은 AtticCharacterSelector 가 담당

            if (_gachaButton != null) _gachaButton.onClick.AddListener(OpenGachaPanel);
            if (_starforceButton != null) _starforceButton.onClick.AddListener(OpenStarforcePanel);
            if (_limitedShopButton != null) _limitedShopButton.onClick.AddListener(OpenLimitedShopPanel);
            GameEvents.OnLimitedOfferActivated += HandleLimitedOfferChanged;
            GameEvents.OnLimitedOfferPurchased += HandleLimitedOfferChanged;
            GameEvents.OnLimitedOfferExpired   += HandleLimitedOfferChanged;

        }

        private void OnDisable()
        {
            // 5005 Sprint 1 — 4 화폐 이벤트 해제는 AtticCurrencyView 가 담당
            // 5005 Sprint 2 — OnCharacterChanged 해제는 AtticCharacterSelector 가 담당

            if (_gachaButton != null) _gachaButton.onClick.RemoveAllListeners();
            if (_starforceButton != null) _starforceButton.onClick.RemoveAllListeners();
            if (_limitedShopButton != null) _limitedShopButton.onClick.RemoveAllListeners();
            GameEvents.OnLimitedOfferActivated -= HandleLimitedOfferChanged;
            GameEvents.OnLimitedOfferPurchased -= HandleLimitedOfferChanged;
            GameEvents.OnLimitedOfferExpired   -= HandleLimitedOfferChanged;

        }

        private void Start()
        {
            Time.timeScale = 1f;
            // 5005 Sprint 1 — 4 화폐 초기 표시는 AtticCurrencyView.Start 가 담당
            RefreshLimitedShopBadge();
            // 5005 Sprint 4 — RefreshEndgameMenu 는 AtticEndgameMenuView.Start 가 담당
            HandleReturnFromBossEncounter();
            // 5005 Sprint 3 — HandlePendingChapterCleared 는 AtticChapterEntry.Start 가 담당

            // 다락방 BGM (Joe Hisaishi 풍 nostalgic)
            AudioManager.Instance?.PlayBgm("attic_nostalgic");

            // 5005 Sprint 2 — 캐릭터 복원/선택은 AtticCharacterSelector.Start 가 담당
        }

        // 5005 Sprint 2 — ShowFirstStartCharacterPrompt / ResolveCharacterById (캐릭터 한정) / HandleSelectCharacter / UpdateSelectedCharacter 모두 AtticCharacterSelector 로 이동

        // 5005 Sprint 1 — UpdateStarFragments / UpdateMagicCrystals / UpdateStarlight / UpdateStarforceShards 모두 AtticCurrencyView 로 이동

        private void OpenGachaPanel()
        {
            if (_gachaPanel != null) _gachaPanel.gameObject.SetActive(true);
        }

        private void OpenStarforcePanel()
        {
            if (_starforcePanel != null) _starforcePanel.gameObject.SetActive(true);
        }

        private void OpenLimitedShopPanel()
        {
            if (_limitedShopPanel != null) _limitedShopPanel.gameObject.SetActive(true);
        }

        private void HandleLimitedOfferChanged(LimitedShopOffer _)
        {
            RefreshLimitedShopBadge();
        }

        private void RefreshLimitedShopBadge()
        {
            if (_limitedShopBadgeText == null) return;
            int n = LimitedShopService.IsInitialized ? LimitedShopService.GetActiveOffers().Count : 0;
            _limitedShopBadgeText.text = n > 0 ? n.ToString() : "";
            _limitedShopBadgeText.gameObject.SetActive(n > 0);
        }

        // 5005 Sprint 4 — RefreshEndgameMenu / AllChaptersFirstCleared / FormatHm / FormatDh 모두 AtticEndgameMenuView 로 이동

        // 5005 Sprint 3 — HandleStart / HandlePendingChapterCleared / ResolveChapterById 모두 AtticChapterEntry 로 이동

        // F5 — 보스 씬에서 신화 발현된 경우 다락방 복귀 시 1회 무지개 Overlay
        private void HandleReturnFromBossEncounter()
        {
            var def = BossEncounterContext.LastMythicDefinition;
            if (def == null) return;

            var overlay = FindFirstObjectByType<MythicEffectOverlay>(FindObjectsInactive.Include);
            if (overlay != null)
            {
                overlay.Play(def);
                Debug.Log($"[Attic] 신화 발현 통지: {def.displayName}");
            }
            else
            {
                Debug.LogWarning("[Attic] MythicEffectOverlay 미존재 — 신화 발현 통지 skip");
            }

            // 1회 표시 후 Clear (재진입 시 중복 표시 방지)
            BossEncounterContext.LastMythicDefinition = null;
            BossEncounterContext.LastMythicGranted = null;
        }
    }
}
