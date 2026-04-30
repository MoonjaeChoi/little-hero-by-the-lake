using System;
using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public class MetaProgression : MonoBehaviour
    {
        public static MetaProgression Instance { get; private set; }

        [SerializeField] private MemoryPool _memoryPool;
        [SerializeField] private MetaUpgradePool _upgradePool;
        [SerializeField] private EnemyPool _enemyPool;

        public SaveData Data { get; private set; }
        public int StarFragments => Data?.starFragments ?? 0;
        public int MagicCrystals => Data?.magicCrystals ?? 0;   // ◇ A5
        public int Starlight     => Data?.starlight ?? 0;       // ⋆ A5
        public ChapterData SelectedChapter { get; set; }
        public CharacterData SelectedCharacter { get; private set; }
        public MemoryPool MemoryPool => _memoryPool;
        public MetaUpgradePool UpgradePool => _upgradePool;
        public EnemyPool EnemyPool => _enemyPool;
        public int MemoryDiscoveredCount => Data?.discoveredMemoryIds?.Length ?? 0;
        public int MemoryTotalCount => _memoryPool?.memories?.Length ?? 0;
        public int EnemyDefeatedCount => Data?.defeatedEnemyIds?.Length ?? 0;
        public int EnemyTotalCount => _enemyPool?.enemies?.Length ?? 0;

        public event System.Action<CharacterData> OnCharacterChanged;
        public event System.Action<MetaUpgradeData> OnUpgradeChanged;
        public event System.Action<EnemyData> OnEnemyDefeated;

        public event Action<int> OnStarFragmentsChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Data = SaveSystem.Load();
            // E3 — 별빛 무료 출처 자동 부트스트랩 (일일 로그인 + 이벤트 구독)
            StarlightGrantService.Bootstrap();
            // E1+E2 — IAP 백그라운드 초기화 (Editor·Services 미설정 시 에러 로그만, 게임플레이는 영향 X)
            try { IAPService.Bootstrap(); }
            catch (System.Exception e) { Debug.LogWarning($"[MetaProgression] IAP bootstrap skip: {e.Message}"); }
        }

        private void OnEnable()
        {
            GameEvents.OnChapterCleared    += HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  += HandleMemoryDiscovered;
            GameEvents.OnEnemyKilled       += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            GameEvents.OnChapterCleared    -= HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  -= HandleMemoryDiscovered;
            GameEvents.OnEnemyKilled       -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(EnemyData data)
        {
            if (data == null || string.IsNullOrEmpty(data.id)) return;
            if (IsEnemyDefeated(data.id)) return;

            var list = new List<string>(Data.defeatedEnemyIds ?? new string[0]);
            list.Add(data.id);
            Data.defeatedEnemyIds = list.ToArray();
            SaveSystem.Save(Data);
            OnEnemyDefeated?.Invoke(data);
        }

        public bool IsEnemyDefeated(string id)
        {
            if (Data?.defeatedEnemyIds == null) return false;
            for (int i = 0; i < Data.defeatedEnemyIds.Length; i++)
                if (Data.defeatedEnemyIds[i] == id) return true;
            return false;
        }

        private void HandleMemoryDiscovered(MemoryData memory)
        {
            if (memory == null || string.IsNullOrEmpty(memory.id)) return;
            if (IsMemoryDiscovered(memory.id)) return;

            var list = new List<string>(Data.discoveredMemoryIds ?? new string[0]);
            list.Add(memory.id);
            Data.discoveredMemoryIds = list.ToArray();
            SaveSystem.Save(Data);
        }

        public bool IsMemoryDiscovered(string id)
        {
            if (Data?.discoveredMemoryIds == null) return false;
            for (int i = 0; i < Data.discoveredMemoryIds.Length; i++)
                if (Data.discoveredMemoryIds[i] == id) return true;
            return false;
        }

        public bool AllMemoriesDiscovered()
        {
            return MemoryTotalCount > 0 && MemoryDiscoveredCount >= MemoryTotalCount;
        }

        private void HandleChapterCleared(ChapterData chapter)
        {
            if (chapter == null) return;
            AddStarFragments(chapter.starFragmentReward);
        }

        public void AddStarFragments(int amount)
        {
            if (amount <= 0) return;
            Data.starFragments += amount;
            OnStarFragmentsChanged?.Invoke(Data.starFragments);
            SaveSystem.Save(Data);
        }

        public bool SpendStarFragments(int amount)
        {
            if (amount <= 0 || Data.starFragments < amount) return false;
            Data.starFragments -= amount;
            OnStarFragmentsChanged?.Invoke(Data.starFragments);
            SaveSystem.Save(Data);
            return true;
        }

        // ✦ A5 — 마법 결정 ◇
        public void AddMagicCrystals(int amount)
        {
            if (amount <= 0) return;
            Data.magicCrystals += amount;
            GameEvents.RaiseMagicCrystalChanged(Data.magicCrystals);
            SaveSystem.Save(Data);
        }

        public bool SpendMagicCrystals(int amount)
        {
            if (amount <= 0 || Data.magicCrystals < amount) return false;
            Data.magicCrystals -= amount;
            GameEvents.RaiseMagicCrystalChanged(Data.magicCrystals);
            SaveSystem.Save(Data);
            return true;
        }

        // ✦ A5 — 별빛 ⋆
        public void AddStarlight(int amount)
        {
            if (amount <= 0) return;
            Data.starlight += amount;
            GameEvents.RaiseStarlightChanged(Data.starlight);
            SaveSystem.Save(Data);
        }

        public bool SpendStarlight(int amount)
        {
            if (amount <= 0 || Data.starlight < amount) return false;
            Data.starlight -= amount;
            GameEvents.RaiseStarlightChanged(Data.starlight);
            SaveSystem.Save(Data);
            return true;
        }

        public void SelectCharacter(CharacterData character)
        {
            SelectedCharacter = character;
            Data.selectedCharacterId = character != null ? character.id : null;
            SaveSystem.Save(Data);
            OnCharacterChanged?.Invoke(character);
        }

        public int GetUpgradeLevel(string id)
        {
            if (Data?.metaUpgrades == null || string.IsNullOrEmpty(id)) return 0;
            for (int i = 0; i < Data.metaUpgrades.Length; i++)
                if (Data.metaUpgrades[i] != null && Data.metaUpgrades[i].id == id)
                    return Data.metaUpgrades[i].level;
            return 0;
        }

        public int GetUpgradeCost(MetaUpgradeData u)
        {
            if (u == null) return int.MaxValue;
            int level = GetUpgradeLevel(u.id);
            return u.baseCost + u.costPerLevel * level;
        }

        public bool CanUpgrade(MetaUpgradeData u)
        {
            if (u == null) return false;
            int level = GetUpgradeLevel(u.id);
            if (level >= u.maxLevel) return false;
            return StarFragments >= GetUpgradeCost(u);
        }

        public bool TryUpgrade(MetaUpgradeData u)
        {
            if (!CanUpgrade(u)) return false;
            int cost = GetUpgradeCost(u);
            if (!SpendStarFragments(cost)) return false;
            SetUpgradeLevel(u.id, GetUpgradeLevel(u.id) + 1);
            OnUpgradeChanged?.Invoke(u);
            return true;
        }

        private void SetUpgradeLevel(string id, int level)
        {
            if (Data.metaUpgrades == null) Data.metaUpgrades = new MetaUpgradeEntry[0];
            for (int i = 0; i < Data.metaUpgrades.Length; i++)
            {
                if (Data.metaUpgrades[i] != null && Data.metaUpgrades[i].id == id)
                {
                    Data.metaUpgrades[i].level = level;
                    SaveSystem.Save(Data);
                    return;
                }
            }
            var list = new List<MetaUpgradeEntry>(Data.metaUpgrades);
            list.Add(new MetaUpgradeEntry { id = id, level = level });
            Data.metaUpgrades = list.ToArray();
            SaveSystem.Save(Data);
        }

        public float GetMetaBonus(MetaUpgradeStat stat)
        {
            if (_upgradePool == null || _upgradePool.upgrades == null) return 0f;
            float total = 0f;
            for (int i = 0; i < _upgradePool.upgrades.Length; i++)
            {
                var u = _upgradePool.upgrades[i];
                if (u == null || u.targetStat != stat) continue;
                total += u.bonusPerLevel * GetUpgradeLevel(u.id);
            }
            return total;
        }
    }
}
