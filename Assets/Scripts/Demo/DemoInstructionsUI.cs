using UnityEngine;
using UnityEngine.InputSystem;

namespace MOBA.Demo
{
    /// <summary>
    /// Displays helpful instructions and tips for the demo
    /// </summary>
    public class DemoInstructionsUI : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private bool showInstructions = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        [SerializeField] private Rect instructionsRect = new Rect(10, 10, 300, 400);
        
        private bool showUI = true;
        
        void Update()
        {
            // Toggle UI display - NEW INPUT SYSTEM
            if (Keyboard.current != null && Keyboard.current[Key.F1].wasPressedThisFrame)
            {
                showUI = !showUI;
            }
        }
        
        void OnGUI()
        {
            if (!showInstructions || !showUI) return;
            
            // Semi-transparent background
            GUI.color = new Color(0f, 0f, 0f, 0.8f);
            GUI.Box(instructionsRect, "");
            GUI.color = Color.white;
            
            GUILayout.BeginArea(instructionsRect);
            GUILayout.BeginVertical();
            
            GUILayout.Label("<size=16><b>🎮 MemeOArena Demo</b></size>");
            GUILayout.Space(10);
            
            GUILayout.Label("<b>Movement:</b>");
            GUILayout.Label("• WASD - Move player");
            GUILayout.Label("• Mouse - Look around");
            GUILayout.Space(5);
            
            GUILayout.Label("<b>Camera:</b>");
            GUILayout.Label("• C - Cycle modes");
            GUILayout.Label("• V - Toggle follow/free");
            GUILayout.Label("• Scroll - Zoom (free mode)");
            GUILayout.Space(5);
            
            GUILayout.Label("<b>Testing:</b>");
            GUILayout.Label("• P - Add points");
            GUILayout.Label("• T - Take damage");
            GUILayout.Label("• E - Score points");
            GUILayout.Space(5);
            
            GUILayout.Label("<b>Game:</b>");
            GUILayout.Label("• Walk over golden orbs");
            GUILayout.Label("• Watch debug info");
            GUILayout.Space(10);
            
            GUILayout.Label($"<i>Press {toggleKey} to toggle this</i>");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
