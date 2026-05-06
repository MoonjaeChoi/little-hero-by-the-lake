#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5029 — EnemyData 262 SO elementResistance[4] 기본값 마이그레이션.
    /// AttackPatternConfig.element 는 무기 SO 별로 별도 처리 (MageWeaponElementMigrator).
    /// </summary>
    public static class ElementMigrator
    {
        [MenuItem("Tools/LittleHero/Migrate Element System")]
        public static void Run()
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.EnemyData");
            int migrated = 0, skipped = 0;
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var d = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                if (d == null) continue;

                if (d.elementResistance != null && d.elementResistance.Length == 4)
                {
                    skipped++;
                    continue;
                }
                d.elementResistance = new[] { 1f, 1f, 1f, 1f };
                EditorUtility.SetDirty(d);
                migrated++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[ElementMigrator] EnemyData elementResistance migrated={migrated} skipped={skipped}");
            EditorUtility.DisplayDialog("Element Migrator",
                $"262 EnemyData 마이그레이션 완료\n갱신: {migrated} / 건너뜀: {skipped}", "OK");
        }
    }
}
#endif
