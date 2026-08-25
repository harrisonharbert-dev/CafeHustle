using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlateScorer : MonoBehaviour
{
    public List<FoodStats> foodsOnPlate = new List<FoodStats>();

    private bool orderCompleted = false;

    private void OnTriggerEnter(Collider other)
    {
        FoodStats food = other.GetComponent<FoodStats>();

        if (food != null && !foodsOnPlate.Contains(food))
        {
            foodsOnPlate.Add(food);

            CheckPlate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FoodStats food = other.GetComponent<FoodStats>();

        if (food != null)
        {
            foodsOnPlate.Remove(food);

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
                return false;
            }

            // Too much food.
            if (plateAmount > required.Value)
            {
                orderCompleted = false;
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
       
        return true;
    }

    public bool IsOrderComplete()
    {
        return orderCompleted;
    }


    public void NextStage()
    {
        if (orderCompleted == true)
        {
            SceneManager.LoadScene("Prototype_environment");
        }
    }
}