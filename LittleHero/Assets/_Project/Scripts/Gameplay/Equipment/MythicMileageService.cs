using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// C7 — 신화 옵션 발현 서비스 (equipment_v1 §3.7 + monetization_v1 §4.4 + §7.1).
    ///
    /// 잭팟 ARPG Part 2 마일리지 공식:
    ///   p = BASE_RATE(0.005) + effectiveMileage / GUARANTEE_THRESHOLD(200)
    /// effectiveMileage = instance.history.mileage + SaveData.mythicMileageBonus[instanceId]
    ///
    /// 발현 조건: <see cref="MetaProgression.AllMemoriesDiscovered"/> 가 true 인 경우만 시도.
    /// 발현 시 풀에서 슬롯 제한을 만족하는 신화 1종 무작위 선택, instance.mythicId 설정,
    /// 마일리지 0 으로 리셋.
    ///
    /// 주의: 별빛 ⋆ 100 → 마일리지 +20 (monetization §4.4) 은 다른 시스템이 SaveData 갱신.
    /// 본 서비스는 SaveData 의 보너스를 읽기만 한다.
    /// </summary>
    public static class MythicMileageService
    {
        public const float BASE_RATE = 0.005f;
        public const int GUARANTEE_THRESHOLD = 200;

        private static readonly System.Random _defaultRng = new System.Random();

        /// <summary>
        /// 마일리지 +1 후 발현 시도. 발현 시 instance.mythicId 설정 + 마일리지 리셋, true 반환.
        /// 추억 미수집·풀 비어있음·이미 발현 시 false.
        /// </summary>
        public static bool TryRoll(EquipmentInstance instance, MythicDefinition[] pool, System.Random rng = null)
        {
            if (instance == null) return false;
            if (!IsUnlocked()) return false;
            if (pool == null || pool.Length == 0) return false;
            if (!string.IsNullOrEmpty(instance.mythicId)) return false; // 이미 발현

            instance.history ??= new InstanceHistory();
            instance.history.mythicAttempts++;
            instance.history.mileage++;

            int eff = GetEffectiveMileage(instance);
            float p = CalcChance(eff);
            rng ??= _defaultRng;
            if (rng.NextDouble() >= p) return false;

            // 발현 — 슬롯 매칭되는 풀에서 무작위 추출
            var template = ResolveTemplate(instance);
            EquipSlot? slot = template?.slot;
            var matches = new System.Collections.Generic.List<MythicDefinition>(pool.Length);
            for (int i = 0; i < pool.Length; i++)
            {
                var m = pool[i];
                if (m == null) continue;
                if (slot.HasValue && m.slotRestriction != null && m.slotRestriction.Length > 0)
                {
                    bool ok = false;
                    for (int s = 0; s < m.slotRestriction.Length; s++)
                        if (m.slotRestriction[s] == slot.Value) { ok = true; break; }
                    if (!ok) continue;
                }
                matches.Add(m);
            }
            if (matches.Count == 0) return false;

            var chosen = matches[rng.Next(matches.Count)];
            instance.mythicId = chosen.id;
            instance.history.mileage = 0;
            return true;
        }

        /// <summary>발현 보장 — 마일리지 50 이상에서 별빛 300 ⋆ 로 호출 (monetization §4.4).</summary>
        public static bool ForceRoll(EquipmentInstance instance, MythicDefinition[] pool, System.Random rng = null)
        {
            if (instance == null || pool == null || pool.Length == 0) return false;
            if (!IsUnlocked()) return false;
            if (!string.IsNullOrEmpty(instance.mythicId)) return false;
            if (GetEffectiveMileage(instance) < 50) return false;

            // 풀 매칭 후 보장 발현
            var template = ResolveTemplate(instance);
            EquipSlot? slot = template?.slot;
            var matches = new System.Collections.Generic.List<MythicDefinition>(pool.Length);
            for (int i = 0; i < pool.Length; i++)
            {
                var m = pool[i];
                if (m == null) continue;
                if (slot.HasValue && m.slotRestriction != null && m.slotRestriction.Length > 0)
                {
                    bool ok = false;
                    for (int s = 0; s < m.slotRestriction.Length; s++)
                        if (m.slotRestriction[s] == slot.Value) { ok = true; break; }
                    if (!ok) continue;
                }
                matches.Add(m);
            }
            if (matches.Count == 0) return false;

            rng ??= _defaultRng;
            instance.mythicId = matches[rng.Next(matches.Count)].id;
            instance.history ??= new InstanceHistory();
            instance.history.mileage = 0;
            return true;
        }

        /// <summary>주간 보스 보상 — 마일리지 게이트 무시하고 발현 보장 (endgame §5.4).</summary>
        public static bool GuaranteeRoll(EquipmentInstance instance, MythicDefinition[] pool, System.Random rng = null)
        {
            if (instance == null || pool == null || pool.Length == 0) return false;
            if (!IsUnlocked()) return false;
            if (!string.IsNullOrEmpty(instance.mythicId)) return false;

            var template = ResolveTemplate(instance);
            EquipSlot? slot = template?.slot;
            var matches = new System.Collections.Generic.List<MythicDefinition>(pool.Length);
            for (int i = 0; i < pool.Length; i++)
            {
                var m = pool[i];
                if (m == null) continue;
                if (slot.HasValue && m.slotRestriction != null && m.slotRestriction.Length > 0)
                {
                    bool ok = false;
                    for (int s = 0; s < m.slotRestriction.Length; s++)
                        if (m.slotRestriction[s] == slot.Value) { ok = true; break; }
                    if (!ok) continue;
                }
                matches.Add(m);
            }
            if (matches.Count == 0) return false;

            rng ??= _defaultRng;
            instance.mythicId = matches[rng.Next(matches.Count)].id;
            instance.history ??= new InstanceHistory();
            instance.history.mileage = 0;
            return true;
        }

        // ===== 헬퍼 =====

        public static bool IsUnlocked()
            => MetaProgression.Instance?.AllMemoriesDiscovered() == true;

        public static float CalcChance(int effectiveMileage)
            => Mathf.Clamp01(BASE_RATE + (float)effectiveMileage / GUARANTEE_THRESHOLD);

        public static int GetEffectiveMileage(EquipmentInstance instance)
        {
            if (instance == null) return 0;
            int baseMileage = instance.history?.mileage ?? 0;
            int bonus = LookupMileageBonus(instance.instanceId);
            return baseMileage + bonus;
        }

        /// <summary>SaveData.mythicMileageBonus 에 별빛으로 산 +20 등을 누적.</summary>
        public static void AddMileageBonus(string instanceId, int amount)
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null || string.IsNullOrEmpty(instanceId) || amount <= 0) return;

            var arr = data.mythicMileageBonus ?? new MythicMileageEntry[0];
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != null && arr[i].instanceId == instanceId)
                {
                    arr[i].bonus += amount;
                    SaveSystem.Save(data);
                    return;
                }
            }
            // 신규 엔트리
            var list = new System.Collections.Generic.List<MythicMileageEntry>(arr);
            list.Add(new MythicMileageEntry { instanceId = instanceId, bonus = amount });
            data.mythicMileageBonus = list.ToArray();
            SaveSystem.Save(data);
        }

        private static int LookupMileageBonus(string instanceId)
        {
            var arr = MetaProgression.Instance?.Data?.mythicMileageBonus;
            if (arr == null || string.IsNullOrEmpty(instanceId)) return 0;
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] != null && arr[i].instanceId == instanceId)
                    return arr[i].bonus;
            return 0;
        }

        private static EquipmentTemplate ResolveTemplate(EquipmentInstance instance)
        {
            // ItemDropService 가 주입한 lookup 이 없는 시점 — 슬롯 제한이 비어있는 신화는 모두 매칭됨.
            // 슬롯 제한이 있는 신화의 정확한 매칭은 호출자(D6/D11)가 templates 사전을 추가 인자로 노출하는 후속에서.
            return null;
        }
    }
}
