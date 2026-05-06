namespace LittleHero
{
    /// <summary>
    /// PIVOT Phase C3 — 무기 가챠 정적 facade (gacha_v1.md §1).
    /// 5027 — 5 service 분리 후 facade. 호출처 시그니처 보존.
    /// 내부 위임:
    ///   Initialize/Reset → GachaRollService
    ///   CanRoll1/CanRoll10 → GachaCanRollService
    ///   CalcSummonLevel/GetProbability/GetNextLevelThreshold → GachaProgressService
    ///   RollSingle/Roll10x/Grant* → GachaRollService
    ///   Get/Grant/SpendTickets → GachaTicketService
    /// </summary>
    public static class GachaService
    {
        private static bool _initialized;

        public static bool IsInitialized => _initialized;

        public static void Initialize(SlotAffixPool[] slotPools, GlobalSubAffixPool globalPool)
        {
            GachaRollService.Initialize(slotPools, globalPool);
            _initialized = true;
        }

        public static void Reset()
        {
            GachaRollService.Reset();
            _initialized = false;
        }

        // ===== CanRoll =====
        public static bool CanRollSingle(GachaPool pool, GachaCurrency currency) => GachaCanRollService.CanRollSingle(pool, currency);
        public static bool CanRoll10x(GachaPool pool, GachaCurrency currency)    => GachaCanRollService.CanRoll10x(pool, currency);

        // ===== Progress =====
        public static int CalcSummonLevel(GachaPool pool, int totalCount)         => GachaProgressService.CalcSummonLevel(pool, totalCount);
        public static SummonLevelProbability GetProbability(GachaPool pool, int summonLevel) => GachaProgressService.GetProbability(pool, summonLevel);
        public static int GetNextLevelThreshold(GachaPool pool, int currentLevel) => GachaProgressService.GetNextLevelThreshold(pool, currentLevel);

        // ===== Roll =====
        public static GachaResult RollSingle(GachaPool pool, GachaCurrency currency, System.Random rng = null)
        {
            if (!_initialized) return null;
            return GachaRollService.RollSingle(pool, currency, rng);
        }
        public static GachaResult[] Roll10x(GachaPool pool, GachaCurrency currency, System.Random rng = null)
        {
            if (!_initialized) return null;
            return GachaRollService.Roll10x(pool, currency, rng);
        }
        public static GachaResult GrantSpecificWeapon(GachaPool pool, string templateId, System.Random rng = null)
        {
            if (!_initialized) return null;
            return GachaRollService.GrantSpecificWeapon(pool, templateId, rng);
        }
        public static GachaResult GrantWeaponByRarity(GachaPool pool, Rarity rarity, System.Random rng = null)
        {
            if (!_initialized) return null;
            return GachaRollService.GrantWeaponByRarity(pool, rarity, rng);
        }

        // ===== Tickets =====
        public static int GetTickets()                  => GachaTicketService.GetTickets();
        public static void GrantTickets(int amount)     => GachaTicketService.GrantTickets(amount);
        public static bool SpendTickets(int amount)     => GachaTicketService.SpendTickets(amount);
    }
}
