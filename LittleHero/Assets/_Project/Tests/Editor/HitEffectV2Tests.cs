using System;
using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5007 damage_hit_effect_v2 — HitEffectResolver 정적 검증 (8 NUnit).
    /// CLAUDE.md §15.9 + Phase 4 Contract C2/C4.
    /// </summary>
    public class HitEffectV2Tests
    {
        private CinematicEffectsConfig MakeCfg()
        {
            var cfg = ScriptableObject.CreateInstance<CinematicEffectsConfig>();
            // default 값으로 초기화 (Inspector 처럼)
            cfg.normalPunchScale = 1.4f;
            cfg.critPunchScale = 1.7f;
            cfg.normalFlashColor = Color.red;
            cfg.critFlashColor = Color.yellow;
            cfg.shakeAmplitude = 0.3f;
            cfg.vfxNormalScale = 0.6f;
            cfg.vfxCritScale = 1.4f;
            cfg.damageIntensityCap = 5.0f;
            cfg.damageIntensityWeight = 0.4f;
            cfg.swordFlashColor = Color.red;
            cfg.bowFlashColor = new Color(0.4f, 0.9f, 1.0f);
            cfg.daggerFlashColor = new Color(1.0f, 0.6f, 0.3f);
            cfg.magicFlashColor = new Color(0.8f, 0.4f, 1.0f);
            cfg.noneProfile        = new TierEffectProfile { punchMul=1.0f,  vfxScaleMul=1.0f,  floaterSizeMul=1.0f,  shakeAmpMul=1.0f };
            cfg.miniBossProfile    = new TierEffectProfile { punchMul=1.10f, vfxScaleMul=1.20f, floaterSizeMul=1.15f, shakeAmpMul=1.20f };
            cfg.chapterBossProfile = new TierEffectProfile { punchMul=1.25f, vfxScaleMul=1.50f, floaterSizeMul=1.35f, shakeAmpMul=1.50f };
            cfg.dailyBossProfile   = new TierEffectProfile { punchMul=1.30f, vfxScaleMul=1.60f, floaterSizeMul=1.40f, shakeAmpMul=1.60f };
            cfg.weeklyBossProfile  = new TierEffectProfile { punchMul=1.40f, vfxScaleMul=1.80f, floaterSizeMul=1.50f, shakeAmpMul=1.80f };
            return cfg;
        }

        private EnemyData MakeEnemy(BossTier tier)
        {
            var d = ScriptableObject.CreateInstance<EnemyData>();
            d.bossTier = tier;
            return d;
        }

        [Test]
        public void CinematicEffectsConfig_Type_Exists()
        {
            Assert.NotNull(Type.GetType("LittleHero.CinematicEffectsConfig, Assembly-CSharp"));
        }

        [Test]
        public void HitEffectResolver_Type_Exists()
        {
            Assert.NotNull(Type.GetType("LittleHero.HitEffectResolver, Assembly-CSharp"));
        }

        [Test]
        public void ResolvedHitEffect_IsStruct_NoBoxing()
        {
            var t = Type.GetType("LittleHero.ResolvedHitEffect, Assembly-CSharp");
            Assert.NotNull(t);
            Assert.IsTrue(t.IsValueType, "ResolvedHitEffect must be struct (no boxing)");
        }

        [Test]
        public void Resolver_NoneTier_NormalHit_BasePunch_1_4()
        {
            var cfg = MakeCfg();
            var enemy = MakeEnemy(BossTier.None);
            var r = HitEffectResolver.Resolve(cfg, enemy, 100f, false, null, 100f);
            // intensity = 1/5 = 0.2, factor = 1 + 0.2*0.4 = 1.08
            // punchScale = 1.4 × 1.0 × 1.08 = 1.512
            Assert.AreEqual(1.512f, r.PunchScale, 0.01f);
        }

        [Test]
        public void Resolver_ChapterBoss_AmplifiedPunch_GreaterThanBase()
        {
            var cfg = MakeCfg();
            var none  = HitEffectResolver.Resolve(cfg, MakeEnemy(BossTier.None),        100f, false, null, 100f);
            var boss  = HitEffectResolver.Resolve(cfg, MakeEnemy(BossTier.ChapterBoss), 100f, false, null, 100f);
            Assert.Greater(boss.PunchScale, none.PunchScale, "ChapterBoss punch must be greater than None");
        }

        [Test]
        public void Resolver_DamageIntensity_5xRatio_FullIntensity()
        {
            var cfg = MakeCfg();
            var enemy = MakeEnemy(BossTier.None);
            // damage=500, avgPlayerDamage=100 → ratio=5.0 → intensity 1.0 → factor 1.4
            var r = HitEffectResolver.Resolve(cfg, enemy, 500f, false, null, 100f);
            Assert.AreEqual(1.4f * 1.0f * 1.4f, r.PunchScale, 0.01f);  // base × tier × intensity
        }

        [Test]
        public void Resolver_BowWeapon_FlashColor_Cyan()
        {
            var cfg = MakeCfg();
            var enemy = MakeEnemy(BossTier.None);
            var r = HitEffectResolver.Resolve(cfg, enemy, 100f, false, WeaponCategory.Bow, 100f);
            Assert.AreEqual(cfg.bowFlashColor, r.FlashColor);
        }

        [Test]
        public void Resolver_NullWeapon_FlashColor_Sword_Default()
        {
            var cfg = MakeCfg();
            var enemy = MakeEnemy(BossTier.None);
            var r = HitEffectResolver.Resolve(cfg, enemy, 100f, false, null, 100f);
            Assert.AreEqual(cfg.swordFlashColor, r.FlashColor);
        }
    }
}
