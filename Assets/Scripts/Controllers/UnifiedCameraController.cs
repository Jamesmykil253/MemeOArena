using UnityEngine;
using UnityEngine.InputSystem;
using MOBA.Core;

namespace MOBA.Controllers
{
    /// <summary>
    /// Unified camera controller that combines the best features from all camera scripts.
    /// Provides smooth following, hold-to-pan functionality, multiple camera modes,
    /// and advanced features for MOBA gameplay.
    /// </summary>
    public class UnifiedCameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        public Transform target;
        public bool followTarget = true;
        
        [Header("Camera Positioning")]
        public CameraMode cameraMode = CameraMode.ThirdPerson;
        public Vector3 offset = new Vector3(0f, 8f, -6f);
        public float followSpeed = 5f;
        public float rotationSpeed = 3f;
        public float followDeadzone = 0.5f;
        
        [Header("Camera Presets")]
        [SerializeField] private CameraPreset[] cameraPresets = new CameraPreset[]
        {
            new CameraPreset("Top-Down", new Vector3(0, 20, -2), new Vector3(75, 0, 0)),
            new CameraPreset("Third Person", new Vector3(0, 8, -12), new Vector3(15, 0, 0)),
            new CameraPreset("Tactical", new Vector3(0, 15, -8), new Vector3(45, 0, 0)),
            new CameraPreset("Isometric", new Vector3(-10, 12, -10), new Vector3(30, 45, 0)),
            new CameraPreset("Action", new Vector3(0, 2, -3), new Vector3(5, 0, 0))
        };
        
        [Header("Pan Settings")]
        public KeyCode panKey = KeyCode.V;
        public float panSpeed = 10f;
        public float returnToPlayerSpeed = 8f;
        public float zoomSpeed = 5f;
        public float minZoom = 3f;
        public float maxZoom = 20f;
        
        [Header("Advanced Settings")]
        [SerializeField] private bool usePresetSystem = false;
        [SerializeField] private int currentPresetIndex = 1; // Default to Third Person
        [SerializeField] private bool smoothTransitions = true;
        [SerializeField] private float transitionSpeed = 2f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Player Death State")]
        public bool playerIsDead = false;
        
        // Private state
        private UnityEngine.Camera cam;
        private Vector3 lastTargetPosition;
        private bool isPanningHeld = false;
        private bool wasFollowingBeforePan = true;
        private Vector3 panStartPosition;
        
        // Transition system
        private bool isTransitioning = false;
        private float transitionProgress = 0f;
        private Vector3 transitionStartPos;
        private Quaternion transitionStartRot;
        private Vector3 transitionTargetPos;
        private Quaternion transitionTargetRot;
        
        [System.Serializable]
        public struct CameraPreset
        {
            public string name;
            public Vector3 offset;
            public Vector3 rotation;
            
            public CameraPreset(string name, Vector3 offset, Vector3 rotation)
            {
                this.name = name;
                this.offset = offset;
                this.rotation = rotation;
            }
        }
        
        public enum CameraMode
        {
            TopDown,        // MOBA-style top-down view
            ThirdPerson,    // Behind and above player
            Isometric,      // Fixed angle isometric view
            Action,         // Close action camera
            Tactical        // Strategic overview
        }
        
        void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            if (cam == null)
            {
                cam = UnityEngine.Camera.main;
                Debug.LogWarning("UnifiedCameraController: No Camera component found, using Camera.main");
            }
            
