using UnityEngine;
using MOBA.Controllers;

namespace MOBA.Demo
{
    /// <summary>
    /// Enhanced demo scene setup with comprehensive system initialization.
    /// Choose between simple setup or use ComprehensiveDemoSetup for full features.
    /// </summary>
    public class DemoSceneSetup : MonoBehaviour
    {
        [Header("Setup Options")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool useComprehensiveSetup = true;
        [SerializeField] private bool createVisualPlayer = true;
        
        void Start()
        {
            if (setupOnStart)
            {
                if (useComprehensiveSetup)
                {
                    SetupComprehensiveDemoInternal();
                }
                else
                {
                    SetupSimpleDemo();
                }
            }
        }
        
        [ContextMenu("Setup Simple Demo")]
        public void SetupSimpleDemo()
        {
            Debug.Log("DemoSceneSetup: Setting up simple demo scene...");
            
            // Find or create camera
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                GameObject camObject = new GameObject("Main Camera");
                mainCam = camObject.AddComponent<UnityEngine.Camera>();
                camObject.tag = "MainCamera";
                camObject.AddComponent<AudioListener>();
            }
            
            // Add camera controller
            CameraController cameraController = mainCam.GetComponent<CameraController>();
            if (cameraController == null)
            {
                cameraController = mainCam.gameObject.AddComponent<CameraController>();
            }
            
            // Find player or create demo player
            DemoPlayerController player = FindFirstObjectByType<DemoPlayerController>();
            if (player == null)
            {
                GameObject playerObject = new GameObject("Demo Player");
                player = playerObject.AddComponent<DemoPlayerController>();
                playerObject.transform.position = Vector3.zero;
                playerObject.tag = "Player";
                
                // Add visual representation if requested
                if (createVisualPlayer)
                {
                    GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    visual.transform.SetParent(playerObject.transform);
                    visual.transform.localPosition = Vector3.zero;
                    visual.name = "Visual";
                    
                    // Make it blue
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = Color.blue;
                    visual.GetComponent<Renderer>().material = mat;
                    
                    // Remove collider from visual (let parent handle collision)
                    DestroyImmediate(visual.GetComponent<CapsuleCollider>());
                }
            }
            
            // Connect camera to player
            cameraController.SetTarget(player.transform);
            cameraController.SetCameraMode(CameraController.CameraMode.ThirdPerson);
            
            Debug.Log("DemoSceneSetup: Simple demo ready - Camera connected to player!");
        }
        
        private void SetupComprehensiveDemoInternal()
        {
            Debug.Log("DemoSceneSetup: Setting up comprehensive demo...");
            
            // Setup full demo inline
            SetupFullDemo();
        }
        
        private void SetupFullDemo()
        {
            Debug.Log("Setting up full demo environment...");
            
            // Create environment
            CreateDemoEnvironment();
            
            // Setup player with visual
            SetupPlayerWithVisual();
            
            // Setup camera
            SetupEnhancedCamera();
            
            // Add UI
            AddDemoUI();
            
            Debug.Log("Full demo setup complete!");
        }
        
        private void CreateDemoEnvironment()
        {
            // Create ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(5f, 1f, 5f);
            ground.transform.position = new Vector3(0, -0.5f, 0);
            
            // Green material
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.2f, 0.8f, 0.2f);
            ground.GetComponent<Renderer>().material = groundMat;
            
            // Add some targets
            for (int i = 0; i < 4; i++)
            {
                float angle = (i / 4f) * 360f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 8f, 0.5f, Mathf.Sin(angle) * 8f);
                
                GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
                target.name = $"Target_{i + 1}";
                target.transform.position = pos;
                
                // Random color
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.HSVToRGB(i / 4f, 0.8f, 1f);
                target.GetComponent<Renderer>().material = mat;
            }
        }
        
        private void SetupPlayerWithVisual()
        {
            DemoPlayerController player = FindFirstObjectByType<DemoPlayerController>();
            if (player == null)
            {
                GameObject playerObj = new GameObject("Demo Player");
                player = playerObj.AddComponent<DemoPlayerController>();
                playerObj.transform.position = Vector3.zero;
                playerObj.tag = "Player";
                
                // Add visual
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.transform.SetParent(playerObj.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.name = "Visual";
                
                Material playerMat = new Material(Shader.Find("Standard"));
                playerMat.color = Color.blue;
                visual.GetComponent<Renderer>().material = playerMat;
                
                DestroyImmediate(visual.GetComponent<CapsuleCollider>());
            }
        }
        
        private void SetupEnhancedCamera()
        {
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }
            
            CameraController cameraController = mainCam.GetComponent<CameraController>();
            if (cameraController == null)
            {
                cameraController = mainCam.gameObject.AddComponent<CameraController>();
            }
            
            DemoPlayerController player = FindFirstObjectByType<DemoPlayerController>();
            if (player != null)
            {
                cameraController.SetTarget(player.transform);
                cameraController.SetCameraMode(CameraController.CameraMode.ThirdPerson);
                cameraController.followSpeed = 8f;
            }
        }
        
        private void AddDemoUI()
        {
            // Add camera controls UI
            GameObject uiObj = new GameObject("Camera Controls UI");
            uiObj.AddComponent<CameraControlsUI>();
        }
        
        [ContextMenu("Setup Comprehensive Demo")]
        public void SetupComprehensiveDemo()
        {
            SetupComprehensiveDemoInternal();
        }
    }
}
