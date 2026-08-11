using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggingScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Camera cam;

    [Header("Carry Settings")]
    public float carryDistance = 2f;
    public float moveSpeed = 15f;


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
    [HideInInspector]
    public FoodStats foodStatsScript;



    void Start()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody>();

        foodStatsScript = GetComponent<FoodStats>();
    }



    void Update()
    {
        // Move while holding
        if (dragging)
        {
            Vector3 mousePos =
                Input.mousePosition;

            mousePos.z = carryDistance;


            Vector3 target =
                cam.ScreenToWorldPoint(mousePos);


            transform.position =
                Vector3.Lerp(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime);
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
                Space.Self);
        }



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
            MeshCollider mesh =
                GetComponent<MeshCollider>();

            if (mesh != null)
                mesh.enabled = true;


            dragging = false;

            rb.useGravity = true;


            if (foodStatsScript != null)
                foodStatsScript.StopCooking();
        }
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



        Vector3 targetRotation =
            transform.localEulerAngles;


        targetRotation.x += 180f;



        transform.DOLocalRotate(
            targetRotation,
            flipDuration,
            RotateMode.FastBeyond360)
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
        MeshCollider mesh =
            GetComponent<MeshCollider>();

        if (mesh != null)
            mesh.enabled = false;



        transform.DOShakeScale(
            jiggleDuration,
            jiggleStrength,
            jiggleVibrato,
            jiggleRandomness,
            true,
            ShakeRandomnessMode.Harmonic);



        dragging = true;


        rb.useGravity = false;


        if (foodStatsScript != null)
            foodStatsScript.StopCooking();
    }





    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mousePos =
            eventData.position;


        mousePos.z = carryDistance;


        Vector3 target =
            cam.ScreenToWorldPoint(mousePos);



        transform.position =
            Vector3.Lerp(
                transform.position,
                target,
                moveSpeed * Time.deltaTime);
    }





    public void OnEndDrag(PointerEventData eventData)
    {
        MeshCollider mesh =
            GetComponent<MeshCollider>();

        if (mesh != null)
            mesh.enabled = true;



        transform.DOShakeScale(
            jiggleDuration,
            jiggleStrength,
            jiggleVibrato,
            jiggleRandomness,
            true,
            ShakeRandomnessMode.Harmonic);



        dragging = false;


        rb.useGravity = true;



        if (foodStatsScript != null)
            foodStatsScript.StopCooking();
    }
}