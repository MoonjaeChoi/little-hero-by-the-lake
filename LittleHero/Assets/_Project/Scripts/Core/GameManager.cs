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
        // PIVOT v2 — 별가루 ★ 는 SaveData.starDust 영구 저장. GameManager 는 라우팅 facade.
        public int StarDust => MetaProgression.Instance != null ? MetaProgression.Instance.Data.starDust : 0;
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

        private float _pendingDust;  // PIVOT v3.1 — fractional 누적 (×0.1 RoundToInt=0 문제 해결)

        private void HandleEnemyKilled(EnemyData data)
        {
            if (data == null) return;
            float dustMult = 1f + (MetaProgression.Instance != null
                ? MetaProgression.Instance.GetMetaBonus(MetaUpgradeStat.StarDustGain)
                : 0f);
            // PIVOT v3 — 적 별가루 × 0.1 (보스 제외). 마리수 ×10 으로 합산 동일.
            float v3DustMul = (Balance != null && !data.isBoss) ? Balance.enemyStarDustMul : 1f;
            float dropF = data.starDustDrop * dustMult * v3DustMul;
            // PIVOT v3.1 — fractional 누적: 1.6 × 0.1 = 0.16 → RoundToInt = 0 → 별가루 0 문제 해결.
            // 누적치 1.0 도달 시 정수 단위로 AddStarDust.
            _pendingDust += dropF;
            int wholeAmount = (int)_pendingDust;
            if (wholeAmount > 0)
            {
                _pendingDust -= wholeAmount;
                AddStarDust(wholeAmount);
            }
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
            SceneTransitionService.Transition(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void StartWave()
        {
            CurrentWave++;
            SetState(GameState.Wave);
            // PIVOT v2 fix — HUD 웨이브 라벨 갱신 보장. CurrentWave 증가 후 OnShopClosed 재발행.
            // (직전 EndWave 의 즉시 발행은 CurrentWave 증가 전이라 HUD 가 같은 번호 표시).
            GameEvents.RaiseShopClosed();
        }

        public void EndWave()
        {
            // PIVOT v2 — 상점 페이즈 폐기 (game_pivot_v2.md §3.1).
            // 웨이브 클리어 이벤트만 발행. 다음 웨이브 진행은 ChapterRunner 의 5초 휴식 코루틴이 담당.
            GameEvents.RaiseWaveCleared(CurrentWave);
            // 호환성 — 비활성 ShopPanel 이 OnShopOpened/OnShopClosed 구독을 정리할 수 있게 발행 후 즉시 close.
            GameEvents.RaiseShopOpened();
            GameEvents.RaiseShopClosed();
        }

        public void CloseShop()
        {
            StartWave();                  // CurrentWave++ 먼저
            GameEvents.RaiseShopClosed(); // 이후 이벤트 발행
        }

        public void AddStarDust(int amount)
        {
            if (amount <= 0) return;
            var meta = MetaProgression.Instance;
            if (meta == null) return;
            meta.Data.starDust += amount;
            GameEvents.RaiseStarDustChanged(meta.Data.starDust);
            SaveSystem.Save(meta.Data);
        }

        public bool SpendStarDust(int amount)
        {
            if (amount <= 0) return false;
            var meta = MetaProgression.Instance;
            if (meta == null || meta.Data.starDust < amount) return false;
            meta.Data.starDust -= amount;
            GameEvents.RaiseStarDustChanged(meta.Data.starDust);
            SaveSystem.Save(meta.Data);
            return true;
        }

        public void ResetRun()
        {
            CurrentWave = 0;
            // PIVOT v2 — 별가루 영구화 후 ResetRun 시 별가루 리셋 X.
            SetState(GameState.Hub);
        }

        private void SetState(GameState state)
        {
            CurrentState = state;
        }
    }
}
