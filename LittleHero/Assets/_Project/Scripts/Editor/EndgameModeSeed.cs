#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// B8 — endgame_v1 §4·§5·§6·§3 의 EndgameModeData 4종 시드.
    /// 메뉴: Tools > LittleHero > Seed Endgame Modes.
    /// 보상은 endgame §4.4·§5.5·§6.4·§3.4 표 그대로.
    /// 보스 풀(bossPool)은 F4·F5 콘텐츠 작업에서 채움.
    /// </summary>
    public static class EndgameModeSeed
    {
        private const string DIR = "Assets/_Project/Data/Endgame";

        [MenuItem("Tools/LittleHero/Seed Endgame Modes")]
        public static void Seed()
        {
            EnsureFolder("Assets/_Project/Data", "Endgame");

            // §4.4 일일 보스 — 매일 1회. 첫 클리어만 보상.
            MakeMode(
                id: "daily_boss",
                displayName: "일일 보스",
                type: ModeType.DailyBoss,
                cooldown: 86400,
                first: new[] {
                    Reward(CurrencyType.MagicCrystal, 5),
                    Reward(CurrencyType.Starlight,    5),
                    Reward(CurrencyType.RainbowOrb,   10),
                    Reward(CurrencyType.Xp,           50000),
                },
                repeat: new RewardEntry[0]
            );

            // §5 주간 보스 — 신화 옵션 발현 보장 (F5 처리)
            MakeMode(
                id: "weekly_boss",
                displayName: "주간 보스",
                type: ModeType.WeeklyBoss,
                cooldown: 604800,
                first: new[] {
                    Reward(CurrencyType.MagicCrystal, 25),
                    Reward(CurrencyType.Starlight,    20),
                    Reward(CurrencyType.RainbowOrb,   50),
                    Reward(CurrencyType.Xp,           200000),
                },
                repeat: new RewardEntry[0]
            );

            // §6 무한 다락방 — 깊이 비례
            MakeMode(
                id: "infinite_tower",
                displayName: "무한 다락방",
                type: ModeType.InfiniteTower,
                cooldown: 0,
                first: new[] {
                    Reward(CurrencyType.MagicCrystal, 1, scale: 1),  // 깊이 N당 +1
                    Reward(CurrencyType.Starlight,    1, scale: 1),
                    Reward(CurrencyType.StarDust,     500, scale: 50),
                },
                repeat: new RewardEntry[0]
            );

            // §3 고난도 챕터 (CH 36~50) — 어둠 모드. 첫·반복 보상 분리.
            MakeMode(
                id: "hardcore_chapter",
                displayName: "고난도 챕터",
                type: ModeType.HardcoreChapter,
                cooldown: 0,
                first: new[] {
                    Reward(CurrencyType.StarDust,    37000),
                    Reward(CurrencyType.RainbowOrb,  252),
                    Reward(CurrencyType.MagicCrystal,26),
                    Reward(CurrencyType.Starlight,   105),
                },
                repeat: new[] {
                    // 반복은 첫의 50% (§3.5)
                    Reward(CurrencyType.StarDust,    18500),
                    Reward(CurrencyType.RainbowOrb,  126),
                    Reward(CurrencyType.MagicCrystal,13),
                    Reward(CurrencyType.Starlight,   52),
                }
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            int n = AssetDatabase.FindAssets("t:EndgameModeData", new[] { DIR }).Length;
            Debug.Log($"[EndgameModeSeed] {n} / 4");
        }

        private static RewardEntry Reward(CurrencyType c, int amount, int scale = 0)
            => new RewardEntry { currency = c, amount = amount, chapterScaleMul = scale };

        private static void MakeMode(string id, string displayName, ModeType type, int cooldown,
            RewardEntry[] first, RewardEntry[] repeat)
        {
            string path = $"{DIR}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<EndgameModeData>(path) ?? ScriptableObject.CreateInstance<EndgameModeData>();
            so.id = id;
            so.displayName = displayName;
            so.type = type;
            so.cooldownSeconds = cooldown;
            so.firstClearRewards = first;
            so.repeatRewards = repeat;
            if (so.bossPool == null) so.bossPool = new EnemyData[0];
            if (so.depthScaling == null || so.depthScaling.length == 0)
                so.depthScaling = AnimationCurve.Linear(1f, 1f, 100f, 10f);
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
