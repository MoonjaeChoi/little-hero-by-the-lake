using UnityEngine;

namespace LittleHero
{
    public class ChapterRunner : MonoBehaviour
    {
        [SerializeField] private ChapterData _chapterData;
        [SerializeField] private SpriteRenderer _backgroundRenderer;  // Phase G2: chapter coverArt 적용용

        private WaveManager _waveManager;
        private EnemySpawner _spawner;

        private void Awake()
        {
            _waveManager = GetComponent<WaveManager>();
            _spawner = GetComponent<EnemySpawner>();
        }

        private void Start()
        {
            // Attic에서 선택된 챕터가 있으면 그것을 우선 사용
            if (MetaProgression.Instance != null && MetaProgression.Instance.SelectedChapter != null)
                _chapterData = MetaProgression.Instance.SelectedChapter;

            GameEvents.OnShopClosed += OnShopClosed;
            // C9 — 챕터 곱셈을 spawner 에 명시적으로 전달 (인스펙터·SelectedChapter 경로 일관)
            _spawner.SetActiveChapter(_chapterData);
            // Phase G2: 챕터 배경 동적 변경
            ApplyChapterBackground();
            RegisterEnemyPool();
            _waveManager.Initialize(_spawner);
            GameManager.Instance.ResetRun();   // 새 런: CurrentWave=0, StarDust=0
            GameManager.Instance.StartWave();  // 0 → 1
            StartNextWave();
        }

        private void OnDestroy()
        {
            GameEvents.OnShopClosed -= OnShopClosed;
        }

        private void StartNextWave()
        {
            WaveConfig config = BuildWaveConfig(GameManager.Instance.CurrentWave);
            _waveManager.StartWave(config, OnWaveComplete);
        }

        private void OnWaveComplete()
        {
            if (GameManager.Instance.CurrentWave >= _chapterData.totalWaves)
            {
                GameEvents.RaiseChapterCleared(_chapterData);
                return;
            }
            GameManager.Instance.EndWave();
        }

        private void OnShopClosed()
        {
            StartNextWave();
        }

        private void ApplyChapterBackground()
        {
            if (_backgroundRenderer == null)
            {
                var bgGO = GameObject.Find("Background");
                if (bgGO != null) _backgroundRenderer = bgGO.GetComponent<SpriteRenderer>();
            }
            if (_backgroundRenderer == null || _chapterData?.coverArt == null) return;
            _backgroundRenderer.sprite = _chapterData.coverArt;
            _backgroundRenderer.sortingOrder = -100;
            _backgroundRenderer.drawMode = SpriteDrawMode.Simple;
            // 1024×768 PNG (PPU 100) → 10.24 unit. Camera ortho width ≈ 16 → scale 1.6 으로 화면 채움.
            _backgroundRenderer.transform.localScale = new Vector3(1.6f, 1.6f, 1f);
            _backgroundRenderer.transform.position = Vector3.zero;
            _backgroundRenderer.color = Color.white;
        }

        private WaveConfig BuildWaveConfig(int waveNumber)
        {
            BalanceConfig balance = GameManager.Instance.Balance;
            bool isMiniB = waveNumber % balance.miniBossWaveInterval == 0
                           && waveNumber < _chapterData.totalWaves;
            bool isBoss = waveNumber >= _chapterData.totalWaves;

            int spawnCount = Mathf.RoundToInt(
                10f * (1f + balance.waveScalingFactor * waveNumber));

            EnemyData mb = isMiniB && _chapterData.miniBossPool != null && _chapterData.miniBossPool.Length > 0
                           ? _chapterData.miniBossPool[Random.Range(0, _chapterData.miniBossPool.Length)]
                           : null;
            EnemyData bs = isBoss ? _chapterData.bossEnemy : null;

            return new WaveConfig
            {
                enemyPool  = _chapterData.enemyPool,
                spawnCount = spawnCount,
                duration   = _chapterData.waveDuration,
                miniBoss   = mb,
                boss       = bs,
            };
        }

        private void RegisterEnemyPool()
        {
            if (_chapterData == null) return;

            foreach (EnemyData e in _chapterData.enemyPool)
                _spawner.RegisterEnemy(e);

            foreach (EnemyData mb in _chapterData.miniBossPool)
                _spawner.RegisterEnemy(mb);

            if (_chapterData.bossEnemy != null)
                _spawner.RegisterEnemy(_chapterData.bossEnemy);
        }
    }
}
