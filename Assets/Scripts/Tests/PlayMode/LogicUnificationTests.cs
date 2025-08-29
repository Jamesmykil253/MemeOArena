using UnityEngine;
using NUnit.Framework;
using MOBA.Controllers;
using MOBA.Core;
using MOBA.Data;

namespace Tests.PlayMode
{
    /// <summary>
    /// Integration tests for the unified logic fixes.
    /// Tests the enhanced ability controller, unified locomotion, and system integration.
    /// </summary>
    public class LogicUnificationTests
    {
        private PlayerContext testContext;
        private TestInputSource testInput;
        
        [SetUp]
        public void Setup()
        {
            // Create test context with proper data
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MaxHP = 1000;
            baseStats.Attack = 100;
            baseStats.Defense = 50;
            baseStats.MoveSpeed = 5f;
            
            var ultimateDef = ScriptableObject.CreateInstance<UltimateEnergyDef>();
            ultimateDef.maxEnergy = 100f;
            ultimateDef.energyRequirement = 100f;
            ultimateDef.regenRate = 2f;
            ultimateDef.cooldownConstant = 10f;
            
            var scoringDef = ScriptableObject.CreateInstance<ScoringDef>();
            scoringDef.thresholds = new[] { 5, 10, 15 };
            scoringDef.baseTimes = new[] { 1f, 2f, 3f };
            scoringDef.synergyMultipliers = new[] { 1f, 0.8f, 0.6f };
            
            testContext = new PlayerContext("test_player", baseStats, ultimateDef, scoringDef);
            
            // Add test abilities
            var basicAbility = ScriptableObject.CreateInstance<AbilityDef>();
            basicAbility.name = "Test Basic";
            basicAbility.CastTime = 1f;
            basicAbility.Cooldown = 5f;
            basicAbility.Ratio = 1f;
            basicAbility.Base = 50;
            
            var ultimateAbility = ScriptableObject.CreateInstance<AbilityDef>();
            ultimateAbility.name = "Test Ultimate";
            ultimateAbility.CastTime = 2f;
            ultimateAbility.Cooldown = 60f;
            ultimateAbility.Ratio = 2f;
            ultimateAbility.Base = 200;
            
            testContext.abilities.Add(basicAbility);
            testContext.abilities.Add(ultimateAbility);
            
            testInput = new TestInputSource();
        }
        
        [Test]
        public void EnhancedAbilityController_HasCorrectFSMStates()
        {
            var abilityController = new EnhancedAbilityController(testContext);
            
            Assert.IsTrue(abilityController.IsIdle);
            Assert.IsFalse(abilityController.IsCasting);
            Assert.IsFalse(abilityController.IsExecuting);
            Assert.IsFalse(abilityController.IsOnCooldown);
        }
        
        [Test]
        public void EnhancedAbilityController_CastBasicAbility()
        {
            var abilityController = new EnhancedAbilityController(testContext);
            var basicAbility = testContext.abilities[0];
            
            bool castStarted = abilityController.TryCastAbility(basicAbility);
            Assert.IsTrue(castStarted);
            Assert.IsTrue(abilityController.IsCasting);
            Assert.AreEqual(basicAbility, abilityController.CurrentAbility);
        }
        
        [Test]
        public void EnhancedAbilityController_CompleteCastCycle()
        {
            var abilityController = new EnhancedAbilityController(testContext);
            var basicAbility = testContext.abilities[0];
            
            // Start casting
            bool castStarted = abilityController.TryCastAbility(basicAbility);
            Assert.IsTrue(castStarted);
            Assert.IsTrue(abilityController.IsCasting);
            
            // Simulate cast time by updating manually
            float castTime = basicAbility.CastTime;
            float timeStep = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < castTime && abilityController.IsCasting)
            {
                abilityController.Update(timeStep);
                elapsed += timeStep;
            }
            
            // Should now be on cooldown (execution is instantaneous)
            Assert.IsTrue(abilityController.IsOnCooldown);
            Assert.IsFalse(abilityController.IsCasting);
        }
        
        [Test]
        public void EnhancedAbilityController_UltimateRequiresEnergy()
        {
            var abilityController = new EnhancedAbilityController(testContext);
            var ultimateAbility = testContext.abilities[1];
            
            // Should fail without energy
            testContext.ultimateEnergy = 50f; // Not enough
            bool castStarted = abilityController.TryCastAbility(ultimateAbility);
            Assert.IsFalse(castStarted);
            Assert.IsTrue(abilityController.IsIdle);
            
            // Should succeed with energy
            testContext.ultimateEnergy = 100f; // Enough
            castStarted = abilityController.TryCastAbility(ultimateAbility);
            Assert.IsTrue(castStarted);
            Assert.IsTrue(abilityController.IsCasting);
        }
        
        [Test]
        public void UnifiedLocomotionController_InitializesCorrectly()
        {
            var gameObj = new GameObject("TestPlayer");
            var locomotion = gameObj.AddComponent<UnifiedLocomotionController>();
            
            locomotion.Initialize(testContext, testInput);
            
            Assert.IsNotNull(locomotion);
            Assert.AreEqual(Vector3.zero, locomotion.DesiredVelocity);
            
            Object.DestroyImmediate(gameObj);
        }
        
