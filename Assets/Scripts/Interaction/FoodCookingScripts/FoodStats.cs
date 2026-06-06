using System.Collections;
using TMPro;
using UnityEngine;

public class FoodStats : MonoBehaviour
{
    public float cookingTime; // Time required to cook the food

    /*public Material[] cookingMaterials; // Materials representing different cooking stages
    public Material burntMaterial; // Material for burnt food
    private Renderer foodRenderer;*/

    [SerializeField] private float cookingProgress;
    public bool isCooking;
    [SerializeField] private TextMeshProUGUI CookingStats;
    public bool IsHovering;

    public cookingStatus cookingStatusScript; //Script ref for the shader script

    private float burnThreshold = 1.2f; // Time multiplier after which food gets burnt
    private bool IsBurnt;
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
            CookingStats.text = $"Cooking Time: {((cookingProgress / cookingTime) * 100f).ToString("F0")}%" + " cooked";
        }
        else
        {
                       CookingStats.text = "";
        }
    }

    private void UpdateCookingStatus()
    {
        cookingStatusScript.targetValue = (cookingProgress/10);
    }

    /*private void UpdateCookingStage()
    {
        int stage = Mathf.FloorToInt((cookingProgress / cookingTime) * cookingMaterials.Length);
        stage = Mathf.Clamp(stage, 0, cookingMaterials.Length - 1);
        foodRenderer.material = cookingMaterials[stage];
            if (cookingProgress >= cookingTime * burnThreshold)
            {
                
                if (stage == cookingMaterials.Length - 1)
                {
                    IsBurnt = true;
                    foodRenderer.material = burntMaterial;
                }
        }
    }*/

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
    }
}