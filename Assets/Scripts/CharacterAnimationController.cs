using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{

    public Animator animator;
    private PlayerInputController playerInputController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInputController = GetComponent<PlayerInputController>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        bool isRunning = horizontal != 0 || vertical != 0;

        if (!playerInputController.lockMovement)
        animator.SetBool("isRunning", isRunning);
    }
}
