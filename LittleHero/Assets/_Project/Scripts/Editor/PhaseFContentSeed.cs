#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// Phase F 콘텐츠 일괄 시드 — F1+F2+F3 (챕터 4~35 적 풀) + F4 일일 보스 7종
    /// + F5 주간 보스 4종 + F7 고난도 챕터 36~50 클론.
    ///
    /// 메뉴: Tools > LittleHero > Seed Phase F Content. 멱등.
    ///
    /// 전략 — **archetype 재사용**: 챕터별로 unique 적 SO 를 만들지 않고, 5 카테고리(MetaMemory/
    /// Nature/Animal/Civilization/Fantasy) 별 1세트(레귤러 3 + 미니 2 + 보스 1) = 6×5 = 30 archetype SO.
    /// 각 ChapterData 는 카테고리에 맞는 archetype 풀을 참조. 챕터 번호↑ 곡선은
    /// EnemySpawner(C9) 가 enemyStatMultiplier 자동 적용 → archetype 베이스값 1세트만 유지.
    /// </summary>
    public static class PhaseFContentSeed
    {
        private const string ENEMY_DIR = "Assets/_Project/Data/Enemies";
        private const string CHAPTER_DIR = "Assets/_Project/Data/Chapters/V2";
        private const string ENDGAME_DIR = "Assets/_Project/Data/Endgame";
        private const string ENEMY_PREFAB_PATH = "Assets/_Project/Prefabs/Enemy.prefab";

        // 5 카테고리 × 6 역할 = 30 archetype
        private struct Archetype
        {
            public string id;
            public string displayName;
            public Color tint;
            public float scale;
            public float baseHp;
            public float baseDmg;
            public float baseXp;
            public float baseDust;
            public float armor;
            public bool isMiniBoss;
            public bool isBoss;
        }

        // 카테고리별 색상 (chapter_structure_v2 §3 톤)
        private static readonly Color META_TINT     = new Color(0.85f, 0.78f, 0.65f); // 따뜻한 갈색
        private static readonly Color NATURE_TINT   = new Color(0.55f, 0.78f, 0.55f); // 푸른 자연
        private static readonly Color ANIMAL_TINT   = new Color(0.95f, 0.78f, 0.50f); // 따뜻한 동물
        private static readonly Color CIV_TINT      = new Color(0.85f, 0.55f, 0.65f); // 도시·인공
        private static readonly Color FANTASY_TINT  = new Color(0.65f, 0.55f, 0.95f); // 환상 보라
        private static readonly Color DAILY_TINT    = new Color(1.00f, 0.75f, 0.30f); // 일일 보스 노랑
        private static readonly Color WEEKLY_TINT   = new Color(1.00f, 0.55f, 0.55f); // 주간 보스 빨강
        private static readonly Color HARDCORE_TINT = new Color(0.30f, 0.20f, 0.45f); // 고난도 짙은 보라

        [MenuItem("Tools/LittleHero/Seed Phase F Content")]
        public static void Seed()
        {
            int created = 0;

            // F1+F2+F3 — 5 카테고리 × 6 archetype = 30 enemy SO
            var metaPool     = MakeArchetypeSet("Meta",     META_TINT,    1.0f, ref created);
            var naturePool   = MakeArchetypeSet("Nature",   NATURE_TINT,  1.0f, ref created);
            var animalPool   = MakeArchetypeSet("Animal",   ANIMAL_TINT,  1.05f, ref created);
            var civPool      = MakeArchetypeSet("Civ",      CIV_TINT,     1.0f, ref created);
            var fantasyPool  = MakeArchetypeSet("Fantasy",  FANTASY_TINT, 1.1f, ref created);

            // F4 — 일일 보스 7종 (요일별)
            var dailyBosses = new EnemyData[7];
            string[] dayNames = { "월", "화", "수", "목", "금", "토", "일" };
            for (int i = 0; i < 7; i++)
                dailyBosses[i] = MakeBoss($"DailyBoss_{i:00}", $"{dayNames[i]}요일 큰 친구", DAILY_TINT, 1.5f, 5000f, 50f, 50f, 50f, ref created);

            // F5 — 주간 보스 4종 (4주 사이클)
            var weeklyBosses = new EnemyData[4];
            for (int i = 0; i < 4; i++)
                weeklyBosses[i] = MakeBoss($"WeeklyBoss_{i:00}", $"환상의 거대 친구 {i + 1}주", WEEKLY_TINT, 2.2f, 25000f, 150f, 80f, 200f, ref created);

            AssetDatabase.SaveAssets();

            // 챕터 와이어링 — chapter_structure_v2 §3 카테고리 매핑
            int wired = 0;
            for (int n = 1; n <= 35; n++)
            {
                var ch = LoadChapter(n);
                if (ch == null) continue;
                var pool = SelectPoolForChapter(n, metaPool, naturePool, animalPool, civPool, fantasyPool);
                ch.enemyPool = new[] { pool[0], pool[1], pool[2] };  // 레귤러 3
                ch.miniBossPool = new[] { pool[3], pool[4] };        // 미니 2
                ch.bossEnemy = pool[5];                               // 메인 1
                EditorUtility.SetDirty(ch);
                wired++;
            }
            AssetDatabase.SaveAssets();

            // F4·F5 — EndgameModeData.bossPool 갱신
            var dailyMode = AssetDatabase.LoadAssetAtPath<EndgameModeData>($"{ENDGAME_DIR}/daily_boss.asset");
            if (dailyMode != null) { dailyMode.bossPool = dailyBosses; EditorUtility.SetDirty(dailyMode); }
            var weeklyMode = AssetDatabase.LoadAssetAtPath<EndgameModeData>($"{ENDGAME_DIR}/weekly_boss.asset");
            if (weeklyMode != null) { weeklyMode.bossPool = weeklyBosses; EditorUtility.SetDirty(weeklyMode); }

            // F7 — 고난도 챕터 36~50 (CH 21~35 클론, x2.0 추가 곱셈)
            int hardcoreCreated = 0;
            for (int n = 36; n <= 50; n++)
            {
                int sourceN = n - 15; // 21~35
                var src = LoadChapter(sourceN);
                if (src == null) continue;
                string id = $"CH_{n}_Hardcore_{src.id}";
                string path = $"{CHAPTER_DIR}/{id}.asset";
                var so = AssetDatabase.LoadAssetAtPath<ChapterData>(path) ?? ScriptableObject.CreateInstance<ChapterData>();
                so.id = id;
                so.displayName = $"어둠의 {src.displayName}";
                so.description = $"고난도 챕터 — {src.displayName} 의 어둠 변형";
                so.totalWaves = 20;
                so.waveDuration = 45f;
                so.chapterNumber = n;
                so.enemyStatMultiplier = src.enemyStatMultiplier * 2.0f;
                so.starFragmentReward = src.starFragmentReward * 2;
                so.enemyPool = src.enemyPool;
                so.miniBossPool = src.miniBossPool;
                so.bossEnemy = src.bossEnemy;
                if (!AssetDatabase.Contains(so)) AssetDatabase.CreateAsset(so, path);
                EditorUtility.SetDirty(so);
                hardcoreCreated++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PhaseFContentSeed] archetype={created} 챕터 와이어링={wired}/35 고난도={hardcoreCreated}/15 dailyMode={(dailyMode != null)} weeklyMode={(weeklyMode != null)}");
        }

        private static EnemyData[] MakeArchetypeSet(string category, Color tint, float baseScale, ref int created)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ENEMY_PREFAB_PATH);
            if (prefab == null)
                Debug.LogWarning($"[PhaseFContentSeed] Enemy.prefab 미발견 — null prefab 사용");

            var arr = new EnemyData[6];
            // 레귤러 3 — 작은/중간/큰 변형
            arr[0] = MakeEnemy($"Enemy_{category}_Small",  $"{Localize(category)} 작은 친구",  prefab, tint, baseScale * 0.9f, 30f, 5f,  2f, 1f, 0f,    false, false, ref created);
            arr[1] = MakeEnemy($"Enemy_{category}_Medium", $"{Localize(category)} 친구",       prefab, tint, baseScale,        50f, 8f,  3f, 2f, 0f,    false, false, ref created);
            arr[2] = MakeEnemy($"Enemy_{category}_Large",  $"{Localize(category)} 큰 친구",    prefab, tint, baseScale * 1.15f, 80f, 12f, 5f, 3f, 5f,    false, false, ref created);
            // 미니보스 2
            arr[3] = MakeEnemy($"Enemy_{category}_MiniA",  $"{Localize(category)} 수호자",     prefab, tint, baseScale * 1.3f, 200f, 20f, 12f, 8f, 15f,  true,  false, ref created);
            arr[4] = MakeEnemy($"Enemy_{category}_MiniB",  $"{Localize(category)} 큰 수호자",  prefab, tint, baseScale * 1.4f, 280f, 24f, 16f, 10f, 20f, true,  false, ref created);
            // 메인 보스 1
            arr[5] = MakeEnemy($"Enemy_{category}_Boss",   $"{Localize(category)} 으뜸 친구",  prefab, tint, baseScale * 1.7f, 700f, 36f, 50f, 25f, 30f, false, true,  ref created);
            return arr;
        }

        private static EnemyData MakeBoss(string id, string display, Color tint, float scale,
                                           float hp, float dmg, float armor, float dust,
                                           ref int created)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ENEMY_PREFAB_PATH);
            return MakeEnemy(id, display, prefab, tint, scale, hp, dmg, 100f, dust, armor, false, true, ref created);
        }

        private static EnemyData MakeEnemy(string id, string displayName, GameObject prefab,
                                            Color tint, float scale,
                                            float hp, float dmg, float xp, float dust, float armor,
                                            bool isMiniB, bool isBoss, ref int created)
        {
            string path = $"{ENEMY_DIR}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<EnemyData>(path) ?? ScriptableObject.CreateInstance<EnemyData>();
            so.id = id;
            so.displayName = displayName;
            so.prefab = prefab;
            so.maxHealth = hp;
            so.damage = dmg;
            so.moveSpeed = isBoss ? 1.6f : (isMiniB ? 2.2f : 2.8f);
            so.xpDrop = xp;
            so.starDustDrop = dust;
            so.armor = armor;
            so.tintColor = tint;
            so.scaleMultiplier = scale;
            so.isMiniB = isMiniB;
            so.isBoss = isBoss;
            if (!AssetDatabase.Contains(so)) AssetDatabase.CreateAsset(so, path);
            EditorUtility.SetDirty(so);
            created++;
            return so;
        }

        private static EnemyData[] SelectPoolForChapter(int chapterNum,
            EnemyData[] meta, EnemyData[] nature, EnemyData[] animal, EnemyData[] civ, EnemyData[] fantasy)
        {
            // chapter_structure_v2 §3 카테고리 매핑
            if (chapterNum <= 5)  return meta;
            if (chapterNum <= 14) return nature;
            if (chapterNum <= 20) return animal;
            if (chapterNum <= 27) return civ;
            return fantasy;
        }

        private static ChapterData LoadChapter(int chapterNum)
        {
            // CH_{N:00}_*.asset 검색
            string[] guids = AssetDatabase.FindAssets($"CH_{chapterNum:00}_ t:ChapterData", new[] { CHAPTER_DIR });
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<ChapterData>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static string Localize(string category) => category switch
        {
            "Meta"    => "마을의",
            "Nature"  => "숲의",
            "Animal"  => "동물 친구",
            "Civ"     => "도시의",
            "Fantasy" => "환상의",
            _ => category,
        };
    }
}
#endif
