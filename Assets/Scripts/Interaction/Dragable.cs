using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Dragable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
   Transform parentAfterDrag;
    Image image;
    public void Start()
    {
        image = GetComponent<Image>();
    }
public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;

        // Ensure the object is reparented to the root Canvas (or stays in the correct sorting layer)
        Canvas canvas = GetComponentInParent<Canvas>();
        transform.SetParent(canvas.transform);

        // Set as last sibling so it stays on top
        transform.SetAsLastSibling();

        //image.raycastTarget = false; // Disable raycast during drag
    }


    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
        transform.parent as RectTransform,
        eventData.position, // Use eventData.position instead of Input.mousePosition
        eventData.pressEventCamera,
        out Vector2 localPoint);
        transform.localPosition = new Vector3(localPoint.x, localPoint.y, 0);

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        // Return to the original parent
        transform.SetParent(parentAfterDrag);

        // Reset z position to 0
        Vector3 position = transform.localPosition;
        position.z = 0;
        transform.localPosition = position;

        // Re-enable raycast after dragging
        image.raycastTarget = true;
    }
}
