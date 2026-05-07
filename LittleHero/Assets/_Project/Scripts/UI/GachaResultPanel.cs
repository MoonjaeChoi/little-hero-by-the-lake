using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// PIVOT D-가챠 2 — 가챠 결과 연출 (gacha_v1.md §1.8).
    ///
    /// 단판: 1.5s 텍스트 페이드인 + 등급 색.
    /// 10연차: 결과 그리드 + 보장 발동 표시 + 소환 레벨업 표시.
    /// 닫기 버튼으로 종료. 비폭력 톤 — 무지개·별 fade 만.
    /// </summary>
    public class GachaResultPanel : MonoBehaviour
    {
        [Header("표시")]
        [SerializeField] private Text _summaryText;        // 결과 요약 (단판: 1줄, 10연차: 그리드)
        [SerializeField] private Text _rarityText;         // 단판 전용 — 등급 색 강조
        [SerializeField] private Text _bonusText;          // 마법의돌 조각 + 소환 레벨업
        [SerializeField] private Image _backdrop;
        [SerializeField] private Button _closeButton;

        [Header("displayName lookup")]
        [SerializeField] private GachaPool _pool;

        [Header("무기 아이콘 (단판: 결과 / 10연차: 최고 등급)")]
        [SerializeField] private Image _weaponIconImage;

        [Header("등급별 frame (6 Image, 결과 시 1개만 활성)")]
        [SerializeField] private Image _frameCommon;
        [SerializeField] private Image _frameMagic;
        [SerializeField] private Image _frameRare;
        [SerializeField] private Image _frameEpic;
        [SerializeField] private Image _frameLegendary;
        [SerializeField] private Image _frameMythic;

        [Header("등급별 색")]
        [SerializeField] private Color _commonColor    = new Color(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color _magicColor     = new Color(0.5f, 0.85f, 1f);
        [SerializeField] private Color _rareColor      = new Color(0.4f, 1f, 0.5f);
        [SerializeField] private Color _epicColor      = new Color(0.85f, 0.5f, 1f);
        [SerializeField] private Color _legendaryColor = new Color(1f, 0.7f, 0.3f);
        [SerializeField] private Color _mythicColor    = new Color(1f, 0.3f, 0.6f);

        private void Awake()
        {
            // 5033.0j — Awake 1회 등록 (panel default active 시 OnEnable 미트리거 회피)
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            // 출시 전 fix #4 패턴 — 활성화 시 가장 마지막에 그려져 입력·시각 우선권 확보.
            transform.SetAsLastSibling();
            // 5033.0j — CloseButton 을 panel 자식 sibling 마지막으로 (10연차 텍스트가 X 가리는 결함 회피)
            if (_closeButton != null)
            {
                _closeButton.gameObject.SetActive(true);
                _closeButton.transform.SetAsLastSibling();
                // 5033.0k — RectTransform 우상단 강제 (Inspector layout 결함 회피)
                var rt = _closeButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot     = new Vector2(1f, 1f);
                    rt.anchoredPosition = new Vector2(-20f, -20f);
                    if (rt.sizeDelta.x < 40f || rt.sizeDelta.y < 40f)
                        rt.sizeDelta = new Vector2(56f, 56f);
                }
            }
        }

        private void OnDestroy()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
        }

        public void ShowSingle(GachaResult result)
        {
            if (result == null) return;
            gameObject.SetActive(true);
            ActivateFrame(result.rarity);
            ApplyWeaponIcon(result);

            string label = ResolveTemplateName(result);
            if (_summaryText != null) _summaryText.text = label;
            if (_rarityText != null)
            {
                _rarityText.text = RarityKorean(result.rarity);
                _rarityText.color = ColorFor(result.rarity);
            }
            if (_bonusText != null)
            {
                var sb = new StringBuilder();
                sb.Append($"+{result.starforceShards} 마법의돌 조각");
                if (result.causedSummonLevelUp)
                    sb.Append($"\n소환 레벨 ⭐ {result.newSummonLevel} 도달!");
                _bonusText.text = sb.ToString();
            }
        }

        public void Show10x(GachaResult[] results)
        {
            if (results == null || results.Length == 0) return;
            gameObject.SetActive(true);

            int totalShards = 0;
            int highestRarity = 0;
            int newLevel = 0;

            // 최고 등급으로 frame 활성
            for (int i = 0; i < results.Length; i++)
                if (results[i] != null && (int)results[i].rarity > highestRarity)
                    highestRarity = (int)results[i].rarity;
            ActivateFrame((Rarity)highestRarity);
            // 최고 등급 결과 1개를 무기 아이콘으로 표시
            GachaResult highest = null;
            for (int i = 0; i < results.Length; i++)
                if (results[i] != null && (highest == null || (int)results[i].rarity >= (int)highest.rarity))
                    highest = results[i];
            ApplyWeaponIcon(highest);
            // (highestRarity 는 후속 sb 루프에서도 재계산되니 0으로 리셋)
            highestRarity = 0;

            var sb = new StringBuilder();
            sb.AppendLine("10연차 결과");
            for (int i = 0; i < results.Length; i++)
            {
                var r = results[i];
                if (r == null) continue;
                string mark = r.isRarePlusGuaranteed ? " ★보장" : "";
                sb.AppendLine($"{i + 1}. [{RarityKorean(r.rarity)}] {ResolveTemplateName(r)}{mark}");
                totalShards += r.starforceShards;
                if ((int)r.rarity > highestRarity) highestRarity = (int)r.rarity;
                if (r.causedSummonLevelUp) newLevel = r.newSummonLevel;
            }

            if (_summaryText != null) _summaryText.text = sb.ToString();
            if (_rarityText != null)
            {
                _rarityText.text = $"최고 등급: {RarityKorean((Rarity)highestRarity)}";
                _rarityText.color = ColorFor((Rarity)highestRarity);
            }
            if (_bonusText != null)
            {
                var bsb = new StringBuilder();
                bsb.Append($"+{totalShards} 마법의돌 조각");
                if (newLevel > 0)
                    bsb.Append($"\n소환 레벨 ⭐ {newLevel} 도달!");
                _bonusText.text = bsb.ToString();
            }
        }

        public void Close() => gameObject.SetActive(false);

        private void ApplyWeaponIcon(GachaResult result)
        {
            if (_weaponIconImage == null) return;
            if (result == null || _pool == null || string.IsNullOrEmpty(result.templateId))
            {
                _weaponIconImage.enabled = false;
                return;
            }
            var t = FindTemplate(_pool, result.templateId);
            if (t == null || t.icon == null)
            {
                _weaponIconImage.enabled = false;
                return;
            }
            _weaponIconImage.sprite = t.icon;
            _weaponIconImage.enabled = true;
        }

        private void ActivateFrame(Rarity rarity)
        {
            if (_frameCommon    != null) _frameCommon.gameObject.SetActive(rarity == Rarity.Common);
            if (_frameMagic     != null) _frameMagic.gameObject.SetActive(rarity == Rarity.Magic);
            if (_frameRare      != null) _frameRare.gameObject.SetActive(rarity == Rarity.Rare);
            if (_frameEpic      != null) _frameEpic.gameObject.SetActive(rarity == Rarity.Epic);
            if (_frameLegendary != null) _frameLegendary.gameObject.SetActive(rarity == Rarity.Legendary);
            if (_frameMythic    != null) _frameMythic.gameObject.SetActive(rarity == Rarity.Mythic);
        }

        private string ResolveTemplateName(GachaResult r)
        {
            if (r == null || string.IsNullOrEmpty(r.templateId)) return "?";
            if (_pool != null)
            {
                var t = FindTemplate(_pool, r.templateId);
                if (t != null && !string.IsNullOrEmpty(t.displayName)) return t.displayName;
            }
            return r.templateId;
        }

        private static EquipmentTemplate FindTemplate(GachaPool pool, string id)
        {
            var groups = new[]
            {
                pool.commonPool, pool.magicPool, pool.rarePool,
                pool.epicPool, pool.legendaryPool, pool.mythicPool,
            };
            for (int g = 0; g < groups.Length; g++)
            {
                var arr = groups[g];
                if (arr == null) continue;
                for (int i = 0; i < arr.Length; i++)
                    if (arr[i] != null && arr[i].id == id) return arr[i];
            }
            return null;
        }

        private static string RarityKorean(Rarity r) => r switch
        {
            Rarity.Common    => "일반",
            Rarity.Magic     => "고급",
            Rarity.Rare      => "희귀",
            Rarity.Epic      => "영웅",
            Rarity.Legendary => "전설",
            Rarity.Mythic    => "신화",
            _ => r.ToString(),
        };

        private Color ColorFor(Rarity r) => r switch
        {
            Rarity.Common    => _commonColor,
            Rarity.Magic     => _magicColor,
            Rarity.Rare      => _rareColor,
            Rarity.Epic      => _epicColor,
            Rarity.Legendary => _legendaryColor,
            Rarity.Mythic    => _mythicColor,
            _ => Color.white,
        };
    }
}
