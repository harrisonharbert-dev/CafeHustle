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
        UpdateOilText();
    }

    private void Update()
    {
        if (foodOnStove > 0 && OliveOilAmount > 0)
        {
            OliveOilMeter.fillAmount -= Time.deltaTime * foodOnStove / 20f;
            OliveOilMeter.fillAmount = Mathf.Clamp01(OliveOilMeter.fillAmount);

            OliveOilAmount = Mathf.RoundToInt(OliveOilMeter.fillAmount * 100);

            UpdateOilText();

            if (OliveOilAmount <= 0)
            {
                StopAllFoodCooking();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // OLIVE OIL
        // Keep the original detection that was working
        if (collision.gameObject.CompareTag("OliveOil"))
        {
            Debug.Log("Olive Oil detected!");

            OliveOil oliveOil = collision.gameObject.GetComponent<OliveOil>();

            if (oliveOil != null)
            {
                oliveOil.SetStove(this);
            }
            else
            {
                Debug.LogWarning("OliveOil script is missing from the tagged OliveOil object!");
            }

            return;
        }

        // FOOD
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

    private void OnCollisionStay(Collision collision)
    {
        // OLIVE OIL
        if (collision.gameObject.CompareTag("OliveOil"))
        {
            OliveOil oliveOil = collision.gameObject.GetComponent<OliveOil>();

            if (oliveOil != null)
            {
                oliveOil.SetStove(this);
            }

            return;
        }

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

    private void OnCollisionExit(Collision collision)
    {
        // OLIVE OIL
        if (collision.gameObject.CompareTag("OliveOil"))
        {
            Debug.Log("Olive Oil left stove!");

            OliveOil oliveOil = collision.gameObject.GetComponent<OliveOil>();

            if (oliveOil != null)
            {
                oliveOil.RemoveStove(this);
            }

            return;
        }

        // FOOD
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

    public void TopUpOil()
    {
        OliveOilAmount = 100f;
        OliveOilMeter.fillAmount = 1f;

        UpdateOilText();

        Debug.Log("Olive oil topped up!");

        // Restart food cooking
        foreach (FoodStats food in foodCurrentlyOnStove)
        {
            if (food != null && !food.isCooking)
            {
                food.StartCooking();
            }
        }

        if (foodOnStove > 0 &&
            CookingVFX != null &&
            CookingVFX.Length > 0)
        {
            CookingVFX[0]?.Invoke();
        }
    }

    private void UpdateOilText()
    {
        if (OliveOilText != null)
        {
            OliveOilText.text =
                "Oil Amount: " + Mathf.RoundToInt(OliveOilAmount) + "%";
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