using UnityEngine;
using UnityEngine.Events;

public class Stove : MonoBehaviour
{
    public bool FoodOnPlate;
    public UnityEvent[] CookingVFX;

    [SerializeField] private int foodOnStove = 0;

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
        StartFoodCooking(collision);
    }

    private void OnCollisionStay(UnityEngine.Collision collision)
    {
        // If flipping caused cooking to stop while still touching
        // the stove, immediately start it again.
        FoodStats food = collision.gameObject.GetComponentInParent<FoodStats>();

        if (food != null && !food.isCooking)
        {
            DraggingScript dragging =
                collision.gameObject.GetComponentInParent<DraggingScript>();

            if (dragging != null && dragging.isFood && !dragging.dragging)
            {
                Debug.Log("Food still on stove - restarting cooking: " + food.gameObject.name);
                food.StartCooking();
            }
        }
    }

    private void OnCollisionExit(UnityEngine.Collision collision)
    {
        FoodStats food =
            collision.gameObject.GetComponentInParent<FoodStats>();

        DraggingScript dragging =
            collision.gameObject.GetComponentInParent<DraggingScript>();

        if (food == null || dragging == null || !dragging.isFood)
            return;

        Debug.Log("Stopping cooking: " + food.gameObject.name);

        food.StopCooking();

        foodOnStove--;
        foodOnStove = Mathf.Max(0, foodOnStove);

        if (foodOnStove == 0 &&
            CookingVFX != null &&
            CookingVFX.Length > 1)
        {
            CookingVFX[1]?.Invoke();
        }
    }

    private void StartFoodCooking(UnityEngine.Collision collision)
    {
        FoodStats food =
            collision.gameObject.GetComponentInParent<FoodStats>();

        DraggingScript dragging =
            collision.gameObject.GetComponentInParent<DraggingScript>();

        if (food == null || dragging == null || !dragging.isFood)
            return;

        Debug.Log("Starting to cook: " + food.gameObject.name);

        food.StartCooking();

        foodOnStove++;

        if (foodOnStove == 1 &&
            CookingVFX != null &&
            CookingVFX.Length > 0)
        {
            CookingVFX[0]?.Invoke();
        }
    }
}