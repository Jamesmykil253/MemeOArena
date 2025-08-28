using UnityEngine;
using MOBA.Controllers;

namespace MOBA.Demo
{
    /// <summary>
    /// UI display for camera system controls and status.
    /// Shows current camera mode, panning state, and control instructions.
    /// </summary>
    public class CameraControlsUI : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private bool showUI = true;
        [SerializeField] private Vector2 uiPosition = new Vector2(10, 10);
        [SerializeField] private float uiWidth = 300f;
        [SerializeField] private float uiHeight = 200f;
        
        private CameraController cameraController;
        private DemoPlayerController playerController;
        
        void Start()
        {
            // Find camera controller
            cameraController = FindFirstObjectByType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogWarning("CameraControlsUI: No CameraController found in scene");
            }
            
            // Find player controller for death state info
            playerController = FindFirstObjectByType<DemoPlayerController>();
        }
        
        void OnGUI()
        {
            if (!showUI) return;
            
            GUI.Box(new Rect(uiPosition.x, uiPosition.y, uiWidth, uiHeight), "Camera Controls");
            
            GUILayout.BeginArea(new Rect(uiPosition.x + 10, uiPosition.y + 25, uiWidth - 20, uiHeight - 35));
            
            // Camera status
            GUILayout.Label("<b>Camera Status:</b>");
            if (cameraController != null)
            {
                GUILayout.Label($"Mode: {cameraController.cameraMode}");
                GUILayout.Label($"Following: {(cameraController.followTarget ? "Yes" : "No")}");
                GUILayout.Label($"Target: {(cameraController.target ? cameraController.target.name : "None")}");
                
                // Check if we're in panning mode based on follow state
                bool isPanning = !cameraController.followTarget;
                GUILayout.Label($"Panning: {(isPanning ? "Active" : "Inactive")}");
            }
            
            // Player status
            GUILayout.Space(5);
            GUILayout.Label("<b>Player Status:</b>");
            if (playerController != null)
            {
                GUILayout.Label($"Player State: {(playerController.IsDead ? "Dead" : "Alive")}");
                GUILayout.Label($"Position: {playerController.transform.position:F1}");
            }
            
            // Controls
            GUILayout.Space(5);
            GUILayout.Label("<b>Controls:</b>");
            GUILayout.Label("WASD - Move Player");
            GUILayout.Label("V (Hold) - Pan Camera");
            GUILayout.Label("Arrow Keys - Camera Pan");
            GUILayout.Label("Right Mouse - Camera Drag");
            GUILayout.Label("Scroll - Zoom Camera");
            GUILayout.Label("C - Cycle Camera Modes");
            GUILayout.Label("K - Simulate Death/Respawn");
            
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
        public void SetCameraController(CameraController controller)
        {
            cameraController = controller;
        }
        
        /// <summary>
        /// Update player controller reference
        /// </summary>
        public void SetPlayerController(DemoPlayerController controller)
        {
            playerController = controller;
        }
    }
}
