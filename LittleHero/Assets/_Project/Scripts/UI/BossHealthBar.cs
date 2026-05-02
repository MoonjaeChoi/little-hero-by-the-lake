using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 보스 체력바 (HUD 상단). GameEvents.OnBossSpawned 구독으로 자동 활성화.
    /// 폴링 갱신 (매 프레임 boss.HealthRatio 읽음, OnDied 시 OnBossDespawned 자동 비활성).
    /// 평소 비활성. 챕터 보스 / 일일 보스 / 주간 보스 / 무한 다락방 보스 모두 표시.
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject _root;     // 활성/비활성 토글 루트
        [SerializeField] private Slider _slider;       // value 0~1 비율
        [SerializeField] private Text _nameText;       // 보스 displayName
        [SerializeField] private Text _hpText;         // "HP 1234 / 5678" (선택)

        private EnemyController _trackedBoss;

        private void Awake()
        {
            if (_root == null) _root = gameObject;
            if (_slider == null) _slider = GetComponentInChildren<Slider>(true);
            // 주의: _root 가 자기 자신이면 GameObject 비활성화 시 OnEnable 호출 안 됨 →
            // 이벤트 구독 못 함 → 보스 spawn 이벤트 못 받는 버그 발생.
            // 자식들만 비활성화하여 컴포넌트는 항상 활성 유지.
            SetVisualActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnBossSpawned += HandleSpawned;
            GameEvents.OnBossDespawned += HandleDespawned;
            GameEvents.OnPlayerDied += HandleDespawned;
            GameEvents.OnChapterCleared += _ => HandleDespawned();
        }

        private void OnDisable()
        {
            GameEvents.OnBossSpawned -= HandleSpawned;
            GameEvents.OnBossDespawned -= HandleDespawned;
            GameEvents.OnPlayerDied -= HandleDespawned;
        }

        private void HandleSpawned(EnemyController boss)
        {
            if (boss == null) return;
            _trackedBoss = boss;
            SetVisualActive(true);
            if (_nameText != null) _nameText.text = boss.Data != null ? boss.Data.displayName : "?";
            if (_slider != null)
            {
                _slider.minValue = 0f;
                _slider.maxValue = 1f;
                _slider.value = 1f;
            }
        }

        private void HandleDespawned()
        {
            _trackedBoss = null;
            SetVisualActive(false);
        }

        private void SetVisualActive(bool active)
        {
            if (_root == null) return;
            if (_root != gameObject)
            {
                _root.SetActive(active);
                return;
            }
            // _root 가 자기 자신이면 자식들만 토글 (이 컴포넌트는 항상 활성 유지)
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(active);
        }

        private void Update()
        {
            if (_trackedBoss == null) return;
            if (_trackedBoss.IsDead) { HandleDespawned(); return; }
            if (_slider != null) _slider.value = _trackedBoss.HealthRatio;
            if (_hpText != null)
                _hpText.text = $"{Mathf.CeilToInt(_trackedBoss.HealthRatio * _trackedBoss.MaxHealth):N0} / {Mathf.CeilToInt(_trackedBoss.MaxHealth):N0}";
        }
    }
}
