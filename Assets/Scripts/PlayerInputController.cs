using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{

    //Move direction vector
    private Vector2 moveInput;
    private Rigidbody rigidBody;
    [SerializeField] private float moveSpeed = 5;

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
    }

    public void Move(InputAction.CallbackContext context)
    {

        //Move input taken from player input
        moveInput = context.ReadValue<Vector2>();

        //Normalize so total magnitude of vector is 1
        moveInput.Normalize();


    }

    public void Update()
    {
        // Update player movement by moveInput direction and moveSpeed
        rigidBody.linearVelocity = new Vector3(moveInput.x * moveSpeed, rigidBody.linearVelocity.y, moveInput.y * moveSpeed);

        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            transform.forward = moveDirection;
        }
    }
    }