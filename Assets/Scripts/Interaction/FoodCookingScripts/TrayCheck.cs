using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class TrayCheck : MonoBehaviour
{
    public List<FoodStats> foodsOnTray = new List<FoodStats>();

    public GameObject NextSectionUI;

    [Header("Tray Animation")]
    [SerializeField] private Transform trayTargetPoint;
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private Ease moveEase = Ease.InOutQuad;

    [Header("Cook Tolerance")]
    [SerializeField] private float minCookRatio = 0.8f;
    [SerializeField] private float maxCookRatio = 1.2f;

    [Header("Emotes")]
    public TextResponse EmoteResponse;

    public UnityEvent Success;

    private bool hasSucceeded = false;
    private bool isMovingTray = false;

    public CameraController CameraController;


    private void Start()
    {
        if (NextSectionUI != null)
        {
            NextSectionUI.SetActive(false);
        }

        if (EmoteResponse == null)
        {
            EmoteResponse = FindAnyObjectByType<TextResponse>();
        }

        CameraController = FindAnyObjectByType<CameraController>();
    }


    private void Update()
    {
        if (hasSucceeded)
            return;

        int oldCount = foodsOnTray.Count;

        foodsOnTray.RemoveAll(item => item == null);

        if (foodsOnTray.Count != oldCount)
        {
            CheckTrayRequirements();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        FoodStats food = other.GetComponent<FoodStats>();

        if (food != null && !foodsOnTray.Contains(food))
        {
            foodsOnTray.Add(food);

            Debug.Log("Food added to tray: " + food.name);

            CheckTrayRequirements();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        FoodStats food = other.GetComponent<FoodStats>();

        if (food != null && foodsOnTray.Contains(food))
        {
            foodsOnTray.Remove(food);

            CheckTrayRequirements();
        }
    }


    public bool IsTrayValid()
    {
        if (GameManager.Instance == null)
            return false;

        Order order = GameManager.Instance.currentOrder;

        if (order == null)
            return false;

        if (foodsOnTray.Count == 0)
        {
            SetEmote("");
            return false;
        }


        Dictionary<FoodStats.FoodType, int> trayCounts =
            new Dictionary<FoodStats.FoodType, int>();


        foreach (FoodStats food in foodsOnTray)
        {
            if (food == null)
                continue;

            if (!IsFoodCookedProperly(food))
            {
                if (food.CookRatio < minCookRatio)
                {
                    SetEmote("Food is undercooked!");
                }
                else if (food.CookRatio > maxCookRatio)
                {
                    SetEmote("Food is overcooked!");
                }
                else
                {
                    SetEmote("Food isn't cooked properly!");
                }

                return false;
            }


            if (!trayCounts.ContainsKey(food.foodType))
            {
                trayCounts.Add(food.foodType, 0);
            }

            trayCounts[food.foodType]++;
        }


        Dictionary<FoodStats.FoodType, int> requiredCounts =
            new Dictionary<FoodStats.FoodType, int>();


        foreach (OrderItem req in order.requiredItems)
        {
            if (!requiredCounts.ContainsKey(req.type))
            {
                requiredCounts.Add(req.type, 0);
            }

            requiredCounts[req.type] += req.amount;
        }


        foreach (KeyValuePair<FoodStats.FoodType, int> required in requiredCounts)
        {
            int trayAmount = 0;

            if (trayCounts.ContainsKey(required.Key))
            {
                trayAmount = trayCounts[required.Key];
            }

            if (trayAmount < required.Value)
            {
                SetEmote("Need more " + required.Key + "!");
                return false;
            }

            if (trayAmount > required.Value)
            {
                SetEmote("Too much " + required.Key + "!");
                return false;
            }
        }


        foreach (KeyValuePair<FoodStats.FoodType, int> trayFood in trayCounts)
        {
            if (!requiredCounts.ContainsKey(trayFood.Key))
            {
                SetEmote("Extra food on tray!");
                return false;
            }
        }


        SetEmote("");

        return true;
    }


    private bool IsFoodCookedProperly(FoodStats food)
    {
        if (food == null)
            return false;

        if (food.cookingTime <= 0)
            return false;

        if (food.requiresTwoSides)
        {
            float sideOneRatio = GetSideOneRatio(food);
            float sideTwoRatio = GetSideTwoRatio(food);

            bool sideOneValid =
                sideOneRatio >= minCookRatio &&
                sideOneRatio <= maxCookRatio;

            bool sideTwoValid =
                sideTwoRatio >= minCookRatio &&
                sideTwoRatio <= maxCookRatio;

            return sideOneValid && sideTwoValid;
        }

        return food.CookRatio >= minCookRatio &&
               food.CookRatio <= maxCookRatio;
    }


    private float GetSideOneRatio(FoodStats food)
    {
        var field = typeof(FoodStats).GetField(
            "sideOneProgress",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );

        if (field != null)
        {
            float progress = (float)field.GetValue(food);

            return progress / food.cookingTime;
        }

        return 0f;
    }


    private float GetSideTwoRatio(FoodStats food)
    {
        var field = typeof(FoodStats).GetField(
            "sideTwoProgress",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );

        if (field != null)
        {
            float progress = (float)field.GetValue(food);

            return progress / food.cookingTime;
        }

        return 0f;
    }


    public void CheckTrayRequirements()
    {
        if (hasSucceeded || isMovingTray)
            return;

        if (IsTrayValid())
        {
            TriggerNextSection();
        }
    }


    private void SetEmote(string message)
    {
        if (EmoteResponse != null)
        {
            EmoteResponse.SetText(message);
        }
    }


    private void TriggerNextSection()
    {
        if (hasSucceeded)
            return;

        hasSucceeded = true;

        SetEmote("");

        Success?.Invoke();

        CameraController.transitioning = true;

        StartCoroutine(EnterNextStage());
    }


    public IEnumerator EnterNextStage()
    {
        Debug.Log("EnterNextStage started.");

        List<FoodStats> foodToMove =
            new List<FoodStats>(foodsOnTray);


        // --------------------------------------------------
        // STOP ALL FOOD ANIMATIONS / PHYSICS
        // --------------------------------------------------

        foreach (FoodStats food in foodToMove)
        {
            if (food == null)
                continue;

            DraggingScript dragging =
                food.GetComponent<DraggingScript>();

            if (dragging != null)
            {
                dragging.Interactable = false;
            }

            // Kill any DOTween animations on the food.
            food.transform.DOKill();

            Rigidbody rb =
                food.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }


        // --------------------------------------------------
        // SAVE FOOD WORLD TRANSFORMS
        // --------------------------------------------------

        List<Vector3> positions =
            new List<Vector3>();

        List<Quaternion> rotations =
            new List<Quaternion>();

        List<Vector3> scales =
            new List<Vector3>();


        foreach (FoodStats food in foodToMove)
        {
            if (food == null)
            {
                positions.Add(Vector3.zero);
                rotations.Add(Quaternion.identity);
                scales.Add(Vector3.one);
                continue;
            }

            positions.Add(food.transform.position);
            rotations.Add(food.transform.rotation);

            // Save LOCAL scale.
            // We never change it.
            scales.Add(food.transform.localScale);
        }


        // --------------------------------------------------
        // MOVE TRAY
        // --------------------------------------------------

        if (trayTargetPoint == null)
        {
            Debug.LogError(
                "Tray Target Point has not been assigned!"
            );

            CameraController.transitioning = false;

            yield break;
        }


        isMovingTray = true;


        transform.DOKill();


        Vector3 trayStartPosition =
            transform.position;

        Vector3 trayEndPosition =
            trayTargetPoint.position;


        Tween trayTween =
            transform.DOMove(
                trayEndPosition,
                moveDuration
            )
            .SetEase(moveEase);


        // --------------------------------------------------
        // MOVE FOOD WITH THE TRAY WITHOUT PARENTING
        // --------------------------------------------------

        while (trayTween.IsActive() && trayTween.IsPlaying())
        {
            Vector3 trayOffset =
                transform.position - trayStartPosition;


            for (int i = 0; i < foodToMove.Count; i++)
            {
                FoodStats food = foodToMove[i];

                if (food == null)
                    continue;

                food.transform.position =
                    positions[i] + trayOffset;

                food.transform.rotation =
                    rotations[i];

                // Explicitly keep the original scale.
                food.transform.localScale =
                    scales[i];
            }


            yield return null;
        }


        // Make sure the final position is exact.
        Vector3 finalOffset =
            trayEndPosition - trayStartPosition;


        for (int i = 0; i < foodToMove.Count; i++)
        {
            FoodStats food = foodToMove[i];

            if (food == null)
                continue;

            food.transform.position =
                positions[i] + finalOffset;

            food.transform.rotation =
                rotations[i];

            food.transform.localScale =
                scales[i];
        }


        isMovingTray = false;


        CameraController.NextStage(2);

        CameraController.transitioning = false;


        foreach (FoodStats food in foodToMove)
        {
            if (food == null)
                continue;

            Rigidbody rb =
                food.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.constraints =
                    RigidbodyConstraints.None;
            }

            yield return new WaitForSeconds(1f);


            DraggingScript dragging =
                food.GetComponent<DraggingScript>();

            if (dragging != null)
            {
                dragging.Interactable = true;
            }
        }


        Debug.Log("Tray reached target point.");
    }
}