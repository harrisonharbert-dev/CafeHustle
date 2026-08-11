using UnityEngine;
using TMPro;
using DG.Tweening;

public class currentTaskManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI textTask;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup group;

    [Header("Animation Properties")]
    [SerializeField] private Vector3 punchScale = new Vector3(.1f,.1f,.1f);
    [SerializeField] private float punchDuration = 1f;
    [SerializeField] private int vibrato = 10;
    [SerializeField] private float elasticity = 1f;
    [Space(15)]
    [SerializeField] private float fadeDuration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    public void updateTask(string text)
    {
        textTask.text = text;
        rectTransform.DOPunchScale(punchScale,punchDuration,vibrato,elasticity);
    }

    public void onTaskVisibility(bool option)
    {
        float targetAlpha = option ? 1f : 0f;
        group.DOFade(targetAlpha,fadeDuration);
        
    }
}
