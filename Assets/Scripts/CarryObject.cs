using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class CarryObject : MonoBehaviour
{

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onPickUpEvent;
    [SerializeField] private UnityEvent onDropEvent;
    [SerializeField] private UnityEvent onDeliverEvent;

    [Header("Player References")]  
    [SerializeField] GameObject player;

    [Header("Item Properties")]
    public string itemID;
    [SerializeField] private Vector3 carryRotation;
    [SerializeField] private Vector3 carryPosition;

    //Private References


    private string animatorTag = "isCarrying";
    private float transitionDuration = 0.3f;
    [HideInInspector] public bool isInRange;



    // Start is called once before the first execution of Update after the MonoBehaviour is created 
    public void SetGrab()
    {

        //Collider and rigid body


            //Attach to player arm
            transform.SetParent(player.transform);
            transform.DOLocalMove(carryPosition,transitionDuration);
            transform.DOLocalRotate(carryRotation,transitionDuration);



            



        onPickUpEvent.Invoke();

        CharacterAnimationController.instance.SetTrigger(animatorTag);
    }

    public void SetDrop()
    {

        // Remove parent and reenable colliders and rigid body
        transform.SetParent(null);






        //
        onDropEvent.Invoke();

        // Set Animator trigger
        CharacterAnimationController.instance.SetTrigger(animatorTag);
    }





    public void SetDeliver()
    {
        onDeliverEvent.Invoke();
        CharacterAnimationController.instance.SetTrigger(animatorTag);

        Destroy(gameObject);
    }







    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = true;
            PlayerInputController.instance.SetCurrentCarry(this);

            InteractPrompt.instance.SetPromptVisibility(true);
            
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = false;

            InteractPrompt.instance.SetPromptVisibility(false);
        }
    }
}
