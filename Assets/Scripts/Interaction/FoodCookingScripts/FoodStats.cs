using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class FoodStats : MonoBehaviour
{
    public float cookingTime; // Time required to cook the food

    [SerializeField] private float cookingProgress;
    [HideInInspector]
    public bool isCooking;
    //[SerializeField] private TextMeshProUGUI CookingStats;
    [HideInInspector]
    public bool IsHovering;

    [HideInInspector] public cookingStatus cookingStatusScript; //Script ref for the shader script
    
    [SerializeField] private float burnThreshold = 1.5f; // Time multiplier after which food gets burnt*/

    public GameObject CookingUI;
    public Image CookingBar; // UI element to visually represent cooking progress
    public Material BurntMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CookingUI.gameObject.SetActive(false);
        cookingStatusScript = GetComponent<cookingStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isCooking)
        {
            cookingProgress += Time.deltaTime;     
            UpdateCookingStatus();

            //Shader
            cookingStatusScript.UpdateShaderStatus();
        }
        if (IsHovering && isCooking)
        {
            //CookingStats.text = $"{((cookingProgress / cookingTime) * 100f).ToString("F0")}%" + " cooked";
            CookingBar.fillAmount = (cookingProgress / (cookingTime));
        }
     
    }

    private void UpdateCookingStatus()
    {
        cookingStatusScript.progress = (cookingProgress/cookingTime);
        switch (cookingStatusScript.progress)
        {
            case < 0.5f:
                CookingBar.color = Color.blue;
                break;
            case >= 1f and <= 1.3f:
                CookingBar.color = Color.green;
                break;
            case >= 1.3f and < 1.5f:
                CookingBar.color = Color.yellow;
                break;
            case >1.5f:
                CookingBar.color = Color.red;
             this.gameObject.GetComponent<Renderer>().material = BurntMaterial;
                break;
        }
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
        if (isCooking == true)
        {
            CookingUI.gameObject.SetActive(true);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            CookingUI.transform.position = screenPos + new Vector3(0, 50f, 0); // 50px above
        }
    }
    public void OnMouseExit()
    {
        IsHovering = false;
        //CookingStats.text = "";
        CookingUI.gameObject.SetActive(false);
    }
}