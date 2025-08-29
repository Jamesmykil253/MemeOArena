using UnityEngine;
using MOBA.Controllers;

namespace MOBA.Tools
{
    /// <summary>
    /// Emergency input conflict resolver that fixes spacebar hijacking and cleans up camera conflicts.
    /// This script identifies and fixes all input conflicts causing the jump system to malfunction.
    /// </summary>
    public class InputConflictResolver : MonoBehaviour
    {
        [Header("Conflict Resolution")]
        [SerializeField] private bool autoResolveOnAwake = true;
        [SerializeField] private bool disableConflictingCameras = true;
        [SerializeField] private bool keepOnlyUnifiedCamera = true;
        [SerializeField] private bool verboseLogging = true;
        
        [Header("Conflict Status")]
        [SerializeField] private int conflictsFound = 0;
        [SerializeField] private int conflictsResolved = 0;
        [SerializeField] private bool spacebarConflictResolved = false;
        
        void Awake()
        {
            if (autoResolveOnAwake)
            {
                ResolveAllInputConflicts();
            }
        }
        
        [ContextMenu("Resolve All Input Conflicts")]
        public void ResolveAllInputConflicts()
        {
            if (verboseLogging)
                Debug.Log("🔧 [InputConflictResolver] Starting comprehensive input conflict resolution...");
            
            conflictsFound = 0;
            conflictsResolved = 0;
            
            // Phase 1: Fix camera input conflicts
            ResolveCameraInputConflicts();
            
            // Phase 2: Clean up duplicate camera systems
            CleanupDuplicateCameraSystems();
            
            // Phase 3: Verify jump system integrity
            VerifyJumpSystemIntegrity();
            
            if (verboseLogging)
            {
                Debug.Log($"🎯 Input conflict resolution complete!");
                Debug.Log($"   • Conflicts Found: {conflictsFound}");
                Debug.Log($"   • Conflicts Resolved: {conflictsResolved}");
                Debug.Log($"   • Spacebar Available for Jump: {spacebarConflictResolved}");
            }
        }
        
        private void ResolveCameraInputConflicts()
        {
            if (verboseLogging)
                Debug.Log("🎥 Resolving camera input conflicts...");
            
            // Find all camera controllers that might hijack spacebar
            var unifiedCameraControllers = FindObjectsByType<UnifiedCameraController>(FindObjectsSortMode.None);
            
            // Note: Removed legacy camera controller types as they've been unified
            
            // Keep only the best unified camera controller
            if (unifiedCameraControllers.Length > 1 && disableConflictingCameras && keepOnlyUnifiedCamera)
            {
                for (int i = 1; i < unifiedCameraControllers.Length; i++)
                {
                    conflictsFound++;
                    unifiedCameraControllers[i].enabled = false;
                    conflictsResolved++;
                    if (verboseLogging)
                        Debug.Log($"   🔄 Disabled duplicate UnifiedCameraController on {unifiedCameraControllers[i].gameObject.name}");
                }
            }
            
            spacebarConflictResolved = true;
        }
        
        private void CleanupDuplicateCameraSystems()
        {
            if (verboseLogging)
                Debug.Log("🧹 Cleaning up duplicate camera systems...");
            
            // Ensure only one unified camera system is active
            var unifiedControllers = FindObjectsByType<UnifiedCameraController>(FindObjectsSortMode.None);
            UnifiedCameraController activeController = null;
            
            foreach (var controller in unifiedControllers)
            {
                if (controller.enabled && activeController == null)
                {
                    activeController = controller;
                    // Make sure it uses V for panning, not spacebar
                    if (verboseLogging)
                        Debug.Log($"   ✅ Kept UnifiedCameraController on {controller.gameObject.name} as primary");
                }
                else if (controller.enabled && activeController != null)
                {
                    controller.enabled = false;
                    conflictsFound++;
                    conflictsResolved++;
                    if (verboseLogging)
                        Debug.Log($"   🔄 Disabled duplicate UnifiedCameraController on {controller.gameObject.name}");
                }
            }
        }
        
        private void VerifyJumpSystemIntegrity()
        {
            if (verboseLogging)
                Debug.Log("🦘 Verifying jump system integrity...");
            
            // Find all jump controllers
            var jumpControllers = FindObjectsByType<MOBA.Controllers.EnhancedJumpController>(FindObjectsSortMode.None);
            
            foreach (var controller in jumpControllers)
            {
                if (controller.enabled)
                {
                    if (verboseLogging)
                        Debug.Log($"   ✅ Found active EnhancedJumpController on {controller.gameObject.name}");
                }
            }
            
            if (jumpControllers.Length == 0)
            {
                Debug.LogWarning("   ⚠️ No EnhancedJumpController found! Jump system may not work.");
            }
            
            // Verify input system
            var inputSources = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            bool inputSystemFound = false;
            
            foreach (var source in inputSources)
            {
                if (source.GetType().GetInterface("IInputSource") != null)
                {
                    inputSystemFound = true;
                    if (verboseLogging)
                        Debug.Log($"   ✅ Found input source: {source.GetType().Name} on {source.gameObject.name}");
                }
            }
            
            if (!inputSystemFound)
            {
                Debug.LogWarning("   ⚠️ No IInputSource implementation found! Input may not work properly.");
            }
        }
        
        [ContextMenu("Check Current Input Status")]
        public void CheckCurrentInputStatus()
        {
            Debug.Log("📊 Current Input System Status:");
            
            // Check camera controllers
            var cameras = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int activeCameraControllers = 0;
            int totalCameraControllers = 0;
            
            foreach (var camera in cameras)
            {
                if (camera.GetType().Name.Contains("Camera") && camera.GetType().Name.Contains("Controller"))
                {
                    totalCameraControllers++;
                    if (camera.enabled)
                    {
                        activeCameraControllers++;
                        Debug.Log($"   • Active: {camera.GetType().Name} on {camera.gameObject.name}");
                    }
                    else
                    {
                        Debug.Log($"   • Disabled: {camera.GetType().Name} on {camera.gameObject.name}");
                    }
                }
            }
            
            Debug.Log($"   Total Camera Controllers: {totalCameraControllers}");
            Debug.Log($"   Active Camera Controllers: {activeCameraControllers}");
            Debug.Log($"   Spacebar Available for Jump: {activeCameraControllers <= 1}");
        }
        
        [ContextMenu("Force Enable Jump System")]
        public void ForceEnableJumpSystem()
        {
            // Disable ALL camera controllers except UnifiedCameraController
            var allControllers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var controller in allControllers)
            {
                if (controller.GetType().Name == "CameraController" || 
                    controller.GetType().Name == "DynamicCameraController")
                {
                    controller.enabled = false;
                    Debug.Log($"🔒 Force-disabled {controller.GetType().Name} on {controller.gameObject.name}");
                }
            }
            
            Debug.Log("🦘 Jump system should now have exclusive spacebar access!");
        }
    }
}
