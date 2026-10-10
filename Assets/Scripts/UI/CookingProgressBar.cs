
using UnityEngine;
using UnityEngine.UI;

public class CookingProgressBar : MonoBehaviour
{
    private enum BarState
    {
        Cooking,   // filling up, not cooked yet
        Cooked,    // full, exactly cooked
        Warning,   // overcooking, about to burn
        Burnt      // burnt and unusable
    }

    [Header("UI References")]
    [Tooltip("The bar object that gets enabled/disabled.")]
    [SerializeField] private GameObject barRoot;
    [Tooltip("The RectTransform that moves above the food.")]
    [SerializeField] private RectTransform barRect;
    [Tooltip("The fill Image. Must be Image Type: Filled.")]
    [SerializeField] private Image progressFill;
    [SerializeField] private Camera worldCamera;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private Vector2 screenOffset = Vector2.zero;

    [Header("Colors")]
    [Tooltip("While the food is still cooking.")]
    [SerializeField] private Color normalColor = Color.white;
    [Tooltip("When the food is fully cooked.")]
    [SerializeField] private Color cookedColor = new Color(0.3f, 0.9f, 0.3f);
    [Tooltip("Overcooking blends from Cooked Color to this as it nears burning.")]
    [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0f);
    [Tooltip("When the food is burnt.")]
    [SerializeField] private Color burntColor = Color.red;

    [Header("Overcook Warning")]
    [Tooltip("Fade in/out cycles per second right after the food is cooked.")]
    [SerializeField] private float pulseSpeed = 1.5f;
    [Tooltip("Fade in/out cycles per second just before the food burns.")]
    [SerializeField] private float pulseSpeedNearBurn = 5f;
    [Range(0f, 1f)]
    [SerializeField] private float minimumPulseAlpha = 0.2f;
    [Tooltip("Keep pulsing once the food is burnt.")]
    [SerializeField] private bool pulseWhenBurnt = false;

    [Header("Debug")]
    [Tooltip("Logs hover changes and bar state changes to the Console.")]
    [SerializeField] private bool debugLogs;

    private FoodStats currentFood;
    private FoodStats lastLoggedFood;
    private Canvas parentCanvas;
    private bool isVisible = true;

    private BarState lastState = BarState.Cooking;
    private float pulsePhase;

    // Every other Graphic in the bar (background, icons, text) so the
    // whole bar fades, not just the fill.
    private Graphic[] otherGraphics;
    private float[] otherBaseAlpha;
    public CameraController cameraController;
    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (barRoot == null && barRect != null)
            barRoot = barRect.gameObject;

        if (barRoot == null)
            barRoot = gameObject;

        if (barRect == null)
            barRect = barRoot.GetComponent<RectTransform>();

        if (barRect != null)
        {
            parentCanvas = barRect.GetComponentInParent<Canvas>();

            if (parentCanvas != null)
                parentCanvas = parentCanvas.rootCanvas;
        }

        if (progressFill == null)
            Debug.LogError("CookingProgressBar: Progress Fill is not assigned.", this);

        if (barRect == null)
            Debug.LogError("CookingProgressBar: Bar Rect is not assigned.", this);

        if (worldCamera == null)
            Debug.LogError("CookingProgressBar: No camera found. Tag your camera MainCamera or assign World Camera.", this);

        CacheOtherGraphics();