            // Auto-find player if no target set
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                    Debug.Log($"[UnifiedCameraController] Auto-assigned target: {player.name}");
                }
            }
        }
        
        void LateUpdate()
        {
            HandleInput();
            UpdateCameraLogic();
        }
        
        private void HandleInput()
        {
            HandlePanInput();
            HandlePresetCycling();
            HandleModeToggle();
        }
        
        private void HandlePanInput()
        {
            // Fixed: Always use V key for panning, never spacebar
            bool panKeyPressed = Keyboard.current != null && Keyboard.current[Key.V].isPressed;
            
            if (panKeyPressed && !isPanningHeld)
            {
                StartPanning();
            }
            else if (!panKeyPressed && isPanningHeld && !playerIsDead)
            {
                StopPanning();
            }
        }
        
        private void HandlePresetCycling()
        {
            if (usePresetSystem && Keyboard.current != null && Keyboard.current[Key.C].wasPressedThisFrame)
            {
                CyclePreset();
            }
        }
        
        private void HandleModeToggle()
        {
            if (!usePresetSystem && Keyboard.current != null && Keyboard.current[Key.C].wasPressedThisFrame)
            {
                CycleCameraMode();
            }
        }
        
        private void UpdateCameraLogic()
        {
            if (isTransitioning)
            {
                UpdateTransition();
            }
            else if (isPanningHeld || (playerIsDead && !followTarget))
            {
                HandlePanMode();
            }
            else if (followTarget && target != null)
            {
                FollowTarget();
            }
        }
        
        private void StartPanning()
        {
            isPanningHeld = true;
            wasFollowingBeforePan = followTarget;
            panStartPosition = transform.position;
            followTarget = false;
        }
        
        private void StopPanning()
        {
            isPanningHeld = false;
            followTarget = wasFollowingBeforePan;
            
            if (followTarget && target != null)
            {
                if (smoothTransitions)
                {
                    StartTransitionToTarget();
                }
                else
                {
                    StartCoroutine(ReturnToPlayer());
                }
            }
        }
        
        private System.Collections.IEnumerator ReturnToPlayer()
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = GetDesiredPosition();
            float journey = 0f;
            float journeyTime = 1f;
            
            while (journey <= journeyTime && followTarget)
            {
                journey += Time.deltaTime;
                float fractionOfJourney = journey / journeyTime;
                fractionOfJourney = Mathf.SmoothStep(0f, 1f, fractionOfJourney);
                
                transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
                yield return null;
                
                targetPos = GetDesiredPosition();
            }
        }
        
        private void FollowTarget()
        {
            Vector3 desiredPosition = GetDesiredPosition();
            
            // Check deadzone for smooth following
            if (followDeadzone > 0)
            {
                float distance = Vector3.Distance(transform.position, desiredPosition);
                if (distance < followDeadzone)
                {
                    return; // Stay put, we're close enough
                }
            }
            
            Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            transform.position = smoothPosition;
            
            // Handle rotation based on camera mode or preset
            Quaternion desiredRotation = GetDesiredRotation();
            if (desiredRotation != transform.rotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        private Vector3 GetDesiredPosition()
        {
            if (target == null) return transform.position;
            
            Vector3 targetPos = target.position;
            
            if (usePresetSystem && cameraPresets.Length > 0)
            {
                // Use preset system
                int index = Mathf.Clamp(currentPresetIndex, 0, cameraPresets.Length - 1);
                return targetPos + cameraPresets[index].offset;
            }
            else
            {
                // Use traditional camera modes
                switch (cameraMode)
                {
                    case CameraMode.TopDown:
                        return targetPos + new Vector3(0f, 12f, 0f);
                        
                    case CameraMode.ThirdPerson:
                        return targetPos + offset;
                        
                    case CameraMode.Isometric:
                        return targetPos + new Vector3(5f, 10f, -5f);
                        
                    case CameraMode.Action:
                        return targetPos + new Vector3(0f, 2f, -3f);
                        
                    case CameraMode.Tactical:
                        return targetPos + new Vector3(0f, 15f, -8f);
                        
                    default:
                        return targetPos + offset;
                }
            }
        }
        
        private Quaternion GetDesiredRotation()
        {
            if (usePresetSystem && cameraPresets.Length > 0)
            {
                int index = Mathf.Clamp(currentPresetIndex, 0, cameraPresets.Length - 1);
                return Quaternion.Euler(cameraPresets[index].rotation);
            }
            else
            {
                // Dynamic rotation based on mode
                if (cameraMode == CameraMode.ThirdPerson || cameraMode == CameraMode.Action)
                {
                    if (target != null)
                    {
                        Vector3 lookDirection = target.position - transform.position;
                        if (lookDirection != Vector3.zero)
                        {
                            return Quaternion.LookRotation(lookDirection);
                        }
                    }
                }
                
                // Default rotation based on mode
                switch (cameraMode)
                {
                    case CameraMode.TopDown:
                        return Quaternion.Euler(90f, 0f, 0f);
                    case CameraMode.Isometric:
                        return Quaternion.Euler(30f, 45f, 0f);
                    case CameraMode.Tactical:
                        return Quaternion.Euler(45f, 0f, 0f);
                    default:
                        return transform.rotation;
                }
            }
        }
        
        private void HandlePanMode()
        {
            float horizontal = 0f;
            float vertical = 0f;
            
            // Arrow keys for camera panning
            if (Keyboard.current != null)
            {
                if (Keyboard.current[Key.LeftArrow].isPressed) horizontal = -1f;
                if (Keyboard.current[Key.RightArrow].isPressed) horizontal = 1f;
                if (Keyboard.current[Key.UpArrow].isPressed) vertical = 1f;
                if (Keyboard.current[Key.DownArrow].isPressed) vertical = -1f;
            }
            
            // Mouse drag for panning
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue() / Screen.height;
                horizontal = mouseDelta.x * 200f;
                vertical = mouseDelta.y * 200f;
            }
            
            Vector3 panDirection = new Vector3(horizontal, 0f, vertical);
            transform.position += panDirection * panSpeed * Time.deltaTime;
            
            // Mouse scroll for zoom
            float scroll = 0f;
            if (Mouse.current != null)
            {
                scroll = Mouse.current.scroll.ReadValue().y / 120f;
            }
            
            if (scroll != 0f)
            {
                Vector3 forward = transform.forward * scroll * zoomSpeed;
                Vector3 newPosition = transform.position + forward;
                
                float distanceFromGround = newPosition.y;
                if (distanceFromGround >= minZoom && distanceFromGround <= maxZoom)
                {
                    transform.position = newPosition;
                }
            }
        }
        
        private void CyclePreset()
        {
            if (cameraPresets.Length == 0) return;
            
            currentPresetIndex = (currentPresetIndex + 1) % cameraPresets.Length;
            
            if (smoothTransitions)
            {
                StartTransitionToTarget();
            }
            
            Debug.Log($"Camera: Switched to preset '{cameraPresets[currentPresetIndex].name}'");
        }
        
        private void CycleCameraMode()
        {
            cameraMode = (CameraMode)(((int)cameraMode + 1) % System.Enum.GetValues(typeof(CameraMode)).Length);
            
            if (smoothTransitions)
            {
                StartTransitionToTarget();
            }
            
            Debug.Log($"Camera: Switched to mode '{cameraMode}'");
        }
        
        private void StartTransitionToTarget()
        {
            isTransitioning = true;
            transitionProgress = 0f;
            transitionStartPos = transform.position;
            transitionStartRot = transform.rotation;
            transitionTargetPos = GetDesiredPosition();
            transitionTargetRot = GetDesiredRotation();
        }
        
        private void UpdateTransition()
        {
            transitionProgress += Time.deltaTime * transitionSpeed;
            float curveValue = transitionCurve.Evaluate(transitionProgress);
            
            transform.position = Vector3.Lerp(transitionStartPos, transitionTargetPos, curveValue);
            transform.rotation = Quaternion.Slerp(transitionStartRot, transitionTargetRot, curveValue);
            
            if (transitionProgress >= 1f)
            {
                isTransitioning = false;
                transitionProgress = 0f;
            }
        }
        
        // Public API
        public void EnableDeathPanning()
        {
            playerIsDead = true;
            followTarget = false;
        }
        
        public void EnableFollowOnRespawn()
        {
            playerIsDead = false;
            isPanningHeld = false;
            followTarget = true;
            
            if (target != null)
            {
                if (smoothTransitions)
                {
                    StartTransitionToTarget();
                }
                else
                {
                    StartCoroutine(ReturnToPlayer());
                }
            }
        }
        
        public void SetCameraMode(CameraMode newMode)
        {
            cameraMode = newMode;
            if (smoothTransitions)
            {
                StartTransitionToTarget();
            }
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            lastTargetPosition = target != null ? target.position : Vector3.zero;
        }
        
        public void SetPreset(int index)
        {
            if (index >= 0 && index < cameraPresets.Length)
            {
                currentPresetIndex = index;
                usePresetSystem = true;
                
                if (smoothTransitions)
                {
                    StartTransitionToTarget();
                }
                
                Debug.Log($"Camera: Set to preset '{cameraPresets[currentPresetIndex].name}'");
            }
        }
        
        public void EnablePresetSystem(bool enable)
        {
            usePresetSystem = enable;
        }
        
        public void SetFollowDeadzone(float deadzone)
        {
            followDeadzone = Mathf.Max(0f, deadzone);
        }
        
        // Getters
        public string CurrentPresetName => usePresetSystem && cameraPresets.Length > 0 ? cameraPresets[currentPresetIndex].name : cameraMode.ToString();
        public bool IsFollowing => followTarget && target != null;
        public bool IsPanning => isPanningHeld || (playerIsDead && !followTarget);
    }
}
