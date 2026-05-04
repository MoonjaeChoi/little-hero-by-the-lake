using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5007 — 적 피격 시 (cfg, enemyData, damage, isCrit, weaponCategory, avgDamage) 입력으로
    /// ResolvedHitEffect 반환. EnemyController.OnHit 가 호출.
    /// 정적 헬퍼 — Singleton 직접 참조 X (§15.7 DI cfg 인자).
    /// </summary>
    public static class HitEffectResolver
    {
        // PG-12 시각 멀미 방지 cap (D4=B 채택, hardcode — SO 잘못된 값 방어)
        private const float MAX_PUNCH_SCALE = 2.0f;

        public static ResolvedHitEffect Resolve(
            CinematicEffectsConfig cfg,
            EnemyData enemyData,
            float damage,
            bool isCrit,
            WeaponCategory? weaponCategory = null,
            float avgPlayerDamage = 0f)
        {
            if (cfg == null) return Default(isCrit);

            // 1. Tier profile 선택 (BossTier 5종)
            TierEffectProfile profile;
            switch (enemyData != null ? enemyData.bossTier : BossTier.None)
            {
                case BossTier.MiniBoss:    profile = cfg.miniBossProfile; break;
                case BossTier.ChapterBoss: profile = cfg.chapterBossProfile; break;
                case BossTier.DailyBoss:   profile = cfg.dailyBossProfile; break;
                case BossTier.WeeklyBoss:  profile = cfg.weeklyBossProfile; break;
                default:                   profile = cfg.noneProfile; break;
            }

            // 2. damage intensity (D3=B) — ratio cap 5×, weight 0.4
            float intensityFactor = 1f;
            if (avgPlayerDamage > 0.01f && cfg.damageIntensityCap > 0.01f)
            {
                float ratio = damage / avgPlayerDamage;
                float intensity = Mathf.Clamp01(ratio / cfg.damageIntensityCap);
                intensityFactor = 1f + intensity * cfg.damageIntensityWeight;
            }

            // 3. weapon flash color (D4=B) — 4 그룹 분기
            Color flashColor;
            if (isCrit) flashColor = cfg.critFlashColor;
            else if (!weaponCategory.HasValue) flashColor = cfg.swordFlashColor;
            else
            {
                switch (weaponCategory.Value)
                {
                    case WeaponCategory.Bow:
                    case WeaponCategory.Crossbow:
                        flashColor = cfg.bowFlashColor; break;
                    case WeaponCategory.Dagger:
                    case WeaponCategory.Boomerang:
                        flashColor = cfg.daggerFlashColor; break;
                    case WeaponCategory.Staff:
                    case WeaponCategory.Wand:
                    case WeaponCategory.Tome:
                        flashColor = cfg.magicFlashColor; break;
                    default:
                        flashColor = cfg.swordFlashColor; break;
                }
            }

            // 4. 최종 — cap 2.0× hardcode (D4)
            float basePunch = isCrit ? cfg.critPunchScale : cfg.normalPunchScale;
            float finalPunch = Mathf.Min(MAX_PUNCH_SCALE, basePunch * profile.punchMul * intensityFactor);

            return new ResolvedHitEffect
            {
                PunchScale     = finalPunch,
                FlashColor     = flashColor,
                VfxScale       = (isCrit ? cfg.vfxCritScale : cfg.vfxNormalScale) * profile.vfxScaleMul,
                FloaterSizeMul = profile.floaterSizeMul,
                ShakeAmplitude = cfg.shakeAmplitude * profile.shakeAmpMul * intensityFactor,
                EnableHitPause = isCrit,
                EnableZoom     = isCrit && cfg.enableZoomOnCrit
            };
        }

        private static ResolvedHitEffect Default(bool isCrit) => new ResolvedHitEffect
        {
            PunchScale = isCrit ? 1.7f : 1.4f,
            FlashColor = isCrit ? Color.yellow : Color.red,
            VfxScale = isCrit ? 1.4f : 0.6f,
            FloaterSizeMul = 1.0f,
            ShakeAmplitude = 0.3f,
            EnableHitPause = isCrit,
            EnableZoom = false
        };
    }

    /// <summary>5007 — HitEffectResolver 결과 (struct, no boxing).</summary>
    public struct ResolvedHitEffect
    {
        public float PunchScale;
        public Color FlashColor;
        public float VfxScale;
        public float FloaterSizeMul;
        public float ShakeAmplitude;
        public bool  EnableHitPause;
        public bool  EnableZoom;
    }
}
