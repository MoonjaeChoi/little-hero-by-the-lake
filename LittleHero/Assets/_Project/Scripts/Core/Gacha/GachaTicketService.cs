namespace LittleHero
{
    /// <summary>
    /// 무기 소환권 카운트 관리 — MetaProgression.weaponSummonTickets.
    /// 5027 — GachaService 분리 (gacha_v1.md §1).
    /// </summary>
    public static class GachaTicketService
    {
        public static int GetTickets()
        {
            var data = MetaProgression.Instance?.Data;
            return data != null ? data.weaponSummonTickets : 0;
        }

        public static void GrantTickets(int amount)
        {
            if (amount <= 0) return;
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            data.weaponSummonTickets += amount;
            GameEvents.RaiseWeaponSummonTicketsChanged(data.weaponSummonTickets);
            SaveSystem.Save(data);
        }

        public static bool SpendTickets(int amount)
        {
            if (amount <= 0) return false;
            var data = MetaProgression.Instance?.Data;
            if (data == null || data.weaponSummonTickets < amount) return false;
            data.weaponSummonTickets -= amount;
            GameEvents.RaiseWeaponSummonTicketsChanged(data.weaponSummonTickets);
            SaveSystem.Save(data);
            return true;
        }
    }
}
