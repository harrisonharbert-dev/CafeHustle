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
        Bread
    }

    // ============================================================
    // COOKING
    // ============================================================

    [Header("Cooking")]
    public float cookingTime;
    [SerializeField] private float cookingProgress;

    [Header("Two Sided Cooking")]
    public bool requiresTwoSides;

    [SerializeField] private float sideOneProgress;
    [SerializeField] private float sideTwoProgress;

    public int currentSide = 1;

    [Header("Cooking State")]
    public bool isCooking;
    public bool IsHovering;

    // ============================================================
    // BURN
    // ============================================================

    [Header("Burn")]
    [SerializeField] private float burnThreshold = 1.5f;

    public Material BurntMaterial;

    private Material[] baseMaterials;

    // ============================================================
    // COOKING STATUS
    // ============================================================

    [Header("Cooking Status")]
    public cookingStatus cookingStatusScript;

    // ============================================================
    // FOOD MODEL
    // ============================================================

    [Header("Food Model")]
    [Tooltip("The CHILD model.")]
    public Transform foodModel;

    // ============================================================
    // FLIP
    // ============================================================

    [Header("Flip")]
    public float flipCooldown = 0.7f;

    public bool isFlipping;
    public bool canFlip = true;

    // ============================================================
    // EVENTS
    // ============================================================

    [Header("Food Events")]

    [Tooltip("Triggered once when the first side reaches 100% cooking.")]
    public UnityEvent FoodFlip;

    [Tooltip("Triggered once when both sides are fully cooked.")]
    public UnityEvent FoodCooked;

    [Tooltip("Triggered once when the food reaches the burn threshold.")]
    public UnityEvent FoodBurnt;

    private bool foodFlipEventTriggered;
    private bool foodCookedEventTriggered;
    private bool foodBurntEventTriggered;

    // ============================================================
    // AUDIO
    // ============================================================

    public AudioSource audioSource;
    public AudioClip[] cookingSound;

    public bool isPlayingSound;

    // ============================================================
    // PROPERTIES
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
                return SideOneCooked && SideTwoCooked;

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

    private void Start()
    {
        if (foodModel == null)
        {
            Debug.LogError(
                $"{gameObject.name} has no Food Model assigned!",
                this
            );

            return;
        }

        cookingStatusScript =
            foodModel.GetComponent<cookingStatus>();

        audioSource =
            GetComponent<AudioSource>();

        Renderer renderer =
            foodModel.GetComponent<Renderer>();

        if (renderer == null)
            renderer = GetComponent<Renderer>();

        if (renderer != null)
            baseMaterials = renderer.materials;

        // Make sure both sides visually start at 0.
        UpdateBothSides();
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (!isCooking)
            return;

        CookFood();

        UpdateCookingStatus();

        CheckCookingEvents();

        CheckBurning();
    }

    // ============================================================
    // COOK FOOD
    // ============================================================

    private void CookFood()
    {
        if (!isPlayingSound)
        {
            isPlayingSound = true;

            if (audioSource != null &&
                cookingSound != null &&
                cookingSound.Length > 0)
            {
                audioSource.loop = true;
                audioSource.clip = cookingSound[0];
                audioSource.Play();
            }
        }

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
    }

    // ============================================================
    // UPDATE COOKING VISUAL
    // ============================================================

    private void UpdateCookingStatus()
    {
        if (cookingStatusScript == null ||
            cookingTime <= 0f)
            return;

        if (requiresTwoSides)
        {
            // IMPORTANT:
            // Each side now keeps its OWN cooking appearance.

            float side1 =
                sideOneProgress / cookingTime;

            float side2 =
                sideTwoProgress / cookingTime;

            cookingStatusScript.UpdateSide1Stage(side1);
            cookingStatusScript.UpdateSide2Stage(side2);

            // Keep progress representing the currently cooking side.
            cookingStatusScript.progress =
                currentSide == 1
                ? side1
                : side2;
        }
        else
        {
            float progress =
                cookingProgress / cookingTime;

            cookingStatusScript.progress =
                progress;

            cookingStatusScript.UpdateShaderStatus(
                progress
            );
        }
    }

    // ============================================================
    // UPDATE BOTH SIDES
    // ============================================================

    private void UpdateBothSides()
    {
        if (cookingStatusScript == null ||
            cookingTime <= 0f)
            return;

        if (requiresTwoSides)
        {
            cookingStatusScript.UpdateSide1Stage(
                sideOneProgress / cookingTime
            );

            cookingStatusScript.UpdateSide2Stage(
                sideTwoProgress / cookingTime
            );
        }
        else
        {
            cookingStatusScript.UpdateShaderStatus(
                cookingProgress / cookingTime
            );
        }
    }

    // ============================================================
    // EVENTS
    // ============================================================

    private void CheckCookingEvents()
    {
        if (requiresTwoSides)
        {
            // First side cooked.
            if (currentSide == 1 &&
                SideOneCooked &&
                !foodFlipEventTriggered)
            {
                foodFlipEventTriggered = true;

                FoodFlip?.Invoke();
            }

            // Both sides cooked.
            if (SideOneCooked &&
                SideTwoCooked &&
                !foodCookedEventTriggered)
            {
                foodCookedEventTriggered = true;

                FoodCooked?.Invoke();
            }
        }
        else
        {
            if (cookingProgress >= cookingTime &&
                !foodCookedEventTriggered)
            {
                foodCookedEventTriggered = true;

                FoodCooked?.Invoke();
            }
        }
    }

    // ============================================================
    // BURN
    // ============================================================

    private void CheckBurning()
    {
        if (foodBurntEventTriggered)
            return;

        if (cookingProgress >= burnThreshold)
        {
            foodBurntEventTriggered = true;

            FoodBurnt?.Invoke();

            if (audioSource != null &&
                cookingSound != null &&
                cookingSound.Length > 1)
            {
                audioSource.PlayOneShot(
                    cookingSound[1]
                );
            }
        }
    }

    // ============================================================
    // START COOKING
    // ============================================================

    public void StartCooking()
    {
        isCooking = true;

        if (baseMaterials != null)
        {
            foreach (Material material in baseMaterials)
            {
                if (material != null)
                    material.EnableKeyword("_ISCOOKING");
            }
        }
    }

    // ============================================================
    // STOP COOKING
    // ============================================================

    public void StopCooking()
    {
        if (isCooking)
            StartCoroutine(FadeAudio());

        isCooking = false;

        if (baseMaterials != null)
        {
            foreach (Material material in baseMaterials)
            {
                if (material != null)
                    material.DisableKeyword("_ISCOOKING");
            }
        }
    }

    // ============================================================
    // AUDIO
    // ============================================================

    private IEnumerator FadeAudio()
    {
        if (audioSource == null)
            yield break;

        float startVolume =
            audioSource.volume;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -=
                startVolume *
                Time.deltaTime;

            yield return null;
        }

        audioSource.Stop();

        audioSource.volume =
            startVolume;

        isPlayingSound = false;
    }

    // ============================================================
    // FLIP FOOD
    // ============================================================

    public void FlipFood()
    {
        if (!requiresTwoSides ||
            !canFlip)
            return;

        // IMPORTANT:
        // There is NO animation here anymore.
        //
        // DraggingScript is responsible for the visual 180 degree
        // flip. FoodStats only changes which side is cooking.

        currentSide =
            currentSide == 1
            ? 2
            : 1;

        cookingProgress =
            currentSide == 1
            ? sideOneProgress
            : sideTwoProgress;

        UpdateCookingStatus();

        canFlip = false;

        StartCoroutine(
            FlipCooldown()
        );
    }

    // ============================================================
    // FLIP COOLDOWN
    // ============================================================

    private IEnumerator FlipCooldown()
    {
        yield return new WaitForSeconds(
            flipCooldown
        );

        canFlip = true;
    }

    // ============================================================
    // READY
    // ============================================================

    public bool IsFoodReady()
    {
        return FullyCooked;
    }

    // ============================================================
    // MOUSE
    // ============================================================

    public void OnMouseEnter()
    {
        IsHovering = true;
    }

    public void OnMouseExit()
    {
        IsHovering = false;
    }
}