using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class InOutUITweener : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private bool scaleIn;
    [SerializeField] private bool rotateIn;
    [SerializeField] private bool slideIn;
    [Space(30)]
    [SerializeField] private bool scaleOut;
    [SerializeField] private bool rotateOut;
    [SerializeField] private bool slideOut;

    [Header("Properties")]
    [SerializeField] private bool animateWithActive = false;
    [Space(30)]
    [SerializeField] private float duration = 0.45f;
    [SerializeField] private Vector2 slideDirection;
    [SerializeField] private float rotationAmount;
    [SerializeField] private Ease easeType = Ease.OutQuad;

    [SerializeField] private float delay;
    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 initialPos;
    private Vector3 initialScale;

    private Quaternion initialRotation;
    private Quaternion targetRotation;

    private readonly object InTweenId = new object();
    private readonly object OutTweenId = new object();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Cache components if not assigned in Inspector
        if (rect == null) rect = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        initialPos = rect.anchoredPosition;
        initialScale = rect.localScale;
        initialRotation = rect.rotation;

        targetRotation = initialRotation * Quaternion.Euler(0f, 0f, rotationAmount);
        
    }
    void OnEnable()
    {
        if(animateWithActive)
        {
            AnimateIn();
        }
    }

    void OnDisable()
    {
        if(animateWithActive)
        {
            AnimateOut();
        }
    }
    public void AnimateIn()
    {
        DOTween.Complete(InTweenId);
        if (scaleIn)
        {
            rect.localScale = Vector3.zero;
        }

        if (slideIn)
        {
            rect.anchoredPosition = initialPos - slideDirection;
        }

        if (rotateIn)
        {
            rect.rotation = targetRotation;
        }

        Sequence InSequence = DOTween.Sequence();

        //Fade in applies to all
        InSequence.Join(canvasGroup.DOFade(1f, duration).SetEase(easeType));

        //Apply effects from bools
        if (scaleIn) { InSequence.Join(rect.DOScale(initialScale, duration).SetEase(easeType)); }

        if (slideIn) {InSequence.Join(rect.DOAnchorPos(initialPos, duration).SetEase(easeType));}

        if (rotateIn) { InSequence.Join(rect.DOLocalRotateQuaternion(initialRotation, duration).SetEase(easeType)); }

        // Apply the delay after adding the tweens so it delays the entire sequence.
        InSequence.SetDelay(delay);
        InSequence.SetId(InTweenId);

    }

    public void AnimateOut()
    {
        DOTween.Complete(OutTweenId);

        if (scaleOut)
        {
            rect.localScale = initialScale;
        }

        if (slideOut)
        {
            rect.anchoredPosition = initialPos;
        }

        if (rotateOut)
        {
            rect.rotation = initialRotation;
        }

        Sequence OutSequence = DOTween.Sequence();

        

        //Fade in applies to all
        OutSequence.Join(canvasGroup.DOFade(0f, duration).SetEase(easeType));

        //Apply effects from bools
        if (scaleIn) { OutSequence.Join(rect.DOScale(Vector3.zero, duration).SetEase(easeType)); }

        if (slideIn) {OutSequence.Join(rect.DOAnchorPos(initialPos + slideDirection, duration).SetEase(easeType));}

        if (rotateIn) { OutSequence.Join(rect.DOLocalRotateQuaternion(targetRotation, duration).SetEase(easeType)); }

        //Add delays and set id
        OutSequence.SetDelay(delay);
        OutSequence.SetId(InTweenId);
        
    }
}
