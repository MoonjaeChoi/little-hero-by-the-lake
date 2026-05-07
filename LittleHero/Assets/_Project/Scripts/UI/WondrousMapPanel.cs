using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D3 — 신기한 지도 (ui_v1 §7).
    /// 35 챕터 선형 지도. 각 챕터 아이콘 = (잠금 / 해방·미클리어 / 첫 클리어). 클릭 시 진입 모달.
    /// 진입 시 권장 레벨 미달이면 D12 RecommendedLevelWarningModal 호출 후 진입.
    /// </summary>
    public class WondrousMapPanel : MonoBehaviour
    {
        [Header("챕터 데이터 35종 (chapterNumber 1~35)")]
        [SerializeField] private ChapterData[] _chapters;

        [Header("아이콘 그리드 (35개, chapterNumber 순)")]
        [SerializeField] private WondrousMapNode[] _nodes;

        [Header("진입 모달")]
        [SerializeField] private GameObject _confirmModalRoot;
        [SerializeField] private Text _confirmTitleText;
        [SerializeField] private Text _confirmDescText;
        [SerializeField] private Text _confirmRewardText;
        [SerializeField] private Button _confirmStartButton;
        [SerializeField] private Button _confirmCancelButton;

        [Header("권장 레벨 경고 (D12)")]
        [SerializeField] private RecommendedLevelWarningModal _warningModal;

        [Header("닫기")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private string _chapterSceneName = "Chapter";

        private ChapterData _pendingChapter;

        private void OnEnable()
        {
            // 출시 전 fix #4 — raycast 누설 방지 (다른 형제 위에 그려져 입력 우선권)
            transform.SetAsLastSibling();

            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (_confirmStartButton != null) _confirmStartButton.onClick.AddListener(OnConfirmStart);
            if (_confirmCancelButton != null) _confirmCancelButton.onClick.AddListener(CloseConfirm);
            BindNodes();
            Refresh();
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            if (_confirmStartButton != null) _confirmStartButton.onClick.RemoveAllListeners();
            if (_confirmCancelButton != null) _confirmCancelButton.onClick.RemoveAllListeners();
            if (_nodes != null)
                foreach (var n in _nodes)
                    if (n != null) n.OnClicked -= HandleNodeClicked;
        }

        private void BindNodes()
        {
            if (_nodes == null) return;
            for (int i = 0; i < _nodes.Length; i++)
            {
                if (_nodes[i] == null) continue;
                _nodes[i].OnClicked -= HandleNodeClicked;
                _nodes[i].OnClicked += HandleNodeClicked;
            }
        }

        private void Refresh()
        {
            if (_nodes == null || _chapters == null) return;
            long firstMask = MetaProgression.Instance?.Data?.firstClearedChapters ?? 0L;
            int playerLevel = PlayerProgress.Instance?.Level ?? (MetaProgression.Instance?.Data?.level ?? 1);

            for (int i = 0; i < _nodes.Length; i++)
            {
                var node = _nodes[i];
                if (node == null) continue;
                ChapterData chapter = i < _chapters.Length ? _chapters[i] : null;
                int chapterNum = chapter != null ? chapter.chapterNumber : (i + 1);
                bool firstCleared = chapterNum >= 1 && chapterNum <= 50 && (firstMask & (1L << (chapterNum - 1))) != 0;
                bool unlocked = IsChapterUnlocked(chapterNum, firstMask);
                node.Bind(chapter, chapterNum, unlocked, firstCleared, playerLevel);
            }
        }

        // 5036 — 개발 단계 디버그 플래그. true 시 모든 챕터 진입 가능 + 권장 레벨 경고 비활성.
        // 5038.6 — Editor 모드 default true (사용자 요청 "모든 챕터 자유롭게"). iOS 빌드 시 false 유지.
        public static bool DevUnlockAllChapters =
#if UNITY_EDITOR
            true;
#else
            false;
#endif

        // chapter N 해방 = chapter N-1 첫 클리어. CH1 은 항상 해방.
        private static bool IsChapterUnlocked(int chapterNum, long firstMask)
        {
            if (DevUnlockAllChapters) return true;  // 5036 — 개발 모드
            if (chapterNum <= 1) return true;
            int prevBit = chapterNum - 2;  // 1-indexed → 0-indexed prev = (N-1)-1
            return (firstMask & (1L << prevBit)) != 0;
        }

        private void HandleNodeClicked(ChapterData chapter)
        {
            if (chapter == null) return;
            _pendingChapter = chapter;
            ShowConfirm(chapter);
        }

        private void ShowConfirm(ChapterData chapter)
        {
            if (_confirmModalRoot != null) _confirmModalRoot.SetActive(true);
            if (_confirmTitleText != null)
                _confirmTitleText.text = $"제 {chapter.chapterNumber}장 — {chapter.displayName}";
            if (_confirmDescText != null)
                _confirmDescText.text = chapter.description;
            if (_confirmRewardText != null)
                _confirmRewardText.text =
                    $"보상: 무지개 구슬 ✦ {chapter.starFragmentReward}" +
                    $" / 마법 결정 ◇ {CurrencyService.CalcChapterMagicCrystalReward(chapter.chapterNumber, true)}" +
                    $" / 별빛 ⋆ {CurrencyService.CalcChapterStarlightReward(chapter.chapterNumber)} (첫 클리어)";
        }

        private void OnConfirmStart()
        {
            if (_pendingChapter == null) return;
            // 5036 — 개발 모드: 권장 레벨 경고 스킵
            if (DevUnlockAllChapters) { StartChapter(); return; }

            int playerLevel = PlayerProgress.Instance?.Level ?? 1;
            int recommended = RecommendedLevelFor(_pendingChapter.chapterNumber);

            if (_warningModal != null && playerLevel < recommended)
            {
                _warningModal.Show(_pendingChapter, playerLevel, recommended, StartChapter);
                return;
            }
            StartChapter();
        }

        private void StartChapter()
        {
            if (_pendingChapter == null) return;
            if (MetaProgression.Instance != null)
                MetaProgressionChapter.SelectedChapter = _pendingChapter;
            CloseConfirm();
            SceneTransitionService.Transition(_chapterSceneName);
        }

        private void CloseConfirm()
        {
            if (_confirmModalRoot != null) _confirmModalRoot.SetActive(false);
            _pendingChapter = null;
        }

        // chapter_curve §3 권장 레벨 매트릭스 (러프 — 콘텐츠 작업에서 정밀 조정).
        // 1~5: lvl 1+, 6~10: 5+, 11~15: 10+, 16~20: 18+, 21~25: 28+, 26~30: 38+, 31~35: 48+
        private static int RecommendedLevelFor(int chapterNum)
        {
            if (chapterNum <= 5) return 1;
            if (chapterNum <= 10) return 5;
            if (chapterNum <= 15) return 10;
            if (chapterNum <= 20) return 18;
            if (chapterNum <= 25) return 28;
            if (chapterNum <= 30) return 38;
            return 48;
        }

        public void Close() => gameObject.SetActive(false);
    }
}
