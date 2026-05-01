using System.Collections;
using System.Collections.Generic;
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
        private AudioSource _bgmSrc;
        private AudioSource _bgmFadeSrc;  // crossfade 보조
        private string _currentBgm;
        private Coroutine _crossFadeCo;
        private readonly Dictionary<string, AudioClip> _bgmCache = new();
        private const float BGM_VOLUME = 0.4f;

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

            _bgmSrc = gameObject.AddComponent<AudioSource>();
            _bgmSrc.playOnAwake = false;
            _bgmSrc.loop = true;
            _bgmSrc.spatialBlend = 0f;
            _bgmSrc.volume = BGM_VOLUME;

            _bgmFadeSrc = gameObject.AddComponent<AudioSource>();
            _bgmFadeSrc.playOnAwake = false;
            _bgmFadeSrc.loop = true;
            _bgmFadeSrc.spatialBlend = 0f;
            _bgmFadeSrc.volume = 0f;

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

        // ===== BGM =====
        // Resources/BGM/<name>.ogg — 처음 호출 시 lazy load + 캐시
        public void PlayBgm(string name, float crossFadeSec = 1.5f)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (_currentBgm == name && _bgmSrc.isPlaying) return;
            AudioClip clip = LoadBgmClip(name);
            if (clip == null) return;
            if (_crossFadeCo != null) StopCoroutine(_crossFadeCo);
            if (!_bgmSrc.isPlaying || crossFadeSec <= 0f)
            {
                _bgmSrc.clip = clip;
                _bgmSrc.volume = BGM_VOLUME;
                _bgmSrc.Play();
            }
            else
            {
                _crossFadeCo = StartCoroutine(CrossFadeRoutine(clip, crossFadeSec));
            }
            _currentBgm = name;
        }

        public void StopBgm(float fadeSec = 0.5f)
        {
            if (_crossFadeCo != null) StopCoroutine(_crossFadeCo);
            if (fadeSec <= 0f)
            {
                _bgmSrc.Stop();
                _bgmFadeSrc.Stop();
            }
            else
            {
                _crossFadeCo = StartCoroutine(FadeOutRoutine(fadeSec));
            }
            _currentBgm = null;
        }

        // 챕터 카테고리 → BGM 이름 (chapter_structure_v2 §3 매핑)
        public static string GetChapterBgm(int chapterNumber)
        {
            string suffix = (chapterNumber % 2 == 0) ? "_a" : "_b";
            if (chapterNumber <= 5)  return "ch_meta"    + suffix;
            if (chapterNumber <= 12) return "ch_nature"  + suffix;
            if (chapterNumber <= 20) return "ch_animal"  + suffix;
            if (chapterNumber <= 28) return "ch_civ"     + suffix;
            return "ch_fantasy" + suffix;
        }

        private AudioClip LoadBgmClip(string name)
        {
            if (_bgmCache.TryGetValue(name, out var cached)) return cached;
            var clip = Resources.Load<AudioClip>("BGM/" + name);
            if (clip != null) _bgmCache[name] = clip;
            else Debug.LogWarning($"AudioManager: BGM not found Resources/BGM/{name}");
            return clip;
        }

        private IEnumerator CrossFadeRoutine(AudioClip newClip, float duration)
        {
            // 현재 → fadeSrc 로 옮겨 fade-out, 새 클립을 mainSrc 로 fade-in
            // 출시 전 fix #5 — prev clip null 가드 (clip 이 없는 audio source 에 .time 설정 시 경고 방지)
            var prev = _bgmSrc.clip;
            if (prev != null)
            {
                _bgmFadeSrc.clip = prev;
                _bgmFadeSrc.time = _bgmSrc.time;
                _bgmFadeSrc.volume = _bgmSrc.volume;
                _bgmFadeSrc.Play();
            }
            else
            {
                _bgmFadeSrc.Stop();
                _bgmFadeSrc.clip = null;
                _bgmFadeSrc.volume = 0f;
            }

            _bgmSrc.clip = newClip;
            _bgmSrc.volume = 0f;
            _bgmSrc.Play();

            float startVol = _bgmFadeSrc.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                _bgmFadeSrc.volume = Mathf.Lerp(startVol, 0f, k);
                _bgmSrc.volume     = Mathf.Lerp(0f, BGM_VOLUME, k);
                yield return null;
            }
            _bgmFadeSrc.Stop();
            _bgmSrc.volume = BGM_VOLUME;
            _crossFadeCo = null;
        }

        private IEnumerator FadeOutRoutine(float duration)
        {
            float startVol = _bgmSrc.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _bgmSrc.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
            _bgmSrc.Stop();
            _bgmSrc.volume = BGM_VOLUME;
            _crossFadeCo = null;
        }
    }
}
