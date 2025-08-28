using UnityEngine;
using MOBA.Core;

namespace MOBA.Controllers
{
    /// <summary>
    /// Dynamic camera controller that follows the player with smooth movement.
    /// Supports hold-to-pan functionality that returns to player when released.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        public Transform target;
        public bool followTarget = true;
        
        [Header("Camera Positioning")]
        public CameraMode cameraMode = CameraMode.ThirdPerson;
        public Vector3 offset = new Vector3(0f, 8f, -6f);
        public float followSpeed = 5f;
        public float rotationSpeed = 3f;
        
        [Header("Pan Settings")]
        public KeyCode panKey = KeyCode.V;
        public float panSpeed = 10f;
        public float returnToPlayerSpeed = 8f;
        public float zoomSpeed = 5f;
        public float minZoom = 3f;
        public float maxZoom = 20f;
        
        [Header("Player Death State")]
        public bool playerIsDead = false;
        
        private UnityEngine.Camera cam;
        private Vector3 lastTargetPosition;
        private bool isPanningHeld = false;
        private bool wasFollowingBeforePan = true;
        private Vector3 panStartPosition;
        
        public enum CameraMode
        {
            TopDown,        // MOBA-style top-down view
            ThirdPerson,    // Behind and above player
            Isometric,      // Fixed angle isometric view
            Action          // Close action camera
        }
        
        void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            if (cam == null)
            {
                cam = UnityEngine.Camera.main;
                Debug.LogWarning("CameraController: No Camera component found, using Camera.main");
            }
        }
        
        void LateUpdate()
        {
            HandlePanInput();
            
            if (isPanningHeld || (playerIsDead && !followTarget))
            {
                HandlePanMode();
            }
            else if (followTarget && target != null)
            {
                FollowTarget();
            }
        }
        
        private void HandlePanInput()
        {
            bool panKeyPressed = UnityEngine.Input.GetKey(panKey);
            
            if (panKeyPressed && !isPanningHeld)
            {
                // Start panning
                StartPanning();
            }
            else if (!panKeyPressed && isPanningHeld && !playerIsDead)
            {
                // Stop panning (only if player is alive)
                StopPanning();
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
            
            // Smoothly return to following the player
            if (followTarget && target != null)
            {
                StartCoroutine(ReturnToPlayer());
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
                
                // Update target position in case player moved
                targetPos = GetDesiredPosition();
            }
        }
        
        private void FollowTarget()
        {
            Vector3 desiredPosition = GetDesiredPosition();
            Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            transform.position = smoothPosition;
            
            // Handle rotation based on camera mode
            if (cameraMode == CameraMode.ThirdPerson || cameraMode == CameraMode.Action)
            {
                Vector3 lookDirection = target.position - transform.position;
                if (lookDirection != Vector3.zero)
                {
                    Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
        
        private Vector3 GetDesiredPosition()
        {
            Vector3 targetPos = target.position;
            
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
                    
                default:
                    return targetPos + offset;
            }
        }
        
        private void HandlePanMode()
        {
            // Use specific keys for camera panning to avoid conflicts with player movement
            float horizontal = 0f;
            float vertical = 0f;
            
            // Arrow keys for camera panning instead of WASD to avoid player movement conflict
            if (UnityEngine.Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
            if (UnityEngine.Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
            if (UnityEngine.Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
            if (UnityEngine.Input.GetKey(KeyCode.DownArrow)) vertical = -1f;
            
            // Alternative: Use mouse drag for panning
            if (UnityEngine.Input.GetMouseButton(1)) // Right mouse button
            {
                Vector2 mouseDelta = new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));
                horizontal = mouseDelta.x * 2f; // Amplify mouse movement
                vertical = mouseDelta.y * 2f;
            }
            
            Vector3 panDirection = new Vector3(horizontal, 0f, vertical);
            transform.position += panDirection * panSpeed * Time.deltaTime;
            
            // Mouse scroll for zoom
            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                Vector3 forward = transform.forward * scroll * zoomSpeed;
                Vector3 newPosition = transform.position + forward;
                
                // Keep within zoom bounds
                float distanceFromGround = newPosition.y;
                if (distanceFromGround >= minZoom && distanceFromGround <= maxZoom)
                {
                    transform.position = newPosition;
                }
            }
        }
        
        /// <summary>
        /// Enable camera panning (called when player dies)
        /// </summary>
        public void EnableDeathPanning()
        {
            playerIsDead = true;
            followTarget = false;
        }
        
        /// <summary>
        /// Return to following the target (called when player respawns)
        /// </summary>
        public void EnableFollowOnRespawn()
        {
            playerIsDead = false;
            isPanningHeld = false;
            followTarget = true;
            
            // Immediately start returning to player
            if (target != null)
            {
                StartCoroutine(ReturnToPlayer());
            }
        }
        
        /// <summary>
        /// Change camera positioning mode
        /// </summary>
        public void SetCameraMode(CameraMode newMode)
        {
            cameraMode = newMode;
        }
        
        /// <summary>
        /// Set the target to follow
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            lastTargetPosition = target != null ? target.position : Vector3.zero;
        }
    }
}
