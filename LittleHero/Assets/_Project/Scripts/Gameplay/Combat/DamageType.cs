namespace LittleHero
{
    /// <summary>데미지 수치 표시 종류 (cinematic_combat_v1.md §4.1).</summary>
    public enum DamageType
    {
        Normal,    // 흰, 24pt
        Critical,  // 골드, 36pt + 진동
        DOT,       // 보라, 20pt
        Heal,      // 녹색, 24pt + "+" prefix
        Block,     // 회색, 20pt "0"
    }
}
