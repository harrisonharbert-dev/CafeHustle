using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class UIAnimator : MonoBehaviour
{
    private RectTransform rect;
    [System.Serializable]
    public class WaveSettings
    {
        public float waveHeight = 1f;
        public float waveDuration = 1f;
    }

    [System.Serializable] 
    public class ScaleSettings
    {
        public Vector3 scaleSize = new Vector3(1f,1f,1f);
        public float scaleDuration = 1f;
    }

    [System.Serializable]
    public class RotateSettings
    {
        public float rotateAmount;
        public float duration;
    }

    [SerializeField] private RotateSettings rotateSetting;
    [SerializeField] private WaveSettings waveSetting;
    [SerializeField] private ScaleSettings scaleSetting;
    [SerializeField] private UnityEvent startEvents;
    void Start()
    {
        rect = GetComponent<RectTransform>();

        startEvents?.Invoke();
    }
    public void Wave()
    {
        float midpoint = rect.position.y;
        rect.DOMoveY(midpoint + waveSetting.waveHeight, waveSetting.waveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void Throb()
    {
        Vector3 scale = rect.localScale;
        rect.DOScale(scale + scaleSetting.scaleSize, scaleSetting.scaleDuration)
        .SetEase(Ease.InOutSine)
        .SetLoops(-1,LoopType.Yoyo);
    }

    public void Rotate()
    {
        rect.DORotate(new Vector3(0f, 0f, rotateSetting.rotateAmount), rotateSetting.duration, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
