using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Stove : MonoBehaviour
{
    public bool FoodOnPlate;
    public UnityEvent[] CookingVFX;

    [SerializeField] private int foodOnStove = 0;

    // Olive Oil Meter
    public Image OliveOilMeter;
    public float OliveOilAmount = 100;
    public TextMeshProUGUI OliveOilText;

    private List<FoodStats> foodCurrentlyOnStove = new List<FoodStats>();
    private void Start()
    {
        OliveOilAmount = 100f;
        OliveOilMeter.fillAmount = OliveOilAmount / 100f;
    }
    private void Update()
    {
        if (foodOnStove > 0 && OliveOilAmount > 0)
        {
            OliveOilMeter.fillAmount -= Time.deltaTime * foodOnStove/20f;
            OliveOilMeter.fillAmount = Mathf.Clamp01(OliveOilMeter.fillAmount);

            OliveOilAmount = Mathf.RoundToInt(OliveOilMeter.fillAmount * 100);
            OliveOilText.text = "Oil Amount: " + OliveOilAmount + "%";

            if (OliveOilAmount <= 0)
            {
                StopAllFoodCooking();
            }
        }
    }

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
        if (collision.gameObject.CompareTag("OliveOil"))
        {
            OliveOilAmount = 100f; 
            OliveOilMeter.fillAmount = OliveOilAmount / 100f;
            Debug.Log("Olive oil added to stove. Amount: " + OliveOilAmount + "%");
        }
        else
        {
            FoodStats food = collision.gameObject.GetComponentInParent<FoodStats>();
            DraggingScript dragging = collision.gameObject.GetComponentInParent<DraggingScript>();

            if (food == null || dragging == null || !dragging.isFood)
                return;

            if (!foodCurrentlyOnStove.Contains(food))
            {
                foodCurrentlyOnStove.Add(food);
                foodOnStove++;
            }

            // No oil = don't cook
            if (OliveOilAmount <= 0)
                return;

            Debug.Log("Starting to cook: " + food.gameObject.name);

            food.StartCooking();

            if (foodOnStove == 1 &&
                CookingVFX != null &&
                CookingVFX.Length > 0)
            {
                CookingVFX[0]?.Invoke();
            }
        }
    }

    private void OnCollisionStay(UnityEngine.Collision collision)
    {
        // No oil = cannot cook
        if (OliveOilAmount <= 0)
            return;

        FoodStats food = collision.gameObject.GetComponentInParent<FoodStats>();
        DraggingScript dragging = collision.gameObject.GetComponentInParent<DraggingScript>();

        if (food == null || dragging == null || !dragging.isFood)
            return;

        // Restart cooking after flipping
        if (!food.isCooking && !dragging.dragging)
        {
            Debug.Log("Food still on stove - restarting cooking: " + food.gameObject.name);
            food.StartCooking();
        }
    }

    private void OnCollisionExit(UnityEngine.Collision collision)
    {
        FoodStats food = collision.gameObject.GetComponentInParent<FoodStats>();
        DraggingScript dragging = collision.gameObject.GetComponentInParent<DraggingScript>();

        if (food == null || dragging == null || !dragging.isFood)
            return;

        Debug.Log("Stopping cooking: " + food.gameObject.name);

        food.StopCooking();

        if (foodCurrentlyOnStove.Contains(food))
        {
            foodCurrentlyOnStove.Remove(food);

            foodOnStove--;
            foodOnStove = Mathf.Max(0, foodOnStove);
        }

        if (foodOnStove == 0 &&
            CookingVFX != null &&
            CookingVFX.Length > 1)
        {
            CookingVFX[1]?.Invoke();
        }
    }

    private void StopAllFoodCooking()
    {
        Debug.Log("Oil empty - stopping all food cooking.");

        foreach (FoodStats food in foodCurrentlyOnStove)
        {
            if (food != null)
            {
                food.StopCooking();
            }
        }

        if (CookingVFX != null &&
            CookingVFX.Length > 1)
        {
            CookingVFX[1]?.Invoke();
        }
    }
}