using DG.Tweening;
using System.Threading.Tasks.Sources;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class HotbarSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Hotbar hotbar;
    public int slotIndex;
    [SerializeField] private Camera cam;
    private Vector3 originalScale;
    private const float IncreasedScale = 1.5f;
   
    private void Start()
    {
        cam = Camera.main;
        originalScale = transform.localScale;
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject prefab = hotbar.GetPrefab(slotIndex);

        if (prefab == null)
            return;

        // Spawn in front of the camera
        Vector3 mouse = Input.mousePosition;
        mouse.z = 2f;

        Vector3 worldPos = cam.ScreenToWorldPoint(mouse);

        GameObject obj = Instantiate(
    prefab,
    worldPos,
    prefab.transform.rotation
);

        // Begin dragging immediately
        DraggingScript drag = obj.GetComponent<DraggingScript>();

        if (drag != null)
        {
            drag.dragging = true;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            MeshCollider col = obj.GetComponent<MeshCollider>();
            if (col != null)
                col.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(originalScale * IncreasedScale, 0.2f)
            .SetEase(Ease.OutBack);
        Debug.Log("Hovering over hotbar slot " + slotIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(originalScale, 0.2f)
            .SetEase(Ease.OutBack);
        Debug.Log("Stopped hovering over hotbar slot " + slotIndex);
    }
}