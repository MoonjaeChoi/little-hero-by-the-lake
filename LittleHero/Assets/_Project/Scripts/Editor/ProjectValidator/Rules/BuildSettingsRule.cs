using System.Collections.Generic;
using UnityEditor;

namespace LittleHero.EditorTools.Rules
{
    public static class BuildSettingsRule
    {
        private const string ChapterScenePath = "Assets/_Project/Scenes/Chapter.unity";
        private const string AtticScenePath   = "Assets/_Project/Scenes/Attic.unity";

        public static void Validate(List<Issue> issues)
        {
            var scenes = EditorBuildSettings.scenes;
            bool hasAttic = false, hasChapter = false;
            foreach (var s in scenes)
            {
                if (!s.enabled) continue;
                if (s.path == AtticScenePath)  hasAttic = true;
                if (s.path == ChapterScenePath) hasChapter = true;
            }
            if (!hasAttic)   issues.Add(new Issue(Severity.Error, "BuildSettings", $"Attic 씬({AtticScenePath})이 활성 빌드 씬에 없음"));
            if (!hasChapter) issues.Add(new Issue(Severity.Error, "BuildSettings", $"Chapter 씬({ChapterScenePath})이 활성 빌드 씬에 없음"));
        }
    }
}
