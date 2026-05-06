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
        // weapon_v2 후속 — 직업 변경 시 무기 자동 unequip 트리거
        public event System.Action<CharacterClass> OnCharacterClassChanged;
        public event System.Action<MetaUpgradeData> OnUpgradeChanged;
        public event System.Action<EnemyData> OnEnemyDefeated;

        public event Action<int> OnStarFragmentsChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Data = SaveSystem.Load();
            ResolveSelectedCharacterFromSave();  // 5021 T03 — Chapter 직접 진입 시 portrait 자동 적용
            // E3 — 별빛 무료 출처 자동 부트스트랩 (일일 로그인 + 이벤트 구독)
            StarlightGrantService.Bootstrap();
            // E1+E2 — IAP 백그라운드 초기화 (Editor·Services 미설정 시 에러 로그만, 게임플레이는 영향 X)
            try { IAPService.Bootstrap(); }
            catch (System.Exception e) { Debug.LogWarning($"[MetaProgression] IAP bootstrap skip: {e.Message}"); }
            // PIVOT v2 — ItemDropService 부트스트랩 (game_pivot_v2.md §10).
            try { BootstrapItemDropService(); }
            catch (System.Exception e) { Debug.LogWarning($"[MetaProgression] ItemDrop bootstrap skip: {e.Message}"); }
        }

        // 5021 T03 — selectedCharacterId 를 SelectedCharacter SO 로 resolve. Awake 단계 1회 호출.
        // 5023 cascade 분리 예정 (MetaProgressionLoader 흡수).
        private void ResolveSelectedCharacterFromSave()
        {
            if (_characterPool == null)
            {
                Debug.LogWarning("[MetaProgression] _characterPool 미wiring — Inspector 확인");
                return;
            }
            SelectedCharacter = _characterPool.FindById(Data?.selectedCharacterId);
        }

        private void BootstrapItemDropService()
        {
            if (ItemDropService.IsInitialized) return;
#if UNITY_EDITOR
            // Editor 모드 — AssetDatabase 자동 검색.
            // 빌드용 Inspector SerializeField 폴백은 후속 (드랍 풀 SO 인덱스 신규).
            var allGuids = UnityEditor.AssetDatabase.FindAssets("t:EquipmentTemplate", new[] { "Assets/_Project/Data/Templates" });
            var templates = new System.Collections.Generic.List<EquipmentTemplate>();
            var allTemplates = new System.Collections.Generic.List<EquipmentTemplate>();  // 옵션 Z — 가챠 무기 포함 전체
            foreach (var g in allGuids)
            {
                var p = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                var t = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentTemplate>(p);
                if (t == null) continue;
                allTemplates.Add(t);
                if (p.Contains("/Weapons/")) continue;  // 가챠 한정 풀 제외 (드랍에서 빠짐)
                templates.Add(t);
            }
            // 옵션 Z 사이클 1 — EquipmentManager 가 가챠 무기 포함 전체 lookup 가능
            EquipmentManager.RegisterTemplates(allTemplates);
            var slotPoolGuids = UnityEditor.AssetDatabase.FindAssets("t:SlotAffixPool", new[] { "Assets/_Project/Data/SlotPools" });
            var slotPools = new System.Collections.Generic.List<SlotAffixPool>();
            foreach (var g in slotPoolGuids)
                slotPools.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<SlotAffixPool>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)));
            var globalPool = UnityEditor.AssetDatabase.LoadAssetAtPath<GlobalSubAffixPool>("Assets/_Project/Data/GlobalPools/GlobalSubAffixPool_Default.asset");
            var balance = UnityEditor.AssetDatabase.LoadAssetAtPath<BalanceConfig>("Assets/_Project/Data/BalanceConfig.asset");

            if (templates.Count == 0 || slotPools.Count == 0 || globalPool == null)
            {
                Debug.LogWarning("[MetaProgression] ItemDrop assets 부족 — 부트스트랩 스킵");
                return;
            }
            ItemDropService.Initialize(slotPools.ToArray(), globalPool, templates.ToArray(), balance);
            Debug.Log($"[MetaProgression] ItemDrop bootstrap: templates={templates.Count} slotPools={slotPools.Count}");
#else
            Debug.LogWarning("[MetaProgression] ItemDrop bootstrap — 빌드 모드: Inspector SerializeField 후속 필요");
#endif
        }

        private void OnEnable()
        {
            GameEvents.OnChapterCleared    += HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  += HandleMemoryDiscovered;
            GameEvents.OnEnemyKilled       += HandleEnemyKilled;
            GameEvents.OnPlayerDied        += FlushDrops;
        }

        private void OnDisable()
        {
            GameEvents.OnChapterCleared    -= HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  -= HandleMemoryDiscovered;
            GameEvents.OnEnemyKilled       -= HandleEnemyKilled;
            GameEvents.OnPlayerDied        -= FlushDrops;
        }

        // PIVOT v3 Phase 2 — drop batching (game_pivot_v2.md GC critical 해결)
        // 적 사망 시 SaveData.equipmentInstances 직접 append + Save 호출 폐기 →
        // _pendingDrops List 에 메모리만 append, Chapter 클리어/사망 시점 1회 일괄 Save.
        private readonly System.Collections.Generic.List<EquipmentInstance> _pendingDrops
            = new System.Collections.Generic.List<EquipmentInstance>(512);

        public void QueueDrop(EquipmentInstance instance)
        {
            if (instance == null) return;
            _pendingDrops.Add(instance);
        }

        public void FlushDrops()
        {
            if (_pendingDrops.Count == 0) return;
            var existing = Data.equipmentInstances ?? new EquipmentInstance[0];
            var arr = new EquipmentInstance[existing.Length + _pendingDrops.Count];
            for (int i = 0; i < existing.Length; i++) arr[i] = existing[i];
            for (int i = 0; i < _pendingDrops.Count; i++) arr[existing.Length + i] = _pendingDrops[i];
            Data.equipmentInstances = arr;
            int count = _pendingDrops.Count;
            _pendingDrops.Clear();
            SaveSystem.Save(Data);
            Debug.Log($"[MetaProgression] FlushDrops: {count} 장비 일괄 저장");
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
            FlushDrops();  // PIVOT v3 — 챕터 클리어 시점 1회 Save (drop batching)
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
            // weapon_v2 — 직업 변경 감지 (이전 → 신규)
            var prevClass = ResolveCharacterClass(SelectedCharacter);
            var newClass = ResolveCharacterClass(character);

            SelectedCharacter = character;
            Data.selectedCharacterId = character != null ? character.id : null;
            SaveSystem.Save(Data);
            OnCharacterChanged?.Invoke(character);
            if (character != null) LimitedShopService.HandleCharacterFirstSelect(character.id);  // 핫딜 트리거

            // 직업 enum 변경 시만 발행 (같은 직업 내 캐릭터 교체 시 X)
            if (prevClass != newClass) OnCharacterClassChanged?.Invoke(newClass);
        }

        private static CharacterClass ResolveCharacterClass(CharacterData ch)
        {
            if (ch == null || string.IsNullOrEmpty(ch.className)) return CharacterClass.Warrior;
            switch (ch.className)
            {
                case "전사": return CharacterClass.Warrior;
                case "궁수": return CharacterClass.Archer;
                case "마법사": return CharacterClass.Mage;
                default: return CharacterClass.Warrior;
            }
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
