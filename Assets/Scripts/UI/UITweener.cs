using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity.Attributes;
using System;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class UITweener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private GameObject objectToAnimate;



    [System.Serializable]
    public class FadeSettings
    {
        public float duration = 0.3f;
        public float delay = 0f;
    }



    [System.Serializable]
    public class ScaleSettings
    {
        public float duration = 0.3f;
        public float delay = 0f;
    }


    [System.Serializable]
    public class PunchSettings
    {
        public Vector3 Punch = new Vector3(0.15f, 0.15f, 0.15f);
        public float duration = 0.3f;
        public int vibrato = 10;
        public float elasticity = 1f;
        public float delay = 0f;
    }

    [System.Serializable]
    public class ShakeSettings
    {
        public Vector3 shake = new Vector3(0.15f, 0.15f, 0.15f);
        public float duration = 0.3f;
        public int vibrato = 10;
        public float randomness = 90f;
        public bool fadeOut = true;
        public ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full;

        public float delay;
    }

    [System.Serializable]
    public class SlideSettings
    {
        public Vector3 distance = new Vector3(0f, -15f, 0f);
        public float duration = 0.3f;

        public float delay = 0f;

    }

    [System.Serializable]
    public class ColorSettings
    {
        public Color targetColor = new Vector4(1f, 1f, 1f);
        public float duration = 0.3f;

        public float delay = 0f;
    }

    [System.Serializable]
    public class TextSettings
    {
        public TextMeshProUGUI text;
        public float duration;
        public float delay;

    }


    [Header("Animation Settings")]
    [SerializeField] private FadeSettings fadeSettings;
    [SerializeField] private ScaleSettings scaleSettings;
    [SerializeField] private PunchSettings punchSettings;
    [SerializeField] private ShakeSettings shakeSettings;
    [SerializeField] private SlideSettings slideSettings;
    [SerializeField] private ColorSettings colorSettings;
    [SerializeField] private TextSettings textSettings;

    //private references
    private CanvasGroup group;
    private RectTransform rect;
    private Vector3 scale;
    private Vector2 startingPos;
    private Image image;
    private Color startingCol;
    private bool isWaitingForDependency = false;

    [SerializeField] private UnityEvent onVisibleEvent;
    [SerializeField] private UnityEvent onDisableEvent;
    [SerializeField] private UnityEvent onHoverEvent;
    [SerializeField] private UnityEvent onLeaveEvent;
    // Awake runs before OnEnable, ensuring fields are initialized
    private void Awake()
    {
        if (objectToAnimate == null)
        {
            objectToAnimate = gameObject;
            group = GetComponent<CanvasGroup>();
            rect = GetComponent<RectTransform>();
            scale = transform.localScale;
            startingPos = rect.anchoredPosition;
            image = GetComponent<Image>();
            if (image != null)
            {
                startingCol = image.color;
            }
        }
        else
        {
            group = objectToAnimate.GetComponent<CanvasGroup>();
            rect = objectToAnimate.GetComponent<RectTransform>();
            scale = objectToAnimate.transform.localScale;
            startingPos = rect.anchoredPosition;
            image = objectToAnimate.GetComponent<Image>();
            if (image != null)
            {
                startingCol = image.color;
            }
        }
    }


    void OnEnable()
    {
        onVisibleEvent?.Invoke();

    }

    void OnDisable()
    {
        onDisableEvent?.Invoke();
    }


    public bool checkForActiveTweens()
    {
        bool isTweening = DOTween.IsTweening(rect, true) || DOTween.IsTweening(group, true) || DOTween.IsTweening(image, true);
        return isTweening;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onLeaveEvent?.Invoke();
    }

    public void Fade(bool option)
    {
        float target = option ? 1f : 0f;
        float start = option ? 0f : 1f;

        if (group == null) return;
        group.alpha = start;
        group.DOFade(target, fadeSettings.duration).SetDelay(fadeSettings.delay);
    }

    public void Scale(bool option)
    {
        //Set target to scale to
        float targetx = option ? scale.x : 0f;
        float targety = option ? scale.y : 0f;
        float targetz = option ? scale.z : 0f;

        //Set the scale to opposite of target when starting
        float startx = option ? 0f : scale.x;
        float starty = option ? 0f : scale.y;
        float startz = option ? 0f : scale.z;

        if (rect == null) return;
        rect.localScale = new Vector3(startx, starty, startz);

        rect.DOScaleX(targetx, scaleSettings.duration).SetDelay(scaleSettings.delay);
        rect.DOScaleY(targety, scaleSettings.duration).SetDelay(scaleSettings.delay);
        rect.DOScaleZ(targetz, scaleSettings.duration).SetDelay(scaleSettings.delay);
    }

    public void Slide(bool option)
    {

        //Set target to scale to
        float targetx = option ? startingPos.x : startingPos.x + slideSettings.distance.x;
        float targety = option ? startingPos.y : startingPos.y + slideSettings.distance.y;
        //Set the scale to opposite of target when starting
        float startx = option ? startingPos.x + slideSettings.distance.x : startingPos.x;
        float starty = option ? startingPos.y + slideSettings.distance.y : startingPos.y;

        if (rect == null) return;
        rect.anchoredPosition = new Vector2(startx, starty);

        rect.DOAnchorPos(new Vector3(targetx, targety), slideSettings.duration).SetDelay(slideSettings.delay);
    }

    public void Punch()
    {
        rect.DOPunchScale(punchSettings.Punch, punchSettings.duration, punchSettings.vibrato, punchSettings.elasticity).SetDelay(punchSettings.delay);
    }

    public void ShakeScale()
    {
        rect.DOShakeScale(shakeSettings.duration, shakeSettings.shake, shakeSettings.vibrato, shakeSettings.randomness, shakeSettings.fadeOut, shakeSettings.randomnessMode).SetDelay(shakeSettings.delay);
    }

    public void ShakeRotation()
    {
        rect.DOShakeRotation(shakeSettings.duration, shakeSettings.shake, shakeSettings.vibrato, shakeSettings.randomness, shakeSettings.fadeOut, shakeSettings.randomnessMode).SetDelay(shakeSettings.delay);
    }

    public void Color()
    {
        image.DOColor(colorSettings.targetColor, colorSettings.duration).SetDelay(colorSettings.delay);
    }

    public void Text(bool option)
    {
        if (textSettings.text == null) return;
        Debug.Log("Text ran");
        int targetCharacters = option ? textSettings.text.textInfo.characterCount : 0;

        //set to inverse of target
        textSettings.text.maxVisibleCharacters = option ? 0 : textSettings.text.textInfo.characterCount;
        DOTween.To(() => textSettings.text.maxVisibleCharacters, x => textSettings.text.maxVisibleCharacters = x, targetCharacters, textSettings.duration).SetDelay(textSettings.delay);
    }

    public void FadeAndScale(bool option)
    {
        Fade(option);
        Scale(option);
    }

    public void FadeAndSlide(bool option)
    {
        Fade(option);
        Slide(option);
    }
}