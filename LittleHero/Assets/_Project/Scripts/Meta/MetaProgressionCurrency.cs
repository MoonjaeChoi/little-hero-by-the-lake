using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5023 — 4 화폐 (StarFragment / MagicCrystal / Starlight + StarDust 는 GameManager) 책임 분리.
    /// 모든 변경 메서드 마지막에 SaveSystem.Save 호출 (D2=C 정합).
    /// </summary>
    public static class MetaProgressionCurrency
    {
        private static SaveData Data => MetaProgression.Instance?.Data;

        public static void AddStarFragments(int amount)
        {
            if (amount <= 0 || Data == null) return;
            Data.starFragments += amount;
            MetaProgression.OnStarFragmentsChanged?.Invoke(Data.starFragments);
            SaveSystem.Save(Data);
        }

        public static bool SpendStarFragments(int amount)
        {
            if (amount <= 0 || Data == null || Data.starFragments < amount) return false;
            Data.starFragments -= amount;
            MetaProgression.OnStarFragmentsChanged?.Invoke(Data.starFragments);
            SaveSystem.Save(Data);
            return true;
        }

        public static void AddMagicCrystals(int amount)
        {
            if (amount <= 0 || Data == null) return;
            Data.magicCrystals += amount;
            GameEvents.RaiseMagicCrystalChanged(Data.magicCrystals);
            SaveSystem.Save(Data);
        }

        public static bool SpendMagicCrystals(int amount)
        {
            if (amount <= 0 || Data == null || Data.magicCrystals < amount) return false;
            Data.magicCrystals -= amount;
            GameEvents.RaiseMagicCrystalChanged(Data.magicCrystals);
            SaveSystem.Save(Data);
            return true;
        }

        public static void AddStarlight(int amount)
        {
            if (amount <= 0 || Data == null) return;
            Data.starlight += amount;
            GameEvents.RaiseStarlightChanged(Data.starlight);
            SaveSystem.Save(Data);
        }

        public static bool SpendStarlight(int amount)
        {
            if (amount <= 0 || Data == null || Data.starlight < amount) return false;
            Data.starlight -= amount;
            GameEvents.RaiseStarlightChanged(Data.starlight);
            SaveSystem.Save(Data);
            return true;
        }
    }
}
