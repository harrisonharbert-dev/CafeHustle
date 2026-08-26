using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FryingMinigame : MonoBehaviour
{

    [Header("Game Items")]
    [SerializeField] private GameObject panPrefab;
    [SerializeField] private GameObject cookingFoodPrefab;


    [Header("Events")]
    public UnityEvent unityEvent;
    public UnityEvent onWinEvent;
    public UnityEvent onFailEvent;
    //Flip Settings
    private float flipHeight = 0.5f;
    private int flipNums = 1;
    private float flipDuration = 1f;

    //QTE Settings
    private bool popUpActive = false;
    private float popUpDuration = 3f;
    private float uiTransitionDuration = 0.3f;
    [SerializeField] private GameObject popUpUI;
    [SerializeField] private Image uiCounter;


    private Vector3 flipRotation = new Vector3(360f, 0f, 0f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        popUpUI.SetActive(false);
    }
    void Update()
    {
        /*if (Input.GetMouseButtonDown(1))
        {
            unityEvent.Invoke();
        }
        */
    }


    public void onPopUpEvent()
    {
        DOTween.Clear();
        //show UI
        uiCounter.fillAmount = 1f;
        popUpUI.SetActive(true);

        popUpActive = true;

        //Count down

        uiCounter.DOFillAmount(0f, popUpDuration).SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            popUpUI.SetActive(false);
            popUpActive = false;
            onFailEvent?.Invoke();
        });
    }


    // Update is called once per frame      
    public void onFlipFood(GameObject food)
    {
        

        food.transform.DOLocalJump(Vector3.zero, flipHeight, flipNums, flipDuration);
        food.transform.DOLocalRotate(flipRotation, flipDuration, RotateMode.FastBeyond360);
    }

    void popUpHide()
    {
        popUpUI.SetActive(false);
        popUpActive = false;
    }
    public void onPlayerInteract(InputAction.CallbackContext context)
    {
        if (context.performed && popUpActive)
        {
            DOTween.Clear();
            popUpUI.SetActive(false);
            popUpActive = false;
            onFlipFood(cookingFoodPrefab);
            onWinEvent?.Invoke();
            Debug.Log("Yay you flipped it");
        }
    }
}
