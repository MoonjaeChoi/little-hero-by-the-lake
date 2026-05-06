namespace LittleHero
{
    /// <summary>
    /// 5029 — 마법사 element 시스템. None = 무속성 (전사/궁수 호환).
    /// 효과:
    ///   Fire: DOT 30% (10% × 3 tick) + 약점 보너스
    ///   Ice: MoveSpeed × 0.5 for 0.5s (둔화)
    ///   Lightning: 인접 적 1 추가 데미지 50% (사슬)
    ///   Arcane: 저항 무시 (1.0× 강제)
    /// </summary>
    public enum MagicElement
    {
        None,
        Fire,
        Ice,
        Lightning,
        Arcane,
    }
}
