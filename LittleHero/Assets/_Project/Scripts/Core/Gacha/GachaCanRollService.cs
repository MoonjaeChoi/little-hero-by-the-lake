namespace LittleHero
{
    /// <summary>
    /// 가챠 비용·가용성 검증 — Starlight / SummonTicket 분기.
    /// 5027 — GachaService 분리.
    /// </summary>
    public static class GachaCanRollService
    {
        public static bool CanRollSingle(GachaPool pool, GachaCurrency currency)
        {
            if (pool == null) return false;
            return currency switch
            {
                GachaCurrency.Starlight    => CurrencyService.Starlight >= pool.starlightCostSingle,
                GachaCurrency.SummonTicket => GachaTicketService.GetTickets() >= 1,
                _ => false,
            };
        }

        public static bool CanRoll10x(GachaPool pool, GachaCurrency currency)
        {
            if (pool == null) return false;
            return currency switch
            {
                GachaCurrency.Starlight    => CurrencyService.Starlight >= pool.starlightCost10Pull,
                GachaCurrency.SummonTicket => GachaTicketService.GetTickets() >= 10,
                _ => false,
            };
        }
    }
}
