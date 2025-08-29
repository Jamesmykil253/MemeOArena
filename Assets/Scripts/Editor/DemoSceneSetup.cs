using UnityEngine;
using MOBA.Demo;

namespace MOBA.Editor
{
    /// <summary>
    /// Simple editor script to setup demo scene with enhanced jump system.
    /// Run this from the Unity menu: Tools/Setup Demo Scene
    /// </summary>
    public class DemoSceneSetup
    {
        [UnityEditor.MenuItem("Tools/Setup Demo Scene")]
        public static void SetupDemoScene()
        {
            // Find or create demo player
            GameObject demoPlayer = GameObject.FindFirstObjectByType<DemoPlayerController>()?.gameObject;
            
            if (demoPlayer == null)
            {
                demoPlayer = new GameObject("DemoPlayer");
                demoPlayer.AddComponent<DemoPlayerController>();
                Debug.Log("Created DemoPlayer GameObject");
            }
            
            // Add test component (using reflection to avoid type reference issues)
            var existingTest = demoPlayer.GetComponent("JumpSystemTest");
            if (existingTest == null)
            {
                var jumpTestType = System.Type.GetType("MOBA.Demo.JumpSystemTest, Assembly-CSharp");
                if (jumpTestType != null)
                {
                    demoPlayer.AddComponent(jumpTestType);
                    Debug.Log("Added JumpSystemTest component");
                }
                else
                {
                    Debug.LogWarning("JumpSystemTest component not found - test component will not be added");
                }
            }
            
            // Ensure position is reasonable
            demoPlayer.transform.position = Vector3.zero;
            
            // Select the demo player for easy inspector access
            UnityEditor.Selection.activeGameObject = demoPlayer;
            
            Debug.Log("Demo scene setup complete! Press Play to test the enhanced jump system.");
            Debug.Log("Controls: WASD = Move, Space = Jump (hold for higher jump, double-tap for double jump)");
        }
    }
}
