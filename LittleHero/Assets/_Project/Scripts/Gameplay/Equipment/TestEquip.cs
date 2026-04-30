using UnityEngine;

namespace LittleHero
{
    // 프로토타입 테스트용 — 게임 시작 시 지정 장비를 자동 장착
    public class TestEquip : MonoBehaviour
    {
        [SerializeField] private EquipmentData _startEquipment;

        private void Start()
        {
            if (_startEquipment == null) return;
            GetComponent<EquipmentManager>().Equip(_startEquipment);
        }
    }
}
