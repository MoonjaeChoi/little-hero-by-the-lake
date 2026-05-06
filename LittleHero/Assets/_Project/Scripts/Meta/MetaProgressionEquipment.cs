using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5023 — 장비 인스턴스 + drop batching 책임 분리 (PIVOT v3 GC critical 정합 보존).
    /// </summary>
    public static class MetaProgressionEquipment
    {
        // PIVOT v3 — drop batching: 메모리 append 후 Chapter 클리어/사망 시점 1회 일괄 Save
        private static readonly List<EquipmentInstance> _pendingDrops = new List<EquipmentInstance>(512);

        public static void QueueDrop(EquipmentInstance instance)
        {
            if (instance == null) return;
            _pendingDrops.Add(instance);
        }

        public static void FlushDrops()
        {
            if (_pendingDrops.Count == 0) return;
            var data = MetaProgression.Instance?.Data;
            if (data == null) { _pendingDrops.Clear(); return; }
            var existing = data.equipmentInstances ?? new EquipmentInstance[0];
            var arr = new EquipmentInstance[existing.Length + _pendingDrops.Count];
            for (int i = 0; i < existing.Length; i++) arr[i] = existing[i];
            for (int i = 0; i < _pendingDrops.Count; i++) arr[existing.Length + i] = _pendingDrops[i];
            data.equipmentInstances = arr;
            int count = _pendingDrops.Count;
            _pendingDrops.Clear();
            SaveSystem.Save(data);
            Debug.Log($"[MetaProgressionEquipment] FlushDrops: {count} 장비 일괄 저장");
        }
    }
}
