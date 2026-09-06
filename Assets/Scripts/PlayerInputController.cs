using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using NUnit.Framework;
using Yarn.Unity;
using UnityEngine.Events;



public class PlayerInputController : MonoBehaviour
{

    //Move direction vector
    [HideInInspector] public Vector2 moveInput;
    private Rigidbody rigidBody;
    private Transform cameraTransform;

    [System.Serializable]
    public struct moveStates
    {
        public float walking;
        public float running;
    }

    public moveStates moveSpeed;
    private float maxSpeed;
    [HideInInspector] public bool isRunning = false;
    public bool isinDialogue = false;

    [HideInInspector] public bool lockMovement = false;





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
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody>();
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
                break;

            case playState.carryingObject:
                break;

        }
    }

    public void setDialogue(bool option)
    {
        isinDialogue = option;
        SetMovementLock(option);
    }
    public void SetMovementLock(bool option)
    {
        lockMovement = option;

        if (isinDialogue)
        {
            lockMovement = true;
        }
                

        Debug.Log("Lock set to" + option);

        //Hide cursor
        Cursor.visible = option;

        if (option == false)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
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
        transform.DOLookAt(target.transform.position, interactRotationDuration, AxisConstraint.Y);
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

                currentCarryItemID = currentCarryObject.itemID;
                break;
            case playState.carryingObject:


                if (!inCarryDeliveryZone)
                {
                    currentCarryObject.SetDrop();
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
    }
    public void useDrop()
    {
        currentCarryObject.SetDrop();
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
    private void FixedUpdate()
    {
        Shader.SetGlobalVector("_PlayerPosition",transform.position+Vector3.up);


        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, transform.up).normalized;

        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        Vector3 verticalVelocity = Vector3.Project(rigidBody.linearVelocity, transform.up);

        rigidBody.linearVelocity = moveDirection * maxSpeed + verticalVelocity;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            rigidBody.MoveRotation(
                Quaternion.LookRotation(moveDirection, transform.up)
            );
        }
    }
}