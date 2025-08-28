using UnityEngine;
using MOBA.Controllers;
using MOBA.Data;
using MOBA.Core;

namespace MOBA.Demo
{
    /// <summary>
    /// Comprehensive demo setup that creates a full testing environment
    /// showcasing all the MOBA systems working together.
    /// </summary>
    public class ComprehensiveDemoSetup : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool createEnvironment = true;
        [SerializeField] private bool showInstructions = true;
        
        [Header("Player Settings")]
        [SerializeField] private Vector3 playerSpawnPosition = Vector3.zero;
        [SerializeField] private Material playerMaterial;
        
        [Header("Environment Settings")]
        [SerializeField] private Material groundMaterial;
        [SerializeField] private Material obstacleMaterial;
        [SerializeField] private Vector2 groundSize = new Vector2(50f, 50f);
        
        // System references
        private DemoPlayerController player;
        private CameraController cameraController;
        private TickManager tickManager;
        private GameObject environment;
        
        void Start()
        {
            if (setupOnStart)
            {
                SetupCompleteDemo();
            }
        }
        
        [ContextMenu("Setup Complete Demo")]
        public void SetupCompleteDemo()
        {
            Debug.Log("=== Setting up Comprehensive MOBA Demo ===");
            
            // Step 1: Create core managers
            CreateTickManager();
            
            // Step 2: Create environment
            if (createEnvironment)
            {
                CreateDemoEnvironment();
            }
            
            // Step 3: Create player
            CreateDemoPlayer();
            
            // Step 4: Setup camera system
            SetupCameraSystem();
            
            // Step 5: Add UI systems
            AddUIComponents();
            
            // Step 6: Final setup
            FinalizeDemo();
            
            Debug.Log("=== Demo Setup Complete! ===");
            
            if (showInstructions)
            {
                ShowDemoInstructions();
            }
        }
        
        private void CreateTickManager()
        {
            tickManager = FindFirstObjectByType<TickManager>();
            if (tickManager == null)
            {
                GameObject tickObj = new GameObject("TickManager");
                tickManager = tickObj.AddComponent<TickManager>();
                Debug.Log("✓ Created TickManager for deterministic simulation");
            }
        }
        
        private void CreateDemoEnvironment()
        {
            // Create parent for environment
            environment = new GameObject("Demo Environment");
            environment.transform.SetParent(transform);
            
            // Create ground plane
            CreateGroundPlane();
            
            // Create some obstacles/targets
            CreateDemoObstacles();
            
            // Add some lighting
            SetupLighting();
            
            Debug.Log("✓ Created demo environment");
        }
        
        private void CreateGroundPlane()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(environment.transform);
            ground.transform.localScale = new Vector3(groundSize.x / 10f, 1f, groundSize.y / 10f);
            ground.transform.position = new Vector3(0, -0.5f, 0);
            
