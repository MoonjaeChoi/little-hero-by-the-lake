using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleHero
{
    /// <summary>
    /// F4·F5 — 일일/주간 보스 전투 씬 컴포넌트.
    /// BossEncounterContext.PendingBoss 단일 spawn → 처치 시 BossEncounterService 호출 → 다락방 복귀.
    /// 사망 시 보상 없이 다락방 복귀.
    ///
    /// 사용: Chapter.unity 복제 → ChapterRunner 제거 → 본 컴포넌트 추가 → Scenes/BossEncounter.unity 저장.
    /// </summary>
    [RequireComponent(typeof(WaveManager))]
    [RequireComponent(typeof(EnemySpawner))]
    public class BossEncounterRunner : MonoBehaviour
    {
        [SerializeField] private string _atticSceneName = "Attic";
        [Tooltip("보스 처치 후 다락방 복귀까지 대기 시간 (별 흩어짐 VFX 보여주기 위함)")]
        [SerializeField] private float _returnDelay = 2.0f;
        [Tooltip("보스 전투 최대 시간 (초). 제한 시간 도달 시 보스 도주 처리(=사망과 동일).")]
        [SerializeField] private float _battleDuration = 90f;
        [Tooltip("5002 — ChapterBoss 모드에서 챕터 BG 적용용 (BackgroundApplier 호출 대상)")]
        [SerializeField] private SpriteRenderer _backgroundRenderer;

        private WaveManager _wave;
        private EnemySpawner _spawner;
        private bool _resolved;

        private void Awake()
        {
            _wave = GetComponent<WaveManager>();
            _spawner = GetComponent<EnemySpawner>();
        }

        private void OnEnable()  => GameEvents.OnPlayerDied += HandleDied;
        private void OnDisable() => GameEvents.OnPlayerDied -= HandleDied;

        private void Start()
        {
            if (!BossEncounterContext.HasPending)
            {
                Debug.LogWarning("[BossEncounter] PendingBoss null — Attic 복귀");
                SceneManager.LoadScene(_atticSceneName);
                return;
            }

            _wave.Initialize(_spawner);
            GameManager.Instance?.ResetRun();

            var boss = BossEncounterContext.PendingBoss;
            _spawner.RegisterEnemy(boss);

            // 5002 ChapterBoss / 5003 MiniBoss — 챕터 BG 적용
            if (BossEncounterContext.PendingType == BossEncounterContext.Mode.ChapterBoss
                || BossEncounterContext.PendingType == BossEncounterContext.Mode.MiniBoss)
                BackgroundApplier.Apply(_backgroundRenderer, BossEncounterContext.PendingChapter);

            var config = new WaveConfig
            {
                enemyPool  = new EnemyData[0],
                spawnCount = 0,
                duration   = _battleDuration,
                miniBoss   = null,
                boss       = boss,
            };
            _wave.StartWave(config, OnBossDefeated);

            Debug.Log($"[BossEncounter] 시작 — type={BossEncounterContext.PendingType} boss={boss.displayName}");
        }

        private void OnBossDefeated()
        {
            if (_resolved) return;
            _resolved = true;

            switch (BossEncounterContext.PendingType)
            {
                case BossEncounterContext.Mode.Daily:
                    bool ok = BossEncounterService.CompleteDailyBoss(BossEncounterContext.PendingMode);
                    Debug.Log($"[BossEncounter] 일일 보스 완료 처리={ok}");
                    BossEncounterContext.LastMythicGranted = null;
                    break;
                case BossEncounterContext.Mode.Weekly:
                    var result = BossEncounterService.CompleteWeeklyBoss(
                        BossEncounterContext.PendingMode,
                        BossEncounterContext.PendingMythicPool);
                    Debug.Log($"[BossEncounter] 주간 보스 완료={result.success} 신화={(result.mythicGranted?.instanceId ?? "없음")}");
                    BossEncounterContext.LastMythicGranted = result.mythicGranted;
                    // Pool 에서 def lookup 후 보관 → 다락방에서 Overlay.Play 직접 호출
                    BossEncounterContext.LastMythicDefinition = ResolveMythicDef(
                        result.mythicGranted?.mythicId,
                        BossEncounterContext.PendingMythicPool);
                    break;
                case BossEncounterContext.Mode.ChapterBoss:
                    // 5002 T04 — T06 에서 본체 구현. 본 시점에는 stub 호출 (T06 추가)
                    BossEncounterService.CompleteChapterBoss(
                        BossEncounterContext.PendingBoss,
                        BossEncounterContext.PendingChapter);
                    break;
                case BossEncounterContext.Mode.MiniBoss:
                    // 5003 T02 — 미니보스 처치. 챕터 클리어 X (firstClearedChapters 변동 X).
                    BossEncounterService.CompleteMiniBoss(
                        BossEncounterContext.PendingBoss,
                        BossEncounterContext.PendingChapter);
                    break;
            }
            string returnScene = !string.IsNullOrEmpty(BossEncounterContext.PendingReturnScene)
                ? BossEncounterContext.PendingReturnScene : _atticSceneName;
            BossEncounterContext.Clear();
            StartCoroutine(LoadSceneAfterDelay(returnScene));
        }

        private void HandleDied()
        {
            if (_resolved) return;
            _resolved = true;
            Debug.Log("[BossEncounter] 사망 — 보상 없이 Attic 복귀");
            string returnScene = !string.IsNullOrEmpty(BossEncounterContext.PendingReturnScene)
                ? BossEncounterContext.PendingReturnScene : _atticSceneName;
            BossEncounterContext.Clear();
            BossEncounterContext.LastMythicGranted = null;
            StartCoroutine(LoadSceneAfterDelay(returnScene));
        }

        private IEnumerator LoadSceneAfterDelay(string scene)
        {
            yield return new WaitForSecondsRealtime(_returnDelay);
            Time.timeScale = 1f;
            SceneManager.LoadScene(scene);
        }

        private static MythicDefinition ResolveMythicDef(string id, MythicDefinition[] pool)
        {
            if (string.IsNullOrEmpty(id) || pool == null) return null;
            for (int i = 0; i < pool.Length; i++)
                if (pool[i] != null && pool[i].id == id) return pool[i];
            return null;
        }
    }
}
