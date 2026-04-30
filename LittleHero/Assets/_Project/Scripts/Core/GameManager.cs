using UnityEngine;

namespace LittleHero
{
    public enum GameState { Hub, Wave, Shop, Paused, GameOver }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private BalanceConfig _balanceConfig;

        public GameState CurrentState { get; private set; }
        public int CurrentWave { get; private set; }
        public int StarDust { get; private set; }
        public BalanceConfig Balance => _balanceConfig;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyKilled    += HandleEnemyKilled;
            GameEvents.OnPlayerDied     += HandlePlayerDied;
            GameEvents.OnChapterCleared += HandleChapterCleared;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyKilled    -= HandleEnemyKilled;
            GameEvents.OnPlayerDied     -= HandlePlayerDied;
            GameEvents.OnChapterCleared -= HandleChapterCleared;
        }

        private void HandleChapterCleared(ChapterData data)
        {
            SetState(GameState.Paused);
            Time.timeScale = 0f;
        }

        private void HandleEnemyKilled(EnemyData data)
        {
            if (data == null) return;
            float dustMult = 1f + (MetaProgression.Instance != null
                ? MetaProgression.Instance.GetMetaBonus(MetaUpgradeStat.StarDustGain)
                : 0f);
            int drop = Mathf.RoundToInt(data.starDustDrop * dustMult);
            if (drop > 0) AddStarDust(drop);
        }

        private void HandlePlayerDied()
        {
            SetState(GameState.GameOver);
            Time.timeScale = 0f;
        }

        public void RestartCurrentScene()
        {
            Time.timeScale = 1f;
            ResetRun();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        public void StartWave()
        {
            CurrentWave++;
            SetState(GameState.Wave);
        }

        public void EndWave()
        {
            GameEvents.RaiseWaveCleared(CurrentWave);
            SetState(GameState.Shop);
            GameEvents.RaiseShopOpened();
        }

        public void CloseShop()
        {
            StartWave();                  // CurrentWave++ 먼저
            GameEvents.RaiseShopClosed(); // 이후 이벤트 발행
        }

        public void AddStarDust(int amount)
        {
            StarDust += amount;
            GameEvents.RaiseStarDustChanged(StarDust);
        }

        public bool SpendStarDust(int amount)
        {
            if (StarDust < amount) return false;
            StarDust -= amount;
            GameEvents.RaiseStarDustChanged(StarDust);
            return true;
        }

        public void ResetRun()
        {
            CurrentWave = 0;
            StarDust = 0;
            SetState(GameState.Hub);
        }

        private void SetState(GameState state)
        {
            CurrentState = state;
        }
    }
}
