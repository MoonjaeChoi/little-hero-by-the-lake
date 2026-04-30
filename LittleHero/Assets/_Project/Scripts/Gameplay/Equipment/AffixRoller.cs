using System;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// C1 — 주옵션·부옵션 굴림·리롤 정적 서비스
    /// (affix_pool_v1 §4 / §7.3 + item_drop_v1 §5.4 / §6).
    ///
    /// 주옵션 = 슬롯 풀(SlotAffixPool) 가중 6:3:1.
    /// 부옵션 = 글로벌 풀(GlobalSubAffixPool) SubAffixDefinition.globalWeight 합산.
    /// 굴림 결과 수치는 등급별 [min, max] 균등 추출.
    /// 리롤은 잠금 안 한 라인만 갱신. 비용은 <see cref="BalanceConfig"/> 의
    /// rerollCostMain / rerollCostSub × (1 + lockedCount × lockMultiplier).
    /// </summary>
    public static class AffixRoller
    {
        // ===== 주옵션 (슬롯 풀) =====

        public static AffixLine RollMain(SlotAffixPool pool, Rarity rarity, System.Random rng)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (rng == null) rng = new System.Random();

            float roll = (float)rng.NextDouble();
            float w1 = pool.weightTier1;
            float w2 = pool.weightTier2;
            AffixDefinition picked =
                roll < w1 ? pool.tier1 :
                roll < w1 + w2 ? pool.tier2 :
                pool.tier3;

            return Build(picked, rarity, rng);
        }

        public static AffixLine[] RollMainSet(SlotAffixPool pool, Rarity rarity, int count, System.Random rng)
        {
            var lines = new AffixLine[count];
            for (int i = 0; i < count; i++)
                lines[i] = RollMain(pool, rarity, rng);
            return lines;
        }

        // ===== 부옵션 (글로벌 풀) =====

        public static AffixLine RollSub(GlobalSubAffixPool pool, Rarity rarity, System.Random rng)
        {
            if (pool == null || pool.entries == null || pool.entries.Length == 0)
                throw new ArgumentException("GlobalSubAffixPool 에 entries 가 비어있습니다.", nameof(pool));
            if (rng == null) rng = new System.Random();

            float total = 0f;
            for (int i = 0; i < pool.entries.Length; i++)
                total += Mathf.Max(0f, pool.entries[i].globalWeight);
            if (total <= 0f)
                throw new InvalidOperationException("GlobalSubAffixPool 의 globalWeight 합이 0 이하입니다.");

            float roll = (float)rng.NextDouble() * total;
            float acc = 0f;
            SubAffixDefinition chosen = pool.entries[pool.entries.Length - 1];
            for (int i = 0; i < pool.entries.Length; i++)
            {
                var def = pool.entries[i];
                acc += Mathf.Max(0f, def.globalWeight);
                if (roll < acc) { chosen = def; break; }
            }

            return BuildSub(chosen, rarity, rng);
        }

        public static AffixLine[] RollSubSet(GlobalSubAffixPool pool, Rarity rarity, int count, System.Random rng)
        {
            var lines = new AffixLine[count];
            for (int i = 0; i < count; i++)
                lines[i] = RollSub(pool, rarity, rng);
            return lines;
        }

        // ===== 리롤 =====

        public static int CalcRerollCostMain(EquipmentInstance instance, BalanceConfig config)
        {
            int locked = CountLocked(instance?.mainAffixes);
            return Mathf.RoundToInt(config.rerollCostMain * (1f + locked * config.lockMultiplier));
        }

        public static int CalcRerollCostSub(EquipmentInstance instance, BalanceConfig config)
        {
            int locked = CountLocked(instance?.subAffixes);
            return Mathf.RoundToInt(config.rerollCostSub * (1f + locked * config.lockMultiplier));
        }

        public static bool CanRerollMain(EquipmentInstance instance)
            => instance?.mainAffixes != null && CountLocked(instance.mainAffixes) < instance.mainAffixes.Length;

        public static bool CanRerollSub(EquipmentInstance instance)
            => instance?.subAffixes != null && CountLocked(instance.subAffixes) < instance.subAffixes.Length;

        public static void RerollMain(EquipmentInstance instance, SlotAffixPool pool, Rarity rarity, System.Random rng)
        {
            if (instance?.mainAffixes == null) return;
            for (int i = 0; i < instance.mainAffixes.Length; i++)
            {
                var line = instance.mainAffixes[i];
                if (line != null && line.locked) continue;
                instance.mainAffixes[i] = RollMain(pool, rarity, rng);
            }
            GameEvents.RaiseAffixRolled(instance);
        }

        public static void RerollSub(EquipmentInstance instance, GlobalSubAffixPool pool, Rarity rarity, System.Random rng)
        {
            if (instance?.subAffixes == null) return;
            for (int i = 0; i < instance.subAffixes.Length; i++)
            {
                var line = instance.subAffixes[i];
                if (line != null && line.locked) continue;
                instance.subAffixes[i] = RollSub(pool, rarity, rng);
            }
            GameEvents.RaiseAffixRolled(instance);
        }

        // ===== 내부 =====

        private static AffixLine Build(AffixDefinition def, Rarity rarity, System.Random rng)
        {
            int r = (int)rarity;
            float min = def.minByRarity != null && r < def.minByRarity.Length ? def.minByRarity[r] : 0f;
            float max = def.maxByRarity != null && r < def.maxByRarity.Length ? def.maxByRarity[r] : min;
            float t = (float)rng.NextDouble();
            float value = Mathf.Lerp(min, max, t);

            return new AffixLine
            {
                affixId = def.id,
                targetName = def.target.ToString(),
                opName = def.op.ToString(),
                value = value,
                locked = false,
            };
        }

        private static AffixLine BuildSub(SubAffixDefinition def, Rarity rarity, System.Random rng)
        {
            int r = (int)rarity;
            float min = def.minByRarity != null && r < def.minByRarity.Length ? def.minByRarity[r] : 0f;
            float max = def.maxByRarity != null && r < def.maxByRarity.Length ? def.maxByRarity[r] : min;
            float t = (float)rng.NextDouble();
            float value = Mathf.Lerp(min, max, t);

            return new AffixLine
            {
                affixId = def.id,
                targetName = def.target.ToString(),
                opName = def.op.ToString(),
                value = value,
                locked = false,
            };
        }

        private static int CountLocked(AffixLine[] lines)
        {
            if (lines == null) return 0;
            int n = 0;
            for (int i = 0; i < lines.Length; i++)
                if (lines[i] != null && lines[i].locked) n++;
            return n;
        }
    }
}
