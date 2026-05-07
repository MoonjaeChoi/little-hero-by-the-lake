using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5033.0i — WeaponCategory 별 Hovl prefab variant lazy load + Spawn.
    /// 전사/궁수 시그니처 시각 (element=None). 마법사 element 무기는 ElementVfxBank 가 우선 처리.
    /// Resources/VFX/Weapon/{Sword,Axe,Mace,Spear,Bow,Crossbow,Dagger,Boomerang}.prefab.
    /// </summary>
    public static class WeaponVfxBank
    {
        private const float SPAWN_LIFETIME_SECONDS = 2f;

        private static GameObject _swordPrefab;
        private static GameObject _axePrefab;
        private static GameObject _macePrefab;
        private static GameObject _spearPrefab;
        private static GameObject _bowPrefab;
        private static GameObject _crossbowPrefab;
        private static GameObject _daggerPrefab;
        private static GameObject _boomerangPrefab;
        private static bool _initialized;

        public static void EnsureLoaded()
        {
            if (_initialized) return;
            _swordPrefab     = Resources.Load<GameObject>("VFX/Weapon/Sword");
            _axePrefab       = Resources.Load<GameObject>("VFX/Weapon/Axe");
            _macePrefab      = Resources.Load<GameObject>("VFX/Weapon/Mace");
            _spearPrefab     = Resources.Load<GameObject>("VFX/Weapon/Spear");
            _bowPrefab       = Resources.Load<GameObject>("VFX/Weapon/Bow");
            _crossbowPrefab  = Resources.Load<GameObject>("VFX/Weapon/Crossbow");
            _daggerPrefab    = Resources.Load<GameObject>("VFX/Weapon/Dagger");
            _boomerangPrefab = Resources.Load<GameObject>("VFX/Weapon/Boomerang");
            _initialized = true;
        }

        public static GameObject GetPrefab(WeaponCategory category)
        {
            EnsureLoaded();
            return category switch
            {
                WeaponCategory.Sword     => _swordPrefab,
                WeaponCategory.Axe       => _axePrefab,
                WeaponCategory.Mace      => _macePrefab,
                WeaponCategory.Spear     => _spearPrefab,
                WeaponCategory.Bow       => _bowPrefab,
                WeaponCategory.Crossbow  => _crossbowPrefab,
                WeaponCategory.Dagger    => _daggerPrefab,
                WeaponCategory.Boomerang => _boomerangPrefab,
                _ => null,
            };
        }

        public static void Spawn(WeaponCategory category, Vector3 position)
        {
            if (category == WeaponCategory.None) return;
            if (category == WeaponCategory.Staff || category == WeaponCategory.Wand || category == WeaponCategory.Tome)
                return;  // 마법사 무기 — ElementVfxBank 가 element 우선 처리
            var prefab = GetPrefab(category);
            if (prefab == null) return;
            var instance = Object.Instantiate(prefab, position, Quaternion.identity);
            Object.Destroy(instance, SPAWN_LIFETIME_SECONDS);
        }
    }
}
