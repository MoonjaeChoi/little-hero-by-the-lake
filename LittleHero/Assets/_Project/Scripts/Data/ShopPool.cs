using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "LittleHero/ShopPool")]
    public class ShopPool : ScriptableObject
    {
        public EquipmentData[] items;
    }
}
