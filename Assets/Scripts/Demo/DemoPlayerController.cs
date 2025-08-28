using UnityEngine;
using MOBA.Data;
using MOBA.Controllers;
using MOBA.Input;
using MOBA.Bootstrap;
using MOBA.Core;

namespace MOBA.Demo
{
    /// <summary>
    /// Simple demo controller that integrates all the core systems for testing.
    /// Attach this to a player GameObject to get a working character with movement, abilities, and scoring.
    /// </summary>
    public class DemoPlayerController : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool useDefaultAssets = true;
        [SerializeField] private DefaultGameConfig gameConfig;
        
        [Header("Testing Controls")]
        [SerializeField] private KeyCode addPointsKey = KeyCode.P;
        [SerializeField] private KeyCode damageKey = KeyCode.T;
        [SerializeField] private KeyCode toggleChannelKey = KeyCode.E;
        [SerializeField] private KeyCode simulateDeathKey = KeyCode.K; // New death simulation key
        
        // Core systems
        private PlayerContext playerContext;
        private LocomotionController locomotion;
        private AbilityController abilities;
        private ScoringController scoring;
        private IInputSource inputSource;
        private TickManager tickManager;
        private CameraController cameraController;
        
        // Runtime state
        private bool isInitialized = false;
        private bool isChanneling = false;
        private bool isDead = false;
        
        // Public properties for UI/external access
        public bool IsDead => isDead;
        
        void Awake()
        {
            InitializeSystems();
        }
        
        void Start()
        {
            if (isInitialized)
            {
                Debug.Log($"DemoPlayerController: Player '{playerContext.playerId}' ready for testing!");
                LogControls();
            }
        }
        
        void Update()
        {
            if (!isInitialized) return;
            
            float dt = Time.deltaTime;
            
            // Update all systems
            locomotion?.Update(dt);
            abilities?.Update(dt);
            scoring?.Update(dt);
            
            // Apply movement from locomotion system
            if (locomotion != null)
            {
                Vector3 desiredVelocity = locomotion.DesiredVelocity;
                if (desiredVelocity.magnitude > 0.01f)
                {
                    // Apply movement directly to transform (simple approach for demo)
                    transform.position += desiredVelocity * dt;
                    
                    // Optional: Rotate to face movement direction
                    if (desiredVelocity.magnitude > 0.1f)
                    {
                        Vector3 lookDirection = desiredVelocity.normalized;
                        lookDirection.y = 0; // Keep on horizontal plane
                        if (lookDirection != Vector3.zero)
                        {
                            transform.rotation = Quaternion.Slerp(transform.rotation, 
                                                               Quaternion.LookRotation(lookDirection), 
                                                               10f * dt);
                        }
                    }
                }
            }
            
            // Handle test inputs
            HandleTestInputs();
            
            // Handle camera controls
            HandleCameraControls();
            
            // Update UI debug info (would be replaced by actual UI)
            UpdateDebugDisplay();
        }
        
