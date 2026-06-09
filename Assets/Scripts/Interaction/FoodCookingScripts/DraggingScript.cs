using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Diagnostics.SymbolStore;

public class DraggingScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Camera cam;
    [Header("Carry Settings")]
    public float carryDistance = 2f;
    public float moveSpeed = 15f;
    private Rigidbody rb;
    public bool dragging;
    public bool isMeat;
    [SerializeField] private bool isFood;
    public float rotationSpeed;
  public FoodStats foodStatsScript;
    void Start()
    {
        foodStatsScript = GetComponent<FoodStats>();
        rb = GetComponent<Rigidbody>();
    }
    public void Update()
    {
      if (Input.GetMouseButton(1) && isFood == true && dragging == true)
        {
            this.gameObject.transform.Rotate(Vector3.right * Time.deltaTime * rotationSpeed);
        }
        
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Started dragging: " + gameObject.name);
   
        // Disable collider to prevent physics interference
        dragging = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        GetComponent<MeshCollider>().enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragging = true;
       
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
        GetComponent<MeshCollider>().enabled = true;
        dragging = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        foodStatsScript.StopCooking();
    }


}
