using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;

public class currentTaskManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI textTask;
    [SerializeField] private GameObject container;


    [SerializeField] private UnityEvent onUpdateTask;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    public void updateTask(string text)
    {
        textTask.text = text;
        onUpdateTask?.Invoke();
    }

    public void onTaskVisibility(bool option)
    {
        foreach (Transform child in container.transform)
        {
            child.gameObject.SetActive(option);
        }
        
    }
}
