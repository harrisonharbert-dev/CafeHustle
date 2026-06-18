using UnityEngine;
using DG.Tweening;
public class AnimationController : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private Vector3 ScaleFactor;
    private Vector2 originalPosition;
    private Vector3 originalScale;
    public bool Expanded;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Expanded = false;
    }
    private void Awake()
    {
        if (target != null)
        {
               originalPosition = target.anchoredPosition;
        originalScale = target.localScale;
        }
    
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void Expand()
    {
        target.DOAnchorPos(Vector2.zero, 0.4f)
              .SetEase(Ease.OutQuad);

        target.DOScale(Vector3.Scale(originalScale, ScaleFactor), 0.4f)
              .SetEase(Ease.OutBack);
    }

    public void Collapse()
    {
        target.DOAnchorPos(originalPosition, 0.3f)
           .SetEase(Ease.InOutQuad);

        target.DOScale(originalScale, 0.3f)
              .SetEase(Ease.InBack);
    }

    public void InFocus()
    {
       if (Expanded == true) {             Collapse();
            Expanded = false;
        }
       else
        {
            Expand();
            Expanded = true;
        }
    }
}

