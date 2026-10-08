using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class Stove : MonoBehaviour
{
    public bool FoodOnPlate;
    public UnityEvent[] CookingVFX;

    [SerializeField] private int foodOnStove = 0;

    [Header("Olive Oil")]
    public Image OliveOilMeter;
    public float OliveOilAmount = 100;
    public TextMeshProUGUI OliveOilText;

    [Header("Oil Plane")]
    public Renderer OilPlane;

    [Tooltip("Maximum size of the oil puddle.")]
    public float MaxOilRadius = 0.72f;

    [Tooltip("How long oil takes to spread across the pan.")]
    public float OilFillDuration = 1f;

    [Tooltip("Maximum alpha when the pan has oil.")]
    [Range(0f, 1f)]
    public float MaxOilAlpha = 0.6f;

    [Header("Food Detection")]
    [SerializeField] private Collider stoveCollider;

    [SerializeField] private float detectionPadding = 0.15f;

    private List<FoodStats> foodCurrentlyOnStove = new List<FoodStats>();

    private Material oilMaterial;
    private Tween oilFillTween;

    private void Start()
    {
        if (stoveCollider == null)
            stoveCollider = GetComponent<Collider>();

        OliveOilMeter.fillAmount = OliveOilAmount / 100f;

        UpdateOilText();

        if (OilPlane != null)
        {
            oilMaterial = OilPlane.material;

            // If starting with oil, show it fully spread.
            if (OliveOilAmount > 0)
            {
                oilMaterial.SetFloat("_Fill", MaxOilRadius);
                UpdateOilAlpha();
            }
            else
            {
                oilMaterial.SetFloat("_Fill", 0f);
                oilMaterial.SetFloat("_OilAlpha", 0f);
            }
        }
    }

    private void Update()
    {
        CheckTrackedFood();

        foodOnStove = foodCurrentlyOnStove.Count;

        // ========================================================
        // DRAIN OIL
        // ========================================================

        if (foodOnStove > 0 && OliveOilAmount > 0)
        {
            OliveOilMeter.fillAmount -=
                Time.deltaTime * foodOnStove * 0.05f;

            OliveOilMeter.fillAmount =
                Mathf.Clamp01(OliveOilMeter.fillAmount);

            OliveOilAmount =
                OliveOilMeter.fillAmount * 100f;

            UpdateOilText();

            // IMPORTANT:
            // Do NOT change _Fill while draining.
            // Only fade the oil.
            UpdateOilAlpha();

            if (OliveOilAmount <= 0)
            {
                OliveOilAmount = 0;

                UpdateOilAlpha();

                StopAllFoodCooking();
            }
        }

        // ========================================================
        // KEEP FOOD COOKING
        // ========================================================

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
        // ========================================================
        // OLIVE OIL
        // ========================================================

        if (collision.gameObject.CompareTag("OliveOil"))
        {
            OliveOil oliveOil =
                collision.gameObject.GetComponent<OliveOil>();

            if (oliveOil != null)
                oliveOil.SetStove(this);

            return;
        }

        // ========================================================
        // FOOD
        // ========================================================

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
        // ========================================================
        // OLIVE OIL
        // ========================================================

        if (collision.gameObject.CompareTag("OliveOil"))
        {
            OliveOil oliveOil =
                collision.gameObject.GetComponent<OliveOil>();

            if (oliveOil != null)
                oliveOil.SetStove(this);

            return;
        }

        // ========================================================
        // FOOD
        // ========================================================

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

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("OliveOil"))
        {
            OliveOil oliveOil =
                collision.gameObject.GetComponent<OliveOil>();

            if (oliveOil != null)
                oliveOil.RemoveStove(this);
        }
    }

    // ============================================================
    // ADD FOOD
    // ============================================================

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
    // CHECK TRACKED FOOD
    // ============================================================

    private void CheckTrackedFood()
    {
        if (stoveCollider == null)
            return;

        Bounds stoveBounds = stoveCollider.bounds;

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

            // Don't remove while being dragged.
            if (dragging != null && dragging.dragging)
                continue;

            Collider foodCollider =
                food.GetComponentInChildren<Collider>();

            if (foodCollider == null)
                continue;

            if (!stoveBounds.Intersects(foodCollider.bounds))
            {
                Debug.Log(
                    "Food actually left stove: " +
                    food.name
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
    // TOP UP OIL
    // ============================================================

    public void TopUpOil()
    {
      

        // Oil becomes fully visible.
        if (oilMaterial != null)
        {
            oilMaterial.SetFloat(
                "_OilAlpha",
                MaxOilAlpha
            );
            if (OliveOilAmount <= 0)
            {
                AnimateOilFill();
            }
        }
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

    // ============================================================
    // OIL FILL ANIMATION
    // ============================================================

    private void AnimateOilFill()
    {
        if (oilMaterial == null)
            return;

        oilFillTween?.Kill();

        // Start puddle from the centre.
        oilMaterial.SetFloat(
            "_Fill",
            0f
        );

        // Spread outward.
        oilFillTween = oilMaterial.DOFloat(
            MaxOilRadius,
            "_Fill",
            OilFillDuration
        )
        .SetEase(Ease.OutSine);
    }

    // ============================================================
    // OIL ALPHA
    // ============================================================

    private void UpdateOilAlpha()
    {
        if (oilMaterial == null)
            return;

        float oilPercent =
            Mathf.Clamp01(
                OliveOilAmount / 100f
            );

        float alpha =
            oilPercent * MaxOilAlpha;

        oilMaterial.SetFloat(
            "_OilAlpha",
            alpha
        );
    }

    // ============================================================
    // UI
    // ============================================================

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

    // ============================================================
    // STOP COOKING
    // ============================================================

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

    private void OnDestroy()
    {
        oilFillTween?.Kill();
    }
}