using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PbwanskaasProductions.BasicThirdPersonController  
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    /// <summary>
    /// A comprehensive third-person character controller.
    /// Manages player movement, camera control, physics, animations, and a lock-on targeting system.
    /// </summary>
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Movement speed of the character in m/s")]
        public float MoveSpeed = 4.0f; 
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 8.0f; 
        [Tooltip("Movement speed of the character when targeting.")]
        public float TargetingMoveSpeed = 2.0f; 
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f; 
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f; 
        [Tooltip("Force that pulls the character to the ground to prevent jumping on ramps.")]
        public float StickingForce = 10f; 

        [Header("Ground Check")]
        [Tooltip("The offset of the ground check sphere")]
        public float GroundedOffset = -0.14f; 
        [Tooltip("The radius of the ground check sphere")]
        public float GroundedRadius = 0.28f; 
        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers; 
        public LayerMask GroundInclined; 
        private bool _isGrounded;
        private bool inclined; 
        public FootPlacement ft; 
        public RigBuilder rbuild; 
        public GameObject rigPlayer; 

        [Header("Obstacle Detection")]
        [Tooltip("Set the layers that should be considered obstacles to block the aim (walls, etc.).")]
        public LayerMask ObstacleLayers; 

        [Header("Combat Targeting")]
        public float MaxTargetDistance = 20f; 
        public GameObject TargetIconPrefab; 
        public float TargetIconHeight = 2.0f; 
        [Tooltip("How smoothly the player rotates when focusing on a target. Lower = faster.")]
        public float TargetRotationSmoothTime = 0.08f; 
        public float TargetSwitchThreshold = 0.8f; 
        [Tooltip("Vertical and horizontal adjustment of the aiming point on the target (to aim at the chest instead of the feet).")]
        public Vector3 TargetAimOffset = new Vector3(0.0f, 1.5f, 0.0f); 

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget; 
        [Tooltip("Offset for the point the camera will aim at, from the player's pivot. Adjust the Y to aim at the chest/head.")]
        public Vector3 CameraTargetOffset = new Vector3(0f, 1.5f, 0f);
        [Tooltip("The offset of the camera when the player is targeting an enemy.")]
        public Vector3 TargetingCameraOffset = new Vector3(0.5f, 1.5f, -0.5f);
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f; 
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f; 
        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f; 
        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false; 

        [Header("Camera Sensitivity")]
        [Tooltip("Camera sensitivity. Works for mouse and controller.")]
        public float CameraSensitivity = 1.0f; 

        // Private variables for internal state management
        private GameObject _instantiatedIcon; 
        private float _targetSwitchCooldown = 0.5f; 
        private float _timeSinceLastSwitch; 
        private Transform _currentTarget; 
        private float _targetRotationVelocity; 
        private float _cinemachineTargetYaw; 
        private float _cinemachineTargetPitch; 
        private float _speed; 
        private float _animationBlend; 
        private float _targetRotation = 0.0f; 
        private float _rotationVelocity; 
        private Vector3 _groundNormal; 
        private CapsuleCollider _capsuleCollider; 
        public bool running; 
        private int _animIDSpeed, _animIDGrounded, _animIDJump, _animIDFreeFall, _animIDMotionSpeed, _animIDTargeting, _animIDTargetingMoveX, _animIDTargetingMoveY; //
        public bool isTargeting; 
