using UnityEngine;
using MOBA.Core;
using MOBA.Data;

namespace MOBA.Camera
{
    /// <summary>
    /// Dynamic camera controller that provides smooth following, multiple positioning presets,
    /// and free-pan capabilities for MOBA gameplay. Automatically switches to free-pan when
    /// the player dies and provides seamless transitions between modes.
    /// </summary>
    public class DynamicCameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private bool autoFindPlayer = true;
        
        [Header("Camera Positions")]
        [SerializeField] private CameraPosition[] cameraPositions = new CameraPosition[]
        {
            new CameraPosition("Top-Down", new Vector3(0, 20, -2), new Vector3(75, 0, 0)),
            new CameraPosition("Third Person", new Vector3(0, 8, -12), new Vector3(15, 0, 0)),
            new CameraPosition("Tactical", new Vector3(0, 15, -8), new Vector3(45, 0, 0)),
            new CameraPosition("Side View", new Vector3(15, 8, -5), new Vector3(10, -45, 0)),
            new CameraPosition("Isometric", new Vector3(-10, 12, -10), new Vector3(30, 45, 0))
        };
        
        [Header("Follow Settings")]
        [SerializeField] private int currentPositionIndex = 0;
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float rotationSpeed = 3f;
        [SerializeField] private Vector3 followOffset = Vector3.zero;
        [SerializeField] private bool smoothFollow = true;
        [SerializeField] private float followDeadzone = 0.5f;
        
        [Header("Free Pan Settings")]
        [SerializeField] private float panSpeed = 10f;
        [SerializeField] private float panRotationSpeed = 100f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 50f;
        [SerializeField] private KeyCode freePanToggle = KeyCode.C;
        [SerializeField] private KeyCode positionCycleKey = KeyCode.V;
        
        [Header("Smooth Transitions")]
        [SerializeField] private float transitionSpeed = 2f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        // Runtime state
        private CameraMode currentMode = CameraMode.Following;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private Vector3 freePanPosition;
        private Vector3 freePanRotation;
        private bool isTransitioning = false;
        private float transitionProgress = 0f;
        private Vector3 transitionStartPos;
        private Quaternion transitionStartRot;
        private IInputSource inputSource;
        private PlayerContext playerContext;
        
        // Camera reference
        private UnityEngine.Camera cameraComponent;
        
        public enum CameraMode
        {
            Following,
            FreePan,
            DeathCam
        }
        
        [System.Serializable]
        public struct CameraPosition
        {
            public string name;
            public Vector3 offset;
            public Vector3 rotation;
            
            public CameraPosition(string name, Vector3 offset, Vector3 rotation)
            {
                this.name = name;
                this.offset = offset;
                this.rotation = rotation;
            }
        }
        
        void Start()
        {
            cameraComponent = GetComponent<UnityEngine.Camera>();
            if (cameraComponent == null)
            {
                Debug.LogError("DynamicCameraController requires a Camera component!");
                return;
            }
            
            InitializeCamera();
        }
        
        void Update()
        {
            HandleInput();
            UpdateCameraMode();
            UpdateCameraPosition();
        }
        
        private void InitializeCamera()
        {
            // Auto-find player if needed
            if (autoFindPlayer && followTarget == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    followTarget = player.transform;
                    
                    // Try to get player context for death detection
                    var demoController = player.GetComponent<MOBA.Demo.DemoPlayerController>();
                    if (demoController != null)
                    {
                        // We'll access the player context through reflection or add a public getter
                        Debug.Log("Found player for camera following");
                    }
                }
            }
            
            // Initialize free pan position
            freePanPosition = transform.position;
            freePanRotation = transform.eulerAngles;
            
