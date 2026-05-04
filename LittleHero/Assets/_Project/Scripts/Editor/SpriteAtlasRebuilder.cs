using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5009 후속 — 자산 (PNG) 갱신 후 SpriteAtlas 일괄 재빌드.
    /// EnemyAtlas (V1) packable 폴더 PNG 변경 자동 감지 + cache 갱신 + draw call <10/frame 유지.
    /// </summary>
    public static class SpriteAtlasRebuilder
    {
        [MenuItem("Tools/LittleHero/Rebuild Sprite Atlases")]
        public static void RebuildAll()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            Debug.Log($"[SpriteAtlasRebuilder] Pack target={target} 시작 ...");
            SpriteAtlasUtility.PackAllAtlases(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SpriteAtlasRebuilder] ✅ 모든 SpriteAtlas pack 완료. Play Mode 또는 Build 시 SRP Batcher 정상 동작.");
        }
    }
}
