using UnityEngine;

namespace LittleHero
{
    public enum MemoryType { Photo, Memo, Sticker, Doodle }

    [CreateAssetMenu(menuName = "LittleHero/Memory")]
    public class MemoryData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite image;
        public MemoryType memoryType;

        [Header("발견 조건")]
        public string chapterId;
        [Range(0f, 1f)] public float spawnChance = 0.1f;
    }
}
