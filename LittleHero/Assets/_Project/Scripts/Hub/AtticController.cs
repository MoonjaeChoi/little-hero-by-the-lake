using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LittleHero
{
    public class AtticController : MonoBehaviour
    {
        [Serializable]
        public class ChapterButton
        {
            public ChapterData chapter;
            public Button button;
        }

        [Serializable]
        public class CharacterButton
        {
            public CharacterData character;
            public Button button;
            public Image highlight;   // 선택된 캐릭터 강조용 (선택사항)
        }

        [SerializeField] private Text _selectedCharacterText;
        [SerializeField] private string _chapterSceneName = "Chapter";
        [SerializeField] private ChapterButton[] _chapterButtons;
        [SerializeField] private CharacterButton[] _characterButtons;
        [SerializeField] private CharacterData _defaultCharacter;

        // 5005 Sprint 1 — 4 화폐 표시는 AtticCurrencyView 로 추출
        // (_starFragmentsText / _magicCrystalText / _starlightText / _starforceShardsHud 모두 컴포넌트 이동)

        [Header("D2 — Endgame 메뉴 (선택, CH35 클리어 후 활성)")]
        [SerializeField] private GameObject _endgameMenuRoot;
        [SerializeField] private Text _dailyBossCooldownText;
        [SerializeField] private Text _weeklyBossCooldownText;
        [SerializeField] private Text _infiniteDepthText;

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
            if (MetaProgression.Instance != null)
            {
                MetaProgression.Instance.OnCharacterChanged     += UpdateSelectedCharacter;
            }
            // 5005 Sprint 1 — 4 화폐 이벤트 구독은 AtticCurrencyView 가 담당

            if (_gachaButton != null) _gachaButton.onClick.AddListener(OpenGachaPanel);
            if (_starforceButton != null) _starforceButton.onClick.AddListener(OpenStarforcePanel);
            if (_limitedShopButton != null) _limitedShopButton.onClick.AddListener(OpenLimitedShopPanel);
            GameEvents.OnLimitedOfferActivated += HandleLimitedOfferChanged;
            GameEvents.OnLimitedOfferPurchased += HandleLimitedOfferChanged;
            GameEvents.OnLimitedOfferExpired   += HandleLimitedOfferChanged;

            if (_chapterButtons != null)
            {
                for (int i = 0; i < _chapterButtons.Length; i++)
                {
                    var entry = _chapterButtons[i];
                    if (entry?.button == null) continue;
                    var captured = entry.chapter;
                    entry.button.onClick.AddListener(() => HandleStart(captured));
                }
            }

            if (_characterButtons != null)
            {
                for (int i = 0; i < _characterButtons.Length; i++)
                {
                    var entry = _characterButtons[i];
                    if (entry?.button == null) continue;
                    var captured = entry.character;
                    entry.button.onClick.AddListener(() => HandleSelectCharacter(captured));
                }
            }
        }

        private void OnDisable()
        {
            if (MetaProgression.Instance != null)
            {
                MetaProgression.Instance.OnCharacterChanged     -= UpdateSelectedCharacter;
            }
            // 5005 Sprint 1 — 4 화폐 이벤트 해제는 AtticCurrencyView 가 담당

            if (_gachaButton != null) _gachaButton.onClick.RemoveAllListeners();
            if (_starforceButton != null) _starforceButton.onClick.RemoveAllListeners();
            if (_limitedShopButton != null) _limitedShopButton.onClick.RemoveAllListeners();
            GameEvents.OnLimitedOfferActivated -= HandleLimitedOfferChanged;
            GameEvents.OnLimitedOfferPurchased -= HandleLimitedOfferChanged;
            GameEvents.OnLimitedOfferExpired   -= HandleLimitedOfferChanged;

            if (_chapterButtons != null)
                foreach (var entry in _chapterButtons)
                    if (entry?.button != null) entry.button.onClick.RemoveAllListeners();
            if (_characterButtons != null)
                foreach (var entry in _characterButtons)
                    if (entry?.button != null) entry.button.onClick.RemoveAllListeners();
        }

        private void Start()
        {
            Time.timeScale = 1f;
            // 5005 Sprint 1 — 4 화폐 초기 표시는 AtticCurrencyView.Start 가 담당
            RefreshLimitedShopBadge();
            RefreshEndgameMenu();
            HandleReturnFromBossEncounter();
            HandlePendingChapterCleared();   // 5002 D2 — 챕터 클리어 후 1회 toast

            // 다락방 BGM (Joe Hisaishi 풍 nostalgic)
            AudioManager.Instance?.PlayBgm("attic_nostalgic");

            // 저장된 캐릭터 ID 복원
            if (MetaProgression.Instance != null)
            {
                var saved = MetaProgression.Instance.SelectedCharacter;
                if (saved == null)
                {
                    string id = MetaProgression.Instance.Data?.selectedCharacterId;
                    saved = ResolveCharacterById(id);
                    // stats_v2 §6.2 — 첫 시작 강제 선택 (자동 default 폐기)
                    if (saved == null)
                    {
                        ShowFirstStartCharacterPrompt();
                        return;
                    }
                    MetaProgression.Instance.SelectCharacter(saved);
                }
                UpdateSelectedCharacter(saved);
            }
        }

        /// <summary>
        /// stats_v2 §6.2 — 첫 시작 시 강제 캐릭터 선택.
        /// 자동 default_boy 폐기 — 다락방 _characterButtons 클릭 대기.
        /// </summary>
        private void ShowFirstStartCharacterPrompt()
        {
            if (_selectedCharacterText != null)
                _selectedCharacterText.text = "영웅을 선택하세요!";
            // 다락방 _characterButtons 가 화면에 표시됨 — 사용자가 클릭하면 HandleSelectCharacter 호출
            Debug.Log("[AtticController] 첫 시작 — 캐릭터 선택 강제 (자동 선택 폐기)");
        }

        private CharacterData ResolveCharacterById(string id)
        {
            if (string.IsNullOrEmpty(id) || _characterButtons == null) return null;
            foreach (var entry in _characterButtons)
                if (entry?.character != null && entry.character.id == id) return entry.character;
            return null;
        }

        private void HandleSelectCharacter(CharacterData character)
        {
            if (MetaProgression.Instance == null || character == null) return;
            MetaProgression.Instance.SelectCharacter(character);
        }

        private void UpdateSelectedCharacter(CharacterData character)
        {
            if (_selectedCharacterText != null)
                _selectedCharacterText.text = character != null ? $"선택: {character.displayName}" : "선택: -";

            if (_characterButtons != null)
                foreach (var entry in _characterButtons)
                    if (entry?.highlight != null)
                        entry.highlight.enabled = (entry.character == character);
        }

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

        private void RefreshEndgameMenu()
        {
            // CH 35 첫 클리어 비트(0~34) 가 모두 set 일 때 활성. MVP 단계에선 단순 표시만.
            if (_endgameMenuRoot != null)
            {
                bool unlocked = AllChaptersFirstCleared();
                _endgameMenuRoot.SetActive(unlocked);
            }
            if (_dailyBossCooldownText != null)
            {
                bool avail = EndgameResetService.IsDailyAvailable();
                _dailyBossCooldownText.text = avail
                    ? "일일 보스 — 도전 가능"
                    : $"일일 보스 — {FormatHm(EndgameResetService.GetSecondsUntilNextDailyReset())}";
            }
            if (_weeklyBossCooldownText != null)
            {
                bool avail = EndgameResetService.IsWeeklyAvailable();
                _weeklyBossCooldownText.text = avail
                    ? "주간 보스 — 도전 가능"
                    : $"주간 보스 — {FormatDh(EndgameResetService.GetSecondsUntilNextWeeklyReset())}";
            }
            if (_infiniteDepthText != null)
                _infiniteDepthText.text = $"무한 다락방 — 최고 깊이 {EndgameResetService.GetInfiniteMaxDepth()}층";
        }

        private static bool AllChaptersFirstCleared()
        {
            // 출시 전 fix #2 — long 승격 후 진짜 35 챕터 첫 클리어 검사 (CH 1~35 비트 0~34 모두 set)
            // 또는 추억 4종 모두 발견 (story-completion 우회) 둘 중 하나 만족 시 endgame 활성.
            long mask = MetaProgression.Instance?.Data?.firstClearedChapters ?? 0L;
            const long target35 = (1L << 35) - 1;  // 0x7FFFFFFFF
            bool allChapters = (mask & target35) == target35;
            bool allMemories = MetaProgression.Instance != null && MetaProgression.Instance.AllMemoriesDiscovered();
            return allChapters || allMemories;
        }

        private static string FormatHm(long seconds)
        {
            long h = seconds / 3600;
            long m = (seconds % 3600) / 60;
            return $"{h}시 {m}분";
        }

        private static string FormatDh(long seconds)
        {
            long d = seconds / 86400;
            long h = (seconds % 86400) / 3600;
            return $"{d}일 {h}시";
        }

        private void HandleStart(ChapterData chapter)
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.SelectedChapter = chapter;
            SceneTransitionService.Transition(_chapterSceneName);
        }

        // 5002 D2 — 챕터 클리어 후 다락방 1회 toast (BossEncounterService.CompleteChapterBoss 가 set)
        private void HandlePendingChapterCleared()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null || string.IsNullOrEmpty(data.pendingChapterClearedId)) return;

            var ch = ResolveChapterById(data.pendingChapterClearedId);
            if (ch != null)
            {
                // Attic 씬에 ChapterClearedPanel prefab 가 있다면 OnChapterCleared 구독으로 표시.
                // StarlightGrantService 이중 raise 안전 (chapterStarlightGranted bit 으로 1회 보상 보장).
                GameEvents.RaiseChapterCleared(ch);
                Debug.Log($"[Attic] 챕터 클리어 1회 toast: {ch.displayName} (5002 D2)");
            }
            data.pendingChapterClearedId = "";
            SaveSystem.Save(data);
        }

        private ChapterData ResolveChapterById(string id)
        {
            if (string.IsNullOrEmpty(id) || _chapterButtons == null) return null;
            foreach (var entry in _chapterButtons)
                if (entry?.chapter != null && entry.chapter.id == id) return entry.chapter;
            return null;
        }

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
