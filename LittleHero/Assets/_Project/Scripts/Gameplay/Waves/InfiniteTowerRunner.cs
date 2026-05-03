using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleHero
{
    /// <summary>
    /// F6 — 무한 다락방 (endgame_v1 §6).
    /// ChapterRunner 와 동일한 구조이지만 totalWaves 무제한 + 깊이↑마다 적 스탯 곱셈 누적.
    /// 사망 시 도달 깊이를 EndgameResetService.RecordInfiniteDepth 로 영구 저장.
    ///
    /// 사용: Chapter.unity 씬의 ChapterRunner 대신 본 컴포넌트 부착, MetaProgression.SelectedChapter 무시.
    /// 적 풀은 인스펙터에서 모든 카테고리 archetype 합집합 또는 fantasy 풀 권장.
    /// </summary>
    [RequireComponent(typeof(WaveManager))]
    [RequireComponent(typeof(EnemySpawner))]
    public class InfiniteTowerRunner : MonoBehaviour
    {
        [SerializeField] private EnemyData[] _regularPool;
        [SerializeField] private EnemyData[] _miniBossPool;
        [SerializeField] private EnemyData _bossArchetype;

        [Header("깊이 곡선")]
        [Tooltip("깊이 1 의 적 스탯 곱셈 베이스. 챕터 35 곡선과 정합 권장 (1.10^34 ≈ 25.5)")]
        [SerializeField] private float _baseStatMultiplier = 25.55f;
        [Tooltip("깊이 +1 마다 추가 곱셈. 1.05 = 매 층 +5%")]
        [SerializeField] private float _depthScalingPerLevel = 1.05f;
        [Tooltip("미니보스 등장 주기 (5층마다 미니, N층마다 보스)")]
        [SerializeField] private int _miniBossEvery = 5;
        [SerializeField] private int _bossEvery = 10;

        [Header("웨이브")]
        [SerializeField] private float _waveDuration = 30f;
        [SerializeField] private int _baseSpawnCount = 8;

        [Header("사망 시 복귀")]
        [SerializeField] private string _atticSceneName = "Attic";
        [SerializeField] private float _returnDelay = 2.5f;

        private WaveManager _wave;
        private EnemySpawner _spawner;
        private int _currentDepth;
        private bool _running;

        private void Awake()
        {
            _wave = GetComponent<WaveManager>();
            _spawner = GetComponent<EnemySpawner>();
        }

        private void OnEnable()  => GameEvents.OnPlayerDied += HandleDied;
        private void OnDisable() => GameEvents.OnPlayerDied -= HandleDied;

        private void Start()
        {
            RegisterEnemies();
            _wave.Initialize(_spawner);
            GameManager.Instance?.ResetRun();
            _currentDepth = 0;
            _running = true;
            StartNextDepth();
        }

        private void RegisterEnemies()
        {
            foreach (var e in _regularPool) if (e != null) _spawner.RegisterEnemy(e);
            foreach (var e in _miniBossPool) if (e != null) _spawner.RegisterEnemy(e);
            if (_bossArchetype != null) _spawner.RegisterEnemy(_bossArchetype);
        }

        private void StartNextDepth()
        {
            if (!_running) return;
            _currentDepth++;
            // 챕터 곱셈 동적 적용 — Spawner 의 ActiveChapter 대신 임시 ChapterData 사용
            var virtualChapter = ScriptableObject.CreateInstance<ChapterData>();
            virtualChapter.id = $"infinite_d{_currentDepth}";
            virtualChapter.displayName = $"무한 다락방 {_currentDepth}층";
            virtualChapter.chapterNumber = _currentDepth;
            virtualChapter.enemyStatMultiplier = _baseStatMultiplier * Mathf.Pow(_depthScalingPerLevel, _currentDepth - 1);
            virtualChapter.totalWaves = 1;
            virtualChapter.waveDuration = _waveDuration;
            _spawner.SetActiveChapter(virtualChapter);

            bool isMini = (_currentDepth % _miniBossEvery == 0) && (_currentDepth % _bossEvery != 0);
            bool isBoss = (_currentDepth % _bossEvery == 0);
            int spawnCount = Mathf.RoundToInt(_baseSpawnCount * (1f + 0.05f * _currentDepth));

            var config = new WaveConfig
            {
                enemyPool  = _regularPool,
                spawnCount = spawnCount,
                duration   = _waveDuration,
                miniBoss   = isMini && _miniBossPool.Length > 0 ? _miniBossPool[Random.Range(0, _miniBossPool.Length)] : null,
                boss       = isBoss ? _bossArchetype : null,
            };
            _wave.StartWave(config, OnDepthCleared);
        }

        private void OnDepthCleared()
        {
            EndgameResetService.RecordInfiniteDepth(_currentDepth);
            // 보상: 깊이마다 별가루·xp 자동 적립 (적 처치로 이미 누적). 깊이 도달 보상은 endgame §6.4 참조.
            StartNextDepth();
        }

        private void HandleDied()
        {
            if (!_running) return;
            _running = false;
            EndgameResetService.RecordInfiniteDepth(_currentDepth);
            Debug.Log($"[InfiniteTower] 종료 — 도달 깊이 {_currentDepth}");
            StartCoroutine(LoadAtticAfterDelay());
        }

        private IEnumerator LoadAtticAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_returnDelay);
            Time.timeScale = 1f;
            SceneTransitionService.Transition(_atticSceneName);
        }
    }
}
