using UnityEngine;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5024 D2=B — RNG 결정성 헬퍼. SetUp 에서 InitDeterministic, TearDown 에서 RestoreRandom.
    /// </summary>
    public static class TestUtils
    {
        private static UnityEngine.Random.State _savedState;
        private static bool _saved;

        public static void InitDeterministic(int seed)
        {
            if (!_saved) { _savedState = UnityEngine.Random.state; _saved = true; }
            UnityEngine.Random.InitState(seed);
        }

        public static void RestoreRandom()
        {
            if (_saved) { UnityEngine.Random.state = _savedState; _saved = false; }
        }
    }
}
