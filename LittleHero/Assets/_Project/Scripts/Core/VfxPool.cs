using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v3 Phase 2 — VFX 오브젝트 풀 (game_pivot_v2.md GC critical 해결).
    ///
    /// EnemyController.Die 매 처치 시 Instantiate(StarBurstFx) 폐기 →
    /// VfxPool.SpawnStarBurst(pos) 가 풀에서 재사용. 10k 처치 시 Instantiate 99% 감소.
    ///
    /// VFX 오브젝트는 SimpleFx (lifetime 자동 비활성) 가 0.6s 후 SetActive(false).
    /// VfxPool 이 다음 호출 시 비활성 오브젝트 재발견 + reuse.
    /// </summary>
    public static class VfxPool
    {
        private const string STAR_BURST_PATH = "VFX/StarBurstFx";
        private const int INITIAL_SIZE = 32;

        private static GameObject _starBurstPrefab;
        private static readonly List<GameObject> _starBurstPool = new List<GameObject>(INITIAL_SIZE);
        private static Transform _root;

        // Warning Circle (보스 telegraph 반경 표시)
        private static Sprite _warningSprite;
        private static readonly List<GameObject> _warningPool = new List<GameObject>(16);

        public static void SpawnWarning(Vector3 position, float radius, float duration)
        {
            EnsureRoot();
            EnsureWarningSprite();
            GameObject go = null;
            for (int i = 0; i < _warningPool.Count; i++)
            {
                if (_warningPool[i] != null && !_warningPool[i].activeSelf) { go = _warningPool[i]; break; }
            }
            if (go == null)
            {
                go = new GameObject("BossWarning");
                go.transform.SetParent(_root);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _warningSprite;
                sr.color = new Color(1f, 0.2f, 0.2f, 0.45f);
                sr.sortingOrder = 6;
                go.AddComponent<WarningAutoDestroy>();
                _warningPool.Add(go);
            }
            go.transform.position = position;
            float scale = radius * 2f;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            go.SetActive(true);
            var fx = go.GetComponent<WarningAutoDestroy>();
            fx.Init(duration);
        }

        private static void EnsureWarningSprite()
        {
            if (_warningSprite != null) return;
            // 단순 흰 원 sprite 생성 (런타임 — 32×32 텍스처)
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = d > radius ? 0f : (d > radius * 0.85f ? 1f : 0.3f);  // 외곽선 강조
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _warningSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        }

        public static void SpawnStarBurst(Vector3 position)
        {
            EnsurePrefab();
            EnsureRoot();
            var go = AcquireStarBurst();
            if (go == null) return;
            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
            go.SetActive(true);
        }

        /// <summary>PIVOT v4 — 피격 별 흩어짐. 5007 — overrideScale 지원 (Resolver tier × vfxScaleMul).</summary>
        public static void SpawnHitStar(Vector3 position, bool isCrit, float overrideScale = -1f)
        {
            EnsurePrefab();
            EnsureRoot();
            var go = AcquireStarBurst();
            if (go == null) return;
            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;
            float scale = overrideScale > 0f ? overrideScale : (isCrit ? 1.4f : 0.6f);
            go.transform.localScale = Vector3.one * scale;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = isCrit ? new Color(1f, 0.84f, 0f) : Color.white;
            go.SetActive(true);
        }

        private static GameObject AcquireStarBurst()
        {
            // 풀에서 비활성 오브젝트 검색 (SimpleFx 가 0.6s 후 SetActive(false))
            for (int i = 0; i < _starBurstPool.Count; i++)
            {
                if (_starBurstPool[i] != null && !_starBurstPool[i].activeSelf)
                    return _starBurstPool[i];
            }
            // 모두 활성 — 신규 Instantiate
            if (_starBurstPrefab == null) return null;
            var go = Object.Instantiate(_starBurstPrefab, _root);
            _starBurstPool.Add(go);
            return go;
        }

        private static void EnsurePrefab()
        {
            if (_starBurstPrefab != null) return;
            _starBurstPrefab = Resources.Load<GameObject>(STAR_BURST_PATH);
            if (_starBurstPrefab == null)
                Debug.LogWarning("[VfxPool] " + STAR_BURST_PATH + " 미존재 — VFX 풀링 스킵");
        }

        private static void EnsureRoot()
        {
            if (_root != null) return;
            var go = new GameObject("[VfxPool]");
            Object.DontDestroyOnLoad(go);
            _root = go.transform;
        }
    }

    /// <summary>Warning Circle 자동 비활성 (duration 후 SetActive(false)).</summary>
    public class WarningAutoDestroy : MonoBehaviour
    {
        private float _t;
        public void Init(float duration) { _t = duration; }
        private void Update()
        {
            _t -= Time.deltaTime;
            // alpha pulse fade
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) {
                float p = Mathf.Clamp01(_t);
                sr.color = new Color(1f, 0.2f, 0.2f, 0.4f + Mathf.Sin(Time.time * 8f) * 0.15f * p);
            }
            if (_t <= 0f) gameObject.SetActive(false);
        }
    }
}
