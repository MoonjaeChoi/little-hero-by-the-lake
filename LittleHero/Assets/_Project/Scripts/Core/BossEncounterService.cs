using System;
using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// F4·F5 — 일일/주간 보스 처치 매니저 (endgame_v1 §4·§5).
    /// 요일/주차 인덱스 → bossPool 선택 / 처치 시 보상·마크·신화 보장 처리.
    ///
    /// 흐름:
    /// 1. EndgameMenuPanel.Enter → ResolveBoss(mode) 로 오늘의 보스 결정
    /// 2. 보스 처치 시점에 CompleteDailyBoss / CompleteWeeklyBoss 호출 (현재는 placeholder — 보스 씬 분리 시 wiring)
    /// 3. 가용성 검사·보상 적용·MarkXxxCleared·StarlightGrant·신화 보장
    ///
    /// 5001 boss_system_v2 — BossTier enum 호환 (T08).
    /// 본 클래스는 mode-based 흐름이라 isBoss 직접 분기 없이 호환.
    /// IsTierMatch 헬퍼로 BossTier enum 점진 마이그레이션 지원.
    /// </summary>
    public static class BossEncounterService
    {
        private static readonly TimeSpan KstOffset = TimeSpan.FromHours(EndgameResetService.KST_OFFSET_HOURS);

        // ===== 5001 BossTier 호환 헬퍼 =====

        /// <summary>EnemyData 의 BossTier 가 지정 tier 와 일치하는지 검사 (T08 — 점진 마이그레이션 지원).</summary>
        public static bool IsTierMatch(EnemyData data, BossTier tier)
        {
            return data != null && data.bossTier == tier;
        }

        /// <summary>주간 보스 처치 시 신화 보장 가능 여부 — bossTier == WeeklyBoss && lootTable.guaranteedMythicChance >= 1.0.</summary>
        public static bool ShouldGuaranteeMythic(EnemyData data)
        {
            return data != null
                && data.bossTier == BossTier.WeeklyBoss
                && data.lootTable != null
                && data.lootTable.guaranteedMythicChance >= 1.0f;
        }

        // ===== 보스 선택 =====

        /// <summary>일일 보스 — 요일별 회전. 일요일 = bossPool 무작위.</summary>
        public static EnemyData ResolveDailyBoss(EndgameModeData mode)
        {
            if (mode == null || mode.bossPool == null || mode.bossPool.Length == 0) return null;
            var nowKst = DateTimeOffset.UtcNow.ToOffset(KstOffset);
            int dow = ((int)nowKst.DayOfWeek + 6) % 7;  // 월=0, 일=6
            if (dow == 6)
                return mode.bossPool[UnityEngine.Random.Range(0, mode.bossPool.Length)];
            return mode.bossPool[dow % mode.bossPool.Length];
        }

        /// <summary>주간 보스 — 4주 사이클. 연중 주차 / 4 = 인덱스.</summary>
        public static EnemyData ResolveWeeklyBoss(EndgameModeData mode)
        {
            if (mode == null || mode.bossPool == null || mode.bossPool.Length == 0) return null;
            var nowKst = DateTimeOffset.UtcNow.ToOffset(KstOffset);
            int weekIdx = ((nowKst.DayOfYear - 1) / 7) % 4;
            return mode.bossPool[weekIdx % mode.bossPool.Length];
        }

        // ===== 처치 처리 =====

        /// <summary>5003 — 미니보스 처치. BossLootGrantService.Apply 만 (챕터 클리어 X — firstClearedChapters / pendingChapterClearedId 변동 X).</summary>
        public static void CompleteMiniBoss(EnemyData boss, ChapterData chapter)
        {
            if (boss == null || chapter == null)
            {
                Debug.LogWarning($"[BossEncounterService] CompleteMiniBoss: boss={boss} chapter={chapter}");
                return;
            }

            if (boss.lootTable != null)
                BossLootGrantService.Apply(boss.lootTable);
            else
                Debug.LogWarning($"[BossEncounterService] {boss.id}: lootTable null — fallback 보상 생략");

            // 미니보스 = 챕터 클리어 아님. RaiseChapterCleared 호출 X / pendingChapterClearedId set X.
            Debug.Log($"[BossEncounterService] CompleteMiniBoss: {boss.displayName} (chapter {chapter.chapterNumber})");
        }

        /// <summary>5002 T06 — 챕터 보스 처치. BossLootGrantService.Apply + ChapterCleared + pendingChapterClearedId set (D2).</summary>
        public static void CompleteChapterBoss(EnemyData boss, ChapterData chapter)
        {
            if (boss == null || chapter == null)
            {
                Debug.LogWarning($"[BossEncounterService] CompleteChapterBoss: boss={boss} chapter={chapter}");
                return;
            }

            // 1) 보상 — BossLootTable 우선
            if (boss.lootTable != null)
                BossLootGrantService.Apply(boss.lootTable);
            else
                Debug.LogWarning($"[BossEncounterService] {boss.id}: lootTable null — fallback 보상 생략");

            // 2) 챕터 클리어 이벤트 (ChapterRunner 가 더 이상 동작 X — 직접 raise)
            GameEvents.RaiseChapterCleared(chapter);

            // 3) MetaProgression — firstClearedChapters bit set + pendingChapterClearedId set (D2 mediator)
            var data = MetaProgression.Instance?.Data;
            if (data != null)
            {
                int idx = chapter.chapterNumber - 1;
                if (idx >= 0 && idx < 50) data.firstClearedChapters |= (1L << idx);
                data.pendingChapterClearedId = chapter.id;  // D2: Attic 1회 toast 트리거
                SaveSystem.Save(data);
            }

            Debug.Log($"[BossEncounterService] CompleteChapterBoss: {boss.displayName} (chapter {chapter.chapterNumber})");
        }

        /// <summary>일일 보스 처치 시점에 호출. 가용성 미충족 또는 mode 결손 시 false.</summary>
        public static bool CompleteDailyBoss(EndgameModeData mode)
        {
            if (mode == null) return false;
            if (!EndgameResetService.IsDailyAvailable()) return false;
            ApplyRewards(mode.firstClearRewards);
            EndgameResetService.MarkDailyCleared();
            StarlightGrantService.GrantDailyBossFirstClear();
            return true;
        }

        /// <summary>주간 보스 처치 — 보상 + 신화 발현 보장 (endgame §5.4).</summary>
        public static WeeklyBossResult CompleteWeeklyBoss(EndgameModeData mode, MythicDefinition[] mythicPool)
        {
            var result = new WeeklyBossResult();
            if (mode == null) return result;
            if (!EndgameResetService.IsWeeklyAvailable()) return result;

            ApplyRewards(mode.firstClearRewards);
            result.mythicGranted = GuaranteeMythicForEquipped(mythicPool);
            EndgameResetService.MarkWeeklyCleared();
            StarlightGrantService.GrantWeeklyBossFirstClear();
            result.success = true;
            return result;
        }

        // ===== 보상 적용 =====

        private static void ApplyRewards(RewardEntry[] rewards)
        {
            if (rewards == null) return;
            foreach (var r in rewards)
            {
                if (r == null || r.amount <= 0) continue;
                switch (r.currency)
                {
                    case CurrencyType.StarDust:
                        GameManager.Instance?.AddStarDust(r.amount);
                        break;
                    case CurrencyType.RainbowOrb:
                        CurrencyService.GrantStarFragment(r.amount);  // 무지개 구슬 = StarFragment
                        break;
                    case CurrencyType.MagicCrystal:
                        CurrencyService.GrantMagicCrystal(r.amount);
                        break;
                    case CurrencyType.Starlight:
                        CurrencyService.GrantStarlight(r.amount);
                        break;
                    case CurrencyType.Xp:
                        PlayerProgress.Instance?.AddXp(r.amount);
                        break;
                }
            }
        }

        // ===== 신화 보장 =====

        /// <summary>장착 인스턴스 중 신화 미발현 1개 무작위 선택 → GuaranteeRoll. 결과 인스턴스 반환(D11 오버레이용).</summary>
        public static EquipmentInstance GuaranteeMythicForEquipped(MythicDefinition[] pool)
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null || data.equipmentInstances == null || pool == null || pool.Length == 0) return null;

            var candidates = new List<EquipmentInstance>();
            foreach (var inst in data.equipmentInstances)
                if (inst != null && inst.isEquipped && string.IsNullOrEmpty(inst.mythicId))
                    candidates.Add(inst);
            if (candidates.Count == 0) return null;

            var chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            if (MythicMileageService.GuaranteeRoll(chosen, pool))
            {
                SaveSystem.Save(data);
                return chosen;
            }
            return null;
        }

        public class WeeklyBossResult
        {
            public bool success;
            public EquipmentInstance mythicGranted; // null 가능 (장착 슬롯 모두 신화 보유 시)
        }
    }
}
