using System;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// C6 — Endgame 일일/주간 가용성 + 무한·고난도 진행 기록 (endgame_v1 §8.3).
    /// 기준 시각: **KST (UTC+9)** — 일일 = 매일 0시 / 주간 = 매주 월요일 0시.
    /// 본 서비스는 시간 도메인 헬퍼 + SaveData R/W 헬퍼만 제공한다.
    /// 보스 처치·보상 적용은 보스 처치 매니저(F4·F5 콘텐츠) 책임.
    /// </summary>
    public static class EndgameResetService
    {
        public const int KST_OFFSET_HOURS = 9;
        private static readonly TimeSpan KstOffset = TimeSpan.FromHours(KST_OFFSET_HOURS);

        // ===== 시간 헬퍼 (순수 함수) =====

        public static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static long GetLastDailyResetUnix()
        {
            var nowKst = DateTimeOffset.UtcNow.ToOffset(KstOffset);
            var resetKst = new DateTimeOffset(nowKst.Year, nowKst.Month, nowKst.Day, 0, 0, 0, KstOffset);
            return resetKst.ToUnixTimeSeconds();
        }

        public static long GetLastWeeklyResetUnix()
        {
            // KST 이번 월요일 0시. C# DayOfWeek: Sun=0, Mon=1, ... → Mon=0 으로 환산.
            var nowKst = DateTimeOffset.UtcNow.ToOffset(KstOffset);
            int daysSinceMonday = ((int)nowKst.DayOfWeek + 6) % 7;
            var monday = nowKst.AddDays(-daysSinceMonday);
            var resetKst = new DateTimeOffset(monday.Year, monday.Month, monday.Day, 0, 0, 0, KstOffset);
            return resetKst.ToUnixTimeSeconds();
        }

        public static long GetSecondsUntilNextDailyReset()
        {
            long nextResetUnix = GetLastDailyResetUnix() + 86400;
            return Math.Max(0, nextResetUnix - NowUnix());
        }

        public static long GetSecondsUntilNextWeeklyReset()
        {
            long nextResetUnix = GetLastWeeklyResetUnix() + 86400 * 7;
            return Math.Max(0, nextResetUnix - NowUnix());
        }

        // ===== Save-aware 헬퍼 (MetaProgression.Instance.Data 사용) =====

        public static bool IsDailyAvailable()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return false;
            return data.lastDailyBossClearTime < GetLastDailyResetUnix();
        }

        public static bool IsWeeklyAvailable()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return false;
            return data.lastWeeklyBossClearTime < GetLastWeeklyResetUnix();
        }

        public static void MarkDailyCleared()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            data.lastDailyBossClearTime = NowUnix();
            SaveSystem.Save(data);
        }

        public static void MarkWeeklyCleared()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            data.lastWeeklyBossClearTime = NowUnix();
            SaveSystem.Save(data);
        }

        public static int GetInfiniteMaxDepth()
            => MetaProgression.Instance?.Data?.infiniteModeMaxDepth ?? 0;

        /// <summary>최고 깊이만 갱신 (퇴보 안 함).</summary>
        public static bool RecordInfiniteDepth(int depth)
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null || depth <= data.infiniteModeMaxDepth) return false;
            data.infiniteModeMaxDepth = depth;
            SaveSystem.Save(data);
            return true;
        }

        /// <summary>chapterNumber 36~50 (인덱스 0~14).</summary>
        public static bool IsHardcoreChapterCleared(int chapterNumber)
        {
            int idx = chapterNumber - 36;
            var arr = MetaProgression.Instance?.Data?.hardcoreChaptersCleared;
            return arr != null && idx >= 0 && idx < arr.Length && arr[idx];
        }

        public static void MarkHardcoreChapterCleared(int chapterNumber)
        {
            int idx = chapterNumber - 36;
            var data = MetaProgression.Instance?.Data;
            if (data?.hardcoreChaptersCleared == null) return;
            if (idx < 0 || idx >= data.hardcoreChaptersCleared.Length) return;
            if (data.hardcoreChaptersCleared[idx]) return;
            data.hardcoreChaptersCleared[idx] = true;
            SaveSystem.Save(data);
        }
    }
}
