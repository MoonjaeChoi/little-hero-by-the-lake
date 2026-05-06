using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 보스 SFX 5종 절차적 합성 (boss_attack_v1.md 사이클 C).
    /// 5027 — AudioManager 분리.
    /// </summary>
    public static class AudioBossSfxBank
    {
        private const int SR = 44100;

        public static AudioClip Telegraph()
        {
            int len = (int)(SR * 1.0f);
            var s = new float[len];
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float vib = Mathf.Sin(2f * Mathf.PI * 8f * t) * 30f;
                float wave = Mathf.Sin(2f * Mathf.PI * (600f + vib) * t);
                float env = Mathf.Lerp(0.6f, 0.2f, t);
                s[i] = wave * env * 0.5f;
            }
            var c = AudioClip.Create("BossTelegraph", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        public static AudioClip Detonate()
        {
            int len = (int)(SR * 0.4f);
            var s = new float[len];
            var rng = new System.Random(7);
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float k = t / 0.4f;
                float freq = Mathf.Lerp(200f, 50f, k * k);
                float env = Mathf.Exp(-t * 8f);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.3f;
                s[i] = (wave + noise) * env * 0.8f;
            }
            var c = AudioClip.Create("BossDetonate", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        public static AudioClip Charge()
        {
            int len = (int)(SR * 0.5f);
            var s = new float[len];
            var rng = new System.Random(11);
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float k = t / 0.5f;
                float freq = Mathf.Lerp(1500f, 400f, k);
                float env = Mathf.Sin(Mathf.PI * k);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.4f;
                s[i] = (wave * 0.6f + noise * env) * env * 0.6f;
            }
            var c = AudioClip.Create("BossCharge", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        public static AudioClip Summon()
        {
            int len = (int)(SR * 0.2f);
            var s = new float[len];
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 12f);
                float wave = Mathf.Sin(2f * Mathf.PI * 440f * t) + 0.7f * Mathf.Sin(2f * Mathf.PI * 660f * t);
                s[i] = wave * env * 0.4f;
            }
            var c = AudioClip.Create("BossSummon", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        public static AudioClip Tornado()
        {
            int len = (int)(SR * 1.0f);
            var s = new float[len];
            var rng = new System.Random(13);
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float wobble = Mathf.Sin(2f * Mathf.PI * 4f * t) * 50f;
                float wave = Mathf.Sin(2f * Mathf.PI * (200f + wobble) * t);
                float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.5f;
                float env = Mathf.Sin(Mathf.PI * t);
                s[i] = (wave * 0.5f + noise * 0.5f) * env * 0.5f;
            }
            var c = AudioClip.Create("BossTornado", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }
    }
}
