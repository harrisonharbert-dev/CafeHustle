using System.Collections.Generic;
using UnityEngine;

public class TrayCheck : MonoBehaviour
{
    public List<FoodStats> foodsOnTray = new List<FoodStats>();
    public GameObject NextSectionUI; // UI or object for the next section

    [Header("Cook Tolerance")]
    [Tooltip("Target ratio is 1.0. Lower/Upper bounds define acceptable cooking perfection for each side.")]
    [SerializeField] private float minCookRatio = 0.8f;  // Minimum acceptable doneness ratio (prevents undercooking)
    [SerializeField] private float maxCookRatio = 1.2f;  // Maximum acceptable doneness ratio (prevents overcooking)

    private void Start()
    {
        if (NextSectionUI != null)
        {
            NextSectionUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        FoodStats food = other.GetComponent<FoodStats>();

        if (food != null && !foodsOnTray.Contains(food))
        {
            foodsOnTray.Add(food);
            CheckTrayRequirements();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FoodStats food = other.GetComponent<FoodStats>();

        if (food != null && foodsOnTray.Contains(food))
        {
            foodsOnTray.Remove(food);
            CheckTrayRequirements();
        }
    }

    /// <summary>
    /// Validates if all items on the tray match the order and are fully cooked on all required sides.
    /// </summary>
    public bool IsTrayValid()
    {
        Order order = GameManager.Instance.currentOrder;

        if (order == null || foodsOnTray.Count == 0)
            return false;

        Dictionary<FoodStats.FoodType, int> trayCounts = new Dictionary<FoodStats.FoodType, int>();

        // 1. Validate doneness (including dual-sided foods) and build count map
        foreach (FoodStats food in foodsOnTray)
        {
            if (!IsFoodCookedProperly(food))
            {
                return false; // Reject if undercooked or overcooked on any side
            }

            if (!trayCounts.ContainsKey(food.foodType))
                trayCounts[food.foodType] = 0;

            trayCounts[food.foodType]++;
        }

        // 2. Check if required order quantities match the tray contents
        foreach (OrderItem req in order.requiredItems)
        {
            int found = trayCounts.ContainsKey(req.type) ? trayCounts[req.type] : 0;

            if (found != req.amount)
            {
                return false;
            }

            trayCounts.Remove(req.type);
        }

        // 3. Reject if extra unrequested food items are present
        if (trayCounts.Count > 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Helper method to evaluate if a food item is cooked properly on all active sides.
    /// </summary>
    private bool IsFoodCookedProperly(FoodStats food)
    {
        if (food.cookingTime <= 0) return false;

        if (food.requiresTwoSides)
        {
            // Evaluate both sides independently
            float sideOneRatio = GetSideOneRatio(food);
            float sideTwoRatio = GetSideTwoRatio(food);

            bool sideOneValid = sideOneRatio >= minCookRatio && sideOneRatio <= maxCookRatio;
            bool sideTwoValid = sideTwoRatio >= minCookRatio && sideTwoRatio <= maxCookRatio;

            return sideOneValid && sideTwoValid;
        }
        else
        {
            // Single-sided evaluation
            float cookRatio = food.CookRatio;
            return cookRatio >= minCookRatio && cookRatio <= maxCookRatio;
        }
    }

    // Helper reflection/accessors to safely extract serialized side progress from FoodStats
    private float GetSideOneRatio(FoodStats food)
    {
        var field = typeof(FoodStats).GetField("sideOneProgress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            float progress = (float)field.GetValue(food);
            return progress / food.cookingTime;
        }
        return 0f;
    }

    private float GetSideTwoRatio(FoodStats food)
    {
        var field = typeof(FoodStats).GetField("sideTwoProgress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            float progress = (float)field.GetValue(food);
            return progress / food.cookingTime;
        }
        return 0f;
    }

    /// <summary>
    /// Evaluates the tray and triggers the next section if requirements pass.
    /// </summary>
    public void CheckTrayRequirements()
    {
        if (IsTrayValid())
        {
            TriggerNextSection();
        }
    }

    private void TriggerNextSection()
    {
        if (NextSectionUI != null)
        {
            NextSectionUI.SetActive(true);
        }

        Debug.Log("Tray check passed! Advancing to the next section.");
    }
}