using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerInputController : MonoBehaviour
{

    //Move direction vector
    private Vector2 moveInput;
    private Rigidbody rigidBody;
    private Transform cameraTransform;
    [SerializeField] private float moveSpeed = 5;

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

        if (currentInteractable.useDialogueCamera) {
            int value = option ? 1 : -1;
            dialogueCamera.Priority = value;
        }

        //Hide cursor
        if (option == false)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
        Cursor.visible = option;
    }


    public void Move(InputAction.CallbackContext context)
    {
        if (lockMovement) return;

        //Move input taken from player input
        moveInput = context.ReadValue<Vector2>().normalized;
    }


    public void Interact(InputAction.CallbackContext context)
    {
        if (currentInteractable.isInRange && lockMovement == false && context.performed && currentInteractable != null)
        {
            currentInteractable.InvokeEvent();
            UI.SetPromptVisibility(false);

            if (currentInteractable.useDialogue)
            {
                transform.DOLookAt(currentInteractable.transform.position, interactRotationDuration, AxisConstraint.Y);
            }
        }
    }

    private void FixedUpdate()
{
    Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized;
    Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, transform.up).normalized;

    Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

    Vector3 verticalVelocity = Vector3.Project(rigidBody.linearVelocity, transform.up);

    rigidBody.linearVelocity = moveDirection * moveSpeed + verticalVelocity;

    if (moveDirection.sqrMagnitude > 0.01f)
    {
        rigidBody.MoveRotation(
            Quaternion.LookRotation(moveDirection, transform.up)
        );
    }
}
}