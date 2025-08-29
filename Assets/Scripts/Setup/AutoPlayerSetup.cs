using UnityEngine;
using UnityEngine.InputSystem;
using MOBA.Controllers;
using MOBA.Core;

namespace MOBA.Setup
{
    /// <summary>
    /// Automatically sets up a working player with the new movement system.
    /// Attach this to an empty GameObject and click "Setup Player" in the inspector.
    /// </summary>
    public class AutoPlayerSetup : MonoBehaviour
    {
        [Header("Setup Configuration")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Vector3 spawnPosition = Vector3.zero;
        
        [Header("Ground Setup")]
        [SerializeField] private bool createGround = true;
        [SerializeField] private Vector3 groundSize = new Vector3(20, 1, 20);
        
        [ContextMenu("Setup Player")]
        public void SetupPlayer()
        {
            Debug.Log("[AutoPlayerSetup] Setting up player...");
            
            // Create ground if needed
            if (createGround)
            {
                CreateGround();
            }
            
            // Create player
            GameObject player = CreatePlayer();
            
            // Setup camera to follow player
            SetupCamera(player);
            
            Debug.Log("[AutoPlayerSetup] Setup complete! Press Play and use WASD + Space to test movement.");
        }
        
        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.tag = "Ground"; // Use direct tag assignment for now
            ground.transform.position = new Vector3(0, -groundSize.y / 2, 0);
            ground.transform.localScale = groundSize;
            
            // Ensure ground has proper collider settings
            Collider groundCollider = ground.GetComponent<Collider>();
            if (groundCollider != null)
            {
                groundCollider.isTrigger = false; // Critical: must not be trigger
                
                // Log ground setup for debugging
                Debug.Log($"[AutoPlayerSetup] Ground created at position {ground.transform.position} with bounds {groundCollider.bounds}");
                Debug.Log($"[AutoPlayerSetup] Ground top Y: {groundCollider.bounds.max.y}");
            }
            
            // Make it look like ground - using sharedMaterial to avoid leaks
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                groundMaterial.color = new Color(0.3f, 0.8f, 0.3f); // Green ground
                renderer.sharedMaterial = groundMaterial;
            }
            
            Debug.Log("[AutoPlayerSetup] Created ground");
        }
        
        private GameObject CreatePlayer()
        {
            GameObject player;
            
            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // Create player from scratch
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player";
                player.tag = "Player"; // Use direct tag assignment for now
                player.transform.position = spawnPosition;
                
                // Remove the primitive collider (we'll use CharacterController)
                DestroyImmediate(player.GetComponent<CapsuleCollider>());
                
                // Make it look like a player - using sharedMaterial to avoid leaks
                Renderer renderer = player.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material playerMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    playerMaterial.color = Color.blue; // Blue player
                    renderer.sharedMaterial = playerMaterial;
                }
            }
            
            // Add CharacterController
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = player.AddComponent<CharacterController>();
                controller.center = new Vector3(0, 1, 0);
                controller.radius = 0.5f;
                controller.height = 2f;
                controller.skinWidth = 0.08f;
                controller.minMoveDistance = 0.001f;
                controller.slopeLimit = 45f;
                controller.stepOffset = 0.3f;
            }
            
            // Add UnifiedLocomotionController
            UnifiedLocomotionController movement = player.GetComponent<UnifiedLocomotionController>();
            if (movement == null)
            {
                movement = player.AddComponent<UnifiedLocomotionController>();
            }
            
            // Add UnifiedLocomotionController
            UnifiedLocomotionController playerController = player.GetComponent<UnifiedLocomotionController>();
            if (playerController == null)
            {
                playerController = player.AddComponent<UnifiedLocomotionController>();
            }
            
            // Configure input actions if available
            if (inputActions != null)
            {
                // Use reflection to set the input actions (since it's a SerializeField)
                var field = typeof(UnifiedLocomotionController).GetField("inputActions", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(movement, inputActions);
                    Debug.Log("[AutoPlayerSetup] Assigned input actions");
                }
            }
            else
            {
                Debug.LogWarning("[AutoPlayerSetup] No InputActionAsset assigned. Player may not respond to input.");
            }
            
            Debug.Log("[AutoPlayerSetup] Created player with all components");
            return player;
        }
        
        private void SetupCamera(GameObject player)
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<UnityEngine.Camera>();
            }
            
            if (mainCamera == null)
            {
                // Create a new Main Camera if none exists
                Debug.Log("[AutoPlayerSetup] No camera found, creating Main Camera...");
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<UnityEngine.Camera>();
                mainCamera.tag = "MainCamera";
                
                // Set up standard camera settings
                mainCamera.clearFlags = CameraClearFlags.Skybox;
                mainCamera.cullingMask = -1; // Everything
                mainCamera.nearClipPlane = 0.3f;
                mainCamera.farClipPlane = 1000f;
                mainCamera.fieldOfView = 60f;
            }
            
            if (mainCamera != null)
            {
                // Position camera to see the player
                mainCamera.transform.position = player.transform.position + new Vector3(0, 5, -10);
                mainCamera.transform.LookAt(player.transform);
                
                // Add a simple follow script
                UnifiedCameraController cameraFollow = mainCamera.GetComponent<UnifiedCameraController>();
                if (cameraFollow == null)
                {
                    cameraFollow = mainCamera.gameObject.AddComponent<UnifiedCameraController>();
                }
                // UnifiedCameraController will auto-detect the player target
                
                Debug.Log("[AutoPlayerSetup] Configured camera to follow player");
            }
        }
        
        [ContextMenu("Find Input Actions")]
        public void FindInputActions()
        {
            // Try to find the input actions asset
            string[] guids = UnityEditor.AssetDatabase.FindAssets("InputSystem_Actions");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                inputActions = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                Debug.Log($"[AutoPlayerSetup] Found input actions at: {path}");
            }
            else
            {
                Debug.LogWarning("[AutoPlayerSetup] Could not find InputSystem_Actions asset");
            }
        }
    }
}
