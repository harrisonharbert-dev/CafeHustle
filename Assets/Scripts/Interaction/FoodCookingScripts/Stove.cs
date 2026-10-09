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

    private readonly List<FoodStats> foodCurrentlyOnStove = new List<FoodStats>();
    private Material oilMaterial;
    private Tween oilFillTween;
    private bool wasOilEmpty;
    public float Speed;

    private void Start()
    {
        if (stoveCollider == null)
            stoveCollider = GetComponent<Collider>();

        if (OliveOilMeter != null)
            OliveOilMeter.fillAmount = Mathf.Clamp01(OliveOilAmount / 100f);

        UpdateOilText();

        if (OilPlane != null)
        {
            oilMaterial = OilPlane.material;

            if (OliveOilAmount > 0)
            {
                oilMaterial.SetFloat("_Fill", MaxOilRadius);
                UpdateOilAlpha();
            }
            else
            {
                oilMaterial.SetFloat("_Fill", 0f);
                oilMaterial.SetFloat("_OilAlpha", 0f);
                wasOilEmpty = true;
            }
        }
        else
        {
            wasOilEmpty = OliveOilAmount <= 0;
        }
    }
    void DrainSpeed()
    {
        switch (foodOnStove)
        {
            case (1):
                Speed = 0.0005f;
                break;
            case (2):
                Speed = 0.001f;
                break;
            case (3):
                Speed = 0.0015f;
                break;
            case (4):
                Speed = 0.002f;
                break;
            case (5):
                Speed = 0.0025f;
                break;
            case (6):
                Speed = 0.03f;
                break;
            default:
                Speed = 0f;
                break;
        }
    }
    private void Update()
    {
        CheckTrackedFood();
        foodOnStove = foodCurrentlyOnStove.Count;

        // Drain oil while food is on the stove.
        if (foodOnStove > 0 && OliveOilAmount > 0 && OliveOilMeter != null)
        {
            DrainSpeed();
            OliveOilMeter.fillAmount -= Time.deltaTime * Speed;
            OliveOilMeter.fillAmount = Mathf.Clamp01(OliveOilMeter.fillAmount);
            OliveOilAmount = OliveOilMeter.fillAmount * 100f;

            UpdateOilText();
            UpdateOilAlpha();

            if (OliveOilAmount <= 0.01f)
            {
                OliveOilAmount = 0f;
                OliveOilMeter.fillAmount = 0f;
                wasOilEmpty = true;

                UpdateOilText();
                UpdateOilAlpha();
                StopAllFoodCooking();
            }
        }

        // Cook food only when oil is available.
        if (OliveOilAmount > 0)
        {
            foreach (FoodStats food in foodCurrentlyOnStove)
            {
                if (food == null)
                    continue;

                DraggingScript dragging = food.GetComponent<DraggingScript>();

                if (dragging != null && !dragging.dragging && !food.isCooking)
                {
                    food.StartCooking();
                    Debug.Log("Restarting cooking: " + food.name);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryAddFood(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryAddFood(collision);
    }

    private void TryAddFood(Collision collision)
    {
        FoodStats food = collision.gameObject.GetComponentInParent<FoodStats>();
        DraggingScript dragging = collision.gameObject.GetComponentInParent<DraggingScript>();

        if (food == null || dragging == null || !dragging.isFood)
            return;

        AddFood(food);
    }

    private void AddFood(FoodStats food)
    {
        if (food == null || foodCurrentlyOnStove.Contains(food))
            return;

        foodCurrentlyOnStove.Add(food);
        foodOnStove = foodCurrentlyOnStove.Count;

        Debug.Log("Food added to stove: " + food.name + " | Total: " + foodOnStove);

        DraggingScript dragging = food.GetComponent<DraggingScript>();

        if (OliveOilAmount > 0 && dragging != null && !dragging.dragging)
            food.StartCooking();

        if (foodOnStove == 1 && CookingVFX != null && CookingVFX.Length > 0 && OliveOilAmount > 0)
            CookingVFX[0]?.Invoke();
    }

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

            DraggingScript dragging = food.GetComponent<DraggingScript>();

            // Keep tracking food while it is being dragged.
            if (dragging != null && dragging.dragging)
                continue;

            Collider[] foodColliders = food.GetComponentsInChildren<Collider>();
            bool stillOnStove = false;

            foreach (Collider foodCollider in foodColliders)
            {
                if (foodCollider != null && stoveBounds.Intersects(foodCollider.bounds))
                {
                    stillOnStove = true;
                    break;
                }
            }

            if (!stillOnStove)
            {
                Debug.Log("Food actually left stove: " + food.name);
                food.StopCooking();
                foodCurrentlyOnStove.RemoveAt(i);
            }
        }

        foodOnStove = foodCurrentlyOnStove.Count;

        if (foodOnStove == 0 && CookingVFX != null && CookingVFX.Length > 1)
            CookingVFX[1]?.Invoke();
    }

    // Called by OliveOil after the pouring animation finishes.
    public void TopUpOil()
    {
        bool oilWasEmpty = OliveOilAmount <= 0 || wasOilEmpty;

        OliveOilAmount = 100f;
        wasOilEmpty = false;

        if (OliveOilMeter != null)
            OliveOilMeter.fillAmount = 1f;

        UpdateOilText();

        if (oilMaterial != null)
        {
            oilMaterial.SetFloat("_OilAlpha", MaxOilAlpha);

            if (oilWasEmpty)
                AnimateOilFill();
            else
                oilMaterial.SetFloat("_Fill", MaxOilRadius);
        }

        Debug.Log("Olive oil topped up!");

        foreach (FoodStats food in foodCurrentlyOnStove)
        {
            if (food == null)
                continue;

            DraggingScript dragging = food.GetComponent<DraggingScript>();

            if (dragging != null && !dragging.dragging && !food.isCooking)
                food.StartCooking();
        }

        if (foodOnStove > 0 && CookingVFX != null && CookingVFX.Length > 0)
            CookingVFX[0]?.Invoke();
    }

    private void AnimateOilFill()
    {
        if (oilMaterial == null)
            return;

        oilFillTween?.Kill();

        oilMaterial.SetFloat("_Fill", 0f);

        oilFillTween = oilMaterial.DOFloat(
            MaxOilRadius,
            "_Fill",
            OilFillDuration
        ).SetEase(Ease.OutSine);
    }

    private void UpdateOilAlpha()
    {
        if (oilMaterial == null)
            return;

        float oilPercent = Mathf.Clamp01(OliveOilAmount / 100f);
        float alpha = oilPercent * MaxOilAlpha;

        oilMaterial.SetFloat("_OilAlpha", alpha);
    }

    private void UpdateOilText()
    {
        if (OliveOilText != null)
            OliveOilText.text = "Oil Amount: " + Mathf.RoundToInt(OliveOilAmount) + "%";
    }

    private void StopAllFoodCooking()
    {
        Debug.Log("Oil empty - stopping all food cooking.");

        foreach (FoodStats food in foodCurrentlyOnStove)
        {
            if (food != null)
                food.StopCooking();
        }

        if (CookingVFX != null && CookingVFX.Length > 1)
            CookingVFX[1]?.Invoke();
    }

    private void OnDestroy()
    {
        oilFillTween?.Kill();

        if (oilMaterial != null)
            Destroy(oilMaterial);
    }
}