using UnityEngine;
using DG.Tweening;

public class CarryObject : MonoBehaviour
{

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onPickUpEvent;
    [SerializeField] private UnityEvent onDropEvent;
    [SerializeField] private UnityEvent onDeliverEvent;

    [Header("Player References")]  
    [SerializeField] GameObject player;

    //Private References

    private CharacterAnimationController animator;
    private PlayerInputController inputController;
    private InteractPrompt UI;
    private string animatorTag = "isCarrying";
    private float transitionDuration = 0.3f;
    [HideInInspector] public bool isInRange;


    void Start()
    {
        animator = FindAnyObjectByType<CharacterAnimationController>();
        inputController = FindAnyObjectByType<PlayerInputController>();
        UI = FindAnyObjectByType<InteractPrompt>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created 
    public void SetGrab()
    {

        //Collider and rigid body
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();


            //Attach to player arm
            transform.SetParent(player.transform);
            transform.DOLocalMove(Vector3.zero,transitionDuration);
            transform.DOLocalRotate(Vector3.zero,transitionDuration);

            rb.isKinematic = true;
            col.enabled = false;

            



        onPickUpEvent.Invoke();

        //Character Animator trigger
        animator.SetTrigger(animatorTag);
    }

    public void SetDrop()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();
        // Remove parent and reenable colliders and rigid body
        transform.SetParent(null);

        rb.isKinematic = false;
        col.enabled = true;

        Debug.Log("dropped");



        //
        onDropEvent.Invoke();

        // Set Animator trigger
        animator.SetTrigger(animatorTag);
    }





    public void SetUse()
    {
        onDeliverEvent.Invoke();
        animator.SetTrigger(animatorTag);
    }







    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = true;
            inputController.SetCurrentCarry(this);

            UI.SetPromptVisibility(true);
            
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = false;

            UI.SetPromptVisibility(false);
        }
    }
}
