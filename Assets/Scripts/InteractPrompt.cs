using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;







public class InteractPrompt : MonoBehaviour
{


    private CanvasGroup canvasGroup;

    [Header("UI")]
    [Space(10)]
    [SerializeField, Min(0.01f)] private float pollInterval = 0.1f;


    [SerializeField] private TextMeshProUGUI interactPromptText;
    [SerializeField] private Image interactImage;


    [Header("Key Sprites")]
    [SerializeField] private InputIconDisplay inputIconImage;
    [SerializeField] private InputIconSet eSprite;
    [SerializeField] private InputIconSet fSprite;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onVisibleEvent;
    [SerializeField] private UnityEvent onHiddenEvent;

    private bool promptVisibleState;


    public static InteractPrompt instance { get; private set; }


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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Get Components needed
        if (TryGetComponent<CanvasGroup>(out canvasGroup))
        {
            canvasGroup.alpha = 0f;
        }

        StartCoroutine(PollState());
    }

    private IEnumerator PollState()
    {
        while (true)
        {
            Refresh();
            yield return new WaitForSecondsRealtime(pollInterval);
        }
    }

    private string GetPromptText(Interactable.PromptText type)
    {
        return type switch
        {
            Interactable.PromptText.Use => "Use",
            Interactable.PromptText.PickUp => "Pick Up",
            Interactable.PromptText.Talk => "Talk",
            Interactable.PromptText.Open => "Open",
            Interactable.PromptText.Read => "Read",
            Interactable.PromptText.Drop => "Drop",
            Interactable.PromptText.Deliver => "Deliver",
            Interactable.PromptText.Inspect => "Inspect",
            _ => "Interact"
        };
    }

    public void UpdateUIInfo(Interactable.PromptText textType, Interactable.PromptKey keyType)
    {
        interactPromptText.text = GetPromptText(textType);

        if (keyType == Interactable.PromptKey.E)
        {
            inputIconImage.OnChangeIconSet(eSprite);
        }
        else if (keyType == Interactable.PromptKey.F)
        {
            inputIconImage.OnChangeIconSet(fSprite);
        }
    }

    public void SetPromptVisibility(bool value)
    {
        if (promptVisibleState == value) return;
        promptVisibleState = value;

        if (value == true)
        {
            onVisibleEvent?.Invoke();
        }
        else if (value == false)
        {
            onHiddenEvent?.Invoke();
        }
    }

    public void Refresh()
    {
        //Get all player states
        PlayerInputController.playState playerState = PlayerInputController.instance.playerState;
        bool currentInteractable = PlayerInputController.instance.currentInteractable != null;
        bool currentCarryObject = PlayerInputController.instance.currentCarryObject != null;
        bool isInDialogue = PlayerInputController.instance.isinDialogue;
        bool isInDeliveryZone = PlayerInputController.instance.inCarryDeliveryZone;
        bool isViewingModel = PlayerInputController.instance.isViewingModel;

        if (PlayerInputController.instance.isDelivering)
        {
            SetPromptVisibility(false);
            return;
        }

        //If player is in dialogue, hide
        if (isInDialogue || isViewingModel)
        {
            SetPromptVisibility(false);
            return;
        }


        switch (playerState)
        {
            case PlayerInputController.playState.none:
                //If player is in range of a current carry or interactable, set prompt to true
                if (currentCarryObject || currentInteractable)
                {
                    SetPromptVisibility(true);
                }
                //Set prompt to false if there isn't a carry or interactable in range
                else if (!currentCarryObject && !currentInteractable)
                {
                    SetPromptVisibility(false);
                }
                break;

            case PlayerInputController.playState.carryingObject:
                // if the player is currently carrying an object, set drop prompt to true
                SetPromptVisibility(true);

                //Update text depending if in a delivery zone
                if (isInDeliveryZone)
                {
                    UpdateUIInfo(Interactable.PromptText.Deliver, Interactable.PromptKey.F);
                }
                else
                {
                    UpdateUIInfo(Interactable.PromptText.Drop, Interactable.PromptKey.F);
                }
                break;



            case PlayerInputController.playState.carryingNonDroppable:

                // Hide prompt if carrying non-droppable and isn't close to a interactable and delivery zone
                if (!currentInteractable && !isInDeliveryZone)
                {
                    SetPromptVisibility(false);
                    return;
                }

                // Set it to true if above doesn't return
                SetPromptVisibility(true);


                //Change text if in delivery zone
                if (isInDeliveryZone)
                    UpdateUIInfo(Interactable.PromptText.Deliver, Interactable.PromptKey.F);
                break;
        }
    }
}