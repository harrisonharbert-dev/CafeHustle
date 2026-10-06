using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Stove : MonoBehaviour
{
    public bool FoodOnPlate;
    public UnityEvent[] CookingVFX;

    [SerializeField] private int foodOnStove = 0;

    [Header("Olive Oil")]
    public Image OliveOilMeter;
    public float OliveOilAmount = 100;
    public TextMeshProUGUI OliveOilText;

    [Header("Food Detection")]
    [SerializeField] private Collider stoveCollider;

    // Extra area around stove so food isn't lost from tiny physics movements
    [SerializeField] private float detectionPadding = 0.15f;

    private List<FoodStats> foodCurrentlyOnStove = new List<FoodStats>();

    private void Start()
    {
        if (stoveCollider == null)
            stoveCollider = GetComponent<Collider>();

        OliveOilMeter.fillAmount = OliveOilAmount / 100f;
        UpdateOilText();
    }

    private void Update()
    {
        CheckTrackedFood();

        foodOnStove = foodCurrentlyOnStove.Count;

        // Drain oil
        if (foodOnStove > 0 && OliveOilAmount > 0)
        {
            OliveOilMeter.fillAmount -=
                Time.deltaTime * foodOnStove * 0.05f;

            OliveOilMeter.fillAmount =
                Mathf.Clamp01(OliveOilMeter.fillAmount);

            OliveOilAmount =
                Mathf.RoundToInt(OliveOilMeter.fillAmount * 100);

            UpdateOilText();

            if (OliveOilAmount <= 0)
            {
                StopAllFoodCooking();
            }
        }

        // Keep all food cooking
        if (OliveOilAmount > 0)
        {
            foreach (FoodStats food in foodCurrentlyOnStove)
            {
                if (food == null)
                    continue;

                DraggingScript dragging =
                    food.GetComponent<DraggingScript>();

                if (dragging == null)
                    continue;

                if (!dragging.dragging && !food.isCooking)
                {
                    food.StartCooking();

                    Debug.Log(
                        "Restarting cooking: " + food.name
                    );
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // =========================
        // OLIVE OIL
        // =========================

        if (collision.gameObject.CompareTag("OliveOil"))
        {
            OliveOil oliveOil =
                collision.gameObject.GetComponent<OliveOil>();

            if (oliveOil != null)
            {
                oliveOil.SetStove(this);
            }

            return;
        }

        // =========================
        // FOOD
        // =========================

        FoodStats food =
            collision.gameObject.GetComponentInParent<FoodStats>();

        DraggingScript dragging =
            collision.gameObject.GetComponentInParent<DraggingScript>();

        if (food == null ||
            dragging == null ||
            !dragging.isFood)
        {
            return;
        }

        AddFood(food);
    }

    private void OnCollisionStay(Collision collision)
    {
        // =========================
        // OLIVE OIL
        // =========================

        if (collision.gameObject.CompareTag("OliveOil"))
        {
            OliveOil oliveOil =
                collision.gameObject.GetComponent<OliveOil>();

            if (oliveOil != null)
            {
                oliveOil.SetStove(this);
            }

            return;
        }

        // =========================
        // FOOD
        // =========================

        FoodStats food =
            collision.gameObject.GetComponentInParent<FoodStats>();

        DraggingScript dragging =
            collision.gameObject.GetComponentInParent<DraggingScript>();

        if (food == null ||
            dragging == null ||
            !dragging.isFood)
        {
            return;
        }

        // CollisionStay can re-add food if physics somehow missed Enter
        AddFood(food);
    }

    private void OnCollisionExit(Collision collision)
    {
        // Only use CollisionExit for olive oil.
        // Food removal is handled by CheckTrackedFood().

        if (collision.gameObject.CompareTag("OliveOil"))
        {
            OliveOil oliveOil =
                collision.gameObject.GetComponent<OliveOil>();

            if (oliveOil != null)
            {
                oliveOil.RemoveStove(this);
            }
        }
    }

    private void AddFood(FoodStats food)
    {
        if (foodCurrentlyOnStove.Contains(food))
            return;

        foodCurrentlyOnStove.Add(food);

        foodOnStove = foodCurrentlyOnStove.Count;

        Debug.Log(
            "Food added to stove: " +
            food.name +
            " | Total: " +
            foodOnStove
        );

        DraggingScript dragging =
            food.GetComponent<DraggingScript>();

        if (OliveOilAmount > 0 &&
            dragging != null &&
            !dragging.dragging)
        {
            food.StartCooking();
        }

        if (foodOnStove == 1 &&
            CookingVFX != null &&
            CookingVFX.Length > 0)
        {
            CookingVFX[0]?.Invoke();
        }
    }

    // ============================================================
    // CHECK IF TRACKED FOOD IS ACTUALLY STILL ON STOVE
    // ============================================================

    private void CheckTrackedFood()
    {
        if (stoveCollider == null)
            return;

        Bounds stoveBounds = stoveCollider.bounds;

        // Make detection slightly larger than actual stove
        stoveBounds.Expand(detectionPadding);

        for (int i = foodCurrentlyOnStove.Count - 1; i >= 0; i--)
        {
            FoodStats food = foodCurrentlyOnStove[i];

            if (food == null)
            {
                foodCurrentlyOnStove.RemoveAt(i);
                continue;
            }

            DraggingScript dragging =
                food.GetComponent<DraggingScript>();

            // Don't remove while player is dragging/flipping it
            if (dragging != null && dragging.dragging)
                continue;

            Collider foodCollider =
                food.GetComponentInChildren<Collider>();

            if (foodCollider == null)
                continue;

            // Check whether food collider still overlaps stove area
            if (!stoveBounds.Intersects(foodCollider.bounds))
            {
                Debug.Log(
                    "Food actually left stove: " + food.name
                );

                food.StopCooking();

                foodCurrentlyOnStove.RemoveAt(i);
            }
        }

        foodOnStove = foodCurrentlyOnStove.Count;

        if (foodOnStove == 0 &&
            CookingVFX != null &&
            CookingVFX.Length > 1)
        {
            CookingVFX[1]?.Invoke();
        }
    }

    // ============================================================
    // OLIVE OIL
    // ============================================================

    public void TopUpOil()
    {
        OliveOilAmount = 100f;
        OliveOilMeter.fillAmount = 1f;

        UpdateOilText();

        Debug.Log("Olive oil topped up!");

        foreach (FoodStats food in foodCurrentlyOnStove)
        {
            if (food == null)
                continue;

            DraggingScript dragging =
                food.GetComponent<DraggingScript>();

            if (dragging != null &&
                !dragging.dragging &&
                !food.isCooking)
            {
                food.StartCooking();
            }
        }

        if (foodOnStove > 0 &&
            CookingVFX != null &&
            CookingVFX.Length > 0)
        {
            CookingVFX[0]?.Invoke();
        }
    }

    private void UpdateOilText()
    {
        if (OliveOilText != null)
        {
            OliveOilText.text =
                "Oil Amount: " +
                Mathf.RoundToInt(OliveOilAmount) +
                "%";
        }
    }

    private void StopAllFoodCooking()
    {
        Debug.Log(
            "Oil empty - stopping all food cooking."
        );

        foreach (FoodStats food in foodCurrentlyOnStove)
        {
            if (food != null)
            {
                food.StopCooking();
            }
        }

        if (CookingVFX != null &&
            CookingVFX.Length > 1)
        {
            CookingVFX[1]?.Invoke();
        }
    }
}