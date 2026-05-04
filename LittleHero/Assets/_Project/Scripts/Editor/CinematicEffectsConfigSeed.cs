#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5007 — CinematicEffectsConfig.asset 자동 생성 (Resources/Combat/ 하위 — Resources.Load 정적 cache 용).
    /// 멱등 — 기존 asset 있으면 보존 (디자이너 튜닝 값 손상 방지).
    /// </summary>
    public static class CinematicEffectsConfigSeed
    {
        private const string DIR = "Assets/_Project/Resources/Combat";
        private const string PATH = DIR + "/CinematicEffectsConfig.asset";

        [MenuItem("Tools/LittleHero/Seed CinematicEffectsConfig")]
        public static void Seed()
        {
            EnsureFolder("Assets/_Project/Resources", "Combat");

            var existing = AssetDatabase.LoadAssetAtPath<CinematicEffectsConfig>(PATH);
            if (existing != null)
            {
                Debug.Log($"[CinematicEffectsConfigSeed] 이미 존재 (기존 값 보존): {PATH}");
                EditorGUIUtility.PingObject(existing);
                return;
            }

            var so = ScriptableObject.CreateInstance<CinematicEffectsConfig>();
            AssetDatabase.CreateAsset(so, PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CinematicEffectsConfigSeed] 신규 생성 (default 값): {PATH}");
            EditorGUIUtility.PingObject(so);
        }

        private static void EnsureFolder(string parent, string folder)
        {
            string full = parent + "/" + folder;
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