        [Test]
        public void UnifiedLocomotionController_RespondsToInput()
        {
            var gameObj = new GameObject("TestPlayer");
            var locomotion = gameObj.AddComponent<UnifiedLocomotionController>();
            
            locomotion.Initialize(testContext, testInput);
            
            // Set test input
            testInput.SetMoveInput(new Vector2(1f, 0f));
            
            // Update locomotion using public Tick method
            locomotion.Tick(Time.fixedDeltaTime);
            
            // Should have desired velocity
            Assert.Greater(locomotion.DesiredVelocity.magnitude, 0f);
            Assert.AreEqual(1f, locomotion.DesiredVelocity.normalized.x, 0.1f);
            
            Object.DestroyImmediate(gameObj);
        }
        
        [Test]
        public void ExtendedInputSource_HandlesAllInputTypes()
        {
            var inputActions = new MOBA.Input.InputSystem_Actions();
            var inputSource = new MOBA.Input.UnityInputSource(inputActions);
            
            // Test that all new input methods exist and don't throw
            Assert.DoesNotThrow(() => inputSource.IsToggleChannelPressed());
            Assert.DoesNotThrow(() => inputSource.IsSimulateDeathPressed());
            Assert.DoesNotThrow(() => inputSource.GetArrowKeyVector());
            Assert.DoesNotThrow(() => inputSource.GetScrollWheelDelta());
            Assert.DoesNotThrow(() => inputSource.IsKeyPressed(KeyCode.E));
        }
        
        [Test]
        public void UnifiedLocomotionController_HasFixedInitialization()
        {
            var gameObj = new GameObject("TestPlayer");
            var locomotion = gameObj.AddComponent<UnifiedLocomotionController>();
            
            // Should not throw null reference exceptions during initialization
            Assert.DoesNotThrow(() => {
                locomotion.Initialize(testContext, testInput);
            });
            
            Assert.IsNotNull(locomotion);
            
            Object.DestroyImmediate(gameObj);
        }
        
        [Test]
        public void SystemIntegration_AllControllersWorkTogether()
        {
            var gameObj = new GameObject("TestPlayer");
            var locomotion = gameObj.AddComponent<UnifiedLocomotionController>();
            var abilityController = new EnhancedAbilityController(testContext);
            var scoring = new ScoringController(testContext);
            
            // Initialize all systems
            locomotion.Initialize(testContext, testInput);
            
            // Test that they can all update without errors
            Assert.DoesNotThrow(() => {
                locomotion.Tick(0.1f);
                abilityController.Update(0.1f);
                scoring.Update(0.1f);
            });
            
            // Test state consistency
            Assert.IsTrue(abilityController.IsIdle);
            Assert.AreEqual(Vector3.zero, locomotion.DesiredVelocity);
            
            Object.DestroyImmediate(gameObj);
        }
        
        // Helper class for testing input
        private class TestInputSource : IInputSource
        {
            private Vector2 moveInput = Vector2.zero;
            private bool jumpPressed = false;
            
            public void SetMoveInput(Vector2 input) => moveInput = input;
            public void SetJumpPressed(bool pressed) => jumpPressed = pressed;
            
            public float GetHorizontal() => moveInput.x;
            public float GetVertical() => moveInput.y;
            public Vector2 GetMoveVector() => moveInput;
            public bool IsJumpPressed() => jumpPressed;
            
            // Stub implementations for all other methods
            public float GetAxis(string axisName) => 0f;
            public bool GetButtonDown(string buttonName) => false;
            public bool GetButton(string buttonName) => false;
            public bool GetButtonUp(string buttonName) => false;
            public bool IsAbility1Pressed() => false;
            public bool IsAbility2Pressed() => false;
            public bool IsUltimatePressed() => false;
            public bool IsScoringPressed() => false;
            public bool IsTestAddPointsPressed() => false;
            public bool IsTestDamagePressed() => false;
            public bool IsCameraTogglePressed() => false;
            public bool IsFreePanPressed() => false;
            public bool IsToggleChannelPressed() => false;
            public bool IsSimulateDeathPressed() => false;
            public bool IsHealPressed() => false;
            public bool IsResetPressed() => false;
            public bool IsArrowUpPressed() => false;
            public bool IsArrowDownPressed() => false;
            public bool IsArrowLeftPressed() => false;
            public bool IsArrowRightPressed() => false;
            public Vector2 GetArrowKeyVector() => Vector2.zero;
            public Vector2 GetMousePosition() => Vector2.zero;
            public Vector2 GetMouseDelta() => Vector2.zero;
            public bool IsMouseButtonDown(int button) => false;
            public bool IsMouseButtonUp(int button) => false;
            public bool IsRightMouseDragActive() => false;
            public float GetScrollWheelDelta() => 0f;
            public bool HasInputThisFrame() => moveInput.magnitude > 0f || jumpPressed;
            public float GetInputMagnitude() => moveInput.magnitude;
            public bool IsKeyPressed(KeyCode key) => false;
            public bool IsKeyHeld(KeyCode key) => false;
            public bool IsKeyReleased(KeyCode key) => false;
        }
    }
}
