using UnityEngine;

public class KnifeDrawer : MonoBehaviour
{
    public Camera cam;
    public LayerMask foodLayer;

    private FoodCuttable currentFood;

    private bool dragging;

    private Vector3 startPoint;
    private Vector3 endPoint;

    public LineRenderer line;

    public float lineOffset = 0.05f;

    void Start()
    {
        line.enabled = false;
        line.positionCount = 2;
        line.useWorldSpace = true;

        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 10f, foodLayer))
            {
                currentFood = hit.collider.GetComponent<FoodCuttable>();

                if (currentFood != null)
                {
                    dragging = true;

                    // Push line slightly above the food surface
                    Vector3 offset = hit.normal * 0.02f;

                    startPoint = hit.point + offset;
                    endPoint = hit.point + offset;

                    line.enabled = true;

                    line.SetPosition(0, startPoint);
                    line.SetPosition(1, endPoint);
                }
            }
        }


        if (dragging)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 10f, foodLayer))
            {
                Vector3 offset = hit.normal * 0.02f;

                endPoint = hit.point + offset;

                line.SetPosition(0, startPoint);
                line.SetPosition(1, endPoint);
            }
        }



        if (Input.GetMouseButtonUp(0) && dragging)
        {
            dragging = false;

            // Remove offset before checking accuracy
            Vector3 checkStart = startPoint;
            Vector3 checkEnd = endPoint;

            currentFood.IsCorrectCut(checkStart, checkEnd);

            line.enabled = false;
        }
    }
}