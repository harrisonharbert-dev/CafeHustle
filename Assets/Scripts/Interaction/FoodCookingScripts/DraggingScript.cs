using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DraggingScript : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    public enum FlipDirection
    {
        Forward,
        Backward
    }

    [SerializeField] private Camera cam;
    public float moveSpeed = 15f;
    public Transform dragPlane;

    [Header("Food Rotation")]
    public float rotationSpeed = 120f;
    public RotationAxis rotationAxis = RotationAxis.Z;
    public bool reverseRotation;

    [Header("Flip Animation")]
    public float flipDuration = 0.5f;
    public Ease flipEase = Ease.InOutSine;
    public RotationAxis flipAxis = RotationAxis.X;
    public FlipDirection flipDirection = FlipDirection.Forward;

    [Header("Food")]
    public bool isFood = true;

    [Header("Food Model")]
    public Transform foodModel;

    [Header("Reactivity")]
    [SerializeField] private float jiggleDuration = 0.3f;
    [SerializeField, Range(0f, 1f)] private float jiggleStrength = 0.3f;
    [SerializeField] private int jiggleVibrato = 10;
    [SerializeField, Range(0f, 180f)] private float jiggleRandomness = 90f;

    [Header("UnityEvents")]
    [SerializeField] private UnityEvent onHoverEvent;
    [SerializeField] private UnityEvent onHoverExitEvent;

    [Header("Screen Bounds")]
    [SerializeField] private float screenPadding = 50f;

    private Rigidbody rb;
    private Collider foodCollider;

    [HideInInspector]
    public bool dragging;

    public bool CanBeFlipped;
    public bool Interactable;

    [HideInInspector]
    public FoodStats foodStatsScript;

    private bool isFlipping;
    private bool flipInputLocked;

    private Tween flipTween;
    private Tween jiggleTween;

    private void Awake()
    {
        if (cam == null)
            cam = FindAnyObjectByType<Camera>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        foodStatsScript = GetComponent<FoodStats>();

        if (foodModel == null && transform.childCount > 0)
            foodModel = transform.GetChild(0);

        if (foodModel == transform)
        {
            Debug.LogError("Food Model must be the CHILD model.", this);
            foodModel = null;
        }

        // Collider is now allowed to be on the child food model
        if (foodModel != null)
            foodCollider = foodModel.GetComponent<Collider>();

        if (dragPlane == null)
        {
            GameObject plane = new GameObject(gameObject.name + "_DragPlane");
            plane.transform.position = transform.position;
            dragPlane = plane.transform;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(1))
            flipInputLocked = false;

        if (CameraController.transitioning || !Interactable)
            return;

        if (dragging)
        {
            if (GetMouseWorldPosition(out Vector3 target))
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );
            }
        }

        // Manual rotation
        if (dragging &&
            Input.GetMouseButton(1) &&
            isFood &&
            !isFlipping)
        {
            RotateFoodModel();
        }

        // Flip
        if (!dragging &&
            Input.GetMouseButtonDown(1) &&
            !flipInputLocked &&
            !isFlipping &&
            isFood &&
            CanBeFlipped)
        {
            flipInputLocked = true;
            CheckForFlipClick();
        }

        if (dragging && Input.GetMouseButtonUp(0))
            DropFood();
    }

    // ============================================================
    // ROTATION
    // ============================================================

    private void RotateFoodModel()
    {
        if (foodModel == null)
            return;

        float amount = rotationSpeed * Time.deltaTime;

        if (reverseRotation)
            amount *= -1f;

        foodModel.Rotate(
            GetAxis(rotationAxis),
            amount,
            Space.Self
        );
    }

    // ============================================================
    // FLIP
    // ============================================================

    private void FlipFood()
    {
        if (isFlipping ||
            foodModel == null ||
            foodStatsScript == null)
            return;

        isFlipping = true;

        if (jiggleTween != null)
        {
            jiggleTween.Kill();
            jiggleTween = null;
        }

        if (flipTween != null)
        {
            flipTween.Kill();
            flipTween = null;
        }

        Quaternion startRotation = foodModel.localRotation;
        Vector3 axis = GetAxis(flipAxis);

        float direction =
            flipDirection == FlipDirection.Forward
            ? 1f
            : -1f;

        float angle = 0f;

        flipTween = DOTween.To(
            () => angle,
            value =>
            {
                angle = value;

                foodModel.localRotation =
                    startRotation *
                    Quaternion.AngleAxis(
                        angle * direction,
                        axis
                    );
            },
            180f,
            flipDuration
        );

        flipTween
            .SetEase(flipEase)
            .SetUpdate(UpdateType.Normal)
            .OnComplete(() =>
            {
                foodModel.localRotation =
                    startRotation *
                    Quaternion.AngleAxis(
                        180f * direction,
                        axis
                    );

                flipTween = null;
                isFlipping = false;

                foodStatsScript.FlipFood();
            });
    }

    // ============================================================
    // AXIS
    // ============================================================

    private Vector3 GetAxis(RotationAxis axis)
    {
        switch (axis)
        {
            case RotationAxis.X:
                return Vector3.right;

            case RotationAxis.Y:
                return Vector3.up;

            case RotationAxis.Z:
                return Vector3.forward;
        }

        return Vector3.right;
    }

    // ============================================================
    // FLIP CLICK
    // ============================================================

    private void CheckForFlipClick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
            {
                FlipFood();
            }
        }
    }

    // ============================================================
    // MOUSE POSITION
    // ============================================================

    private bool GetMouseWorldPosition(out Vector3 worldPosition)
    {
        Vector3 mouse = Input.mousePosition;

        mouse.x = Mathf.Clamp(
            mouse.x,
            screenPadding,
            Screen.width - screenPadding
        );

        mouse.y = Mathf.Clamp(
            mouse.y,
            screenPadding,
            Screen.height - screenPadding
        );

        Ray ray = cam.ScreenPointToRay(mouse);

        Plane plane = new Plane(
            Vector3.up,
            dragPlane.position
        );

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hit = ray.GetPoint(distance);

            hit.y = dragPlane.position.y;

            worldPosition = hit;
            return true;
        }

        worldPosition = transform.position;
        return false;
    }

    // ============================================================
    // DRAGGING
    // ============================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (foodCollider != null)
            foodCollider.enabled = false;

        transform.DOKill();

        Jiggle();

        dragging = true;

        if (rb != null)
            rb.useGravity = false;

        // Stop cooking when food is picked up.
        if (foodStatsScript != null)
            foodStatsScript.StopCooking();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GetMouseWorldPosition(out Vector3 target))
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
        if (foodCollider != null)
            foodCollider.enabled = true;

        transform.DOKill();

        Jiggle();

        dragging = false;

        if (rb != null)
            rb.useGravity = true;

        // DO NOT StopCooking here.
        // Stove decides whether the food should cook.
    }

    // ============================================================
    // JIGGLE
    // ============================================================

    private void Jiggle()
    {
        if (foodModel == null || isFlipping)
            return;

        if (jiggleTween != null)
            jiggleTween.Kill();

        jiggleTween = foodModel.DOPunchRotation(
            Random.insideUnitSphere *
            jiggleStrength *
            15f,
            jiggleDuration,
            jiggleVibrato,
            jiggleRandomness
        )
        .OnComplete(() =>
        {
            jiggleTween = null;
        });
    }

    // ============================================================
    // HOVER
    // ============================================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverExitEvent?.Invoke();
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        if (flipTween != null)
            flipTween.Kill();

        if (jiggleTween != null)
            jiggleTween.Kill();
    }
}