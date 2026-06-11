using DG.Tweening;
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
    private Tween cookingBarTween;

    private Material baseMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        cookingStatusScript = GetComponent<cookingStatus>();
   
        baseMaterial = GetComponent<Renderer>().material;
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
           
            CookingBar.fillAmount = (cookingProgress / (cookingTime));
            switch (cookingStatusScript.progress)
            {
                case >= 1.3f and < 1.5f:
                    CookingBar.color = Color.yellow;
                    break;
                case > 1.5f:
                    CookingBar.color = Color.red;
                    //this.gameObject.GetComponent<Renderer>().material = BurntMaterial;
                    break;
            }
        }
        if (cookingProgress >= cookingTime && IsHovering)
        {
            if (cookingBarTween == null || !cookingBarTween.IsActive())
            {
                cookingBarTween = CookingBar.DOFade(0f, 0.5f)
                                            .SetLoops(-1, LoopType.Yoyo);
            }
        }
        else
        {
            if (cookingBarTween != null && cookingBarTween.IsActive())
            {
                cookingBarTween.Kill();
                cookingBarTween = null;

                Color c = CookingBar.color;
                c.a = 1f;
                CookingBar.color = c;
            }
        }
    }

    private void UpdateCookingStatus()
    {
        cookingStatusScript.progress = (cookingProgress/cookingTime);
   
    }

    public void StartCooking()
    {
        isCooking = true;

        //Wobble when cooking
        baseMaterial.EnableKeyword("_ISCOOKING");
    }
    
    public void StopCooking()
    {
        isCooking = false;

        //disable wobble
        baseMaterial.DisableKeyword("_ISCOOKING");
    }
    public void OnMouseEnter()
    {
        IsHovering = true;

        if (isCooking)
        {
            CookingUI.gameObject.SetActive(true);

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            CookingUI.transform.position = screenPos + new Vector3(0, 50f, 0);
        }

 
    }

    public void OnMouseExit()
    {
        IsHovering = false;


        CookingUI.gameObject.SetActive(false);
    }
}