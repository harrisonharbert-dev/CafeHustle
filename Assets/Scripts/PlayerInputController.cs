using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Yarn.Unity;
using UnityEngine.Events;
using System.Collections;



public class PlayerInputController : MonoBehaviour
{

    //Move direction vector
    [HideInInspector] public Vector2 moveInput;
    private Rigidbody rigidBody;
    private Transform cameraTransform;
    private Renderer obstructionRenderer;
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
    [SerializeField] private float groundCheckDistance = 0.3f; // distance below capsule bottom to check
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


    public bool lockMovement = false;





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
        onStartEvent.Invoke();


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
        transform.DODynamicLookAt(target.transform.position, 3f, AxisConstraint.Y);
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
        if (currentInteractable.isInRange && !lockMovement && context.performed && currentInteractable != null && currentInteractable.interactType == Interactable.interactableType.interactableWithInput)
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
        if (currentCarryObject.isInRange && !lockMovement && context.performed && currentCarryObject != null)
        {
            transform.DOLookAt(currentCarryObject.transform.position, interactRotationDuration, AxisConstraint.Y).OnComplete(() =>
            {
                useItem();
            });
        }
    }

    public void useItem()
    {
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
                    currentCarryObject.SetDeliver();
                    clearHeldItem();
                }
                break;

            case playState.carryingNonDroppable:
                if (inCarryDeliveryZone)
                {
                    currentCarryObject.SetDeliver();
                    clearHeldItem();
                }
                break;
        }
    }
    void clearHeldItem()
    {

        playerState = playState.none;
        currentCarryItemID = null;
        InteractPrompt.instance.Refresh();
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
        // Cast from the bottom of the capsule (or transform position if no capsule found)
        Vector3 origin = transform.position;
        float castDistance = groundCheckDistance;

        if (capsuleCollider != null)
        {
            // Bottom of the capsule in world space, pulled up slightly so the cast starts inside the collider
            float bottomOffset = (capsuleCollider.height * 0.5f) - capsuleCollider.radius;
            Vector3 localBottom = capsuleCollider.center - Vector3.up * bottomOffset;
            origin = transform.TransformPoint(localBottom) + Vector3.up * 0.1f;
            castDistance = groundCheckDistance + 0.1f;
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

    private void FixedUpdate()
    {
        Shader.SetGlobalVector("_PlayerPosition", transform.position + Vector3.up);

        if (lockMovement)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            moveInput = new Vector2(0f, 0f);
            currentVelocity = Vector3.zero;
            rigidBody.linearVelocity = Vector3.Project(rigidBody.linearVelocity, Vector3.up);
            return;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
        Vector3 dir = transform.position - cameraTransform.position;
        Renderer hitRenderer = null;

        if (Physics.Raycast(cameraTransform.position, dir.normalized, out RaycastHit hit, dir.magnitude, obstructionMask))
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty(obstructionSizeId))
            {
                hitRenderer = renderer;
            }
        }

        if (hitRenderer != obstructionRenderer)
        {
            if (obstructionRenderer != null)
            {
                TweenObstructionSize(obstructionRenderer, 0f);
            }

            obstructionRenderer = hitRenderer;

            if (obstructionRenderer != null)
            {
                TweenObstructionSize(obstructionRenderer, 0.3f);
            }
        }
    }

    private void TweenObstructionSize(Renderer targetRenderer, float targetSize)
    {
        DOTween.Kill(targetRenderer);

        obstructionPropertyBlock.Clear();
        targetRenderer.GetPropertyBlock(obstructionPropertyBlock);
        float startSize = obstructionPropertyBlock.HasFloat(obstructionSizeId)
            ? obstructionPropertyBlock.GetFloat(obstructionSizeId)
            : targetRenderer.sharedMaterial.GetFloat(obstructionSizeId);

        DOTween.To(() => startSize, value => SetObstructionSize(targetRenderer, value), targetSize, obstructionFadeDuration)
            .SetTarget(targetRenderer);
    }

    private void SetObstructionSize(Renderer targetRenderer, float size)
    {
        obstructionPropertyBlock.Clear();
        targetRenderer.GetPropertyBlock(obstructionPropertyBlock);
        obstructionPropertyBlock.SetFloat(obstructionSizeId, size);
        targetRenderer.SetPropertyBlock(obstructionPropertyBlock);
    }
}