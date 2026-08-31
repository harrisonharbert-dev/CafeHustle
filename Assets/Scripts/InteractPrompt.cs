using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using CsvHelper.Configuration.Attributes;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Events;







public class InteractPrompt : MonoBehaviour
{


    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    [SerializeField] private float fadeDuration;
    public UITweener tweener;

    [Header("UI")]
    [Space(10)]

    [SerializeField] private TextMeshProUGUI interactPromptText;
    [SerializeField] private Image interactImage;


    [Header("Key Sprites")]
    [SerializeField] private Sprite eSprite;
    [SerializeField] private Sprite fSprite;

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
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        canvasGroup.alpha = 0f;
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
            _ => "Interact"
        };
    }

    private Sprite GetKeySprite(Interactable.PromptKey key)
    {
        return key switch
        {
            Interactable.PromptKey.E => eSprite,
            Interactable.PromptKey.F => fSprite,
            _ => null
        };
    }


    public void UpdateUIInfo(Interactable.PromptText textType, Interactable.PromptKey keyType)
    {
        interactPromptText.text = GetPromptText(textType);
        interactImage.sprite = GetKeySprite(keyType);
    }

    public void SetPromptVisibility(bool value)
    {
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetActive(value);
        }
    }

    public void Refresh()
    {
        PlayerInputController.playState playerState = PlayerInputController.instance.playerState;
        bool currentInteractable = PlayerInputController.instance.currentInteractable != null;
        bool currentCarryObject = PlayerInputController.instance.currentCarryObject != null;
        bool isInDialogue = PlayerInputController.instance.isinDialogue;
        bool isInDeliveryZone = PlayerInputController.instance.inCarryDeliveryZone;

        if (isInDialogue)
        {
            SetPromptVisibility(false);
            return;
        }


        switch (playerState)
        {
            case PlayerInputController.playState.none:
                if (currentCarryObject || currentInteractable)
                {
                    SetPromptVisibility(true);
                }
                else if (!currentCarryObject && !currentInteractable)
                {
                    SetPromptVisibility(false);
                }
                break;

            case PlayerInputController.playState.carryingObject:
                SetPromptVisibility(true);
                if (isInDeliveryZone)
                {
                    UpdateUIInfo(Interactable.PromptText.Deliver, Interactable.PromptKey.F);
                }
                else
                {
                    UpdateUIInfo(Interactable.PromptText.Drop, Interactable.PromptKey.F);
                }
                ;
                break;

            case PlayerInputController.playState.carryingNonDroppable:
                if (!currentInteractable && !isInDeliveryZone)
                {
                    SetPromptVisibility(false);
                    return;
                }
                SetPromptVisibility(true);
                if (isInDeliveryZone)
                    UpdateUIInfo(Interactable.PromptText.Deliver, Interactable.PromptKey.F);


                break;
        }
    }
}