        private void InitializeSystems()
        {
            try
            {
                // Ensure this GameObject has the Player tag for camera following
                if (!gameObject.CompareTag("Player"))
                {
                    gameObject.tag = "Player";
                }
                
                // Create or use provided game configuration
                if (useDefaultAssets || gameConfig == null)
                {
                    gameConfig = DefaultAssetCreator.CreateCompleteDefaultConfig();
                    Debug.Log("DemoPlayerController: Using default game configuration");
                }
                
                // Find or create tick manager
                tickManager = FindFirstObjectByType<TickManager>();
                if (tickManager == null)
                {
                    var tickManagerObj = new GameObject("TickManager");
                    tickManager = tickManagerObj.AddComponent<TickManager>();
                    Debug.Log("DemoPlayerController: Created TickManager");
                }
                
                // Create player context with demo ID
                string playerId = $"Player_{GetInstanceID()}";
                playerContext = new PlayerContext(playerId, gameConfig.playerStats, gameConfig.ultimateEnergy, gameConfig.scoring);
                
                // Add abilities to context
                playerContext.abilities.Add(gameConfig.basicAbility);
                playerContext.abilities.Add(gameConfig.ultimateAbility);
                
                // Create input source (using our stub implementation for now)
                inputSource = new UnityInputSource(new MOBA.Input.InputSystem_Actions());
                
                // Initialize controllers
                locomotion = new LocomotionController(playerContext, inputSource);
                abilities = new AbilityController(playerContext);
                scoring = new ScoringController(playerContext);
                
                // Find camera controller
                cameraController = FindFirstObjectByType<CameraController>();
                
                isInitialized = true;
                Debug.Log($"DemoPlayerController: Successfully initialized for player {playerId}");
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"DemoPlayerController: Failed to initialize systems: {e.Message}");
                enabled = false;
            }
        }
        
        private void HandleTestInputs()
        {
            // Add points for scoring testing - using new Input System
            if (inputSource.IsTestAddPointsPressed())
            {
                int pointsToAdd = Random.Range(1, 8);
                playerContext.carriedPoints += pointsToAdd;
                Debug.Log($"Added {pointsToAdd} points. Total carried: {playerContext.carriedPoints}");
            }
            
            // Test damage application - using new Input System
            if (inputSource.IsTestDamagePressed())
            {
                int damage = Random.Range(50, 200);
                playerContext.ApplyDamage(damage);
                Debug.Log($"Applied {damage} damage. HP: {playerContext.currentHP}/{playerContext.baseStats.MaxHP}");
                
                // Heal if HP gets too low
                if (playerContext.currentHP <= 100)
                {
                    playerContext.currentHP = playerContext.baseStats.MaxHP;
                    Debug.Log("Healed to full HP for continued testing");
                }
            }
            
            // Toggle scoring channel for testing - E key through regular input check
            if (UnityEngine.Input.GetKeyDown(toggleChannelKey))
            {
                if (playerContext.carriedPoints > 0)
                {
                    if (isChanneling)
                    {
                        scoring.Interrupt();
                        isChanneling = false;
                        Debug.Log("Interrupted scoring channel");
                    }
                    else
                    {
                        scoring.StartChanneling(1, new string[0]); // Simulate 1 ally present, no buffs
                        isChanneling = true;
                        Debug.Log($"Started channeling {playerContext.carriedPoints} points");
                    }
                }
                else
                {
                    Debug.Log($"No points to score! Press {addPointsKey} to add points first.");
                }
            }
            
            // Simulate player death/respawn for testing camera behavior - K key
            if (UnityEngine.Input.GetKeyDown(simulateDeathKey))
            {
                if (isDead)
                {
                    // Respawn player
                    RespawnPlayer();
                }
                else
                {
                    // Kill player
                    KillPlayer();
                }
            }
        }
        
        private void HandleCameraControls()
        {
            if (cameraController == null) return;
            
            // Check for camera mode cycling (C key) - for demo purposes only
            if (inputSource.IsCameraTogglePressed())
            {
                // Cycle through camera modes for demo testing
                var currentMode = cameraController.cameraMode;
                var modes = System.Enum.GetValues(typeof(CameraController.CameraMode));
                int currentIndex = System.Array.IndexOf(modes, currentMode);
                int nextIndex = (currentIndex + 1) % modes.Length;
                cameraController.SetCameraMode((CameraController.CameraMode)modes.GetValue(nextIndex));
                Debug.Log($"Demo: Camera mode changed to: {cameraController.cameraMode}");
            }
            
            // Note: Pan behavior is now handled automatically by the camera controller
            // - Hold V to pan camera
            // - Release V to return to following player
            // - Camera will stay in pan mode if player is dead
        }
        
        private void UpdateDebugDisplay()
        {
            // This would be replaced by actual UI in a full implementation
            if (Time.frameCount % 60 == 0) // Update once per second at 60 FPS
            {
                string status = $"HP: {playerContext.currentHP}/{playerContext.baseStats.MaxHP} | " +
                               $"Points: {playerContext.carriedPoints} | " +
                               $"Energy: {playerContext.ultimateEnergy:F1}/{playerContext.ultimateDef.required} | " +
                               $"Ultimate: {(abilities.IsUltimateReady ? "READY" : "Not Ready")}";
                               
                Debug.Log($"DemoPlayer Status: {status}");
            }
        }
        
        private void LogControls()
        {
            Debug.Log("=== Demo Controls ===");
            Debug.Log("WASD: Move player");
            Debug.Log("V: Hold to pan camera (release to return to player)");
            Debug.Log("Arrow Keys: Pan camera while holding V");
            Debug.Log("Right Mouse: Drag to pan camera while holding V");
            Debug.Log("Scroll: Zoom camera while panning");
            Debug.Log($"{addPointsKey}: Add random points (1-7)");
            Debug.Log($"{damageKey}: Take random damage (50-200)");
            Debug.Log($"{toggleChannelKey}: Start/stop scoring channel");
            Debug.Log($"{simulateDeathKey}: Simulate death/respawn (test camera behavior)");
            Debug.Log("Q: Basic Ability (if input connected)");
            Debug.Log("R: Ultimate Ability (if input connected and energy ready)");
            Debug.Log("C: Cycle camera modes (demo only)");
            Debug.Log("=====================");
        }
        
        private void KillPlayer()
        {
            isDead = true;
            Debug.Log("💀 Player died! Camera switched to death pan mode.");
            Debug.Log($"Use Arrow Keys or Right Mouse to pan camera. Press {simulateDeathKey} to respawn.");
            
            // Tell camera to enter death panning mode
            if (cameraController != null)
            {
                cameraController.EnableDeathPanning();
            }
            
            // Make player visual semi-transparent or change color
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;
                Color color = mat.color;
                color.a = 0.5f;
                mat.color = color;
            }
        }
        
        private void RespawnPlayer()
        {
            isDead = false;
            Debug.Log("✨ Player respawned! Camera returning to follow mode.");
            
            // Reset player health
            playerContext.currentHP = playerContext.baseStats.MaxHP;
            
            // Tell camera to return to following
            if (cameraController != null)
            {
                cameraController.EnableFollowOnRespawn();
            }
            
            // Restore player visual
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;
                Color color = mat.color;
                color.a = 1f;
                mat.color = color;
            }
        }
        
        void OnGUI()
        {
            if (!isInitialized) return;
            
            // Simple on-screen debug display
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label($"Player: {playerContext.playerId}");
            GUILayout.Label($"HP: {playerContext.currentHP}/{playerContext.baseStats.MaxHP}");
            GUILayout.Label($"Carried Points: {playerContext.carriedPoints}");
            GUILayout.Label($"Ultimate Energy: {playerContext.ultimateEnergy:F1}/{playerContext.ultimateDef.required}");
            GUILayout.Label($"Ultimate Status: {(abilities.IsUltimateReady ? "READY" : "Charging")}");
            GUILayout.Label($"Status: <color={(isDead ? "red" : "lime")}>{(isDead ? "DEAD" : "ALIVE")}</color>");
            
            // Movement debugging
            Vector2 moveInput = inputSource?.GetMoveVector() ?? Vector2.zero;
            GUILayout.Label($"Input: ({moveInput.x:F2}, {moveInput.y:F2})");
            GUILayout.Label($"Desired Velocity: {locomotion.DesiredVelocity.magnitude:F2}");
            GUILayout.Label($"Position: ({transform.position.x:F1}, {transform.position.z:F1})");
            
            GUILayout.Space(10);
            GUILayout.Label("Test Controls:");
            GUILayout.Label($"{addPointsKey} - Add Points");
            GUILayout.Label($"{damageKey} - Take Damage");
            GUILayout.Label($"{toggleChannelKey} - Score Points");
            GUILayout.Label($"{simulateDeathKey} - Death/Respawn");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        void OnValidate()
        {
            // Refresh configuration if changed in inspector
            if (useDefaultAssets && Application.isPlaying && isInitialized)
            {
                Debug.Log("DemoPlayerController: Configuration changed, reinitializing...");
                InitializeSystems();
            }
        }
    }
}
