using UnityEngine;
using MOBA.Controllers;

namespace MOBA.Demo
{
    /// <summary>
    /// Debug component to ensure player spawning works correctly.
    /// Add this to any GameObject and it will create a player if none exists.
    /// </summary>
    public class PlayerSpawnDebugger : MonoBehaviour
    {
        [Header("Player Spawn Settings")]
        [SerializeField] private bool autoSpawn = true;
        [SerializeField] private bool forceRespawn = false;
        [SerializeField] private Vector3 spawnPosition = Vector3.zero;
        [SerializeField] private bool createVisual = true;
        [SerializeField] private bool setupCamera = true;
        
        [Header("Visual Settings")]
        [SerializeField] private Color playerColor = Color.blue;
        [SerializeField] private Vector3 visualScale = new Vector3(1f, 1.2f, 1f);
        
        // Runtime references
        private DemoPlayerController currentPlayer;
        private CameraController cameraController;
        
        void Start()
        {
            if (autoSpawn)
            {
                StartCoroutine(SpawnPlayerWithDelay());
            }
        }
        
        private System.Collections.IEnumerator SpawnPlayerWithDelay()
        {
            // Wait a frame to ensure all systems are initialized
            yield return null;
            
            SpawnPlayerIfNeeded();
        }
        
        [ContextMenu("Force Spawn Player")]
        public void ForceSpawnPlayer()
        {
            if (currentPlayer != null && !forceRespawn)
            {
                Debug.Log("Player already exists. Check 'Force Respawn' to recreate.");
                return;
            }
            
            if (currentPlayer != null)
            {
                DestroyImmediate(currentPlayer.gameObject);
            }
            
            SpawnPlayer();
        }
        
        public void SpawnPlayerIfNeeded()
        {
            // Check if player already exists
            currentPlayer = FindFirstObjectByType<DemoPlayerController>();
            
            if (currentPlayer == null)
            {
                SpawnPlayer();
            }
            else
            {
                Debug.Log($"Player found: {currentPlayer.name}");
                if (setupCamera)
                {
                    SetupCameraForPlayer();
                }
            }
        }
        
        private void SpawnPlayer()
        {
            Debug.Log("🎮 Spawning player...");
            
            // Create player GameObject
            GameObject playerObj = new GameObject("Debug Player");
            playerObj.transform.position = spawnPosition;
            playerObj.tag = "Player";
            
            // Add visual representation
            if (createVisual)
            {
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.transform.SetParent(playerObj.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = visualScale;
                visual.name = "Player Visual";
                
                // Apply material using asset manager or create default
                DemoAssetManager assetManager = FindFirstObjectByType<DemoAssetManager>();
                Material playerMat;
                
                if (assetManager != null)
                {
                    playerMat = assetManager.GetMaterial("Player");
                }
                else
                {
                    // Fallback: create material manually
                    playerMat = new Material(Shader.Find("Standard"));
                    playerMat.color = playerColor;
                    playerMat.SetFloat("_Metallic", 0.3f);
                    playerMat.SetFloat("_Smoothness", 0.6f);
                }
                
                visual.GetComponent<Renderer>().material = playerMat;
                
                // Remove collider from visual (parent will handle physics)
                DestroyImmediate(visual.GetComponent<CapsuleCollider>());
            }
            
            // Add player controller
            currentPlayer = playerObj.AddComponent<DemoPlayerController>();
            
            Debug.Log($"✓ Player spawned at {spawnPosition}");
            
            // Setup camera
            if (setupCamera)
            {
                SetupCameraForPlayer();
            }
        }
        
        private void SetupCameraForPlayer()
        {
            // Find camera
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                Debug.LogWarning("No main camera found!");
                return;
            }
            
            // Get or add camera controller
            cameraController = mainCam.GetComponent<CameraController>();
            if (cameraController == null)
            {
                cameraController = mainCam.gameObject.AddComponent<CameraController>();
            }
            
            // Configure camera
            cameraController.SetTarget(currentPlayer.transform);
            cameraController.followTarget = true; // Ensure camera follows
            cameraController.SetCameraMode(CameraController.CameraMode.ThirdPerson);
            
            Debug.Log("✓ Camera connected to player");
        }
        
        void Update()
        {
            // Debug input to manually spawn player
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
            {
                ForceSpawnPlayer();
            }
            
            // Check if player still exists
            if (currentPlayer == null && autoSpawn)
            {
                SpawnPlayerIfNeeded();
            }
        }
        
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 150, 250, 140));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b>Player Debug</b>");
            GUILayout.Label($"Player: {(currentPlayer ? "✓ Active" : "✗ Missing")}");
            GUILayout.Label($"Camera: {(cameraController ? "✓ Connected" : "✗ Not Setup")}");
            
            if (GUILayout.Button("Spawn Player"))
            {
                ForceSpawnPlayer();
            }
            
            if (currentPlayer && GUILayout.Button("Reset Camera"))
            {
                SetupCameraForPlayer();
            }
            
            GUILayout.Label("<i>F9 - Force Spawn</i>");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// Call this to ensure camera follows player properly
        /// </summary>
        public void ResetCameraToFollow()
        {
            if (cameraController != null)
            {
                cameraController.followTarget = true;
                cameraController.EnableFollowOnRespawn(); // Exit free-pan mode
                Debug.Log("Camera reset to follow mode");
            }
        }
        
        /// <summary>
        /// Get the current player reference
        /// </summary>
        public DemoPlayerController GetCurrentPlayer()
        {
            return currentPlayer;
        }
    }
}
