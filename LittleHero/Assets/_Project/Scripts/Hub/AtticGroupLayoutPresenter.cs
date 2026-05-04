using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5006 Sprint 1 — 다락방 진입 버튼 4 그룹 RectTransform 컨테이너 + 44pt 터치 타겟 검증.
    /// AtticController 잔여 책임과 분리 (§15.1 SRP). Inspector 만 사용 (코드 hardcode X).
    /// 한 손 플레이 ✅ — 좌상(G1 전투) / 좌하(G2 장비) / 우상(G3 상점) / 우하(G4 기록) anchor.
    /// D4=A: 4 RectTransform 자식 이름은 Panel/Modal/Overlay 접미어 X → AtticPanelToggleManager 자동 영향 0.
    /// </summary>
    public class AtticGroupLayoutPresenter : MonoBehaviour
    {
        [Header("4 그룹 RectTransform 컨테이너 (D4=A — Group{N}{Cat} 이름)")]
        [SerializeField] private RectTransform _group1Combat;     // 좌상 — 전투 (챕터/Endgame/지도)
        [SerializeField] private RectTransform _group2Equipment;  // 좌하 — 장비 (정보/인벤토리/강화/단련)
        [SerializeField] private RectTransform _group3Shop;       // 우상 — 상점 (가챠/마법의돌/한정/별빛/모루)
        [SerializeField] private RectTransform _group4Record;     // 우하 — 기록 (일기/도감)

        [Header("44pt 터치 타겟 검증 (CLAUDE.md §2)")]
        [SerializeField] private float _minTouchSize = 44f;

        private void OnValidate()
        {
            ValidateTouchSize(_group1Combat);
            ValidateTouchSize(_group2Equipment);
            ValidateTouchSize(_group3Shop);
            ValidateTouchSize(_group4Record);
        }

        private void ValidateTouchSize(RectTransform group)
        {
            if (group == null) return;
            foreach (Transform child in group)
            {
                var rect = child.GetComponent<RectTransform>();
                if (rect == null) continue;
                if (rect.sizeDelta.x < _minTouchSize || rect.sizeDelta.y < _minTouchSize)
                    Debug.LogWarning($"[AtticGroupLayout] {child.name} 터치 타겟 {rect.sizeDelta} < {_minTouchSize}pt (CLAUDE.md §2)");
            }
        }
    }
}
