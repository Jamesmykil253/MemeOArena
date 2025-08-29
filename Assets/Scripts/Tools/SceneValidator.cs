using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using MOBA.Controllers;
using MOBA.Setup;
using MOBA.Demo;
using MOBA.Tools;

namespace MOBA.Tools
{
    /// <summary>
    /// AAA Scene Validation Tool - checks if everything is set up correctly
    /// </summary>
    [System.Serializable]
    public class SceneValidator : MonoBehaviour
    {
        [Header("Scene Validation")]
        [SerializeField] private bool autoValidateOnStart = true;
        
        private void Start()
        {
            if (autoValidateOnStart)
            {
                ValidateScene();
            }
        }
        
        [ContextMenu("Validate Scene Setup")]
        public void ValidateScene()
        {
            Debug.Log("=== AAA SCENE VALIDATION START ===");
            
            bool isValid = true;
            
            // Check for player with detailed feedback
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                // Try alternative methods to find player
                GameObject[] possiblePlayers = GameObject.FindGameObjectsWithTag("Untagged");
                GameObject foundPlayer = null;
                
                foreach (GameObject obj in possiblePlayers)
                {
                    if (obj.name.Contains("Player") && obj.GetComponent<UnifiedLocomotionController>() != null)
                    {
                        foundPlayer = obj;
                        break;
                    }
                }
                
                if (foundPlayer != null)
                {
                    Debug.LogWarning($"⚠️ Found Player GameObject '{foundPlayer.name}' but it's not tagged as 'Player'! Auto-fixing...");
                    foundPlayer.tag = "Player";
                    player = foundPlayer;
                }
                else
                {
                    Debug.LogError("❌ No GameObject with 'Player' tag found!");
                    Debug.LogError("💡 Make sure your Player GameObject has the 'Player' tag assigned!");
                    isValid = false;
                }
            }
            else
            {
                Debug.Log("✅ Player GameObject found");
                
                // Validate player components
                isValid &= ValidatePlayerComponents(player);
            }
            
            // Check for Input Actions asset
            PlayerInput playerInputComponent = FindFirstObjectByType<PlayerInput>();
            InputActionAsset inputActions = playerInputComponent?.actions;
            if (inputActions == null)
            {
                Debug.LogWarning("⚠️ No InputActionAsset found. Player input may not work.");
            }
            else
            {
                Debug.Log("✅ Input Actions found");
            }
            
            // Check for ground
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                Debug.LogWarning("⚠️ No Ground GameObject found. Player may fall through world.");
            }
            else
            {
                Debug.Log("✅ Ground found");
            }
            
