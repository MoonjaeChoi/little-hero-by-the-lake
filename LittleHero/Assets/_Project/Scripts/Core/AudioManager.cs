using UnityEngine;

namespace LittleHero
{
    public enum SfxType
    {
        EnemyPop, Pickup, LevelUp, MemoryFound, PlayerHit,
        // 보스 공격 (boss_attack_v1.md 사이클 C)
        BossTelegraph, BossDetonate, BossCharge, BossSummon, BossTornado,
        // PIVOT v4 Premium 피격
        EnemyHit, CritHit,
    }

    /// <summary>
    /// 5027 — facade. SFX clip cache + Play 위임 + AudioBgmController + ChapterBgmMap.
    /// AudioPlayerSfxBank/AudioBossSfxBank 분리, AudioBgmController POCO 위임.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _src;
        private AudioBgmController _bgm;

        // SFX clip cache (Awake 1회 합성)
        private AudioClip _popClip, _pickupClip, _levelUpClip, _memoryClip, _hitClip;
        private AudioClip _bossTelegraph, _bossDetonate, _bossCharge, _bossSummon, _bossTornado;
        private AudioClip _enemyHit, _critHit;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<AudioManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _src = gameObject.AddComponent<AudioSource>();
            _src.playOnAwake = false; _src.loop = false; _src.spatialBlend = 0f; _src.volume = 0.6f;

            var bgmMain = gameObject.AddComponent<AudioSource>();
            bgmMain.playOnAwake = false; bgmMain.loop = true; bgmMain.spatialBlend = 0f; bgmMain.volume = 0.4f;
            var bgmFade = gameObject.AddComponent<AudioSource>();
            bgmFade.playOnAwake = false; bgmFade.loop = true; bgmFade.spatialBlend = 0f; bgmFade.volume = 0f;
            _bgm = new AudioBgmController(bgmMain, bgmFade, this);

            _popClip       = AudioPlayerSfxBank.Pop();
            _pickupClip    = AudioPlayerSfxBank.Pickup();
            _levelUpClip   = AudioPlayerSfxBank.LevelUp();
            _memoryClip    = AudioPlayerSfxBank.Memory();
            _hitClip       = AudioPlayerSfxBank.Hit();
            _enemyHit      = AudioPlayerSfxBank.EnemyHit();
            _critHit       = AudioPlayerSfxBank.CritHit();
            _bossTelegraph = AudioBossSfxBank.Telegraph();
            _bossDetonate  = AudioBossSfxBank.Detonate();
            _bossCharge    = AudioBossSfxBank.Charge();
            _bossSummon    = AudioBossSfxBank.Summon();
            _bossTornado   = AudioBossSfxBank.Tornado();

            GameEvents.OnEnemyKilled       += _ => Play(SfxType.EnemyPop);
            GameEvents.OnLevelUp           += _ => Play(SfxType.LevelUp);
            GameEvents.OnMemoryDiscovered  += _ => Play(SfxType.MemoryFound);
            GameEvents.OnPlayerHealthChanged += _ => { /* hit 은 호출자가 직접 Play */ };
        }

        public void Play(SfxType type)
        {
            if (_src == null) return;
            AudioClip clip = type switch
            {
                SfxType.EnemyPop      => _popClip,
                SfxType.Pickup        => _pickupClip,
                SfxType.LevelUp       => _levelUpClip,
                SfxType.MemoryFound   => _memoryClip,
                SfxType.PlayerHit     => _hitClip,
                SfxType.BossTelegraph => _bossTelegraph,
                SfxType.BossDetonate  => _bossDetonate,
                SfxType.BossCharge    => _bossCharge,
                SfxType.BossSummon    => _bossSummon,
                SfxType.BossTornado   => _bossTornado,
                SfxType.EnemyHit      => _enemyHit,
                SfxType.CritHit       => _critHit,
                _ => null,
            };
            if (clip != null) _src.PlayOneShot(clip);
        }

        public void PlayBgm(string name, float crossFadeSec = 1.5f) => _bgm?.PlayBgm(name, crossFadeSec);
        public void StopBgm(float fadeSec = 0.5f) => _bgm?.StopBgm(fadeSec);

        public static string GetChapterBgm(int chapterNumber) => AudioChapterBgmMap.Get(chapterNumber);
    }
}
