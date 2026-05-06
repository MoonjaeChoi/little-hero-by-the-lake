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
    }
}
