#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5037 — 6 Variant EnemyData SO 멱등 생성/갱신. ChapterVariantSeeder 의 정적 헬퍼.
    /// 본체 60L 이내. §15.6 100L 룰 정합.
    /// </summary>
    internal static class VariantSoFactory
    {
        private const string VARIANTS_PATH = "Assets/_Project/Data/Enemies/Variants";

        public static void EnsureAllVariants()
        {
            EnsureFolder();
            EnsureStationary("Variant_Stationary_Sentry", "포대 보초", 30f, 5f);
            EnsureCharger   ("Variant_Charger_Bull",      "돌진 황소", 60f, 8f);
            var child = EnsureFollow("Variant_Splitter_BearChild", "작은 곰", 12f, 2f);
            EnsureSplitter ("Variant_Splitter_Bear",      "분열 곰",  40f, 4f, child);
            EnsureRanged   ("Variant_Ranged_Archer",      "원거리 궁수", 25f, 6f);
            EnsureBomber   ("Variant_Bomber_Mine",        "지뢰",     20f, 8f);
        }

        private static EnemyData EnsureBase(string id, string display, float hp, float dmg, float speed)
        {
            string path = $"{VARIANTS_PATH}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (so == null) { so = ScriptableObject.CreateInstance<EnemyData>(); AssetDatabase.CreateAsset(so, path); }
            so.id = id; so.displayName = display; so.maxHealth = hp; so.damage = dmg; so.moveSpeed = speed;
            so.xpDrop = Mathf.Max(1f, hp * 0.1f); so.starDustDrop = Mathf.Max(1f, hp * 0.05f);
            EditorUtility.SetDirty(so);
            return so;
        }

        private static EnemyData EnsureFollow(string id, string display, float hp, float dmg)
        { var so = EnsureBase(id, display, hp, dmg, 1.8f); so.behaviorType = EnemyBehaviorType.Follow; return so; }

        private static void EnsureStationary(string id, string display, float hp, float dmg)
        { var so = EnsureBase(id, display, hp, dmg, 0f); so.behaviorType = EnemyBehaviorType.Stationary; so.armor = 5f; so.rangedFireInterval = 1.5f; }

        private static void EnsureCharger(string id, string display, float hp, float dmg)
        { var so = EnsureBase(id, display, hp, dmg, 2.5f); so.behaviorType = EnemyBehaviorType.Charger; so.chargerInterval = 3f; so.chargerSpeedMultiplier = 3f; }

        private static void EnsureSplitter(string id, string display, float hp, float dmg, EnemyData child)
        { var so = EnsureBase(id, display, hp, dmg, 1.5f); so.behaviorType = EnemyBehaviorType.Splitter;
          so.deathReactionType = EnemyDeathReactionType.Splitter; so.splitterChildData = child; so.splitterChildCount = 3; }

        private static void EnsureRanged(string id, string display, float hp, float dmg)
        { var so = EnsureBase(id, display, hp, dmg, 1.2f); so.behaviorType = EnemyBehaviorType.Ranged;
          so.rangedKeepDistance = 4f; so.rangedFireInterval = 1.5f; }

        private static void EnsureBomber(string id, string display, float hp, float dmg)
        { var so = EnsureBase(id, display, hp, dmg, 1.8f); so.behaviorType = EnemyBehaviorType.Bomber;
          so.deathReactionType = EnemyDeathReactionType.Explode;
          so.bomberRadius = 1.5f; so.bomberDamage = 30f; so.bomberTelegraphSeconds = 1f; }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(VARIANTS_PATH))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Enemies"))
                    AssetDatabase.CreateFolder("Assets/_Project/Data", "Enemies");
                AssetDatabase.CreateFolder("Assets/_Project/Data/Enemies", "Variants");
            }
        }
    }
}
#endif
