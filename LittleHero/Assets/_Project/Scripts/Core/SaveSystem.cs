using System.IO;
using UnityEngine;

namespace LittleHero
{
    [System.Serializable]
    public class MetaUpgradeEntry
    {
        public string id;
        public int level;
    }

    [System.Serializable]
    public class SaveData
    {
        public int starFragments;                                    // 무지개 구슬 ✦ (영구)
        public string selectedCharacterId;
        public string[] unlockedCharacterIds = new string[0];
        public string[] unlockedEquipmentIds = new string[0];
        public string[] discoveredMemoryIds = new string[0];
        public string[] defeatedEnemyIds = new string[0];
        public int[] clearedChapterWaves = new int[0];
        public MetaUpgradeEntry[] metaUpgrades = new MetaUpgradeEntry[0];

        // ✦ A6 — 신규 화폐·진행도 (economy_v1 §9.2 + character_progression §7.4 + monetization §7.1)
        public int magicCrystals;                                    // 마법 결정 ◇ (영구, +16 이상 강화 전용)
        public int starlight;                                        // 별빛 ⋆ (영구·캐시)
        public int level = 1;                                        // 캐릭터 레벨 (L100 캡)
        public int xp;                                               // 누적 XP (현 레벨 내 진행도는 BalanceConfig.xpThresholds로 환산)
        public long firstClearedChapters;                            // 35+15 챕터 첫 클리어 비트마스크 (long, 출시 전 fix #2 — int 32비트 한계 해소)

        // ✦ B2 — 장비 인스턴스 영구 저장 (equipment §4.2)
        public EquipmentInstance[] equipmentInstances = new EquipmentInstance[0];

        // monetization §7.1 — 별빛으로 영구 잠금/마일리지 산 결과 (인스턴스ID 기반)
        public string[] permanentlyLockedInstanceIds = new string[0];
        public MythicMileageEntry[] mythicMileageBonus = new MythicMileageEntry[0];

        // ✦ C6 — Endgame 진행 상태 (endgame_v1 §8.2 + §8.3)
        public long lastDailyBossClearTime;                          // Unix 초. 0 = 미클리어
        public long lastWeeklyBossClearTime;                         // Unix 초
        public int infiniteModeMaxDepth;                             // 무한 모드 최고 깊이
        public bool[] hardcoreChaptersCleared = new bool[15];        // CH 36~50 (인덱스 0~14)

        // ✦ E3 — 별빛 무료 출처 추적 (monetization_v1 §3.1)
        public long lastDailyLoginUnix;                              // 일일 로그인 5⋆ 보상 일자 (KST 0시)
        public bool[] chapterStarlightGranted = new bool[35];        // 챕터 N×2⋆ 1회 보상 비트
        public bool[] memoryStarlightGranted = new bool[8];          // 추억 발견 10⋆ 1회 보상 비트 (4 + 여유)
        public bool[] bestiaryFullStarlightGranted = new bool[35];   // 챕터별 도감 100% 50⋆ 비트

        // ✦ E4 — 시즌 패스 (monetization_v1 §3.3)
        public int seasonId;                                         // 현재 시즌 ID
        public bool seasonPassPremium;                               // 프리미엄 구매 여부
        public int seasonPoints;                                     // 시즌 진행도
        public int seasonRewardClaimedFreeMask;                      // 무료 선 보상 수령 비트
        public int seasonRewardClaimedPremiumMask;                   // 프리미엄 보상 수령 비트

        // ✦ PIVOT Phase C3 — 가챠·스타포스 (gacha_v1.md)
        public int totalGachaCount;                                  // 누적 무기 가챠 소환 횟수 (소환 레벨 계산용)
        public int summonLevel = 1;                                  // 현재 소환 레벨 (1~10)
        public int starforceShards;                                  // 스타포스 조각 (슬롯 강화 재화)
        public int weaponSummonTickets;                              // 무기 소환권 (무료 출처)
        public int[] slotStarforceLevels = new int[12];              // 12 슬롯별 ⭐ 단계 (0~25)

        // ✦ PIVOT — 한정 상점 (limited_shop_v1.md §5.2)
        public LimitedOfferTrigger[] limitedOfferTriggers = new LimitedOfferTrigger[0];
        public string[] purchasedLimitedOfferIds = new string[0];
        public long firstLaunchUnix;                                 // NewUser 트리거 (게임 첫 시작 시각)

        // ✦ PIVOT v2 — 별가루 ★ 영구화 (game_pivot_v2.md §2.1)
        public int starDust;                                         // ★ 별가루 (영구, 상점 폐기 후)

        // ✦ 챕터 진행 일시 정지 + 재개 (사용자 요청 2026-05-03)
        public string activeChapterId;                               // 진행 중인 챕터 ID (빈 문자열 = 활성 X)
        public int activeChapterWave;                                // 진행 wave 번호

        // ✦ 5002 D2 — 챕터 클리어 후 Attic 1회 toast 트리거 (mediator 결정)
        public string pendingChapterClearedId = "";                  // BossEncounterService.CompleteChapterBoss 가 set, AtticController 가 1회 표시 후 clear
    }

    [System.Serializable]
    public class MythicMileageEntry
    {
        public string instanceId;
        public int bonus;
    }

    public static class SaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        public static void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        public static SaveData Load()
        {
            if (!File.Exists(SavePath))
                return new SaveData();
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public static void Delete()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }

        public static bool HasSave() => File.Exists(SavePath);
    }
}
