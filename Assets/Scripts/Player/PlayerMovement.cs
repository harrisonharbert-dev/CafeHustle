using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [Header("References")]
    [Tooltip("Rigid-body for player movement")]
    public Rigidbody rigidBody;
    [Tooltip("Float for the player move speed")]
    public float moveSpeed = 5;

    //Move direction vector
    private Vector2 moveInput;

    [Header("Sprite")]
    [Tooltip("Character sprite renderer")]
    public SpriteRenderer spriteRenderer;


    //Toggle Debug logs
    [SerializeField] private bool enableDebugLogs = true;

    //Player Collider
    private CapsuleCollider playerCollider;


    //Interactable script reference
    private Interactable currentInteractable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        // Get Rigid body if unassigned
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        //Get Sprite Renderer if unassigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider>();
        }
    }

   public void Move(InputAction.CallbackContext context)
    {
        
            //Move input taken from player input
            moveInput = context.ReadValue<Vector2>();

            //Normalize so total magnitude of vector is 1
            moveInput.Normalize();

            //Debug Log
            if (enableDebugLogs)
            {
                Debug.Log(moveInput);
            }
        
        
    }
    
    //Set current interactable function to call from other scripts
    public void SetCurrentInteractable(Interactable interactable) => currentInteractable = interactable;

    //Interactable function

    public void OnInteract(InputAction.CallbackContext context)
    {
        
        if (!currentInteractable) return;
        if (context.performed && currentInteractable.isInRange)
        {
            //Invoke event on current interactable
            currentInteractable.InvokeEvent();
        }



    }


    // Update is called once per frame
    void Update()
    {
        // Update player movement by moveInput direction and moveSpeed
        rigidBody.linearVelocity = new Vector3(moveInput.x * moveSpeed, rigidBody.linearVelocity.y , moveInput.y * moveSpeed);


        // Flip Sprite depending on move direction
        if(spriteRenderer.flipX && moveInput.x < 0 )
        {
            spriteRenderer.flipX = false;

        } 
        else if (!spriteRenderer.flipX && moveInput.x > 0)
        {
            spriteRenderer.flipX = true;
        }

    }

    private void LateUpdate()
    {
        //Face Camera
        spriteRenderer.transform.LookAt(Camera.main.transform);
        spriteRenderer.transform.Rotate(0, 180, 0);

        //Set Player Position for shader effects
        Shader.SetGlobalVector("_PlayerPosition", transform.position + Vector3.up * playerCollider.radius);
    }
}
