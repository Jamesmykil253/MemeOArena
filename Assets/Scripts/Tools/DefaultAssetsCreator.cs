using UnityEngine;
using MOBA.Data;

namespace MOBA.Tools
{
    /// <summary>
    /// Helper to create default game assets. Right-click in Project and use context menu.
    /// </summary>
    public static class DefaultAssetsCreator
    {
        [UnityEditor.MenuItem("Assets/Create/Game/Create Default Player Stats")]
        public static void CreateDefaultPlayerStats()
        {
            BaseStatsTemplate stats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            
            // Set AAA-standard values
            stats.MaxHP = 100;
            stats.Attack = 25;
            stats.Defense = 15;
            stats.MoveSpeed = 5.0f;
            stats.MagicDefense = 10;
            
            string path = "Assets/Data/DefaultPlayerStats.asset";
            UnityEditor.AssetDatabase.CreateAsset(stats, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            // Select the created asset
            UnityEditor.Selection.activeObject = stats;
            UnityEditor.EditorGUIUtility.PingObject(stats);
            
            Debug.Log($"[DefaultAssetsCreator] Created DefaultPlayerStats at {path}");
        }
        
        [UnityEditor.MenuItem("Assets/Create/Game/Setup Complete Scene")]
        public static void CreateCompleteScene()
        {
            // Create main managers
            GameObject managers = new GameObject("=== MANAGERS ===");
            GameObject gameManager = new GameObject("GameManager");
            GameObject uiManager = new GameObject("UIManager");
            
            gameManager.transform.SetParent(managers.transform);
            uiManager.transform.SetParent(managers.transform);
            
            // Create environment
            GameObject environment = new GameObject("=== ENVIRONMENT ===");
            
            // Create ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * 2; // 20x20 units
            ground.transform.SetParent(environment.transform);
            
            // Create player section
            GameObject playerSection = new GameObject("=== PLAYER ===");
            
            // Create UI section
            GameObject uiSection = new GameObject("=== UI ===");
            
            Debug.Log("[DefaultAssetsCreator] Created professional scene structure!");
        }
    }
}
