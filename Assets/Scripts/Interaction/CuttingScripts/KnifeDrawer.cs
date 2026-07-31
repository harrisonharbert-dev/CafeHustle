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

    [Header("Knife Movement")]
    public float hoverHeight = 0.25f;
    public float cutHeight = 0.02f;
    public float knifeSpeed = 15f;

    [Header("Cut Settings")]
    public float minimumCutDistance = 0.5f;
    public float bladeCheckRadius = 0.05f;

    private bool holdingKnife;
    private bool knifeLowered;

    private FoodCuttable currentFood;

    private Vector3 cutStart;
    private Vector3 lastBladePosition;

    private float cutDistance;
    public LayerMask boardLayer;

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
        // Drop knife
        if (holdingKnife && Input.GetMouseButtonDown(0))
        {
            holdingKnife = false;
            knifeLowered = false;
            currentFood = null;
            cutDistance = 0f;
            return;
        }


        // Pickup knife
        if (!holdingKnife && Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, knifeLayer))
            {
                holdingKnife = true;
            }
        }
    }


    void MoveKnife()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, boardLayer))
        {
            Vector3 target = hit.point;

            target.y = knifeLowered ? cutHeight : hoverHeight;

            Vector3 bladeOffset =
                knife.TransformVector(
                    knife.InverseTransformPoint(bladeTip.position)
                );

            Vector3 finalPosition = target - bladeOffset;


            knife.position = Vector3.Lerp(
                knife.position,
                finalPosition,
                knifeSpeed * Time.deltaTime
            );
        }
    }


    void HandleCutting()
    {
        // Lower knife
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
                    FoodCuttable food =
                        hit.collider.GetComponent<FoodCuttable>();

                    if (food != null)
                    {
                        currentFood = food;
                    }
                }
            }


            lastBladePosition = bladeTip.position;
        }


        // Release cut
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
}