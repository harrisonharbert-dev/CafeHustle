using UnityEngine;
using DG.Tweening;

public class CarryObject : MonoBehaviour
{
    [SerializeField] GameObject player;

    private CharacterAnimationController animator;
    private PlayerInputController inputController;
    private InteractPrompt UI;
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
        
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();

            transform.SetParent(player.transform);
            transform.DOLocalMove(Vector3.zero,0.3f);
            transform.DOLocalRotate(Vector3.zero,0.3f);

            rb.isKinematic = true;
            col.enabled = false;

            Debug.Log("carrying");



        animator.SetTrigger("isCarrying");
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


        // Set Animator trigger
        animator.SetTrigger("isCarrying");
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
