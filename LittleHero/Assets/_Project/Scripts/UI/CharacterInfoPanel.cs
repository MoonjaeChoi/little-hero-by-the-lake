using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D8 — 캐릭터 정보 풀스크린 (ui_v1 §11.2).
    /// 4 코어(힘/빠름/슬기/마음) + 파생 스탯(MaxHealth/AttackDamage/AttackSpeed/MoveSpeed/Range/CritChance/CritDamage/Armor 등)
    /// + 레벨/XP 진행도 + 캐릭터 이름·아이콘.
    /// PlayerStats 로부터 직접 읽음 — 다락방에서 호출 시 PlayerStats 가 활성이어야 함(스폰 후 enable).
    /// </summary>
    public class CharacterInfoPanel : MonoBehaviour
    {
        [Header("기본 정보")]
        [SerializeField] private Text _characterNameText;
        [SerializeField] private Image _characterIcon;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _xpText;
        [SerializeField] private Slider _xpBar;

        [Header("4 코어 스탯")]
        [SerializeField] private Text _strengthText;
        [SerializeField] private Text _agilityText;
        [SerializeField] private Text _wisdomText;
        [SerializeField] private Text _spiritText;

        [Header("전투 파생")]
        [SerializeField] private Text _maxHealthText;
        [SerializeField] private Text _attackDamageText;
        [SerializeField] private Text _attackSpeedText;
        [SerializeField] private Text _moveSpeedText;
        [SerializeField] private Text _rangeText;

        [Header("크리티컬·방어")]
        [SerializeField] private Text _critChanceText;
        [SerializeField] private Text _critDamageText;
        [SerializeField] private Text _armorText;
        [SerializeField] private Text _damageReductionText;

        [Header("기타")]
        [SerializeField] private Text _xpGainText;
        [SerializeField] private Text _dustGainText;
        [SerializeField] private Text _dropRateText;

        [Header("닫기")]
        [SerializeField] private Button _closeButton;

        private PlayerStats _stats;

        private void OnEnable()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            ResolveStats();
            Refresh();
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
        }

        private void ResolveStats()
        {
            // 다락방 시점에 PlayerStats 가 없을 수 있음 — 시뮬값(캐릭터 SO + level)으로 폴백
            var player = GameObject.FindWithTag("Player");
            if (player != null) _stats = player.GetComponent<PlayerStats>();
        }

        private void Refresh()
        {
            var character = MetaProgression.Instance?.SelectedCharacter;
            if (_characterNameText != null)
                _characterNameText.text = character != null ? character.displayName : "-";
            if (_characterIcon != null && character != null)
            {
                // CharacterData 에 icon 필드가 있으면 사용. 없으면 무시.
            }

            int level = PlayerProgress.Instance?.Level ?? (MetaProgression.Instance?.Data?.level ?? 1);
            int xp = PlayerProgress.Instance?.Xp ?? (MetaProgression.Instance?.Data?.xp ?? 0);
            int nextXp = PlayerProgress.Instance?.NextLevelXp ?? PlayerProgress.CalcNextLevelXp(level);
            if (_levelText != null) _levelText.text = $"Lv {level}";
            if (_xpText != null)    _xpText.text = $"{xp:N0} / {nextXp:N0}";
            if (_xpBar != null)     _xpBar.value = nextXp > 0 ? Mathf.Clamp01((float)xp / nextXp) : 0f;

            // 4 코어 (PlayerStats 우선, 없으면 캐릭터 비중 × level 시뮬)
            int strength, agility, wisdom, spirit;
            if (_stats != null)
            {
                strength = _stats.Strength; agility = _stats.Agility; wisdom = _stats.Wisdom; spirit = _stats.Spirit;
            }
            else if (character != null)
            {
                strength = Mathf.RoundToInt(level * character.strengthMultiplier);
                agility  = Mathf.RoundToInt(level * character.agilityMultiplier);
                wisdom   = Mathf.RoundToInt(level * character.wisdomMultiplier);
                spirit   = Mathf.RoundToInt(level * character.spiritMultiplier);
            }
            else { strength = agility = wisdom = spirit = level; }

            if (_strengthText != null) _strengthText.text = $"힘 {strength}";
            if (_agilityText != null)  _agilityText.text  = $"빠름 {agility}";
            if (_wisdomText != null)   _wisdomText.text   = $"슬기 {wisdom}";
            if (_spiritText != null)   _spiritText.text   = $"마음 {spirit}";

            if (_stats != null)
            {
                SetText(_maxHealthText,    $"체력 {Mathf.RoundToInt(_stats.MaxHealth)}");
                SetText(_attackDamageText, $"공격력 {_stats.AttackDamage:F1}");
                SetText(_attackSpeedText,  $"공격 속도 {_stats.AttackSpeed:F2}");
                SetText(_moveSpeedText,    $"이동 속도 {_stats.MoveSpeed:F2}");
                SetText(_rangeText,        $"사거리 {_stats.Range:F1}");
                SetText(_critChanceText,   $"한방 확률 {_stats.CritChance * 100f:F1}%");
                SetText(_critDamageText,   $"한방 피해 ×{_stats.CritDamage:F2}");
                SetText(_armorText,        $"방어력 {Mathf.RoundToInt(_stats.Armor)}");
                SetText(_damageReductionText, $"피해 감소 {_stats.DamageReduction * 100f:F1}%");
                SetText(_xpGainText,       $"경험치 ×{_stats.XpGainMultiplier:F2}");
                SetText(_dustGainText,     $"별가루 ×{_stats.DustGainMultiplier:F2}");
                SetText(_dropRateText,     $"획득률 ×{_stats.DropRateMultiplier:F2}");
            }
            else if (character != null)
            {
                SetText(_maxHealthText,    $"체력 {Mathf.RoundToInt(character.maxHealth)}");
                SetText(_attackDamageText, $"공격력 {character.attackDamage:F1}");
                SetText(_attackSpeedText,  $"공격 속도 {character.attackSpeed:F2}");
                SetText(_moveSpeedText,    $"이동 속도 {character.moveSpeed:F2}");
                SetText(_rangeText,        $"사거리 {character.range:F1}");
                SetText(_critChanceText,   "한방 확률 -");
                SetText(_critDamageText,   "한방 피해 -");
                SetText(_armorText,        "방어력 -");
                SetText(_damageReductionText, "피해 감소 -");
                SetText(_xpGainText,       "경험치 ×1.00");
                SetText(_dustGainText,     "별가루 ×1.00");
                SetText(_dropRateText,     "획득률 ×1.00");
            }
        }

        private static void SetText(Text t, string s) { if (t != null) t.text = s; }

        public void Close() => gameObject.SetActive(false);
    }
}
