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
            // 5033.1 — VfxPool Register (GC 0 cascade)
            VfxPool.Register("weapon_Sword",     _swordPrefab);
            VfxPool.Register("weapon_Axe",       _axePrefab);
            VfxPool.Register("weapon_Mace",      _macePrefab);
            VfxPool.Register("weapon_Spear",     _spearPrefab);
            VfxPool.Register("weapon_Bow",       _bowPrefab);
            VfxPool.Register("weapon_Crossbow",  _crossbowPrefab);
            VfxPool.Register("weapon_Dagger",    _daggerPrefab);
            VfxPool.Register("weapon_Boomerang", _boomerangPrefab);
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
            EnsureLoaded();
            var prefab = GetPrefab(category);
            if (prefab == null) return;
            // 5033.1 — VfxPool Pool 활용 (GC 0)
            VfxPool.Spawn("weapon_" + category, position, SPAWN_LIFETIME_SECONDS);
        }
    }
}
