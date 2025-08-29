using UnityEngine;
using MOBA.Controllers;

namespace MOBA.Demo
{
    /// <summary>
    /// UI display for camera system controls and status.
    /// Shows current camera mode, panning state, and control instructions.
    /// Supports UnifiedCameraController only.
    /// </summary>
    public class CameraControlsUI : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private bool showUI = true;
        [SerializeField] private Vector2 uiPosition = new Vector2(10, 10);
        [SerializeField] private float uiWidth = 300f;
        [SerializeField] private float uiHeight = 200f;
        
        private UnifiedCameraController unifiedCameraController;
        
        void Start()
        {
            // Find unified camera controller only
            unifiedCameraController = FindFirstObjectByType<UnifiedCameraController>();
            if (unifiedCameraController == null)
            {
                Debug.LogWarning("CameraControlsUI: No UnifiedCameraController found in scene");
            }
        }
        
        void OnGUI()
        {
            if (!showUI) return;
            
            GUI.Box(new Rect(uiPosition.x, uiPosition.y, uiWidth, uiHeight), "Camera Controls");
            
            GUILayout.BeginArea(new Rect(uiPosition.x + 10, uiPosition.y + 25, uiWidth - 20, uiHeight - 35));
            
            // Camera status
            GUILayout.Label("<b>Camera Status:</b>");
            if (unifiedCameraController != null)
            {
                GUILayout.Label($"System: Unified Camera");
                GUILayout.Label($"Mode: {unifiedCameraController.CurrentPresetName}");
                GUILayout.Label($"Following: {(unifiedCameraController.IsFollowing ? "Yes" : "No")}");
                GUILayout.Label($"Target: {(unifiedCameraController.target ? unifiedCameraController.target.name : "None")}");
                GUILayout.Label($"Panning: {(unifiedCameraController.IsPanning ? "Active" : "Inactive")}");
            }
            else
            {
                GUILayout.Label("No camera controller found");
            }
            
            // Player status
            GUILayout.Space(5);
            GUILayout.Label("<b>Player Status:</b>");
            GUILayout.Label("Player system simplified - no demo player controller");
            
            // Controls
            GUILayout.Space(5);
            GUILayout.Label("<b>Controls:</b>");
            GUILayout.Label("WASD - Move Player");
            GUILayout.Label("SPACE - Jump (all multipliers)");
            GUILayout.Label("V (Hold) - Pan Camera");
            GUILayout.Label("Arrow Keys - Camera Pan");
            GUILayout.Label("Right Mouse - Camera Drag");
            GUILayout.Label("Scroll - Zoom Camera");
            
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// Toggle UI visibility
        /// </summary>
        public void ToggleUI()
        {
            showUI = !showUI;
        }
        
        /// <summary>
        /// Set UI position
        /// </summary>
        public void SetUIPosition(Vector2 position)
        {
            uiPosition = position;
        }
        
        /// <summary>
        /// Update camera controller reference
        /// </summary>
        public void SetUnifiedCameraController(UnifiedCameraController controller)
        {
            unifiedCameraController = controller;
        }
    }
}
