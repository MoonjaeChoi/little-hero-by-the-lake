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
            GameEvents.OnPlayerDied += HandlePlayerDied;
            // C9 — 챕터 곱셈을 spawner 에 명시적으로 전달 (인스펙터·SelectedChapter 경로 일관)
            _spawner.SetActiveChapter(_chapterData);

            // BGM — 챕터 카테고리별 트랙 (chapter_structure_v2 §3 매핑)
            int chN = _chapterData != null ? _chapterData.chapterNumber : 1;
            string bgm = AudioManager.GetChapterBgm(chN);
            AudioManager.Instance?.PlayBgm(bgm, crossFadeSec: 1.0f);
            // Phase G2: 챕터 배경 동적 변경
            ApplyChapterBackground();
            RegisterEnemyPool();
            _waveManager.Initialize(_spawner);
            GameManager.Instance.ResetRun();   // 새 런: CurrentWave=0, StarDust 영구

            // 사용자 요청 2026-05-03 — 활성 챕터 session 있으면 wave 부터 재개
            int resumeWave = 0;
            var data = MetaProgression.Instance?.Data;
            if (data != null && _chapterData != null
                && !string.IsNullOrEmpty(data.activeChapterId)
                && data.activeChapterId == _chapterData.id
                && data.activeChapterWave > 0
                && data.activeChapterWave < _chapterData.totalWaves)
            {
                resumeWave = data.activeChapterWave;
                Debug.Log($"[ChapterRunner] 재개: wave {resumeWave} 부터");
            }
            for (int i = 0; i <= resumeWave; i++)
                GameManager.Instance.StartWave();  // 0→1, 또는 0→resumeWave+1

            StartNextWave();
        }

        public void SaveSession()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null || _chapterData == null) return;
            data.activeChapterId = _chapterData.id;
            data.activeChapterWave = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 0;
            SaveSystem.Save(data);
            Debug.Log($"[ChapterRunner] 세션 저장: {data.activeChapterId} wave {data.activeChapterWave}");
        }

        public static void ClearSession()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            data.activeChapterId = "";
            data.activeChapterWave = 0;
            SaveSystem.Save(data);
        }

        private void OnDestroy()
        {
            GameEvents.OnShopClosed -= OnShopClosed;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
        }

        private void HandlePlayerDied()
        {
            ClearSession();  // 사망 시 session 정리 (재개 X — 처음부터 시작)
        }

        private void StartNextWave()
        {
            int wave = GameManager.Instance.CurrentWave;
            WaveConfig config = BuildWaveConfig(wave);

            // BGM — 보스 웨이브(20) 진입 시 보스 트랙으로 crossfade
            if (config.boss != null)
                AudioManager.Instance?.PlayBgm("boss_chapter", crossFadeSec: 1.5f);
            else if (config.miniBoss != null)
                AudioManager.Instance?.PlayBgm("miniboss_chase", crossFadeSec: 1.0f);

            _waveManager.StartWave(config, OnWaveComplete);
        }

        private void OnWaveComplete()
        {
            if (GameManager.Instance.CurrentWave >= _chapterData.totalWaves)
            {
                GameEvents.RaiseChapterCleared(_chapterData);
                ClearSession();  // 클리어 시 session 정리
                return;
            }
            GameManager.Instance.EndWave();
            // PIVOT v2 — 상점 폐기 + 5초 휴식 후 자동 다음 웨이브 (game_pivot_v2.md §3.2).
            StartCoroutine(WaveRestCoroutine());
        }

        private System.Collections.IEnumerator WaveRestCoroutine()
        {
            // HP 50% 회복 (휴식 의미 부여)
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
            var stats = UnityEngine.Object.FindFirstObjectByType<PlayerStats>(FindObjectsInactive.Include);
            if (player != null && stats != null) player.Heal(stats.MaxHealth * 0.5f);
            yield return new WaitForSeconds(5f);
            StartNextWave();
        }

        private void OnShopClosed()
        {
            // PIVOT v2 — 상점 archive (game_pivot_v2.md §3.1). 호환성 위해 메서드 보존, 호출 X.
        }

        private void ApplyChapterBackground()
        {
            if (_backgroundRenderer == null)
            {
                var bgGO = GameObject.Find("Background");
                if (bgGO != null) _backgroundRenderer = bgGO.GetComponent<SpriteRenderer>();
            }
            if (_backgroundRenderer == null || _chapterData?.coverArt == null) return;
            // PIVOT v3 — 정적 BG (사용자 요청 2026-05-03: BG 안 움직이고 player·적만 이동).
            // Tiled 절대 좌표 + 매우 큰 size → player 가 영역 벗어나도 안 보일 때까지 충분.
            _backgroundRenderer.sprite = _chapterData.coverArt;
            _backgroundRenderer.sortingOrder = -100;
            _backgroundRenderer.drawMode = SpriteDrawMode.Tiled;
            _backgroundRenderer.tileMode = SpriteTileMode.Continuous;
            _backgroundRenderer.size = new Vector2(300f, 300f);  // 300×300 unit, 절대 좌표
            _backgroundRenderer.transform.localScale = Vector3.one;
            _backgroundRenderer.transform.position = Vector3.zero;
            _backgroundRenderer.color = Color.white;
            // BackgroundFollow 제거 (BG 정적, player 추적 X)
            var follow = _backgroundRenderer.GetComponent<BackgroundFollow>();
            if (follow != null) Object.Destroy(follow);
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
