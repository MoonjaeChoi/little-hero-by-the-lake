#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5033.3 — 개발 / QA 검증 편의 메뉴. SaveData 의 5 화폐 일괄 충전 후 이벤트 발행.
    /// Play Mode 활성 시 — 즉시 HUD/Attic UI 갱신.
    /// Play Mode 비활성 — SaveData 만 갱신 (다음 Play 진입 시 반영).
    /// </summary>
    public static class AddTestCurrenciesMenu
    {
        private const string MENU_PATH = "Tools/LittleHero/Add Test Currencies";

        private const int STAR_FRAGMENTS  = 9_999_999;  // 별가루 ★
        private const int MAGIC_CRYSTALS  = 99_999;     // 마법 결정 ◇
        private const int STARLIGHT       = 999_999;    // 별빛 ⋆
        private const int STARFORCE_SHARDS = 9_999_999; // 마법의돌 조각 ◈
        private const int WEAPON_TICKETS  = 999;        // 무기 소환권

        [MenuItem(MENU_PATH)]
        public static void AddCurrencies()
        {
            var instance = MetaProgression.Instance;
            if (instance != null && Application.isPlaying)
            {
                var data = instance.Data;
                ApplyTo(data);
                SaveSystem.Save(data);
                MetaProgression.RaiseStarFragmentsChanged(data.starFragments);
                GameEvents.RaiseMagicCrystalChanged(data.magicCrystals);
                GameEvents.RaiseStarlightChanged(data.starlight);
                GameEvents.RaiseStarforceShardsChanged(data.starforceShards);
                Debug.Log($"[AddTestCurrencies] Play Mode 갱신 완료 — ★={data.starFragments} ◇={data.magicCrystals} ⋆={data.starlight} ◈={data.starforceShards} 소환권={data.weaponSummonTickets}");
                return;
            }

            // Editor 모드 — SaveData 직접
            var sd = SaveSystem.Load();
            if (sd == null)
            {
                Debug.LogWarning("[AddTestCurrencies] SaveData null — Play Mode 진입 후 재실행 권장");
                return;
            }
            ApplyTo(sd);
            SaveSystem.Save(sd);
            Debug.Log($"[AddTestCurrencies] Editor SaveData 갱신 — Play 진입 시 반영. ★={sd.starFragments} ◇={sd.magicCrystals} ⋆={sd.starlight} ◈={sd.starforceShards} 소환권={sd.weaponSummonTickets}");
        }

        private static void ApplyTo(SaveData data)
        {
            data.starFragments    = STAR_FRAGMENTS;
            data.magicCrystals    = MAGIC_CRYSTALS;
            data.starlight        = STARLIGHT;
            data.starforceShards  = STARFORCE_SHARDS;
            data.weaponSummonTickets = WEAPON_TICKETS;
        }
    }
}
#endif
