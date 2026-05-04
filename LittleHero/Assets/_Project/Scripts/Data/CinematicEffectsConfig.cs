using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5007 — 적 타격 데미지 이펙트 통합 SO. PIVOT v4 Cinematic Combat 의 모든 magic number 외부화.
    /// 디자이너가 Inspector 에서 튜닝 + BossTier 5종 × 효과별 4 multiplier 매트릭스.
    /// 호출자: HitEffectResolver.Resolve, CinematicEffects, DamageFloaterPool prewarm.
    /// </summary>
    [CreateAssetMenu(fileName = "CinematicEffectsConfig", menuName = "LittleHero/Combat/Cinematic Effects Config")]
    public class CinematicEffectsConfig : ScriptableObject
    {
        [Header("Punch (적 몸체 확대)")]
        public float normalPunchScale = 1.4f;
        public float critPunchScale = 1.7f;
        public float punchUpDuration = 0.05f;
        public float punchDownDuration = 0.10f;
        public float punchUpCritDuration = 0.07f;
        public float punchDownCritDuration = 0.15f;

        [Header("Flash (색감)")]
        public Color normalFlashColor = Color.red;
        public Color critFlashColor = Color.yellow;

        [Header("Hit Pause (Slow-Mo, 크리만)")]
        public float slowMoScale = 0.05f;
        public float slowMoDuration = 0.08f;

        [Header("Screen Shake")]
        public float shakeAmplitude = 0.3f;
        public float shakeDuration = 0.2f;

        [Header("Camera Zoom (D7=B 신규, 크리만)")]
        public bool enableZoomOnCrit = true;
        public float zoomMul = 1.05f;
        public float zoomUpDuration = 0.05f;
        public float zoomDownDuration = 0.15f;

        [Header("VFX Hit Star")]
        public float vfxNormalScale = 0.6f;
        public float vfxCritScale = 1.4f;

        [Header("Pool 크기 (D5 부족 해결)")]
        public int floaterPoolSize = 300;
        public int vfxPoolSize = 100;

        [Header("Tier Profile (D2 적 티어 차별화)")]
        public TierEffectProfile noneProfile        = new TierEffectProfile { punchMul = 1.0f,  vfxScaleMul = 1.0f,  floaterSizeMul = 1.0f,  shakeAmpMul = 1.0f };
        public TierEffectProfile miniBossProfile    = new TierEffectProfile { punchMul = 1.10f, vfxScaleMul = 1.20f, floaterSizeMul = 1.15f, shakeAmpMul = 1.20f };
        public TierEffectProfile chapterBossProfile = new TierEffectProfile { punchMul = 1.25f, vfxScaleMul = 1.50f, floaterSizeMul = 1.35f, shakeAmpMul = 1.50f };
        public TierEffectProfile dailyBossProfile   = new TierEffectProfile { punchMul = 1.30f, vfxScaleMul = 1.60f, floaterSizeMul = 1.40f, shakeAmpMul = 1.60f };
        public TierEffectProfile weeklyBossProfile  = new TierEffectProfile { punchMul = 1.40f, vfxScaleMul = 1.80f, floaterSizeMul = 1.50f, shakeAmpMul = 1.80f };

        [Header("Damage Intensity (D3=B)")]
        public float damageIntensityCap = 5.0f;       // damage / avg cap (5× 이상 동일)
        public float damageIntensityWeight = 0.4f;    // intensity → effect intensity factor

        [Header("Weapon Category Color (D4=B)")]
        public Color swordFlashColor = Color.red;
        public Color bowFlashColor = new Color(0.4f, 0.9f, 1.0f);
        public Color daggerFlashColor = new Color(1.0f, 0.6f, 0.3f);
        public Color magicFlashColor = new Color(0.8f, 0.4f, 1.0f);

        [Header("Hit Stun (D6=C 디바운스 only)")]
        public float hitDebounceTime = 0.05f;
    }

    [System.Serializable]
    public struct TierEffectProfile
    {
        public float punchMul;
        public float vfxScaleMul;
        public float floaterSizeMul;
        public float shakeAmpMul;
    }
}
