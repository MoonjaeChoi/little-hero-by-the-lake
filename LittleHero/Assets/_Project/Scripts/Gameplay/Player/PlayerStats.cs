using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// stats_v2_infinite.md — 3 코어 (힘·민첩·지혜) + 무한 레벨 + 24 파생 스탯.
    /// Spirit (마음) 폐기, 기능 흡수: HP→힘, XP→지혜.
    /// 보스/일반 분기 제거 — 단일 데미지 공식.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        private BalanceConfig _config;
        private CharacterData _character;
        private StatModifiers _equipmentBonus = new StatModifiers();

        // ===== 기존 파생 (보존) =====
        public float MaxHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float AttackDamage { get; private set; }
        public float AttackSpeed { get; private set; }
        public float Range { get; private set; }
        public float XpGainMultiplier { get; private set; }

        // ===== 3 코어 (Spirit 폐기) =====
        public float StrengthRaw { get; private set; }
        public float AgilityRaw { get; private set; }
        public float WisdomRaw { get; private set; }
        public int Strength => Mathf.RoundToInt(StrengthRaw);
        public int Agility  => Mathf.RoundToInt(AgilityRaw);
        public int Wisdom   => Mathf.RoundToInt(WisdomRaw);

        // ===== 데미지 채널 (3) — 보스/일반 분기 제거 =====
        public float FinalDamageBonus { get; private set; }      // 곱연산 (망토·하의·안경)
        public float AdditiveDamageBonus { get; private set; }   // 합연산 (옵션 라인)

        // ===== 크리티컬 (4) =====
        public float CritChance { get; private set; }
        public float CritDamage { get; private set; }
        public float MagicCritChance { get; private set; }       // v2 신규
        public float MagicCritDamage { get; private set; }       // v2 신규

        // ===== 방어 (5) =====
        public float Armor { get; private set; }
        public float DamageReduction { get; private set; }
        public float EvasionRate { get; private set; }           // v2 신규 (민첩 비례)
        public float StatusResist { get; private set; }          // v2 신규
        public float ArmorIgnore { get; private set; }           // v2 신규 (메이플 방무)

        // ===== 공격 (4) =====
        public int ExtraAttackTargets { get; private set; }
        public int ChainTargets { get; private set; }            // v2 신규
        public float ArmorPenetration { get; private set; }      // 기존 (별개 채널)

        // ===== 회복 (3) =====
        public float HpRegenPerSec { get; private set; }         // v2 신규
        public float LifestealRatio { get; private set; }        // v2 신규 (cap 0.5)

        // ===== 메타 (5) =====
        public float DustGainMultiplier { get; private set; }
        public float DropRateMultiplier { get; private set; }
        public float CompanionDurationBonus { get; private set; }
        public float CompanionDamage { get; private set; }       // v2 신규

        // ===== 마법 (2) =====
        public float MagicDamage { get; private set; }           // v2 신규 — 지팡이·룬 무기
        public float MagicDamageMul { get; private set; }        // v2 신규

        // ===== 5034 — 직업별 특수 스탯 (D4=B 메이플 정합, D8=B Add/Remove 메서드) =====
        private float _bossDamageBonus;   // 전사: 보스 데미지 +%
        private float _critChanceBonus;   // 궁수: 크리 확률 +%
        private float _aoeBonus;          // 마법사: 광역 데미지 +%
        public float BossDamageBonus => _bossDamageBonus;
        public float CritChanceBonus => _critChanceBonus;
        public float AoeBonus => _aoeBonus;
        public void AddBossDamageBonus(float v) => _bossDamageBonus += v;
        public void RemoveBossDamageBonus(float v) => _bossDamageBonus -= v;
        public void AddCritChanceBonus(float v) => _critChanceBonus += v;
        public void RemoveCritChanceBonus(float v) => _critChanceBonus -= v;
        public void AddAoeBonus(float v) => _aoeBonus += v;
        public void RemoveAoeBonus(float v) => _aoeBonus -= v;

        // stats_v2.5 — L100 cap 복원 (사용자 결정 2026-05-03). 장비 강화는 무한.
        public const int LEVEL_CAP = 100;
        public int Level { get; private set; } = 1;

        public void Initialize(BalanceConfig config, CharacterData character = null)
        {
            _config = config;
            _character = character;

            // 코어 시작값 (Lv1 = 0, SetLevel 호출 시 누적)
            StrengthRaw = 0f;
            AgilityRaw  = 0f;
            WisdomRaw   = 0f;
            Level = 1;

            // 베이스
            CritDamage = 1.5f;
            DustGainMultiplier = 1f;
            DropRateMultiplier = 1f;
            LifestealRatio = 0f;

            RecalculateStats();
        }

        public void ApplyEquipmentBonus(StatModifiers bonus)
        {
            _equipmentBonus.maxHealthBonus    += bonus.maxHealthBonus;
            _equipmentBonus.speedBonus        += bonus.speedBonus;
            _equipmentBonus.attackDamageBonus += bonus.attackDamageBonus;
            _equipmentBonus.attackSpeedBonus  += bonus.attackSpeedBonus;
            _equipmentBonus.rangeBonus        += bonus.rangeBonus;
            _equipmentBonus.xpGainBonus       += bonus.xpGainBonus;
            RecalculateStats();
        }

        public void RemoveEquipmentBonus(StatModifiers bonus)
        {
            _equipmentBonus.maxHealthBonus    -= bonus.maxHealthBonus;
            _equipmentBonus.speedBonus        -= bonus.speedBonus;
            _equipmentBonus.attackDamageBonus -= bonus.attackDamageBonus;
            _equipmentBonus.attackSpeedBonus  -= bonus.attackSpeedBonus;
            _equipmentBonus.rangeBonus        -= bonus.rangeBonus;
            _equipmentBonus.xpGainBonus       -= bonus.xpGainBonus;
            RecalculateStats();
        }

        // stats_v2.5 — Level [1, 100] cap. 장비 강화는 별도 무한.
        public void SetLevel(int level)
        {
            Level = Mathf.Clamp(level, 1, LEVEL_CAP);

            float sm = _character != null ? _character.strengthMultiplier : 1f;
            float am = _character != null ? _character.agilityMultiplier  : 1f;
            float wm = _character != null ? _character.wisdomMultiplier   : 1f;
            StrengthRaw = Level * sm;
            AgilityRaw  = Level * am;
            WisdomRaw   = Level * wm;

            RecalculateStats();
        }

        public void ApplyLevelUp() => SetLevel(Level + 1);

        // weapon_v2 시너지 setter (WeaponClassBonus 가 호출)
        public void AddAdditiveDamageBonus(float percent) => AdditiveDamageBonus += percent;
        public void MultiplyAttackSpeed(float mul) => AttackSpeed *= mul;
        public void AddMagicDamageMul(float ratio) => MagicDamageMul += ratio;

        /// <summary>
        /// EffectResolver 결과를 PlayerStats 채널에 합산 (Sprint 3 통합).
        /// affix MUL% / FinalDamage% / CritChance% / ArmorIgnore% 등 자동 반영.
        /// </summary>
        public void ApplyResolvedBonus(ResolvedEquipmentBonus bonus)
        {
            if (bonus == null) return;
            // 코어 가산 (AllStats / Strength / Agility / Wisdom affix)
            StrengthRaw += bonus.StrengthAdd;
            AgilityRaw  += bonus.AgilityAdd;
            WisdomRaw   += bonus.WisdomAdd;
            // 전투 채널 — RecalculateStats 가 코어 누적 후 호출됨, 여기서 추가 가산
            // (실제 호출 흐름: SetLevel → RecalculateStats → ApplyResolvedBonus → 채널 갱신)
            AdditiveDamageBonus    += bonus.AttackDamageMulPercent;
            FinalDamageBonus       += bonus.FinalDamageBonusPercent;
            CritChance              = Mathf.Clamp01(CritChance + bonus.CritChancePercent / 100f);
            CritDamage             += bonus.CritDamageAddPercent / 100f;
            ExtraAttackTargets     += bonus.ExtraAttackTargets;
            ArmorPenetration        = Mathf.Clamp01(ArmorPenetration + bonus.ArmorPenetrationPercent / 100f);
            ArmorIgnore             = Mathf.Clamp01(ArmorIgnore + bonus.ArmorPenetrationPercent / 100f);
            Armor                  += bonus.ArmorAdd;
            DamageReduction         = Mathf.Clamp01(DamageReduction + bonus.DamageReductionPercent / 100f);
            HpRegenPerSec          += bonus.HpRegenAdd;
            // MaxHealthMulPercent 적용
            MaxHealth              *= 1f + bonus.MaxHealthMulPercent / 100f;
            MaxHealth              += bonus.MaxHealthAdd;
            AttackSpeed            *= 1f + bonus.AttackSpeedMulPercent / 100f;
            MoveSpeed              += bonus.MoveSpeedAdd;
            Range                  += bonus.RangeAdd;
            // 메타 채널
            XpGainMultiplier       += bonus.XpGainPercent / 100f;
            DustGainMultiplier     += bonus.DustGainPercent / 100f;
            DropRateMultiplier      = 1f + bonus.DropRatePercent / 100f;
            CompanionDurationBonus += bonus.CompanionDurationBonusPercent / 100f;
        }

        private void RecalculateStats()
        {
            if (_config == null) return;

            float baseHp  = _character != null ? _character.maxHealth     : _config.baseMaxHealth;
            float baseSpd = _character != null ? _character.moveSpeed     : _config.baseMoveSpeed;
            float baseDmg = _character != null ? _character.attackDamage  : _config.baseAttackDamage;
            float baseAs  = _character != null ? _character.attackSpeed   : _config.baseAttackSpeed;
            float baseR   = _character != null ? _character.range         : _config.baseRange;

            float metaHp  = MetaProgression.Instance != null ? MetaProgression.Instance.GetMetaBonus(MetaUpgradeStat.MaxHealth)    : 0f;
            float metaDmg = MetaProgression.Instance != null ? MetaProgression.Instance.GetMetaBonus(MetaUpgradeStat.AttackDamage) : 0f;
            float metaDust= MetaProgression.Instance != null ? MetaProgression.Instance.GetMetaBonus(MetaUpgradeStat.StarDustGain) : 0f;

            // stats_v2.5 — L100 cap chunk 곡선 단순화
            int lvOff = Mathf.Clamp(Level - 1, 0, LEVEL_CAP - 1);
            int chunk1 = Mathf.Min(lvOff, 49);              // L1~50  +10HP +1ATK
            int chunk2 = Mathf.Max(0, lvOff - 49);          // L51~100 +5  +0.5

            float lvHp  = chunk1 * 10f + chunk2 * 5f;
            float lvDmg = chunk1 * 1f  + chunk2 * 0.5f;
            float lvSpd = (chunk1 / 5) * 0.05f;
            float lvCrit= (chunk1 / 5) * 0.01f;

            // 코어 100점당 효과 (stats_v2 §2)
            // 힘: 공격력 +0.1 / 방어 +0.1 / HP +0.5 (마음 흡수)
            float coreHp     = StrengthRaw * 0.5f;
            float coreDmg    = StrengthRaw * 0.1f;
            float coreArmor  = StrengthRaw * 0.1f;
            // 민첩: 공속 +0.0005 / 이속 +0.005 / 회피 +0.0002 (신규)
            float coreAS     = AgilityRaw * 0.0005f;
            float coreSpd    = AgilityRaw * 0.005f;
            float coreEvasion= AgilityRaw * 0.0002f;
            // 지혜: 크확 +0.0002 / 사거리 +0.005 / XP +0.0005 (마음 흡수) / 마법 데미지 +0.1
            float coreCrit   = WisdomRaw * 0.0002f;
            float coreRange  = WisdomRaw * 0.005f;
            float coreXp     = WisdomRaw * 0.0005f;
            float coreMagicDmg = WisdomRaw * 0.1f;

            // 파생 스탯 합산
            MaxHealth         = baseHp  + lvHp  + metaHp + coreHp  + _equipmentBonus.maxHealthBonus;
            MoveSpeed         = baseSpd + lvSpd + coreSpd + _equipmentBonus.speedBonus;
            AttackDamage      = baseDmg + lvDmg + metaDmg + coreDmg + _equipmentBonus.attackDamageBonus;
            // 사용자 baseline (2026-05-05) — AttackSpeed × 5 영구 적용. 정상 IEnumerator+Lerp + cap 300 환경에서 적 처치 X 문제 해결.
            AttackSpeed       = (baseAs  + coreAS + _equipmentBonus.attackSpeedBonus) * 5f;
            Range             = baseR   + coreRange + _equipmentBonus.rangeBonus;
            XpGainMultiplier  = 1f + coreXp + _equipmentBonus.xpGainBonus;

            // PIVOT v4 — 베이스 5% 보장
            CritChance        = Mathf.Max(0.05f, lvCrit + coreCrit);
            CritDamage        = 1.5f;  // 기본 (affix → ApplyResolvedBonus 에서 가산)
            Armor             = coreArmor;
            EvasionRate       = coreEvasion;

            // 신규 v2 — 코어 베이스만 (affix → ApplyResolvedBonus 에서 가산)
            DamageReduction   = 0f;
            StatusResist      = 0f;
            ArmorIgnore       = 0f;
            ArmorPenetration  = 0f;
            HpRegenPerSec     = 0f;
            LifestealRatio    = 0f;
            ExtraAttackTargets = 0;
            ChainTargets      = 0;
            FinalDamageBonus  = 0f;
            AdditiveDamageBonus = 0f;
            CompanionDurationBonus = 0f;
            CompanionDamage   = 0f;
            MagicCritChance   = 0f;
            MagicCritDamage   = 0f;
            MagicDamage       = coreMagicDmg;  // 지혜 비례
            MagicDamageMul    = 0f;

            DustGainMultiplier = 1f + metaDust;
            DropRateMultiplier = 1f;
        }
    }
}