#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput; 
#endif
        public Animator _animator; 
        private Rigidbody _rigidbody; 
        private StarterAssetsInputs _input; 
        private GameObject _mainCamera; 
        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse =>
     Mouse.current != null && Mouse.current.wasUpdatedThisFrame;

        private void Awake()
        {
            // Get references to essential components
            if (_mainCamera == null) _mainCamera = GameObject.FindGameObjectWithTag("MainCamera"); 
            _rigidbody = GetComponent<Rigidbody>(); 
            _capsuleCollider = GetComponent<CapsuleCollider>(); 
        }

        private void Start()
        {
            // Initialize states and references at the start of the game
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y; 
            _input = GetComponent<StarterAssetsInputs>(); 
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>(); 
#endif
            AssignAnimationIDs(); 
            _timeSinceLastSwitch = _targetSwitchCooldown; 
            ft = GetComponentInChildren<FootPlacement>(); 
            rbuild = GetComponentInChildren<RigBuilder>();
            rigPlayer = GameObject.FindGameObjectWithTag("MyRig"); 
        }

        private void Update()
        {
            // Handle logic that needs to be checked every frame (non-physics)
            _timeSinceLastSwitch += Time.deltaTime; 
            HandleSprintLogic(); 
            UpdateAnimator(); 

            if (isTargeting && _currentTarget != null) 
            {
                if (!_isGrounded) { ClearTarget(); return; } 
                HandleTargetSwitching(); 
                UpdateTargetIconPosition(); 
            }

            FoccusOnTheTarget(); 

            // Reset single-frame input flags
            if (_input.sprint) _input.sprint = false; 
            if (_input.locked) _input.locked = false; 
            if(_input.dodge) _input.dodge = false;
        }

        private void FixedUpdate()
        {
            // Handle physics-related logic at a fixed time interval
            GroundedCheck();
            Move();

            if (isTargeting && _currentTarget != null) 
            {
                RotateCameraTowardsTarget(); 
            }

            // Apply gravity manually for consistent behavior
            if (!_isGrounded) 
            {
                _rigidbody.AddForce(Vector3.down * 10f, ForceMode.Force); 
            }
        }

        private void LateUpdate()
        { // Handle camera updates after all other updates to prevent jitter
            if (CinemachineCameraTarget != null)
            {
                // Use a different offset when targeting vs. not targeting
                if (isTargeting)
                {
                    // When targeting, apply the offset relative to the player's current rotation.
                    // This creates a stable "over-the-shoulder" view that rotates with the player.
                    Vector3 relativeOffset = (transform.right * TargetingCameraOffset.x) +
                                             (transform.up * TargetingCameraOffset.y) +
                                             (transform.forward * TargetingCameraOffset.z);

                    CinemachineCameraTarget.transform.position = transform.position + relativeOffset;
                }
                else
                {
                    // When not targeting, use the default camera offset and allow free rotation
                    CinemachineCameraTarget.transform.position = transform.position + CameraTargetOffset;
                    CameraRotation();
                }
            }
        }

        /// <summary>
        /// Caches animator parameter IDs for performance.
        /// </summary>
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed"); 
            _animIDGrounded = Animator.StringToHash("Grounded"); 
            _animIDJump = Animator.StringToHash("Jump"); 
            _animIDFreeFall = Animator.StringToHash("FreeFall"); 
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed"); 
            _animIDTargeting = Animator.StringToHash("Targeting"); 
            _animIDTargetingMoveX = Animator.StringToHash("TargetingMoveX"); 
            _animIDTargetingMoveY = Animator.StringToHash("TargetingMoveY"); 
        }

        /// <summary>
        /// Checks if the player is on the ground and enables/disables foot IK accordingly.
        /// </summary>
        private void GroundedCheck()
        {
            Vector3 rayStartPoint = transform.position + Vector3.up * 0.1f; 
            float raycastDistance = 0.3f;
            if (Physics.Raycast(rayStartPoint, Vector3.down, out RaycastHit hitInfo, raycastDistance, GroundLayers, QueryTriggerInteraction.Ignore)) 
            {
                _isGrounded = true; 
                _groundNormal = hitInfo.normal; 
            }
            else
            {
                _isGrounded = false; 
                _groundNormal = Vector3.up; 
            }

            // Separately check for inclined surfaces to manage foot placement IK
            if (Physics.Raycast(rayStartPoint, Vector3.down, out RaycastHit hitInfo2, raycastDistance, GroundInclined, QueryTriggerInteraction.Ignore)) 
            {
                inclined = true; ft.enabled = true; rbuild.enabled = true; rigPlayer.SetActive(true); 
            }
            else
            {
                inclined = false; ft.enabled = false; rbuild.enabled = false; rigPlayer.SetActive(false); 
            }

            // Update animator parameters
            _animator.SetBool(_animIDGrounded, _isGrounded); 
            _animator.SetBool(_animIDFreeFall, !_isGrounded); 
        }

        /// <summary>
        /// Handles the logic for toggling sprint.
        /// </summary>
        private void HandleSprintLogic()
        {
            if (_input.sprint) 
            {
                running = !running && _input.move != Vector2.zero && !isTargeting; 
            }
            if (_input.move == Vector2.zero || isTargeting || !_isGrounded) 
            {
                running = false; 
            }
        }

        /// <summary>
        /// Rotates the camera based on player input when not targeting.
        /// </summary>
        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition) 
            {
                // Adjust sensitivity based on device
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime; 
                _cinemachineTargetYaw += _input.look.x * CameraSensitivity * deltaTimeMultiplier; 
                _cinemachineTargetPitch += _input.look.y * CameraSensitivity * deltaTimeMultiplier; 
            }
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue); 
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp); 
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f); 
        }

        /// <summary>
        /// Handles all player movement, rotation, and collision.
        /// </summary>
        // In your ThirdPersonController.cs script

        private void Move()
        {
            // Set target speed based on player state (targeting, sprinting, or normal)
            float targetSpeed = isTargeting ? TargetingMoveSpeed : (running ? SprintSpeed : MoveSpeed);

            // If there is no input, set target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            Vector3 moveDirection;

            // --- TARGETING MOVEMENT ---
            if (isTargeting && _currentTarget != null)
            {
                // Player rotation is handled by RotatePlayerTowardsTarget()
                RotatePlayerTowardsTarget();

                // Calculate move direction relative to the player's orientation (strafe/forward/backward)
                Vector3 inputDirTargeting = new Vector3(_input.move.x, 0.0f, _input.move.y);
                moveDirection = (transform.right * inputDirTargeting.x + transform.forward * inputDirTargeting.z);
            }
            // --- FREE-LOOK MOVEMENT ---
            else
            {
                if (_input.move != Vector2.zero)
                {
                    // Normalize input vector to get a direction
                    Vector3 inputDir = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

                    // Calculate the desired move direction based on camera rotation.
                    // This decouples movement direction from player rotation, preventing jitter.
                    moveDirection = Quaternion.Euler(0, _mainCamera.transform.eulerAngles.y, 0) * inputDir;

                    // If the character should be moving, smoothly rotate towards the calculated direction
                    if (moveDirection.magnitude >= _threshold)
                    {
                        _targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                        // Apply the calculated rotation to the Rigidbody
                        _rigidbody.MoveRotation(Quaternion.Euler(0.0f, rotation, 0.0f));
                    }
                }
                else
                {
                    moveDirection = Vector3.zero;
                }
            }

            // Calculate the final target velocity vector
            Vector3 targetVelocity = moveDirection.normalized * targetSpeed;

            // --- COLLISION AND SLOPE HANDLING ---
            // Perform a capsule cast to detect obstacles and adjust velocity to slide along them
            if (targetVelocity.magnitude > 0)
            {
                float radius = _capsuleCollider.radius;
                float height = _capsuleCollider.height;
                Vector3 p1 = _rigidbody.position + _capsuleCollider.center + Vector3.up * (height * 0.5f - radius);
                Vector3 p2 = _rigidbody.position + _capsuleCollider.center - Vector3.up * (height * 0.5f - radius);
                if (Physics.CapsuleCast(p1, p2, radius, targetVelocity.normalized, out RaycastHit hit, targetVelocity.magnitude * Time.fixedDeltaTime, ObstacleLayers))
                {
                    targetVelocity = Vector3.ProjectOnPlane(targetVelocity, hit.normal);
                }
            }

            // Project velocity onto the ground plane to handle slopes correctly
            if (_isGrounded)
            {
                targetVelocity = Vector3.ProjectOnPlane(targetVelocity, _groundNormal);
            }

            // --- RAMP STICKING LOGIC ---
            // If grounded and not moving, apply a downward velocity to "stick" to slopes
            if (_isGrounded && _input.move == Vector2.zero)
            {
                _rigidbody.linearVelocity = Vector3.zero;
            }
            else
            {
                // While moving, preserve the existing vertical velocity (for jumping and gravity)
                targetVelocity.y = _rigidbody.linearVelocity.y;
                // Apply the final calculated velocity to the Rigidbody
                _rigidbody.linearVelocity = targetVelocity;
            }

           

            // --- ANIMATOR UPDATES ---
            // Smoothly update the animation blend value for fluid transitions
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;
        }

        /// <summary>
        /// Updates the animator with the current state of the player.
        /// </summary>
        private void UpdateAnimator()
        {
            _animator.SetBool(_animIDTargeting, isTargeting); 
            if (isTargeting) 
            {
                // Set blend tree values for strafing
                _animator.SetFloat(_animIDTargetingMoveX, _input.move.x, 0.1f, Time.deltaTime); 
                _animator.SetFloat(_animIDTargetingMoveY, _input.move.y, 0.1f, Time.deltaTime); 
            }
            else
            {
                // Set speed for normal movement
                float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
                _animator.SetFloat(_animIDSpeed, _animationBlend); 
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude); 
            }
        }

        /// <summary>
        /// Main logic loop for the targeting system.
        /// </summary>
        void FoccusOnTheTarget()
        {
            // Toggle targeting on input
            if (_input.locked) 
            { 
                if (isTargeting) ClearTarget(); 
                else FindAndSetTarget(); 
            }

            // Validate the current target
            if (_currentTarget != null) 
            {
                bool outOfRange = Vector3.Distance(transform.position, _currentTarget.position) > MaxTargetDistance; 
                Vector3 raycastOrigin = transform.position + CameraTargetOffset; 
                Vector3 targetPoint = _currentTarget.position + TargetAimOffset; 
                bool isObstructed = Physics.Linecast(raycastOrigin, targetPoint, ObstacleLayers); 
                if (outOfRange || isObstructed) 
                {
                    ClearTarget(); 
                }
            }

            // If the target is lost, try to find a new one automatically
            if (_currentTarget == null && isTargeting) 
            {
                Transform newTarget = FindNearestEnemy(); 
                if (newTarget != null) SetNewTarget(newTarget); 
                else ClearTarget(); 
            }
        }

        /// <summary>
        /// Finds the best available target and initiates targeting mode.
        /// </summary>
        private void FindAndSetTarget()
        {
            if (!_isGrounded) return; 
            Transform bestTarget = FindNearestEnemy(); 
            if (bestTarget != null) 
            {
                isTargeting = true; 
                _currentTarget = bestTarget; 
                if (TargetIconPrefab != null) _instantiatedIcon = Instantiate(TargetIconPrefab, bestTarget.position, Quaternion.identity);    
            }
        }

        /// <summary>
        /// Clears the current target and exits targeting mode.
        /// </summary>
        private void ClearTarget()
        {
            isTargeting = false; 
            _currentTarget = null; 
            if (_instantiatedIcon != null) { Destroy(_instantiatedIcon); _instantiatedIcon = null; } 
        }

        /// <summary>
        /// Finds all valid targets within range and line of sight.
        /// </summary>
        private List<Transform> GetAllVisibleTargets()
        {
            List<Transform> visibleTargets = new List<Transform>(); 
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); 
            Camera mainCam = _mainCamera.GetComponent<Camera>(); 
            Vector3 raycastOrigin = transform.position + CameraTargetOffset; 

            foreach (GameObject potentialEnemy in enemies) 
            {
                if (potentialEnemy == null) continue; 
                Vector3 enemyPosition = potentialEnemy.transform.position; 
                float distanceToEnemy = Vector3.Distance(raycastOrigin, enemyPosition); 
                if (distanceToEnemy > MaxTargetDistance) continue; 

                // Check if the enemy is within the camera's viewport
                Vector3 viewPos = mainCam.WorldToViewportPoint(enemyPosition); 
                if (viewPos.z > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1) 
                {
                    // Check for obstacles
                    Vector3 directionToEnemy = (enemyPosition - raycastOrigin).normalized; 
                    if (!Physics.Raycast(raycastOrigin, directionToEnemy, distanceToEnemy, ObstacleLayers)) 
                    {
                        visibleTargets.Add(potentialEnemy.transform); 
                    }
                }
            }
            return visibleTargets; 
        }

        /// <summary>
        /// Finds the nearest enemy from a list of visible targets.
        /// </summary>
        private Transform FindNearestEnemy()
        {
            List<Transform> visibleTargets = GetAllVisibleTargets(); 
            Transform bestTarget = null; 
            float closestDistance = Mathf.Infinity; 
            foreach (Transform potentialTarget in visibleTargets) 
            {
                if (potentialTarget == null) continue; 
                float distance = Vector3.Distance(transform.position, potentialTarget.position); 
                if (distance < closestDistance) { closestDistance = distance; bestTarget = potentialTarget; } 
            }
            return bestTarget; 
        }

        /// <summary>
        /// Rotates the player smoothly to face the current target.
        /// </summary>
        private void RotatePlayerTowardsTarget()
        {
            Vector3 aimPoint = _currentTarget.position + TargetAimOffset; 
            Vector3 direction = (aimPoint - transform.position).normalized; 
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg; 
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _targetRotationVelocity, TargetRotationSmoothTime); 
            _rigidbody.MoveRotation(Quaternion.Euler(0.0f, angle, 0.0f)); 
        }

        /// <summary>
        /// Rotates the camera to focus on the current target.
        /// </summary>
        private void RotateCameraTowardsTarget()
        {
            if (_currentTarget == null) return; 
            Vector3 aimPoint = _currentTarget.position + TargetAimOffset; 
            Vector3 direction = (aimPoint - CinemachineCameraTarget.transform.position).normalized; 
            _cinemachineTargetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg; 
            float horizontalMagnitude = new Vector2(direction.x, direction.z).magnitude; 
            _cinemachineTargetPitch = Mathf.Atan2(-direction.y, horizontalMagnitude) * Mathf.Rad2Deg; 
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp); 
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f); 
        }

        /// <summary>
        /// Updates the position of the visual target icon.
        /// </summary>
        private void UpdateTargetIconPosition()
        {
            if (_instantiatedIcon == null || _currentTarget == null) return; 
            _instantiatedIcon.transform.position = _currentTarget.position + Vector3.up * TargetIconHeight; 
        }

        /// <summary>
        /// Handles switching targets based on horizontal input.
        /// </summary>
        private void HandleTargetSwitching()
        {
            if (_timeSinceLastSwitch < _targetSwitchCooldown) return; 
            float switchInput = _input.look.x; 
            if (Mathf.Abs(switchInput) > TargetSwitchThreshold) 
            {
                int direction = switchInput > 0 ? 1 : -1; 
                FindNextTarget(direction); 
                _timeSinceLastSwitch = 0f; 
            }
        }

        /// <summary>
        /// Finds the next best target in a given horizontal direction.
        /// </summary>
        private void FindNextTarget(int direction)
        {
            List<Transform> visibleTargets = GetAllVisibleTargets(); 
            if (visibleTargets.Count <= 1) return; 
            Camera mainCam = _mainCamera.GetComponent<Camera>(); 
            Vector3 currentTargetScreenPoint = mainCam.WorldToScreenPoint(_currentTarget.position); 
            Transform bestCandidate = null; 
            float closestXDistance = Mathf.Infinity; 
            foreach (Transform potentialTarget in visibleTargets) 
            {
                if (potentialTarget == _currentTarget) continue; 
                Vector3 potentialTargetScreenPoint = mainCam.WorldToScreenPoint(potentialTarget.position); 
                float xDifference = potentialTargetScreenPoint.x - currentTargetScreenPoint.x; 

                // Find the closest target in the desired direction on the screen
                if (Mathf.Sign(xDifference) == direction && Mathf.Abs(xDifference) < closestXDistance) 
                {
                    closestXDistance = Mathf.Abs(xDifference); 
                    bestCandidate = potentialTarget; 
                }
            }
            if (bestCandidate != null) SetNewTarget(bestCandidate); 
        }

        /// <summary>
        /// Sets a new target and updates the icon.
        /// </summary>
        private void SetNewTarget(Transform newTarget)
        {
            _currentTarget = newTarget; 
            if (_instantiatedIcon != null) _instantiatedIcon.transform.SetParent(newTarget); 
            UpdateTargetIconPosition(); 
        }

        /// <summary>
        /// Helper function to clamp an angle between a min and max value.
        /// </summary>
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f; 
            if (lfAngle > 360f) lfAngle -= 360f; 
            return Mathf.Clamp(lfAngle, lfMin, lfMax); 
        }

        /// <summary>
        /// Draws debug gizmos in the editor for easier visualization.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f); 
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f); 
            if (_isGrounded) Gizmos.color = transparentGreen; 
            else Gizmos.color = transparentRed; 

            // Draw a sphere for the ground check
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius); 
        }

    }

}