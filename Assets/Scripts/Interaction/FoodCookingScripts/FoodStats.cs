using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FoodStats : MonoBehaviour
{
    //FoodTypeStuff
     public FoodType foodType;
    
    public float cookingTime; // Time required to cook the food

    [SerializeField] private float cookingProgress;
                         
    //Leave this viewable in the inspector for testing sake
    public bool isCooking;
    public bool IsHovering;

    [HideInInspector] public cookingStatus cookingStatusScript; //Script ref for the shader script
    
    [SerializeField] private float burnThreshold = 1.5f; // Time multiplier after which food gets burnt*/

    public GameObject CookingUI;
    public Image CookingBar; // UI element to visually represent cooking progress
    public Material BurntMaterial;
    private Tween cookingBarTween;

    private Material baseMaterial;
    public Animator FlickerAnimation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public enum FoodType
    {
        Bacon,
        Egg,
        Tomato,
        Sausage,
    }
    public float CookRatio
    {
        get
        {
            return cookingProgress / cookingTime;
        }
    }
    void Start()
    {
        
        cookingStatusScript = GetComponent<cookingStatus>();
        FlickerAnimation = CookingBar.GetComponent<Animator>();
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
        if (IsHovering)
        { 
            CookingBar.fillAmount = (cookingProgress / (cookingTime));
            if (cookingStatusScript.progress < 1.3f)
            {
                CookingBar.color = Color.green;
            }
            else if (cookingStatusScript.progress < 1.5f)
            {
                CookingBar.color = Color.yellow;
            }
            else
            {
                CookingBar.color = Color.red;
            }
        
            if (cookingStatusScript.progress >= 1.1f)
            {
                FlickerAnimation.SetBool("IsFlickering", true);
            }
            if (cookingStatusScript.progress < 1.1f || isCooking == false)
            {
                FlickerAnimation.SetBool("IsFlickering", false);
            }
        }
        if (CameraController.isMoving) //Removes UI when the camera is moving to prevent UI from being left behind in the world space
        {
            CookingUI.gameObject.SetActive(false);
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
            CookingUI.gameObject.SetActive(true);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            CookingUI.transform.position = screenPos + new Vector3(0, 50f, 0);
    }

    public void OnMouseExit()
    {
        IsHovering = false;
        CookingUI.gameObject.SetActive(false);
        FlickerAnimation.SetBool("IsFlickering", false);
    }
}