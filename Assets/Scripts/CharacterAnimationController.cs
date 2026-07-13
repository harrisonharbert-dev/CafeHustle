using Unity.VisualScripting;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{

    [SerializeField] private Animator animator;
    private float moveSpeed;
    private string blendName = "Blend";

    public static CharacterAnimationController instance {get; private set;}
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

    public void SetTrigger(string trigger)
    {
        animator.SetTrigger(trigger);        
    }

    // Update is called once per frame
    void Update()
    {
        float blendValue = 0f;

        if (!PlayerInputController.instance.lockMovement)
        {
            float moveSpeed = PlayerInputController.instance.moveInput.magnitude;

            if (!PlayerInputController.instance.isRunning)
            {
                moveSpeed *= 0.5f;
            }

            blendValue = moveSpeed;
        }

        animator.SetFloat(blendName, blendValue, 0.1f, Time.deltaTime);
    }
}
