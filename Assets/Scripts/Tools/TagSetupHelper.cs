using UnityEngine;
using MOBA.Controllers;

namespace MOBA.Tools
{
    /// <summary>
    /// Helper tool to set up all required tags for the project
    /// </summary>
    public static class TagSetupHelper
    {
        /// <summary>
        /// Checks and reports which tags are missing from the project
        /// </summary>
        [UnityEditor.MenuItem("Tools/MemeOArena/Validate Required Tags")]
        public static void ValidateRequiredTags()
        {
            string[] requiredTags = {
                "Player",
                "Enemy", 
                "Ground",
                "Wall",
                "Goal",
                "Coin",
                "Obstacle",
                "GameController",
                "Respawn"
            };
            
            Debug.Log("=== TAG VALIDATION START ===");
            
            bool allTagsExist = true;
            
            foreach (string tag in requiredTags)
            {
                if (TagExists(tag))
                {
                    Debug.Log($"✅ Tag '{tag}' exists");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Tag '{tag}' is missing!");
                    allTagsExist = false;
                }
            }
            
            if (allTagsExist)
            {
                Debug.Log("🎉 All required tags are present!");
            }
            else
            {
                Debug.LogError("💥 Some tags are missing. Add them in Edit > Project Settings > Tags & Layers");
                Debug.LogError("📋 Missing tags need to be added manually in Unity Editor");
            }
            
            Debug.Log("=== TAG VALIDATION END ===");
        }
        
        /// <summary>
        /// Auto-fix common tagging issues in the current scene
        /// </summary>
        [UnityEditor.MenuItem("Tools/MemeOArena/Auto-Fix Scene Tags")]
        public static void AutoFixSceneTags()
        {
            Debug.Log("=== AUTO-FIX SCENE TAGS START ===");
            
            // Find and tag player objects
            UnifiedLocomotionController[] players = Object.FindObjectsByType<UnifiedLocomotionController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.gameObject.tag != "Player")
                {
                    player.gameObject.tag = "Player";
                    Debug.Log($"✅ Tagged '{player.gameObject.name}' as Player");
                }
            }
            
            // Find and tag ground objects
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("ground") && obj.tag == "Untagged")
                {
                    if (TagExists("Ground"))
                    {
                        obj.tag = "Ground";
                        Debug.Log($"✅ Tagged '{obj.name}' as Ground");
                    }
                }
                
                // Tag goals
                if (obj.name.ToLower().Contains("goal") && obj.tag == "Untagged")
                {
                    if (TagExists("Goal"))
                    {
                        obj.tag = "Goal";
                        Debug.Log($"✅ Tagged '{obj.name}' as Goal");
                    }
                }
            }
            
            Debug.Log("=== AUTO-FIX SCENE TAGS COMPLETE ===");
        }
        
        private static bool TagExists(string tag)
        {
            try
            {
                GameObject.FindWithTag(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
