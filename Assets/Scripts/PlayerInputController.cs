using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

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
    [HideInInspector] private InteractPrompt UI;





    [Header("Interactables")]
    [SerializeField] private Interactable currentInteractable;
    [SerializeField] private float interactRotationDuration = 0.5f;
    [SerializeField] private CinemachineCamera dialogueCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //Find UI Prompt
        UI = GameObject.FindGameObjectWithTag("InteractPrompt").GetComponent<InteractPrompt>();

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
        
        moveInput = new Vector2(0f,0f);

        if (currentInteractable.useDialogueCamera) {
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
        if (currentInteractable.isInRange && lockMovement == false && context.performed && currentInteractable != null)
        {
            currentInteractable.InvokeEvent();
            UI.SetPromptVisibility(false);
            transform.DOLookAt(currentInteractable.transform.position, interactRotationDuration, AxisConstraint.Y);
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