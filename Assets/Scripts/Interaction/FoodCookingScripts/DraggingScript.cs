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

        // If no drag plane is assigned,
        // create one automatically at the food's current height.
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
            // Move while holding
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


            // Rotate ONLY local Z while holding
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


            // Flip when right clicking food
            if (!dragging &&
                Input.GetMouseButtonDown(1) &&
                isFood &&
                CanBeFlipped)
            {
                CheckForFlipClick();
            }


            // Drop
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

        /*
         * The drag plane is horizontal.
         *
         * Its position determines the height of the food.
         */
        Plane plane = new Plane(
            Vector3.up,
            dragPlane.position
        );

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            /*
             * carryDistance offsets the food away from the camera.
             *
             * This is applied along the camera's forward direction,
             * but the final position is projected back onto the
             * drag plane so the food stays at the correct height.
             */

            Vector3 adjustedPoint = hitPoint;

            // Keep the food locked to the drag plane height.
            adjustedPoint.y = dragPlane.position.y;

            worldPosition = adjustedPoint;

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

        // Kill only THIS food's tweens
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

            // Tell only this food its side changed
            foodStatsScript.FlipFood();
        });
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        MeshCollider mesh = GetComponent<MeshCollider>();

        if (mesh != null)
            mesh.enabled = false;


        transform.DOShakeScale(
            jiggleDuration,
            jiggleStrength,
            jiggleVibrato,
            jiggleRandomness,
            true,
            ShakeRandomnessMode.Harmonic
        );


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


        transform.DOShakeScale(
            jiggleDuration,
            jiggleStrength,
            jiggleVibrato,
            jiggleRandomness,
            true,
            ShakeRandomnessMode.Harmonic
        );


        dragging = false;

        rb.useGravity = true;

        if (foodStatsScript != null)
            foodStatsScript.StopCooking();
    }
}