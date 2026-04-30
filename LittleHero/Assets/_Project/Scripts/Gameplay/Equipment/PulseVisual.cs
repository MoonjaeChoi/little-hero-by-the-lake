using UnityEngine;

namespace LittleHero
{
    public class PulseVisual : MonoBehaviour
    {
        private float _maxRadius;
        private float _duration;
        private float _elapsed;
        private System.Action<PulseVisual> _onDone;
        private SpriteRenderer _sr;

        private void Awake() { _sr = GetComponent<SpriteRenderer>(); }

        public void Play(float maxRadius, float duration, System.Action<PulseVisual> onDone)
        {
            _maxRadius = maxRadius;
            _duration = Mathf.Max(0.05f, duration);
            _elapsed = 0f;
            _onDone = onDone;
            transform.localScale = Vector3.one * 0.1f;
            if (_sr != null) _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, 1f);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / _duration;
            transform.localScale = Vector3.one * Mathf.Lerp(0.1f, _maxRadius, t);
            if (_sr != null)
            {
                Color c = _sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                _sr.color = c;
            }
            if (_elapsed >= _duration) _onDone?.Invoke(this);
        }
    }
}
