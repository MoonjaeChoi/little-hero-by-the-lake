using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// Phase G2 — 단순 VFX 컴포넌트. Sprite + 짧은 fade-out + 자동 destroy.
    /// 적 처치·피격·픽업 시각효과에 사용. CLAUDE.md §2 절대 원칙: 별·하트·꽃잎·비눗방울.
    /// 풀 사용 X — 1초 안에 destroy 되어 GC 부담 적음. 빈번 사용 시 ObjectPool 도입 권장.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleFx : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 0.6f;
        [SerializeField] private float _scaleStart = 0.3f;
        [SerializeField] private float _scaleEnd = 0.8f;
        [SerializeField] private bool _spin = true;
        [SerializeField] private float _spinSpeed = 360f;

        private SpriteRenderer _sr;
        private float _t;
        private Color _baseColor;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _baseColor = _sr.color;
        }

        private void OnEnable()
        {
            _t = 0f;
            transform.localScale = Vector3.one * _scaleStart;
            if (_sr != null) _sr.color = _baseColor;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            float k = Mathf.Clamp01(_t / _lifetime);
            // scale ease-out (작게 시작 → 크게 종료)
            float s = Mathf.Lerp(_scaleStart, _scaleEnd, 1f - (1f - k) * (1f - k));
            transform.localScale = new Vector3(s, s, 1f);
            // alpha fade (1.0 → 0.0)
            if (_sr != null)
            {
                var c = _baseColor;
                c.a = _baseColor.a * (1f - k);
                _sr.color = c;
            }
            if (_spin) transform.Rotate(0f, 0f, _spinSpeed * Time.deltaTime);
            if (_t >= _lifetime) Destroy(gameObject);
        }
    }
}
