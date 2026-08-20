using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggingScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Camera cam;

    public float moveSpeed = 15f;

    [Tooltip("The height/plane that the food will move across while being dragged.")]
    public Transform dragPlane;

    [Header("Food Rotation")]
    public float rotationSpeed = 120f;

    [Header("Flip Animation")]
    public float flipDuration = 0.5f;
    public Ease flipEase = Ease.InOutSine;

    [Header("Food")]
    public bool isFood = true;

    [Header("Reactivity")]
    [SerializeField] private float jiggleDuration = 0.3f;
    [SerializeField][Range(0f, 1f)] private float jiggleStrength = 0.3f;
    [SerializeField] private int jiggleVibrato = 10;
    [SerializeField][Range(0f, 180f)] private float jiggleRandomness = 90f;

    private Rigidbody rb;

    [HideInInspector]
    public bool dragging;

    private bool isFlipping;

    public bool CanBeFlipped;
    public bool Interactable;

    [HideInInspector]
    public FoodStats foodStatsScript;


    void Start()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody>();

        foodStatsScript = GetComponent<FoodStats>();

        if (dragPlane == null)
        {
            GameObject planeObject = new GameObject(
                gameObject.name + "_DragPlane"
            );

            planeObject.transform.position = transform.position;

            dragPlane = planeObject.transform;
        }
    }


    void Update()
    {
        if (CameraController.transitioning == false && Interactable == true)
        {
            if (dragging)
            {
                Vector3 target;

                if (GetMouseWorldPosition(out target))
                {
                    transform.position = Vector3.Lerp(
                        transform.position,
                        target,
                        moveSpeed * Time.deltaTime
                    );
                }
            }


            if (dragging &&
                Input.GetMouseButton(1) &&
                isFood)
            {
                transform.Rotate(
                    0f,
                    0f,
                    rotationSpeed * Time.deltaTime,
                    Space.Self
                );
            }


            if (!dragging &&
                Input.GetMouseButtonDown(1) &&
                isFood &&
                CanBeFlipped)
            {
                CheckForFlipClick();
            }


            if (dragging &&
                Input.GetMouseButtonUp(0))
            {
                DropFood();
            }
        }
    }


    private bool GetMouseWorldPosition(out Vector3 worldPosition)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(
            Vector3.up,
            dragPlane.position
        );

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            hitPoint.y = dragPlane.position.y;

            worldPosition = hitPoint;

            return true;
        }

        worldPosition = transform.position;

        return false;
    }


    void CheckForFlipClick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                FlipFood();
            }
        }
    }


    private void FlipFood()
    {
        if (isFlipping)
            return;

        if (foodStatsScript == null)
            return;

        isFlipping = true;

        transform.DOKill();

        Vector3 targetRotation = transform.localEulerAngles;

        targetRotation.x += 180f;

        transform.DOLocalRotate(
            targetRotation,
            flipDuration,
            RotateMode.FastBeyond360
        )
        .SetEase(flipEase)
        .OnComplete(() =>
        {
            isFlipping = false;

            foodStatsScript.FlipFood();
        });
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        MeshCollider mesh = GetComponent<MeshCollider>();

        if (mesh != null)
            mesh.enabled = false;

        // Kill any previous animation on this food.
        transform.DOKill();

        // IMPORTANT:
        // Do NOT use DOShakeScale.
        // Scale is never modified.

        Jiggle();

        dragging = true;

        rb.useGravity = false;

        if (foodStatsScript != null)
            foodStatsScript.StopCooking();
    }


    public void OnDrag(PointerEventData eventData)
    {
        Vector3 target;

        if (GetMouseWorldPosition(out target))
        {
            transform.position = Vector3.Lerp(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        DropFood();
    }


    private void DropFood()
    {
        MeshCollider mesh = GetComponent<MeshCollider>();

        if (mesh != null)
            mesh.enabled = true;

        // Kill any existing jiggle.
        transform.DOKill();

        // Jiggle without touching scale.
        Jiggle();

        dragging = false;

        rb.useGravity = true;

        if (foodStatsScript != null)
            foodStatsScript.StopCooking();
    }


    private void Jiggle()
    {
        // Small rotation jiggle instead of scale jiggle.
        transform.DOPunchRotation(
            Random.insideUnitSphere * jiggleStrength * 15f,
            jiggleDuration,
            jiggleVibrato,
            jiggleRandomness
        );
    }
}