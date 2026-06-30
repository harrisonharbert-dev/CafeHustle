using UnityEngine;
using DG.Tweening;

public class InteractPrompt : MonoBehaviour
{

    private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    
    public void SetPromptVisibility(bool value)
    {
            float target = value ? 1f : 0f;
            canvasGroup.DOFade(target, fadeDuration);
    }
}
