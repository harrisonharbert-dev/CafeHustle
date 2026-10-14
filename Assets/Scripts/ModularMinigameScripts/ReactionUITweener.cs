using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ReactionUITweener : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private float hoverScale = 0.1f;
    [SerializeField] private float rotateStrength = 10f;
    [SerializeField] private Vector3 punchStrength = Vector3.one * 0.1f;
    [SerializeField] private Ease easeType;
    private RectTransform rect;

    private Vector3 initialScale;
    private readonly object scaleTweenId = new object();
    private readonly object punchTweenId = new object();
    private readonly object scaleToTweenId = new object();

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        initialScale = rect.localScale;
    }

    public void OnScale(bool option)
    {
        //Kill any existing OnScale tweens
        DOTween.Kill(scaleTweenId);


        //Set target from boolean
        Vector3 targetScale = option ? initialScale + new Vector3(hoverScale, hoverScale, hoverScale) : initialScale;


        Sequence scaleSequence = DOTween.Sequence();
        //Set ID so it can kill this tween if interrupted
        scaleSequence.SetId(scaleTweenId);
        scaleSequence.Join(rect.DOScale(targetScale, duration).SetEase(easeType));
    }

    public void Punch()
    {
        DOTween.Complete(punchTweenId);

        Sequence punchSequence = DOTween.Sequence();
        punchSequence.SetId(punchTweenId);
        punchSequence.Join(rect.DOPunchScale(punchStrength,duration).SetEase(easeType));
        punchSequence.Join(rect.DOShakeRotation(duration,rotateStrength).SetEase(easeType));
        
    }

    public void ScaleTo(float target)
    {
        DOTween.Kill(scaleToTweenId);

        Vector3 targetScale = initialScale + new Vector3(target,target,target);

        Sequence scaleToSequence = DOTween.Sequence();
        scaleToSequence.SetId(scaleToTweenId);
        scaleToSequence.Join(rect.DOScale(targetScale,duration).SetEase(easeType));
        
    }

}
