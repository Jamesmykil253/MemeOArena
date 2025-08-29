using UnityEngine;

namespace MOBA.Core
{
    /// <summary>
    /// Centralized tag management for MemeOArena - AAA best practice
    /// </summary>
    public static class GameTags
    {
        // Core gameplay tags
        public const string PLAYER = "Player";
        public const string ENEMY = "Enemy";
        public const string GROUND = "Ground";
        public const string WALL = "Wall";
        public const string GOAL = "Goal";
        public const string COIN = "Coin";
        public const string OBSTACLE = "Obstacle";
        
        // UI and system tags
        public const string MAIN_CAMERA = "MainCamera";
        public const string UI = "UI";
        public const string GAME_MANAGER = "GameController";
        public const string SPAWN_POINT = "Respawn";
        
        // Networking tags
        public const string NETWORK_PLAYER = "NetworkPlayer";
        public const string NETWORK_OBJECT = "NetworkObject";
        
        /// <summary>
        /// Safely find GameObject by tag with error logging
        /// </summary>
        public static GameObject FindWithTag(string tag, bool required = true)
        {
            GameObject obj = GameObject.FindWithTag(tag);
            
            if (obj == null && required)
            {
                Debug.LogError($"[GameTags] Required GameObject with tag '{tag}' not found!");
            }
            else if (obj != null)
            {
                Debug.Log($"[GameTags] Found GameObject '{obj.name}' with tag '{tag}'");
            }
            
            return obj;
        }
        
        /// <summary>
        /// Safely assign tag to GameObject with validation
        /// </summary>
        public static void AssignTag(GameObject obj, string tag)
        {
            if (obj == null)
            {
                Debug.LogError($"[GameTags] Cannot assign tag '{tag}' to null GameObject!");
                return;
            }
            
            try
            {
                obj.tag = tag;
                Debug.Log($"[GameTags] Assigned tag '{tag}' to GameObject '{obj.name}'");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameTags] Failed to assign tag '{tag}' to '{obj.name}': {e.Message}");
                Debug.LogError($"[GameTags] Make sure tag '{tag}' exists in Tags & Layers settings!");
            }
        }
        
        /// <summary>
        /// Check if a tag exists in the project
        /// </summary>
        public static bool TagExists(string tag)
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
