using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using DG.Tweening;


public class ModularFlippingManager : MonoBehaviour
{

    [SerializeField] private List<FlipStage> flipStages;
    public FlipStage loadedFlipStage;

    private GameObject panPrefab;
    private GameObject foodInPanPrefab;

    private bool isMinigameRunning = false;

    [SerializeField] private GameObject popUpUI;
    [SerializeField] private Image uiCounter;
    private bool popUpActive = false;

    [SerializeField] private GameObject target;
    private Vector2 targetRange;
    private float flipHeight = 0.5f;
    private int flipNums = 1;
    private float flipDuration = 1f;
    private Vector3 flipRotation = new Vector3(360f, 0f, 0f);


    [Header("Events")]
    public UnityEvent onWinEvent;
    public UnityEvent onFailEvent;

    public UnityEvent onPopUpStart;
    public UnityEvent onPopUpEnd;

    void Awake()
    {
        if (loadedFlipStage != null)
        {
            InitializeFlipStage();
        }
        else if (loadedFlipStage == null)
        {
            Debug.LogWarning($"{this} No flipping stage loaded, loading default.");

            LoadFlipStage("default");
            InitializeFlipStage();
        }
    }

    void Start()
    {
        StartStage();
    }


    void LoadFlipStage(string name)
    {
        //Find stage name by id string
        var match = flipStages?.Find(p => p != null && p.stageID == name);
        loadedFlipStage = match;
    }

    void InitializeFlipStage()
    {
        Debug.Log("Initialized");
        //Load pan prefab
        panPrefab = Instantiate(loadedFlipStage.panPrefab, gameObject.transform);
        panPrefab.transform.localPosition = new Vector3(0f, 0f, 0f);
        panPrefab.transform.rotation = loadedFlipStage.panRotation;
        panPrefab.transform.localScale = loadedFlipStage.panScale;

        //Load food prefab
        foodInPanPrefab = Instantiate(loadedFlipStage.foodInPanPrefab, panPrefab.transform);
        //Set Default scale and pos
        foodInPanPrefab.transform.localPosition = loadedFlipStage.foodInPanPosition;
        foodInPanPrefab.transform.localScale = loadedFlipStage.foodInPanScale;
        foodInPanPrefab.transform.rotation = loadedFlipStage.foodInPanRotation;
    }

    void ClearStage()
    {
        if(panPrefab != null && foodInPanPrefab != null){
        Destroy(panPrefab);
        Destroy(foodInPanPrefab);
        }
    }

    void StartStage()
    {
        Debug.Log("Minigame Started");
        StartCoroutine(MinigameTimer(loadedFlipStage.gameDuration));
    }

    private IEnumerator MinigameTimer(float duration)
    {
        isMinigameRunning = true;
        float endTime = Time.time + duration;
        float minInterval = Mathf.Max(0f, loadedFlipStage.popUpTiming.x);
        float maxInterval = Mathf.Max(minInterval, loadedFlipStage.popUpTiming.y);
        float endBuffer = Mathf.Max(0f, loadedFlipStage.popUpDuration);

        while (isMinigameRunning && Time.time < endTime)
        {
            float remainingTime = endTime - Time.time;
            float interval = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(Mathf.Min(interval, remainingTime));

            if (!isMinigameRunning || Time.time + endBuffer >= endTime)
            {
                break;
            }

            SpawnPopUp();
        }

        EndStage();
    }

    private void SpawnPopUp()
    {
        setTarget();
        DOTween.Clear();
        //show UI
        uiCounter.fillAmount = 0f;
        popUpState(true);

        uiCounter.DOFillAmount(1f, loadedFlipStage.popUpDuration).SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            popUpState(false);
            onFailEvent?.Invoke();
        });
    }

    public void onPlayerInteract(InputAction.CallbackContext context)
    {
        if (context.performed && popUpActive)
        {
            DOTween.Clear();
            popUpState(false);

            if(isWithinTargetLimit())
            {
                onFlipFood(foodInPanPrefab);
                onWinEvent?.Invoke();
                Debug.Log("Yay you flipped it");
            }
            else
            {
                onFailEvent?.Invoke();
            }

            
        }
    }


    bool isWithinTargetLimit()
    {
        if (uiCounter.fillAmount > targetRange.x && uiCounter.fillAmount < targetRange.y)
        {
            return true;
        }
        Debug.Log($"{uiCounter.fillAmount} target: {targetRange}");
        return false;
    }
    void setTarget()
    {
        //Set position from random
        float randomPositionLocal = Random.Range(loadedFlipStage.targetPositionRange.x, loadedFlipStage.targetPositionRange.y);
        float targetPosX = Remap(randomPositionLocal, 0f, 1f, -50f, 50f);

        RectTransform rect = target.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(targetPosX, rect.anchoredPosition.y);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, loadedFlipStage.targetSize * 100f);

        //Get the range the marker has to be
        targetRange = new Vector2(randomPositionLocal - (loadedFlipStage.targetSize / 2), randomPositionLocal + (loadedFlipStage.targetSize / 2));
    }

    float Remap(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public void onFlipFood(GameObject food)
    {
        Vector3 initialPos = food.transform.position;
        
        food.transform.DOLocalJump(initialPos, flipHeight, flipNums, flipDuration);
        food.transform.DOLocalRotate(flipRotation, flipDuration, RotateMode.FastBeyond360).SetRelative();
    }

    void popUpState(bool option)
    {
        popUpUI.SetActive(option);
        popUpActive = option;
    }

    void EndStage()
    {
        isMinigameRunning = false;
        Debug.Log("Minigame Ended");
    }


}