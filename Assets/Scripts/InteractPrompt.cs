using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using CsvHelper.Configuration.Attributes;
using Unity.VisualScripting;
using UnityEngine.UI;







public class InteractPrompt : MonoBehaviour
{


    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    [SerializeField] private float fadeDuration;


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


    public void SetPromptVisibility(bool value)
    {
        float target = value ? 1f : 0f;
        if (target == canvasGroup.alpha) return;
        if (PlayerInputController.instance.lockMovement) { target = 0f; }
        ;
        canvasGroup.DOFade(target, fadeDuration);

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
}
