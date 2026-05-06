using System.Collections.Generic;
using System.IO;

namespace LittleHero.EditorTools.Rules
{
    /// <summary>
    /// CLAUDE.md §15.12 — OOP 정책 자동 검증 (200줄+ 클래스 경고).
    /// </summary>
    public static class OOPPolicyRule
    {
        public static void Validate(List<Issue> issues)
        {
            const int LINE_LIMIT = 200;  // 100줄 룰 + 여유 100
            var scriptFiles = Directory.GetFiles(
                "Assets/_Project/Scripts", "*.cs", SearchOption.AllDirectories);
            int violations = 0;
            foreach (var f in scriptFiles)
            {
                int lineCount = File.ReadAllLines(f).Length;
                if (lineCount > LINE_LIMIT)
                {
                    issues.Add(new Issue(Severity.Warning, "OOP",
                        $"§15.6 100줄 룰 — {Path.GetFileName(f)} ({lineCount} 라인) 책임 분리 권장"));
                    violations++;
                }
            }
            if (violations == 0)
                issues.Add(new Issue(Severity.Warning, "OOP", $"§15.6 100줄 룰 — {scriptFiles.Length} 파일 모두 통과 (LINE_LIMIT={LINE_LIMIT})"));
        }
    }
}
