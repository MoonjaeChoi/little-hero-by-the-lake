using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v3 — 챕터 배경 player 따라 이동 (무한 반복 효과).
    /// Tiled drawMode + 큰 size (80×80) + player 위치 따라가기 → 카메라 어디 가도 ground 보임.
    /// </summary>
    public class BackgroundFollow : MonoBehaviour
    {
        private Transform _player;

        private void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void LateUpdate()
        {
            if (_player == null) return;
            // player 따라 이동, z 는 그대로
            transform.position = new Vector3(_player.position.x, _player.position.y, transform.position.z);
        }
    }
}
