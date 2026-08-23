using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections;

public class currentTaskManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI textTask;
    [SerializeField] private GameObject container;


    [SerializeField] private UnityEvent onUpdateTask;

    private UITweener[] tweeners;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        tweeners = GetComponentsInChildren<UITweener>(true);
    }
    // Update is called once per frame
    public void updateTask(string text)
    {
        int target = textTask.textInfo.characterCount;
        textTask.maxVisibleCharacters = 0;
        textTask.text = text;
        DOTween.To(() => textTask.maxVisibleCharacters, x => textTask.maxVisibleCharacters = x, target, 1f);

        onUpdateTask?.Invoke();
    }

    public void onTaskVisibility(bool option)
    {
        foreach (UITweener tweens in tweeners)
        {
            tweens.enabled = option;
        }
        
    }
}
