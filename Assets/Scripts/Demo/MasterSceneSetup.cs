using UnityEngine;
using MOBA.Controllers;

namespace MOBA.Demo
{
    /// <summary>
    /// Master scene setup that ensures everything works correctly.
    /// This component diagnoses and fixes common setup issues.
    /// </summary>
    public class MasterSceneSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool diagnosticMode = true;
        
        [Header("Components to Setup")]
        [SerializeField] private bool ensurePlayer = true;
        [SerializeField] private bool ensureCamera = true;
        [SerializeField] private bool ensureEnvironment = true;
        [SerializeField] private bool addDebugUI = true;
        [SerializeField] private bool ensureAssetManager = true;
        
        void Start()
        {
            if (setupOnStart)
            {
                StartCoroutine(SetupWithDiagnostics());
            }
        }
        
        private System.Collections.IEnumerator SetupWithDiagnostics()
        {
            // Wait for all Awake calls
            yield return null;
            
            RunSetupDiagnostics();
        }
        
        [ContextMenu("Run Setup Diagnostics")]
        public void RunSetupDiagnostics()
        {
            Debug.Log("🔍 Running MemeOArena Setup Diagnostics...");
            
            // Check asset manager first
            if (ensureAssetManager)
            {
                CheckAssetManager();
            }
            
            // Check each system
            CheckPlayer();
            CheckCamera();
            CheckEnvironment();
            CheckInput();
            
            if (addDebugUI)
            {
                AddDebugComponents();
            }
            
            Debug.Log("✅ Setup diagnostics complete!");
        }
        
        private void CheckAssetManager()
        {
            var assetManager = FindFirstObjectByType<DemoAssetManager>();
            
            if (assetManager == null)
            {
                Debug.Log("❌ No DemoAssetManager found - creating one");
                
                GameObject assetObj = new GameObject("Demo Asset Manager");
                assetManager = assetObj.AddComponent<DemoAssetManager>();
                
                // Force create materials immediately
                assetManager.CreateAllMaterials();
            }
            else
            {
                Debug.Log("✅ DemoAssetManager found");
                // Ensure materials are created
                assetManager.CreateAllMaterials();
            }
        }
        
        private void CheckPlayer()
        {
            var player = FindFirstObjectByType<DemoPlayerController>();
            
            if (player == null && ensurePlayer)
            {
                Debug.Log("❌ No player found - adding PlayerSpawnDebugger");
                
                // Add player spawn debugger
                GameObject debugObj = new GameObject("Player Spawn Debugger");
                var spawner = debugObj.AddComponent<PlayerSpawnDebugger>();
                spawner.SpawnPlayerIfNeeded();
            }
            else if (player != null)
            {
                Debug.Log($"✅ Player found: {player.name}");
                
                // Check if player has visual
                var visual = player.transform.GetChild(0);
                if (visual == null)
                {
                    Debug.Log("⚠️ Player has no visual - consider adding one");
                }
            }
        }
        
        private void CheckCamera()
        {
            var mainCam = UnityEngine.Camera.main;
            
            if (mainCam == null && ensureCamera)
            {
                Debug.Log("❌ No main camera - creating one");
                
                GameObject camObj = new GameObject("Main Camera");
                var cam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
                
                mainCam = cam;
            }
            
            if (mainCam != null)
            {
                var cameraController = mainCam.GetComponent<CameraController>();
                if (cameraController == null)
                {
                    Debug.Log("⚠️ Camera missing CameraController - adding one");
                    cameraController = mainCam.gameObject.AddComponent<CameraController>();
                }
                
                // Check if camera has target
                if (cameraController.target == null)
                {
                    var player = FindFirstObjectByType<DemoPlayerController>();
                    if (player != null)
                    {
                        Debug.Log("🔧 Connecting camera to player");
                        cameraController.SetTarget(player.transform);
                        cameraController.followTarget = true;
                    }
                }
                
                Debug.Log($"✅ Camera setup: Target={cameraController.target?.name ?? "None"}, Mode={cameraController.cameraMode}");
            }
        }
        
        private void CheckEnvironment()
        {
            if (!ensureEnvironment) return;
            
            // Check for ground
            var ground = GameObject.Find("Ground") ?? GameObject.Find("Demo Ground");
            if (ground == null)
            {
                Debug.Log("⚠️ No ground plane found - consider adding environment");
                
                // Create simple ground
                GameObject groundObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                groundObj.name = "Demo Ground";
                groundObj.transform.localScale = new Vector3(5f, 1f, 5f);
                groundObj.transform.position = new Vector3(0f, -0.5f, 0f);
                
                // Green material
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.3f, 0.7f, 0.3f);
                groundObj.GetComponent<Renderer>().material = mat;
                
                Debug.Log("✅ Created basic ground plane");
            }
            else
            {
                Debug.Log($"✅ Environment found: {ground.name}");
            }
        }
        
        private void CheckInput()
        {
            Debug.Log("🎮 Input System Status:");
            
            // Check if input managers are present
            var inputManager = FindFirstObjectByType<MOBA.Input.InputManager>();
            if (inputManager != null)
            {
                Debug.Log("✅ Input Manager detected");
            }
            else
            {
                Debug.Log("⚠️ No Input Manager found");
            }
            
            // Test basic input
            if (UnityEngine.Input.GetKey(KeyCode.W))
            {
                Debug.Log("✅ Input responsive (W key detected)");
            }
        }
        
        private void AddDebugComponents()
        {
            // Add player spawn debugger if not present
            if (FindFirstObjectByType<PlayerSpawnDebugger>() == null)
            {
                GameObject debugObj = new GameObject("Player Spawn Debugger");
                debugObj.AddComponent<PlayerSpawnDebugger>();
            }
            
            // Add camera controls UI if not present
            if (FindFirstObjectByType<CameraControlsUI>() == null)
            {
                GameObject uiObj = new GameObject("Camera Controls UI");
                uiObj.AddComponent<CameraControlsUI>();
            }
            
            // Add instructions UI if not present
            if (FindFirstObjectByType<DemoInstructionsUI>() == null)
            {
                GameObject instructionsObj = new GameObject("Demo Instructions UI");
                instructionsObj.AddComponent<DemoInstructionsUI>();
            }
            
            Debug.Log("✅ Debug UI components added");
        }
        
        void OnGUI()
        {
            if (!diagnosticMode) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b>Scene Diagnostics</b>");
            
            // System status
            var player = FindFirstObjectByType<DemoPlayerController>();
            var camera = UnityEngine.Camera.main?.GetComponent<CameraController>();
            
            GUILayout.Label($"Player: {(player ? "✓" : "✗")}");
            GUILayout.Label($"Camera: {(camera ? "✓" : "✗")}");
            GUILayout.Label($"Target: {(camera?.target ? "✓" : "✗")}");
            GUILayout.Label($"Follow: {(camera?.followTarget == true ? "✓" : "✗")}");
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Run Diagnostics"))
            {
                RunSetupDiagnostics();
            }
            
            if (camera && !camera.followTarget && GUILayout.Button("Fix Camera"))
            {
                camera.followTarget = true;
                camera.EnableFollowOnRespawn();
                Debug.Log("Camera fixed - now following player");
            }
            
            if (GUILayout.Button("Reset Scene"))
            {
                // Clean up and restart
                var players = FindObjectsByType<DemoPlayerController>(FindObjectsSortMode.None);
                foreach (var p in players)
                {
                    DestroyImmediate(p.gameObject);
                }
                
                RunSetupDiagnostics();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
