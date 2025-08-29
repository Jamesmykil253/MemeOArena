using UnityEngine;
using UnityEditor;

namespace MOBA.Demo.Editor
{
    /// <summary>
    /// Quick material fix for demo - creates proper materials if they're missing or broken
    /// </summary>
    public class MaterialFixer : EditorWindow
    {
        [MenuItem("MemeOArena/Fix Demo Materials")]
        public static void ShowWindow()
        {
            GetWindow<MaterialFixer>("Material Fixer");
        }
        
        [MenuItem("MemeOArena/Quick Demo Fix")]
        public static void QuickDemoFix()
        {
            FixAllMaterials();
            CreateDemoScene();
        }
        
        void OnGUI()
        {
            GUILayout.Label("MemeOArena Demo Material Fixer", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Fix All Materials", GUILayout.Height(30)))
            {
                FixAllMaterials();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Create Working Demo Scene", GUILayout.Height(30)))
            {
                CreateDemoScene();
            }
            
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox("This will:\n• Fix pink/missing materials\n• Create working player prefab\n• Setup proper scene lighting\n• Add demo objects for testing", MessageType.Info);
        }
        
        static void FixAllMaterials()
        {
            Debug.Log("🔧 Fixing materials...");
            
            // Create basic materials
            CreateMaterial("Ground", new Color(0.3f, 0.7f, 0.3f), "Assets/Materials/Ground.mat");
            CreateMaterial("Player", new Color(0.2f, 0.6f, 1f), "Assets/Materials/Player.mat");
            CreateMaterial("Coins", Color.yellow, "Assets/Materials/Coins.mat");
            CreateMaterial("Goal", new Color(1f, 0.5f, 0f), "Assets/Materials/Goal.mat");
            CreateMaterial("Enemy", Color.red, "Assets/Materials/Enemy.mat");
            CreateMaterial("Obstacle", new Color(0.6f, 0.6f, 0.6f), "Assets/Materials/Obstacle.mat");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("✅ Materials fixed!");
        }
        
        static void CreateMaterial(string name, Color color, string path)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Smoothness", 0.5f);
            
            if (name == "Player" || name == "Coins")
            {
                mat.SetFloat("_Metallic", 0.3f);
                mat.SetFloat("_Smoothness", 0.8f);
            }
            
            AssetDatabase.CreateAsset(mat, path);
            Debug.Log($"Created material: {name} at {path}");
        }
        
        static void CreateDemoScene()
        {
            Debug.Log("🎮 Creating working demo scene...");
            
            // Add the master scene setup to handle everything
            GameObject demoManager = new GameObject("Demo Manager");
            demoManager.AddComponent<MOBA.Demo.MasterSceneSetup>();
            
            Debug.Log("✅ Demo scene created! MasterSceneSetup will auto-configure everything.");
            Debug.Log("Press Play to see the working demo with player, camera, and environment!");
        }
    }
}
