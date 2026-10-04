using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Events;

public class PressToStart : MonoBehaviour
{

    [SerializeField] private UnityEvent ClickEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(waitForStart());
    }

    IEnumerator waitForStart()
    {
        yield return new WaitUntil(()=> Input.GetMouseButtonDown(0));

        ClickEvent?.Invoke();
    }
}
