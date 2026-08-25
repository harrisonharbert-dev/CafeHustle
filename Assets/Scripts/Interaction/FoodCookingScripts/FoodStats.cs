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
    public float cookingTime;

    [SerializeField] private float cookingProgress;


    [Header("Meat Cooking")]
    public bool requiresTwoSides;

    [SerializeField] private float sideOneProgress;
    [SerializeField] private float sideTwoProgress;

    public int currentSide = 1;


    public bool isCooking;
    public bool IsHovering;


    [Header("Burn")]
    [SerializeField] private float burnThreshold = 1.5f;


    [Header("References")]
    public GameObject CookingUI;
    public Image CookingBar;
    public Material BurntMaterial;

    private Material baseMaterial;

    public Animator FlickerAnimation;
    public DraggingScript draggingScript;

    [HideInInspector]
    public cookingStatus cookingStatusScript;


    [Header("Flip Animation")]
    public float flipDuration = 0.6f;
    public float flipHeight = 0.25f;

    public bool isFlipping;

    public Transform foodModel;


    [Header("Flip Settings")]
    public float flipCooldown = 0.7f;

    public bool canFlip = true;


    public bool SideOneCooked =>
        sideOneProgress >= cookingTime;


    public bool SideTwoCooked =>
        sideTwoProgress >= cookingTime;



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
            return cookingProgress / cookingTime;
        }
    }



    void Start()
    {
        draggingScript = GetComponent<DraggingScript>();

        cookingStatusScript =
            GetComponent<cookingStatus>();


        if (CookingBar != null)
            FlickerAnimation =
                CookingBar.GetComponent<Animator>();


        baseMaterial =
            GetComponent<Renderer>().material;
    }



    void Update()
    {
        if (CookingBar != null)
        {
            if (canFlip == true && requiresTwoSides == true)
            {
                // Right click flip
                if (IsHovering &&
                    Input.GetMouseButtonDown(1) &&
                    isFlipping == false)
                {
                    FlipFood();
                }
            }


            if (isCooking)
            {
                if (requiresTwoSides)
                {
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


                if (cookingStatusScript != null)
                {
                    cookingStatusScript.UpdateShaderStatus(
                        cookingProgress / cookingTime
                    );
                }
            }



            if (IsHovering &&
                draggingScript != null &&
                !draggingScript.dragging)
            {
                if (CookingBar != null)
                {
                    CookingBar.fillAmount =
                        cookingProgress / cookingTime;
                }


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


                if (cookingStatusScript.progress >= 1.1f && isCooking == true)
                {
                    FlickerAnimation.SetBool(
                        "IsFlickering",
                        true);
                }
                else
                {
                    FlickerAnimation.SetBool(
                        "IsFlickering",
                        false);
                }
            }



            if (CameraController.isMoving ||
                (draggingScript != null &&
                 draggingScript.dragging))
            {
                CookingUI.SetActive(false);
            }
        }
    }



    void UpdateCookingStatus()
    {
        cookingStatusScript.progress =
            cookingProgress / cookingTime;
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



    public void FlipFood()
    {
        // Only meat has two sides
        if (!requiresTwoSides ||
            isFlipping ||
            !canFlip)
            return;


        StopCooking();


        isFlipping = true;
        canFlip = false;


        Transform target =
            foodModel != null
            ? foodModel
            : transform;



        Vector3 startPosition =
            target.position;


        Vector3 startRotation =
            target.eulerAngles;



        Sequence flip =
            DOTween.Sequence();



        flip.Append(
            target.DOMoveY(
                startPosition.y + flipHeight,
                flipDuration / 2f)
            .SetEase(Ease.OutQuad)
        );



        flip.Join(
            target.DORotate(
                startRotation +
                new Vector3(180f, 0, 0),
                flipDuration)
            .SetEase(Ease.InOutSine)
        );



        flip.Append(
            target.DOMoveY(
                startPosition.y,
                flipDuration / 2f)
            .SetEase(Ease.InQuad)
        );



        flip.OnComplete(() =>
        {
            currentSide =
                currentSide == 1
                ? 2
                : 1;



            cookingProgress =
                currentSide == 1
                ? sideOneProgress
                : sideTwoProgress;



            UpdateCookingStatus();


            if (cookingStatusScript != null)
            {
                cookingStatusScript.UpdateShaderStatus(
                    cookingProgress / cookingTime);
            }



            isFlipping = false;


            StartCoroutine(
                FlipCooldown());
        });
    }



    IEnumerator FlipCooldown()
    {
        yield return new WaitForSeconds(
            flipCooldown);

        canFlip = true;
    }



    public bool IsFoodReady()
    {
        return FullyCooked;
    }



    public void OnMouseEnter()
    {
        IsHovering = true;


        if (CookingUI != null)
            CookingUI.SetActive(true);



        if (CookingUI != null)
        {
            Vector3 screenPos =
                Camera.main.WorldToScreenPoint(
                    transform.position);


            CookingUI.transform.position =
                screenPos +
                new Vector3(0, 50f, 0);
        }
    }



    public void OnMouseExit()
    {
        IsHovering = false;


        if (CookingUI != null)
            CookingUI.SetActive(false);


        if (FlickerAnimation != null)
        {
            FlickerAnimation.SetBool(
                "IsFlickering",
                false);
        }
    }
}