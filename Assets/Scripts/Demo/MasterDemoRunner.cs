using UnityEngine;
using MOBA.Controllers;

namespace MOBA.Demo
{
    /// <summary>
    /// Master demo runner that orchestrates the complete demo experience.
    /// This script should be added to an empty GameObject in the scene.
    /// </summary>
    public class MasterDemoRunner : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private bool showWelcomeMessage = true;
        [SerializeField] private bool createPickups = true;
        [SerializeField] private int numberOfPickups = 5;
        
        [Header("Scene References")]
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform cameraPosition;
        
        // Runtime references
        private DemoPlayerController playerController;
        private CameraController cameraController;
        private bool isSetupComplete = false;
        
        void Start()
        {
            if (autoSetup)
            {
                StartCoroutine(SetupDemoWithDelay());
            }
        }
        
        private System.Collections.IEnumerator SetupDemoWithDelay()
        {
            // Small delay to ensure all Awake calls are done
            yield return new WaitForSeconds(0.1f);
            
            SetupMasterDemo();
        }
        
        [ContextMenu("Setup Master Demo")]
        public void SetupMasterDemo()
        {
            if (isSetupComplete)
            {
                Debug.Log("Demo already set up. Use 'Reset Demo' to start over.");
                return;
            }
            
            Debug.Log("🎮 Starting MemeOArena Master Demo Setup...");
            
            try
            {
                // Step 1: Environment
                CreateEnvironment();
                
                // Step 2: Player
                SetupPlayer();
                
                // Step 3: Camera
                SetupCamera();
                
                // Step 4: Interactive elements
                if (createPickups)
                {
                    CreatePointPickups();
                }
                
                // Step 5: UI and instructions
                SetupUI();
                
                isSetupComplete = true;
                
                Debug.Log("🎉 MemeOArena Demo Setup Complete!");
                
                if (showWelcomeMessage)
                {
                    ShowWelcomeInstructions();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Demo setup failed: {e.Message}");
            }
        }
        
        private void CreateEnvironment()
        {
            Debug.Log("Creating demo environment...");
            
            // Create ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Demo Ground";
            ground.transform.localScale = new Vector3(10f, 1f, 10f);
            ground.transform.position = new Vector3(0, -0.5f, 0);
            
            // Ground material
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.3f, 0.6f, 0.3f); // Nice grass green
            ground.GetComponent<Renderer>().material = groundMat;
            
            // Create some obstacles
            CreateObstacles();
            
            // Add lighting
            EnsureLighting();
        }
        
        private void CreateObstacles()
        {
            // Create a few interesting obstacles
            Vector3[] positions = {
                new Vector3(8f, 1f, 8f),
                new Vector3(-8f, 1f, 8f),
                new Vector3(8f, 1f, -8f),
                new Vector3(-8f, 1f, -8f),
                new Vector3(0f, 2f, 12f),
                new Vector3(12f, 1f, 0f)
            };
            
            string[] shapes = { "Cube", "Sphere", "Cube", "Sphere", "Cylinder", "Cube" };
            
            for (int i = 0; i < positions.Length; i++)
            {
                PrimitiveType shapeType = shapes[i] == "Sphere" ? PrimitiveType.Sphere : 
                                        shapes[i] == "Cylinder" ? PrimitiveType.Cylinder : 
                                        PrimitiveType.Cube;
                
                GameObject obstacle = GameObject.CreatePrimitive(shapeType);
                obstacle.name = $"Obstacle_{i + 1}";
                obstacle.transform.position = positions[i];
                obstacle.transform.localScale = Vector3.one * Random.Range(1.5f, 2.5f);
                
                // Random material
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.HSVToRGB((float)i / positions.Length, 0.7f, 0.9f);
                obstacle.GetComponent<Renderer>().material = mat;
            }
        }
        
        private void EnsureLighting()
        {
            Light mainLight = FindFirstObjectByType<Light>();
            if (mainLight == null)
            {
                GameObject lightObj = new GameObject("Demo Light");
                mainLight = lightObj.AddComponent<Light>();
                mainLight.type = LightType.Directional;
                mainLight.transform.rotation = Quaternion.Euler(50f, 30f, 0f);
                mainLight.intensity = 1.2f;
            }
        }
        
