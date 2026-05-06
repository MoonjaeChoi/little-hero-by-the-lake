using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 가챠 굴림 핵심 — RollSingle / Roll10x / Grant + 등급 풀.
    /// 5027 — GachaService 분리. 내부 인스턴스 빌드는 GachaInstanceBuilder 위임.
    /// </summary>
    public static class GachaRollService
    {
        private static SlotAffixPool[] _slotPools;
        private static GlobalSubAffixPool _globalPool;
        private static readonly System.Random _defaultRng = new System.Random();

        public static void Initialize(SlotAffixPool[] slotPools, GlobalSubAffixPool globalPool)
        {
            _slotPools = slotPools;
            _globalPool = globalPool;
        }

        public static void Reset()
        {
            _slotPools = null;
            _globalPool = null;
        }

        public static GachaResult RollSingle(GachaPool pool, GachaCurrency currency, System.Random rng = null)
        {
            if (pool == null) return null;
            if (!SpendCost(pool, currency, isMultiple: false)) return null;
            rng = rng ?? _defaultRng;
            var result = RollOne(pool, rng, forcedRarity: null);
            GachaResultApplier.Apply(pool, new[] { result });
            return result;
        }

        public static GachaResult[] Roll10x(GachaPool pool, GachaCurrency currency, System.Random rng = null)
        {
            if (pool == null) return null;
            if (!SpendCost(pool, currency, isMultiple: true)) return null;
            rng = rng ?? _defaultRng;

            var results = new GachaResult[10];
            bool hasRarePlus = false;
            for (int i = 0; i < 10; i++)
            {
                results[i] = RollOne(pool, rng, forcedRarity: null);
                if (results[i].rarity >= Rarity.Rare) hasRarePlus = true;
            }
            if (!hasRarePlus)
            {
                results[9] = RollOne(pool, rng, forcedRarity: Rarity.Rare);
                results[9].isRarePlusGuaranteed = true;
            }
            GachaResultApplier.Apply(pool, results);
            return results;
        }

        public static GachaResult GrantSpecificWeapon(GachaPool pool, string templateId, System.Random rng = null)
        {
            if (pool == null || string.IsNullOrEmpty(templateId)) return null;
            rng = rng ?? _defaultRng;

            EquipmentTemplate template = null;
            var groups = new[] { pool.commonPool, pool.magicPool, pool.rarePool, pool.epicPool, pool.legendaryPool, pool.mythicPool };
            for (int g = 0; g < groups.Length && template == null; g++)
            {
                var arr = groups[g];
                if (arr == null) continue;
                for (int i = 0; i < arr.Length; i++)
                    if (arr[i] != null && arr[i].id == templateId) { template = arr[i]; break; }
            }
            if (template == null)
            {
                Debug.LogWarning("[GachaRollService] GrantSpecificWeapon: '" + templateId + "' 미존재");
                return null;
            }
            return GachaInstanceBuilder.BuildAndStore(pool, template, _slotPools, _globalPool, rng, isForcedRarity: false);
        }

        public static GachaResult GrantWeaponByRarity(GachaPool pool, Rarity rarity, System.Random rng = null)
        {
            if (pool == null) return null;
            rng = rng ?? _defaultRng;
            var template = PickFromRarityPool(pool, rarity, rng);
            if (template == null)
            {
                Debug.LogWarning("[GachaRollService] GrantWeaponByRarity: " + rarity + " 풀 비어있음");
                return null;
            }
            return GachaInstanceBuilder.BuildAndStore(pool, template, _slotPools, _globalPool, rng, isForcedRarity: true);
        }


        private static GachaResult RollOne(GachaPool pool, System.Random rng, Rarity? forcedRarity)
        {
            var meta = MetaProgression.Instance;
            int summonLevel = meta != null ? Mathf.Max(1, meta.Data.summonLevel) : 1;
            Rarity rarity = forcedRarity ?? RollRarity(pool, summonLevel, rng);

            var template = PickFromRarityPool(pool, rarity, rng);
            int fallback = (int)rarity;
            while (template == null && fallback > 0)
            {
                fallback--;
                template = PickFromRarityPool(pool, (Rarity)fallback, rng);
            }
            if (template == null)
            {
                Debug.LogWarning("[GachaRollService] 모든 등급 풀 비어있음");
                return new GachaResult { instance = null, rarity = Rarity.Common, templateId = "", starforceShards = 0 };
            }
            rarity = template.rarity;

            int mainCount = Mathf.Max(0, template.mainAffixLineCount);
            int subCount = Mathf.Max(0, template.subAffixLineCount);
            var instance = new EquipmentInstance
            {
                instanceId = System.Guid.NewGuid().ToString(),
                templateId = template.id,
                enhanceLevel = 0, failCount = 0,
                mainAffixes = new AffixLine[mainCount],
                subAffixes = new AffixLine[subCount],
                mythicId = "", isLocked = false, isEquipped = false,
                history = new InstanceHistory(),
            };
            var slotPool = GachaInstanceBuilder.FindSlotPool(_slotPools, template.slot);
            if (slotPool != null)
                for (int i = 0; i < mainCount; i++)
                    instance.mainAffixes[i] = AffixRoller.RollMain(slotPool, rarity, rng);
            if (_globalPool != null)
                for (int i = 0; i < subCount; i++)
                    instance.subAffixes[i] = AffixRoller.RollSub(_globalPool, rarity, rng);

            int shards = GachaInstanceBuilder.GetShardsForRarity(pool, rarity);
            return new GachaResult
            {
                instance = instance, rarity = rarity, templateId = template.id,
                starforceShards = shards, isRarePlusGuaranteed = false,
                causedSummonLevelUp = false, newSummonLevel = 0,
            };
        }

        private static Rarity RollRarity(GachaPool pool, int summonLevel, System.Random rng)
        {
            var prob = GachaProgressService.GetProbability(pool, summonLevel);
            if (prob == null) return Rarity.Common;

            float r = (float)rng.NextDouble();
            float acc = 0f;
            acc += Mathf.Max(0f, prob.commonRate);    if (r < acc) return Rarity.Common;
            acc += Mathf.Max(0f, prob.magicRate);     if (r < acc) return Rarity.Magic;
            acc += Mathf.Max(0f, prob.rareRate);      if (r < acc) return Rarity.Rare;
            acc += Mathf.Max(0f, prob.epicRate);      if (r < acc) return Rarity.Epic;
            acc += Mathf.Max(0f, prob.legendaryRate); if (r < acc) return Rarity.Legendary;
            return Rarity.Mythic;
        }

        private static EquipmentTemplate PickFromRarityPool(GachaPool pool, Rarity rarity, System.Random rng)
        {
            var playerClass = EquipmentManager.ResolvePlayerClass();
            var arr = GetPoolForRarity(pool, rarity);
            if (arr == null || arr.Length == 0) return null;
            var live = new List<EquipmentTemplate>(arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                var t = arr[i];
                if (t == null) continue;
                if (t.category == WeaponCategory.None || t.requiredClass == playerClass) live.Add(t);
            }
            if (live.Count == 0) return null;
            return live[rng.Next(live.Count)];
        }

        private static EquipmentTemplate[] GetPoolForRarity(GachaPool pool, Rarity rarity) => rarity switch
        {
            Rarity.Common    => pool.commonPool,
            Rarity.Magic     => pool.magicPool,
            Rarity.Rare      => pool.rarePool,
            Rarity.Epic      => pool.epicPool,
            Rarity.Legendary => pool.legendaryPool,
            Rarity.Mythic    => pool.mythicPool,
            _ => null,
        };

        private static bool SpendCost(GachaPool pool, GachaCurrency currency, bool isMultiple)
        {
            int starlightCost = isMultiple ? pool.starlightCost10Pull : pool.starlightCostSingle;
            int ticketCost    = isMultiple ? 10 : 1;
            return currency switch
            {
                GachaCurrency.Starlight    => CurrencyService.SpendStarlight(starlightCost),
                GachaCurrency.SummonTicket => GachaTicketService.SpendTickets(ticketCost),
                _ => false,
            };
        }

    }
}
