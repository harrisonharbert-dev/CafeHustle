using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;

public class CarryObject : MonoBehaviour
{

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onPickUpEvent;
    [SerializeField] private UnityEvent onDropEvent;
    [SerializeField] private UnityEvent onDeliverEvent;

    [Header("Player References")]
    [SerializeField] GameObject playerCarryPosition;

    [Header("Item Properties")]
    public string itemID;
    [SerializeField] private Vector3 carryRotation;
    [SerializeField] private Vector3 carryPosition;

    //Private References


    private string carryingTag = "isCarrying";
    private float transitionDuration = 0.3f;
    private float jumpPower = 1.5f;
    private float throwDelay = 0.75f;
    [HideInInspector] public bool isInRange;



    // Start is called once before the first execution of Update after the MonoBehaviour is created 
    public void SetGrab()
    {



        //Attach to player arm
        transform.SetParent(playerCarryPosition.transform);
        transform.DOLocalMove(carryPosition, transitionDuration);
        transform.DOLocalRotate(carryRotation, transitionDuration);


        onPickUpEvent.Invoke();

        CharacterAnimationController.instance.SetTrigger(carryingTag);
    }

    public void SetDrop()
    {

        // Remove parent and reenable colliders and rigid body
        transform.SetParent(null);






        //
        onDropEvent.Invoke();

        // Set Animator trigger
        CharacterAnimationController.instance.SetTrigger(carryingTag);
    }





    public void SetDeliver()
    {
        CutsceneAnimator.instance.playAction("character_throw");
        CharacterAnimationController.instance.SetTrigger(carryingTag);


        StartCoroutine(Throw(throwDelay));

    }
    public IEnumerator Throw(float delay)
    {
        yield return new WaitForSeconds(delay);
        onDeliverEvent.Invoke();


        if (PlayerInputController.instance.deliveryZonePos != null)

        {
            transform.SetParent(PlayerInputController.instance.deliveryZonePos.transform);
        }

        transform.DOLocalJump(Vector3.zero, jumpPower, 1, transitionDuration).OnComplete(() =>
        {
            Destroy(gameObject);
        }
        );
    }





    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = true;
            if (PlayerInputController.instance.playerCarryingState == PlayerInputController.carryingState.none)
            {
                PlayerInputController.instance.SetCurrentCarry(this);
                InteractPrompt.instance.SetPromptVisibility(true);
            }


        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = false;
            if (PlayerInputController.instance.playerCarryingState == PlayerInputController.carryingState.none)
            {
                InteractPrompt.instance.SetPromptVisibility(false);
            }
        }
    }
}
