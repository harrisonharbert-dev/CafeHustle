using System.Collections;
using TMPro;
using UnityEngine;

public class FoodStats : MonoBehaviour
{
    public float cookingTime; // Time required to cook the food

    [SerializeField] private float cookingProgress;
    public bool isCooking;
    [SerializeField] private TextMeshProUGUI CookingStats;
    public bool IsHovering;

    public cookingStatus cookingStatusScript; //Script ref for the shader script

    private float burnThreshold = 1.5f; // Time multiplier after which food gets burnt
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /*foodRenderer = GetComponent<Renderer>();*/
    }

    // Update is called once per frame
    void Update()
    {
        if (isCooking)
        {
            cookingProgress += Time.deltaTime;     
            UpdateCookingStatus();
        }
        if (IsHovering)
        {
            CookingStats.text = $"{((cookingProgress / cookingTime) * 100f).ToString("F0")}%" + " cooked";
        }
      
    }

    private void UpdateCookingStatus()
    {
        cookingStatusScript.targetValue = (cookingProgress/10);
    }

    public void StartCooking()
    {
        isCooking = true;
        
    }

    public void StopCooking()
    {
        isCooking = false;
                      
    }
    public void OnMouseEnter()
    {
        
        IsHovering = true;
    }
    public void OnMouseExit()
    {
        IsHovering = false;
        CookingStats.text = "";
    }
}