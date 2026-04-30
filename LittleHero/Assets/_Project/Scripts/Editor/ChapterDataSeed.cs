#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// B7 — chapter_structure_v2 §3 의 35 챕터 ChapterData SO 일괄 시드.
    /// 메뉴: Tools > LittleHero > Seed 35 Chapter SOs.
    /// 멱등. 기존 3개(Chapter_01_RedHood 등)는 폐기 동화 모델 — 본 시드는 별도 폴더 `Chapters/V2/` 에 생성하여 공존.
    /// enemyStatMultiplier = 1.10^(N-1) (chapter_curve §9.2).
    /// 적/보스 풀(enemyPool/miniBossPool/bossEnemy)은 F1+ 콘텐츠 작업에서 채움.
    /// </summary>
    public static class ChapterDataSeed
    {
        private const string DIR = "Assets/_Project/Data/Chapters/V2";

        private static readonly string[] NAMES = {
            "호숫가 작은 마을",        // 1
            "다락방 깊숙한 곳",        // 2
            "잊혀진 학교",             // 3
            "마당의 옛 우물",           // 4
            "아빠의 비밀 기지",         // 5
            "안개 골짜기",             // 6
            "깊은 숲",                 // 7
            "노래하는 폭포",           // 8
            "반딧불 풀밭",             // 9
            "모래 회오리 사막",         // 10
            "얼음 동굴",               // 11
            "산호초 바다",             // 12
            "화산 분화구 가장자리",     // 13
            "하늘 구름 다리",           // 14
            "토끼굴 깊은 곳",           // 15
            "새들의 둥지 도시",         // 16
            "발광 해파리 정원",         // 17
            "고래 노래 바다",           // 18
            "거대 거북이 등",           // 19
            "별빛 늑대 산골",           // 20
            "등불 시장",               // 21
            "사탕 공장",               // 22
            "인형 공방",               // 23
            "우편 비행기 마을",         // 24
            "두루마리 미로",           // 25
            "음악 동굴",               // 26
            "종이 인형 무대",           // 27
            "살아있는 정원",           // 28
            "거울 갤러리",             // 29
            "거대한 시계탑 안",         // 30
            "시간이 멈춘 마을",         // 31
            "거꾸로 매달린 지하 도시",  // 32
            "달의 뒷면",               // 33
            "황혼의 들판",             // 34
            "별과 별 사이",            // 35
        };

        [MenuItem("Tools/LittleHero/Seed 35 Chapter SOs")]
        public static void Seed()
        {
            EnsureFolder("Assets/_Project/Data", "Chapters");
            EnsureFolder("Assets/_Project/Data/Chapters", "V2");

            for (int i = 0; i < NAMES.Length; i++)
            {
                int n = i + 1;
                string id = $"CH_{n:00}";
                string path = $"{DIR}/{id}_{ToFileSafe(NAMES[i])}.asset";
                var so = AssetDatabase.LoadAssetAtPath<ChapterData>(path) ?? ScriptableObject.CreateInstance<ChapterData>();
                so.id = id;
                so.displayName = NAMES[i];
                so.description = $"챕터 {n} — {NAMES[i]}";
                so.totalWaves = 20;
                so.waveDuration = 45f;
                so.chapterNumber = n;
                so.enemyStatMultiplier = Mathf.Pow(1.10f, n - 1);
                // economy §6.1 무지개 구슬 첫 클리어 보상
                so.starFragmentReward = Mathf.RoundToInt(5f * Mathf.Pow(1.10f, n - 1));
                // 적/보스 풀은 비워둠 — F1 콘텐츠 작업에서 채움
                if (so.enemyPool == null) so.enemyPool = new EnemyData[0];
                if (so.miniBossPool == null) so.miniBossPool = new EnemyData[0];
                if (!AssetDatabase.Contains(so)) AssetDatabase.CreateAsset(so, path);
                EditorUtility.SetDirty(so);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            int n2 = AssetDatabase.FindAssets("t:ChapterData", new[] { DIR }).Length;
            Debug.Log($"[ChapterDataSeed] V2 챕터 SO {n2} / 35");
        }

        private static string ToFileSafe(string s)
            => s.Replace(' ', '_').Replace('.', '_');

        private static void EnsureFolder(string parent, string name)
        {
            string full = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
