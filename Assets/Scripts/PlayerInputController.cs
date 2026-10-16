using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Yarn.Unity;
using UnityEngine.Events;
using System.Collections;
using System;
using System.Collections.Generic;




public class PlayerInputController : MonoBehaviour
{

    //Move direction vector
    [SerializeField] private bool useStartEvent;
    [HideInInspector] public Vector2 moveInput;
    private Rigidbody rigidBody;
    private Transform cameraTransform;
    private HashSet<Renderer> obstructionRenderers = new HashSet<Renderer>();
    private HashSet<Renderer> hitObstructionRenderers = new HashSet<Renderer>();
    private MaterialPropertyBlock obstructionPropertyBlock;
    private static readonly int obstructionSizeId = Shader.PropertyToID("_Size");
    private const float obstructionFadeDuration = 0.25f;

    [System.Serializable]
    public struct moveStates
    {
        public float walking;
        public float running;
    }
    [Header("Movement")]
    public moveStates moveSpeed;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float rotationSpeed = 720f;
    private Vector3 currentVelocity;
    private float maxSpeed;

    [SerializeField] private LayerMask obstructionMask;

    [Header("Ground / Slope Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f; // distance below capsule bottom to check
    [SerializeField] private float groundCheckRadius = 0.25f;  // should roughly match capsule radius
    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;
    private CapsuleCollider capsuleCollider;

    [HideInInspector] public bool isRunning = false;
    public bool isinDialogue = false;
    public bool isViewingModel = false;

    [SerializeField] private PlayerFootstepController footstepController;
    [SerializeField] private float footstepFrequency = 2f;
    private float footstepTimer;


    [HideInInspector] public bool lockMovement = false;
    public Transform NPCDialogueLocation;
    private float NPCDialogueDistance = 3.5f;





    [Header("Interactables")]
    public Interactable currentInteractable;
    public CarryObject currentCarryObject;

    public enum playState
    {
        none,
        carryingObject,
        carryingNonDroppable
    }
    public playState playerState;
    [HideInInspector] public GameObject deliveryZonePos;
    public bool inCarryDeliveryZone;
    public string currentCarryItemID;
    [HideInInspector] public bool isDelivering;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onStartEvent;




    [SerializeField] private float interactRotationDuration = 0.5f;
    [SerializeField] private CinemachineCamera dialogueCamera;

    public static PlayerInputController instance { get; private set; }
    private void Awake()
    {
        obstructionPropertyBlock = new MaterialPropertyBlock();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (useStartEvent)
        {
            onStartEvent.Invoke();
        }


        //Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        maxSpeed = moveSpeed.walking;

        cameraTransform = Camera.main.transform;

        // Get Rigid body if unassigned
        if (TryGetComponent(out Rigidbody body))
        {
            rigidBody = body;
        }

        // Get capsule collider for ground-check sizing
        if (TryGetComponent(out CapsuleCollider capsule))
        {
            capsuleCollider = capsule;
        }

    }



    public void SetCurrentInteractable(Interactable newTarget)
    {
        currentInteractable = newTarget;

    }

    public void SetCurrentCarry(CarryObject newTarget)
    {
        currentCarryObject = newTarget;

        switch (playerState)
        {
            case playState.none:
                InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.PickUp, Interactable.PromptKey.F);

                break;

            case playState.carryingObject:
                InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.Drop, Interactable.PromptKey.F);

                break;

        }
    }

    public void setDialogue(bool option)
    {
        isinDialogue = option;


        if (option == false && currentInteractable != null && currentInteractable.useDialogueCamera)
        {
            //Wait for camera transition back then re-enable movement. Change float time to match
            StartCoroutine(waitLock(false, 0.9f));
        }
        else
        {
            SetMovementLock(option);
        }
    }

    public void SetViewingModel(bool option)
    {
        isViewingModel = option;


        SetMovementLock(option);
    }

    private IEnumerator waitLock(bool option, float time)
    {
        yield return new WaitForSeconds(time);
        SetMovementLock(option);
    }
    public void SetMovementLock(bool option)
    {
        lockMovement = option;


        if (isinDialogue || isViewingModel)
        {
            lockMovement = true;
        }

        moveInput = new Vector2(0f, 0f);

    }
    public bool isValidDialogueLocation()
    {
        //Raycast out in front and check if collides with Obstacle.
        if (Physics.SphereCast(transform.position, .2f, transform.forward, out RaycastHit hit, 3f, obstructionMask))
        {
            NPCDialogueLocation.position = hit.point;
            return false;
        }

        NPCDialogueLocation.localPosition = new Vector3(0f, 0f, NPCDialogueDistance);
        return true;
    }
    public void onDialogueCamera(GameObject target)
    {
        dialogueCamera.Priority = 1;
        if (!target) return;
        LookAt(target);
    }

    public void offDialogueCamera()
    {
        dialogueCamera.Priority = -1;
    }

