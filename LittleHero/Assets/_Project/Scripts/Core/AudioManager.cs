using UnityEngine;

namespace LittleHero
{
    public enum SfxType
    {
        EnemyPop,    // 적 처치 "퐁!" (CLAUDE.md §2 절대 원칙)
        Pickup,      // 별가루·아이템 픽업
        LevelUp,     // 레벨업 상승 아르페지오
        MemoryFound, // 추억 발견 — 따뜻한 차임
        PlayerHit,   // 플레이어 피격 (가벼운 만화풍)
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _src;
        private AudioClip _popClip;
        private AudioClip _pickupClip;
        private AudioClip _levelUpClip;
        private AudioClip _memoryClip;
        private AudioClip _hitClip;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<AudioManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _src = gameObject.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.loop = false;
            _src.spatialBlend = 0f;
            _src.volume = 0.6f;

            _popClip      = SynthPop();
            _pickupClip   = SynthPickup();
            _levelUpClip  = SynthLevelUp();
            _memoryClip   = SynthMemory();
            _hitClip      = SynthHit();

            GameEvents.OnEnemyKilled       += _ => Play(SfxType.EnemyPop);
            GameEvents.OnLevelUp           += _ => Play(SfxType.LevelUp);
            GameEvents.OnMemoryDiscovered  += _ => Play(SfxType.MemoryFound);
            GameEvents.OnPlayerHealthChanged += _ => { /* hit 은 호출자가 직접 Play */ };
        }

        public void Play(SfxType type)
        {
            if (_src == null) return;
            AudioClip clip = type switch
            {
                SfxType.EnemyPop    => _popClip,
                SfxType.Pickup      => _pickupClip,
                SfxType.LevelUp     => _levelUpClip,
                SfxType.MemoryFound => _memoryClip,
                SfxType.PlayerHit   => _hitClip,
                _ => null,
            };
            if (clip != null) _src.PlayOneShot(clip);
        }

        private const int SR = 44100;

        // 적 처치 "퐁!" — 880→220Hz 빠른 sweep + exp decay (0.15s). 부드럽고 비폭력 톤.
        private static AudioClip SynthPop()
        {
            int len = (int)(SR * 0.15f);
            var samples = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / SR;
                float k = (float)i / len;
                float freq = Mathf.Lerp(880f, 220f, k * k);
                float env = Mathf.Exp(-t * 18f);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                samples[i] = wave * env * 0.7f;
            }
            var clip = AudioClip.Create("sfx_pop", len, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // 픽업 차임 — 1320 + 1760Hz 짧은 듀얼 사인 (0.1s)
        private static AudioClip SynthPickup()
        {
            int len = (int)(SR * 0.1f);
            var samples = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 25f);
                float a = Mathf.Sin(2f * Mathf.PI * 1320f * t);
                float b = Mathf.Sin(2f * Mathf.PI * 1760f * t);
                samples[i] = (a + b) * 0.3f * env;
            }
            var clip = AudioClip.Create("sfx_pickup", len, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // 레벨업 — C5(523) → E5(659) → G5(784) → C6(1047) 4음 아르페지오 (0.4s)
        private static AudioClip SynthLevelUp()
        {
            int len = (int)(SR * 0.4f);
            var samples = new float[len];
            float[] freqs = { 523.25f, 659.25f, 783.99f, 1046.5f };
            int noteLen = len / 4;
            for (int i = 0; i < len; i++)
            {
                int note = Mathf.Min(i / noteLen, 3);
                float t = (float)(i - note * noteLen) / SR;
                float env = Mathf.Exp(-t * 8f);
                float wave = Mathf.Sin(2f * Mathf.PI * freqs[note] * t);
                samples[i] = wave * env * 0.5f;
            }
            var clip = AudioClip.Create("sfx_levelup", len, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // 추억 발견 — A4(440) + E5(659) 동시 + 긴 fade (0.6s). 따뜻한 정서.
        private static AudioClip SynthMemory()
        {
            int len = (int)(SR * 0.6f);
            var samples = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 4f) * (1f - Mathf.Exp(-t * 30f)); // attack-decay
                float a = Mathf.Sin(2f * Mathf.PI * 440f * t);
                float b = Mathf.Sin(2f * Mathf.PI * 659.25f * t);
                samples[i] = (a + b) * 0.3f * env;
            }
            var clip = AudioClip.Create("sfx_memory", len, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // 피격 — 200Hz 짧은 펄스 + 약한 노이즈 (0.08s, 만화풍 KO)
        private static AudioClip SynthHit()
        {
            int len = (int)(SR * 0.08f);
            var samples = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 30f);
                float wave = Mathf.Sin(2f * Mathf.PI * 200f * t);
                float noise = (Random.value - 0.5f) * 0.4f;
                samples[i] = (wave + noise) * 0.5f * env;
            }
            var clip = AudioClip.Create("sfx_hit", len, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
