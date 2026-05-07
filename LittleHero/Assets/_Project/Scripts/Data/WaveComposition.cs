using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 T05 — Wave 정의 (SO 재사용 ↑).
    /// 1 WaveComposition = 여러 EnemyComposition 묶음 + WaveType (Standard/Rush/Elite/Boss/SplitterRush).
    /// 다른 챕터/모드 (Endgame / 무한 다락방) 에서 reuse 가능.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/WaveComposition")]
    public class WaveComposition : ScriptableObject
    {
        [Tooltip("Wave 종류 — Standard=일반 / Rush=러시 / Elite=미니보스 / Boss=메인 / SplitterRush=분열적 무리")]
        public WaveType waveType = WaveType.Standard;

        [Tooltip("이 wave 에 등장하는 적 그룹들 (1+개, 동시 진행)")]
        public EnemyComposition[] compositions;

        [Tooltip("Wave 진행 시간 (초). Boss/Elite 는 처치 시 종료")]
        public float waveDuration = 30f;

        [Tooltip("Wave 시작 직전 휴식 시간 (5초 권장)")]
        public float restBeforeWave = 5f;
    }

    public enum WaveType
    {
        Standard      = 0,  // 일반 (1~4, 6~9, 11~14, 16~19)
        Rush          = 1,  // 러시 (대량 동시 spawn — 후반)
        Elite         = 2,  // 미니보스 (5/10/15)
        Boss          = 3,  // 메인 (20)
        SplitterRush  = 4,  // 곰형 적 무리 (탕탕 답습)
    }
}
