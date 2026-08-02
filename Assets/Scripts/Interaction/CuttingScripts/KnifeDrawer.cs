using UnityEngine;

public class KnifeDrawer : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform knife;
    public Transform bladeTip;

    [Header("Layers")]
    public LayerMask knifeLayer;
    public LayerMask foodLayer;

    [Header("Board")]
    [Tooltip("Y position of the cutting board.")]
    public float boardHeight = 0f;

    [Header("Knife Movement")]
    public float hoverHeight = 0.25f;
    public float cutHeight = 0.02f;
    public float knifeSpeed = 20f;

    [Header("Cut Settings")]
    public float minimumCutDistance = 0.5f;
    public float bladeCheckRadius = 0.05f;

    private bool holdingKnife;
    private bool knifeLowered;

    private FoodCuttable currentFood;

    private Vector3 cutStart;
    private Vector3 lastBladePosition;
    private float cutDistance;

    private Vector3 bladeOffset;

    private Plane boardPlane;

    private void Start()
    {
        // Offset from knife pivot to blade tip.
        bladeOffset = bladeTip.position - knife.position;

        // Infinite board plane.
        boardPlane = new Plane(Vector3.up, new Vector3(0f, boardHeight, 0f));
    }

    void Update()
    {
        HandlePickup();

        if (!holdingKnife)
            return;

        MoveKnife();
        HandleCutting();
    }

    void HandlePickup()
    {
        if (holdingKnife && Input.GetMouseButtonDown(0))
        {
            holdingKnife = false;
            knifeLowered = false;
            currentFood = null;
            cutDistance = 0f;
            return;
        }

        if (!holdingKnife && Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, knifeLayer))
            {
                holdingKnife = true;

                // Recalculate in case knife moved in editor.
                bladeOffset = bladeTip.position - knife.position;
            }
        }
    }

    void MoveKnife()
    {
        float knifeY = knifeLowered ? cutHeight : hoverHeight;

        // Plane at the knife's current movement height.
        Plane knifePlane = new Plane(Vector3.up, new Vector3(0f, knifeY, 0f));

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (knifePlane.Raycast(ray, out float enter))
        {
            Vector3 target = ray.GetPoint(enter);

            // Keep the blade tip under the cursor.
            Vector3 offset = bladeTip.position - knife.position;

            knife.position = target - offset;

   
            knife.position = Vector3.MoveTowards(
              knife.position,
               target - offset,
             knifeSpeed * Time.deltaTime);
        }
    }

    void HandleCutting()
    {
        if (Input.GetMouseButtonDown(1))
        {
            knifeLowered = true;

            cutStart = bladeTip.position;
            lastBladePosition = bladeTip.position;
            cutDistance = 0f;
            currentFood = null;
        }

        if (knifeLowered)
        {
            float movement = Vector3.Distance(
                bladeTip.position,
                lastBladePosition
            );

            cutDistance += movement;

            Vector3 direction = bladeTip.position - lastBladePosition;

            if (direction.sqrMagnitude > 0.0001f)
            {
                if (Physics.SphereCast(
                    lastBladePosition,
                    bladeCheckRadius,
                    direction.normalized,
                    out RaycastHit hit,
                    direction.magnitude,
                    foodLayer))
                {
                    FoodCuttable food = hit.collider.GetComponent<FoodCuttable>();

                    if (food != null)
                    {
                        currentFood = food;
                    }
                }
            }

            lastBladePosition = bladeTip.position;
        }

        if (Input.GetMouseButtonUp(1))
        {
            knifeLowered = false;

            if (currentFood != null &&
                cutDistance >= minimumCutDistance)
            {
                currentFood.IsCorrectCut(
                    cutStart,
                    bladeTip.position
                );
            }

            currentFood = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (bladeTip == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(bladeTip.position, 0.02f);
    }
}