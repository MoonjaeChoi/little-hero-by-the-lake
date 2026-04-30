using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "LittleHero/EnemyPool")]
    public class EnemyPool : ScriptableObject
    {
        public EnemyData[] enemies;
    }
}
