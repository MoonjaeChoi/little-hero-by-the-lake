using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public static class LMotionExtensions
    {
        public static MotionHandle DOFadeAlpha(this SpriteRenderer sr, float endAlpha, float duration)
        {
            var start = sr.color.a;
            return LMotion.Create(start, endAlpha, duration)
                .Bind(sr, (a, target) =>
                {
                    var c = target.color;
                    c.a = a;
                    target.color = c;
                });
        }

        public static MotionHandle DOFadeAlpha(this Image img, float endAlpha, float duration)
        {
            var start = img.color.a;
            return LMotion.Create(start, endAlpha, duration)
                .Bind(img, (a, target) =>
                {
                    var c = target.color;
                    c.a = a;
                    target.color = c;
                });
        }

        public static MotionHandle DOFade(this CanvasGroup cg, float endValue, float duration)
        {
            var start = cg.alpha;
            return LMotion.Create(start, endValue, duration)
                .Bind(cg, (a, target) => target.alpha = a);
        }

        public static MotionHandle DOPunchScale(this Transform tr, float strength, float duration)
        {
            var origin = tr.localScale;
            var peak = origin * (1f + strength);
            float halfDur = duration * 0.5f;
            return LSequence.Create()
                .Append(LMotion.Create(origin, peak, halfDur).BindToLocalScale(tr))
                .Append(LMotion.Create(peak, origin, halfDur).BindToLocalScale(tr))
                .Run();
        }

        public static MotionHandle DOMove(this Transform tr, Vector3 endValue, float duration)
        {
            var start = tr.position;
            return LMotion.Create(start, endValue, duration).BindToPosition(tr);
        }

        public static MotionHandle DOFillAmount(this Image img, float endValue, float duration)
        {
            var start = img.fillAmount;
            return LMotion.Create(start, endValue, duration)
                .Bind(img, (v, target) => target.fillAmount = v);
        }
    }
}
