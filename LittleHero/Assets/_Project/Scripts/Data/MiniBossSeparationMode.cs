namespace LittleHero
{
    /// <summary>
    /// 5003 — 미니보스 씬 분리 모드 (D3 Rebuttal 자동 수용 — §15.4 SO 우선 + §15.1 SRP).
    /// BalanceConfig.miniBossSeparation SerializeField 로 토글.
    /// </summary>
    public enum MiniBossSeparationMode
    {
        None,       // 분리 X (5002 종료 상태)
        LastOnly,   // wave 15 만 분리 (옵션 B — 보스 빌드업 효과)
        All,        // 5/10/15 모두 분리 (옵션 A — 일관 cinematic)
    }
}
