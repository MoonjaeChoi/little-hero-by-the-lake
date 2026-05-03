using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5005 Sprint 6 — 보스 씬 복귀 시 신화 발현 1회 토스트 (5002 F5 D2).
    /// AtticController 에서 추출 (§15.1 SRP). BossEncounterContext.LastMythicDefinition 소비.
    /// </summary>
    public class AtticReturnHandler : MonoBehaviour
    {
        private void Start()
        {
            HandleReturnFromBossEncounter();
        }

        private void HandleReturnFromBossEncounter()
        {
            var def = BossEncounterContext.LastMythicDefinition;
            if (def == null) return;

            var overlay = Object.FindFirstObjectByType<MythicEffectOverlay>(FindObjectsInactive.Include);
            if (overlay != null)
            {
                overlay.Play(def);
                Debug.Log($"[AtticReturnHandler] 신화 발현 통지: {def.displayName}");
            }
            else
            {
                Debug.LogWarning("[AtticReturnHandler] MythicEffectOverlay 미존재 — 신화 발현 통지 skip");
            }

            BossEncounterContext.LastMythicDefinition = null;
            BossEncounterContext.LastMythicGranted = null;
        }
    }
}
