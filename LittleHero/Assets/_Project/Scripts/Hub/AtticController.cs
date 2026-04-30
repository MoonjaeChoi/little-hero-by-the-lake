using System;
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

        [SerializeField] private Text _starFragmentsText;
        [SerializeField] private Text _selectedCharacterText;
        [SerializeField] private string _chapterSceneName = "Chapter";
        [SerializeField] private ChapterButton[] _chapterButtons;
        [SerializeField] private CharacterButton[] _characterButtons;
        [SerializeField] private CharacterData _defaultCharacter;

        // ✦ D2 — 4 화폐 + endgame placeholder (ui_v1 §6)
        [Header("D2 — 추가 화폐 (선택)")]
        [SerializeField] private Text _magicCrystalText;     // ◇
        [SerializeField] private Text _starlightText;        // ⋆

        [Header("D2 — Endgame 메뉴 (선택, CH35 클리어 후 활성)")]
        [SerializeField] private GameObject _endgameMenuRoot;
        [SerializeField] private Text _dailyBossCooldownText;
        [SerializeField] private Text _weeklyBossCooldownText;
        [SerializeField] private Text _infiniteDepthText;

        private void OnEnable()
        {
            if (MetaProgression.Instance != null)
            {
                MetaProgression.Instance.OnStarFragmentsChanged += UpdateStarFragments;
                MetaProgression.Instance.OnCharacterChanged     += UpdateSelectedCharacter;
            }
            GameEvents.OnMagicCrystalChanged += UpdateMagicCrystals;
            GameEvents.OnStarlightChanged    += UpdateStarlight;

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
                MetaProgression.Instance.OnStarFragmentsChanged -= UpdateStarFragments;
                MetaProgression.Instance.OnCharacterChanged     -= UpdateSelectedCharacter;
            }
            GameEvents.OnMagicCrystalChanged -= UpdateMagicCrystals;
            GameEvents.OnStarlightChanged    -= UpdateStarlight;

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
            int frags = MetaProgression.Instance != null ? MetaProgression.Instance.StarFragments : 0;
            UpdateStarFragments(frags);
            UpdateMagicCrystals(CurrencyService.MagicCrystals);
            UpdateStarlight(CurrencyService.Starlight);
            RefreshEndgameMenu();

            // 저장된 캐릭터 ID 복원, 없으면 기본 캐릭터 자동 선택
            if (MetaProgression.Instance != null)
            {
                var saved = MetaProgression.Instance.SelectedCharacter;
                if (saved == null)
                {
                    string id = MetaProgression.Instance.Data?.selectedCharacterId;
                    saved = ResolveCharacterById(id) ?? _defaultCharacter;
                    if (saved != null) MetaProgression.Instance.SelectCharacter(saved);
                }
                UpdateSelectedCharacter(saved);
            }
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

        private void UpdateStarFragments(int amount)
        {
            if (_starFragmentsText != null)
                _starFragmentsText.text = $"무지개 구슬 ✦ {amount}";
        }

        private void UpdateMagicCrystals(int amount)
        {
            if (_magicCrystalText != null)
                _magicCrystalText.text = $"마법 결정 ◇ {amount}";
        }

        private void UpdateStarlight(int amount)
        {
            if (_starlightText != null)
                _starlightText.text = $"별빛 ⋆ {amount}";
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
            int mask = MetaProgression.Instance?.Data?.firstClearedChapters ?? 0;
            // 35 챕터 — 비트 0~34 모두 set
            const int target = (1 << 0) - 1;  // placeholder; 실제는 (1 << 35) - 1 인데 int 32bit 한계 → long 필요
            // long 비교를 우회: 단순히 첫 35개 비트 set 여부를 별도 필드로 검사하는 게 정확.
            // 임시: chapterNumber 1~35 비트가 모두 set 인지 long 으로 비교 — 현재 SaveData.firstClearedChapters 가 int 라
            // 35bit 가 표현 불가. 후속 (F1~F3 콘텐츠 + B7 챕터 도입 시) long 으로 승격 예정.
            // MVP D2 placeholder: 추억 4종 모두 발견 시 endgame 활성으로 대용.
            _ = mask; _ = target;
            return MetaProgression.Instance != null && MetaProgression.Instance.AllMemoriesDiscovered();
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
            SceneManager.LoadScene(_chapterSceneName);
        }
    }
}
