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
        public static event Action OnPlayerDied;
        public static event Action<MemoryData> OnMemoryDiscovered;
        public static event Action<ChapterData> OnChapterCleared;

        // ✦ A7 — 신규 화폐 이벤트 (A5 CurrencyService 가 발행)
        public static event Action<int> OnMagicCrystalChanged;   // ◇ 마법 결정
        public static event Action<int> OnStarlightChanged;      // ⋆ 별빛
        // OnAffixRolled 는 B2 EquipmentInstance 도입 후 시그니처 확정.

        public static void RaiseEnemyKilled(EnemyData data) => OnEnemyKilled?.Invoke(data);
        public static void RaiseLevelUp(int level) => OnLevelUp?.Invoke(level);
        public static void RaiseWaveCleared(int wave) => OnWaveCleared?.Invoke(wave);
        public static void RaiseEquipmentChanged(SlotType slot, EquipmentData data) => OnEquipmentChanged?.Invoke(slot, data);
        public static void RaiseShopOpened() => OnShopOpened?.Invoke();
        public static void RaiseShopClosed() => OnShopClosed?.Invoke();
        public static void RaiseStarDustChanged(int amount) => OnStarDustChanged?.Invoke(amount);
        public static void RaisePlayerHealthChanged(int health) => OnPlayerHealthChanged?.Invoke(health);
        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
        public static void RaiseMemoryDiscovered(MemoryData data) => OnMemoryDiscovered?.Invoke(data);
        public static void RaiseChapterCleared(ChapterData data) => OnChapterCleared?.Invoke(data);
        public static void RaiseMagicCrystalChanged(int amount) => OnMagicCrystalChanged?.Invoke(amount);
        public static void RaiseStarlightChanged(int amount) => OnStarlightChanged?.Invoke(amount);
    }
}
