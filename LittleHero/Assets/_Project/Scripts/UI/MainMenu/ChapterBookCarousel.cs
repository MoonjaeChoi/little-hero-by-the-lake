using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero.MainMenu
{
    /// <summary>
    /// 5040 T04 — 동화책 메타포 좌우 carousel. 35 챕터 = 7 권 × 5 챕터 + Hardcore 3 권 (페이지 1~10).
    /// 좌/우 화살표로 페이지 전환. Bind 시 ChapterCardLeft/Right 갱신.
    /// </summary>
    public class ChapterBookCarousel : MonoBehaviour
    {
        [SerializeField] private ChapterData[] _chapters;       // 50 챕터 (chapterNumber 순)
        [SerializeField] private ChapterCardView _cardLeft;     // 짝수 챕터 (1, 3, 5, ...)
        [SerializeField] private ChapterCardView _cardRight;    // 홀수 챕터 (2, 4, ...)
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Text _pageIndicatorText;

        private int _currentPage = 0;  // 0~9 (페이지 = 2 챕터)
        private const int CHAPTERS_PER_PAGE = 2;
        private const int TOTAL_PAGES = 25;  // 50 챕터 / 2

        public event Action<ChapterData> OnChapterSelected;

        private void OnEnable()
        {
            if (_prevButton != null) _prevButton.onClick.AddListener(GoPrev);
            if (_nextButton != null) _nextButton.onClick.AddListener(GoNext);
            if (_cardLeft != null)  _cardLeft.OnSelected  += OnCardSelected;
            if (_cardRight != null) _cardRight.OnSelected += OnCardSelected;
            Refresh();
        }

        private void OnDisable()
        {
            if (_prevButton != null) _prevButton.onClick.RemoveAllListeners();
            if (_nextButton != null) _nextButton.onClick.RemoveAllListeners();
            if (_cardLeft != null)  _cardLeft.OnSelected  -= OnCardSelected;
            if (_cardRight != null) _cardRight.OnSelected -= OnCardSelected;
        }

        private void GoPrev()
        {
            _currentPage = Mathf.Max(0, _currentPage - 1);
            Refresh();
        }

        private void GoNext()
        {
            _currentPage = Mathf.Min(TOTAL_PAGES - 1, _currentPage + 1);
            Refresh();
        }

        public void Refresh()
        {
            int leftIdx  = _currentPage * CHAPTERS_PER_PAGE;
            int rightIdx = leftIdx + 1;
            BindCard(_cardLeft, leftIdx);
            BindCard(_cardRight, rightIdx);
            if (_pageIndicatorText != null)
                _pageIndicatorText.text = $"{_currentPage + 1} / {TOTAL_PAGES}";
        }

        private void BindCard(ChapterCardView card, int chapterIdx)
        {
            if (card == null) return;
            ChapterData chapter = _chapters != null && chapterIdx < _chapters.Length ? _chapters[chapterIdx] : null;
            if (chapter == null) { card.gameObject.SetActive(false); return; }
            card.gameObject.SetActive(true);
            int chapterNum = chapter.chapterNumber;
            long firstMask = MetaProgression.Instance?.Data?.firstClearedChapters ?? 0L;
            bool firstCleared = chapterNum >= 1 && chapterNum <= 50 && (firstMask & (1L << (chapterNum - 1))) != 0;
            bool unlocked = WondrousMapPanel.DevUnlockAllChapters || chapterNum == 1
                          || (chapterNum >= 2 && (firstMask & (1L << (chapterNum - 2))) != 0);
            int playerLevel = PlayerProgress.Instance?.Level ?? (MetaProgression.Instance?.Data?.level ?? 1);
            int recommended = RecommendedLevel(chapterNum);
            card.Bind(chapter, unlocked, firstCleared, playerLevel, recommended);
        }

        private static int RecommendedLevel(int chapterNum)
        {
            if (chapterNum <= 5) return 1;
            if (chapterNum <= 10) return 5;
            if (chapterNum <= 15) return 10;
            if (chapterNum <= 20) return 18;
            if (chapterNum <= 25) return 28;
            if (chapterNum <= 30) return 38;
            return 48;
        }

        private void OnCardSelected(ChapterData chapter)
        {
            if (MetaProgression.Instance != null)
                MetaProgressionChapter.SelectedChapter = chapter;
            OnChapterSelected?.Invoke(chapter);
        }
    }
}
