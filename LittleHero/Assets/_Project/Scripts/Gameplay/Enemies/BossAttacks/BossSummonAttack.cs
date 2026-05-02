using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// Meta 보스 공격 — 작은 적 3마리 소환 (boss_attack_v1.md §3 사이클 2).
    /// 1s telegraph (sprite green flash) → boss 주변 3 위치에 EnemySpawner 통해 spawn.
    /// 소환 적은 챕터 enemyPool 의 첫 EnemyData (Small role 추정).
    /// </summary>
    public class BossSummonAttack : IBossAttackPattern
    {
        private const float TELEGRAPH_SEC = 1f;
        private const int SUMMON_COUNT = 3;
        private const float SUMMON_RADIUS = 2f;

        private EnemyController _owner;
        private EnemyData _data;
        private SpriteRenderer _sr;
        private Color _origColor;
        private EnemySpawner _spawner;
        private EnemyData _summonTarget;  // 소환할 EnemyData
        private float _cooldown;
        private float _telegraphTimer;
        private bool _summoning;

        public void Initialize(EnemyController owner, EnemyData data)
        {
            _owner = owner; _data = data;
            _sr = owner.GetComponentInChildren<SpriteRenderer>();
            if (_sr != null) _origColor = _sr.color;
            _cooldown = Random.Range(0f, data.bossAttackCooldown);
            _spawner = Object.FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include);
            // 소환 대상: 챕터의 첫 적 (Small)
            var chapter = MetaProgression.Instance?.SelectedChapter;
            if (chapter != null && chapter.enemyPool != null && chapter.enemyPool.Length > 0)
                _summonTarget = chapter.enemyPool[0];
        }

        public void Tick(float dt)
        {
            if (_owner == null || _owner.IsDead) return;
            if (_summoning)
            {
                _telegraphTimer -= dt;
                if (_sr != null)
                    _sr.color = Color.Lerp(_origColor, new Color(0.4f, 0.9f, 0.5f), Mathf.PingPong((TELEGRAPH_SEC - _telegraphTimer) * 6f, 1f));
                if (_telegraphTimer <= 0f) DoSummon();
            }
            else
            {
                _cooldown -= dt;
                if (_cooldown <= 0f) BeginSummon();
            }
        }

        private void BeginSummon() {
            _summoning = true; _telegraphTimer = TELEGRAPH_SEC;
            AudioManager.Instance?.Play(SfxType.BossSummon);
        }

        private void DoSummon()
        {
            _summoning = false;
            _cooldown = _data.bossAttackCooldown;
            if (_sr != null) _sr.color = _origColor;
            if (_spawner == null || _summonTarget == null) return;
            for (int i = 0; i < SUMMON_COUNT; i++)
            {
                float angle = (i / (float)SUMMON_COUNT) * Mathf.PI * 2f;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * SUMMON_RADIUS;
                var enemy = _spawner.Spawn(_summonTarget);
                if (enemy != null) enemy.transform.position = (Vector2)_owner.transform.position + offset;
            }
        }

        public void Cleanup() { if (_sr != null) _sr.color = _origColor; }
    }
}
