using UnityEngine;
using UnityEngine.Events;

public class Bin : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private HotbarSlot[] hotbarSlots;

    public UnityEvent OnIngredientReturned;

    private void OnTriggerEnter(Collider other)
    {
        // Find FoodStats on the parent
        FoodStats food = other.GetComponentInParent<FoodStats>();

        if (food == null)
            return;

        // Check the actual parent food object's tag
        if (!food.gameObject.CompareTag("Ingredient"))
            return;

        Debug.Log("Ingredient thrown in bin: " + food.gameObject.name);

        ReturnIngredientToInventory(food.foodType);

        OnIngredientReturned?.Invoke();

        // Destroy the entire food object, not just the child collider
        Destroy(food.gameObject);
    }

    private void ReturnIngredientToInventory(FoodStats.FoodType foodType)
    {
        foreach (HotbarSlot slot in hotbarSlots)
        {
            if (slot == null)
                continue;

            GameObject prefab = slot.hotbar.GetPrefab(slot.slotIndex);

            if (prefab == null)
                continue;

            FoodStats prefabFood = prefab.GetComponent<FoodStats>();

            if (prefabFood == null)
                continue;

            if (prefabFood.foodType != foodType)
                continue;

            slot.AddOne();

            return;
        }

        Debug.LogWarning(
            $"Could not find a hotbar slot for ingredient type: {foodType}"
        );
    }
}