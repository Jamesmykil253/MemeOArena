using System;
using System.Collections.Generic;
using UnityEngine;
using MOBA.Core;
using MOBA.Networking;
using MOBA.Telemetry;

namespace MOBA.Input
{
    /// <summary>
    /// Enhanced input manager with buffering, aim assist, and networking integration.
    /// Provides deterministic input processing for multiplayer gameplay.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private float inputBufferTime = 0.1f; // 100ms input buffer
        [SerializeField] private float aimAssistRadius = 2f;
        [SerializeField] private float aimAssistStrength = 0.5f;
        [SerializeField] private bool enableInputSmoothing = true;
        [SerializeField] private float smoothingFactor = 0.1f;
        
        [Header("Input Validation")]
        [SerializeField] private float maxInputMagnitude = 1f;
        [SerializeField] private bool validateInputs = true;
        [SerializeField] private float inputDeadzone = 0.1f;
        
        // Input state
        private readonly Queue<BufferedInput> inputBuffer = new Queue<BufferedInput>();
        private readonly Dictionary<string, float> buttonStates = new Dictionary<string, float>();
        private Vector2 currentMoveInput = Vector2.zero;
        private Vector2 smoothedMoveInput = Vector2.zero;
        private uint inputSequence = 0;
        
        // Input events
        public static event Action<InputCmd> OnInputGenerated;
        public static event Action<string> OnButtonPressed; // button name
        public static event Action<string> OnButtonReleased; // button name
        public static event Action<Vector2> OnMovementInput; // movement vector
        
        private IInputSource inputSource;
        private TickManager tickManager;
        private UnityEngine.Camera playerCamera;
        
        public Vector2 CurrentMoveInput => currentMoveInput;
        public Vector2 SmoothedMoveInput => smoothedMoveInput;
        public bool HasBufferedInputs => inputBuffer.Count > 0;
        
        private void Awake()
        {
            tickManager = FindFirstObjectByType<TickManager>();
            playerCamera = UnityEngine.Camera.main;
            
            InitializeButtonStates();
        }
        
        private void Start()
        {
            // Subscribe to tick events
            if (tickManager != null)
            {
                TickManager.OnTick += OnTick;
            }
            
            GameLogger.LogGameplayEvent(0, "INPUT", "INIT", "Input manager initialized");
        }
        
        private void InitializeButtonStates()
        {
            // Initialize all tracked button states
            string[] buttons = { "Jump", "Ability1", "Ability2", "Ultimate", "Scoring" };
            foreach (string button in buttons)
            {
                buttonStates[button] = 0f;
            }
        }
        
        /// <summary>
        /// Set the input source for this manager
        /// </summary>
        public void SetInputSource(IInputSource source)
        {
            inputSource = source;
            
            GameLogger.LogGameplayEvent(tickManager?.CurrentTick ?? 0, "INPUT", "SOURCE_SET", 
                source?.GetType().Name ?? "null");
        }
        
        private void Update()
        {
            if (inputSource == null) return;
            
            // Process raw input
            ProcessRawInput();
            
            // Update input smoothing
            if (enableInputSmoothing)
            {
                UpdateInputSmoothing();
            }
            
            // Process buffered inputs
            ProcessBufferedInputs();
        }
        
        private void OnTick(uint tick)
        {
            // Generate input command for this tick
            GenerateInputCommand(tick);
        }
        
        private void ProcessRawInput()
        {
            // Get movement input
            float horizontal = inputSource.GetHorizontal();
            float vertical = inputSource.GetVertical();
            
            Vector2 rawInput = new Vector2(horizontal, vertical);
            
            // Apply deadzone
            if (rawInput.magnitude < inputDeadzone)
            {
                rawInput = Vector2.zero;
            }
            
            // Validate and clamp input
            if (validateInputs)
            {
                rawInput = ValidateInput(rawInput);
            }
            
            // Update current input
            currentMoveInput = rawInput;
            
            // Check for button state changes
            ProcessButtonInputs();
            
            // Fire movement event if input changed
            OnMovementInput?.Invoke(currentMoveInput);
        }
        
