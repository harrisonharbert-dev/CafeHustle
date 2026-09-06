using UnityEngine;

public class Bin : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private HotbarSlot[] hotbarSlots;


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ingredient"))
            return;

        FoodStats food = other.GetComponent<FoodStats>();

        if (food == null)
        {
            Debug.LogWarning(
                $"Ingredient {other.name} has no FoodStats component.",
                other
            );

            return;
        }

        ReturnIngredientToInventory(food.foodType);

        Destroy(other.gameObject);
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

