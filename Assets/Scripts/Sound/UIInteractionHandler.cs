using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIInteractionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private UnityEvent OnPointerEnterEvent;
    [SerializeField] private UnityEvent OnPointerExitEvent;
    [SerializeField] private UnityEvent OnPointerDownEvent;
    [SerializeField] private UnityEvent OnPointerUpEvent;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(OnPointerEnterEvent!=null)
        {
            OnPointerEnterEvent?.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(OnPointerExitEvent!=null)
        {
            OnPointerExitEvent?.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(OnPointerDownEvent!=null)
        {
            OnPointerDownEvent?.Invoke();
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if(OnPointerEnterEvent!=null)
        {
            OnPointerUpEvent?.Invoke();
        }
    }

}
