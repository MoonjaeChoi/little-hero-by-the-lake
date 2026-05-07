using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// F8 — 신화 옵션 런타임 효과 (equipment_v1 §3.7 + endgame §5.4).
    ///
    /// 4 신화 효과는 모두 조건부이므로 EffectResolver(C4) 가 stats 에 합산하지 않고
    /// `ResolvedEquipmentBonus.ActiveMythicIds` 만 노출한다. 본 클래스가 런타임 처리:
    ///
    ///  - mythic_dad_compass : 보스 대상 데미지 × 1.20 (DamageSystem 측에서 enemy.isBoss 검사)
    ///  - mythic_dad_lullaby : HP < 30% 일 때 받는 데미지 × 0.50 (PlayerDamageReceiver 측)
    ///  - mythic_dad_courage : 적이 처치 직전 (HP < 15%) 일 때 데미지 × 1.50
    ///  - mythic_dad_promise : Trail 자취 길이 × 2 + 옵션 리롤 1회 무료 (Equipment 측 별도 처리)
    ///
    /// 본 정적 헬퍼는 게임플레이 코드(DamageSystem 등) 가 한 줄로 호출 가능한 곱셈 계산을 제공.
    /// 발현된 신화 ID 는 <see cref="MetaProgression.Instance.Data.equipmentInstances"/> 의
    /// isEquipped + mythicId 비공백 인스턴스에서 수집.
    /// </summary>
    public static class MythicEffectRuntime
    {
        public const string ID_DAD_COMPASS = "mythic_dad_compass";
        public const string ID_DAD_LULLABY = "mythic_dad_lullaby";
        public const string ID_DAD_COURAGE = "mythic_dad_courage";
        public const string ID_DAD_PROMISE = "mythic_dad_promise";

        public const float COMPASS_BOSS_DMG_MUL  = 1.20f;
        public const float LULLABY_DMG_TAKEN_MUL = 0.50f;
        public const float LULLABY_HP_THRESHOLD  = 0.30f;
        public const float COURAGE_DMG_MUL       = 1.50f;
        public const float COURAGE_HP_THRESHOLD  = 0.15f;
        public const float PROMISE_TRAIL_LENGTH_MUL = 2.0f;

        /// <summary>
        /// 플레이어 → 적 데미지 곱셈 보정. 적이 보스 / 처치 직전 시 신화 효과 적용.
        /// </summary>
        public static float CalcOutgoingDamageMultiplier(EnemyController target)
        {
            if (target == null) return 1f;
            float mul = 1f;
            if (HasMythic(ID_DAD_COMPASS) && (target.Data?.isBoss == true))
                mul *= COMPASS_BOSS_DMG_MUL;
            if (HasMythic(ID_DAD_COURAGE) && IsTargetNearDeath(target))
                mul *= COURAGE_DMG_MUL;
            return mul;
        }

        /// <summary>
        /// 적 → 플레이어 받는 데미지 곱셈 보정. 자장가 발동.
        /// </summary>
        public static float CalcIncomingDamageMultiplier(PlayerHealth player)
        {
            if (player == null) return 1f;
            float mul = 1f;
            if (HasMythic(ID_DAD_LULLABY) && IsPlayerLowHp(player))
                mul *= LULLABY_DMG_TAKEN_MUL;
            return mul;
        }

        /// <summary>Trail 자취 길이 곱셈 (TrailAttack 등이 생성 시 호출).</summary>
        public static float CalcTrailLengthMultiplier()
            => HasMythic(ID_DAD_PROMISE) ? PROMISE_TRAIL_LENGTH_MUL : 1f;

        /// <summary>F8 — 약속(약속의 부적) 발현 시 1회 무료 리롤 토큰 (세션당 1회).
        /// EquipmentCardPanel 의 Reroll 처리에서 차감 시도 — true 반환 시 별가루 비용 면제.
        /// 카드 패널 닫기/씬 전환 시 ResetPromiseFreeReroll 로 재충전.</summary>
        public static bool TryConsumePromiseFreeReroll()
        {
            if (!HasMythic(ID_DAD_PROMISE)) return false;
            if (_promiseFreeRerollUsed) return false;
            _promiseFreeRerollUsed = true;
            return true;
        }

        public static void ResetPromiseFreeReroll() => _promiseFreeRerollUsed = false;
        private static bool _promiseFreeRerollUsed;

        // ===== 헬퍼 =====

        public static bool HasMythic(string mythicId)
        {
            var data = MetaProgression.Instance?.Data;
            if (data?.equipmentInstances == null || string.IsNullOrEmpty(mythicId)) return false;
            for (int i = 0; i < data.equipmentInstances.Length; i++)
            {
                var inst = data.equipmentInstances[i];
                if (inst != null && inst.IsEquippedByCurrentClass() && inst.mythicId == mythicId) return true;
            }
            return false;
        }

        private static bool IsTargetNearDeath(EnemyController target)
            => target != null && target.HealthRatio <= COURAGE_HP_THRESHOLD;

        private static bool IsPlayerLowHp(PlayerHealth player)
            => player != null && player.HealthRatio <= LULLABY_HP_THRESHOLD;
    }
}
