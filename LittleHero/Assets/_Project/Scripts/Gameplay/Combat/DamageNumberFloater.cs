using System.Collections.Generic;
using System.Text;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 데미지 수치 부동 텍스트 (cinematic_combat_v1.md §4).
    /// WorldSpace Canvas + UnityEngine.UI.Text (Legacy, CLAUDE.md §12).
    /// 5016 T04 — Update() 매 프레임 Lerp + new Vector3 폐기 → LMotion (init 시 1회 schedule).
    /// 핫 패스 GC alloc ↓ (매 hit Update callback 폐기).
    /// </summary>
    public class DamageNumberFloater : MonoBehaviour
    {
        private Canvas _canvas;
        private Text _text;
        private CanvasGroup _cg;
        private Vector3 _shakeOrigin;

        // 5016 T04 — 동시 활성 motion 보유 + 풀 반환 시 cancel
        private MotionHandle _moveHandle;
        private MotionHandle _alphaHandle;
        private MotionHandle _scaleHandle;
        private MotionHandle _shakeHandle;

        public void EnsureSetup()
        {
            if (_canvas != null) return;
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.sortingOrder = 100;
            transform.localScale = Vector3.one * 0.05f;

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

            var outline = go.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2f, -2f);

            _cg = gameObject.AddComponent<CanvasGroup>();
        }

        public void Initialize(int damage, DamageType type, Vector3 worldPos, float sizeMul = 1f)
        {
            EnsureSetup();
            CancelAll();

            bool isCrit = type == DamageType.Critical;
            float maxLifeTime = isCrit ? 1.0f : 0.6f;

            float xOff = Random.Range(-0.3f, 0.3f);
            Vector3 startPos = worldPos + new Vector3(xOff, 0f, 0f);
            float upDist = isCrit ? 1.5f : 1.0f;
            Vector3 endPos = startPos + new Vector3(0f, upDist, 0f);
            transform.position = startPos;
            _shakeOrigin = startPos;

            _text.text = FormatDamage(damage, type);
            _text.color = ColorFor(type);
            _text.fontSize = Mathf.RoundToInt(FontSizeFor(type) * sizeMul);
            _cg.alpha = 1f;

            float baseScale = (isCrit ? 0.025f : 0.05f) * sizeMul;
            transform.localScale = Vector3.one * baseScale;

            // 1) 위로 부동 — OutQuad ease
            _moveHandle = LMotion.Create(startPos, endPos, maxLifeTime)
                .WithEase(Ease.OutQuad)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() =>
                {
                    gameObject.SetActive(false);
                    DamageFloaterPool.Return(this);
                })
                .Bind(transform, (p, t) => t.position = new Vector3(p.x + (t.position.x - _shakeOrigin.x), p.y, p.z));

            // Position bind 의 shake offset 보존을 위해 _shakeOrigin 갱신
            _shakeOrigin = startPos;

            // 2) Alpha fade — delay 후 0→1
            float fadeDelay = isCrit ? 0.3f : 0.2f;
            float fadeDur = isCrit ? 0.5f : 0.4f;
            _alphaHandle = LMotion.Create(1f, 0f, fadeDur)
                .WithDelay(fadeDelay)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(_cg, (a, cg) => cg.alpha = a);

            // 3) Critical 진입 punch — scale base → peak 0.15s
            if (isCrit)
            {
                float peakScale = 0.07f * sizeMul;
                _scaleHandle = LMotion.Create(baseScale, peakScale, 0.15f)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .Bind(transform, (s, t) => t.localScale = Vector3.one * s);

                // 4) Critical shake — 0.15s 동안 X 축만
                _shakeHandle = LMotion.Shake.Create(0f, 0.05f, 0.15f)
                    .WithFrequency(20)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .Bind(transform, (offset, t) =>
                    {
                        Vector3 p = t.position;
                        p.x = _shakeOrigin.x + offset;
                        t.position = p;
                    });
            }
        }

        private void CancelAll()
        {
            if (_moveHandle.IsActive()) _moveHandle.Cancel();
            if (_alphaHandle.IsActive()) _alphaHandle.Cancel();
            if (_scaleHandle.IsActive()) _scaleHandle.Cancel();
            if (_shakeHandle.IsActive()) _shakeHandle.Cancel();
        }

        private void OnDisable() => CancelAll();

        // 5017 T01 — FormatDamage cache (Dictionary + StringBuilder).
        // 동일 damage+type 반복 시 cache hit → string alloc 0. cap 1024 메모리 ~30 KB 영구 (D1=A).
        private const int FORMAT_CACHE_CAP = 1024;
        private static readonly StringBuilder _formatBuffer = new StringBuilder(16);
        private static readonly Dictionary<(int, DamageType), string> _formatCache = new Dictionary<(int, DamageType), string>(FORMAT_CACHE_CAP);

        public static int FormatCacheCount => _formatCache.Count;  // NUnit 검증

        private static string FormatDamage(int damage, DamageType type)
        {
            var key = (damage, type);
            if (_formatCache.TryGetValue(key, out string cached)) return cached;

            var sb = _formatBuffer;
            sb.Length = 0;
            if (type == DamageType.Heal) sb.Append('+');
            int abs = Mathf.Abs(damage);
            if (abs >= 1_000_000) sb.Append((damage / 1_000_000f).ToString("F1")).Append('M');
            else if (abs >= 10_000) sb.Append(damage / 1000).Append('K');
            else if (abs >= 1_000) sb.Append((damage / 1000f).ToString("F1")).Append('K');
            else sb.Append(damage);
            if (type == DamageType.Critical) sb.Append('!');
            string result = sb.ToString();
            if (_formatCache.Count < FORMAT_CACHE_CAP) _formatCache[key] = result;
            return result;
        }

        private static Color ColorFor(DamageType type)
        {
            switch (type)
            {
                case DamageType.Critical: return new Color(1f, 0.84f, 0f);
                case DamageType.DOT:      return new Color(0.65f, 0.42f, 0.79f);
                case DamageType.Heal:     return new Color(0.29f, 0.87f, 0.5f);
                case DamageType.Block:    return new Color(0.53f, 0.53f, 0.53f);
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
    }
}
