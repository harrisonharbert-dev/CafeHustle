using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class HotbarSlot : MonoBehaviour, IPointerDownHandler
{
    public Hotbar hotbar;
    public int slotIndex;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        
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
}