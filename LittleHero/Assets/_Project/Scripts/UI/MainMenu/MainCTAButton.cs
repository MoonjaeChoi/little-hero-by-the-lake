using UnityEngine;
using UnityEngine.UI;

namespace LittleHero.MainMenu
{
    /// <summary>
    /// 5040 T03 — 다음 챕터 자동 추천 + 큰 출격 button (탕탕특공대 답습).
    /// firstClearedChapters 기반 다음 챕터 (마지막 클리어 + 1).
    /// </summary>
    public class MainCTAButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _label;
        [SerializeField] private Text _recommendText;
        [SerializeField] private string _chapterSceneName = "Chapter";
        [Tooltip("WondrousMapPanel 답습 — 35 챕터 (chapterNumber 순)")]
        [SerializeField] private ChapterData[] _chapters;

        private ChapterData _nextChapter;

        private void OnEnable()
        {
            if (_button != null) _button.onClick.AddListener(OnClick);
            Refresh();
        }

        private void OnDisable()
        {
            if (_button != null) _button.onClick.RemoveAllListeners();
        }

        public void Refresh()
        {
            _nextChapter = ResolveNextChapter();
            if (_nextChapter == null) return;
            if (_label != null) _label.text = $"제 {_nextChapter.chapterNumber}장 출격";
            if (_recommendText != null)
            {
                int recommended = RecommendedLevel(_nextChapter.chapterNumber);
                int playerLevel = PlayerProgress.Instance?.Level ?? (MetaProgression.Instance?.Data?.level ?? 1);
                _recommendText.text = playerLevel >= recommended ? "" : $"권장 Lv.{recommended}";
            }
        }

        private ChapterData ResolveNextChapter()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return LoadChapterByNumber(1);
            int next = 1;
            for (int n = 1; n <= 50; n++)
                if ((data.firstClearedChapters & (1L << (n - 1))) != 0) next = n + 1;
            if (next > 50) next = 50;
            return LoadChapterByNumber(next);
        }

        private ChapterData LoadChapterByNumber(int n)
        {
            if (_chapters == null) return null;
            for (int i = 0; i < _chapters.Length; i++)
                if (_chapters[i] != null && _chapters[i].chapterNumber == n) return _chapters[i];
            // fallback — chapterNumber 매칭 실패 시 첫 챕터
            return _chapters.Length > 0 ? _chapters[0] : null;
        }

        // 5034 답습 (BalanceConfig.recommendedLevelByChapter)
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

        private void OnClick()
        {
            if (_nextChapter == null) return;
            if (MetaProgression.Instance != null)
                MetaProgressionChapter.SelectedChapter = _nextChapter;
            SceneTransitionService.Transition(_chapterSceneName);
        }
    }
}
