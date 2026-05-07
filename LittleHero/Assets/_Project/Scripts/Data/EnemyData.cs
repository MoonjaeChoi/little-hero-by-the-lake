using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "LittleHero/Enemy")]
    public class EnemyData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public GameObject prefab;

        [Header("스탯")]
        public float maxHealth;
        public float moveSpeed;
        public float damage;
        public float xpDrop;
        public float starDustDrop;

        [Tooltip("방어력 — 0=무방어. DamageCalculator 가 dmg/(1 + armor×(1-관통)/100). 일몹 0, 보스만 ~30~80 권장 (stats_v1 §5)")]
        public float armor = 0f;

        [Header("외형")]
        public Color tintColor = Color.white;
        public float scaleMultiplier = 1f;

        [Header("분류")]
        public bool isMiniB;
        public bool isBoss;

        // PIVOT v3 보스 공격 패턴 (boss_attack_v1.md §2.2). CH6+ chapter boss 만 사용.
        [Header("보스 공격 패턴 (CH6+ chapter boss)")]
        public BossAttackType bossAttackType = BossAttackType.None;
        public float bossAttackCooldown = 4f;
        public float bossAttackDamage = 20f;
        public float bossAttackRange = 5f;

        // 5001 boss_system_v2 — Tier·Phase·Loot 분리. 기존 isMiniB/isBoss boolean 호환 유지.
        [Header("Boss System v2 (5001)")]
        [Tooltip("보스 분류 — None=일반 적, MiniBoss=5/10/15 wave, ChapterBoss=20 wave, Daily/Weekly=Endgame")]
        public BossTier bossTier = BossTier.None;

        [Tooltip("Phase 2 패턴 (ChapterBoss+ 만). MiniBoss = None (D3=A PRD 그대로)")]
        public BossAttackType phase2Pattern = BossAttackType.None;

        [Range(0f, 1f), Tooltip("Phase 2 진입 HP 임계 (기본 50%)")]
        public float phase2Threshold = 0.5f;

        [Tooltip("Phase 2 공격 속도 배율 (D2=C SerializeField, 기본 1.3×). 보스별 차별 가능")]
        public float phase2SpeedMultiplier = 1.3f;

        [Tooltip("보스 처치 보상 테이블 — Tier 별 4 인스턴스 중 하나 wiring (T06)")]
        public BossLootTable lootTable;

        [Header("5029 Element 저항 (선택)")]
        [Tooltip("element 저항 곱셈 [Fire/Ice/Lightning/Arcane]. 1.0=중립 / 1.5=약점 / 0.7=저항. 길이 != 4 시 default 사용")]
        public float[] elementResistance = { 1f, 1f, 1f, 1f };

        // 5035 enemy_system_v2 — Strategy 패턴 (D1=A 5종 + Follow 기본)
        [Header("5035 — Behavior + Death Reaction (Strategy 패턴)")]
        [Tooltip("적 이동/공격 패턴 — Follow=기본 추적 / Stationary=위치 고정 / Charger=돌진 / Splitter=분열용 / Ranged=원거리 / Bomber=자폭")]
        public EnemyBehaviorType behaviorType = EnemyBehaviorType.Follow;

        [Tooltip("사망 시 반응 — None=기본 / Splitter=분열 (곰형) / Explode=폭발 (Bomber)")]
        public EnemyDeathReactionType deathReactionType = EnemyDeathReactionType.None;

        [Tooltip("Splitter 사망 시 소환할 작은 적 (D3=B 3마리)")]
        public EnemyData splitterChildData;
        [Tooltip("Splitter 분열 수 (D3=B 3 권장)")]
        public int splitterChildCount = 3;

        [Tooltip("Bomber 폭발 반경 (D4=B 1.5u 권장)")]
        public float bomberRadius = 1.5f;
        [Tooltip("Bomber 폭발 데미지 (별도, damage 보다 낮을 수도)")]
        public float bomberDamage = 30f;
        [Tooltip("Bomber 텔레그래프 시간 (회피 가능 영역, 1초 권장)")]
        public float bomberTelegraphSeconds = 1f;

        [Tooltip("Charger 돌진 주기 (3초 권장)")]
        public float chargerInterval = 3f;
        [Tooltip("Charger 돌진 시 속도 배수 (3× 권장)")]
        public float chargerSpeedMultiplier = 3f;

        [Tooltip("Stationary/Ranged 발사체 주기 (1.5초 권장)")]
        public float rangedFireInterval = 1.5f;
        [Tooltip("Ranged 유지 거리 (4u 권장)")]
        public float rangedKeepDistance = 4f;

        [Tooltip("Stationary/Ranged 발사체 prefab (Projectile 컴포넌트 보유)")]
        public GameObject projectilePrefab;
    }

    /// <summary>5035 — 적 behavior 종류.</summary>
    public enum EnemyBehaviorType
    {
        Follow     = 0,  // 기본 추적 (기존 EnemyAI 답습)
        Stationary = 1,  // 위치 고정 + 발사체
        Charger    = 2,  // 주기적 돌진
        Splitter   = 3,  // 추적 + 사망 분열
        Ranged     = 4,  // 거리 유지 + 발사체
        Bomber     = 5,  // 자폭 (텔레그래프)
    }

    /// <summary>5035 — 사망 반응 종류.</summary>
    public enum EnemyDeathReactionType
    {
        None     = 0,  // 기본
        Splitter = 1,  // 작은 적 N마리 분열
        Explode  = 2,  // 폭발 (Bomber)
    }
}
