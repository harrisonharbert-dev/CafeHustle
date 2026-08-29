using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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
        Bread,
    }


    [Header("Cooking")]
    public float cookingTime;

    [SerializeField] private float cookingProgress;


    [Header("Meat Cooking")]
    public bool requiresTwoSides;

    [SerializeField] private float sideOneProgress;
    [SerializeField] private float sideTwoProgress;

    public int currentSide = 1;


    [Header("Cooking State")]
    public bool isCooking;
    public bool IsHovering;


    [Header("Burn")]
    [SerializeField] private float burnThreshold = 1.5f;

    public Material BurntMaterial;

    private Material baseMaterial;


    [Header("Cooking Status")]
    public cookingStatus cookingStatusScript;


    [Header("Food Model")]
    [Tooltip("The CHILD model. Only this object will visually flip.")]
    public Transform foodModel;


    [Header("Flip Animation")]
    public float flipDuration = 0.6f;
    public float flipHeight = 0.25f;

    public bool isFlipping;


    [Header("Flip Settings")]
    public float flipCooldown = 0.7f;

    public bool canFlip = true;


    [Header("Food Events")]
    [Tooltip("Triggered once when the first side reaches 100% cooking.")]
    public UnityEvent FoodFlip;

    [Tooltip("Triggered once when both sides are fully cooked.")]
    public UnityEvent FoodCooked;

    [Tooltip("Triggered once when the food reaches the burn threshold.")]
    public UnityEvent FoodBurnt;


    // ============================================================
    // EVENT LOCKS
    // ============================================================

    [SerializeField] private bool foodFlipEventTriggered = false;

    [SerializeField] private bool foodCookedEventTriggered = false;

    [SerializeField] private bool foodBurntEventTriggered = false;

    // ============================================================
    // COOKING PROPERTIES
    // ============================================================

    public bool SideOneCooked =>
        sideOneProgress >= cookingTime;


    public bool SideTwoCooked =>
        sideTwoProgress >= cookingTime;


    public bool FullyCooked
    {
        get
        {
            if (requiresTwoSides)
            {
                return SideOneCooked &&
                       SideTwoCooked;
            }

            return cookingProgress >= cookingTime;
        }
    }


    public bool IsBurnt =>
        cookingProgress >= burnThreshold;


    public float CookRatio
    {
        get
        {
            if (cookingTime <= 0f)
                return 0f;

            return cookingProgress / cookingTime;
        }
    }


    // ============================================================
    // START
    // ============================================================

    void Start()
    {
        // Get the cookingStatus component from the parent.

        cookingStatusScript = foodModel.GetComponent<cookingStatus>();



        // Get the renderer.
        //
        // With the new hierarchy, the renderer will normally
        // be on the child foodModel.
        Renderer renderer = null;


        if (foodModel != null)
        {
            renderer =
                foodModel.GetComponent<Renderer>();
        }


        // Fallback if renderer is on the parent.
        if (renderer == null)
        {
            renderer =
                GetComponent<Renderer>();
        }


        if (renderer != null)
        {
            baseMaterial =
                renderer.material;
        }


        // Safety check.
        if (foodModel == null)
        {
            Debug.LogError(
                $"FoodStats on {gameObject.name} has no Food Model assigned!",
                this);
        }
        else if (foodModel == transform)
        {
            Debug.LogError(
                $"FoodStats on {gameObject.name}: " +
                $"Food Model cannot be the parent. " +
                $"Assign the CHILD model instead.",
                this);
        }
    }


    // ============================================================
    // UPDATE
    // ============================================================

    void Update()
    {
        // --------------------------------------------------------
        // RIGHT CLICK FLIP
        // --------------------------------------------------------

        if (canFlip &&
            requiresTwoSides &&
            IsHovering &&
            Input.GetMouseButtonDown(1) &&
            !isFlipping)
        {
            FlipFood();
        }


        // --------------------------------------------------------
        // COOKING
        // --------------------------------------------------------

        if (isCooking)
        {
            CookFood();


            // Update the cooking shader/look.
            UpdateCookingStatus();


            // Check cooked events.
            CheckCookingEvents();


            // Check burnt event.
            CheckBurning();
        }
    }


    // ============================================================
    // COOK FOOD
    // ============================================================

    private void CookFood()
    {
        if (requiresTwoSides)
        {
            if (currentSide == 1)
            {
                sideOneProgress +=
                    Time.deltaTime;

                cookingProgress =
                    sideOneProgress;
            }
            else
            {
                sideTwoProgress +=
                    Time.deltaTime;

                cookingProgress =
                    sideTwoProgress;
            }
        }
        else
        {
            cookingProgress +=
                Time.deltaTime;
        }
    }


    // ============================================================
    // UPDATE COOKING STATUS / SHADER
    // ============================================================

    private void UpdateCookingStatus()
    {
        if (cookingStatusScript == null)
            return;


        // This is the value used by cookingStatus
        // to change the appearance of the food.
        cookingStatusScript.progress =
            cookingProgress / cookingTime;


        // Update the actual shader/material appearance.
        cookingStatusScript.UpdateShaderStatus(
            cookingProgress / cookingTime
        );
    }


    // ============================================================
    // COOKING EVENTS
    // ============================================================

    private void CheckCookingEvents()
    {
        // ========================================================
        // TWO-SIDED FOOD
        // ========================================================

        if (requiresTwoSides)
        {
            // ----------------------------------------------------
            // SIDE ONE COOKED
            // ----------------------------------------------------

            if (currentSide == 1 &&
                SideOneCooked &&
                !foodFlipEventTriggered)
            {
                foodFlipEventTriggered = true;


                // IMPORTANT:
                // We DO NOT stop cooking.
                //
                // This means cookingStatus continues increasing
                // and the food can become burnt.

                if (FoodFlip != null)
                {
                    FoodFlip.Invoke();
                }
            }


            // ----------------------------------------------------
            // BOTH SIDES COOKED
            // ----------------------------------------------------

            if (SideOneCooked &&
                SideTwoCooked &&
                !foodCookedEventTriggered)
            {
                foodCookedEventTriggered = true;


                // Again, DO NOT stop cooking.
                //
                // The food can continue into the burn state.

                if (FoodCooked != null)
                {
                    FoodCooked.Invoke();
                }
            }
        }


        // ========================================================
        // ONE-SIDED FOOD
        // ========================================================

        else
        {
            if (cookingProgress >= cookingTime &&
                !foodCookedEventTriggered)
            {
                foodCookedEventTriggered = true;


                if (FoodCooked != null)
                {
                    FoodCooked.Invoke();
                }
            }
        }
    }


    // ============================================================
    // BURNING
    // ============================================================

    private void CheckBurning()
    {
        if (foodBurntEventTriggered)
            return;


        if (cookingProgress >= burnThreshold)
        {
            foodBurntEventTriggered = true;

            if (FoodBurnt != null)
            {
                FoodBurnt.Invoke();
            }
        }
    }



    // ============================================================
    // START COOKING
    // ============================================================

    public void StartCooking()
    {
        isCooking = true;


        if (baseMaterial != null)
        {
            baseMaterial.EnableKeyword(
                "_ISCOOKING");
        }
    }


    // ============================================================
    // STOP COOKING
    // ============================================================

    public void StopCooking()
    {
        isCooking = false;


        if (baseMaterial != null)
        {
            baseMaterial.DisableKeyword(
                "_ISCOOKING");
        }
    }


    // ============================================================
    // FLIP FOOD
    // ============================================================

    public void FlipFood()
    {
        if (!requiresTwoSides)
            return;


        if (isFlipping)
            return;


        if (!canFlip)
            return;


        // Don't flip if second side is already cooked.
        if (currentSide == 2 &&
            SideTwoCooked)
            return;


        // Make sure model exists.
        if (foodModel == null)
        {
            Debug.LogError(
                $"Cannot flip {gameObject.name}: " +
                $"Food Model is not assigned.",
                this);

            return;
        }


        // Make absolutely sure we aren't flipping
        // the parent object.
        if (foodModel == transform)
        {
            Debug.LogError(
                $"Cannot flip {gameObject.name}: " +
                $"Food Model is assigned to the PARENT. " +
                $"Assign the child model.",
                this);

            return;
        }


        isFlipping = true;

        canFlip = false;


        // Kill only model tweens.
        foodModel.DOKill();


        // Store model local position.
        Vector3 startPosition =
            foodModel.localPosition;


        // Store model local rotation.
        Vector3 startRotation =
            foodModel.localEulerAngles;


        Sequence flip =
            DOTween.Sequence();


        // --------------------------------------------------------
        // MODEL UP
        // --------------------------------------------------------

        flip.Append(
            foodModel.DOLocalMoveY(
                startPosition.y +
                flipHeight,
                flipDuration / 2f
            )
            .SetEase(Ease.OutQuad)
        );


        // --------------------------------------------------------
        // MODEL ROTATE 180
        // --------------------------------------------------------

        flip.Join(
            foodModel.DOLocalRotate(
                startRotation +
                new Vector3(
                    180f,
                    0f,
                    0f
                ),
                flipDuration
            )
            .SetEase(Ease.InOutSine)
        );


        // --------------------------------------------------------
        // MODEL DOWN
        // --------------------------------------------------------

        flip.Append(
            foodModel.DOLocalMoveY(
                startPosition.y,
                flipDuration / 2f
            )
            .SetEase(Ease.InQuad)
        );


        // --------------------------------------------------------
        // FLIP COMPLETE
        // --------------------------------------------------------

        flip.OnComplete(() =>
        {
            // Change cooking side.
            currentSide =
                currentSide == 1
                ? 2
                : 1;


            // Load progress from new side.
            cookingProgress =
                currentSide == 1
                ? sideOneProgress
                : sideTwoProgress;


            // Update shader immediately after flip.
            UpdateCookingStatus();


            isFlipping = false;


            StartCoroutine(
                FlipCooldown());
        });
    }


    // ============================================================
    // FLIP COOLDOWN
    // ============================================================

    IEnumerator FlipCooldown()
    {
        yield return new WaitForSeconds(
            flipCooldown);


        canFlip = true;
    }


    // ============================================================
    // FOOD READY
    // ============================================================

    public bool IsFoodReady()
    {
        return FullyCooked;
    }


    // ============================================================
    // MOUSE ENTER
    // ============================================================

    public void OnMouseEnter()
    {
        IsHovering = true;
    }


    // ============================================================
    // MOUSE EXIT
    // ============================================================

    public void OnMouseExit()
    {
        IsHovering = false;
    }
}