#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5038.1 — CH1 시범 wave 단위 콘텐츠 시드.
    /// Step 3 대안 BuildFromComposition 단순화 답습 — count 합산만 사용, enemyPool/miniBoss 는 _chapterData 답습.
    /// 매트릭스: wave 1~4/6~9/11~14/16~19=Standard / 5,10,15=Elite (count 증가) / 20=Boss는 BossEncounter 분리.
    /// </summary>
    public static class WaveCompositionSeeder
    {
        private const string WAVES_PATH = "Assets/_Project/Data/Waves";

        [MenuItem("Tools/LittleHero/Seed Wave Compositions — CH1 시범")]
        public static void SeedCH1() => SeedRange(1, 1);

        /// <summary>5038.2 — CH2~35 일괄 시드 (V2 35 챕터 완전 wave 단위 적용).</summary>
        [MenuItem("Tools/LittleHero/Seed Wave Compositions — CH2~35 일괄")]
        public static void SeedAllV2() => SeedRange(2, 35);

        /// <summary>5038.2 — CH1~35 일괄 (CH1 멱등 갱신 포함).</summary>
        [MenuItem("Tools/LittleHero/Seed Wave Compositions — CH1~35 전체")]
        public static void SeedAll() => SeedRange(1, 35);

        /// <summary>5038.3 — Hardcore CH36~50 (50 챕터 완전 wave 단위 적용).</summary>
        [MenuItem("Tools/LittleHero/Seed Wave Compositions — Hardcore CH36~50")]
        public static void SeedHardcore() => SeedRange(36, 50);

        private static void SeedRange(int startChapter, int endChapter)
        {
            EnsureFolder();
            int wired = 0, skipped = 0;
            for (int n = startChapter; n <= endChapter; n++)
            {
                if (SeedOneChapter(n)) wired++;
                else skipped++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[WaveCompositionSeeder] CH{startChapter}~CH{endChapter} 시드 완료 — wired={wired}, skipped={skipped}.");
        }

        private static bool SeedOneChapter(int chapterNum)
        {
            var ch = ChapterVariantSeeder.LoadChapterByNumber(chapterNum);
            if (ch == null) { Debug.LogWarning($"[WaveCompositionSeeder] CH{chapterNum} ChapterData 부재"); return false; }
            if (ch.enemyPool == null || ch.enemyPool.Length == 0) { Debug.LogWarning($"[WaveCompositionSeeder] CH{chapterNum} enemyPool 비어있음"); return false; }

            var waves = new WaveComposition[ch.totalWaves];
            for (int i = 0; i < ch.totalWaves; i++)
            {
                int waveNum = i + 1;
                bool isElite = waveNum == 5 || waveNum == 10 || waveNum == 15;
                bool isBoss  = waveNum == ch.totalWaves;
                if (isBoss) { waves[i] = null; continue; }
                waves[i] = EnsureWaveComp(ch, waveNum, isElite);
            }
            ch.waves = waves;
            EditorUtility.SetDirty(ch);
            return true;
        }

        private static WaveComposition EnsureWaveComp(ChapterData ch, int waveNum, bool isElite)
        {
            string id = $"Wave_CH{ch.chapterNumber:D2}_W{waveNum:D2}";
            string path = $"{WAVES_PATH}/{id}.asset";
            var wc = AssetDatabase.LoadAssetAtPath<WaveComposition>(path);
            if (wc == null) { wc = ScriptableObject.CreateInstance<WaveComposition>(); AssetDatabase.CreateAsset(wc, path); }
            wc.waveType = isElite ? WaveType.Elite : WaveType.Standard;
            wc.waveDuration = ch.waveDuration;
            wc.restBeforeWave = 5f;

            // count 합산만 사용 — enemyData 는 BuildFromComposition 에서 무시 (_chapterData.enemyPool 답습)
            int regularCount = 8 + waveNum;  // 점진 증가 (wave 1=9, wave 19=27)
            var comps = new System.Collections.Generic.List<EnemyComposition>();
            comps.Add(EnsureEnemyComp(id + "_E1", ch.enemyPool[0], regularCount));
            wc.compositions = comps.ToArray();
            EditorUtility.SetDirty(wc);
            return wc;
        }

        private static EnemyComposition EnsureEnemyComp(string id, EnemyData enemy, int count)
        {
            string path = $"{WAVES_PATH}/{id}.asset";
            var ec = AssetDatabase.LoadAssetAtPath<EnemyComposition>(path);
            if (ec == null) { ec = ScriptableObject.CreateInstance<EnemyComposition>(); AssetDatabase.CreateAsset(ec, path); }
            ec.enemyData = enemy;  // 참고용 (BuildFromComposition 가 무시)
            ec.count = count;
            ec.pattern = SpawnPattern.Continuous;
            ec.interval = 0.5f;
            ec.startDelay = 0f;
            EditorUtility.SetDirty(ec);
            return ec;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(WAVES_PATH))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Waves");
        }
    }
}
#endif