        private void ProcessButtonInputs()
        {
            // Check all button states
            CheckButton("Jump", inputSource.IsJumpPressed());
            CheckButton("Ability1", inputSource.GetButtonDown("Fire1"));
            CheckButton("Ability2", inputSource.GetButtonDown("Fire2"));
            CheckButton("Ultimate", inputSource.GetButtonDown("Fire3"));
            CheckButton("Scoring", inputSource.GetButtonDown("Submit"));
        }
        
        private void CheckButton(string buttonName, bool isPressed)
        {
            float currentTime = Time.time;
            bool wasPressed = buttonStates.ContainsKey(buttonName) && 
                             (currentTime - buttonStates[buttonName]) < inputBufferTime;
            
            if (isPressed && !wasPressed)
            {
                // Button pressed
                buttonStates[buttonName] = currentTime;
                BufferInput(buttonName, true);
                OnButtonPressed?.Invoke(buttonName);
                
                GameLogger.LogGameplayEvent(tickManager.CurrentTick, "INPUT", "BUTTON_PRESS", buttonName);
            }
            else if (!isPressed && wasPressed)
            {
                // Button released
                OnButtonReleased?.Invoke(buttonName);
            }
        }
        
        private void UpdateInputSmoothing()
        {
            // Smooth movement input over time
            smoothedMoveInput = Vector2.Lerp(smoothedMoveInput, currentMoveInput, 
                                           smoothingFactor * Time.deltaTime * 60f);
        }
        
        private Vector2 ValidateInput(Vector2 input)
        {
            // Clamp input magnitude
            if (input.magnitude > maxInputMagnitude)
            {
                input = input.normalized * maxInputMagnitude;
            }
            
            // Apply aim assist if applicable
            if (aimAssistRadius > 0f && input.magnitude > 0f)
            {
                input = ApplyAimAssist(input);
            }
            
            return input;
        }
        
        private Vector2 ApplyAimAssist(Vector2 input)
        {
            if (playerCamera == null) return input;
            
            // Find nearby targets for aim assist
            Vector3 worldInput = playerCamera.transform.TransformDirection(new Vector3(input.x, 0f, input.y));
            Vector3 playerPos = playerCamera.transform.position;
            
            // Find closest target within assist radius
            Collider[] targets = UnityEngine.Physics.OverlapSphere(playerPos, aimAssistRadius);
            Collider closestTarget = null;
            float closestDistance = float.MaxValue;
            
            foreach (var target in targets)
            {
                if (target.CompareTag("Enemy") || target.CompareTag("Target"))
                {
                    float distance = Vector3.Distance(playerPos, target.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = target;
                    }
                }
            }
            
            // Apply aim assist toward closest target
            if (closestTarget != null)
            {
                Vector3 targetDirection = (closestTarget.transform.position - playerPos).normalized;
                Vector3 assistedDirection = Vector3.Slerp(worldInput.normalized, targetDirection, aimAssistStrength);
                
                Vector2 assistedInput = new Vector2(assistedDirection.x, assistedDirection.z);
                assistedInput = assistedInput.normalized * input.magnitude;
                
                GameMetrics.Instance.RecordMetric("input_aim_assist_applied", 1);
                
                return assistedInput;
            }
            
            return input;
        }
        
        private void BufferInput(string buttonName, bool pressed)
        {
            // Add input to buffer for processing
            BufferedInput bufferedInput = new BufferedInput
            {
                buttonName = buttonName,
                pressed = pressed,
                timestamp = Time.time,
                tick = tickManager.CurrentTick
            };
            
            inputBuffer.Enqueue(bufferedInput);
            
            // Limit buffer size
            while (inputBuffer.Count > 100)
            {
                inputBuffer.Dequeue();
            }
        }
        
