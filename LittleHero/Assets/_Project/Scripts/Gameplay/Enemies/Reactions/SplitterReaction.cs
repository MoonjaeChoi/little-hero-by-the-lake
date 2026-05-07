using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 T04 — 사망 시 splitterChildData 의 적을 splitterChildCount 마리 소환 (D3=B 3마리 default).
    /// 곰형 적 (탕탕특공대 답습) — 사망 위치에서 분열.
    /// </summary>
    public class SplitterReaction : IDeathReaction
    {
        public void OnDeath(EnemyController owner, Vector3 deathPosition)
        {
            if (owner == null || owner.Data == null) return;
            var data = owner.Data;
            if (data.splitterChildData == null || data.splitterChildData.prefab == null) return;
            if (data.splitterChildCount <= 0) return;

            for (int i = 0; i < data.splitterChildCount; i++)
            {
                // 0.5u 분산 spawn (원형 배치)
                float angle = i * Mathf.PI * 2f / data.splitterChildCount;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f, 0f);
                Vector3 pos = deathPosition + offset;

                var go = Object.Instantiate(data.splitterChildData.prefab, pos, Quaternion.identity);
                var child = go.GetComponent<EnemyController>();
                if (child != null) child.Initialize(data.splitterChildData);
            }
        }
    }
}
