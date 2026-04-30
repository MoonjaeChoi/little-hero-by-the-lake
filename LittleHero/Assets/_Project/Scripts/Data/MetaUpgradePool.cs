using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "LittleHero/MetaUpgradePool")]
    public class MetaUpgradePool : ScriptableObject
    {
        public MetaUpgradeData[] upgrades;
    }
}