        // Force the first SetVisible call to apply.
        isVisible = true;
        SetVisible(false);
    }

    private void CacheOtherGraphics()
    {
        Graphic[] all = barRoot.GetComponentsInChildren<Graphic>(true);

        int count = 0;

        foreach (Graphic g in all)
        {
            if (g != progressFill)
                count++;
        }

        otherGraphics = new Graphic[count];
        otherBaseAlpha = new float[count];

        int index = 0;

        foreach (Graphic g in all)
        {
            if (g == progressFill)
                continue;

            otherGraphics[index] = g;
            otherBaseAlpha[index] = g.color.a;
            index++;
        }
    }

    private void Update()
    {
        if (cameraController != null && cameraController.StageIndex == 1) 
            {

                // FoodStats.HoveredFood is set by DraggingScript's pointer events.
                currentFood = FoodStats.HoveredFood;

                if (debugLogs && currentFood != lastLoggedFood)
                {
                    lastLoggedFood = currentFood;

                    Debug.Log(
                        currentFood != null
                            ? "CookingProgressBar: hovering " + currentFood.name
                            : "CookingProgressBar: no food hovered",
                        this
                    );
                }

                if (currentFood == null)
                {
                    SetVisible(false);
                    return;
                }

                if (barRect != null && worldCamera != null)
                {
                    Vector3 screenPosition = worldCamera.WorldToScreenPoint(
                        currentFood.transform.position + worldOffset);

                    // Food is behind the camera.
                    if (screenPosition.z <= 0f)
                    {
                        SetVisible(false);
                        return;
                    }

                    PositionBar(screenPosition + (Vector3)screenOffset);
                }

                SetVisible(true);
                UpdateProgress();
            }
        else 
        {
            SetVisible(false);
        }
    }

    private void PositionBar(Vector3 screenPosition)
    {
        bool overlay =
            parentCanvas == null ||
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay;

        if (overlay)
        {
            barRect.position = screenPosition;
        }
        else
        {
            // Screen Space - Camera canvas.
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPosition,
                parentCanvas.worldCamera,
                out Vector3 worldPosition
            );

            barRect.position = worldPosition;
        }
    }

    private void UpdateProgress()
    {
        if (currentFood == null || progressFill == null)
            return;

        // The active side is used, so flipping switches to the other side.
        float cookRatio = currentFood.DisplayCookRatio;
        float overcook = currentFood.OvercookRatio;

        BarState state = GetState(cookRatio, overcook);

        // Fills 0 -> 1 over the cooking time, stays full while
        // overcooking, and is full when burnt.
        progressFill.fillAmount =
            state == BarState.Burnt
                ? 1f
                : cookRatio;

        if (debugLogs && state != lastState)
        {
            Debug.Log(
                "CookingProgressBar: " + currentFood.name +
                " state " + state +
                " (seconds " + currentFood.ActiveProgress.ToString("F1") +
                ", cook " + cookRatio.ToString("F2") +
                ", overcook " + overcook.ToString("F2") + ")",
                this
            );
        }

        lastState = state;

        Color color;
        float alpha = 1f;

        switch (state)
        {
            case BarState.Burnt:
                color = burntColor;

                if (pulseWhenBurnt)
                    alpha = AdvancePulse(pulseSpeedNearBurn);
                else
                    pulsePhase = 0f;

                break;

            case BarState.Warning:
                // Green blends toward orange as it nears burning, and the
                // pulse speeds up the closer it gets.
                color = Color.Lerp(cookedColor, warningColor, overcook);

                alpha = AdvancePulse(
                    Mathf.Lerp(pulseSpeed, pulseSpeedNearBurn, overcook));

                break;

            case BarState.Cooked:
                color = cookedColor;
                pulsePhase = 0f;
                break;

            default:
                color = normalColor;
                pulsePhase = 0f;
                break;
        }

        ApplyColor(color, alpha);
    }

    private BarState GetState(float cookRatio, float overcook)
    {
        if (currentFood.IsBurnt)
            return BarState.Burnt;

        if (cookRatio < 1f)
            return BarState.Cooking;

        // Fully cooked. Anything past this point starts warning.
        return overcook > 0f
            ? BarState.Warning
            : BarState.Cooked;
    }

    // Advances the fade in/out and returns the current alpha.
    // Starts at full alpha, fades down, then back up.
    private float AdvancePulse(float cyclesPerSecond)
    {
        pulsePhase += Time.unscaledDeltaTime *
                      cyclesPerSecond *
                      Mathf.PI * 2f;

        float wave = 0.5f + 0.5f * Mathf.Cos(pulsePhase);

        return Mathf.Lerp(minimumPulseAlpha, 1f, wave);
    }

    private void ApplyColor(Color color, float alpha)
    {
        // Fill: chosen color, faded.
        Color fill = color;
        fill.a = color.a * alpha;
        progressFill.color = fill;

        // Everything else in the bar fades with it.
        for (int i = 0; i < otherGraphics.Length; i++)
        {
            if (otherGraphics[i] == null)
                continue;

            Color c = otherGraphics[i].color;
            c.a = otherBaseAlpha[i] * alpha;
            otherGraphics[i].color = c;
        }
    }

    // If this script is on the same object as Bar Root, the object itself
    // must stay active (or Update would stop), so its children are toggled.
    private void SetVisible(bool visible)
    {
        if (isVisible == visible)
            return;

        isVisible = visible;

        if (barRoot == null)
            return;

        if (barRoot == gameObject)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(visible);

            Graphic selfGraphic = GetComponent<Graphic>();

            if (selfGraphic != null)
                selfGraphic.enabled = visible;
        }
        else
        {
            barRoot.SetActive(visible);
        }
    }

    private void OnDisable()
    {
        SetVisible(false);
        currentFood = null;
    }
}