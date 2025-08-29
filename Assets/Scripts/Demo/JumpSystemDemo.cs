using UnityEngine;
using MOBA.Controllers;
using MOBA.Data;
using MOBA.Input;
using MOBA.Core;
using MOBA.Core.Logging;

namespace MOBA.Demo
{
    /// <summary>
    /// Demo script showcasing the Enhanced Jump System with variable heights and apex boost.
    /// Features: Normal jump (1x), High jump (1.5x), Double jump (2x), Apex boost mechanics.
    /// </summary>
    public class JumpSystemDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool enableDemo = true;
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private KeyCode resetDemoKey = KeyCode.R;
        
        [Header("Jump System Components")]
        [SerializeField] private EnhancedJumpController jumpController;
        [SerializeField] private PhysicsLocomotionController locomotionController;
        [SerializeField] private JumpPhysicsDef jumpPhysicsDef;
        
        [Header("Demo Objects")]
        [SerializeField] private Transform[] jumpPlatforms;
        [SerializeField] private Material[] jumpTypeMaterials; // Normal, High, Double, ApexBoosted
        [SerializeField] private ParticleSystem jumpEffect;
        [SerializeField] private AudioSource jumpAudioSource;
        [SerializeField] private AudioClip[] jumpSounds; // Different sounds for different jump types
        
        // Demo state
        private EnhancedJumpController.JumpStatistics lastStats;
        private Vector3 initialPosition;
        private float demoStartTime;
        private int demonstrationPhase = 0;
        
        // UI state
        private Rect windowRect = new Rect(20, 20, 350, 400);
        private bool showAdvancedStats = false;
        
        private void Start()
        {
            if (!enableDemo) return;
            
            InitializeDemo();
            SetupEventHandlers();
            
            demoStartTime = Time.time;
            initialPosition = transform.position;
            
            EnterpriseLogger.LogInfo("DEMO", "JumpSystem", "Jump System Demo initialized");
        }
        
        private void InitializeDemo()
        {
            // Auto-find components if not assigned
            if (jumpController == null)
                jumpController = GetComponent<EnhancedJumpController>();
            
            if (locomotionController == null)
                locomotionController = GetComponent<PhysicsLocomotionController>();
            
            // Create default jump physics definition if none exists
            if (jumpPhysicsDef == null)
            {
                jumpPhysicsDef = ScriptableObject.CreateInstance<JumpPhysicsDef>();
                SetupDemoJumpPhysics();
            }
            
            // Assign to jump controller if available
            if (jumpController != null)
            {
                var jumpDefField = typeof(EnhancedJumpController).GetField("jumpDef", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                jumpDefField?.SetValue(jumpController, jumpPhysicsDef);
            }
        }
        
        private void SetupDemoJumpPhysics()
        {
            if (jumpPhysicsDef == null) return;
            
            // Configure jump physics for demo (using reflection to access private fields)
            var defType = typeof(JumpPhysicsDef);
            
            SetPrivateField(defType, jumpPhysicsDef, "baseJumpVelocity", 12f);
            SetPrivateField(defType, jumpPhysicsDef, "normalJumpMultiplier", 1.0f);
            SetPrivateField(defType, jumpPhysicsDef, "highJumpMultiplier", 1.5f);
            SetPrivateField(defType, jumpPhysicsDef, "doubleJumpMultiplier", 2.0f);
            SetPrivateField(defType, jumpPhysicsDef, "apexBoostMultiplier", 1.8f);
            SetPrivateField(defType, jumpPhysicsDef, "minHoldTime", 0.2f);
            SetPrivateField(defType, jumpPhysicsDef, "maxHoldTime", 1.0f);
            SetPrivateField(defType, jumpPhysicsDef, "apexWindow", 0.3f);
            SetPrivateField(defType, jumpPhysicsDef, "allowDoubleJump", true);
            SetPrivateField(defType, jumpPhysicsDef, "enableApexBoost", true);
        }
        
        private void SetPrivateField(System.Type type, object instance, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(instance, value);
        }
        
        private void SetupEventHandlers()
        {
            if (jumpController == null) return;
            
            jumpController.OnJumpPerformed += OnJumpPerformed;
            jumpController.OnApexReached += OnApexReached;
            jumpController.OnLanding += OnLanding;
            jumpController.OnDoubleJumpReady += OnDoubleJumpReady;
            jumpController.OnApexBoostReady += OnApexBoostReady;
        }
        
        private void Update()
        {
            if (!enableDemo) return;
            
            HandleDemoInput();
            UpdateDemoVisuals();
            
            // Update statistics
            if (jumpController != null)
            {
                lastStats = jumpController.GetStatistics();
            }
        }
        
        private void HandleDemoInput()
        {
            // Reset demo position
            if (UnityEngine.Input.GetKeyDown(resetDemoKey))
            {
                ResetDemo();
            }
            
            // Toggle advanced stats
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                showAdvancedStats = !showAdvancedStats;
            }
            
            // Demonstration phases
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                demonstrationPhase = 1;
                ShowDemonstrationPhase("Normal Jump (1x)", "Press and quickly release jump button");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                demonstrationPhase = 2;
                ShowDemonstrationPhase("High Jump (1.5x)", "Hold jump button for 0.2+ seconds");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                demonstrationPhase = 3;
                ShowDemonstrationPhase("Double Jump (2x)", "Jump twice in air");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
            {
                demonstrationPhase = 4;
                ShowDemonstrationPhase("Apex Boost (1.8x + lateral)", "Double jump at the peak of high jump");
            }
        }
        
