using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
public class Clickable : MonoBehaviour
{
    public UnityEvent Action;
    public UnityEvent HoverIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnMouseDown()
    {
        
            Action.Invoke();
           Debug.Log("The click event was invoked on: " + this);
        
    }

    void OnMouseOver()
    {
        HoverIcon.Invoke();
    }
}
