using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// BGM 재생/cross-fade — AudioManager(MonoBehaviour) 가 AudioSource 2개 + Coroutine runner 주입.
    /// 5027 — AudioManager 분리. POCO + reference (5023 답습 정적 service 패턴).
    /// </summary>
    public class AudioBgmController
    {
        private const float BGM_VOLUME = 0.4f;

        private readonly AudioSource _main;
        private readonly AudioSource _fade;
        private readonly MonoBehaviour _runner;
        private readonly Dictionary<string, AudioClip> _cache = new();
        private string _current;
        private Coroutine _co;

        public AudioBgmController(AudioSource main, AudioSource fade, MonoBehaviour runner)
        {
            _main = main; _fade = fade; _runner = runner;
        }

        public void PlayBgm(string name, float crossFadeSec = 1.5f)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (_current == name && _main.isPlaying) return;
            AudioClip clip = LoadClip(name);
            if (clip == null) return;
            if (_co != null) _runner.StopCoroutine(_co);
            if (!_main.isPlaying || crossFadeSec <= 0f)
            {
                _main.clip = clip;
                _main.volume = BGM_VOLUME;
                _main.Play();
            }
            else
            {
                _co = _runner.StartCoroutine(CrossFadeRoutine(clip, crossFadeSec));
            }
            _current = name;
        }

        public void StopBgm(float fadeSec = 0.5f)
        {
            if (_co != null) _runner.StopCoroutine(_co);
            if (fadeSec <= 0f)
            {
                _main.Stop();
                _fade.Stop();
            }
            else
            {
                _co = _runner.StartCoroutine(FadeOutRoutine(fadeSec));
            }
            _current = null;
        }

        private AudioClip LoadClip(string name)
        {
            if (_cache.TryGetValue(name, out var cached)) return cached;
            var clip = Resources.Load<AudioClip>("BGM/" + name);
            if (clip != null) _cache[name] = clip;
            else Debug.LogWarning("AudioBgmController: BGM not found Resources/BGM/" + name);
            return clip;
        }

        private IEnumerator CrossFadeRoutine(AudioClip newClip, float duration)
        {
            var prev = _main.clip;
            if (prev != null)
            {
                _fade.clip = prev;
                _fade.time = _main.time;
                _fade.volume = _main.volume;
                _fade.Play();
            }
            else
            {
                _fade.Stop();
                _fade.clip = null;
                _fade.volume = 0f;
            }
            _main.clip = newClip;
            _main.volume = 0f;
            _main.Play();

            float startVol = _fade.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                _fade.volume = Mathf.Lerp(startVol, 0f, k);
                _main.volume = Mathf.Lerp(0f, BGM_VOLUME, k);
                yield return null;
            }
            _fade.Stop();
            _main.volume = BGM_VOLUME;
            _co = null;
        }

        private IEnumerator FadeOutRoutine(float duration)
        {
            float startVol = _main.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _main.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
            _main.Stop();
            _main.volume = BGM_VOLUME;
            _co = null;
        }
    }
}
