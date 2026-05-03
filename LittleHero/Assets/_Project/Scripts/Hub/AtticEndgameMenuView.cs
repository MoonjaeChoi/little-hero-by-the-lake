using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5005 Sprint 4 — Endgame 메뉴 가용성 + cooldown 표시 (CH 35 첫 클리어 시 활성).
    /// AtticController 에서 추출 (§15.1 SRP).
    /// </summary>
    public class AtticEndgameMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _endgameMenuRoot;
        [SerializeField] private Text _dailyBossCooldownText;
        [SerializeField] private Text _weeklyBossCooldownText;
        [SerializeField] private Text _infiniteDepthText;

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
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
            const long target35 = (1L << 35) - 1;
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
    }
}
