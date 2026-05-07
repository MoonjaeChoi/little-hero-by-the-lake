#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5033.4 — 방어구 16 EquipmentTemplate 의 baseStats 등급별 자동 보강.
    /// 슬롯 강화 multiplier 가 의미 있게 적용되도록 0 stat 보강.
    /// 무기 25 SO 는 attackConfig.damage 에 starforce 적용 (DamageDealer.EnhanceDamage) — 별도.
    /// </summary>
    public static class MigrateArmorBaseStats
    {
        private const string MENU_PATH = "Tools/LittleHero/Migrate Armor baseStats";

        // 등급별 stat 보강 (Common 0 / Magic 1 / Rare 2 / Epic 3 / Legendary 4 / Mythic 5)
        private static readonly int[] _hpByRarity   = { 10, 20, 40, 80, 150, 250 };
        private static readonly int[] _atkByRarity  = { 1, 3, 6, 12, 20, 35 };
        private static readonly float[] _spdByRarity = { 0.05f, 0.10f, 0.15f, 0.20f, 0.30f, 0.45f };
        private static readonly float[] _aspByRarity = { 0.05f, 0.10f, 0.15f, 0.20f, 0.30f, 0.45f };

        // slot 별 적합한 stat 선택 (slot 정수 — EquipSlot enum 정합)
        // 0=Hat / 1=Top / 2=Bottom / 3=Cloak / 4=Gloves / 5=Weapon / 6=Shoes
        // 7=Earring / 8=Glasses / 9=Necklace / 10=Ring / 11=Pet
        private static readonly Dictionary<int, string[]> _slotStats = new Dictionary<int, string[]>
        {
            { 0, new[] { "hp", "atk" } },         // Hat: 체력 + 공격
            { 1, new[] { "hp", "atk" } },         // Top: 체력 + 공격
            { 2, new[] { "hp", "spd" } },         // Bottom: 체력 + 속도
            { 3, new[] { "spd", "atk" } },        // Cloak: 속도 + 공격
            { 4, new[] { "atk", "asp" } },        // Gloves: 공격 + 공속
            { 6, new[] { "spd", "asp" } },        // Shoes: 속도 + 공속
            { 7, new[] { "atk", "asp" } },        // Earring: 공격 + 공속
            { 8, new[] { "atk", "spd" } },        // Glasses: 공격 + 속도
            { 9, new[] { "atk", "hp" } },         // Necklace: 공격 + 체력
            { 10, new[] { "atk", "hp" } },        // Ring: 공격 + 체력
            { 11, new[] { "hp", "atk" } },        // Pet: 체력 + 공격
        };

        [MenuItem(MENU_PATH)]
        public static void Migrate()
        {
            var guids = AssetDatabase.FindAssets("t:EquipmentTemplate");
            int updated = 0;
            int skipped = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Weapons/")) { skipped++; continue; }  // 무기 25 SO skip
                var t = AssetDatabase.LoadAssetAtPath<EquipmentTemplate>(path);
                if (t == null) continue;
                if (t.baseStats == null) t.baseStats = new StatModifiers();
                var s = t.baseStats;
                int rar = Mathf.Clamp((int)t.rarity, 0, _hpByRarity.Length - 1);
                int slotIdx = (int)t.slot;
                if (!_slotStats.TryGetValue(slotIdx, out var fields)) fields = new[] { "hp", "atk" };

                bool changed = false;
                foreach (var field in fields)
                {
                    switch (field)
                    {
                        case "hp":
                            if (s.maxHealthBonus < 0.01f) { s.maxHealthBonus = _hpByRarity[rar]; changed = true; }
                            break;
                        case "atk":
                            if (s.attackDamageBonus < 0.01f) { s.attackDamageBonus = _atkByRarity[rar]; changed = true; }
                            break;
                        case "spd":
                            if (s.speedBonus < 0.01f) { s.speedBonus = _spdByRarity[rar]; changed = true; }
                            break;
                        case "asp":
                            if (s.attackSpeedBonus < 0.01f) { s.attackSpeedBonus = _aspByRarity[rar]; changed = true; }
                            break;
                    }
                }

                if (changed)
                {
                    EditorUtility.SetDirty(t);
                    updated++;
                    Debug.Log($"[MigrateArmor] {t.name} (slot={t.slot} rar={t.rarity}) → hp={s.maxHealthBonus} spd={s.speedBonus} atk={s.attackDamageBonus} asp={s.attackSpeedBonus}");
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[MigrateArmor] 완료 — updated={updated} skipped(weapons)={skipped}");
        }
    }
}
#endif
