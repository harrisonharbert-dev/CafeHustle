using UnityEngine;
using UnityEngine.Events;

public class Stove : MonoBehaviour
{
    public bool FoodOnPlate;
    public UnityEvent[] CookingVFX;

    [SerializeField] private int foodOnStove;

    public void FoodEntered(FoodStats food, DraggingScript dragging)
    {
        if (food == null || dragging == null || !dragging.isFood)
            return;

        // Prevent duplicate collision calls
        if (food.isCooking)
            return;

        Debug.Log("STOVE: Starting to cook " + food.gameObject.name);

        food.StartCooking();
        foodOnStove++;

        if (foodOnStove == 1 && CookingVFX.Length > 0)
            CookingVFX[0]?.Invoke();
    }

    public void FoodExited(FoodStats food, DraggingScript dragging)
    {
        if (food == null || dragging == null || !dragging.isFood)
            return;

        if (!food.isCooking)
            return;

        Debug.Log("STOVE: Stopping cooking " + food.gameObject.name);

        food.StopCooking();

        foodOnStove = Mathf.Max(0, foodOnStove - 1);

        if (foodOnStove == 0 && CookingVFX.Length > 1)
            CookingVFX[1]?.Invoke();
    }
}