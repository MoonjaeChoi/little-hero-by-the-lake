using UnityEngine;

namespace LittleHero
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class MemoryPickup : MonoBehaviour
    {
        private MemoryData _data;
        private bool _consumed;

        public void Initialize(MemoryData data)
        {
            _data = data;
            _consumed = false;
            GetComponent<Collider2D>().enabled = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed || _data == null) return;
            if (!other.CompareTag("Player")) return;

            _consumed = true;
            GetComponent<Collider2D>().enabled = false;
            GameEvents.RaiseMemoryDiscovered(_data);
            gameObject.SetActive(false);
        }
    }
}
