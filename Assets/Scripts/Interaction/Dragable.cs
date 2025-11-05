using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Dragable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform parentAfterDrag;
    private Image image;
    private Canvas canvas;
    private Camera uiCamera;

    private void Start()
    {
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();

        // Use the canvas' assigned camera if in World Space
        uiCamera = canvas.renderMode == RenderMode.WorldSpace ? canvas.worldCamera : null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;

        // Reparent to the canvas so it stays visible above other elements
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

      
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransform canvasRect = canvas.transform as RectTransform;

        // Convert screen position to world or local space correctly
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            uiCamera,
            out localPoint))
        {
            transform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Return to original parent
        transform.SetParent(parentAfterDrag);

        // Keep z position consistent (important for world-space canvases)
        Vector3 position = transform.localPosition;
        position.z = 0;
        transform.localPosition = position;

  
    }
}