        private void UpdateDemoVisuals()
        {
            if (jumpController == null) return;
            
            // Update platform materials based on jump state
            UpdatePlatformVisuals();
            
            // Update particle effects
            UpdateParticleEffects();
        }
        
        private void UpdatePlatformVisuals()
        {
            if (jumpPlatforms == null || jumpTypeMaterials == null) return;
            
            var currentState = jumpController.CurrentState;
            int materialIndex = 0;
            
            switch (currentState)
            {
                case EnhancedJumpController.JumpState.Grounded:
                    materialIndex = 0; // Normal
                    break;
                case EnhancedJumpController.JumpState.Rising:
                    materialIndex = jumpController.JumpHoldTime > 0.2f ? 1 : 0; // High or Normal
                    break;
                case EnhancedJumpController.JumpState.AtApex:
                    materialIndex = jumpController.IsAtApexWindow ? 3 : 1; // ApexBoosted or High
                    break;
                case EnhancedJumpController.JumpState.DoubleJumping:
                    materialIndex = 2; // Double
                    break;
                case EnhancedJumpController.JumpState.Falling:
                    materialIndex = 0; // Normal
                    break;
            }
            
            // Apply material to platforms
            if (materialIndex < jumpTypeMaterials.Length)
            {
                foreach (var platform in jumpPlatforms)
                {
                    var renderer = platform.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = jumpTypeMaterials[materialIndex];
                    }
                }
            }
        }
        
        private void UpdateParticleEffects()
        {
            if (jumpEffect == null) return;
            
            // Show particle effect when at apex and boost is ready
            bool shouldShowEffect = jumpController.IsAtApexWindow && jumpController.CanDoubleJump;
            
            if (shouldShowEffect && !jumpEffect.isPlaying)
            {
                jumpEffect.Play();
            }
            else if (!shouldShowEffect && jumpEffect.isPlaying)
            {
                jumpEffect.Stop();
            }
        }
        
        private void ShowDemonstrationPhase(string phaseName, string instruction)
        {
            EnterpriseLogger.LogInfo("DEMO", "JumpSystem", 
                $"Demonstration Phase: {phaseName} - {instruction}");
        }
        
        private void ResetDemo()
        {
            transform.position = initialPosition;
            demonstrationPhase = 0;
            
            if (jumpController != null)
            {
                // Reset jump controller state if needed
                EnterpriseLogger.LogInfo("DEMO", "JumpSystem", "Demo reset");
            }
        }
        
        // Event Handlers
        private void OnJumpPerformed(EnhancedJumpController.JumpType jumpType, float velocity)
        {
            EnterpriseLogger.LogInfo("DEMO", "JumpSystem", 
                $"Jump: {jumpType}, Velocity: {velocity:F2}");
            
            // Play appropriate sound
            PlayJumpSound(jumpType);
            
            // Show visual effect
            ShowJumpEffect(jumpType);
        }
        
        private void OnApexReached()
        {
            EnterpriseLogger.LogDebug("DEMO", "JumpSystem", "Apex reached - double jump window active");
        }
        
        private void OnLanding(bool isHardLanding)
        {
            string landingType = isHardLanding ? "Hard" : "Soft";
            EnterpriseLogger.LogDebug("DEMO", "JumpSystem", $"{landingType} landing");
        }
        
        private void OnDoubleJumpReady()
        {
            EnterpriseLogger.LogDebug("DEMO", "JumpSystem", "Double jump available");
        }
        
        private void OnApexBoostReady()
        {
            EnterpriseLogger.LogDebug("DEMO", "JumpSystem", "Apex boost available!");
        }
        
        private void PlayJumpSound(EnhancedJumpController.JumpType jumpType)
        {
            if (jumpAudioSource == null || jumpSounds == null) return;
            
            int soundIndex = (int)jumpType;
            if (soundIndex < jumpSounds.Length && jumpSounds[soundIndex] != null)
            {
                jumpAudioSource.pitch = GetJumpSoundPitch(jumpType);
                jumpAudioSource.PlayOneShot(jumpSounds[soundIndex]);
            }
        }
        
        private float GetJumpSoundPitch(EnhancedJumpController.JumpType jumpType)
        {
            switch (jumpType)
            {
                case EnhancedJumpController.JumpType.Normal: return 1.0f;
                case EnhancedJumpController.JumpType.High: return 1.2f;
                case EnhancedJumpController.JumpType.Double: return 1.4f;
                case EnhancedJumpController.JumpType.ApexBoosted: return 1.6f;
                default: return 1.0f;
            }
        }
        
        private void ShowJumpEffect(EnhancedJumpController.JumpType jumpType)
        {
            if (jumpEffect == null) return;
            
            // Adjust particle effect based on jump type
            var main = jumpEffect.main;
            main.startColor = GetJumpEffectColor(jumpType);
            main.startSize = GetJumpEffectSize(jumpType);
            
            jumpEffect.Emit(10);
        }
        
        private Color GetJumpEffectColor(EnhancedJumpController.JumpType jumpType)
        {
            switch (jumpType)
            {
                case EnhancedJumpController.JumpType.Normal: return Color.white;
                case EnhancedJumpController.JumpType.High: return Color.blue;
                case EnhancedJumpController.JumpType.Double: return Color.green;
                case EnhancedJumpController.JumpType.ApexBoosted: return new Color(1f, 0.84f, 0f); // Gold color
                default: return Color.white;
            }
        }
        
        private float GetJumpEffectSize(EnhancedJumpController.JumpType jumpType)
        {
            switch (jumpType)
            {
                case EnhancedJumpController.JumpType.Normal: return 1.0f;
                case EnhancedJumpController.JumpType.High: return 1.5f;
                case EnhancedJumpController.JumpType.Double: return 2.0f;
                case EnhancedJumpController.JumpType.ApexBoosted: return 2.5f;
                default: return 1.0f;
            }
        }
        
        // Debug UI
        private void OnGUI()
        {
            if (!showDebugUI) return;
            
            windowRect = GUI.Window(0, windowRect, DrawDebugWindow, "Enhanced Jump System Demo");
        }
        
        private void DrawDebugWindow(int windowID)
        {
            GUILayout.Space(5);
            
            // Demo controls
            GUILayout.Label("Demo Controls:", EditorStyles.boldLabel);
            GUILayout.Label("1 - Normal Jump Demo");
            GUILayout.Label("2 - High Jump Demo");
            GUILayout.Label("3 - Double Jump Demo");
            GUILayout.Label("4 - Apex Boost Demo");
            GUILayout.Label($"R - Reset ({resetDemoKey})");
            GUILayout.Label("Tab - Toggle Advanced Stats");
            
            GUILayout.Space(10);
            
            // Current stats
            GUILayout.Label("Current Statistics:", EditorStyles.boldLabel);
            if (jumpController != null)
            {
                GUILayout.Label($"State: {lastStats.CurrentState}");
                GUILayout.Label($"Total Jumps: {lastStats.TotalJumps}");
                GUILayout.Label($"Apex Boosts: {lastStats.ApexBoosts}");
                GUILayout.Label($"Boost Rate: {lastStats.ApexBoostRate:P1}");
                GUILayout.Label($"Has Double Jump: {lastStats.HasDoubleJump}");
                GUILayout.Label($"At Apex: {lastStats.IsAtApex}");
                GUILayout.Label($"Hold Time: {jumpController.JumpHoldTime:F3}s");
            }
            
            if (showAdvancedStats)
            {
                GUILayout.Space(10);
                GUILayout.Label("Advanced Stats:", EditorStyles.boldLabel);
                GUILayout.Label($"Demo Phase: {demonstrationPhase}");
                GUILayout.Label($"Runtime: {Time.time - demoStartTime:F1}s");
                
                if (jumpPhysicsDef != null)
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Jump Physics:");
                    GUILayout.Label($"Base Velocity: {jumpPhysicsDef.BaseJumpVelocity:F1}");
                    GUILayout.Label($"Normal: {jumpPhysicsDef.NormalJumpMultiplier:F1}x");
                    GUILayout.Label($"High: {jumpPhysicsDef.HighJumpMultiplier:F1}x");
                    GUILayout.Label($"Double: {jumpPhysicsDef.DoubleJumpMultiplier:F1}x");
                    GUILayout.Label($"Apex Boost: {jumpPhysicsDef.ApexBoostMultiplier:F1}x");
                }
            }
            
            GUI.DragWindow();
        }
        
        private void OnDestroy()
        {
            if (jumpController != null)
            {
                jumpController.OnJumpPerformed -= OnJumpPerformed;
                jumpController.OnApexReached -= OnApexReached;
                jumpController.OnLanding -= OnLanding;
                jumpController.OnDoubleJumpReady -= OnDoubleJumpReady;
                jumpController.OnApexBoostReady -= OnApexBoostReady;
            }
        }
    }
    
    // Helper class for editor styles in runtime
    public static class EditorStyles
    {
        public static GUIStyle boldLabel = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
    }
}