        private void ProcessBufferedInputs()
        {
            float currentTime = Time.time;
            
            // Remove expired inputs from buffer
            while (inputBuffer.Count > 0)
            {
                BufferedInput input = inputBuffer.Peek();
                if (currentTime - input.timestamp > inputBufferTime)
                {
                    inputBuffer.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }
        
        private void GenerateInputCommand(uint tick)
        {
            inputSequence++;
            
            // Check buffered button states
            bool jumpPressed = IsButtonBuffered("Jump");
            bool ability1Pressed = IsButtonBuffered("Ability1");
            bool ability2Pressed = IsButtonBuffered("Ability2");
            bool ultimatePressed = IsButtonBuffered("Ultimate");
            bool scoringPressed = IsButtonBuffered("Scoring");
            
            // Create input command
            Vector2 inputToUse = enableInputSmoothing ? smoothedMoveInput : currentMoveInput;
            InputCmd inputCmd = new InputCmd(
                inputSequence,
                inputToUse,
                jumpPressed,
                ability1Pressed,
                ability2Pressed,
                ultimatePressed,
                scoringPressed
            );
            
            // Fire input event
            OnInputGenerated?.Invoke(inputCmd);
            
            // Log significant inputs
            if (inputToUse.magnitude > 0.1f || jumpPressed || ability1Pressed || ability2Pressed || ultimatePressed)
            {
                GameLogger.LogGameplayEvent(tick, "INPUT", "CMD_GENERATED", 
                    $"Seq:{inputSequence} Move:{inputToUse} Buttons:{GetButtonString(inputCmd)}");
            }
            
            GameMetrics.Instance.RecordMetric("input_commands_generated", 1);
        }
        
        private bool IsButtonBuffered(string buttonName)
        {
            float currentTime = Time.time;
            foreach (var input in inputBuffer)
            {
                if (input.buttonName == buttonName && input.pressed && 
                    (currentTime - input.timestamp) <= inputBufferTime)
                {
                    return true;
                }
            }
            return false;
        }
        
        private string GetButtonString(InputCmd cmd)
        {
            List<string> pressedButtons = new List<string>();
            if (cmd.jumpPressed) pressedButtons.Add("J");
            if (cmd.ability1Pressed) pressedButtons.Add("A1");
            if (cmd.ability2Pressed) pressedButtons.Add("A2");
            if (cmd.ultimatePressed) pressedButtons.Add("U");
            if (cmd.scoringPressed) pressedButtons.Add("S");
            return string.Join(",", pressedButtons);
        }
        
        /// <summary>
        /// Check if a button is currently pressed (including buffer)
        /// </summary>
        public bool IsButtonPressed(string buttonName)
        {
            return IsButtonBuffered(buttonName);
        }
        
        /// <summary>
        /// Get current movement input with optional smoothing
        /// </summary>
        public Vector2 GetMovementInput(bool useSmoothing = true)
        {
            return useSmoothing && enableInputSmoothing ? smoothedMoveInput : currentMoveInput;
        }
        
        /// <summary>
        /// Clear all buffered inputs (useful for state transitions)
        /// </summary>
        public void ClearInputBuffer()
        {
            inputBuffer.Clear();
            GameLogger.LogGameplayEvent(tickManager.CurrentTick, "INPUT", "BUFFER_CLEARED", "");
        }
        
        /// <summary>
        /// Enable or disable input processing
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            this.enabled = enabled;
            if (!enabled)
            {
                currentMoveInput = Vector2.zero;
                smoothedMoveInput = Vector2.zero;
                ClearInputBuffer();
            }
        }
        
        private void OnDestroy()
        {
            if (tickManager != null)
            {
                TickManager.OnTick -= OnTick;
            }
        }
        
        // Debug visualization
        private void OnDrawGizmos()
        {
            if (playerCamera != null && aimAssistRadius > 0f)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(playerCamera.transform.position, aimAssistRadius);
            }
        }
    }
    
    /// <summary>
    /// Represents a buffered input for processing
    /// </summary>
    [Serializable]
    public struct BufferedInput
    {
        public string buttonName;
        public bool pressed;
        public float timestamp;
        public uint tick;
    }
}
