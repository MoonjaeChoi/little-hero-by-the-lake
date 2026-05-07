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
        [SerializeField] private CharacterPool _characterPool;  // 5021 T03

        public SaveData Data { get; private set; }
        public int StarFragments => Data?.starFragments ?? 0;
        public int MagicCrystals => Data?.magicCrystals ?? 0;   // ◇ A5
        public int Starlight     => Data?.starlight ?? 0;       // ⋆ A5
        // 5023 T02 — SelectedChapter / SelectedCharacter 는 MetaProgressionChapter / Character 로 이동
        public MemoryPool MemoryPool => _memoryPool;
        public MetaUpgradePool UpgradePool => _upgradePool;
        public EnemyPool EnemyPool => _enemyPool;
        public int MemoryDiscoveredCount => Data?.discoveredMemoryIds?.Length ?? 0;
        public int MemoryTotalCount => _memoryPool?.memories?.Length ?? 0;
        public int EnemyDefeatedCount => Data?.defeatedEnemyIds?.Length ?? 0;
        public int EnemyTotalCount => _enemyPool?.enemies?.Length ?? 0;

        // 5023 T02 D3=B — 정적 events (구독자 호환성: instance 만 제거)
        public static event System.Action<CharacterData> OnCharacterChanged;
        // weapon_v2 후속 — 직업 변경 시 무기 자동 unequip 트리거
        public static event System.Action<CharacterClass> OnCharacterClassChanged;
        public static event System.Action<MetaUpgradeData> OnUpgradeChanged;
        public static event System.Action<EnemyData> OnEnemyDefeated;

        public static event Action<int> OnStarFragmentsChanged;

        // 5023 T04 — 정적 event 외부 발행 불가 (CS0070) 회피 — Raise 헬퍼 (서비스 클래스가 호출)
        public static void RaiseCharacterChanged(CharacterData c) => OnCharacterChanged?.Invoke(c);
        public static void RaiseCharacterClassChanged(CharacterClass c) => OnCharacterClassChanged?.Invoke(c);
        public static void RaiseUpgradeChanged(MetaUpgradeData u) => OnUpgradeChanged?.Invoke(u);
        public static void RaiseEnemyDefeated(EnemyData e) => OnEnemyDefeated?.Invoke(e);
        public static void RaiseStarFragmentsChanged(int v) => OnStarFragmentsChanged?.Invoke(v);

        private void Awake()
        {
            // 5023 — Awake slim. Loader + Bootstrapper 분리.
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 5037.0 cascade — Load 또는 마이그레이션 throw 시에도 Data 보장 (NullRef 회피)
            try { Data = MetaProgressionLoader.Load(); }
            catch (System.Exception e) { Debug.LogWarning($"[MetaProgression] Load failed: {e.Message}"); Data = new SaveData(); }
            if (Data == null) Data = new SaveData();
            MetaProgressionCharacter.InitFromSave(MetaProgressionLoader.ResolveSelectedCharacterFromSave(_characterPool, Data));
            MetaProgressionBootstrapper.Run();
        }

        // 5037.0a — Singleton instance 가 destroy 시 정적 reference 정리 (Test 격리 + DontDestroyOnLoad 잔존 회피)
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 5037.0a — Test 전용 헬퍼. Test 프레임워크가 Singleton 정적 상태를 명시 reset.
        /// 어셈블리 분리 (LittleHero.Tests.Editor) 로 internal 접근 불가 → public + TestOnly_ prefix.
        /// </summary>
        public static void TestOnly_OverrideInstance(MetaProgression instance, SaveData data)
        {
            Instance = instance;
            if (instance != null) instance.Data = data ?? new SaveData();
        }
#endif

        private void OnEnable()
        {
            GameEvents.OnChapterCleared    += HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  += HandleMemoryDiscovered;
            GameEvents.OnEnemyKilled       += HandleEnemyKilled;
            GameEvents.OnPlayerDied        += MetaProgressionEquipment.FlushDrops;
        }

        private void OnDisable()
        {
            GameEvents.OnChapterCleared    -= HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  -= HandleMemoryDiscovered;
            GameEvents.OnEnemyKilled       -= HandleEnemyKilled;
            GameEvents.OnPlayerDied        -= MetaProgressionEquipment.FlushDrops;
        }

        // 5023 T03 — _pendingDrops + QueueDrop + FlushDrops 는 MetaProgressionEquipment 로 이동

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
            MetaProgressionEquipment.FlushDrops();  // PIVOT v3 — 챕터 클리어 시점 1회 Save
            MetaProgressionCurrency.AddStarFragments(chapter.starFragmentReward);
        }

        // 5023 T03 — Currency 메서드 (AddStarFragments / SpendXxx / AddMagicCrystals / AddStarlight 등) 는 MetaProgressionCurrency 로 이동

        // 5023 T02 — SelectCharacter / ResolveCharacterClass 는 MetaProgressionCharacter 로 이동

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
            if (!MetaProgressionCurrency.SpendStarFragments(cost)) return false;
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
