using UnityEngine;

namespace LittleHero
{
    public class PlayerStats : MonoBehaviour
    {
        private BalanceConfig _config;
        private CharacterData _character;
        private StatModifiers _equipmentBonus = new StatModifiers();

        // 기존 파생 스탯 (유지)
        public float MaxHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float AttackDamage { get; private set; }
        public float AttackSpeed { get; private set; }
        public float Range { get; private set; }
        public float XpGainMultiplier { get; private set; }

        // ✦ A1: 4 코어 스탯 (stats_v1 §1) — 레벨업 시 +1 × character multiplier 누적
        public float StrengthRaw { get; private set; }   // 누적 소수점 (반올림 전)
        public float AgilityRaw { get; private set; }
        public float WisdomRaw { get; private set; }
        public float SpiritRaw { get; private set; }
        public int Strength => Mathf.RoundToInt(StrengthRaw);
        public int Agility  => Mathf.RoundToInt(AgilityRaw);
        public int Wisdom   => Mathf.RoundToInt(WisdomRaw);
        public int Spirit   => Mathf.RoundToInt(SpiritRaw);

        // ✦ A1: 신규 파생 스탯 (stats_v1 §6)
        public float CritChance { get; private set; }            // 0.0 ~ 1.0
        public float CritDamage { get; private set; }            // 1.5 = 베이스
        public float Armor { get; private set; }
        public float DamageReduction { get; private set; }       // 0.0 ~ 1.0
        public float DustGainMultiplier { get; private set; }    // 1.0 = 베이스
        public float DropRateMultiplier { get; private set; }    // 1.0 = 베이스

        // ✦ A1: 메이플 키우기 채널 (stats_v1 §1, 스킬 시스템 제외)
        public float FinalDamageBonus { get; private set; }      // 망토·하의·안경 (버킷 2)
        public int   ExtraAttackTargets { get; private set; }    // 상의 (Projectile.count 가산)
        public float ArmorPenetration { get; private set; }      // 0.0 ~ 1.0, 무기
        public float CompanionDurationBonus { get; private set; }// 신발·펫

        // ✦ A1: 데미지 버킷 1 (합산 데미지%) — 옵션 라인·신화·메타
        public float AdditiveDamageBonus { get; private set; }

        // 현재 플레이어 레벨 (A2 PlayerProgress가 SetLevel 호출, A1 단계에선 1로 고정)
        public int Level { get; private set; } = 1;

        public void Initialize(BalanceConfig config, CharacterData character = null)
        {
            _config = config;
            _character = character;

            // 코어 스탯 시작값 (Lv1 = 0, 레벨업마다 +1 × multiplier 누적)
            StrengthRaw = 0f;
            AgilityRaw  = 0f;
            WisdomRaw   = 0f;
            SpiritRaw   = 0f;
            Level = 1;

            // 크리티컬 베이스
            CritDamage = 1.5f;

            // 배수 베이스 1.0
            DustGainMultiplier = 1f;
            DropRateMultiplier = 1f;

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

        // A2 PlayerProgress 가 OnLevelUp 시 호출. character_progression §7 의 L100 piecewise 곡선과
        // 결합되며, 본 메서드는 1 레벨씩 적용한다(여러 레벨업 시 호출 N회).
        public void ApplyLevelUp()
        {
            Level++;

            // 코어 스탯 +1 × character multiplier (stats_v1 §3.3)
            float sm = _character != null ? _character.strengthMultiplier : 1f;
            float am = _character != null ? _character.agilityMultiplier  : 1f;
            float wm = _character != null ? _character.wisdomMultiplier   : 1f;
            float pm = _character != null ? _character.spiritMultiplier   : 1f;
            StrengthRaw += sm;
            AgilityRaw  += am;
            WisdomRaw   += wm;
            SpiritRaw   += pm;

            RecalculateStats();
        }

        private void RecalculateStats()
        {
            // Initialize() 이전에 장비가 장착되면 config가 없으므로 대기
            if (_config == null) return;

            // 캐릭터 선택 시 그 스탯을 base로, 미선택 시 BalanceConfig 기본값
            float baseHp  = _character != null ? _character.maxHealth     : _config.baseMaxHealth;
            float baseSpd = _character != null ? _character.moveSpeed     : _config.baseMoveSpeed;
            float baseDmg = _character != null ? _character.attackDamage  : _config.baseAttackDamage;
            float baseAs  = _character != null ? _character.attackSpeed   : _config.baseAttackSpeed;
            float baseR   = _character != null ? _character.range         : _config.baseRange;

            float metaHp  = MetaProgression.Instance != null ? MetaProgression.Instance.GetMetaBonus(MetaUpgradeStat.MaxHealth) : 0f;
            float metaDmg = MetaProgression.Instance != null ? MetaProgression.Instance.GetMetaBonus(MetaUpgradeStat.AttackDamage) : 0f;
            float metaDust= MetaProgression.Instance != null ? MetaProgression.Instance.GetMetaBonus(MetaUpgradeStat.StarDustGain) : 0f;

            // 레벨업 자동 증가 (stats_v1 §3.1) — 캐릭터 레벨에 비례
            int lvOffset = Mathf.Max(0, Level - 1);
            float lvHp  = lvOffset * 10f;          // +10 / Lv
            float lvDmg = lvOffset * 1f;           // +1 / Lv
            float lvSpd = (lvOffset / 5) * 0.05f;  // +0.05 / 5Lv
            float lvCrit= (lvOffset / 5) * 0.01f;  // +1% / 5Lv

            // 코어 스탯 100점당 효과 (stats_v1 §3.2)
            float coreHp   = SpiritRaw   * 0.5f;     // 마음 100 → +50 HP
            float coreDmg  = StrengthRaw * 0.1f;     // 힘 100 → +10 공격력
            float coreArmor= StrengthRaw * 0.1f;     // 힘 100 → +10 방어력
            float coreAS   = AgilityRaw  * 0.0005f;  // 빠름 100 → +5% (= 0.05) 공속
            float coreSpd  = AgilityRaw  * 0.005f;   // 빠름 100 → +0.5 이속
            float coreCrit = WisdomRaw   * 0.0002f;  // 슬기 100 → +2% 크리확
            float coreRange= WisdomRaw   * 0.005f;   // 슬기 100 → +0.5 사거리
            float coreXp   = SpiritRaw   * 0.0005f;  // 마음 100 → +5% xp

            MaxHealth        = baseHp  + lvHp  + metaHp  + coreHp   + _equipmentBonus.maxHealthBonus;
            MoveSpeed        = baseSpd + lvSpd + coreSpd + _equipmentBonus.speedBonus;
            AttackDamage     = baseDmg + lvDmg + metaDmg + coreDmg  + _equipmentBonus.attackDamageBonus;
            AttackSpeed      = baseAs  + coreAS + _equipmentBonus.attackSpeedBonus;
            Range            = baseR   + coreRange + _equipmentBonus.rangeBonus;
            XpGainMultiplier = 1f + coreXp + _equipmentBonus.xpGainBonus;

            // 신규 파생 — 코어에서 받는 부분만 계산. affix·옵션·신화 영향분은
            // 별도 ApplyAffix… 채널이 들어오기 전까지 0/베이스 유지 (B2~C4).
            CritChance         = lvCrit + coreCrit;
            // CritDamage/Armor/DamageReduction/FinalDamageBonus/ExtraAttackTargets/
            // ArmorPenetration/CompanionDurationBonus/AdditiveDamageBonus 는 affix 도입 시 합산.
            Armor              = coreArmor;

            DustGainMultiplier = 1f + metaDust;
        }
    }
}
