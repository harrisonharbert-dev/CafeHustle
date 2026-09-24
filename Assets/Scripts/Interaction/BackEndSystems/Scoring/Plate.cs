using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlateScorer : MonoBehaviour
{
    public List<FoodStats> foodsOnPlate = new List<FoodStats>();

    public UnityEvent onOrderCompleted;

    [SerializeField] private bool orderCompleted = false;

    public UnityEvent onOrderSucceed;
    public UnityEvent onOrderFailed;

    public void Start()
    {
        // GetComponent<BoxCollider>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Collider is on the child food model,
        // so find FoodStats on the parent.
        FoodStats food = other.GetComponentInParent<FoodStats>();

        if (food != null && !foodsOnPlate.Contains(food))
        {
            foodsOnPlate.Add(food);

            Debug.Log("Food added to plate: " + food.name);

            CheckPlate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Collider is on the child food model,
        // so find FoodStats on the parent.
        FoodStats food = other.GetComponentInParent<FoodStats>();

        if (food != null && foodsOnPlate.Contains(food))
        {
            foodsOnPlate.Remove(food);

            Debug.Log("Food removed from plate: " + food.name);

            CheckPlate();
        }
    }

    public bool CheckPlate()
    {
        if (GameManager.Instance == null)
            return false;

        Order order = GameManager.Instance.currentOrder;

        if (order == null)
            return false;

        if (foodsOnPlate.Count == 0)
        {
            orderCompleted = false;
            return false;
        }

        // Count all food currently on the plate.
        Dictionary<FoodStats.FoodType, int> plateCounts =
            new Dictionary<FoodStats.FoodType, int>();

        foreach (FoodStats food in foodsOnPlate)
        {
            if (food == null)
                continue;

            if (!plateCounts.ContainsKey(food.foodType))
            {
                plateCounts.Add(food.foodType, 0);
            }

            plateCounts[food.foodType]++;
        }

        // Count everything required by the order.
        Dictionary<FoodStats.FoodType, int> requiredCounts =
            new Dictionary<FoodStats.FoodType, int>();

        foreach (OrderItem item in order.requiredItems)
        {
            if (!requiredCounts.ContainsKey(item.type))
            {
                requiredCounts.Add(item.type, 0);
            }

            requiredCounts[item.type] += item.amount;
        }

        // Check that every required food has the correct quantity.
        foreach (KeyValuePair<FoodStats.FoodType, int> required in requiredCounts)
        {
            int plateAmount = 0;

            if (plateCounts.ContainsKey(required.Key))
            {
                plateAmount = plateCounts[required.Key];
            }

            // Missing food.
            if (plateAmount < required.Value)
            {
                orderCompleted = false;

                Debug.Log(
                    "Missing food: " +
                    required.Key +
                    " (required: " +
                    required.Value +
                    ", on plate: " +
                    plateAmount +
                    ")"
                );

                return false;
            }

            // Too much food.
            if (plateAmount > required.Value)
            {
                orderCompleted = false;

                Debug.Log(
                    "Too much food: " +
                    required.Key +
                    " (required: " +
                    required.Value +
                    ", on plate: " +
                    plateAmount +
                    ")"
                );

                return false;
            }
        }

        // Check for food that isn't part of the order.
        foreach (KeyValuePair<FoodStats.FoodType, int> plateFood in plateCounts)
        {
            if (!requiredCounts.ContainsKey(plateFood.Key))
            {
                orderCompleted = false;
                return false;
            }
        }

        // Everything matches the order.
        orderCompleted = true;

        onOrderSucceed?.Invoke();

        return true;
    }

    public bool IsOrderComplete()
    {
        return orderCompleted;
    }

    public void NextStage()
    {
        // GetComponent<BoxCollider>().enabled = true;

        if (orderCompleted)
        {
            onOrderCompleted?.Invoke();
        }
        else
        {
            onOrderFailed?.Invoke();
        }
    }
}