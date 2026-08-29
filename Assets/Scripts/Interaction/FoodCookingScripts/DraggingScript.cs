using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggingScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Camera cam;

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


    [Header("Food Model")]
    [Tooltip("The CHILD model. Only this object will rotate/jiggle.")]
    public Transform foodModel;


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


    private void Awake()
    {
        if (cam == null)
        {
            cam = FindAnyObjectByType<Camera>();
        }
    }


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        foodStatsScript =
            GetComponent<FoodStats>();


        // If no model was assigned, try to find the first child.
        if (foodModel == null)
        {
            if (transform.childCount > 0)
            {
                foodModel =
                    transform.GetChild(0);
            }
        }


        // Warn if the model is incorrectly assigned.
        if (foodModel == transform)
        {
            Debug.LogError(
                $"DraggingScript on {gameObject.name}: " +
                $"Food Model cannot be the parent object. " +
                $"Assign the CHILD model instead.",
                this);

            foodModel = null;
        }


        if (dragPlane == null)
        {
            GameObject planeObject =
                new GameObject(
                    gameObject.name + "_DragPlane"
                );

            planeObject.transform.position =
                transform.position;

            dragPlane =
                planeObject.transform;
        }
    }


    void Update()
    {
        if (CameraController.transitioning ||
            !Interactable)
        {
            return;
        }


        // ========================================================
        // DRAGGING
        // ========================================================

        if (dragging)
        {
            Vector3 target;

            if (GetMouseWorldPosition(out target))
            {
                // IMPORTANT:
                // The PARENT moves.
                //
                // This is intentional because the Rigidbody,
                // MeshCollider, FoodStats and DraggingScript
                // are all on the parent.

                transform.position =
                    Vector3.Lerp(
                        transform.position,
                        target,
                        moveSpeed * Time.deltaTime
                    );
            }
        }


        // ========================================================
        // MANUAL ROTATION WHILE DRAGGING
        // ========================================================

        if (dragging &&
            Input.GetMouseButton(1) &&
            isFood)
        {
            RotateFoodModel();
        }


        // ========================================================
        // RIGHT CLICK FLIP
        // ========================================================

        if (!dragging &&
            Input.GetMouseButtonDown(1) &&
            isFood &&
            CanBeFlipped)
        {
            CheckForFlipClick();
        }


        // ========================================================
        // DROP
        // ========================================================

        if (dragging &&
            Input.GetMouseButtonUp(0))
        {
            DropFood();
        }
    }


    // ============================================================
    // ROTATE MODEL
    // ============================================================

    private void RotateFoodModel()
    {
        if (foodModel == null)
            return;


        // ONLY THE CHILD MODEL ROTATES.
        //
        // Previously this was:
        //
        // transform.Rotate(...)
        //
        // which rotated the parent.

        foodModel.Rotate(
            0f,
            0f,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }


    // ============================================================
    // MOUSE WORLD POSITION
    // ============================================================

    private bool GetMouseWorldPosition(
        out Vector3 worldPosition)
    {
        Ray ray =
            cam.ScreenPointToRay(
                Input.mousePosition
            );


        Plane plane =
            new Plane(
                Vector3.up,
                dragPlane.position
            );


        if (plane.Raycast(
                ray,
                out float distance))
        {
            Vector3 hitPoint =
                ray.GetPoint(distance);


            hitPoint.y =
                dragPlane.position.y;


            worldPosition =
                hitPoint;


            return true;
        }


        worldPosition =
            transform.position;


        return false;
    }


    // ============================================================
    // CHECK FLIP CLICK
    // ============================================================

    void CheckForFlipClick()
    {
        Ray ray =
            cam.ScreenPointToRay(
                Input.mousePosition
            );


        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                100f))
        {
            // The collider is on the PARENT.
            //
            // So checking hit.collider.gameObject
            // against transform is correct.

            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
            {
                FlipFood();
            }
        }
    }


    // ============================================================
    // FLIP FOOD
    // ============================================================

    private void FlipFood()
    {
        if (isFlipping)
            return;


        if (foodStatsScript == null)
            return;


        if (foodModel == null)
        {
            Debug.LogError(
                $"Cannot flip {gameObject.name}: " +
                $"Food Model is not assigned.",
                this);

            return;
        }


        isFlipping = true;


        // IMPORTANT:
        // Kill ONLY animations on the model.
        //
        // Do not touch transform.DOKill()
        // because the parent is the draggable object.

        foodModel.DOKill();


        // Store the model's local rotation.
        Quaternion startRotation =
            foodModel.localRotation;


        Quaternion targetRotation =
            startRotation *
            Quaternion.Euler(
                180f,
                0f,
                0f
            );


        // ONLY ROTATE THE MODEL.
        foodModel
            .DOLocalRotateQuaternion(
                targetRotation,
                flipDuration
            )
            .SetEase(flipEase)
            .OnComplete(() =>
            {
                isFlipping = false;


                // Tell FoodStats that the flip has happened.
                //
                // FoodStats handles:
                // - currentSide
                // - cooking progress
                // - FoodFlip / FoodCooked events
                // - cooking state

                foodStatsScript.FlipFood();
            });
    }


    // ============================================================
    // BEGIN DRAG
    // ============================================================

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        MeshCollider mesh =
            GetComponent<MeshCollider>();


        if (mesh != null)
            mesh.enabled = false;


        // Kill parent movement animations if any.
        //
        // We are NOT rotating the parent anymore.
        transform.DOKill();


        // Jiggle ONLY the model.
        Jiggle();


        dragging = true;


        if (rb != null)
        {
            rb.useGravity = false;
        }


        if (foodStatsScript != null)
        {
            foodStatsScript.StopCooking();
        }
    }


    // ============================================================
    // DRAG
    // ============================================================

    public void OnDrag(
        PointerEventData eventData)
    {
        Vector3 target;


        if (GetMouseWorldPosition(
                out target))
        {
            // Parent moves.
            transform.position =
                Vector3.Lerp(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );
        }
    }


    // ============================================================
    // END DRAG
    // ============================================================

    public void OnEndDrag(
        PointerEventData eventData)
    {
        DropFood();
    }


    // ============================================================
    // DROP FOOD
    // ============================================================

    private void DropFood()
    {
        MeshCollider mesh =
            GetComponent<MeshCollider>();


        if (mesh != null)
            mesh.enabled = true;


        // Kill movement animation on parent.
        transform.DOKill();


        // Jiggle ONLY the model.
        Jiggle();


        dragging = false;


        if (rb != null)
        {
            rb.useGravity = true;
        }


        if (foodStatsScript != null)
        {
            foodStatsScript.StopCooking();
        }
    }


    // ============================================================
    // JIGGLE MODEL
    // ============================================================

    private void Jiggle()
    {
        if (foodModel == null)
            return;


        // IMPORTANT:
        // Previously this was:
        //
        // transform.DOPunchRotation(...)
        //
        // which rotated the parent.
        //
        // Now ONLY the child model jiggles.

        foodModel.DOPunchRotation(
            Random.insideUnitSphere *
            jiggleStrength *
            15f,
            jiggleDuration,
            jiggleVibrato,
            jiggleRandomness
        );
    }
}