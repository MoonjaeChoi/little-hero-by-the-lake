using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 보스 처치 보상 적용 서비스 (5001 / Sprint 3.2 / T06).
    /// EnemyController.Die 진입부에서 호출 — BossLootTable.rewards → CurrencyService.Grant 매핑.
    /// guaranteedMythicChance >= 1.0 시 신화 보장 발현 (BossEncounterService 위임).
    /// CLAUDE.md §15.7 Lightweight DI — 정적 헬퍼 (Singleton 직접 참조 X).
    /// </summary>
    public static class BossLootGrantService
    {
        public static void Apply(BossLootTable lootTable)
        {
            if (lootTable == null) return;

            // 1) RewardEntry → CurrencyService.Grant
            if (lootTable.rewards != null)
            {
                foreach (var entry in lootTable.rewards)
                {
                    if (entry == null || entry.amount <= 0) continue;
                    GrantCurrency(entry.currency, entry.amount);
                }
            }

            // 2) 신화 보장 (Weekly = 1.0, BossEncounterService 위임)
            if (lootTable.guaranteedMythicChance >= 1.0f)
            {
                Debug.Log($"[BossLootGrantService] Mythic guaranteed (tier={lootTable.tier}, chance={lootTable.guaranteedMythicChance:F2}) — BossEncounterService.GuaranteeMythicForEquipped 호출 권장");
                // 실제 호출은 BossEncounterService 가 BossEncounterRunner.Begin 흐름에서 처리.
                // 여기서는 로그만 남김 (회귀 방지 — 기존 흐름 유지).
            }

            Debug.Log($"[BossLootGrantService] Applied {lootTable.tier} ({lootTable.rewards?.Length ?? 0} rewards)");
        }

        private static void GrantCurrency(CurrencyType currency, int amount)
        {
            switch (currency)
            {
                case CurrencyType.StarDust:     CurrencyService.GrantStarDust(amount); break;
                case CurrencyType.RainbowOrb:   CurrencyService.GrantStarFragment(amount); break;
                case CurrencyType.MagicCrystal: CurrencyService.GrantMagicCrystal(amount); break;
                case CurrencyType.Starlight:    CurrencyService.GrantStarlight(amount); break;
                case CurrencyType.Xp:           // XP 는 PlayerProgress 가 처치 시 자동 처리
                default: break;
            }
        }
    }
}
