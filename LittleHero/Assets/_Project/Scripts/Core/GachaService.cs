using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT Phase C3 — 무기 가챠 정적 서비스 (gacha_v1.md §1).
    ///
    /// 단판/10연차 굴림 + 등급 굴림 + Rare+ 보장 + 누적 카운트 + 소환 레벨 갱신 + 마법의돌 조각 부수 획득.
    /// 무기 슬롯은 EquipSlot.Weapon 고정 — drop 풀과 분리.
    ///
    /// 비용:
    ///   단판: 별빛 ⋆ <see cref="GachaPool.starlightCostSingle"/> 또는 무기 소환권 1장
    ///   10연차: 별빛 ⋆ <see cref="GachaPool.starlightCost10Pull"/> 또는 무기 소환권 10장
    ///
    /// 부트스트랩: <see cref="Initialize"/> 1회 호출. ItemDropService 와 SlotPool 공유.
    /// </summary>
    public static class GachaService
    {
        private static SlotAffixPool[] _slotPools;
        private static GlobalSubAffixPool _globalPool;
        private static readonly System.Random _defaultRng = new System.Random();
        private static bool _initialized;

        public static bool IsInitialized => _initialized;

        public static void Initialize(SlotAffixPool[] slotPools, GlobalSubAffixPool globalPool)
        {
            _slotPools = slotPools;
            _globalPool = globalPool;
            _initialized = true;
        }

        public static void Reset()
        {
            _slotPools = null;
            _globalPool = null;
            _initialized = false;
        }

        // ===== 비용·가용성 =====

        public static bool CanRollSingle(GachaPool pool, GachaCurrency currency)
        {
            if (pool == null) return false;
            return currency switch
            {
                GachaCurrency.Starlight    => CurrencyService.Starlight >= pool.starlightCostSingle,
                GachaCurrency.SummonTicket => GetTickets() >= 1,
                _ => false,
            };
        }

        public static bool CanRoll10x(GachaPool pool, GachaCurrency currency)
        {
            if (pool == null) return false;
            return currency switch
            {
                GachaCurrency.Starlight    => CurrencyService.Starlight >= pool.starlightCost10Pull,
                GachaCurrency.SummonTicket => GetTickets() >= 10,
                _ => false,
            };
        }

        // ===== 소환 레벨 =====

        /// <summary>현재 누적 카운트로 도달한 소환 레벨(1~MAX) 계산.</summary>
        public static int CalcSummonLevel(GachaPool pool, int totalCount)
        {
            if (pool == null || pool.summonLevelThresholds == null || pool.summonLevelThresholds.Length == 0)
                return 1;
            int level = 1;
            for (int i = 0; i < pool.summonLevelThresholds.Length; i++)
            {
                if (totalCount >= pool.summonLevelThresholds[i])
                    level = i + 1;
                else break;
            }
            return Mathf.Clamp(level, 1, GachaPool.MAX_SUMMON_LEVEL);
        }

        public static SummonLevelProbability GetProbability(GachaPool pool, int summonLevel)
        {
            if (pool == null || pool.probabilities == null || pool.probabilities.Length == 0)
                return null;
            int idx = Mathf.Clamp(summonLevel, 1, pool.probabilities.Length) - 1;
            return pool.probabilities[idx];
        }

        public static int GetNextLevelThreshold(GachaPool pool, int currentLevel)
        {
            if (pool == null || pool.summonLevelThresholds == null) return -1;
            if (currentLevel >= pool.summonLevelThresholds.Length) return -1;
            return pool.summonLevelThresholds[currentLevel];  // currentLevel = 1 이면 [1]번 (= 다음 레벨 임계값)
        }

        // ===== 굴림 =====

        public static GachaResult RollSingle(GachaPool pool, GachaCurrency currency, System.Random rng = null)
        {
            if (!_initialized || pool == null) return null;
            if (!SpendCost(pool, currency, isMultiple: false)) return null;
            rng = rng ?? _defaultRng;

            var result = RollOne(pool, rng, forcedRarity: null);
            ApplyResult(pool, new[] { result });
            return result;
        }

        public static GachaResult[] Roll10x(GachaPool pool, GachaCurrency currency, System.Random rng = null)
        {
            if (!_initialized || pool == null) return null;
            if (!SpendCost(pool, currency, isMultiple: true)) return null;
            rng = rng ?? _defaultRng;

            var results = new GachaResult[10];
            bool hasRarePlus = false;
            for (int i = 0; i < 10; i++)
            {
                results[i] = RollOne(pool, rng, forcedRarity: null);
                if (results[i].rarity >= Rarity.Rare) hasRarePlus = true;
            }

            // Rare+ 보장: 10회 모두 Common/Magic 이면 마지막 1개 강제 Rare
            if (!hasRarePlus)
            {
                results[9] = RollOne(pool, rng, forcedRarity: Rarity.Rare);
                results[9].isRarePlusGuaranteed = true;
            }

            ApplyResult(pool, results);
            return results;
        }

        // ===== 한정 상점 grant 헬퍼 (limited_shop_v1.md §6.2) =====

        /// <summary>
        /// 한정 상점에서 특정 templateId 무기 1개 확정 grant. AffixRoller 로 6줄 옵션 굴림.
        /// 본 함수는 비용 차감 X — 호출자(LimitedShopService)가 별빛/IAP 차감 후 호출.
        /// </summary>
        public static GachaResult GrantSpecificWeapon(GachaPool pool, string templateId, System.Random rng = null)
        {
            if (!_initialized || pool == null || string.IsNullOrEmpty(templateId)) return null;
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
                Debug.LogWarning("[GachaService] GrantSpecificWeapon: templateId '" + templateId + "' 미존재");
                return null;
            }
            return BuildAndStoreInstance(pool, template, rng, isForcedRarity: false);
        }

        /// <summary>
        /// 한정 상점에서 등급 풀 1개 확정 grant (예: Rare 풀 무작위 1개).
        /// </summary>
        public static GachaResult GrantWeaponByRarity(GachaPool pool, Rarity rarity, System.Random rng = null)
        {
            if (!_initialized || pool == null) return null;
            rng = rng ?? _defaultRng;
            var template = PickFromRarityPool(pool, rarity, rng);
            if (template == null)
            {
                Debug.LogWarning("[GachaService] GrantWeaponByRarity: " + rarity + " 풀 비어있음");
                return null;
            }
            return BuildAndStoreInstance(pool, template, rng, isForcedRarity: true);
        }

        private static GachaResult BuildAndStoreInstance(GachaPool pool, EquipmentTemplate template, System.Random rng, bool isForcedRarity)
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
            var slotPool = FindSlotPool(template.slot);
            if (slotPool != null)
                for (int i = 0; i < mainCount; i++)
                    instance.mainAffixes[i] = AffixRoller.RollMain(slotPool, template.rarity, rng);
            if (_globalPool != null)
                for (int i = 0; i < subCount; i++)
                    instance.subAffixes[i] = AffixRoller.RollSub(_globalPool, template.rarity, rng);

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

        // ===== 내부 =====

        private static GachaResult RollOne(GachaPool pool, System.Random rng, Rarity? forcedRarity)
        {
            var meta = MetaProgression.Instance;
            int summonLevel = meta != null ? Mathf.Max(1, meta.Data.summonLevel) : 1;
            Rarity rarity = forcedRarity ?? RollRarity(pool, summonLevel, rng);

            var template = PickFromRarityPool(pool, rarity, rng);
            // 풀이 비어있으면 한 단계 낮은 등급으로 폴백
            int fallback = (int)rarity;
            while (template == null && fallback > 0)
            {
                fallback--;
                template = PickFromRarityPool(pool, (Rarity)fallback, rng);
            }
            if (template == null)
            {
                Debug.LogWarning("[GachaService] 모든 등급 풀이 비어있음 — null 반환");
                return new GachaResult
                {
                    instance = null,
                    rarity = Rarity.Common,
                    templateId = "",
                    starforceShards = 0,
                };
            }
            // 등급은 실제 풀에서 뽑힌 템플릿의 등급으로 보정 (풀 폴백 시)
            rarity = template.rarity;

            int mainCount = Mathf.Max(0, template.mainAffixLineCount);
            int subCount = Mathf.Max(0, template.subAffixLineCount);
            var instance = new EquipmentInstance
            {
                instanceId = System.Guid.NewGuid().ToString(),
                templateId = template.id,
                enhanceLevel = 0,
                failCount = 0,
                mainAffixes = new AffixLine[mainCount],
                subAffixes = new AffixLine[subCount],
                mythicId = "",
                isLocked = false,
                isEquipped = false,
                history = new InstanceHistory(),
            };

            var slotPool = FindSlotPool(template.slot);
            if (slotPool != null)
            {
                for (int i = 0; i < mainCount; i++)
                    instance.mainAffixes[i] = AffixRoller.RollMain(slotPool, rarity, rng);
            }
            if (_globalPool != null)
            {
                for (int i = 0; i < subCount; i++)
                    instance.subAffixes[i] = AffixRoller.RollSub(_globalPool, rarity, rng);
            }

            int shards = GetShardsForRarity(pool, rarity);

            return new GachaResult
            {
                instance = instance,
                rarity = rarity,
                templateId = template.id,
                starforceShards = shards,
                isRarePlusGuaranteed = false,
                causedSummonLevelUp = false,
                newSummonLevel = 0,
            };
        }

        private static Rarity RollRarity(GachaPool pool, int summonLevel, System.Random rng)
        {
            var prob = GetProbability(pool, summonLevel);
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
            // weapon_v2 / gacha_class_filter_v1 — 본인 직업 무기만 추출
            var playerClass = EquipmentManager.ResolvePlayerClass();
            var arr = GetPoolForRarity(pool, rarity);
            if (arr == null || arr.Length == 0) return null;
            var live = new List<EquipmentTemplate>(arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                var t = arr[i];
                if (t == null) continue;
                // None 카테고리 (비-무기) 또는 본인 직업 매칭만 추출
                if (t.category == WeaponCategory.None || t.requiredClass == playerClass)
                    live.Add(t);
            }
            if (live.Count == 0) return null;
            return live[rng.Next(live.Count)];
        }

        private static EquipmentTemplate[] GetPoolForRarity(GachaPool pool, Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common    => pool.commonPool,
                Rarity.Magic     => pool.magicPool,
                Rarity.Rare      => pool.rarePool,
                Rarity.Epic      => pool.epicPool,
                Rarity.Legendary => pool.legendaryPool,
                Rarity.Mythic    => pool.mythicPool,
                _ => null,
            };
        }

        private static int GetShardsForRarity(GachaPool pool, Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common    => pool.shardsCommon,
                Rarity.Magic     => pool.shardsMagic,
                Rarity.Rare      => pool.shardsRare,
                Rarity.Epic      => pool.shardsEpic,
                Rarity.Legendary => pool.shardsLegendary,
                Rarity.Mythic    => pool.shardsMythic,
                _ => 0,
            };
        }

        private static SlotAffixPool FindSlotPool(EquipSlot slot)
        {
            if (_slotPools == null) return null;
            for (int i = 0; i < _slotPools.Length; i++)
                if (_slotPools[i] != null && _slotPools[i].slot == slot)
                    return _slotPools[i];
            return null;
        }

        private static bool SpendCost(GachaPool pool, GachaCurrency currency, bool isMultiple)
        {
            int starlightCost = isMultiple ? pool.starlightCost10Pull : pool.starlightCostSingle;
            int ticketCost    = isMultiple ? 10 : 1;

            return currency switch
            {
                GachaCurrency.Starlight    => CurrencyService.SpendStarlight(starlightCost),
                GachaCurrency.SummonTicket => SpendTickets(ticketCost),
                _ => false,
            };
        }

        private static void ApplyResult(GachaPool pool, GachaResult[] results)
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
                    AppendInstance(meta.Data, r.instance);
                    GameEvents.RaiseGachaResult(r.instance, r.rarity);
                }
                totalShards += r.starforceShards;
            }

            int newLevel = CalcSummonLevel(pool, meta.Data.totalGachaCount);
            if (newLevel != prevLevel)
            {
                meta.Data.summonLevel = newLevel;
                // 마지막 결과에 레벨업 표시
                for (int i = results.Length - 1; i >= 0; i--)
                    if (results[i] != null) { results[i].causedSummonLevelUp = true; results[i].newSummonLevel = newLevel; break; }
                GameEvents.RaiseSummonLevelUp(newLevel);
                LimitedShopService.HandleSummonLevelUp(newLevel);  // 핫딜 트리거
            }

            if (totalShards > 0)
            {
                meta.Data.starforceShards += totalShards;
                GameEvents.RaiseStarforceShardsChanged(meta.Data.starforceShards);
            }

            SaveSystem.Save(meta.Data);
        }

        private static void AppendInstance(SaveData data, EquipmentInstance instance)
        {
            int len = data.equipmentInstances?.Length ?? 0;
            var newArr = new EquipmentInstance[len + 1];
            for (int i = 0; i < len; i++) newArr[i] = data.equipmentInstances[i];
            newArr[len] = instance;
            data.equipmentInstances = newArr;
        }

        // ===== 무기 소환권 =====

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

    public enum GachaCurrency { Starlight, SummonTicket }

    /// <summary>
    /// 단일 가챠 결과 (gacha_v1.md §1).
    /// 10연차의 경우 results[i].isRarePlusGuaranteed 가 true 면 보장 발동.
    /// </summary>
    public class GachaResult
    {
        public EquipmentInstance instance;
        public Rarity rarity;
        public string templateId;
        public int starforceShards;
        public bool isRarePlusGuaranteed;
        public bool causedSummonLevelUp;
        public int newSummonLevel;
    }
}
