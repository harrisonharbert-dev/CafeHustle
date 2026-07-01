using Unity.VisualScripting;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{

    [SerializeField] private Animator animator;
    private float moveSpeed;
    private string blendName = "Blend";
    private PlayerInputController playerInputController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        playerInputController = GetComponent<PlayerInputController>();
    }

    // Update is called once per frame
    void Update()
    {
        float blendValue = 0f;

        if (!playerInputController.lockMovement)
        {
            float moveSpeed = playerInputController.moveInput.magnitude;

            if (!playerInputController.isRunning)
            {
                moveSpeed *= 0.5f;
            }

            blendValue = moveSpeed;
        }

        animator.SetFloat(blendName, blendValue, 0.1f, Time.deltaTime);
    }
}