            if (groundMaterial != null)
            {
                ground.GetComponent<Renderer>().material = groundMaterial;
            }
            else
            {
                // Create a basic green material
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.8f, 0.2f);
                ground.GetComponent<Renderer>().material = mat;
            }
        }
        
        private void CreateDemoObstacles()
        {
            // Create some cubes as obstacles/targets
            Vector3[] positions = {
                new Vector3(10f, 0.5f, 10f),
                new Vector3(-10f, 0.5f, 10f),
                new Vector3(10f, 0.5f, -10f),
                new Vector3(-10f, 0.5f, -10f),
                new Vector3(0f, 0.5f, 15f),
                new Vector3(0f, 0.5f, -15f)
            };
            
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = $"Target_{i + 1}";
                obstacle.transform.SetParent(environment.transform);
                obstacle.transform.position = positions[i];
                obstacle.transform.localScale = Vector3.one * 1.5f;
                
                // Add a simple color
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.HSVToRGB((float)i / positions.Length, 0.8f, 1f);
                obstacle.GetComponent<Renderer>().material = mat;
                
                // Add a simple interaction component
                obstacle.AddComponent<DemoTarget>();
            }
        }
        
        private void SetupLighting()
        {
            // Find or create directional light
            Light mainLight = FindFirstObjectByType<Light>();
            if (mainLight == null)
            {
                GameObject lightObj = new GameObject("Main Light");
                lightObj.transform.SetParent(environment.transform);
                mainLight = lightObj.AddComponent<Light>();
            }
            
            mainLight.type = LightType.Directional;
            mainLight.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
            mainLight.intensity = 1.2f;
            mainLight.shadows = LightShadows.Soft;
        }
        
        private void CreateDemoPlayer()
        {
            // Find existing player or create new one
            player = FindFirstObjectByType<DemoPlayerController>();
            
            if (player == null)
            {
                GameObject playerObj = new GameObject("Demo Player");
                playerObj.transform.position = playerSpawnPosition;
                
                // Add visual representation
                GameObject visualMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visualMesh.transform.SetParent(playerObj.transform);
                visualMesh.transform.localPosition = Vector3.zero;
                visualMesh.name = "Visual Mesh";
                
                // Set material
                if (playerMaterial != null)
                {
                    visualMesh.GetComponent<Renderer>().material = playerMaterial;
                }
                else
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = Color.blue;
                    visualMesh.GetComponent<Renderer>().material = mat;
                }
                
                // Remove collider from visual mesh (we'll use it for the main object if needed)
                DestroyImmediate(visualMesh.GetComponent<CapsuleCollider>());
                
                // Add the demo player controller
                player = playerObj.AddComponent<DemoPlayerController>();
                
                // Tag as player for camera system
                playerObj.tag = "Player";
                
                Debug.Log("✓ Created Demo Player with visual representation");
            }
        }
        
        private void SetupCameraSystem()
        {
            // Find main camera
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
                
                // Add audio listener
                camObj.AddComponent<AudioListener>();
            }
            
            // Add camera controller
            cameraController = mainCam.GetComponent<CameraController>();
            if (cameraController == null)
            {
                cameraController = mainCam.gameObject.AddComponent<CameraController>();
            }
            
            // Configure camera
            cameraController.SetTarget(player.transform);
            cameraController.SetCameraMode(CameraController.CameraMode.ThirdPerson);
            cameraController.followSpeed = 8f;
            cameraController.rotationSpeed = 5f;
            
            // Position camera for good initial view
            mainCam.transform.position = playerSpawnPosition + new Vector3(0f, 8f, -6f);
            
            Debug.Log("✓ Setup camera system with dynamic following");
        }
        
        private void AddUIComponents()
        {
            // Find or create UI Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Demo UI Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            // Add camera controls UI
            GameObject cameraUIObj = new GameObject("Camera Controls UI");
            cameraUIObj.transform.SetParent(canvas.transform);
            cameraUIObj.AddComponent<CameraControlsUI>();
            
            Debug.Log("✓ Added UI components");
        }
        
        private void FinalizeDemo()
        {
            // Ensure player is properly initialized
            if (player != null && player.enabled)
            {
                // Player will initialize itself in Awake/Start
                Debug.Log("✓ Player systems initializing...");
            }
            
            // Set up some demo objects in the scene
            CreateDemoInteractables();
            
            Debug.Log("✓ Demo finalization complete");
        }
        
        private void CreateDemoInteractables()
        {
            // Create some point pickup objects
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-8f, 8f),
                    1f,
                    Random.Range(-8f, 8f)
                );
                
                GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pickup.name = $"Point Pickup {i + 1}";
                pickup.transform.position = pos;
                pickup.transform.localScale = Vector3.one * 0.7f;
                pickup.transform.SetParent(environment.transform);
                
                // Make it golden
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.yellow;
                mat.SetFloat("_Metallic", 0.8f);
                pickup.GetComponent<Renderer>().material = mat;
                
                // Add pickup behavior
                pickup.AddComponent<DemoPointPickup>();
                
                // Make it spin
                pickup.AddComponent<DemoSpinner>();
            }
        }
        
        private void ShowDemoInstructions()
        {
            Debug.Log("=== DEMO INSTRUCTIONS ===");
            Debug.Log("MOVEMENT:");
            Debug.Log("• WASD - Move player around");
            Debug.Log("• Mouse - Look around (when in free pan)");
            Debug.Log("");
            Debug.Log("CAMERA:");
            Debug.Log("• C - Cycle camera modes (TopDown/ThirdPerson/Isometric/Action)");
            Debug.Log("• V - Toggle follow/free pan mode");
            Debug.Log("• Scroll - Zoom (in free pan mode)");
            Debug.Log("");
            Debug.Log("TESTING:");
            Debug.Log("• P - Add random points (1-7)");
            Debug.Log("• T - Take random damage");
            Debug.Log("• E - Start/stop scoring channel");
            Debug.Log("• Q - Basic ability (when implemented)");
            Debug.Log("• R - Ultimate ability (when energy ready)");
            Debug.Log("");
            Debug.Log("Walk over the golden spheres to collect points!");
            Debug.Log("Check the debug UI for system status.");
            Debug.Log("=========================");
        }
        
        [ContextMenu("Clean Demo")]
        public void CleanDemo()
        {
            if (environment != null)
            {
                DestroyImmediate(environment);
            }
            
            if (player != null)
            {
                DestroyImmediate(player.gameObject);
            }
            
            Debug.Log("Demo cleaned up");
        }
        
        void OnGUI()
        {
            if (!showInstructions) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 150));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b>Demo Controls</b>");
            GUILayout.Space(5);
            
            if (GUILayout.Button("Setup Demo"))
            {
                SetupCompleteDemo();
            }
            
            if (GUILayout.Button("Clean Demo"))
            {
                CleanDemo();
            }
            
            if (GUILayout.Button("Show Instructions"))
            {
                ShowDemoInstructions();
            }
            
            GUILayout.Space(5);
            GUILayout.Label($"Systems Status:");
            GUILayout.Label($"Player: {(player ? "✓" : "✗")}");
            GUILayout.Label($"Camera: {(cameraController ? "✓" : "✗")}");
            GUILayout.Label($"Environment: {(environment ? "✓" : "✗")}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
