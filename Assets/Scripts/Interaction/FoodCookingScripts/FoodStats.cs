using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodStats : MonoBehaviour
{
    [Header("Food Type")]
    public FoodType foodType;

    public enum FoodType
    {
        Bacon,
        Egg,
        Tomato,
        Sausage,
    }


    [Header("Cooking")]
    public float cookingTime; // Time required to cook one side

    [SerializeField] private float cookingProgress;

    [Header("Meat Cooking")]
    public bool requiresTwoSides;

    [SerializeField] private float sideOneProgress;
    [SerializeField] private float sideTwoProgress;

    public int currentSide = 1;


    // Inspector testing
    public bool isCooking;
    public bool IsHovering;


    [Header("Burn")]
    [SerializeField] private float burnThreshold = 1.5f;

    [Header("References")]
    public GameObject CookingUI;
    public Image CookingBar;
    public Material BurntMaterial;

    private Tween cookingBarTween;

    private Material baseMaterial;

    public Animator FlickerAnimation;
    public DraggingScript draggingScript;

    [HideInInspector] public cookingStatus cookingStatusScript;


    // Cooking checks
    public bool SideOneCooked => sideOneProgress >= cookingTime;
    public bool SideTwoCooked => sideTwoProgress >= cookingTime;
    [Header("Flip Animation")]
    public float flipDuration = 0.6f;
    public float flipHeight = 0.25f;
    public bool isFlipping;

    public Transform foodModel; // Drag your visible food mesh here
    public bool FullyCooked
    {
        get
        {
            if (requiresTwoSides)
                return SideOneCooked && SideTwoCooked;

            return cookingProgress >= cookingTime;
        }
    }


    public float CookRatio
    {
        get
        {
            if (requiresTwoSides)
                return cookingProgress / cookingTime;

            return cookingProgress / cookingTime;
        }
    }


    void Start()
    {
        draggingScript = GetComponent<DraggingScript>();
        cookingStatusScript = GetComponent<cookingStatus>();

        FlickerAnimation = CookingBar.GetComponent<Animator>();

        baseMaterial = GetComponent<Renderer>().material;


        // Meat needs two sides
        requiresTwoSides = foodType == FoodType.Bacon ||
                           foodType == FoodType.Sausage;
    }


    void Update()
    {
        if (isCooking)
        {
            if (requiresTwoSides)
            {
                // Cook current side
                if (currentSide == 1)
                {
                    sideOneProgress += Time.deltaTime;
                    cookingProgress = sideOneProgress;
                }
                else
                {
                    sideTwoProgress += Time.deltaTime;
                    cookingProgress = sideTwoProgress;
                }
            }
            else
            {
                cookingProgress += Time.deltaTime;
            }


            UpdateCookingStatus();

            cookingStatusScript.UpdateShaderStatus(cookingProgress);
        }


        if (IsHovering && !draggingScript.dragging)
        {
            CookingBar.fillAmount = cookingProgress / cookingTime;


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


            if (cookingStatusScript.progress < 1.1f || !isCooking)
            {
                FlickerAnimation.SetBool("IsFlickering", false);
            }
        }


        if (CameraController.isMoving || draggingScript.dragging)
        {
            CookingUI.SetActive(false);
        }
    }



    private void UpdateCookingStatus()
    {
        cookingStatusScript.progress = cookingProgress / cookingTime;
    }



    public void StartCooking()
    {
        isCooking = true;

        baseMaterial.EnableKeyword("_ISCOOKING");
    }



    public void StopCooking()
    {
        isCooking = false;

        baseMaterial.DisableKeyword("_ISCOOKING");
    }



    // Call this when food gets flipped
    public void FlipFood()
    {
        if (!requiresTwoSides || isFlipping)
            return;


        // Stop cooking while flipping
        StopCooking();

        isFlipping = true;


        // Pick model to rotate
        Transform target = foodModel != null ? foodModel : transform;


        Sequence flipSequence = DOTween.Sequence();


        // Jump upwards
        flipSequence.Append(
            target.DOMoveY(
                target.position.y + flipHeight,
                flipDuration / 2f
            )
            .SetEase(Ease.OutQuad)
        );


        // Rotate halfway through the jump
        flipSequence.Join(
            target.DORotate(
                target.eulerAngles + new Vector3(180f, 0f, 0f),
                flipDuration
            )
            .SetEase(Ease.InOutSine)
        );


        // Come back down
        flipSequence.Append(
            target.DOMoveY(
                target.position.y,
                flipDuration / 2f
            )
            .SetEase(Ease.InQuad)
        );


        flipSequence.OnComplete(() =>
        {
            // Change cooking side
            currentSide = currentSide == 1 ? 2 : 1;


            // Load correct progress
            cookingProgress = currentSide == 1
                ? sideOneProgress
                : sideTwoProgress;


            isFlipping = false;


            Debug.Log("Now cooking side " + currentSide);
        });
    }


    public bool IsFoodReady()
    {
        return FullyCooked;
    }



    public void OnMouseEnter()
    {
        IsHovering = true;

        CookingUI.SetActive(true);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        CookingUI.transform.position =
            screenPos + new Vector3(0, 50f, 0);
    }



    public void OnMouseExit()
    {
        IsHovering = false;

        CookingUI.SetActive(false);

        FlickerAnimation.SetBool("IsFlickering", false);
    }
}