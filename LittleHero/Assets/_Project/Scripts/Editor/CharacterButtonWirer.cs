#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5040.x — AtticCharacterSelector 의 _buttons[] 자동 wiring 진단 + fix.
    /// CharBtn name → CharacterData id 매트릭스:
    ///   CharBtn_default_boy   → hero_warrior
    ///   CharBtn_brave_girl    → hero_archer
    ///   CharBtn_quiet_friend  → hero_mage
    /// </summary>
    public static class CharacterButtonWirer
    {
        private const string ATTIC_SCENE = "Assets/_Project/Scenes/Attic.unity";
        private const string CHAR_DIR = "Assets/_Project/Data/Characters";

        [MenuItem("Tools/LittleHero/Diagnose + Fix CharBtn Wiring")]
        public static void DiagnoseAndFix()
        {
            var scene = EditorSceneManager.OpenScene(ATTIC_SCENE, OpenSceneMode.Single);
            AtticCharacterSelector selector = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                selector = root.GetComponentInChildren<AtticCharacterSelector>();
                if (selector != null) break;
            }
            if (selector == null) { Debug.LogError("[CharBtnWirer] AtticCharacterSelector 부재"); return; }

            // _buttons reflection
            var fld = typeof(AtticCharacterSelector).GetField("_buttons", BindingFlags.NonPublic | BindingFlags.Instance);
            var entries = (System.Collections.IList)fld.GetValue(selector);
            if (entries == null || entries.Count == 0) { Debug.LogError("[CharBtnWirer] _buttons 비어있음"); return; }

            // CharacterData 3 로드
            var warrior = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_HeroWarrior.asset");
            var archer  = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_HeroArcher.asset");
            var mage    = AssetDatabase.LoadAssetAtPath<CharacterData>($"{CHAR_DIR}/Char_HeroMage.asset");

            int fixedCount = 0, ok = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var charField = entry.GetType().GetField("character");
                var btnField = entry.GetType().GetField("button");
                if (charField == null || btnField == null) continue;
                var btn = (Button)btnField.GetValue(entry);
                if (btn == null) continue;
                string btnName = btn.gameObject.name;
                CharacterData expected = null;
                if (btnName.Contains("default_boy")) expected = warrior;
                else if (btnName.Contains("brave_girl")) expected = archer;
                else if (btnName.Contains("quiet_friend")) expected = mage;
                else continue;
                var current = (CharacterData)charField.GetValue(entry);
                string curId = current != null ? current.id : "null";
                Debug.Log($"[CharBtnWirer] entry[{i}] btn={btnName} current={curId} expected={expected.id}");
                if (current != expected)
                {
                    charField.SetValue(entry, expected);
                    fixedCount++;
                }
                else ok++;
            }
            fld.SetValue(selector, entries);
            EditorUtility.SetDirty(selector);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[CharBtnWirer] 완료 — fixed={fixedCount}, ok={ok}.");
        }
    }
}
#endif
