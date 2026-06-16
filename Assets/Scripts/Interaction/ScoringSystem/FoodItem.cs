using UnityEngine;

public enum FoodType
{
  Bacon,
  Egg,
  Tomato,
  Sausage,
}

public class FoodItem : MonoBehaviour
{
    public FoodType foodType;

    // 100 = perfectly cooked
    public float cookPercentage = 100f;
}