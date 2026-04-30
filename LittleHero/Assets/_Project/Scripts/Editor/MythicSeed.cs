#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// B10 — 아빠의 마법 4종 (equipment §3.7) 시드.
    /// 메뉴: Tools > LittleHero > Seed Mythic SOs.
    /// 멱등.
    /// </summary>
    public static class MythicSeed
    {
        private const string DIR = "Assets/_Project/Data/Mythics";

        [MenuItem("Tools/LittleHero/Seed Mythic SOs")]
        public static void Seed()
        {
            EnsureFolder("Assets/_Project/Data", "Mythics");

            Make("mythic_dad_compass", "아빠의 나침반",
                "아빠가 길을 알려준다",
                "보스 위치 미니맵 표시 + 보스 데미지 +20% (MUL)",
                new EquipSlot[0]);

            Make("mythic_dad_lullaby", "아빠의 자장가",
                "위험할 때 진정시켜준다",
                "HP 30% 이하 시 받는 데미지 -50% (when_hp_below_30)",
                new EquipSlot[0]);

            Make("mythic_dad_courage", "아빠의 용기",
                "마지막 순간 폭발",
                "적 처치 직전 일정 시간 공격력 +50%",
                new EquipSlot[0]);

            Make("mythic_dad_promise", "아빠의 약속",
                "자취가 두 배로",
                "Trail 자취 길이 ×2 + 옵션 리롤 1회 무료",
                new[] { EquipSlot.Shoes });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            int n = AssetDatabase.FindAssets("t:MythicDefinition", new[] { DIR }).Length;
            Debug.Log($"[MythicSeed] {n} / 4");
        }

        private static void Make(string id, string display, string flavor, string effect, EquipSlot[] restriction)
        {
            string path = $"{DIR}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<MythicDefinition>(path) ?? ScriptableObject.CreateInstance<MythicDefinition>();
            so.id = id;
            so.displayName = display;
            so.flavorText = flavor;
            so.effectDescription = effect;
            so.slotRestriction = restriction;
            if (!AssetDatabase.Contains(so)) AssetDatabase.CreateAsset(so, path);
            EditorUtility.SetDirty(so);
        }

        private static void EnsureFolder(string parent, string name)
        {
            string full = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
