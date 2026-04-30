using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D10 — Endgame 메뉴 풀 (endgame_v1 §9). 일일/주간/무한/고난도 진입.
    /// CH35 첫 클리어 후 활성. 각 모드는 EndgameModeData SO + EndgameResetService 가용성.
    /// MVP: 진입 모달 + 가용성 표시. 실제 보스 처치 흐름은 F4·F5 콘텐츠.
    /// </summary>
    public class EndgameMenuPanel : MonoBehaviour
    {
        [Header("4 모드 데이터 (B8)")]
        [SerializeField] private EndgameModeData _dailyBoss;
        [SerializeField] private EndgameModeData _weeklyBoss;
        [SerializeField] private EndgameModeData _infiniteTower;
        [SerializeField] private EndgameModeData _hardcoreChapter;

        [Header("표시")]
        [SerializeField] private Text _dailyText;
        [SerializeField] private Text _weeklyText;
        [SerializeField] private Text _infiniteText;
        [SerializeField] private Text _hardcoreText;

        [Header("버튼")]
        [SerializeField] private Button _dailyButton;
        [SerializeField] private Button _weeklyButton;
        [SerializeField] private Button _infiniteButton;
        [SerializeField] private Button _hardcoreButton;
        [SerializeField] private Button _closeButton;

        [Header("씬 이름 (Phase 2 placeholder)")]
        [SerializeField] private string _endgameSceneName = "Chapter";

        private void OnEnable()
        {
            if (_dailyButton != null)    _dailyButton.onClick.AddListener(() => Enter(_dailyBoss, EndgameResetService.IsDailyAvailable()));
            if (_weeklyButton != null)   _weeklyButton.onClick.AddListener(() => Enter(_weeklyBoss, EndgameResetService.IsWeeklyAvailable()));
            if (_infiniteButton != null) _infiniteButton.onClick.AddListener(() => Enter(_infiniteTower, true));
            if (_hardcoreButton != null) _hardcoreButton.onClick.AddListener(() => Enter(_hardcoreChapter, true));
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            Refresh();
        }

        private void OnDisable()
        {
            if (_dailyButton != null) _dailyButton.onClick.RemoveAllListeners();
            if (_weeklyButton != null) _weeklyButton.onClick.RemoveAllListeners();
            if (_infiniteButton != null) _infiniteButton.onClick.RemoveAllListeners();
            if (_hardcoreButton != null) _hardcoreButton.onClick.RemoveAllListeners();
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
        }

        private void Refresh()
        {
            bool dailyAvail = EndgameResetService.IsDailyAvailable();
            bool weeklyAvail = EndgameResetService.IsWeeklyAvailable();

            if (_dailyText != null)
                _dailyText.text = dailyAvail
                    ? "일일 보스 — 도전 가능"
                    : $"일일 보스 — 다음 도전까지 {FormatHm(EndgameResetService.GetSecondsUntilNextDailyReset())}";
            if (_weeklyText != null)
                _weeklyText.text = weeklyAvail
                    ? "주간 보스 — 도전 가능"
                    : $"주간 보스 — 다음 도전까지 {FormatDh(EndgameResetService.GetSecondsUntilNextWeeklyReset())}";
            if (_infiniteText != null)
                _infiniteText.text = $"무한 다락방 — 최고 깊이 {EndgameResetService.GetInfiniteMaxDepth()}층";
            if (_hardcoreText != null)
                _hardcoreText.text = "고난도 챕터 — CH 36~50";

            if (_dailyButton != null)  _dailyButton.interactable  = dailyAvail;
            if (_weeklyButton != null) _weeklyButton.interactable = weeklyAvail;
        }

        private void Enter(EndgameModeData mode, bool available)
        {
            if (mode == null || !available) return;
            // Phase 2 — 실제 보스 spawn / 보상 적용은 F4·F5 콘텐츠 작업.
            // MVP placeholder: 챕터 씬으로 이동 (보스 풀 빈 상태에서는 동작 안 함).
            Debug.Log($"[EndgameMenuPanel] Enter mode={mode.id} (placeholder)");
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

        public void Close() => gameObject.SetActive(false);
    }
}
