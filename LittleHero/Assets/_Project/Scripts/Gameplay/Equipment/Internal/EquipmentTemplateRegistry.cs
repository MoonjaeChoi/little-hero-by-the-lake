using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5028 — EquipmentTemplate id → 인스턴스 lookup. Resources.LoadAll 자동 + RegisterTemplates 수동.
    /// </summary>
    public static class EquipmentTemplateRegistry
    {
        private static Dictionary<string, EquipmentTemplate> _cache;

        public static void EnsureCache()
        {
            if (_cache != null) return;
            _cache = new Dictionary<string, EquipmentTemplate>();
            var all = Resources.LoadAll<EquipmentTemplate>("");
            foreach (var t in all)
                if (t != null && !string.IsNullOrEmpty(t.id) && !_cache.ContainsKey(t.id))
                    _cache[t.id] = t;
        }

        public static void RegisterTemplates(IEnumerable<EquipmentTemplate> templates)
        {
            if (_cache == null) _cache = new Dictionary<string, EquipmentTemplate>();
            foreach (var t in templates)
                if (t != null && !string.IsNullOrEmpty(t.id)) _cache[t.id] = t;
        }

        public static bool TryGet(string id, out EquipmentTemplate template)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out template);
        }

        public static Dictionary<string, EquipmentTemplate> GetCache()
        {
            EnsureCache();
            return _cache;
        }
    }
}
