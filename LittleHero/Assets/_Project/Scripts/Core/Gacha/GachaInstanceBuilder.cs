using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 가챠 결과 → EquipmentInstance 빌드 + SaveData 저장.
    /// 5027.1 — RollService 318L 200L 초과 해소를 위해 helper 분리.
    /// </summary>
    public static class GachaInstanceBuilder
    {
        public static GachaResult BuildAndStore(
            GachaPool pool, EquipmentTemplate template, SlotAffixPool[] slotPools, GlobalSubAffixPool globalPool,
            System.Random rng, bool isForcedRarity)
        {
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
            var slotPool = FindSlotPool(slotPools, template.slot);
            if (slotPool != null)
                for (int i = 0; i < mainCount; i++)
                    instance.mainAffixes[i] = AffixRoller.RollMain(slotPool, template.rarity, rng);
            if (globalPool != null)
                for (int i = 0; i < subCount; i++)
                    instance.subAffixes[i] = AffixRoller.RollSub(globalPool, template.rarity, rng);

            int shards = GetShardsForRarity(pool, template.rarity);
            var meta = MetaProgression.Instance;
            if (meta != null && meta.Data != null)
            {
                AppendInstance(meta.Data, instance);
                if (shards > 0)
                {
                    meta.Data.starforceShards += shards;
                    GameEvents.RaiseStarforceShardsChanged(meta.Data.starforceShards);
                }
                SaveSystem.Save(meta.Data);
            }
            var result = new GachaResult
            {
                instance = instance, rarity = template.rarity, templateId = template.id,
                starforceShards = shards, isRarePlusGuaranteed = isForcedRarity,
                causedSummonLevelUp = false, newSummonLevel = 0,
            };
            GameEvents.RaiseGachaResult(instance, template.rarity);
            return result;
        }

        public static SlotAffixPool FindSlotPool(SlotAffixPool[] pools, EquipSlot slot)
        {
            if (pools == null) return null;
            for (int i = 0; i < pools.Length; i++)
                if (pools[i] != null && pools[i].slot == slot) return pools[i];
            return null;
        }

        public static int GetShardsForRarity(GachaPool pool, Rarity rarity) => rarity switch
        {
            Rarity.Common    => pool.shardsCommon,
            Rarity.Magic     => pool.shardsMagic,
            Rarity.Rare      => pool.shardsRare,
            Rarity.Epic      => pool.shardsEpic,
            Rarity.Legendary => pool.shardsLegendary,
            Rarity.Mythic    => pool.shardsMythic,
            _ => 0,
        };

        public static void AppendInstance(SaveData data, EquipmentInstance instance)
        {
            int len = data.equipmentInstances?.Length ?? 0;
            var newArr = new EquipmentInstance[len + 1];
            for (int i = 0; i < len; i++) newArr[i] = data.equipmentInstances[i];
            newArr[len] = instance;
            data.equipmentInstances = newArr;
        }
    }
}
