using UnityEngine;
using DG.Tweening;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private Vector3 ScaleFactor;

    [Header("Idle Animation")]
    [SerializeField] private float idleScaleAmount = 1.1f;
    [SerializeField] private float idleDuration = 0.7f;

    private Vector2 originalPosition;
    private Vector3 originalScale;

    public bool Expanded;

    private void Awake()
    {
        if (target != null)
        {
            originalPosition = target.anchoredPosition;
            originalScale = target.localScale;
        }
    }

    private void Start()
    {
        Expanded = false;

        StartIdleAnimation();
    }

    private void StartIdleAnimation()
    {
        if (target == null)
            return;

        target.DOKill();

        target.localScale = originalScale;

        target.DOScale(
            originalScale * idleScaleAmount,
            idleDuration
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }

    public void Expand()
    {
        if (target == null)
            return;

        // Stop the idle pulsing.
        target.DOKill();

        target.DOAnchorPos(Vector2.zero, 0.4f)
            .SetEase(Ease.OutQuad);

        target.DOScale(
            Vector3.Scale(originalScale, ScaleFactor),
            0.4f
        )
        .SetEase(Ease.OutBack);
    }

    public void Collapse()
    {
        if (target == null)
            return;

        // Stop the current animation.
        target.DOKill();

        target.DOAnchorPos(originalPosition, 0.3f)
            .SetEase(Ease.InOutQuad);

        target.DOScale(originalScale, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(StartIdleAnimation);
    }

    public void InFocus()
    {
        if (Expanded)
        {
            Collapse();
            Expanded = false;
        }
        else
        {
            Expand();
            Expanded = true;
        }
    }
}