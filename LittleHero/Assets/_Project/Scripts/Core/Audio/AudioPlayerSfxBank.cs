using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 플레이어/일반 SFX 7종 절차적 합성 (5027 — AudioManager 분리).
    /// Pop / Pickup / LevelUp / Memory / Hit / EnemyHit / CritHit.
    /// </summary>
    public static class AudioPlayerSfxBank
    {
        private const int SR = 44100;

        public static AudioClip Pop()
        {
            int len = (int)(SR * 0.08f);
            var s = new float[len];
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 22f);
                float wave = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(900f, 400f, t / 0.08f) * t);
                s[i] = wave * env * 0.4f;
            }
            var c = AudioClip.Create("sfx_pop", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        public static AudioClip Pickup()
        {
            int len = (int)(SR * 0.12f);
            var s = new float[len];
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 8f);
                float wave = Mathf.Sin(2f * Mathf.PI * (660f + 220f * t) * t);
                s[i] = wave * env * 0.5f;
            }
            var c = AudioClip.Create("sfx_pickup", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        public static AudioClip LevelUp()
        {
            int len = (int)(SR * 0.4f);
            var s = new float[len];
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 5f);
                float f1 = 523f, f2 = 659f, f3 = 784f;
                float wave = (Mathf.Sin(2f * Mathf.PI * f1 * t) + Mathf.Sin(2f * Mathf.PI * f2 * t) + Mathf.Sin(2f * Mathf.PI * f3 * t)) / 3f;
                s[i] = wave * env * 0.5f;
            }
            var c = AudioClip.Create("sfx_levelup", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        public static AudioClip Memory()
        {
            int len = (int)(SR * 0.6f);
            var s = new float[len];
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float env = Mathf.Sin(Mathf.PI * t / 0.6f);
                float shimmer = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 6f * t);
                float wave = Mathf.Sin(2f * Mathf.PI * 880f * t) * shimmer;
                s[i] = wave * env * 0.3f;
            }
            var c = AudioClip.Create("sfx_memory", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        public static AudioClip Hit()
        {
            int len = (int)(SR * 0.15f);
            var samples = new float[len];
            var rng = new System.Random(3);
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 14f);
                float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.6f;
                float wave = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.4f;
                samples[i] = (noise + wave) * env;
            }
            var clip = AudioClip.Create("sfx_hit", len, 1, SR, false);
            clip.SetData(samples, 0); return clip;
        }

        public static AudioClip EnemyHit()
        {
            int len = (int)(SR * 0.07f);
            var s = new float[len];
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 35f);
                float wave = Mathf.Sin(2f * Mathf.PI * 350f * t);
                float noise = (Random.value - 0.5f) * 0.3f;
                s[i] = (wave + noise) * 0.45f * env;
            }
            var c = AudioClip.Create("sfx_enemy_hit", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        public static AudioClip CritHit()
        {
            int len = (int)(SR * 0.12f);
            var s = new float[len];
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 18f);
                float f1 = 1200f, f2 = 1600f;
                float wave = (Mathf.Sin(2f * Mathf.PI * f1 * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * f2 * t)) / 1.5f;
                float noise = (Random.value - 0.5f) * 0.2f;
                s[i] = (wave + noise) * 0.55f * env;
            }
            var c = AudioClip.Create("sfx_crit_hit", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }
    }
}
