using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 데미지 수치 부동 텍스트 (cinematic_combat_v1.md §4).
    /// WorldSpace Canvas + UnityEngine.UI.Text (Legacy, CLAUDE.md §12).
    /// 코드 애니메이션: 위로 이동 + alpha fade. Critical 시 진동 + 큰 크기.
    /// </summary>
    public class DamageNumberFloater : MonoBehaviour
    {
        private Canvas _canvas;
        private Text _text;
        private CanvasGroup _cg;
        private float _lifeTime;
        private float _maxLifeTime;
        private Vector3 _startPos;
        private Vector3 _endPos;
        private bool _isCrit;
        private float _shakePhase;

        public void EnsureSetup()
        {
            if (_canvas != null) return;
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.sortingOrder = 100;
            transform.localScale = Vector3.one * 0.05f;  // WorldSpace Canvas — fontSize 24 × 0.05 = 1.2 unit (~7.5% screen)

            var go = new GameObject("Text");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200f, 60f);
            rt.anchoredPosition = Vector2.zero;
            _text = go.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _text.fontSize = 24;
            _text.alignment = TextAnchor.MiddleCenter;
            _text.color = Color.white;
            _text.horizontalOverflow = HorizontalWrapMode.Overflow;
            _text.verticalOverflow = VerticalWrapMode.Overflow;

            // Outline 검정 — 가독성
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2f, -2f);

            _cg = gameObject.AddComponent<CanvasGroup>();
        }

        public void Initialize(int damage, DamageType type, Vector3 worldPos)
        {
            EnsureSetup();
            _isCrit = type == DamageType.Critical;
            _maxLifeTime = _isCrit ? 1.0f : 0.6f;
            _lifeTime = 0f;

            float xOff = Random.Range(-0.3f, 0.3f);
            _startPos = worldPos + new Vector3(xOff, 0f, 0f);
            float upDist = _isCrit ? 1.5f : 1.0f;
            _endPos = _startPos + new Vector3(0f, upDist, 0f);
            transform.position = _startPos;

            _text.text = FormatDamage(damage, type);
            _text.color = ColorFor(type);
            _text.fontSize = FontSizeFor(type);
            _cg.alpha = 1f;
            _shakePhase = Random.Range(0f, Mathf.PI * 2f);

            // Critical 진입 punch — scale 0.5 → 1.0 (베이스 0.05)
            transform.localScale = Vector3.one * (_isCrit ? 0.025f : 0.05f);
        }

        private static string FormatDamage(int damage, DamageType type)
        {
            string prefix = type == DamageType.Heal ? "+" : "";
            string body;
            int abs = Mathf.Abs(damage);
            if (abs >= 1_000_000) body = (damage / 1_000_000f).ToString("F1") + "M";
            else if (abs >= 10_000) body = (damage / 1000) + "K";
            else if (abs >= 1_000) body = (damage / 1000f).ToString("F1") + "K";
            else body = damage.ToString();
            if (type == DamageType.Critical) return body + "!";
            return prefix + body;
        }

        private static Color ColorFor(DamageType type)
        {
            switch (type)
            {
                case DamageType.Critical: return new Color(1f, 0.84f, 0f);     // 골드
                case DamageType.DOT:      return new Color(0.65f, 0.42f, 0.79f); // 보라
                case DamageType.Heal:     return new Color(0.29f, 0.87f, 0.5f);  // 녹
                case DamageType.Block:    return new Color(0.53f, 0.53f, 0.53f); // 회
                default:                  return Color.white;
            }
        }

        private static int FontSizeFor(DamageType type)
        {
            switch (type)
            {
                case DamageType.Critical: return 36;
                case DamageType.DOT:      return 20;
                case DamageType.Block:    return 20;
                default:                  return 24;
            }
        }

        private void Update()
        {
            _lifeTime += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(_lifeTime / _maxLifeTime);
            // 위로 부동 — OutQuad
            float ease = 1f - (1f - k) * (1f - k);
            transform.position = Vector3.Lerp(_startPos, _endPos, ease);

            // Critical 진입 punch — 0.5→1.0 in 0.15s (베이스 0.05, Critical 0.07)
            if (_isCrit && _lifeTime < 0.15f)
            {
                float p = _lifeTime / 0.15f;
                float scale = Mathf.Lerp(0.025f, 0.07f, p);
                transform.localScale = Vector3.one * scale;
                // 진동
                float shake = Mathf.Sin(_shakePhase + Time.time * 30f) * 0.05f * (1f - p);
                transform.position += new Vector3(shake, 0f, 0f);
            }
            else if (_isCrit)
            {
                transform.localScale = Vector3.one * 0.07f;
            }

            // Fade out — 0.4s after delay 0.2s (일반) / 0.5s after delay 0.3s (크리)
            float fadeDelay = _isCrit ? 0.3f : 0.2f;
            float fadeDur = _isCrit ? 0.5f : 0.4f;
            if (_lifeTime > fadeDelay)
            {
                float fk = Mathf.Clamp01((_lifeTime - fadeDelay) / fadeDur);
                _cg.alpha = 1f - fk;
            }

            if (_lifeTime >= _maxLifeTime)
            {
                gameObject.SetActive(false);
                DamageFloaterPool.Return(this);
            }
        }
    }
}
