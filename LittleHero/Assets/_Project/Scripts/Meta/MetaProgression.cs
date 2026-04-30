using System;
using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public class MetaProgression : MonoBehaviour
    {
        public static MetaProgression Instance { get; private set; }

        [SerializeField] private MemoryPool _memoryPool;

        public SaveData Data { get; private set; }
        public int StarFragments => Data?.starFragments ?? 0;
        public ChapterData SelectedChapter { get; set; }
        public CharacterData SelectedCharacter { get; private set; }
        public MemoryPool MemoryPool => _memoryPool;
        public int MemoryDiscoveredCount => Data?.discoveredMemoryIds?.Length ?? 0;
        public int MemoryTotalCount => _memoryPool?.memories?.Length ?? 0;

        public event System.Action<CharacterData> OnCharacterChanged;

        public event Action<int> OnStarFragmentsChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Data = SaveSystem.Load();
        }

        private void OnEnable()
        {
            GameEvents.OnChapterCleared    += HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  += HandleMemoryDiscovered;
        }

        private void OnDisable()
        {
            GameEvents.OnChapterCleared    -= HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  -= HandleMemoryDiscovered;
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

        public void SelectCharacter(CharacterData character)
        {
            SelectedCharacter = character;
            Data.selectedCharacterId = character != null ? character.id : null;
            SaveSystem.Save(Data);
            OnCharacterChanged?.Invoke(character);
        }
    }
}
