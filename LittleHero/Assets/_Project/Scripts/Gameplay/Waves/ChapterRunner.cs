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
            if (MetaProgression.Instance != null && MetaProgressionChapter.SelectedChapter != null)
                _chapterData = MetaProgressionChapter.SelectedChapter;

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
            BalanceConfig balance = GameManager.Instance.Balance;

            // 5002 — 챕터 보스 wave 진입 시 BossEncounter 씬으로 분리 (옵션 C)
            if (wave >= _chapterData.totalWaves && _chapterData.bossEnemy != null)
            {
                SaveSession();
                BossEncounterLauncher.Launch(new BossEncounterRequest
                {
                    mode = BossEncounterContext.Mode.ChapterBoss,
                    boss = _chapterData.bossEnemy,
                    chapter = _chapterData,
                    returnSceneName = "Attic",
                });
                return;
            }

            // 5003 — 미니보스 wave 분리 (BalanceConfig.miniBossSeparation 토글)
            bool isMiniBossWave = wave > 0 && balance.miniBossWaveInterval > 0
                && wave % balance.miniBossWaveInterval == 0
                && wave < _chapterData.totalWaves;
            int lastMiniBossWave = _chapterData.totalWaves - balance.miniBossWaveInterval;
            bool shouldSeparateMini = isMiniBossWave && _chapterData.miniBossPool != null && _chapterData.miniBossPool.Length > 0
                && (balance.miniBossSeparation == MiniBossSeparationMode.All
                    || (balance.miniBossSeparation == MiniBossSeparationMode.LastOnly && wave == lastMiniBossWave));
            if (shouldSeparateMini)
            {
                var mb = _chapterData.miniBossPool[Random.Range(0, _chapterData.miniBossPool.Length)];
                SaveSession();
                BossEncounterLauncher.Launch(new BossEncounterRequest
                {
                    mode = BossEncounterContext.Mode.MiniBoss,
                    boss = mb,
                    chapter = _chapterData,
                    returnSceneName = "Chapter",  // 챕터 씬 복귀 + wave 재개
                });
                return;
            }

            WaveConfig config = BuildWaveConfig(wave);

            // BGM — 미니보스 wave 진입 시 트랙 crossfade (보스 wave 는 BossEncounter 씬으로 분리)
            if (config.miniBoss != null)
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
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>(FindObjectsInactive.Include);
            var stats = UnityEngine.Object.FindAnyObjectByType<PlayerStats>(FindObjectsInactive.Include);
            if (player != null && stats != null) player.Heal(stats.MaxHealth * 0.5f);
            // 사용자 baseline (2026-05-05) — Realtime 사용 (timeScale 영향 X). HitPause 누적 시 wave 진행 막힘 회피.
            yield return new WaitForSecondsRealtime(5f);
            // 사용자 보고 (2026-05-05) — wave 2+ 진입 시 CurrentWave++ 누락 fix.
            // PIVOT v2 상점 폐기 시 CloseShop() 의 StartWave() 호출이 사라지며 발생한 회귀.
            GameManager.Instance.StartWave();
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
            // 5002 D1 Rebuttal — BackgroundApplier 정적 헬퍼로 위임 (DRY).
            BackgroundApplier.Apply(_backgroundRenderer, _chapterData);
        }

        private WaveConfig BuildWaveConfig(int waveNumber)
        {
            // 5038 step 3 (대안) — waves[N-1] 가 set 시 spawnCount 만 WaveComposition 합산 사용.
            //   enemyPool / miniBoss 는 _chapterData 답습 (RegisterEnemyPool 이미 등록 — 변경 X).
            //   동적 RegisterEnemy 호출 제거 — Spawner 상태 안전.
            if (_chapterData.waves != null && waveNumber >= 1 && waveNumber <= _chapterData.waves.Length)
            {
                var wc = _chapterData.waves[waveNumber - 1];
                if (wc != null && wc.compositions != null && wc.compositions.Length > 0)
                    return BuildFromComposition(wc, waveNumber);
            }

            BalanceConfig balance = GameManager.Instance.Balance;
            bool isMiniB = waveNumber % balance.miniBossWaveInterval == 0
                           && waveNumber < _chapterData.totalWaves;
            // 5002 T07 — bool isBoss 변수 제거. 챕터 보스는 StartNextWave 분기에서 BossEncounter 씬으로 진입.

            int spawnCount = Mathf.RoundToInt(
                10f * (1f + balance.waveScalingFactor * waveNumber));

            EnemyData mb = isMiniB && _chapterData.miniBossPool != null && _chapterData.miniBossPool.Length > 0
                           ? _chapterData.miniBossPool[Random.Range(0, _chapterData.miniBossPool.Length)]
                           : null;
            // 5002 T07 — 챕터 보스는 BossEncounter 씬에서 처리 (StartNextWave 분기). isBoss 분기 제거.

            return new WaveConfig
            {
                enemyPool  = _chapterData.enemyPool,
                spawnCount = spawnCount,
                duration   = _chapterData.waveDuration,
                miniBoss   = mb,
                boss       = null,
            };
        }

        // 5038 step 3 (대안) — WaveComposition 의 count 합산만 사용. enemyPool / miniBoss 는 _chapterData 답습.
        // 동적 RegisterEnemy 호출 X — Spawner 상태 보존 (RegisterEnemyPool 등록만 사용).
        private WaveConfig BuildFromComposition(WaveComposition wc, int waveNumber)
        {
            int total = 0;
            for (int i = 0; i < wc.compositions.Length; i++)
            {
                var ec = wc.compositions[i];
                if (ec != null) total += ec.count;
            }
            // miniBoss 는 _chapterData.miniBossPool 답습 (Elite waveType 시 wave 5/10/15 정합)
            BalanceConfig balance = GameManager.Instance.Balance;
            bool isMiniB = wc.waveType == WaveType.Elite
                           && _chapterData.miniBossPool != null && _chapterData.miniBossPool.Length > 0;
            EnemyData mb = isMiniB ? _chapterData.miniBossPool[Random.Range(0, _chapterData.miniBossPool.Length)] : null;
            return new WaveConfig
            {
                enemyPool  = _chapterData.enemyPool,
                spawnCount = total,
                duration   = wc.waveDuration > 0f ? wc.waveDuration : _chapterData.waveDuration,
                miniBoss   = mb,
                boss       = null,
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
