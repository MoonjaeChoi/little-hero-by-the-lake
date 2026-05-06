using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v4 — 충돌 폐기 + Auto-Target 정적 헬퍼 (damage_system_v2.md §5.1).
    /// 5032 — Physics2D.OverlapCircle (v6) + ContactFilter2D 정적 (NonAlloc 대체).
    /// 0 GC (사전 buffer 64 + filter 재사용) + 매 프레임 호출 안전.
    /// </summary>
    public static class DamageQuery
    {
        private const int BUFFER_SIZE = 64;
        private static readonly Collider2D[] _buffer = new Collider2D[BUFFER_SIZE];
        private static readonly List<EnemyController> _scratch = new List<EnemyController>(BUFFER_SIZE);
        private static int _enemyLayerMask = -1;
        private static ContactFilter2D _filter;
        private static bool _filterInit;

        private static int EnemyMask
        {
            get
            {
                if (_enemyLayerMask < 0) _enemyLayerMask = LayerMask.GetMask("Enemy");
                return _enemyLayerMask;
            }
        }

        private static void EnsureFilter()
        {
            if (_filterInit) return;
            _filter = new ContactFilter2D();
            _filter.SetLayerMask(EnemyMask);
            _filter.useTriggers = true;  // 기존 OverlapCircleNonAlloc 답습
            _filterInit = true;
        }

        /// <summary>원형 영역 내 모든 적 검색 (outResult 에 채움). 반환 = 검색된 수.</summary>
        public static int FindEnemiesInCircle(Vector2 center, float radius, List<EnemyController> outResult)
        {
            EnsureFilter();
            outResult.Clear();
            int count = Physics2D.OverlapCircle(center, radius, _filter, _buffer);
            for (int i = 0; i < count; i++)
            {
                var col = _buffer[i];
                if (col == null) continue;
                var enemy = col.GetComponentInParent<EnemyController>();
                if (enemy != null && !enemy.IsDead) outResult.Add(enemy);
            }
            return outResult.Count;
        }

        /// <summary>가장 가까운 적 1마리 (없으면 null).</summary>
        public static EnemyController FindNearest(Vector2 center, float radius)
        {
            FindEnemiesInCircle(center, radius, _scratch);
            EnemyController nearest = null;
            float minSq = float.MaxValue;
            for (int i = 0; i < _scratch.Count; i++)
            {
                float sq = ((Vector2)_scratch[i].transform.position - center).sqrMagnitude;
                if (sq < minSq) { minSq = sq; nearest = _scratch[i]; }
            }
            return nearest;
        }

        /// <summary>가까운 N마리 (거리 정렬, outResult 에 N 자르기).</summary>
        public static int FindNearestN(Vector2 center, float radius, int n, List<EnemyController> outResult)
        {
            FindEnemiesInCircle(center, radius, outResult);
            outResult.Sort((a, b) =>
            {
                float sa = ((Vector2)a.transform.position - center).sqrMagnitude;
                float sb = ((Vector2)b.transform.position - center).sqrMagnitude;
                return sa.CompareTo(sb);
            });
            if (outResult.Count > n) outResult.RemoveRange(n, outResult.Count - n);
            return outResult.Count;
        }
    }
}
