using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{

    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        bool isRunning = horizontal != 0 || vertical != 0;

        animator.SetBool("isRunning", isRunning);
    }
}
