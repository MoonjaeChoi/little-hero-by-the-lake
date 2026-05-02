using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public enum SfxType
    {
        EnemyPop,
        Pickup,
        LevelUp,
        MemoryFound,
        PlayerHit,
        // 보스 공격 (boss_attack_v1.md 사이클 C)
        BossTelegraph,  // 1.5s warning chime
        BossDetonate,   // 폭발 boom
        BossCharge,     // 돌진 swoosh
        BossSummon,     // 소환 pop
        BossTornado,    // 회전 영역 (단발 — loop 미사용 단순화)
        // PIVOT v4 Premium 피격 (cinematic_combat_v1.md §3)
        EnemyHit,       // 일반 적 피격 — 짧은 thud + 노이즈
        CritHit,        // 크리티컬 — 띵! 청량한 high tone + 노이즈
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
        private AudioClip _bossTelegraphClip;
        private AudioClip _bossDetonateClip;
        private AudioClip _bossChargeClip;
        private AudioClip _bossSummonClip;
        private AudioClip _bossTornadoClip;
        private AudioClip _enemyHitClip;
        private AudioClip _critHitClip;

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
            _bossTelegraphClip = SynthBossTelegraph();
            _bossDetonateClip  = SynthBossDetonate();
            _bossChargeClip    = SynthBossCharge();
            _bossSummonClip    = SynthBossSummon();
            _bossTornadoClip   = SynthBossTornado();
            _enemyHitClip      = SynthEnemyHit();
            _critHitClip       = SynthCritHit();

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
                SfxType.EnemyPop      => _popClip,
                SfxType.Pickup        => _pickupClip,
                SfxType.LevelUp       => _levelUpClip,
                SfxType.MemoryFound   => _memoryClip,
                SfxType.PlayerHit     => _hitClip,
                SfxType.BossTelegraph => _bossTelegraphClip,
                SfxType.BossDetonate  => _bossDetonateClip,
                SfxType.BossCharge    => _bossChargeClip,
                SfxType.BossSummon    => _bossSummonClip,
                SfxType.BossTornado   => _bossTornadoClip,
                SfxType.EnemyHit      => _enemyHitClip,
                SfxType.CritHit       => _critHitClip,
                _ => null,
            };
            if (clip != null) _src.PlayOneShot(clip);
        }

        // === 보스 SFX 5종 (절차적 합성, Phase G2 P9 패턴) ===

        // 위협 chime — 600Hz 단음 진동 (vibrato), 1s
        private static AudioClip SynthBossTelegraph()
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

        // 폭발 boom — 200→50Hz sweep + 노이즈 + exp decay 0.4s
        private static AudioClip SynthBossDetonate()
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

        // 돌진 swoosh — 1500→400Hz fast sweep, 0.5s, 노이즈 부분
        private static AudioClip SynthBossCharge()
        {
            int len = (int)(SR * 0.5f);
            var s = new float[len];
            var rng = new System.Random(11);
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float k = t / 0.5f;
                float freq = Mathf.Lerp(1500f, 400f, k);
                float env = Mathf.Sin(Mathf.PI * k);  // attack-decay
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.4f;
                s[i] = (wave * 0.6f + noise * env) * env * 0.6f;
            }
            var c = AudioClip.Create("BossCharge", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        // 소환 pop — 440 + 660Hz 듀얼 + 짧은 attack-release 0.2s
        private static AudioClip SynthBossSummon()
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

        // 회전 — 200Hz wobble + 화이트 노이즈 1초
        private static AudioClip SynthBossTornado()
        {
            int len = (int)(SR * 1.0f);
            var s = new float[len];
            var rng = new System.Random(13);
            for (int i = 0; i < len; i++) {
                float t = (float)i / SR;
                float wobble = Mathf.Sin(2f * Mathf.PI * 4f * t) * 50f;
                float wave = Mathf.Sin(2f * Mathf.PI * (200f + wobble) * t);
                float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.5f;
                float env = Mathf.Sin(Mathf.PI * t);  // 부드러운 fade in/out
                s[i] = (wave * 0.5f + noise * 0.5f) * env * 0.5f;
            }
            var c = AudioClip.Create("BossTornado", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        private const int SR = 44100;

        // === PIVOT v4 — Premium 피격 SFX 2종 ===

        // 일반 적 피격 — 350Hz 짧은 thud + 약한 노이즈 0.07s
        private static AudioClip SynthEnemyHit()
        {
            int len = (int)(SR * 0.07f);
            var s = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 35f);
                float wave = Mathf.Sin(2f * Mathf.PI * 350f * t);
                float noise = (Random.value - 0.5f) * 0.3f;
                s[i] = (wave + noise) * 0.45f * env;
            }
            var c = AudioClip.Create("sfx_enemy_hit", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }

        // 크리티컬 — 1200+1800Hz 듀얼 + 짧은 attack-decay 0.15s (청량한 띵!)
        private static AudioClip SynthCritHit()
        {
            int len = (int)(SR * 0.15f);
            var s = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / SR;
                float env = Mathf.Exp(-t * 14f) * (1f - Mathf.Exp(-t * 80f));
                float a = Mathf.Sin(2f * Mathf.PI * 1200f * t);
                float b = Mathf.Sin(2f * Mathf.PI * 1800f * t);
                float noise = (Random.value - 0.5f) * 0.2f * env;
                s[i] = (a + b * 0.7f) * 0.35f * env + noise;
            }
            var c = AudioClip.Create("sfx_crit_hit", len, 1, SR, false);
            c.SetData(s, 0); return c;
        }


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
