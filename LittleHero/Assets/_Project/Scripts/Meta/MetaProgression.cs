using System;
using UnityEngine;

namespace LittleHero
{
    public class MetaProgression : MonoBehaviour
    {
        public static MetaProgression Instance { get; private set; }

        public SaveData Data { get; private set; }
        public int StarFragments => Data?.starFragments ?? 0;
        public ChapterData SelectedChapter { get; set; }
        public CharacterData SelectedCharacter { get; private set; }

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
            GameEvents.OnChapterCleared += HandleChapterCleared;
        }

        private void OnDisable()
        {
            GameEvents.OnChapterCleared -= HandleChapterCleared;
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
