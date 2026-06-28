using DG.Tweening;
using System.Collections;
using System.Diagnostics.SymbolStore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggingScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    private Camera cam;

    [Header("Carry Settings")]
    public float carryDistance = 2f;
    public float moveSpeed = 15f;
    public float rotationSpeed;
    private Rigidbody rb;
    [HideInInspector] public bool dragging;
    [SerializeField] public bool isMeat;
    [SerializeField] private bool isFood;


    [Header("Reactivity Settings")]
    [SerializeField] private float jiggleDuration = 0.3f;
    [SerializeField][Range(0f, 1f)] private float jiggleStrength = 0.3f;
    [SerializeField] private int jiggleVibrato = 10;
    [SerializeField][Range(0f, 180f)] private float jiggleRandomness = 90f;

    [HideInInspector] public FoodStats foodStatsScript;
    void Start()
    {
        foodStatsScript = GetComponent<FoodStats>();
        rb = GetComponent<Rigidbody>();
        cam = Camera.main; 
    }
    void Update()
        {
            if (dragging)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = carryDistance;

                Vector3 targetPos = cam.ScreenToWorldPoint(mousePos);

                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );
            }

            if (Input.GetMouseButton(1) && isFood && dragging)
            {
                transform.Rotate(
                    Vector3.up * Time.deltaTime * rotationSpeed,
                    Space.World
                );
            }                 
        // Drop when left mouse is released
        if (dragging && Input.GetMouseButtonUp(0))
        {
            GetComponent<MeshCollider>().enabled = true;

            dragging = false;
            rb.useGravity = true;
           

            if (foodStatsScript != null)
                foodStatsScript.StopCooking();
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Started dragging: " + gameObject.name);
        this.gameObject.GetComponent<MeshCollider>().enabled = false;
        // Disable collider to prevent physics interference
        transform.DOShakeScale(jiggleDuration, jiggleStrength, jiggleVibrato, jiggleRandomness, true, ShakeRandomnessMode.Harmonic);
        foodStatsScript.isCooking = false;
        dragging = true;
        rb.useGravity = false;
      
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
        this.gameObject.GetComponent<MeshCollider>().enabled = true;

        transform.DOShakeScale(jiggleDuration, jiggleStrength, jiggleVibrato, jiggleRandomness, true, ShakeRandomnessMode.Harmonic);
        dragging = false;
        rb.useGravity = true;
        foodStatsScript.StopCooking();
    }


}
