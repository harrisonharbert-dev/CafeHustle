using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggingScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Camera cam;
    [Header("Carry Settings")]
    public float carryDistance = 2f;
    public float moveSpeed = 15f;
    private Rigidbody rb;
    public bool dragging;
    public bool isMeat;
    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        GetComponent<BoxCollider>().enabled = false; // Disable collider to prevent physics interference
        dragging = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent rotation while dragging
        rb.constraints |= RigidbodyConstraints.FreezeAll; // Freeze all movement while dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragging = true;
        GetComponent<BoxCollider>().enabled = false; // Disable collider to prevent physics interference
        // Mouse position on screen
        Vector3 mousePos = eventData.position;

        // Set depth from camera
        mousePos.z = carryDistance;

        // Convert to world position near camera
        Vector3 targetPos = cam.ScreenToWorldPoint(mousePos);

        // Smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        rb.useGravity = true;
        GetComponent<BoxCollider>().enabled = true; // Re-enable collider after dragging
            rb.constraints = RigidbodyConstraints.None; // Unfreeze all constraints
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Keep rotation frozen after dropping
    }


}