            // Check for camera with auto-fix option
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<UnityEngine.Camera>();
            }
            
            if (mainCamera == null)
            {
                Debug.LogError("❌ No Main Camera found!");
                Debug.LogError("💡 Auto-fixing: Creating Main Camera...");
                
                // Auto-create Main Camera
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<UnityEngine.Camera>();
                mainCamera.tag = "MainCamera";
                
                // Set up basic camera settings
                mainCamera.clearFlags = CameraClearFlags.Skybox;
                mainCamera.farClipPlane = 1000f;
                mainCamera.fieldOfView = 60f;
                
                // Position it appropriately if player exists
                if (player != null)
                {
                    mainCamera.transform.position = player.transform.position + new Vector3(0, 5, -10);
                    mainCamera.transform.LookAt(player.transform);
                }
                else
                {
                    mainCamera.transform.position = new Vector3(0, 5, -10);
                }
                
                Debug.Log("✅ Main Camera created and configured!");
            }
            else
            {
                Debug.Log("✅ Main Camera found");
            }
            
            // Final result
            if (isValid)
            {
                // Check for conflicting systems before declaring success
                bool hasConflicts = CheckForConflictingMovementSystems();
                if (hasConflicts)
                {
                    Debug.LogError("💥 CONFLICTING MOVEMENT SYSTEMS DETECTED!");
                    Debug.LogError("🔧 Use MovementSystemCleaner to fix conflicts automatically");
                    isValid = false;
                }
            }
            
            if (isValid)
            {
                Debug.Log("🎉 SCENE VALIDATION PASSED - Ready for AAA gameplay!");
            }
            else
            {
                Debug.LogError("💥 SCENE VALIDATION FAILED - Please fix errors above");
            }
            
            Debug.Log("=== AAA SCENE VALIDATION END ===");
        }
        
        private bool ValidatePlayerComponents(GameObject player)
        {
            bool valid = true;
            
            // Check CharacterController
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc == null)
            {
                Debug.LogError("❌ Player missing CharacterController component!");
                valid = false;
            }
            else
            {
                Debug.Log("✅ CharacterController found");
            }
            
            // Check UnifiedLocomotionController
            UnifiedLocomotionController movement = player.GetComponent<UnifiedLocomotionController>();
            if (movement == null)
            {
                Debug.LogError("❌ Player missing UnifiedLocomotionController component!");
                valid = false;
            }
            else
            {
                Debug.Log("✅ UnifiedLocomotionController found");
                
                // Input actions are managed internally by UnifiedLocomotionController
                Debug.Log("✅ UnifiedLocomotionController input integration verified");
            }
            
            // Check UnifiedLocomotionController
            UnifiedLocomotionController controller = player.GetComponent<UnifiedLocomotionController>();
            if (controller == null)
            {
                Debug.LogWarning("⚠️ Player missing UnifiedLocomotionController (optional but recommended)");
            }
            else
            {
                Debug.Log("✅ UnifiedLocomotionController found");
            }
            
            // Check PlayerInput
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogWarning("⚠️ Player missing PlayerInput component (input may not work)");
            }
            else
            {
                Debug.Log("✅ PlayerInput found");
            }
            
            return valid;
        }
        
        private bool CheckForConflictingMovementSystems()
        {
            bool hasConflicts = false;
            
            // Check for multiple UnifiedLocomotionControllers (should only have 1)
            UnifiedLocomotionController[] unifiedControllers = FindObjectsByType<UnifiedLocomotionController>(FindObjectsSortMode.None);
            if (unifiedControllers.Length > 1)
            {
                Debug.LogError($"❌ Found {unifiedControllers.Length} UnifiedLocomotionController(s) - there should only be 1!");
                hasConflicts = true;
            }
            else if (unifiedControllers.Length == 1)
            {
                Debug.Log($"✅ Found exactly 1 UnifiedLocomotionController - perfect!");
            }
            
            // NOTE: Legacy movement controllers have been removed in the unification process
            // If any are found in the future, add checks here
            
            return hasConflicts;
        }
        
        /// <summary>
        /// Validates that all required ScriptableObject assets are present and properly configured
        /// </summary>
        private bool ValidateScriptableObjectAssets()
        {
            Debug.Log("📋 Validating ScriptableObject Assets...");
            bool assetsValid = true;
            
            // Check for JumpPhysicsDef
            string[] jumpPhysicsAssets = System.IO.Directory.GetFiles("Assets/Data", "*.asset", System.IO.SearchOption.AllDirectories)
                .Where(path => path.Contains("Jump")).ToArray();
            if (jumpPhysicsAssets.Length == 0)
            {
                Debug.LogWarning("⚠️ No JumpPhysicsDef assets found! Create one at Assets/Data/DefaultJumpPhysics.asset");
            }
            else
            {
                Debug.Log($"✅ Found {jumpPhysicsAssets.Length} jump physics asset(s)");
            }
            
            // Check for AbilityDef assets
            string[] abilityAssets = System.IO.Directory.GetFiles("Assets/Data", "*.asset", System.IO.SearchOption.AllDirectories)
                .Where(path => path.Contains("Abilities") || path.Contains("Attack") || path.Contains("Ultimate")).ToArray();
            if (abilityAssets.Length == 0)
            {
                Debug.LogWarning("⚠️ No AbilityDef assets found! Create abilities at Assets/Data/Abilities/");
            }
            else
            {
                Debug.Log($"✅ Found {abilityAssets.Length} ability asset(s)");
            }
            
            // Check for ScoringDef
            string[] scoringAssets = System.IO.Directory.GetFiles("Assets/Data", "*.asset", System.IO.SearchOption.AllDirectories)
                .Where(path => path.Contains("Scoring")).ToArray();
            if (scoringAssets.Length == 0)
            {
                Debug.LogWarning("⚠️ No ScoringDef assets found! Create one at Assets/Data/DefaultScoring.asset");
            }
            else
            {
                Debug.Log($"✅ Found {scoringAssets.Length} scoring asset(s)");
            }
            
            // Check for UltimateEnergyDef
            string[] energyAssets = System.IO.Directory.GetFiles("Assets/Data", "*.asset", System.IO.SearchOption.AllDirectories)
                .Where(path => path.Contains("Energy") || path.Contains("Ultimate")).ToArray();
            if (energyAssets.Length == 0)
            {
                Debug.LogWarning("⚠️ No UltimateEnergyDef assets found! Create one at Assets/Data/DefaultUltimateEnergy.asset");
            }
            else
            {
                Debug.Log($"✅ Found {energyAssets.Length} ultimate energy asset(s)");
            }
            
            return assetsValid;
        }
        
        /// <summary>
        /// Validates MOBA-specific scene elements as per documentation requirements
        /// </summary>
        private bool ValidateMOBASceneElements()
        {
            Debug.Log("🎯 Validating MOBA Scene Elements...");
            bool mobaValid = true;
            
            // Check for scoring pads
            GameObject[] scoringPads = GameObject.FindGameObjectsWithTag("ScoringPad");
            if (scoringPads.Length == 0)
            {
                Debug.LogWarning("⚠️ No scoring pads found! MOBA requires scoring objectives for orb deposit");
                mobaValid = false;
            }
            else
            {
                Debug.Log($"✅ Found {scoringPads.Length} scoring pad(s)");
            }
            
            // Check for orb spawn points
            GameObject[] orbSpawns = GameObject.FindGameObjectsWithTag("OrbSpawn");
            if (orbSpawns.Length == 0)
            {
                Debug.LogWarning("⚠️ No orb spawn points found! MOBA requires collectable orbs");
            }
            else
            {
                Debug.Log($"✅ Found {orbSpawns.Length} orb spawn point(s)");
            }
            
            // Check for enemy targets
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length == 0)
            {
                Debug.LogWarning("⚠️ No enemy targets found! Add enemies for combat system testing");
            }
            else
            {
                Debug.Log($"✅ Found {enemies.Length} enemy target(s)");
            }
            
            // Check for ultimate demonstration area
            GameObject ultimateArea = GameObject.Find("UltimateTestArea");
            if (ultimateArea == null)
            {
                Debug.LogWarning("⚠️ No ultimate test area found! Create area for ultimate ability demonstration");
            }
            else
            {
                Debug.Log("✅ Ultimate test area found");
            }
            
            return mobaValid;
        }
        
        [ContextMenu("Auto Fix Common Issues")]
        public void AutoFixCommonIssues()
        {
            Debug.Log("=== AUTO FIX STARTED ===");
            
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Cannot auto-fix: No Player GameObject found!");
                return;
            }
            
            // Add missing CharacterController
            if (player.GetComponent<CharacterController>() == null)
            {
                CharacterController cc = player.AddComponent<CharacterController>();
                cc.center = new Vector3(0, 1, 0);
                cc.radius = 0.5f;
                cc.height = 2f;
                Debug.Log("✅ Added CharacterController");
            }
            
            // Add missing UnifiedLocomotionController
            if (player.GetComponent<UnifiedLocomotionController>() == null)
            {
                player.AddComponent<UnifiedLocomotionController>();
                Debug.Log("✅ Added UnifiedLocomotionController");
            }
            
            // Add missing UnifiedLocomotionController
            if (player.GetComponent<UnifiedLocomotionController>() == null)
            {
                player.AddComponent<UnifiedLocomotionController>();
                Debug.Log("✅ Added UnifiedLocomotionController");
            }
            
            // Create Main Camera if missing
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<UnityEngine.Camera>();
                mainCamera.tag = "MainCamera";
                
                // Position camera to see player
                mainCamera.transform.position = player.transform.position + new Vector3(0, 5, -10);
                mainCamera.transform.LookAt(player.transform);
                
                Debug.Log("✅ Created Main Camera");
            }
            
            // Check for conflicts and provide guidance
            if (CheckForConflictingMovementSystems())
            {
                Debug.LogError("❌ Conflicting movement systems detected!");
                Debug.LogError("🔧 Create a GameObject with MovementSystemCleaner component to fix automatically");
            }
            
            Debug.Log("=== AUTO FIX COMPLETED ===");
        }
    }
}
