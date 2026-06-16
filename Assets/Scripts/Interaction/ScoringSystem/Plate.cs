using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using UnityEngine;

public class PlateScorer : MonoBehaviour
{
    public List<FoodStats> foodsOnPlate = new List<FoodStats>();

    private void OnTriggerEnter(Collider other)
    {
        FoodStats food = other.GetComponent<FoodStats>();

        if (food != null && !foodsOnPlate.Contains(food))
        {
            foodsOnPlate.Add(food);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FoodStats food = other.GetComponent<FoodStats>();

        if (food != null)
        {
            foodsOnPlate.Remove(food);
        }
    }

    public PlateScorer plate;
    public void TriggerScore()
    {
        float score = ScorePlate();
        Debug.Log("Plate scored: " + score);
    }
    public float ScorePlate()
    {
        Order order = GameManager.Instance.currentOrder;

        if (order == null || foodsOnPlate.Count == 0)
            return 0f;

        float score = order.baseReward;

        Dictionary<FoodStats.FoodType, int> plateCounts = new();

        float cookPenalty = 0f;

        foreach (FoodStats food in foodsOnPlate)
        {
            if (!plateCounts.ContainsKey(food.foodType))
                plateCounts[food.foodType] = 0;

            plateCounts[food.foodType]++;

            cookPenalty += Mathf.Abs(food.CookRatio - 1f);
        }

        score -= cookPenalty * 40f;

        foreach (OrderItem req in order.requiredItems)
        {
            int found = plateCounts.ContainsKey(req.type) ? plateCounts[req.type] : 0;

            int diff = found - req.amount;

            if (diff < 0)
                score -= Mathf.Abs(diff) * 30f;
            else if (diff > 0)
                score -= diff * 15f;

            plateCounts.Remove(req.type);
        }

        foreach (var extra in plateCounts)
        {
            score -= extra.Value * 20f;
        }

        return Mathf.Max(0, score);
    }
}
