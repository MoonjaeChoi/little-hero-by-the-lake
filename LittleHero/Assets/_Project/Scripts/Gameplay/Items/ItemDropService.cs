using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public enum DropSource { Enemy, MiniBoss, Boss, BossRepeat, Shop }

    /// <summary>
    /// 드랍 결정에 필요한 컨텍스트 (item_drop_v1 §4 / §5.3).
    /// 5017 T02 — class → struct (heap alloc 폐기, by-value 전달).
    /// 호출 시 명시 초기화 필수 (struct default 의 DropSource.Enemy=0 / chapterNumber=0 trap).
    /// </summary>
    public struct DropContext
    {
        public DropSource source;
        public int chapterNumber;
        public EnemyData enemy;          // optional, 원인 적
        public EquipSlot? forcedSlot;    // Shop / 보스 티어 매핑 시
        public Rarity? forcedRarity;     // Shop 직구매 시
    }

    /// <summary>
    /// C2 — 아이템 드랍 결정 트리 (item_drop_v1 §4).
    /// 4 드랍원(Enemy/MiniBoss/Boss/BossRepeat/Shop) → 슬롯 → 등급 → 6줄 affix → 신화 시도(C7 보류).
    /// 등급 분포는 affix_pool_v1 §5 챕터 묶음 표 그대로.
    /// 슬롯·등급은 컨텍스트 강제(`forcedSlot`/`forcedRarity`)가 있으면 그것을 사용, 없으면 굴림.
    /// </summary>
    public static class ItemDropService
    {
        private static SlotAffixPool[] _slotPools;
        private static GlobalSubAffixPool _globalPool;
        private static EquipmentTemplate[] _templates;
        private static MythicDefinition[] _mythics;
        private static BalanceConfig _config;
        private static readonly System.Random _defaultRng = new System.Random();
        private static bool _initialized;

        public static bool IsInitialized => _initialized;

        /// <summary>
        /// 게임 부트스트랩 시점에 1회 호출. 미호출 상태에서 TryDrop 시 null 반환 + 경고.
        /// </summary>
        public static void Initialize(SlotAffixPool[] slotPools, GlobalSubAffixPool globalPool,
                                      EquipmentTemplate[] templates, BalanceConfig config,
                                      MythicDefinition[] mythics = null)
        {
            _slotPools = slotPools;
            _globalPool = globalPool;
            _templates = templates;
            _mythics = mythics;
            _config = config;
            _initialized = true;
        }

        public static void Reset()
        {
            _slotPools = null;
            _globalPool = null;
            _templates = null;
            _mythics = null;
            _config = null;
            _initialized = false;
        }

        /// <summary>
        /// 드랍 시도. null = 드랍 미발생 또는 매칭 템플릿 없음.
        /// </summary>
        public static EquipmentInstance TryDrop(DropContext ctx, System.Random rng = null)
        {
            if (!_initialized)
            {
                Debug.LogWarning("[ItemDropService] Initialize 미호출 — 드랍 무시");
                return null;
            }
            // 5017 T02 — DropContext struct 변환 후 null 검사 폐기 (struct 는 항상 valid)
            rng = rng ?? _defaultRng;

            if (!RollDropChance(ctx, rng)) return null;

            // C6 (PIVOT): 무기 슬롯은 가챠 한정 — drop 풀에서 제외 (gacha_v1.md §2.2).
            // forcedSlot=Weapon 도 차단 (Shop 직구매도 무기 X).
            if (ctx.forcedSlot == EquipSlot.Weapon) return null;

            EquipSlot slot = ctx.forcedSlot ?? RollSlot(ctx, rng);
            Rarity rarity = ctx.forcedRarity ?? RollRarity(ctx, rng);

            var template = PickTemplate(slot, rarity, rng);
            if (template == null) return null;

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

            var slotPool = FindSlotPool(slot);
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

            // 6. 신화 시도 — C7 MythicMileageService 가 마일리지 +1 + 발현 시도.
            //    추억 4종 미수집 시 IsUnlocked()=false 이므로 마일리지만 증가시키지 못함;
            //    잠금 상태에서 마일리지를 누적하려면 별도 호출 — 본 서비스는 잠금 시점에는 굴리지 않음.
            if (_mythics != null && _mythics.Length > 0)
                MythicMileageService.TryRoll(instance, _mythics, rng);

            GameEvents.RaiseAffixRolled(instance);
            return instance;
        }

        // ===== 1. 드랍 확률 =====

        private static bool RollDropChance(DropContext ctx, System.Random rng)
        {
            // PIVOT v2 — 상점 폐기 + 영구 인벤토리 (game_pivot_v2.md §2.3).
            // Enemy 0.5%→5% (10×) / MiniBoss 8%→30% (3.75×). Boss/Repeat 동일.
            float p = ctx.source switch
            {
                DropSource.Enemy => 0.05f,
                DropSource.MiniBoss => 0.30f,
                DropSource.Boss => 1.0f,
                DropSource.BossRepeat => 0.30f,
                DropSource.Shop => 1.0f,
                _ => 0f,
            };
            // PIVOT v3 — 적 드랍 확률 × 0.1 (마리수 ×10 으로 합산 동일). MiniBoss/Boss 변경 X.
            if (ctx.source == DropSource.Enemy && _config != null)
                p *= _config.enemyDropChanceMul;
            return rng.NextDouble() < p;
        }

        // ===== 2. 슬롯 결정 =====

        private static EquipSlot RollSlot(DropContext ctx, System.Random rng)
        {
            // item_drop §4.2 — 챕터 1~10 주 슬롯 7개 균등, 11+ 부터 장신구도 일정 확률.
            // MVP 단순화: 11+ 면 12 슬롯 균등.
            // C6 (PIVOT): 무기 슬롯(EquipSlot.Weapon=5) 은 가챠 한정 — drop 굴림에서 제외.
            int slotCount = ctx.chapterNumber <= 10 ? 7 : 12;
            // 12 슬롯 중 Weapon 1개 제외 — slotCount - 1 범위에서 굴리고, Weapon 이후 슬롯은 +1 보정.
            int picked = rng.Next(0, slotCount - 1);
            if (picked >= (int)EquipSlot.Weapon) picked += 1;
            return (EquipSlot)picked;
        }

        // ===== 3. 등급 굴림 =====

        private static Rarity RollRarity(DropContext ctx, System.Random rng)
        {
            // 미니/보스 가중 +1 등급 (item_drop §4.1)
            int bonus = ctx.source == DropSource.MiniBoss ? 1
                      : (ctx.source == DropSource.Boss || ctx.source == DropSource.BossRepeat) ? 1
                      : 0;
            double[] dist = GetChapterRarityDistribution(ctx.chapterNumber);
            double r = rng.NextDouble();
            double acc = 0;
            int picked = 0;
            for (int i = 0; i < 5; i++)
            {
                acc += dist[i];
                if (r < acc) { picked = i; break; }
            }
            picked = Mathf.Clamp(picked + bonus, 0, 4);
            return (Rarity)picked;
        }

        // affix_pool_v1 §5 챕터 묶음별 등급 분포 [Common, Magic, Rare, Epic, Legendary, Mythic]
        private static double[] GetChapterRarityDistribution(int chapterN)
        {
            if (chapterN <= 5) return new[] { 0.90, 0.10, 0.0, 0.0, 0.0 };
            if (chapterN <= 10) return new[] { 0.70, 0.30, 0.0, 0.0, 0.0 };
            if (chapterN <= 15) return new[] { 0.50, 0.40, 0.10, 0.0, 0.0 };
            if (chapterN <= 20) return new[] { 0.0, 0.50, 0.40, 0.10, 0.0 };
            if (chapterN <= 25) return new[] { 0.0, 0.30, 0.50, 0.20, 0.0 };
            if (chapterN <= 30) return new[] { 0.0, 0.0, 0.40, 0.50, 0.10 };
            return new[] { 0.0, 0.0, 0.20, 0.60, 0.20 };
        }

        // ===== 4. 템플릿 선택 =====

        private static EquipmentTemplate PickTemplate(EquipSlot slot, Rarity rarity, System.Random rng)
        {
            if (_templates == null || _templates.Length == 0) return null;

            // 1순위: 정확 매칭 (slot + rarity)
            var matches = new List<EquipmentTemplate>(8);
            for (int i = 0; i < _templates.Length; i++)
            {
                var t = _templates[i];
                if (t == null) continue;
                if (t.slot == slot && t.rarity == rarity) matches.Add(t);
            }
            if (matches.Count > 0) return matches[rng.Next(matches.Count)];

            // 2순위: 슬롯만 매칭 (등급 무관 — 폴백)
            for (int i = 0; i < _templates.Length; i++)
            {
                var t = _templates[i];
                if (t != null && t.slot == slot) matches.Add(t);
            }
            if (matches.Count > 0) return matches[rng.Next(matches.Count)];

            return null;
        }

        private static SlotAffixPool FindSlotPool(EquipSlot slot)
        {
            if (_slotPools == null) return null;
            for (int i = 0; i < _slotPools.Length; i++)
                if (_slotPools[i] != null && _slotPools[i].slot == slot)
                    return _slotPools[i];
            return null;
        }
    }
}
