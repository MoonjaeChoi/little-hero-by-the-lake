using System;

namespace LittleHero
{
    public static class GameEvents
    {
        public static event Action<EnemyData> OnEnemyKilled;
        public static event Action<int> OnLevelUp;
        public static event Action<int> OnWaveCleared;
        public static event Action<SlotType, EquipmentData> OnEquipmentChanged;
        public static event Action OnShopOpened;
        public static event Action OnShopClosed;
        public static event Action<int> OnStarDustChanged;
        public static event Action<int> OnPlayerHealthChanged;
        public static event Action<MemoryData> OnMemoryDiscovered;
        public static event Action<ChapterData> OnChapterCleared;

        public static void RaiseEnemyKilled(EnemyData data) => OnEnemyKilled?.Invoke(data);
        public static void RaiseLevelUp(int level) => OnLevelUp?.Invoke(level);
        public static void RaiseWaveCleared(int wave) => OnWaveCleared?.Invoke(wave);
        public static void RaiseEquipmentChanged(SlotType slot, EquipmentData data) => OnEquipmentChanged?.Invoke(slot, data);
        public static void RaiseShopOpened() => OnShopOpened?.Invoke();
        public static void RaiseShopClosed() => OnShopClosed?.Invoke();
        public static void RaiseStarDustChanged(int amount) => OnStarDustChanged?.Invoke(amount);
        public static void RaisePlayerHealthChanged(int health) => OnPlayerHealthChanged?.Invoke(health);
        public static void RaiseMemoryDiscovered(MemoryData data) => OnMemoryDiscovered?.Invoke(data);
        public static void RaiseChapterCleared(ChapterData data) => OnChapterCleared?.Invoke(data);
    }
}