        private void SetupPlayer()
        {
            Debug.Log("Setting up player...");
            
            // Find or create player
            playerController = FindFirstObjectByType<DemoPlayerController>();
            
            if (playerController == null)
            {
                GameObject playerObj = new GameObject("MemeO Player");
                
                // Position
                Vector3 spawnPos = playerSpawnPoint ? playerSpawnPoint.position : Vector3.zero;
                playerObj.transform.position = spawnPos;
                playerObj.tag = "Player";
                
                // Add visual representation
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.transform.SetParent(playerObj.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(1f, 1.2f, 1f); // Slightly taller
                visual.name = "Player Visual";
                
                // Cool blue material
                Material playerMat = new Material(Shader.Find("Standard"));
                playerMat.color = new Color(0.2f, 0.5f, 1f); // Nice blue
                playerMat.SetFloat("_Metallic", 0.3f);
                visual.GetComponent<Renderer>().material = playerMat;
                
                // Remove collider from visual (let controller handle physics)
                DestroyImmediate(visual.GetComponent<CapsuleCollider>());
                
                // Add the controller
                playerController = playerObj.AddComponent<DemoPlayerController>();
                
                Debug.Log("✓ Player created with visual representation");
            }
            else
            {
                Debug.Log("✓ Existing player found");
            }
        }
        
        private void SetupCamera()
        {
            Debug.Log("Setting up camera system...");
            
            // Find or create camera
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Demo Camera");
                mainCam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }
            
            // Add controller
            cameraController = mainCam.GetComponent<CameraController>();
            if (cameraController == null)
            {
                cameraController = mainCam.gameObject.AddComponent<CameraController>();
            }
            
            // Configure camera
            if (playerController != null)
            {
                cameraController.SetTarget(playerController.transform);
                cameraController.SetCameraMode(CameraController.CameraMode.ThirdPerson);
                
                // Nice settings for demo
                cameraController.followSpeed = 6f;
                cameraController.rotationSpeed = 4f;
                cameraController.panSpeed = 8f;
                
                Debug.Log("✓ Camera connected to player");
            }
        }
        
        private void CreatePointPickups()
        {
            Debug.Log($"Creating {numberOfPickups} point pickups...");
            
            for (int i = 0; i < numberOfPickups; i++)
            {
                // Random position around the play area
                Vector3 pos = new Vector3(
                    Random.Range(-15f, 15f),
                    1f,
                    Random.Range(-15f, 15f)
                );
                
                // Create pickup
                GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pickup.name = $"Point Orb {i + 1}";
                pickup.transform.position = pos;
                pickup.transform.localScale = Vector3.one * 0.8f;
                
                // Golden material
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.yellow;
                mat.SetFloat("_Metallic", 0.9f);
                mat.SetFloat("_Smoothness", 0.8f);
                pickup.GetComponent<Renderer>().material = mat;
                
                // Make it a trigger
                pickup.GetComponent<SphereCollider>().isTrigger = true;
                
                // Add pickup behavior
                DemoPointPickup pickupScript = pickup.AddComponent<DemoPointPickup>();
                
                // Add spinning
                DemoSpinner spinner = pickup.AddComponent<DemoSpinner>();
                spinner.SetSpinSpeed(45f);
                
                Debug.Log($"✓ Created pickup at {pos}");
            }
        }
        
        private void SetupUI()
        {
            Debug.Log("Setting up UI...");
            
            // Add camera controls UI
            GameObject uiObj = new GameObject("Camera UI");
            uiObj.AddComponent<CameraControlsUI>();
            
            Debug.Log("✓ UI components added");
        }
        
        private void ShowWelcomeInstructions()
        {
            Debug.Log("════════════════════════════════════════");
            Debug.Log("🎮 WELCOME TO MEME-O-ARENA DEMO! 🎮");
            Debug.Log("════════════════════════════════════════");
            Debug.Log("");
            Debug.Log("📋 CONTROLS:");
            Debug.Log("• WASD - Move your character");
            Debug.Log("• C - Cycle camera modes");
            Debug.Log("• V - Toggle free-pan camera");
            Debug.Log("• P - Add test points");
            Debug.Log("• T - Take test damage");
            Debug.Log("• E - Channel scoring (need points)");
            Debug.Log("");
            Debug.Log("🎯 OBJECTIVES:");
            Debug.Log("• Walk over golden orbs to collect points");
            Debug.Log("• Test the combat system with T key");
            Debug.Log("• Try different camera modes with C key");
            Debug.Log("• Use E to score points when you have them");
            Debug.Log("");
            Debug.Log("💡 TIP: Watch the debug UI for system status!");
            Debug.Log("════════════════════════════════════════");
        }
        
        [ContextMenu("Reset Demo")]
        public void ResetDemo()
        {
            // Clean up existing demo objects
            var demoObjects = GameObject.FindGameObjectsWithTag("Player");
            foreach (var obj in demoObjects)
            {
                DestroyImmediate(obj);
            }
            
            // Clean up environment
            var ground = GameObject.Find("Demo Ground");
            if (ground) DestroyImmediate(ground);
            
            // Reset state
            isSetupComplete = false;
            playerController = null;
            cameraController = null;
            
            Debug.Log("Demo reset - ready for new setup");
        }
        
        void OnGUI()
        {
            // Simple status display
            GUILayout.BeginArea(new Rect(Screen.width - 200, Screen.height - 120, 190, 110));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b>Demo Status</b>");
            GUILayout.Label($"Setup: {(isSetupComplete ? "✓" : "✗")}");
            GUILayout.Label($"Player: {(playerController ? "✓" : "✗")}");
            GUILayout.Label($"Camera: {(cameraController ? "✓" : "✗")}");
            
            if (GUILayout.Button("Setup Demo"))
            {
                SetupMasterDemo();
            }
            
            if (GUILayout.Button("Reset"))
            {
                ResetDemo();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
