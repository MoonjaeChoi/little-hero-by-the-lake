namespace LittleHero
{
    /// <summary>
    /// 가챠 결과 일괄 적용 — 누적 카운트 + 소환 레벨업 + 마법의돌 조각 + Save.
    /// 5027 — RollService 200L 초과 해소.
    /// </summary>
    public static class GachaResultApplier
    {
        public static void Apply(GachaPool pool, GachaResult[] results)
        {
            var meta = MetaProgression.Instance;
            if (meta == null || meta.Data == null) return;

            int prevLevel = meta.Data.summonLevel;
            int totalShards = 0;
            for (int i = 0; i < results.Length; i++)
            {
                var r = results[i];
                if (r == null) continue;
                meta.Data.totalGachaCount += 1;
                if (r.instance != null)
                {
                    GachaInstanceBuilder.AppendInstance(meta.Data, r.instance);
                    GameEvents.RaiseGachaResult(r.instance, r.rarity);
                }
                totalShards += r.starforceShards;
            }

            int newLevel = GachaProgressService.CalcSummonLevel(pool, meta.Data.totalGachaCount);
            if (newLevel != prevLevel)
            {
                meta.Data.summonLevel = newLevel;
                for (int i = results.Length - 1; i >= 0; i--)
                    if (results[i] != null) { results[i].causedSummonLevelUp = true; results[i].newSummonLevel = newLevel; break; }
                GameEvents.RaiseSummonLevelUp(newLevel);
                LimitedShopService.HandleSummonLevelUp(newLevel);
            }
            if (totalShards > 0)
            {
                meta.Data.starforceShards += totalShards;
                GameEvents.RaiseStarforceShardsChanged(meta.Data.starforceShards);
            }
            SaveSystem.Save(meta.Data);
        }
    }
}
