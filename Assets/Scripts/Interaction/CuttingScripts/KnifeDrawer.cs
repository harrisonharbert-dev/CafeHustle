using UnityEngine;

public class KnifeDrawer : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform knife;

    [Header("Layers")]
    public LayerMask knifeLayer;
    public LayerMask foodLayer;

    [Header("Knife Movement")]
    public float hoverHeight = 0.25f;
    public float cutHeight = 0.02f;
    public float boardHeight = 0f;
    public float knifeFollowSpeed = 50f;

    [Header("Cut Settings")]
    public float minimumCutDistance = 0.5f;
    public float bladeCheckRadius = 0.05f;

    private bool holdingKnife;
    private bool knifeLowered;

    private FoodCuttable currentFood;

    private Vector3 cutStart;
    private Vector3 lastKnifePosition;

    private float cutDistance;


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
            cutDistance = 0;

            return;
        }


        // Pick up knife
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

        Plane plane = new Plane(
            Vector3.up,
            new Vector3(0, boardHeight, 0)
        );


        if (plane.Raycast(ray, out float distance))
        {
            Vector3 target = ray.GetPoint(distance);

            target.y = boardHeight +
                (knifeLowered ? cutHeight : hoverHeight);


            knife.position = Vector3.Lerp(
                knife.position,
                target,
                Time.deltaTime * knifeFollowSpeed
            );
        }
    }


    void HandleCutting()
    {
        // Start cutting
        if (Input.GetMouseButtonDown(1))
        {
            knifeLowered = true;

            cutStart = knife.position;

            lastKnifePosition = knife.position;

            cutDistance = 0;

            currentFood = null;
        }


        // While knife is lowered
        if (knifeLowered)
        {
            float movement = Vector3.Distance(
                knife.position,
                lastKnifePosition
            );

            cutDistance += movement;


            Vector3 direction = knife.position - lastKnifePosition;


            if (direction.sqrMagnitude > 0.001f)
            {
                if (Physics.SphereCast(
                    lastKnifePosition,
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


            lastKnifePosition = knife.position;
        }


        // Finish cutting
        if (Input.GetMouseButtonUp(1))
        {
            knifeLowered = false;


            if (currentFood != null &&
               cutDistance >= minimumCutDistance)
            {
                currentFood.IsCorrectCut(
                    cutStart,
                    knife.position
                );
            }


            currentFood = null;
        }
    }
}