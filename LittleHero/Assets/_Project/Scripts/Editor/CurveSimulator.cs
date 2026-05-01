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

        // ── §3. 챕터별 적 스탯 ───────────────────────────────────────────
        static void SimulateEnemyStats(StringBuilder sb)
        {
            sb.AppendLine("## 3. 챕터별 적 스탯");
            sb.AppendLine();
            sb.AppendLine("| N | 일반 HP | 일반 ATK | 미니 HP | 미니 ATK | 보스 HP | 보스 ATK |");
            sb.AppendLine("|---|---|---|---|---|---|---|");

            for (int n = 1; n <= CHAPTER_COUNT; n++)
            {
                if (n != 1 && n % 5 != 0 && n != CHAPTER_COUNT) continue;

                float mul = Mathf.Pow(ENEMY_STAT_GROWTH, n - 1);
                float regHp = REGULAR_HP_BASE * mul;
                float regAtk = REGULAR_ATK_BASE * mul;
                float miniHp = regHp * MINI_HP_RATIO;
                float miniAtk = regAtk * MINI_ATK_RATIO;
                float bossHp = regHp * BOSS_HP_RATIO;
                float bossAtk = regAtk * BOSS_ATK_RATIO;

                sb.AppendLine(
                    $"| {n} | {regHp:N0} | {regAtk:N0} | {miniHp:N0} | {miniAtk:N0} | {bossHp:N0} | {bossAtk:N0} |"
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

        // ── §5. TTK 매트릭스 ─────────────────────────────────────────────
        static void SimulateTtk(StringBuilder sb)
        {
            sb.AppendLine("## 5. TTK 매트릭스 (권장 레벨 × 티어 vs 보스 HP)");
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

        // ── §10. 경고 ────────────────────────────────────────────────────
        static void SimulateAlerts(StringBuilder sb)
        {
            sb.AppendLine("## 10. 경고·플래그");
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
    }
}
