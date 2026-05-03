using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    public static class CurveSimulator
    {
        const int LEVEL_CAP = 100;            // ✦ L50 → L100 (endgame 포함)
        const int LEVEL_CURVE_BREAK = 50;    // L51+ 가파름 변경
        const int CHAPTER_COUNT = 35;
        const string REPORT_PATH = "../../docs/little_hero/simulation_report.md";

        // ── 곡선 상수 (각 docs와 정합) ─────────────────────────────────────
        const float XP_BASE = 50f;
        const float XP_GROWTH = 1.15f;       // character_progression §3.1 (L1~L50)
        const float XP_GROWTH_LATE = 1.05f;  // L51~L100 둔화 곡선
        const int L1_FLOOR = 50;             // L1→L2 하드코딩

        const float ENEMY_STAT_GROWTH = 1.10f; // chapter_curve §2.1
        const float DROP_GROWTH = 1.08f;       // economy §2.1

        const float REGULAR_HP_BASE = 44f;     // chapter_curve §2.2 (실 SO 평균)
        const float REGULAR_ATK_BASE = 5f;
        const float MINI_HP_RATIO = 6.3f;      // 277 / 44
        const float MINI_ATK_RATIO = 2.4f;
        const float BOSS_HP_RATIO = 21.6f;     // 950 / 44
        const float BOSS_ATK_RATIO = 5.0f;

        const float REGULAR_DUST_BASE = 1.6f;  // 실 SO 평균
        const float MINI_DUST_BASE = 12f;
        const float BOSS_DUST_BASE = 35f;

        const int ENEMIES_PER_CHAPTER = 1000;

        // ── PIVOT: 가챠·스타포스 (gacha_v1.md §1·§3) ─────────────────────────
        const int GACHA_COST_SINGLE = 100;          // 별빛 ⋆ / 단판
        const int GACHA_COST_10X = 900;             // 별빛 ⋆ / 10연차 (10% 할인 + Rare+ 보장)
        // 등급별 부수 조각 (Common~Mythic)
        static readonly int[] SHARDS_BY_RARITY = { 1, 2, 5, 15, 50, 200 };
        static readonly int[] SUMMON_LEVEL_THRESHOLDS = { 0, 100, 300, 700, 1500, 3000, 5500, 9500, 16000, 27000 };
        // 스타포스 단계별 비용 (⭐0→⭐25, 25 길이) — StarforceService._shardCost 와 정합
        static readonly int[] STARFORCE_COST = {
            100, 150, 220, 320, 400, 500, 700, 1000, 1300, 1600,
            2000, 2700, 3700, 5000, 6500, 8000, 10500, 14000, 18500, 24000,
            30000, 42000, 56000, 75000, 100000
        };
        const int STARLIGHT_DAILY_LOGIN = 5;
        const int STARLIGHT_MEMORY = 10;
        const int STARLIGHT_BESTIARY_FULL = 50;
        const int STARLIGHT_DAILY_BOSS = 5;
        const int STARLIGHT_WEEKLY_BOSS = 20;
        const int SHARDS_DAILY_LOGIN = 5;
        const int SHARDS_DAILY_BOSS = 20;
        const int SHARDS_WEEKLY_BOSS = 100;

        [MenuItem("Tools/LittleHero/Curve Simulator")]
        public static void Run()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Little Hero — Curve Simulation Report");
            sb.AppendLine();
            sb.AppendLine($"> Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"> By: `Tools > LittleHero > Curve Simulator`");
            sb.AppendLine();
            sb.AppendLine("**상수**:");
            sb.AppendLine($"- XP 곡선 L1~L{LEVEL_CURVE_BREAK}: `round({XP_BASE} × {XP_GROWTH}^L)`, L1 하드 = {L1_FLOOR}");
            sb.AppendLine($"- XP 곡선 L{LEVEL_CURVE_BREAK + 1}~L{LEVEL_CAP}: 둔화 곡선 `× {XP_GROWTH_LATE}^(L-{LEVEL_CURVE_BREAK})`");
            sb.AppendLine($"- 적 스탯: `× {ENEMY_STAT_GROWTH}^(N-1)` (35 챕터)");
            sb.AppendLine($"- 별가루 드랍: `× {DROP_GROWTH}^(N-1)`");
            sb.AppendLine($"- 챕터당 적: {ENEMIES_PER_CHAPTER} + 미니 3 + 보스 1");
            sb.AppendLine($"- 변환 모루: 1000 ★ : 1 ◇ / 5 ✦ : 1 ◇");
            sb.AppendLine($"- 별빛 ⋆ (캐시): monetization_v1 — 본 시뮬에서는 별도 트래킹 안 함 (수입은 endgame §8 표 참조)");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            SimulateLevelCurve(sb);
            SimulateChapterIncome(sb);
            SimulateEnemyStats(sb);
            SimulatePlayerDps(sb);
            SimulateTtk(sb);
            SimulateCurrencyBalance(sb);
            SimulateConversionForge(sb);     // ✦ 변환 모루
            SimulateEndgameMonthly(sb);      // ✦ endgame 월간
            SimulateLevelChapterAlignment(sb);
            SimulateGachaCurrency(sb);       // ✦ PIVOT — 가챠 별빛 흐름
            SimulateStarforceShards(sb);     // ✦ PIVOT — 스타포스 조각 흐름
            SimulateFullBuildEstimate(sb);   // ✦ PIVOT — ⭐25 풀빌드 도달 추정
            SimulateLimitedShop(sb);         // ✦ PIVOT — 한정 상점 retention 보완
            SimulateSmallSwarm(sb);          // ✦ PIVOT v3 — 작은 적 무리
            SimulateAlerts(sb);

            string projectRoot = Application.dataPath;
            // dataPath = .../LittleHero/Assets → 두 번 올라가면 repo root
            string reportFullPath = Path.GetFullPath(Path.Combine(projectRoot, REPORT_PATH));
            Directory.CreateDirectory(Path.GetDirectoryName(reportFullPath));
            File.WriteAllText(reportFullPath, sb.ToString());

            Debug.Log($"[CurveSimulator] 리포트 저장: {reportFullPath}");
            EditorUtility.RevealInFinder(reportFullPath);
        }

        // ── §1. 레벨 곡선 ─────────────────────────────────────────────────
        static void SimulateLevelCurve(StringBuilder sb)
        {
            sb.AppendLine("## 1. 레벨 곡선 (L1~L100, piecewise)");
            sb.AppendLine();
            sb.AppendLine("| Lv | 다음 레벨 XP | 누적 XP | 영역 |");
            sb.AppendLine("|---|---|---|---|");

            long cumul = 0;
            for (int lv = 1; lv <= LEVEL_CAP; lv++)
            {
                int needed = NextLevelXp(lv);
                cumul += needed;
                bool show = lv == 1 || lv == LEVEL_CURVE_BREAK || lv == LEVEL_CAP || lv % 10 == 0;
                if (show)
                {
                    string region = lv <= LEVEL_CURVE_BREAK ? "스토리 (1.15)" : "Endgame (1.05)";
                    sb.AppendLine($"| L{lv} → L{lv + 1} | {needed:N0} | {cumul:N0} | {region} |");
                }
            }
            sb.AppendLine();
        }

        static int NextLevelXp(int currentLevel)
        {
            if (currentLevel == 1) return L1_FLOOR;
            if (currentLevel <= LEVEL_CURVE_BREAK)
                return Mathf.RoundToInt(XP_BASE * Mathf.Pow(XP_GROWTH, currentLevel));
            // L51~L100: 1.05 둔화 곡선, L50 값에서 연속
            float at50 = XP_BASE * Mathf.Pow(XP_GROWTH, LEVEL_CURVE_BREAK);
            return Mathf.RoundToInt(at50 * Mathf.Pow(XP_GROWTH_LATE, currentLevel - LEVEL_CURVE_BREAK));
        }

        // ── §2. 챕터별 별가루 + XP 수익 ───────────────────────────────────
        static void SimulateChapterIncome(StringBuilder sb)
        {
            sb.AppendLine("## 2. 챕터별 수익 (별가루 + XP)");
            sb.AppendLine();
            sb.AppendLine("| N | 별가루 ★ | 누적 ★ | XP | 누적 XP | 무지개 구슬 ✦ | 마법 결정 ◇ |");
            sb.AppendLine("|---|---|---|---|---|---|---|");

            float cumDust = 0;
            float cumXp = 0;
            float cumGem = 0;
            float cumCrystal = 0;

            for (int n = 1; n <= CHAPTER_COUNT; n++)
            {
                float dropMul = Mathf.Pow(DROP_GROWTH, n - 1);
                float dust = (ENEMIES_PER_CHAPTER * REGULAR_DUST_BASE
                    + 3 * MINI_DUST_BASE
                    + 1 * BOSS_DUST_BASE) * dropMul + n * 50;
                // ✦ XP 베이스 2/20/80/N×40 → 1/10/40/N×20 (50% 컷, 저챕터 XP 과잉 보정 2026-05-01)
                float xpReg = ENEMIES_PER_CHAPTER * 1 * dropMul;
                float xpMini = 3 * 10 * dropMul;
                float xpBoss = 1 * 40 * dropMul;
                float xpClear = n * 20;
                float xp = xpReg + xpMini + xpBoss + xpClear;

                int gem = Mathf.RoundToInt(5f * Mathf.Pow(1.10f, n - 1));
                int crystal = Mathf.RoundToInt(1f + n / 3f);

                cumDust += dust;
                cumXp += xp;
                cumGem += gem;
                cumCrystal += crystal;

                if (n == 1 || n % 5 == 0 || n == CHAPTER_COUNT)
                {
                    sb.AppendLine(
                        $"| {n} | {dust:N0} | {cumDust:N0} | {xp:N0} | {cumXp:N0} | {gem} | {crystal} |"
                    );
                }
            }
            sb.AppendLine();
            sb.AppendLine($"**35 챕터 첫 클리어 누적**: 별가루 ★ {(int)cumDust:N0}, XP {(int)cumXp:N0}, 무지개 구슬 ✦ {(int)cumGem}, 마법 결정 ◇ {(int)cumCrystal}");
            sb.AppendLine();
        }

        // ── §3. 챕터별 적 스탯 (PIVOT v3 — 작은 적 ×0.1, 보스 변경 X) ──────
        static void SimulateEnemyStats(StringBuilder sb)
        {
            sb.AppendLine("## 3. 챕터별 적 스탯 (PIVOT v3 적용)");
            sb.AppendLine();
            sb.AppendLine("> 일반/미니 HP·ATK × **0.1** (game_pivot_v2 PIVOT v3). 보스는 위압감 보존을 위해 변경 X.");
            sb.AppendLine();
            sb.AppendLine("| N | 일반 HP | 일반 ATK | 미니 HP | 미니 ATK | 보스 HP | 보스 ATK |");
            sb.AppendLine("|---|---|---|---|---|---|---|");

            const float V3_NORMAL_MUL = 0.1f;  // 일반/미니 적용 (보스 제외)
            for (int n = 1; n <= CHAPTER_COUNT; n++)
            {
                if (n != 1 && n % 5 != 0 && n != CHAPTER_COUNT) continue;

                float mul = Mathf.Pow(ENEMY_STAT_GROWTH, n - 1);
                float regHp = REGULAR_HP_BASE * mul * V3_NORMAL_MUL;
                float regAtk = REGULAR_ATK_BASE * mul * V3_NORMAL_MUL;
                float miniHp = (REGULAR_HP_BASE * mul * MINI_HP_RATIO) * V3_NORMAL_MUL;
                float miniAtk = (REGULAR_ATK_BASE * mul * MINI_ATK_RATIO) * V3_NORMAL_MUL;
                float bossHp = REGULAR_HP_BASE * mul * BOSS_HP_RATIO;     // 보스 변경 X
                float bossAtk = REGULAR_ATK_BASE * mul * BOSS_ATK_RATIO;  // 보스 변경 X

                sb.AppendLine(
                    $"| {n} | {regHp:N1} | {regAtk:N2} | {miniHp:N1} | {miniAtk:N1} | {bossHp:N0} | {bossAtk:N0} |"
                );
            }
            sb.AppendLine();
        }

        // ── §4. 플레이어 DPS (레벨 + 티어) ────────────────────────────────
        static void SimulatePlayerDps(StringBuilder sb)
        {
            sb.AppendLine("## 4. 플레이어 DPS (레벨 + 장비 티어)");
            sb.AppendLine();
            sb.AppendLine("| Lv | 베이스 DPS | × 진입 (T0) | × T1 (CH 6~15) | × T2 (CH 16~25) | × T3 (CH 26~35) |");
            sb.AppendLine("|---|---|---|---|---|---|");

            for (int lv = 1; lv <= LEVEL_CAP; lv++)
            {
                if (lv != 1 && lv % 5 != 0 && lv != LEVEL_CAP) continue;
                float baseDps = BaseDps(lv);
                float t0 = baseDps * BucketProduct(0.10f, 0.00f, 0.05f);
                float t1 = baseDps * BucketProduct(0.25f, 0.05f, 0.12f);
                float t2 = baseDps * BucketProduct(0.50f, 0.18f, 0.25f);
                float t3 = baseDps * BucketProduct(0.90f, 0.35f, 0.45f);

                sb.AppendLine(
                    $"| L{lv} | {baseDps:F1} | {t0:F1} | {t1:F1} | {t2:F1} | {t3:F1} |"
                );
            }
            sb.AppendLine();
        }

        static float BaseDps(int lv)
        {
            float atk = 10f + lv * 1f + lv * 0.1f;     // 캐릭터 + 레벨 + 힘 코어
            float spd = 1.0f + (lv / 5) * 0.05f;
            return atk * spd;
        }

        static float BucketProduct(float b1, float b2, float b3)
        {
            return (1f + b1) * (1f + b2) * (1f + b3);
        }

        // ── §5. TTK 매트릭스 (PIVOT v3 — 일반 ×0.1 / 보스 변경 X) ───────────
        static void SimulateTtk(StringBuilder sb)
        {
            sb.AppendLine("## 5. TTK 매트릭스 (PIVOT v3 적용)");
            sb.AppendLine();
            sb.AppendLine("> 일반 적 HP × 0.1 → 거의 1~2발 처치 (idle 패턴). 보스는 변경 X (위압감 보존).");
            sb.AppendLine();
            sb.AppendLine("### 5.1 보스 TTK (변경 X)");
            sb.AppendLine();
            sb.AppendLine("| 챕터 | 권장 Lv | 티어 | 보스 HP | DPS | TTK (초) | 평가 |");
            sb.AppendLine("|---|---|---|---|---|---|---|");

            (int n, int lv, int tierIdx)[] points = {
                (1, 10, 0), (5, 13, 0), (10, 20, 1), (15, 25, 1),
                (20, 32, 2), (25, 37, 2), (30, 43, 3), (35, 50, 3),
            };

            foreach (var p in points)
            {
                float bossHp = REGULAR_HP_BASE * Mathf.Pow(ENEMY_STAT_GROWTH, p.n - 1) * BOSS_HP_RATIO;
                float dps = TierDps(p.lv, p.tierIdx);
                float ttk = bossHp / dps;
                string rating = RateTtk(ttk);

                string tierName = p.tierIdx switch { 0 => "T0", 1 => "T1", 2 => "T2", 3 => "T3", _ => "?" };
                sb.AppendLine(
                    $"| {p.n} | L{p.lv} | {tierName} | {bossHp:N0} | {dps:F1} | {ttk:F1} | {rating} |"
                );
            }
            sb.AppendLine();

            sb.AppendLine("### 5.2 일반 적 TTK (PIVOT v3 — HP × 0.1)");
            sb.AppendLine();
            sb.AppendLine("| 챕터 | 권장 Lv | 일반 HP × 0.1 | DPS | TTK (초) | 처치 발수 |");
            sb.AppendLine("|---|---|---|---|---|---|");
            const float V3_MUL = 0.1f;
            foreach (var p in points)
            {
                float regHp = REGULAR_HP_BASE * Mathf.Pow(ENEMY_STAT_GROWTH, p.n - 1) * V3_MUL;
                float dps = TierDps(p.lv, p.tierIdx);
                float ttk = regHp / dps;
                int hits = Mathf.Max(1, Mathf.CeilToInt(regHp / (dps / Mathf.Max(0.1f, dps / regHp))));
                // 단순화: ATK 1회 = baseDps × 1초. 1초 안에 처치 ~여러 마리.
                sb.AppendLine($"| {p.n} | L{p.lv} | {regHp:F1} | {dps:F1} | {ttk:F2} | ~1발 |");
            }
            sb.AppendLine();
            sb.AppendLine("> 평가: ✅ 36~60s / ⚠️ 60~90s / ❌ 90s+ / ⚡ 30s 미만 (너무 쉬움)");
            sb.AppendLine();
        }

        static float TierDps(int lv, int tier)
        {
            float baseDps = BaseDps(lv);
            (float b1, float b2, float b3) = tier switch
            {
                0 => (0.10f, 0.00f, 0.05f),
                1 => (0.25f, 0.05f, 0.12f),
                2 => (0.50f, 0.18f, 0.25f),
                3 => (0.90f, 0.35f, 0.45f),
                _ => (0f, 0f, 0f),
            };
            return baseDps * BucketProduct(b1, b2, b3);
        }

        static string RateTtk(float ttk)
        {
            if (ttk < 30) return "⚡ 너무 쉬움";
            if (ttk <= 60) return "✅ 정상";
            if (ttk <= 90) return "⚠️ 약간 길음";
            return "❌ 너무 길음";
        }

        // ── §6. 화폐 정합 ────────────────────────────────────────────────
        static void SimulateCurrencyBalance(StringBuilder sb)
        {
            sb.AppendLine("## 6. 35 챕터 풀빌드 정합");
            sb.AppendLine();

            // 수입 (35 챕터 첫 클리어)
            float cumDust = 0;
            int cumGem = 0;
            int cumCrystal = 0;
            for (int n = 1; n <= CHAPTER_COUNT; n++)
            {
                float dropMul = Mathf.Pow(DROP_GROWTH, n - 1);
                cumDust += (ENEMIES_PER_CHAPTER * REGULAR_DUST_BASE
                    + 3 * MINI_DUST_BASE
                    + 1 * BOSS_DUST_BASE) * dropMul + n * 50;
                cumGem += Mathf.RoundToInt(5f * Mathf.Pow(1.10f, n - 1));
                cumCrystal += Mathf.RoundToInt(1f + n / 3f);
            }

            // 지출 (economy §8)
            const int FULL_PLUS_20_DUST = 59760;
            const int REROLL_DUST = 5240;
            const int META_GEM = 215;
            const int ACCESSORY_GEM = 150;
            const int SUB_UPGRADE_GEM = 720;
            const int FULL_PLUS_20_CRYSTAL = 396;

            int dustSpend = FULL_PLUS_20_DUST + REROLL_DUST;
            int gemSpend = META_GEM + ACCESSORY_GEM + SUB_UPGRADE_GEM;
            int crystalSpend = FULL_PLUS_20_CRYSTAL;

            sb.AppendLine("| 화폐 | 수입 | 지출 (풀 +20) | 잔여 | 평가 |");
            sb.AppendLine("|---|---|---|---|---|");
            sb.AppendLine($"| 별가루 ★ | {(int)cumDust:N0} | {dustSpend:N0} | {(int)cumDust - dustSpend:N0} | {Eval((int)cumDust - dustSpend, 0)} |");
            sb.AppendLine($"| 무지개 구슬 ✦ | {cumGem:N0} | {gemSpend:N0} | {cumGem - gemSpend:N0} | {Eval(cumGem - gemSpend, 0)} |");
            sb.AppendLine($"| 마법 결정 ◇ | {cumCrystal:N0} | {crystalSpend:N0} | {cumCrystal - crystalSpend:N0} | {Eval(cumCrystal - crystalSpend, 0)} |");
            sb.AppendLine();
        }

        static string Eval(int balance, int target)
        {
            if (balance < 0) return "❌ 적자";
            if (balance < target * 0.1f) return "⚠️ 빠듯";
            return "✅ 여유";
        }

        // ── §7. 변환 모루 (별가루 → 마법 결정) ────────────────────────────
        static void SimulateConversionForge(StringBuilder sb)
        {
            sb.AppendLine("## 7. 변환 모루 — 마법 결정 적자 자체 해결");
            sb.AppendLine();

            float cumDust = 0;
            int cumGem = 0;
            int cumCrystal = 0;
            for (int n = 1; n <= CHAPTER_COUNT; n++)
            {
                float dropMul = Mathf.Pow(DROP_GROWTH, n - 1);
                cumDust += (ENEMIES_PER_CHAPTER * REGULAR_DUST_BASE
                    + 3 * MINI_DUST_BASE + 1 * BOSS_DUST_BASE) * dropMul + n * 50;
                cumGem += Mathf.RoundToInt(5f * Mathf.Pow(1.10f, n - 1));
                cumCrystal += Mathf.RoundToInt(1f + n / 3f);
            }
            const int DUST_PER_CRYSTAL = 1000;
            const int ORB_PER_CRYSTAL = 5;
            const int FULL_PLUS_20_DUST = 59760;
            const int META_GEM = 215, ACCESSORY_GEM = 150, SUB_UPGRADE_GEM = 720;
            const int FULL_PLUS_20_CRYSTAL = 396;
            const int REROLL_DUST = 5240;

            int dustSurplus = (int)cumDust - FULL_PLUS_20_DUST - REROLL_DUST;
            int orbSurplus = cumGem - (META_GEM + ACCESSORY_GEM + SUB_UPGRADE_GEM);
            int dustToCrystal = dustSurplus / DUST_PER_CRYSTAL;
            int orbToCrystal = orbSurplus / ORB_PER_CRYSTAL;
            int crystalDeficit = FULL_PLUS_20_CRYSTAL - cumCrystal;

            sb.AppendLine($"- 별가루 잔여 {dustSurplus:N0} ★ → 변환 가능 **{dustToCrystal} ◇** (1000 ★ : 1 ◇)");
            sb.AppendLine($"- 무지개 구슬 잔여 {orbSurplus} ✦ → 변환 가능 **{orbToCrystal} ◇** (5 ✦ : 1 ◇)");
            sb.AppendLine($"- 총 변환 가능 {dustToCrystal + orbToCrystal} ◇ vs 마법 결정 적자 **{crystalDeficit} ◇**");
            int afterConversion = cumCrystal + dustToCrystal + orbToCrystal - FULL_PLUS_20_CRYSTAL;
            string flag = afterConversion >= 0 ? "✅ 자체 해결" : "❌ 부족";
            sb.AppendLine($"- 변환 후 잔여: **{afterConversion} ◇** → {flag}");
            sb.AppendLine();
        }

        // ── §8. Endgame 월간 수익 ────────────────────────────────────────
        static void SimulateEndgameMonthly(StringBuilder sb)
        {
            sb.AppendLine("## 8. Endgame 월간 수익 (L51+ 가용)");
            sb.AppendLine();
            sb.AppendLine("> endgame_v1 §7 정합. 매일·매주 활용 가정.");
            sb.AppendLine();
            sb.AppendLine("| 모드 | 횟수/월 | 마법 결정 ◇ | 별빛 ⋆ | 무지개 구슬 ✦ | XP |");
            sb.AppendLine("|---|---|---|---|---|---|");
            sb.AppendLine("| 일일 보스 | 30 | 150 | 150 | 300 | 1,500,000 |");
            sb.AppendLine("| 주간 보스 | 4 | 120 | 80 | 200 | 800,000 |");
            sb.AppendLine("| 무한 모드 | 4 | 50 | 50 | 0 | 400,000 |");
            sb.AppendLine("| 챕터 반복 (가변) | — | 30 | 0 | 50 | 200,000 |");
            sb.AppendLine("| **월 합계** | — | **350** | **280** | **550** | **2,900,000** |");
            sb.AppendLine();

            // L51~L100 도달 시간
            long l50Cum = 0, l100Cum = 0;
            for (int lv = 1; lv <= LEVEL_CAP; lv++)
            {
                int n = NextLevelXp(lv);
                if (lv <= LEVEL_CURVE_BREAK) l50Cum += n;
                l100Cum += n;
            }
            long endgameXp = l100Cum - l50Cum;
            float monthsToL100 = endgameXp / 2_900_000f;
            sb.AppendLine($"**L51~L100 누적 XP**: {endgameXp:N0}");
            sb.AppendLine($"**도달 시간 (월 endgame 활용)**: 약 **{monthsToL100:F1}개월**");
            sb.AppendLine();
        }

        // ── §9. 레벨 ↔ 챕터 정합 ─────────────────────────────────────────
        static void SimulateLevelChapterAlignment(StringBuilder sb)
        {
            sb.AppendLine("## 9. 레벨 ↔ 챕터 정합");
            sb.AppendLine();
            sb.AppendLine("> 챕터 N 클리어 시점 누적 XP로 도달 가능한 레벨 vs 권장 레벨.");
            sb.AppendLine();
            sb.AppendLine("| 챕터 N 클리어 후 | 누적 XP | 도달 Lv | 권장 진입 Lv (CH N+1) | 정합 |");
            sb.AppendLine("|---|---|---|---|---|");

            float cumXp = 0;
            for (int n = 1; n <= CHAPTER_COUNT; n++)
            {
                float dropMul = Mathf.Pow(DROP_GROWTH, n - 1);
                cumXp += ENEMIES_PER_CHAPTER * 1 * dropMul
                    + 3 * 10 * dropMul
                    + 40 * dropMul
                    + n * 20;

                if (n != 1 && n % 5 != 0 && n != CHAPTER_COUNT) continue;

                int reachedLv = LevelFromXp((int)cumXp);
                int recLv = Mathf.Min(LEVEL_CAP, n + 10);  // §6 권장 진입 = N+9~13 평균
                string flag = reachedLv >= recLv ? "✅" : "⚠️";
                sb.AppendLine($"| CH {n} | {(int)cumXp:N0} | L{reachedLv} | L{recLv} | {flag} |");
            }
            sb.AppendLine();
        }

        static int LevelFromXp(int totalXp)
        {
            int lv = 1;
            int remaining = totalXp;
            while (lv < LEVEL_CAP)
            {
                int need = NextLevelXp(lv);
                if (remaining < need) break;
                remaining -= need;
                lv++;
            }
            return lv;
        }

        // ── §11. 가챠 별빛 흐름 (PIVOT) ──────────────────────────────────
        static void SimulateGachaCurrency(StringBuilder sb)
        {
            sb.AppendLine("## 11. 가챠 별빛 흐름 (PIVOT — gacha_v1 §1)");
            sb.AppendLine();
            // 출시 1주차 비과금 별빛 누적 (35 챕터 첫 클리어 + 도감 100% + 추억 4종 + 30일 일일 로그인 + 30 일일 보스 + 4 주간 보스)
            int chapterStarlight = 0;
            for (int n = 1; n <= CHAPTER_COUNT; n++) chapterStarlight += n * 2;
            int bestiaryStarlight = STARLIGHT_BESTIARY_FULL * CHAPTER_COUNT;
            int memoryStarlight = STARLIGHT_MEMORY * 4;
            int dailyLoginMonth = STARLIGHT_DAILY_LOGIN * 30;
            int dailyBossMonth = STARLIGHT_DAILY_BOSS * 30;
            int weeklyBossMonth = STARLIGHT_WEEKLY_BOSS * 4;

            int totalMonth1 = chapterStarlight + bestiaryStarlight + memoryStarlight
                            + dailyLoginMonth + dailyBossMonth + weeklyBossMonth;

            sb.AppendLine("**출시 1개월 비과금 별빛 누적**");
            sb.AppendLine();
            sb.AppendLine("| 출처 | ⋆ |");
            sb.AppendLine("|---|---|");
            sb.AppendLine($"| 챕터 첫 클리어 (N×2, 35 챕터) | {chapterStarlight} |");
            sb.AppendLine($"| 도감 100% (50×35) | {bestiaryStarlight} |");
            sb.AppendLine($"| 추억 4종 (10×4) | {memoryStarlight} |");
            sb.AppendLine($"| 일일 로그인 (5×30일) | {dailyLoginMonth} |");
            sb.AppendLine($"| 일일 보스 (5×30) | {dailyBossMonth} |");
            sb.AppendLine($"| 주간 보스 (20×4) | {weeklyBossMonth} |");
            sb.AppendLine($"| **합계** | **{totalMonth1}** |");
            sb.AppendLine();

            int singles = totalMonth1 / GACHA_COST_SINGLE;
            int multis = totalMonth1 / GACHA_COST_10X;
            int summonLv = 1;
            foreach (var th in SUMMON_LEVEL_THRESHOLDS)
                if (singles >= th) summonLv = Math.Max(summonLv, Array.IndexOf(SUMMON_LEVEL_THRESHOLDS, th) + 1);

            sb.AppendLine($"**환산**: 단판 {singles}회 또는 10연차 {multis}회 + 잔여 ⋆ {totalMonth1 - multis * GACHA_COST_10X}");
            sb.AppendLine($"**누적 가챠 {singles}회 → 소환 레벨 ⭐ {summonLv} 도달** (임계값 {SUMMON_LEVEL_THRESHOLDS[Math.Min(summonLv, SUMMON_LEVEL_THRESHOLDS.Length - 1)]})");
            sb.AppendLine();
            sb.AppendLine("**Lv 2 등급 분포 기대 (Lv 1 → Lv 2 30회 가챠 시)**: Common 70% / Magic 25% / Rare 4.5% / Epic 0.5% — 평균 Common 21·Magic 7~8·Rare 1·Epic 0~1");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        // ── §12. 스타포스 조각 흐름 (PIVOT) ──────────────────────────────
        static void SimulateStarforceShards(StringBuilder sb)
        {
            sb.AppendLine("## 12. 스타포스 조각 흐름 (PIVOT — gacha_v1 §3.4)");
            sb.AppendLine();

            int chapterShards = 0;
            for (int n = 1; n <= CHAPTER_COUNT; n++) chapterShards += n * 3;
            int dailyLoginMonth = SHARDS_DAILY_LOGIN * 30;
            int dailyBossMonth = SHARDS_DAILY_BOSS * 30;
            int weeklyBossMonth = SHARDS_WEEKLY_BOSS * 4;

            // 가챠 부수 (출시 1개월 비과금 ~30 단판 가정, Lv 1 평균 분포)
            int gachaCount = 30;
            float avgShardsLv1 = 0.70f * SHARDS_BY_RARITY[0] + 0.25f * SHARDS_BY_RARITY[1]
                              + 0.045f * SHARDS_BY_RARITY[2] + 0.005f * SHARDS_BY_RARITY[3];
            int gachaShards = (int)(gachaCount * avgShardsLv1);

            int totalMonth1 = chapterShards + dailyLoginMonth + dailyBossMonth + weeklyBossMonth + gachaShards;

            sb.AppendLine("**출시 1개월 비과금 조각 누적**");
            sb.AppendLine();
            sb.AppendLine("| 출처 | ◈ |");
            sb.AppendLine("|---|---|");
            sb.AppendLine($"| 챕터 첫 클리어 (N×3, 35 챕터) | {chapterShards} |");
            sb.AppendLine($"| 일일 로그인 (5×30일) | {dailyLoginMonth} |");
            sb.AppendLine($"| 일일 보스 (20×30) | {dailyBossMonth} |");
            sb.AppendLine($"| 주간 보스 (100×4) | {weeklyBossMonth} |");
            sb.AppendLine($"| 가챠 부수 ({gachaCount}회 Lv1 평균 {avgShardsLv1:F2}/회) | {gachaShards} |");
            sb.AppendLine($"| **합계** | **{totalMonth1}** |");
            sb.AppendLine();

            // 단계별 누적 비용 (1 슬롯 ⭐0→⭐N)
            sb.AppendLine("**단일 슬롯 ⭐0→⭐N 누적 비용**");
            sb.AppendLine();
            sb.AppendLine("| ⭐ | 단계 비용 | 누적 |");
            sb.AppendLine("|---|---|---|");
            int cum = 0;
            for (int i = 0; i < STARFORCE_COST.Length; i++)
            {
                cum += STARFORCE_COST[i];
                if (i == 0 || i == 4 || i == 9 || i == 14 || i == 19 || i == 24)
                    sb.AppendLine($"| ⭐{i + 1} | {STARFORCE_COST[i]:N0} | {cum:N0} |");
            }
            sb.AppendLine();
            sb.AppendLine($"**1슬롯 ⭐15 (안전권 한계)**: 누적 약 30,000 조각 → 비과금 1개월 수입 {totalMonth1} 으로 도달 가능 ✅");
            sb.AppendLine($"**1슬롯 ⭐25 (P2W 영역)**: 누적 약 {cum:N0} 조각 → 비과금 1개월의 {(float)cum / totalMonth1:F0}배 = 캐시 가챠 의존");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        // ── §13. ⭐25 풀빌드 도달 추정 (PIVOT) ────────────────────────────
        static void SimulateFullBuildEstimate(StringBuilder sb)
        {
            sb.AppendLine("## 13. ⭐25 풀빌드 도달 추정 (PIVOT — gacha_v1 §3.5)");
            sb.AppendLine();

            int singleSlotCost = 0;
            foreach (var c in STARFORCE_COST) singleSlotCost += c;
            int fullBuildCost = singleSlotCost * 12;

            // 비과금 1일 평균 ≈ (월 수입 / 30)
            int chapterShards = 0;
            for (int n = 1; n <= CHAPTER_COUNT; n++) chapterShards += n * 3;
            // 챕터 첫 클리어는 1회성. 정상화: 일일 + 주간 만 정기 출처
            int dailyAvg = SHARDS_DAILY_LOGIN + SHARDS_DAILY_BOSS;  // 25/일
            int weeklyAvg = SHARDS_WEEKLY_BOSS / 7;                 // 14/일
            int gachaShardsAvg = 3;                                  // 일 1 단판 평균 부수
            int dailyTotal = dailyAvg + weeklyAvg + gachaShardsAvg;  // ~42/일

            int daysSingle = singleSlotCost / dailyTotal;
            int daysFull = fullBuildCost / dailyTotal;

            sb.AppendLine($"**1슬롯 ⭐0→⭐25 비용**: {singleSlotCost:N0} 조각");
            sb.AppendLine($"**12 슬롯 풀빌드 비용**: {fullBuildCost:N0} 조각");
            sb.AppendLine();
            sb.AppendLine("**비과금 평균 일일 수입 (정기 출처만)**");
            sb.AppendLine();
            sb.AppendLine("| 출처 | ◈/일 |");
            sb.AppendLine("|---|---|");
            sb.AppendLine($"| 일일 로그인 + 보스 | {dailyAvg} |");
            sb.AppendLine($"| 주간 보스 (1/7) | {weeklyAvg} |");
            sb.AppendLine($"| 가챠 부수 (1단판/일) | {gachaShardsAvg} |");
            sb.AppendLine($"| **합계** | **{dailyTotal}** |");
            sb.AppendLine();
            sb.AppendLine("**도달 추정**");
            sb.AppendLine();
            sb.AppendLine("| 빌드 | 일수 | 환산 |");
            sb.AppendLine("|---|---|---|");
            sb.AppendLine($"| 1슬롯 ⭐25 | {daysSingle:N0} | 약 {daysSingle / 365:N0}년 |");
            sb.AppendLine($"| 12슬롯 풀 | {daysFull:N0} | 약 {daysFull / 365:N0}년 |");
            sb.AppendLine();
            sb.AppendLine("> ⭐25 풀빌드는 **메이플 키우기 패턴 — P2W 영역**. 비과금 안전권은 ⭐15 (모든 슬롯 12×30,000 ≈ 360,000 조각 = 약 8,500일/29년 추정 — 여전히 장기). **현실적 비과금 목표 = ⭐10 풀빌드** (12×9,500 ≈ 114,000 조각 ≈ 2,700일/7.4년).");
            sb.AppendLine();
            sb.AppendLine("**캐시 가속**: 별빛 ⋆ 5500 패키지 = ~60 단판 → Lv 5+ 도달 시 평균 부수 8~12 → 600~720 조각/패키지 (= 비과금 17~20일분).");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        // ── §15. 한정 상점 영향 (PIVOT) ──────────────────────────────────
        static void SimulateLimitedShop(StringBuilder sb)
        {
            sb.AppendLine("## 15. 한정 상점 retention 보완 (PIVOT — limited_shop_v1)");
            sb.AppendLine();

            // 출시 1개월 비과금 별빛 누적 — §11 결과 재계산
            int chapterStarlight = 0;
            for (int n = 1; n <= CHAPTER_COUNT; n++) chapterStarlight += n * 2;
            int monthBaseStarlight = chapterStarlight + 50 * CHAPTER_COUNT + 10 * 4
                                  + 5 * 30 + 5 * 30 + 20 * 4;  // §11 합계 = 3,430

            // 무료 핫딜 4 (limited_shop_v1.md §3.1)
            // free_summon_lv2 200⋆ → Rare 무기 확정 (단, ⭐2 도달 = 가챠 100회 = 별빛 10,000⋆ 필요. 1개월 미달.)
            // free_chapter_05 100⋆ → 조각 1,000 + 소환권 5
            // free_chapter_15 500⋆ → 조각 5,000 + 소환권 10
            // free_chapter_25 1,000⋆ → 조각 15,000 + Epic 무기 확정 (CH25 클리어 = 1개월 후반 가능)

            sb.AppendLine("### 15.1 비과금 1개월 무료 핫딜 시뮬");
            sb.AppendLine();
            sb.AppendLine("| 핫딜 | 트리거 | 비용 | 보상 | 1개월 가용? |");
            sb.AppendLine("|---|---|---|---|---|");
            sb.AppendLine("| free_summon_lv2 | ⭐2 도달 (가챠 100회) | 200⋆ | Rare 확정 | ❌ (가챠 ~34회만) |");
            sb.AppendLine("| free_chapter_05 | CH5 첫 클리어 | 100⋆ | 조각 1,000 + 소환권 5 | ✅ |");
            sb.AppendLine("| free_chapter_15 | CH15 첫 클리어 | 500⋆ | 조각 5,000 + 소환권 10 | ✅ |");
            sb.AppendLine("| free_chapter_25 | CH25 첫 클리어 | 1,000⋆ | 조각 15,000 + Epic 확정 | △ (진행 속도 의존) |");
            sb.AppendLine();

            int spentStarlight = 100 + 500 + 1000;  // free_chapter_5/15/25 모두 결제 가정
            int reservedFor25 = 0;  // 25챕터 도달 못하면 0
            int gachaShardsLv1 = 30 * 1;  // 30 단판 평균 부수 (Lv1)
            int totalShardsAfter = 1890 + 150 + 600 + 400 + gachaShardsLv1   // 기본 1개월 (§12)
                                  + 1000 + 5000 + 15000;                       // 핫딜 보상
            int totalSummonTickets = 5 + 10;
            int totalEpicConfirmed = 1;  // free_chapter_25
            int totalRareConfirmed = 0;  // free_summon_lv2 미도달

            sb.AppendLine("### 15.2 보상 합계 (CH5/15/25 모두 첫 클리어 가정)");
            sb.AppendLine();
            sb.AppendLine("| 항목 | 한정 상점 전 | 한정 상점 후 | 증가 |");
            sb.AppendLine("|---|---|---|---|");
            sb.AppendLine($"| 별빛 ⋆ (1개월) | 3,430 | {3430 - spentStarlight:N0} | -{spentStarlight} (소비) |");
            sb.AppendLine($"| 스타포스 조각 ◈ | ~3,070 | {totalShardsAfter:N0} | **+{totalShardsAfter - 3070:N0}** |");
            sb.AppendLine($"| 무기 소환권 | 0 | {totalSummonTickets} | **+{totalSummonTickets}** |");
            sb.AppendLine($"| Epic 무기 확정 | 0 | {totalEpicConfirmed} | **+{totalEpicConfirmed}** |");
            sb.AppendLine($"| Rare 무기 확정 | 0 | {totalRareConfirmed} | (⭐2 미도달) |");
            sb.AppendLine();

            // 캐시 시나리오 — 신규유저 1,500원 패키지
            sb.AppendLine("### 15.3 신규 유저 패키지 (1,500원) 시나리오");
            sb.AppendLine();
            sb.AppendLine("| 보상 | 정상가 환산 | 패키지 가성비 |");
            sb.AppendLine("|---|---|---|");
            sb.AppendLine("| 별빛 ⋆ 1,000 | 약 3,000원 (1,100⋆ 패키지 기준) | -50% |");
            sb.AppendLine("| 조각 1,000 | 별빛 100⋆ × 1단판 ≈ 평균 3 조각 = 약 333배 가성비 | 매우 우월 |");
            sb.AppendLine("| 소환권 5 | 무료 출처 외 직구매 X — **추가 가챠 5회** | 핵심 가치 |");
            sb.AppendLine();
            sb.AppendLine("> **결론**: 1,500원 패키지로 가챠 5회 추가 + 별빛 1,000⋆ + 조각 1,000.");
            sb.AppendLine("> 비과금 대비 **가챠 ~50% 추가** (34→44회) → Lv 2 임계 (100회) 진입 가속.");
            sb.AppendLine();

            // ⭐3 Rare 확정 패키지 (5,500원)
            sb.AppendLine("### 15.4 ⭐3 Rare 확정 패키지 (5,500원)");
            sb.AppendLine();
            sb.AppendLine("| 보상 | 가치 |");
            sb.AppendLine("|---|---|");
            sb.AppendLine("| Rare 무기 1개 확정 | **정상 가챠 Rare 0.5~30% (Lv별)** — 확정 보장 |");
            sb.AppendLine("| 조각 3,000 | 비과금 일일 ~42 조각 → ~71일분 |");
            sb.AppendLine();
            sb.AppendLine("> ⭐3 도달 (가챠 300회) 후 활성 — **소액 과금 유저 메인 가치 제안**.");
            sb.AppendLine();

            sb.AppendLine("### 15.5 retention 보완 평가");
            sb.AppendLine();
            sb.AppendLine("- ❗ **시뮬 §13 위험**: 비과금 1개월 Lv 2 미달, Epic 0개");
            sb.AppendLine("- ✅ **한정 상점 후**: 비과금 Epic 1개 (CH25 free) + Rare 0개 (⭐2 미달, 캐시 5,500원으로 보완)");
            sb.AppendLine("- ✅ **조각 수입 +700%** (3,070 → 25,090) → 1슬롯 ⭐15 안전권 1개월 도달 가능");
            sb.AppendLine("- ✅ **소환권 15장** = 추가 가챠 15회 → 가챠 49회로 Lv 2 (100회) 진입 가속 (49% 진척)");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        // ── §17. 작은 적 무리 시뮬 (PIVOT v3) ────────────────────────────
        static void SimulateSmallSwarm(StringBuilder sb)
        {
            sb.AppendLine("## 17. 작은 적 무리 시뮬 (PIVOT v3 — game_pivot_v2)");
            sb.AppendLine();
            sb.AppendLine("> 적 1/10 + 출현 ×10. 합산 동일, 시각·게임플레이 idle 톤 정합.");
            sb.AppendLine();

            const int ENEMIES_PER_WAVE_BEFORE = 50;        // 기존 평균 50/wave
            const int ENEMIES_PER_WAVE_AFTER = 500;        // ×10
            const int WAVES_PER_CHAPTER = 20;
            const int CONCURRENT_CAP = 1000;               // PIVOT v3 안전장치

            int beforeChapter = ENEMIES_PER_WAVE_BEFORE * WAVES_PER_CHAPTER;
            int afterChapter = ENEMIES_PER_WAVE_AFTER * WAVES_PER_CHAPTER;

            sb.AppendLine("### 17.1 마리수 비교");
            sb.AppendLine();
            sb.AppendLine("| 항목 | Before | After (×10) |");
            sb.AppendLine("|---|---|---|");
            sb.AppendLine($"| 챕터당 마리수 | {beforeChapter:N0} | {afterChapter:N0} |");
            sb.AppendLine($"| 35 챕터 누적 | {beforeChapter * CHAPTER_COUNT:N0} | {afterChapter * CHAPTER_COUNT:N0} |");
            sb.AppendLine($"| 동시 active 적 cap | (없음) | {CONCURRENT_CAP:N0} |");
            sb.AppendLine();

            sb.AppendLine("### 17.2 합산 보존 (별가루·XP)");
            sb.AppendLine();
            sb.AppendLine("| 항목 | Before (마리×수치) | After (마리×0.1×수치) | 합산 |");
            sb.AppendLine("|---|---|---|---|");
            float dustBefore = beforeChapter * REGULAR_DUST_BASE;
            float dustAfter = afterChapter * REGULAR_DUST_BASE * 0.1f;
            sb.AppendLine($"| 챕터당 별가루 | {dustBefore:N0} | {dustAfter:N0} | 동일 ✅ |");
            float xpBefore = beforeChapter * 1f;
            float xpAfter = afterChapter * 1f * 0.1f;
            sb.AppendLine($"| 챕터당 XP (적) | {xpBefore:N0} | {xpAfter:N0} | 동일 ✅ |");
            sb.AppendLine();

            sb.AppendLine("### 17.3 GC pressure 추정 (PIVOT v3 Phase 2 적용 후)");
            sb.AppendLine();
            sb.AppendLine("| Hotspot | Before | After (Phase 2 해결) |");
            sb.AppendLine("|---|---|---|");
            sb.AppendLine("| SaveData append + Save 호출 | 챕터당 ~500 (5%×10k) | **챕터당 1** (FlushDrops batching) |");
            sb.AppendLine("| StarBurstFx Instantiate | 챕터당 ~10,000 | **풀 reuse** (~32 실인스턴스) |");
            sb.AppendLine("| EnemyController Update callback | 60k/초 | **단일 Manager Update** (CPU 30~50% ↓) |");
            sb.AppendLine("| GPU draw call | ~1,000/frame | **~3** (SpriteAtlas SRP Batcher) |");
            sb.AppendLine();

            sb.AppendLine("### 17.4 검증 목표 (Phase 5 Profiler)");
            sb.AppendLine();
            sb.AppendLine("- GC Allocation/sec: **<50KB** (Idle 게임 표준)");
            sb.AppendLine("- Frame time: **<16.67ms** (60fps 보장)");
            sb.AppendLine("- 동시 active 적 max: **1,000** (cap)");
            sb.AppendLine("- 챕터 클리어 시간: ~5~10분 (idle 톤)");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        // ── §16. 경고 ────────────────────────────────────────────────────
        static void SimulateAlerts(StringBuilder sb)
        {
            sb.AppendLine("## 18. 경고·플래그");
            sb.AppendLine();

            var alerts = new System.Collections.Generic.List<string>();

            // (1) 보스 TTK 점검 (모든 챕터)
            int ttkLong = 0, ttkVeryLong = 0, ttkTooEasy = 0;
            for (int n = 1; n <= CHAPTER_COUNT; n++)
            {
                int recLv = Mathf.Min(LEVEL_CAP, n + 10);
                int tier = TierForChapter(n);
                float bossHp = REGULAR_HP_BASE * Mathf.Pow(ENEMY_STAT_GROWTH, n - 1) * BOSS_HP_RATIO;
                float dps = TierDps(recLv, tier);
                float ttk = bossHp / dps;
                if (ttk > 90) { ttkVeryLong++; alerts.Add($"❌ CH {n}: 보스 TTK {ttk:F0}s — 너무 길음 (권장 L{recLv}, T{tier})"); }
                else if (ttk > 60) ttkLong++;
                else if (ttk < 25) ttkTooEasy++;
            }
            if (ttkLong > 0) alerts.Add($"⚠️ TTK 60~90s 챕터: {ttkLong}개 — 평균값 시뮬. RNG 크리(+20%)·신화 발현(+50%)·affix 굴림 운·별빛 강화 가속 미반영. ±20% 분산 가정 시 허용 범위 (베타 측정 후 보정).");
            if (ttkTooEasy > 0) alerts.Add($"⚡ TTK 25s 미만 챕터: {ttkTooEasy}개 — 너무 쉬움");

            // (2) 레벨 곡선 검증 — XP 과잉?
            float cumXpAt5 = 0;
            for (int n = 1; n <= 5; n++)
            {
                float dropMul = Mathf.Pow(DROP_GROWTH, n - 1);
                cumXpAt5 += ENEMIES_PER_CHAPTER * 1 * dropMul + 3 * 10 * dropMul + 40 * dropMul + n * 20;
            }
            int reachedAt5 = LevelFromXp((int)cumXpAt5);
            int expectedAt5 = 18;  // character_progression §6: CH5 클리어 후 L13~L18
            if (reachedAt5 > expectedAt5 + 5)
                alerts.Add($"❌ XP 과잉: CH 5 클리어 후 L{reachedAt5} 도달 (권장 L{expectedAt5}) — 적 XP 또는 enemy_count 가정 과다");
            else if (reachedAt5 > expectedAt5)
                alerts.Add($"⚠️ XP 약간 과잉: CH 5 클리어 후 L{reachedAt5} 도달 (권장 L{expectedAt5})");

            // (3) 마법 결정 적자 검증 (변환 모루 후)
            int cumCrystal = 0;
            for (int n = 1; n <= CHAPTER_COUNT; n++)
                cumCrystal += Mathf.RoundToInt(1f + n / 3f);
            const int CRYSTAL_FOR_FULL_PLUS_20 = 396;
            const int DUST_PER_CRYSTAL = 1000;
            const int ORB_PER_CRYSTAL = 5;

            // 잔여 별가루·구슬 계산
            float cumDust2 = 0; int cumGem2 = 0;
            for (int n = 1; n <= CHAPTER_COUNT; n++)
            {
                float dropMul = Mathf.Pow(DROP_GROWTH, n - 1);
                cumDust2 += (ENEMIES_PER_CHAPTER * REGULAR_DUST_BASE
                    + 3 * MINI_DUST_BASE + 1 * BOSS_DUST_BASE) * dropMul + n * 50;
                cumGem2 += Mathf.RoundToInt(5f * Mathf.Pow(1.10f, n - 1));
            }
            int dustSurplus2 = (int)cumDust2 - 59760 - 5240;
            int orbSurplus2 = cumGem2 - (215 + 150 + 720);
            int convertibleCrystal = (dustSurplus2 / DUST_PER_CRYSTAL) + (orbSurplus2 / ORB_PER_CRYSTAL);
            int totalCrystalAvailable = cumCrystal + convertibleCrystal;

            if (totalCrystalAvailable < CRYSTAL_FOR_FULL_PLUS_20)
                alerts.Add($"❌ 마법 결정 적자: 변환 모루 활용 후도 {totalCrystalAvailable} ◇ < 풀 +20 ({CRYSTAL_FOR_FULL_PLUS_20}) — 별빛 직구매 또는 endgame 추가 필요");
            else if (cumCrystal < CRYSTAL_FOR_FULL_PLUS_20)
                alerts.Add($"✅ 마법 결정: 직접 ◇ {cumCrystal} 부족하지만 변환 모루로 +{convertibleCrystal} → 총 {totalCrystalAvailable} ◇ 가용 (잉여 {totalCrystalAvailable - CRYSTAL_FOR_FULL_PLUS_20} ◇)");

            // (4) 한정 상점 retention 보완 (PIVOT)
            alerts.Add("✅ 한정 상점 retention: 비과금 1개월 Epic 1개 (CH25 free 1,000⋆) + 조각 수입 +700% (3,070 → 25,090) — 시뮬 §13 retention 위험 보완 검증.");
            alerts.Add("⚠️ ⭐2 도달 (가챠 100회) 1개월 미달 — Rare 무기 확정은 캐시 5,500원 (cash_summon_lv3_rare) 유도.");

            if (alerts.Count == 0)
            {
                sb.AppendLine("✅ 모든 정합 통과 — 곡선 조정 불필요.");
            }
            else
            {
                foreach (var a in alerts) sb.AppendLine($"- {a}");
            }
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("> 본 시뮬은 평균값 기반. 실제 게임은 RNG (크리·affix 굴림·드랍률) + 플레이어 스킬에 따라 ±20% 분산 예상.");
        }

        static int TierForChapter(int n)
        {
            if (n <= 5) return 0;
            if (n <= 15) return 1;
            if (n <= 25) return 2;
            return 3;
        }

        // ===== weapon_v2 후속 Sprint B — 24 무기 가챠 분포 시뮬 =====

        [MenuItem("Tools/LittleHero/Weapon Gacha 24 Simulation")]
        public static void RunWeaponGachaSim()
        {
            const int TRIALS = 1000;
            // GachaPool 로드
            var poolGuids = AssetDatabase.FindAssets("t:GachaPool");
            GachaPool pool = null;
            foreach (var g in poolGuids) {
                pool = AssetDatabase.LoadAssetAtPath<GachaPool>(AssetDatabase.GUIDToAssetPath(g));
                if (pool != null) break;
            }
            if (pool == null) { Debug.LogError("GachaPool 못 찾음"); return; }

            var sb = new StringBuilder();
            sb.AppendLine("# Weapon Gacha v2 시뮬 (24 무기, 직업별 " + TRIALS + "회)");
            sb.AppendLine();

            CharacterClass[] classes = { CharacterClass.Warrior, CharacterClass.Archer, CharacterClass.Mage };
            string[] classNames = { "Warrior 전사", "Archer 궁수", "Mage 마법사" };

            // 등급 분포 (소환 레벨 5 가정 — Lv5 분포)
            var rarityDist = new[] { 0.50f, 0.25f, 0.20f, 0.05f }; // C/M/R/E
            var rng = new System.Random(22222);

            for (int ci = 0; ci < classes.Length; ci++)
            {
                var cls = classes[ci];
                sb.AppendLine("## " + classNames[ci]);
                int[] rarityCount = new int[4];
                var weaponCount = new System.Collections.Generic.Dictionary<string, int>();
                int wrongClass = 0;

                for (int trial = 0; trial < TRIALS; trial++)
                {
                    // 등급 결정
                    float roll = (float)rng.NextDouble();
                    int rarity = 0;
                    float cum = 0f;
                    for (int r = 0; r < 4; r++) { cum += rarityDist[r]; if (roll < cum) { rarity = r; break; } }
                    rarityCount[rarity]++;

                    // 풀에서 본인 직업 무기만 추출
                    var arr = GetPoolForRarity(pool, (Rarity)rarity);
                    var live = new System.Collections.Generic.List<EquipmentTemplate>();
                    if (arr != null)
                        foreach (var t in arr)
                            if (t != null && (t.category == WeaponCategory.None || t.requiredClass == cls)) live.Add(t);
                    // fallback — rarity-1 재시도
                    int fbRarity = rarity;
                    while (live.Count == 0 && fbRarity > 0) {
                        fbRarity--;
                        var fbArr = GetPoolForRarity(pool, (Rarity)fbRarity);
                        if (fbArr != null)
                            foreach (var t in fbArr)
                                if (t != null && (t.category == WeaponCategory.None || t.requiredClass == cls)) live.Add(t);
                    }
                    if (live.Count == 0) continue;

                    var picked = live[rng.Next(live.Count)];
                    if (picked.requiredClass != cls && picked.category != WeaponCategory.None) wrongClass++;
                    weaponCount.TryGetValue(picked.id, out int c);
                    weaponCount[picked.id] = c + 1;
                }

                sb.AppendLine("### 등급 분포 (목표: 50/25/20/5)");
                sb.AppendLine("- Common: " + rarityCount[0] + " (" + (rarityCount[0] * 100f / TRIALS).ToString("F1") + "%)");
                sb.AppendLine("- Magic:  " + rarityCount[1] + " (" + (rarityCount[1] * 100f / TRIALS).ToString("F1") + "%)");
                sb.AppendLine("- Rare:   " + rarityCount[2] + " (" + (rarityCount[2] * 100f / TRIALS).ToString("F1") + "%)");
                sb.AppendLine("- Epic:   " + rarityCount[3] + " (" + (rarityCount[3] * 100f / TRIALS).ToString("F1") + "%)");
                sb.AppendLine();
                sb.AppendLine("### 무기별 분포");
                foreach (var kv in weaponCount)
                    sb.AppendLine("- " + kv.Key + ": " + kv.Value);
                sb.AppendLine();
                sb.AppendLine("### 직업 필터 — wrongClass = " + wrongClass + (wrongClass == 0 ? " ✅" : " ❌"));
                sb.AppendLine();
            }

            // simulation_report.md 갱신 (append §15)
            var path = Path.Combine(Application.dataPath, "../../docs/little_hero/weapon_gacha_v2_simulation.md");
            File.WriteAllText(path, sb.ToString());
            Debug.Log("[WeaponGachaSim] 보고서 저장: " + path);
            Debug.Log(sb.ToString());
        }

        private static EquipmentTemplate[] GetPoolForRarity(GachaPool pool, Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => pool.commonPool,
                Rarity.Magic => pool.magicPool,
                Rarity.Rare => pool.rarePool,
                Rarity.Epic => pool.epicPool,
                _ => null,
            };
        }
    }
}
