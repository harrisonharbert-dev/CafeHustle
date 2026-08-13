using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrayCheck : MonoBehaviour
{
    public List<FoodStats> foodsOnTray = new List<FoodStats>();

    public GameObject NextSectionUI;

    [Header("Cook Tolerance")]
    [Tooltip("Target ratio is 1.0. Lower/Upper bounds define acceptable cooking perfection.")]
    [SerializeField] private float minCookRatio = 0.8f;

    [SerializeField] private float maxCookRatio = 1.2f;

    [Header("Emotes")]
    public TextResponse EmoteResponse;

    public UnityEvent Success;

    private bool hasSucceeded = false;

    private void Start()
    {
        if (NextSectionUI != null)
        {
            NextSectionUI.SetActive(false);
        }

        if (EmoteResponse == null)
        {
            EmoteResponse = FindAnyObjectByType<TextResponse>();
        }
    }

    private void Update()
    {
        int oldCount = foodsOnTray.Count;

        foodsOnTray.RemoveAll(item => item == null);

        if (foodsOnTray.Count != oldCount)
        {
            CheckTrayRequirements();
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

    public bool IsTrayValid()
    {
        if (GameManager.Instance == null)
        {
            return false;
        }

        Order order = GameManager.Instance.currentOrder;

        if (order == null)
        {
            return false;
        }

        if (foodsOnTray.Count == 0)
        {
            SetEmote("");
            return false;
        }

        Dictionary<FoodStats.FoodType, int> trayCounts =
            new Dictionary<FoodStats.FoodType, int>();

        // Check every food item on the tray.
        foreach (FoodStats food in foodsOnTray)
        {
            if (food == null)
                continue;

            if (!IsFoodCookedProperly(food))
            {
                if (food.CookRatio < minCookRatio)
                {
                    SetEmote("Food is undercooked!");
                }
                else if (food.CookRatio > maxCookRatio)
                {
                    SetEmote("Food is overcooked!");
                }
                else
                {
                    SetEmote("Food isn't cooked properly!");
                }

                return false;
            }

            if (!trayCounts.ContainsKey(food.foodType))
            {
                trayCounts.Add(food.foodType, 0);
            }

            trayCounts[food.foodType]++;
        }

        // Count required items.
        Dictionary<FoodStats.FoodType, int> requiredCounts =
            new Dictionary<FoodStats.FoodType, int>();

        foreach (OrderItem req in order.requiredItems)
        {
            if (!requiredCounts.ContainsKey(req.type))
            {
                requiredCounts.Add(req.type, 0);
            }

            requiredCounts[req.type] += req.amount;
        }

        // Check required quantities.
        foreach (KeyValuePair<FoodStats.FoodType, int> required in requiredCounts)
        {
            int trayAmount = 0;

            if (trayCounts.ContainsKey(required.Key))
            {
                trayAmount = trayCounts[required.Key];
            }

            if (trayAmount < required.Value)
            {
                SetEmote("Need more " + required.Key + "!");
                return false;
            }

            if (trayAmount > required.Value)
            {
                SetEmote("Too much " + required.Key + "!");
                return false;
            }
        }

        // Check for extra food.
        foreach (KeyValuePair<FoodStats.FoodType, int> trayFood in trayCounts)
        {
            if (!requiredCounts.ContainsKey(trayFood.Key))
            {
                SetEmote("Extra food on tray!");
                return false;
            }
        }

        // Everything is correct.
        SetEmote("");

        return true;
    }

    private bool IsFoodCookedProperly(FoodStats food)
    {
        if (food == null)
            return false;

        if (food.cookingTime <= 0)
            return false;

        if (food.requiresTwoSides)
        {
            float sideOneRatio = GetSideOneRatio(food);
            float sideTwoRatio = GetSideTwoRatio(food);

            bool sideOneValid =
                sideOneRatio >= minCookRatio &&
                sideOneRatio <= maxCookRatio;

            bool sideTwoValid =
                sideTwoRatio >= minCookRatio &&
                sideTwoRatio <= maxCookRatio;

            return sideOneValid && sideTwoValid;
        }

        float cookRatio = food.CookRatio;

        return cookRatio >= minCookRatio &&
               cookRatio <= maxCookRatio;
    }

    private float GetSideOneRatio(FoodStats food)
    {
        var field = typeof(FoodStats).GetField(
            "sideOneProgress",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );

        if (field != null)
        {
            float progress = (float)field.GetValue(food);

            return progress / food.cookingTime;
        }

        return 0f;
    }

    private float GetSideTwoRatio(FoodStats food)
    {
        var field = typeof(FoodStats).GetField(
            "sideTwoProgress",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );

        if (field != null)
        {
            float progress = (float)field.GetValue(food);

            return progress / food.cookingTime;
        }

        return 0f;
    }

    public void CheckTrayRequirements()
    {
        if (hasSucceeded)
            return;

        bool valid = IsTrayValid();

        if (valid)
        {
            TriggerNextSection();
        }
    }

    private void SetEmote(string message)
    {
        if (EmoteResponse != null)
        {
            EmoteResponse.SetText(message);
        }
    }

    private void TriggerNextSection()
    {
        if (hasSucceeded)
            return;

        hasSucceeded = true;

        SetEmote("");

        Success?.Invoke();

        StartCoroutine(EnterNextStage());
    }

    public IEnumerator EnterNextStage()
    {
        yield return new WaitForSeconds(2f);

        if (NextSectionUI != null)
        {
            NextSectionUI.SetActive(true);
        }

        Debug.Log("Tray check passed! Advancing to the next section.");
    }
}