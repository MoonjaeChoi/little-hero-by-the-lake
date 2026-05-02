using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// A2 — XP 적립·레벨업 매니저 (싱글턴, DontDestroyOnLoad).
    /// L100 piecewise 곡선(L1~50 1.15^L, L51~100 1.05^(L-50)).
    /// 사망 시 XP 후퇴 없음 (자부심 보호 — character_progression §1).
    /// SaveData.level / xp 영구 저장.
    /// (참조: docs/little_hero/character_progression_v1.md §3 / §4 / §5 / §7)
    /// </summary>
    public class PlayerProgress : MonoBehaviour
    {
        public static PlayerProgress Instance { get; private set; }

        // 어떤 씬으로 시작하든(Attic·Chapter) 자동으로 인스턴스화. 사용자가 prefab/씬에 부착 불필요.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[PlayerProgress]");
            go.AddComponent<PlayerProgress>();
        }

        public const int LEVEL_CAP_FULL    = 100;
        public const int LEVEL_CURVE_BREAK = 50;
        public const int L1_TO_L2_XP       = 50;

        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }
        public int NextLevelXp { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromSave();
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyKilled       += HandleEnemyKilled;
            GameEvents.OnChapterCleared    += HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  += HandleMemoryDiscovered;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyKilled       -= HandleEnemyKilled;
            GameEvents.OnChapterCleared    -= HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  -= HandleMemoryDiscovered;
        }

        private void LoadFromSave()
        {
            var data = MetaProgression.Instance != null ? MetaProgression.Instance.Data : null;
            if (data != null)
            {
                Level = Mathf.Clamp(data.level, 1, LEVEL_CAP_FULL);
                Xp = Mathf.Max(0, data.xp);
            }
            else
            {
                Level = 1;
                Xp = 0;
            }
            NextLevelXp = CalcNextLevelXp(Level);
        }

        // PlayerStats.Initialize 직후 호출. 씬 진입 시점에 코어 등 절대 동기화.
        public void SyncStatsToCurrentLevel(PlayerStats stats)
        {
            if (stats != null) stats.SetLevel(Level);
        }

        // character_progression §3.1 — piecewise 곡선
        public static int CalcNextLevelXp(int currentLevel)
        {
            if (currentLevel <= 0) return L1_TO_L2_XP;
            if (currentLevel == 1) return L1_TO_L2_XP;
            if (currentLevel <= LEVEL_CURVE_BREAK)
                return Mathf.RoundToInt(50f * Mathf.Pow(1.15f, currentLevel));
            float at50 = 50f * Mathf.Pow(1.15f, LEVEL_CURVE_BREAK);
            return Mathf.RoundToInt(at50 * Mathf.Pow(1.05f, currentLevel - LEVEL_CURVE_BREAK));
        }

        private int CurrentChapterNumber()
            => MetaProgression.Instance?.SelectedChapter?.chapterNumber ?? 0;

        // character_progression §5.1 — 시뮬 §5.1 50% 컷 (저챕터 XP 과잉 보정 2026-05-01).
        // 일반 적 = 1, 미니보스 = 10, 보스 = 40
        // 모디파이어 = 1 + chapterN × 0.1
        private void HandleEnemyKilled(EnemyData enemy)
        {
            if (enemy == null) return;
            int baseXp = enemy.isBoss ? 40 : enemy.isMiniB ? 10 : 1;
            int n = CurrentChapterNumber();
            float modifier = 1f + n * 0.1f;
            // PIVOT v3 — 적 XP × 0.1 (보스 제외). 마리수 ×10 으로 합산 동일.
            var balance = GameManager.Instance != null ? GameManager.Instance.Balance : null;
            float v3XpMul = (balance != null && !enemy.isBoss) ? balance.enemyXpMul : 1f;
            int gain = Mathf.RoundToInt(baseXp * modifier * v3XpMul);
            AddXp(gain);
        }

        private void HandleChapterCleared(ChapterData chapter)
        {
            if (chapter == null) return;
            // 챕터 클리어 보너스 = N × 20 (시뮬 §5.1 50% 컷). chapterNumber=0 → 0 (B7 채움 전)
            AddXp(chapter.chapterNumber * 20);
        }

        private void HandleMemoryDiscovered(MemoryData _)
        {
            AddXp(20);  // 시뮬 §5.1 50% 컷
        }

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            if (Level >= LEVEL_CAP_FULL)
            {
                Xp = 0;
                PersistAndNotify(leveledUp: false);
                return;
            }

            Xp += amount;

            bool leveledUp = false;
            int safety = 0;
            while (Xp >= NextLevelXp && Level < LEVEL_CAP_FULL && safety++ < 200)
            {
                Xp -= NextLevelXp;
                Level += 1;
                NextLevelXp = CalcNextLevelXp(Level);
                ApplyLevelUpToStats();
                GameEvents.RaiseLevelUp(Level);
                leveledUp = true;
            }
            if (Level >= LEVEL_CAP_FULL) Xp = 0;

            PersistAndNotify(leveledUp);
        }

        private void ApplyLevelUpToStats()
        {
            // 씬 안에 PlayerStats 가 있으면 절대 동기화. 없으면 스킵 (Hub/Attic 씬 등).
            var stats = FindFirstObjectByType<PlayerStats>();
            if (stats != null) stats.SetLevel(Level);
        }

        private void PersistAndNotify(bool leveledUp)
        {
            var meta = MetaProgression.Instance;
            if (meta != null && meta.Data != null)
            {
                meta.Data.level = Level;
                meta.Data.xp    = Xp;
                SaveSystem.Save(meta.Data);
            }
            // leveledUp 미사용 (RaiseLevelUp 은 루프 안에서 이미 발행). 향후 HUD XP 바 즉시 갱신 이벤트 추가 시 활용 가능.
        }
    }
}
