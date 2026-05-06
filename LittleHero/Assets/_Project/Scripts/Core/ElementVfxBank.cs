using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5029/5033 — element 별 VFX prefab. Hovl Magic Effects FREE 4 prefab variant 를
    /// Resources/VFX/Element/{Fire,Ice,Lightning,Arcane}.prefab 에서 lazy load.
    /// 5033.1 후속 — VfxPool Register/Spawn API 정착 (GC 0).
    /// </summary>
    public static class ElementVfxBank
    {
        private const float SPAWN_LIFETIME_SECONDS = 3f;

        private static GameObject _firePrefab;
        private static GameObject _icePrefab;
        private static GameObject _lightningPrefab;
        private static GameObject _arcanePrefab;
        private static bool _initialized;

        public static void EnsureLoaded()
        {
            if (_initialized) return;
            _firePrefab      = Resources.Load<GameObject>("VFX/Element/Fire");
            _icePrefab       = Resources.Load<GameObject>("VFX/Element/Ice");
            _lightningPrefab = Resources.Load<GameObject>("VFX/Element/Lightning");
            _arcanePrefab    = Resources.Load<GameObject>("VFX/Element/Arcane");
            _initialized = true;
        }

        public static GameObject GetPrefab(MagicElement element)
        {
            EnsureLoaded();
            return element switch
            {
                MagicElement.Fire      => _firePrefab,
                MagicElement.Ice       => _icePrefab,
                MagicElement.Lightning => _lightningPrefab,
                MagicElement.Arcane    => _arcanePrefab,
                _ => null,
            };
        }

        public static Color GetColor(MagicElement element) => element switch
        {
            MagicElement.Fire      => new Color(1.0f, 0.4f, 0.1f),
            MagicElement.Ice       => new Color(0.4f, 0.8f, 1.0f),
            MagicElement.Lightning => new Color(1.0f, 0.95f, 0.3f),
            MagicElement.Arcane    => new Color(0.8f, 0.4f, 1.0f),
            _                      => Color.white,
        };

        /// <summary>5033 — Hovl prefab Instantiate + SPAWN_LIFETIME_SECONDS 후 Destroy. T02 에서 본격 사용.</summary>
        public static void Spawn(MagicElement element, Vector3 position)
        {
            if (element == MagicElement.None) return;
            var prefab = GetPrefab(element);
            if (prefab == null)
            {
                VfxPool.SpawnStarBurst(position);
                return;
            }
            var instance = Object.Instantiate(prefab, position, Quaternion.identity);
            Object.Destroy(instance, SPAWN_LIFETIME_SECONDS);
        }
    }
}
