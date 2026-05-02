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
        [SerializeField] private Text _bonusText;          // 스타포스 조각 + 소환 레벨업
        [SerializeField] private Image _backdrop;
        [SerializeField] private Button _closeButton;

        [Header("displayName lookup")]
        [SerializeField] private GachaPool _pool;

        [Header("등급별 색")]
        [SerializeField] private Color _commonColor    = new Color(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color _magicColor     = new Color(0.5f, 0.85f, 1f);
        [SerializeField] private Color _rareColor      = new Color(0.4f, 1f, 0.5f);
        [SerializeField] private Color _epicColor      = new Color(0.85f, 0.5f, 1f);
        [SerializeField] private Color _legendaryColor = new Color(1f, 0.7f, 0.3f);
        [SerializeField] private Color _mythicColor    = new Color(1f, 0.3f, 0.6f);

        private void OnEnable()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            // 출시 전 fix #4 패턴 — 활성화 시 가장 마지막에 그려져 입력·시각 우선권 확보.
            transform.SetAsLastSibling();
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
        }

        public void ShowSingle(GachaResult result)
        {
            if (result == null) return;
            gameObject.SetActive(true);

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
                sb.Append($"+{result.starforceShards} 스타포스 조각");
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
                bsb.Append($"+{totalShards} 스타포스 조각");
                if (newLevel > 0)
                    bsb.Append($"\n소환 레벨 ⭐ {newLevel} 도달!");
                _bonusText.text = bsb.ToString();
            }
        }

        public void Close() => gameObject.SetActive(false);

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