    [YarnCommand("player_look_at")]
    public void LookAt(GameObject target)
    {
        transform.DOLookAt(target.transform.position, 0.3f, AxisConstraint.Y);
        dialogueCamera.LookAt = target.transform;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!lockMovement)
        {
            //Move input taken from player input
            moveInput = context.ReadValue<Vector2>().normalized;
        }
    }


    public void Interact(InputAction.CallbackContext context)
    {
        if (currentInteractable != null && currentInteractable.isInRange && !lockMovement && context.performed && currentInteractable.interactType == Interactable.interactableType.interactableWithInput)
        {
            InteractPrompt.instance.SetPromptVisibility(false);
            transform.DOLookAt(currentInteractable.transform.position, interactRotationDuration, AxisConstraint.Y).OnComplete(() =>
            {
                currentInteractable.InvokeEvent();
            });

        }
    }


    public void Grab(InputAction.CallbackContext context)
    {
        if (currentCarryObject != null && currentCarryObject.isInRange && !lockMovement && context.performed)
        {
            transform.DOLookAt(currentCarryObject.transform.position, interactRotationDuration, AxisConstraint.Y).OnComplete(() =>
            {
                useItem();
            });
        }
    }
    public void useItem()
    {
        if (currentCarryObject == null)
        {
            return;
        }



        currentCarryObject.isInRange = true;
        switch (playerState)
        {
            case playState.none:
                currentCarryObject.SetGrab();

                if (currentCarryObject.canDrop)
                {
                    playerState = playState.carryingObject;
                }
                else
                {
                    playerState = playState.carryingNonDroppable;
                }
                InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.Drop, Interactable.PromptKey.F);
                currentCarryItemID = currentCarryObject.itemID;
                break;
            case playState.carryingObject:


                if (!inCarryDeliveryZone)
                {
                    currentCarryObject.SetDrop();
                    InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.PickUp, Interactable.PromptKey.F);
                    clearHeldItem();
                }
                else
                {
                    CarryObject deliveredObject = currentCarryObject;
                    deliveredObject.SetDeliver();
                    clearHeldItem();
                    SetCurrentCarry(null);
                }
                break;

            case playState.carryingNonDroppable:
                if (inCarryDeliveryZone)
                {
                    CarryObject deliveredObject = currentCarryObject;
                    deliveredObject.SetDeliver();
                    clearHeldItem();
                    SetCurrentCarry(null);
                }
                break;
        }
    }
    void clearHeldItem()
    {
        playerState = playState.none;
        currentCarryItemID = null;
    }
    public void useDrop()
    {

        currentCarryObject.SetDrop();
        InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.PickUp, Interactable.PromptKey.F);
        playerState = playState.none;
    }
    public void Run(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            maxSpeed = moveSpeed.running;
            isRunning = true;
        }

        else if (context.canceled)
        {
            maxSpeed = moveSpeed.walking;
            isRunning = false;
        }

    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * (groundCheckRadius + 0.02f);
        float castDistance = groundCheckDistance + 0.02f;

        if (capsuleCollider != null)
        {
            Vector3 localBottom = capsuleCollider.center - Vector3.up * (capsuleCollider.height * 0.5f);
            origin = transform.TransformPoint(localBottom) + Vector3.up * (groundCheckRadius + 0.02f);
        }

        if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out RaycastHit hit, castDistance, groundLayer))
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }

    void OnShowMouse(bool option)
    {
        if (option == true)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (option == false)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    private void FixedUpdate()
    {
        isValidDialogueLocation();
        Shader.SetGlobalVector("_PlayerPosition", transform.position + Vector3.up);

        if (lockMovement)
        {
            if (InputIconManager.instance.CurrentDevice == InputIconManager.DeviceType.KeyboardMouse)
            {
                OnShowMouse(true);
            }
            else
            {
                OnShowMouse(false);
            }
            moveInput = new Vector2(0f, 0f);
            currentVelocity = Vector3.zero;
            rigidBody.linearVelocity = Vector3.Project(rigidBody.linearVelocity, Vector3.up);
            return;
        }
        else
        {
            OnShowMouse(false);
        }

        CheckGround();

        // --- MOVEMENT (horizontal, slope-aware) ---
        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        // Project movement onto the ground slope so we don't fight the collision response on inclines
        Vector3 slopeMoveDirection = isGrounded
            ? Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized
            : moveDirection;

        Vector3 targetVelocity = slopeMoveDirection * maxSpeed;

        float rate = moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;
        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);

        if (isGrounded)
        {
            // currentVelocity already includes the correct vertical component from the slope
            // projection, so we don't stack collision-response vertical velocity on top of it.
            rigidBody.linearVelocity = currentVelocity;
        }
        else
        {
            // Airborne: preserve gravity/fall velocity, only control horizontal movement
            Vector3 verticalVelocity = Vector3.Project(rigidBody.linearVelocity, Vector3.up);
            rigidBody.linearVelocity = currentVelocity + verticalVelocity;
        }

        // --- ROTATION ---
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            rigidBody.MoveRotation(Quaternion.RotateTowards(rigidBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

            if (footstepController != null && footstepFrequency > 0f)
            {
                footstepTimer += Time.fixedDeltaTime;
                if (footstepTimer >= 1f / footstepFrequency)
                {
                    footstepController.PlayFootstep();
                    footstepTimer = 0f;
                }
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        Vector3 screenPixelPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 normalizedScreenPos = new Vector2(screenPixelPos.x / Screen.width, screenPixelPos.y / Screen.height);
        Shader.SetGlobalVector("_PlayerScreenPos", normalizedScreenPos);

        //Raycast Obstacles
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger == true)
        {

        }
    }
}