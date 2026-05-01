using System;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// E3 — 별빛 ⋆ 무료 출처 자동 적립 (monetization_v1 §3.1).
    ///
    /// 출처 (1회성 또는 일일):
    ///  - 일일 로그인          5 ⋆ (KST 0시 기준 1일 1회)
    ///  - 챕터 N 첫 클리어     N×2 ⋆ (chapter §3.1, 챕터 1~35 각 1회)
    ///  - 추억 발견            10 ⋆ (추억 1개당 1회, 최대 4종)
    ///  - 도감 100% (챕터별)   50 ⋆ (챕터 N 의 적 풀 100% 처치 시 1회)
    ///  - 일일 보스 첫 클리어  5 ⋆ (EndgameResetService.MarkDailyCleared 와 연동, F4)
    ///  - 주간 보스 첫 클리어  20 ⋆ (F5)
    ///
    /// 본 서비스는 GameEvents 를 구독하여 자동 발급. 게임 시작 시 1회 `Bootstrap` 호출.
    /// </summary>
    public static class StarlightGrantService
    {
        public const int DAILY_LOGIN_AMOUNT = 5;
        public const int MEMORY_FOUND_AMOUNT = 10;
        public const int BESTIARY_FULL_AMOUNT = 50;
        public const int DAILY_BOSS_AMOUNT = 5;
        public const int WEEKLY_BOSS_AMOUNT = 20;

        private static bool _bootstrapped;

        public static void Bootstrap()
        {
            if (_bootstrapped) return;
            _bootstrapped = true;
            GameEvents.OnChapterCleared    += HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  += HandleMemoryDiscovered;
            GameEvents.OnEnemyKilled       += HandleEnemyKilled;
            TryGrantDailyLogin();
        }

        public static void Shutdown()
        {
            if (!_bootstrapped) return;
            _bootstrapped = false;
            GameEvents.OnChapterCleared    -= HandleChapterCleared;
            GameEvents.OnMemoryDiscovered  -= HandleMemoryDiscovered;
            GameEvents.OnEnemyKilled       -= HandleEnemyKilled;
        }

        // ===== 일일 로그인 =====

        public static bool TryGrantDailyLogin()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return false;
            long todayKstUnix = EndgameResetService.GetLastDailyResetUnix();
            if (data.lastDailyLoginUnix >= todayKstUnix) return false;
            data.lastDailyLoginUnix = todayKstUnix;
            CurrencyService.GrantStarlight(DAILY_LOGIN_AMOUNT);
            SaveSystem.Save(data);
            Debug.Log($"[StarlightGrant] 일일 로그인 +{DAILY_LOGIN_AMOUNT}⋆");
            return true;
        }

        // ===== 챕터 첫 클리어 =====

        private static void HandleChapterCleared(ChapterData chapter)
        {
            if (chapter == null || chapter.chapterNumber <= 0 || chapter.chapterNumber > 35) return;
            TryGrantChapterFirstClear(chapter.chapterNumber);
        }

        public static bool TryGrantChapterFirstClear(int chapterNumber)
        {
            var data = MetaProgression.Instance?.Data;
            if (data?.chapterStarlightGranted == null) return false;
            int idx = chapterNumber - 1;
            if (idx < 0 || idx >= data.chapterStarlightGranted.Length) return false;
            if (data.chapterStarlightGranted[idx]) return false;

            int reward = CurrencyService.CalcChapterStarlightReward(chapterNumber);
            CurrencyService.GrantStarlight(reward);
            data.chapterStarlightGranted[idx] = true;
            // firstClearedChapters 비트마스크도 동기화 (D3 지도 해방용, long 50비트 — CH 1~35 + hardcore 36~50)
            if (idx >= 0 && idx < 50) data.firstClearedChapters |= (1L << idx);
            SaveSystem.Save(data);
            Debug.Log($"[StarlightGrant] 챕터 {chapterNumber} 첫 클리어 +{reward}⋆");
            return true;
        }

        // ===== 추억 발견 =====

        private static void HandleMemoryDiscovered(MemoryData memory)
        {
            if (memory == null) return;
            TryGrantMemoryFound(memory.id);
        }

        public static bool TryGrantMemoryFound(string memoryId)
        {
            var data = MetaProgression.Instance?.Data;
            if (data?.memoryStarlightGranted == null || string.IsNullOrEmpty(memoryId)) return false;
            int idx = HashToIndex(memoryId, data.memoryStarlightGranted.Length);
            if (data.memoryStarlightGranted[idx]) return false;
            data.memoryStarlightGranted[idx] = true;
            CurrencyService.GrantStarlight(MEMORY_FOUND_AMOUNT);
            SaveSystem.Save(data);
            Debug.Log($"[StarlightGrant] 추억 '{memoryId}' 발견 +{MEMORY_FOUND_AMOUNT}⋆");
            return true;
        }

        // ===== 도감 100% (챕터별) =====

        private static void HandleEnemyKilled(EnemyData enemy)
        {
            // 처치 시점에 챕터 도감 100% 도달 여부 확인.
            // 챕터별 적 풀은 ChapterRunner 의 _chapterData 또는 SelectedChapter 에서 가져옴.
            var chapter = MetaProgression.Instance?.SelectedChapter;
            if (chapter == null || chapter.chapterNumber <= 0 || chapter.chapterNumber > 35) return;
            if (IsChapterBestiary100(chapter))
                TryGrantBestiaryFull(chapter.chapterNumber);
        }

        private static bool IsChapterBestiary100(ChapterData chapter)
        {
            if (chapter == null) return false;
            var defeated = MetaProgression.Instance?.Data?.defeatedEnemyIds;
            if (defeated == null) return false;

            bool All(EnemyData[] pool)
            {
                if (pool == null) return true;
                for (int i = 0; i < pool.Length; i++)
                {
                    if (pool[i] == null || string.IsNullOrEmpty(pool[i].id)) continue;
                    if (Array.IndexOf(defeated, pool[i].id) < 0) return false;
                }
                return true;
            }
            return All(chapter.enemyPool) && All(chapter.miniBossPool)
                   && (chapter.bossEnemy == null || Array.IndexOf(defeated, chapter.bossEnemy.id) >= 0);
        }

        public static bool TryGrantBestiaryFull(int chapterNumber)
        {
            var data = MetaProgression.Instance?.Data;
            if (data?.bestiaryFullStarlightGranted == null) return false;
            int idx = chapterNumber - 1;
            if (idx < 0 || idx >= data.bestiaryFullStarlightGranted.Length) return false;
            if (data.bestiaryFullStarlightGranted[idx]) return false;
            data.bestiaryFullStarlightGranted[idx] = true;
            CurrencyService.GrantStarlight(BESTIARY_FULL_AMOUNT);
            SaveSystem.Save(data);
            Debug.Log($"[StarlightGrant] 챕터 {chapterNumber} 도감 100% +{BESTIARY_FULL_AMOUNT}⋆");
            return true;
        }

        // ===== Endgame 보스 (F4·F5 보스 처치 매니저가 호출) =====

        public static bool GrantDailyBossFirstClear() => GrantOneShotIfNotPaid(ref DailyBossPaidThisCycle, DAILY_BOSS_AMOUNT, "일일 보스");
        public static bool GrantWeeklyBossFirstClear() => GrantOneShotIfNotPaid(ref WeeklyBossPaidThisCycle, WEEKLY_BOSS_AMOUNT, "주간 보스");

        // 메모리 캐시 — EndgameResetService 가 cycle reset 시 false 로 돌림 (호출 책임은 F4·F5 매니저)
        public static bool DailyBossPaidThisCycle;
        public static bool WeeklyBossPaidThisCycle;

        private static bool GrantOneShotIfNotPaid(ref bool flag, int amount, string label)
        {
            if (flag) return false;
            flag = true;
            CurrencyService.GrantStarlight(amount);
            Debug.Log($"[StarlightGrant] {label} 첫 클리어 +{amount}⋆");
            return true;
        }

        // ===== 헬퍼 =====

        private static int HashToIndex(string id, int slotCount)
        {
            // 추억 ID → 안정적 인덱스. 충돌 시 같은 슬롯 공유 (4개+여유 4 → 8 슬롯이라 거의 충돌 없음).
            unchecked
            {
                int h = 17;
                for (int i = 0; i < id.Length; i++) h = h * 31 + id[i];
                return (h & 0x7FFFFFFF) % Mathf.Max(1, slotCount);
            }
        }
    }
}
