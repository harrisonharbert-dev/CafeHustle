using UnityEngine;
using DG.Tweening;

public class ObjectInOutTweener : MonoBehaviour
{

    [SerializeField] private float FromHeight = 1f;
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float rotationAmount = 45f;

    [Header("Squash & Stretch")]
    private Vector3 stretchScale = new Vector3(0.7f, 1.4f, 0.7f); // Tall and skinny while falling
    private Vector3 squashScale = new Vector3(1.4f, 0.5f, 1.4f);  // Flat and wide on impact
    private Vector3 originalScale;
    

    void OnEnable()
    {
        FallIn();
    }


    [ContextMenu("FallIn")]
    void FallIn()
    {
        originalScale = transform.localScale;
        Vector3 initialPosition = transform.position;
        Quaternion initialRotation = transform.rotation;
        Vector3 stretchedScale = Vector3.Scale(originalScale, stretchScale);
        Vector3 squashedScale = Vector3.Scale(originalScale, squashScale);
        
        transform.position = initialPosition + new Vector3(0f,FromHeight,0f);
        transform.localScale = stretchedScale;
        


         // Create a sequence to run position and rotation in parallel
        Sequence bounceSequence = DOTween.Sequence();

        // 1. The Fall and Bounce Position
        bounceSequence.Join(transform.DOMove(initialPosition, duration).SetEase(Ease.OutBounce));

        // Squash on impact, then restore the object's original scale.
        Sequence scaleSequence = DOTween.Sequence();
        scaleSequence.Append(transform.DOScale(squashedScale, duration * 0.25f).SetEase(Ease.OutQuad));
        scaleSequence.Append(transform.DOScale(originalScale, duration * 0.25f).SetEase(Ease.OutBack));
        bounceSequence.Join(scaleSequence);

        // 2. The Rotation Wobble (Rotates forward, then settles back to 0)
        bounceSequence.Join(transform.DOShakeRotation(duration, rotationAmount)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // Rotates back to straight when it hits the ground/bounces
                transform.DORotateQuaternion(initialRotation, duration * 0.6f).SetEase(Ease.OutElastic);
            }));
    }
    
    
}
