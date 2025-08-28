using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MOBA.Input
{
    /// <summary>
    /// Modern Input System wrapper for MemeOArena.
    /// This class loads the InputSystem_Actions.inputactions asset and provides clean access to all actions.
    /// </summary>
    public class InputSystem_Actions : IDisposable
    {
        private InputActionAsset asset;
        private InputActionMap playerMap;
        private InputActionMap uiMap;

        // Player actions
        public InputAction Move { get; private set; }
        public InputAction Look { get; private set; }
        public InputAction Jump { get; private set; }
        public InputAction Attack { get; private set; }
        public InputAction Interact { get; private set; }
        public InputAction Crouch { get; private set; }
        public InputAction Sprint { get; private set; }
        public InputAction Ability1 { get; private set; }
        public InputAction Ability2 { get; private set; }
        public InputAction Ultimate { get; private set; }
        public InputAction Scoring { get; private set; }
        public InputAction TestAddPoints { get; private set; }
        public InputAction TestDamage { get; private set; }
        
        // Camera actions
        public InputAction CameraToggle { get; private set; }
        public InputAction FreePan { get; private set; }

        // Player actions wrapper (for compatibility)
        public PlayerActions Player { get; private set; }

        public InputSystem_Actions()
        {
            // Try direct asset database load first (most reliable)
            #if UNITY_EDITOR
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            #endif
            
            if (asset == null)
            {
                // Try Resources loading (requires asset to be in Resources folder)
                asset = Resources.Load<InputActionAsset>("InputSystem_Actions");
            }
            
            if (asset != null)
            {
                Debug.Log("Successfully loaded InputSystem_Actions asset.");
                InitializeFromAsset();
            }
            else
            {
                Debug.LogWarning("InputSystem_Actions asset not found. Creating fallback actions.");
                CreateFallbackActions();
            }

            Player = new PlayerActions(this);
        }

        private void InitializeFromAsset()
        {
            playerMap = asset.FindActionMap("Player");
            uiMap = asset.FindActionMap("UI");

            if (playerMap != null)
            {
                Debug.Log($"Found Player action map with {playerMap.actions.Count} actions");
                
                Move = playerMap.FindAction("Move");
                Look = playerMap.FindAction("Look");
                Jump = playerMap.FindAction("Jump");
                Attack = playerMap.FindAction("Attack");
                Interact = playerMap.FindAction("Interact");
                Crouch = playerMap.FindAction("Crouch");
                Sprint = playerMap.FindAction("Sprint");
                Ability1 = playerMap.FindAction("Ability1");
                Ability2 = playerMap.FindAction("Ability2");
                Ultimate = playerMap.FindAction("Ultimate");
                Scoring = playerMap.FindAction("Scoring");
                TestAddPoints = playerMap.FindAction("TestAddPoints");
                TestDamage = playerMap.FindAction("TestDamage");
                CameraToggle = playerMap.FindAction("CameraToggle");
                FreePan = playerMap.FindAction("FreePan");
                
                Debug.Log($"Move action found: {Move != null}, Ability1 found: {Ability1 != null}, Camera actions found: {CameraToggle != null}");
            }
            else
            {
                Debug.LogError("Player action map not found in asset!");
            }
        }

        private void CreateFallbackActions()
        {
            // Create a basic action map for fallback
            asset = ScriptableObject.CreateInstance<InputActionAsset>();
            playerMap = new InputActionMap("Player");
            asset.AddActionMap(playerMap);

            Move = playerMap.AddAction("Move", InputActionType.Value, "<Keyboard>/wasd");
            Jump = playerMap.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
            Attack = playerMap.AddAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
            Interact = playerMap.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            Crouch = playerMap.AddAction("Crouch", InputActionType.Button, "<Keyboard>/c");
            Sprint = playerMap.AddAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            Ability1 = playerMap.AddAction("Ability1", InputActionType.Button, "<Keyboard>/q");
            Ability2 = playerMap.AddAction("Ability2", InputActionType.Button, "<Keyboard>/f");
            Ultimate = playerMap.AddAction("Ultimate", InputActionType.Button, "<Keyboard>/r");
            Scoring = playerMap.AddAction("Scoring", InputActionType.Button, "<Keyboard>/g");
            TestAddPoints = playerMap.AddAction("TestAddPoints", InputActionType.Button, "<Keyboard>/p");
            TestDamage = playerMap.AddAction("TestDamage", InputActionType.Button, "<Keyboard>/t");
            CameraToggle = playerMap.AddAction("CameraToggle", InputActionType.Button, "<Keyboard>/c");
            FreePan = playerMap.AddAction("FreePan", InputActionType.Button, "<Keyboard>/v");
            
            // Enable the fallback action map immediately
            playerMap.Enable();
        }

        public void Enable()
        {
            asset?.Enable();
        }

        public void Disable()
        {
            asset?.Disable();
        }

        public void Dispose()
        {
            asset?.Disable();
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(asset);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        public class PlayerActions
        {
            private InputSystem_Actions parent;

            public PlayerActions(InputSystem_Actions parent)
            {
                this.parent = parent;
            }

            public InputAction Move => parent.Move;
            public InputAction Look => parent.Look;
            public InputAction Jump => parent.Jump;
            public InputAction Attack => parent.Attack;
            public InputAction Interact => parent.Interact;
            public InputAction Crouch => parent.Crouch;
            public InputAction Sprint => parent.Sprint;
            public InputAction Ability1 => parent.Ability1;
            public InputAction Ability2 => parent.Ability2;
            public InputAction Ultimate => parent.Ultimate;
            public InputAction Scoring => parent.Scoring;
            public InputAction TestAddPoints => parent.TestAddPoints;
            public InputAction TestDamage => parent.TestDamage;
            public InputAction CameraToggle => parent.CameraToggle;
            public InputAction FreePan => parent.FreePan;

            public void Enable() => parent.Enable();
            public void Disable() => parent.Disable();
        }
    }
}
