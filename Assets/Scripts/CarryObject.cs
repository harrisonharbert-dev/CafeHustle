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
    [SerializeField] private string throwAnimation = "character_throw";

    [Header("Item Properties")]
    [SerializeField] private bool isPlaceObject;
    [SerializeField] public bool canDrop = true;
    public string itemID;
    [SerializeField] private Vector3 carryRotation;
    [SerializeField] private Vector3 carryPosition;


    [Header("Interaction Prompt")]
    [SerializeField] private InteractPrompt3D prompt;
    [SerializeField] private interactableZoneIndicator zoneIndicator;

    //Private References


    private string carryingTag = "isCarrying";
    private float transitionDuration = 0.3f;
    private float jumpPower = 1.5f;
    private float throwDelay = 0.75f;
    [HideInInspector] public bool isInRange;

    private void Start()
    {
        if (prompt == null)
        {
            Debug.LogWarning($"[CarryObject] Missing interact prompt UI on {this}");
        }

        if (zoneIndicator == null)
        {
            Debug.LogWarning($"[CarryObject] Missing zone indicator on {this}");
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created 
    public void SetGrab()
    {
        //Attach to player arm
        transform.SetParent(playerCarryPosition.transform);
        transform.DOLocalMove(carryPosition, transitionDuration);
        transform.DOLocalRotate(carryRotation, transitionDuration);


        onPickUpEvent.Invoke();

        if (prompt != null)
        {
            prompt.onUI(false);
        }

        if (zoneIndicator != null)
        {
            zoneIndicator.changeIndicatorVisibility(true);
        }
        


        CharacterAnimationController.instance.SetTrigger(carryingTag);
    }

    public void SetDrop()
    {

        // Remove parent and reenable colliders and rigid body
        transform.SetParent(null);
        //
        onDropEvent.Invoke();

        if (prompt != null)
        {
            prompt.onUI(true);
        }

        if (zoneIndicator != null)
        {
            zoneIndicator.changeIndicatorVisibility(false);
        }


        // Set Animator trigger
        CharacterAnimationController.instance.SetTrigger(carryingTag);
    }





    public void SetDeliver()
    {
        CutsceneAnimator.instance.playAction(throwAnimation);
        CharacterAnimationController.instance.SetTrigger(carryingTag);
        StartCoroutine(Throw(throwDelay));

        if (prompt != null)
        {
            prompt.onUI(false);
        }

        if (zoneIndicator != null)
        {
            zoneIndicator.changeIndicatorVisibility(false);
        }

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
            PlayerInputController.instance.SetCurrentCarry(null);
            if (!isPlaceObject)
            {
                Destroy(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }
        );
    }





    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = true;
            if (PlayerInputController.instance.playerState == PlayerInputController.playState.none)
            {
                PlayerInputController.instance.SetCurrentCarry(this);
                if (prompt != null)
                {
                    prompt.onUI(true);
                }
            }


        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInRange = false;
            if (PlayerInputController.instance.playerState == PlayerInputController.playState.none)
            {
                PlayerInputController.instance.SetCurrentCarry(null);
                if (prompt != null)
                {
                    prompt.onUI(false);
                }
            }
        }
    }
}
