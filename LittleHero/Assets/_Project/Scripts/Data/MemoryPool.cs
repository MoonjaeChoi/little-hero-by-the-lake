using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "LittleHero/MemoryPool")]
    public class MemoryPool : ScriptableObject
    {
        public MemoryData[] memories;
    }
}
