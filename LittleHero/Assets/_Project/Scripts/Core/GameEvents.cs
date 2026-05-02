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

        // ✦ C1 — 굴림/리롤 결과 (AffixRoller 가 발행)
        public static event Action<EquipmentInstance> OnAffixRolled;

        // ✦ 보스 체력바 — WaveManager / BossEncounterRunner 가 발행
        public static event Action<EnemyController> OnBossSpawned;
        public static event Action OnBossDespawned;

        // ✦ PIVOT Phase C3 — 가챠·스타포스 (gacha_v1.md)
        public static event Action<EquipmentInstance, Rarity> OnGachaResult;  // 단일 결과
        public static event Action<int> OnSummonLevelUp;                       // 소환 레벨 N 도달
        public static event Action<int> OnStarforceShardsChanged;              // 조각 잔량
        public static event Action<int> OnWeaponSummonTicketsChanged;          // 소환권 잔량
        public static event Action<int /*slotIdx*/, int /*newLevel*/> OnSlotStarforceChanged;

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
        public static void RaiseAffixRolled(EquipmentInstance instance) => OnAffixRolled?.Invoke(instance);
        public static void RaiseBossSpawned(EnemyController boss) => OnBossSpawned?.Invoke(boss);
        public static void RaiseBossDespawned() => OnBossDespawned?.Invoke();
        public static void RaiseGachaResult(EquipmentInstance instance, Rarity rarity) => OnGachaResult?.Invoke(instance, rarity);
        public static void RaiseSummonLevelUp(int level) => OnSummonLevelUp?.Invoke(level);
        public static void RaiseStarforceShardsChanged(int amount) => OnStarforceShardsChanged?.Invoke(amount);
        public static void RaiseWeaponSummonTicketsChanged(int amount) => OnWeaponSummonTicketsChanged?.Invoke(amount);
        public static void RaiseSlotStarforceChanged(int slotIdx, int newLevel) => OnSlotStarforceChanged?.Invoke(slotIdx, newLevel);
    }
}
