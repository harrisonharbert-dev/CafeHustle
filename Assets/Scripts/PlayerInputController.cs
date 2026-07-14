using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using NUnit.Framework;



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

    [HideInInspector] public bool lockMovement = false;





    [Header("Interactables")]
    private Interactable currentInteractable;
    private CarryObject currentCarryObject;

    public enum carryingState
    {
        none,
        carryingObject,
    }
    public carryingState playerCarryingState;
    [HideInInspector] public GameObject deliveryZone;
    [HideInInspector] public bool inCarryDeliveryZone;
    public string currentCarryItemID;






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
        switch (playerCarryingState)
        {
            case carryingState.none:
                InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.PickUp, Interactable.PromptKey.F);
                break;

            case carryingState.carryingObject:
                InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.Drop, Interactable.PromptKey.F);
                break;

        }
    }

    public void SetMovementLock(bool option)
    {
        lockMovement = option;
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

        if (currentInteractable != null && currentInteractable.useDialogueCamera && currentInteractable.isInRange)
        {
            int value = option ? 1 : -1;
            dialogueCamera.Priority = value;
        }



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
        if (currentInteractable.isInRange && !lockMovement && context.performed && currentInteractable != null)
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


                switch (playerCarryingState)
                {
                    case carryingState.none:
                        currentCarryObject.SetGrab();
                        playerCarryingState = carryingState.carryingObject;
                        InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.Drop, Interactable.PromptKey.F);
                        currentCarryItemID = currentCarryObject.itemID;
                        break;
                    case carryingState.carryingObject:
                        if (!inCarryDeliveryZone)
                        {
                            currentCarryObject.SetDrop();
                            InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.PickUp, Interactable.PromptKey.F);
                        }
                        else
                        {
                            currentCarryObject.SetDeliver();
                            InteractPrompt.instance.SetPromptVisibility(false);
                        }
                        
                        playerCarryingState = carryingState.none;
                        currentCarryItemID = null;
                        break;
                }
            });
        }
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