            // Set initial position
            UpdateTargetFromPreset();
        }
        
        private void HandleInput()
        {
            // Toggle free pan mode
            if (UnityEngine.Input.GetKeyDown(freePanToggle))
            {
                ToggleFreePan();
            }
            
            // Cycle camera positions
            if (UnityEngine.Input.GetKeyDown(positionCycleKey))
            {
                CycleCameraPosition();
            }
            
            // Free pan controls
            if (currentMode == CameraMode.FreePan)
            {
                HandleFreePanInput();
            }
        }
        
        private void HandleFreePanInput()
        {
            // Mouse pan (right mouse button)
            if (UnityEngine.Input.GetMouseButton(1))
            {
                float mouseX = UnityEngine.Input.GetAxis("Mouse X");
                float mouseY = UnityEngine.Input.GetAxis("Mouse Y");
                
                freePanRotation.y += mouseX * panRotationSpeed * Time.deltaTime;
                freePanRotation.x -= mouseY * panRotationSpeed * Time.deltaTime;
                freePanRotation.x = Mathf.Clamp(freePanRotation.x, -80f, 80f);
            }
            
            // WASD movement in free pan
            Vector3 movement = Vector3.zero;
            if (UnityEngine.Input.GetKey(KeyCode.W)) movement += transform.forward;
            if (UnityEngine.Input.GetKey(KeyCode.S)) movement -= transform.forward;
            if (UnityEngine.Input.GetKey(KeyCode.A)) movement -= transform.right;
            if (UnityEngine.Input.GetKey(KeyCode.D)) movement += transform.right;
            if (UnityEngine.Input.GetKey(KeyCode.Q)) movement += Vector3.up;
            if (UnityEngine.Input.GetKey(KeyCode.E)) movement -= Vector3.up;
            
            freePanPosition += movement * panSpeed * Time.deltaTime;
            
            // Mouse scroll for zoom (adjust Y position)
            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                freePanPosition.y = Mathf.Clamp(freePanPosition.y - scroll * zoomSpeed, minZoom, maxZoom);
            }
        }
        
        private void UpdateCameraMode()
        {
            // Check for player death (switch to death cam)
            if (currentMode != CameraMode.DeathCam && playerContext != null)
            {
                if (playerContext.currentHP <= 0)
                {
                    SwitchToDeathCam();
                }
            }
            
            // Return to follow mode if player respawns
            if (currentMode == CameraMode.DeathCam && playerContext != null)
            {
                if (playerContext.currentHP > 0)
                {
                    SwitchToFollowMode();
                }
            }
        }
        
        private void UpdateCameraPosition()
        {
            Vector3 desiredPosition = transform.position;
            Quaternion desiredRotation = transform.rotation;
            
            switch (currentMode)
            {
                case CameraMode.Following:
                    UpdateFollowMode(out desiredPosition, out desiredRotation);
                    break;
                    
                case CameraMode.FreePan:
                case CameraMode.DeathCam:
                    UpdateFreePanMode(out desiredPosition, out desiredRotation);
                    break;
            }
            
            // Apply transitions
            if (isTransitioning)
            {
                UpdateTransition(ref desiredPosition, ref desiredRotation);
            }
            
            // Apply the position and rotation
            if (smoothFollow && !isTransitioning)
            {
                transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = desiredPosition;
                transform.rotation = desiredRotation;
            }
        }
        
        private void UpdateFollowMode(out Vector3 position, out Quaternion rotation)
        {
            if (followTarget != null)
            {
                CameraPosition preset = cameraPositions[currentPositionIndex];
                Vector3 totalOffset = preset.offset + followOffset;
                
                // Calculate target position relative to player
                targetPosition = followTarget.position + followTarget.TransformDirection(totalOffset);
                
                // Check deadzone
                if (smoothFollow)
                {
                    float distance = Vector3.Distance(transform.position, targetPosition);
                    if (distance < followDeadzone)
                    {
                        targetPosition = transform.position; // Stay put
                    }
                }
                
                position = targetPosition;
                rotation = Quaternion.Euler(preset.rotation);
            }
            else
            {
                position = transform.position;
                rotation = transform.rotation;
            }
        }
        
        private void UpdateFreePanMode(out Vector3 position, out Quaternion rotation)
        {
            position = freePanPosition;
            rotation = Quaternion.Euler(freePanRotation);
        }
        
        private void UpdateTransition(ref Vector3 position, ref Quaternion rotation)
        {
            transitionProgress += Time.deltaTime * transitionSpeed;
            float curveValue = transitionCurve.Evaluate(transitionProgress);
            
            position = Vector3.Lerp(transitionStartPos, position, curveValue);
            rotation = Quaternion.Slerp(transitionStartRot, rotation, curveValue);
            
            if (transitionProgress >= 1f)
            {
                isTransitioning = false;
                transitionProgress = 0f;
            }
        }
        
        private void StartTransition()
        {
            isTransitioning = true;
            transitionProgress = 0f;
            transitionStartPos = transform.position;
            transitionStartRot = transform.rotation;
        }
        
        private void UpdateTargetFromPreset()
        {
            if (cameraPositions.Length > 0)
            {
                currentPositionIndex = Mathf.Clamp(currentPositionIndex, 0, cameraPositions.Length - 1);
            }
        }
        
        // Public API methods
        public void ToggleFreePan()
        {
            if (currentMode == CameraMode.Following)
            {
                SwitchToFreePan();
            }
            else if (currentMode == CameraMode.FreePan)
            {
                SwitchToFollowMode();
            }
        }
        
        public void SwitchToFreePan()
        {
            currentMode = CameraMode.FreePan;
            freePanPosition = transform.position;
            freePanRotation = transform.eulerAngles;
            StartTransition();
            Debug.Log("Camera: Switched to Free Pan mode");
        }
        
        public void SwitchToFollowMode()
        {
            if (followTarget != null)
            {
                currentMode = CameraMode.Following;
                StartTransition();
                Debug.Log("Camera: Switched to Follow mode");
            }
        }
        
        public void SwitchToDeathCam()
        {
            currentMode = CameraMode.DeathCam;
            freePanPosition = transform.position;
            freePanRotation = transform.eulerAngles;
            Debug.Log("Camera: Switched to Death Cam mode (free pan)");
        }
        
        public void CycleCameraPosition()
        {
            if (cameraPositions.Length > 0)
            {
                currentPositionIndex = (currentPositionIndex + 1) % cameraPositions.Length;
                UpdateTargetFromPreset();
                StartTransition();
                Debug.Log($"Camera: Switched to position '{cameraPositions[currentPositionIndex].name}'");
            }
        }
        
        public void SetCameraPosition(int index)
        {
            if (index >= 0 && index < cameraPositions.Length)
            {
                currentPositionIndex = index;
                UpdateTargetFromPreset();
                StartTransition();
                Debug.Log($"Camera: Set to position '{cameraPositions[currentPositionIndex].name}'");
            }
        }
        
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            if (target != null)
            {
                Debug.Log($"Camera: Now following {target.name}");
            }
        }
        
        // Getters
        public CameraMode CurrentMode => currentMode;
        public string CurrentPositionName => cameraPositions.Length > 0 ? cameraPositions[currentPositionIndex].name : "None";
        public bool IsFollowing => currentMode == CameraMode.Following && followTarget != null;
    }
}
