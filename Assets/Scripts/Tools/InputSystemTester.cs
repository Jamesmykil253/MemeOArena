using UnityEngine;
using MOBA.Controllers;

namespace MOBA.Tools
{
    /// <summary>
    /// Simple test script to verify that the spacebar input conflict has been resolved
    /// and the jump system is working correctly.
    /// </summary>
    public class InputSystemTester : MonoBehaviour
    {
        [Header("Test Results")]
        [SerializeField] private bool spacebarAvailableForJump = false;
        [SerializeField] private bool jumpSystemResponding = false;
        [SerializeField] private int cameraControllersActive = 0;
        [SerializeField] private string currentInputStatus = "Unknown";
        
        void Update()
        {
            TestSpacebarInput();
            CheckCameraControllers();
        }
        
        private void TestSpacebarInput()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                bool spacePressed = UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;
                
                if (spacePressed)
                {
                    Debug.Log("🔧 [InputSystemTester] Spacebar pressed - checking for conflicts...");
                    
                    // Check if any camera controllers are intercepting spacebar
                    var cameras = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                    bool conflictDetected = false;
                    
                    foreach (var cam in cameras)
                    {
                        if (cam.enabled && cam is UnifiedCameraController)
                        {
                            Debug.LogWarning($"   ⚠️ Active camera controller detected: {cam.GetType().Name} on {cam.gameObject.name}");
                            conflictDetected = true;
                        }
                    }
                    
                    if (!conflictDetected)
                    {
                        Debug.Log("   ✅ No camera conflicts detected - spacebar should work for jumping!");
                        spacebarAvailableForJump = true;
                    }
                    else
                    {
                        Debug.LogError("   ❌ Camera controllers still active - spacebar may be hijacked!");
                        spacebarAvailableForJump = false;
                    }
                    
                    // Test jump system response
                    TestJumpSystemResponse();
                }
            }
        }
        
        private void TestJumpSystemResponse()
        {
            var jumpControllers = FindObjectsByType<EnhancedJumpController>(FindObjectsSortMode.None);
            jumpSystemResponding = false;
            
            foreach (var jumpController in jumpControllers)
            {
                if (jumpController.enabled)
                {
                    Debug.Log($"   🦘 Found active jump controller on {jumpController.gameObject.name}");
                    jumpSystemResponding = true;
                }
            }
            
            if (!jumpSystemResponding)
            {
                Debug.LogWarning("   ⚠️ No active jump controllers found!");
            }
        }
        
        private void CheckCameraControllers()
        {
            cameraControllersActive = 0;
            var cameras = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var cam in cameras)
            {
                if (cam.enabled && cam.GetType().Name.Contains("Camera") && cam.GetType().Name.Contains("Controller"))
                {
                    cameraControllersActive++;
                }
            }
            
            // Update status
            if (cameraControllersActive == 0)
            {
                currentInputStatus = "✅ No camera conflicts";
            }
            else if (cameraControllersActive == 1)
            {
                currentInputStatus = "⚠️ One camera active - check V key binding";
            }
            else
            {
                currentInputStatus = $"❌ {cameraControllersActive} camera controllers active - conflict likely";
            }
        }
        
        void OnGUI()
        {
            GUI.Box(new Rect(10f, 10f, 400f, 120f), "Input System Test Status");
            
            GUI.Label(new Rect(20f, 35f, 380f, 20f), $"Spacebar Available: {(spacebarAvailableForJump ? "✅ YES" : "❌ NO")}");
            GUI.Label(new Rect(20f, 55f, 380f, 20f), $"Jump System: {(jumpSystemResponding ? "✅ ACTIVE" : "❌ INACTIVE")}");
            GUI.Label(new Rect(20f, 75f, 380f, 20f), $"Camera Controllers: {cameraControllersActive}");
            GUI.Label(new Rect(20f, 95f, 380f, 20f), $"Status: {currentInputStatus}");
            
            if (GUI.Button(new Rect(20, 135, 150, 25), "Test Spacebar"))
            {
                Debug.Log("🔧 Manual spacebar test triggered");
                TestSpacebarInput();
            }
            
            if (GUI.Button(new Rect(180, 135, 150, 25), "Disable All Cameras"))
            {
                var cameras = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var cam in cameras)
                {
                    if (cam.GetType().Name == "CameraController" || cam.GetType().Name == "DynamicCameraController")
                    {
                        cam.enabled = false;
                        Debug.Log($"🔒 Disabled {cam.GetType().Name} on {cam.gameObject.name}");
                    }
                }
            }
        }
        
        [ContextMenu("Run Full Input Test")]
        public void RunFullInputTest()
        {
            Debug.Log("🧪 Running comprehensive input system test...");
            
            CheckCameraControllers();
            TestJumpSystemResponse();
            
            Debug.Log("📊 Test Results:");
            Debug.Log($"   • Camera Controllers Active: {cameraControllersActive}");
            Debug.Log($"   • Jump System Responding: {jumpSystemResponding}");
            Debug.Log($"   • Current Status: {currentInputStatus}");
            
            if (cameraControllersActive > 1)
            {
                Debug.LogError("🚨 Multiple camera controllers detected - input conflicts likely!");
            }
            else if (cameraControllersActive == 0 && jumpSystemResponding)
            {
                Debug.Log("🎉 Perfect! No camera conflicts and jump system is active!");
            }
        }
    }
}
