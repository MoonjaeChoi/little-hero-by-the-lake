using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public class AtticController : MonoBehaviour
    {
        // 5005 Sprint 1 — 4 화폐 표시는 AtticCurrencyView 로 추출
        // 5005 Sprint 2 — 캐릭터 선택은 AtticCharacterSelector 로 추출
        // 5005 Sprint 3 — 챕터 진입 + 5002 D2 toast 는 AtticChapterEntry 로 추출 (ChapterButton 클래스 + _chapterButtons + _chapterSceneName + HandleStart + HandlePendingChapterCleared + ResolveChapterById)

        // 5005 Sprint 4 — Endgame 메뉴는 AtticEndgameMenuView 로 추출 (4 SerializeField + Refresh + AllChaptersFirstCleared + FormatHm/FormatDh)
        // 5005 Sprint 5 — 패널 토글 관리는 AtticPanelToggleManager 로 추출 (_managedPanels + _excludePanelNames + Update + AutoDiscoverPanels)
        // 5005 Sprint 6 — 보스 씬 복귀 신화 발현 토스트는 AtticReturnHandler 로 추출 (HandleReturnFromBossEncounter)

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
            // 5005 Sprint 6 — HandleReturnFromBossEncounter 는 AtticReturnHandler.Start 가 담당
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

        // 5005 Sprint 6 — HandleReturnFromBossEncounter 는 AtticReturnHandler 로 이동
    }
}
