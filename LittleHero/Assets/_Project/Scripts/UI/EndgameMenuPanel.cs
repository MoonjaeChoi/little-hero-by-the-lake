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

        [Header("씬 이름")]
        [SerializeField] private string _bossEncounterSceneName = "BossEncounter";
        [SerializeField] private string _infiniteTowerSceneName = "InfiniteTower";

        [Header("F5 신화 보장 — 주간 보스 처치 시 사용")]
        [SerializeField] private MythicDefinition[] _mythicPool;

        [Header("F4·F5 — 즉시 처치 처리 (테스트용)")]
        [Tooltip("true = 보스 씬 진입 없이 즉시 처치 시뮬. false = BossEncounter 씬 진입.")]
        [SerializeField] private bool _instantCompleteForTesting = false;

        private void OnEnable()
        {
            // 5006 D3=C — Panel 자체 검증 (CH 35 클리어 OR 추억 4종 발견 둘 중 하나 만족)
            if (!IsEndgameUnlocked())
            {
                Debug.Log("[EndgameMenuPanel] CH 35 클리어 후 도전 가능 (또는 추억 4종 발견)");
                gameObject.SetActive(false);
                return;
            }

            if (_dailyButton != null)    _dailyButton.onClick.AddListener(OnDailyClicked);
            if (_weeklyButton != null)   _weeklyButton.onClick.AddListener(OnWeeklyClicked);
            if (_infiniteButton != null) _infiniteButton.onClick.AddListener(OnInfiniteClicked);
            if (_hardcoreButton != null) _hardcoreButton.onClick.AddListener(() => Enter(_hardcoreChapter, true));
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            Refresh();
        }

        private static bool IsEndgameUnlocked()
        {
            long mask = MetaProgression.Instance?.Data?.firstClearedChapters ?? 0L;
            const long target35 = (1L << 35) - 1;
            bool allChapters = (mask & target35) == target35;
            bool allMemories = MetaProgression.Instance != null && MetaProgression.Instance.AllMemoriesDiscovered();
            return allChapters || allMemories;
        }

        private void OnDailyClicked()
        {
            if (!EndgameResetService.IsDailyAvailable() || _dailyBoss == null) return;
            var boss = BossEncounterService.ResolveDailyBoss(_dailyBoss);
            if (boss == null) { Debug.LogWarning("[EndgameMenu] 일일 보스 풀이 비어있음"); return; }
            Debug.Log($"[EndgameMenu] 일일 보스: {boss.displayName}");

            if (_instantCompleteForTesting)
            {
                bool ok = BossEncounterService.CompleteDailyBoss(_dailyBoss);
                Debug.Log($"[EndgameMenu] 일일 보스 완료 처리: {ok}");
                Refresh();
            }
            else
            {
                // 5002 T08 — Launcher 위임 (정적 Context.Set 직접 호출 제거)
                BossEncounterLauncher.Launch(new BossEncounterRequest
                {
                    mode = BossEncounterContext.Mode.Daily,
                    boss = boss,
                    endgameMode = _dailyBoss,
                    mythicPool = _mythicPool,
                    returnSceneName = "Attic",
                });
            }
        }

        private void OnWeeklyClicked()
        {
            if (!EndgameResetService.IsWeeklyAvailable() || _weeklyBoss == null) return;
            var boss = BossEncounterService.ResolveWeeklyBoss(_weeklyBoss);
            if (boss == null) { Debug.LogWarning("[EndgameMenu] 주간 보스 풀이 비어있음"); return; }
            Debug.Log($"[EndgameMenu] 주간 보스: {boss.displayName}");

            if (_instantCompleteForTesting)
            {
                var result = BossEncounterService.CompleteWeeklyBoss(_weeklyBoss, _mythicPool);
                Debug.Log($"[EndgameMenu] 주간 보스 완료={result.success} 신화발현={(result.mythicGranted != null ? result.mythicGranted.instanceId : "없음")}");
                if (result.mythicGranted != null)
                {
                    var overlay = FindAnyObjectByType<MythicEffectOverlay>();
                    var def = ResolveMythicDefinition(result.mythicGranted.mythicId);
                    if (overlay != null && def != null) overlay.Play(def);
                }
                Refresh();
            }
            else
            {
                // 5002 T08 — Launcher 위임 (정적 Context.Set 직접 호출 제거)
                BossEncounterLauncher.Launch(new BossEncounterRequest
                {
                    mode = BossEncounterContext.Mode.Weekly,
                    boss = boss,
                    endgameMode = _weeklyBoss,
                    mythicPool = _mythicPool,
                    returnSceneName = "Attic",
                });
            }
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

        private void OnInfiniteClicked()
        {
            Debug.Log($"[EndgameMenu] 무한 다락방 진입 — scene={_infiniteTowerSceneName}");
            if (string.IsNullOrEmpty(_infiniteTowerSceneName)) return;
            Time.timeScale = 1f;
            SceneTransitionService.Transition(_infiniteTowerSceneName);
        }

        public void Close() => gameObject.SetActive(false);

        private MythicDefinition ResolveMythicDefinition(string id)
        {
            if (_mythicPool == null || string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < _mythicPool.Length; i++)
                if (_mythicPool[i] != null && _mythicPool[i].id == id) return _mythicPool[i];
            return null;
        }
    }
}
