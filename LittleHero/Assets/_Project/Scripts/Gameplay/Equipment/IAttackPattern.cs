using UnityEngine;

namespace LittleHero
{
    public interface IAttackPattern
    {
        void Initialize(AttackPatternConfig config, Transform owner);
        void Tick(float deltaTime);
        void Cleanup();
    }
}
