using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrderItem
{
    public FoodStats.FoodType type;
    public int amount;
}

public class Order : MonoBehaviour
{
    public List<OrderItem> requiredItems = new List<OrderItem>();
    public float baseReward = 100f;
}