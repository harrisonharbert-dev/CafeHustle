        using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DG.Tweening;

public class KnifeDrawer : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform knife;
    public Transform pickupPoint;
    public Transform bladeTip;
    public Transform bladeDirection;


    [Header("Layers")]
    public LayerMask knifeLayer;
    public LayerMask foodLayer;


    [Header("Knife Movement")]
    public float hoverHeight = 0.25f;
    public float cutHeight = 0.02f;
    public float knifeMoveSpeed = 18f;


    [Header("Pickup Animation")]
    public Vector3 pickedUpRotation = new Vector3(90f, 0f, 0f);
    public float pickupRotateDuration = 0.25f;
    public Ease pickupEase = Ease.OutBack;


    [Header("Knife Lower Animation")]
    public Vector3 loweredRotation = new Vector3(110f, 0f, 0f);
    public float lowerRotateDuration = 0.15f;
    public Ease lowerEase = Ease.OutQuad;


    [Header("Cut Detection")]
    public float bladeCheckRadius = 0.04f;


    public bool holdingKnife;
    public bool knifeLowered;
    public bool cutting;
    public bool interactable = true;


    private Quaternion originalLocalRotation;

    private Vector3 pickupOffset;

    private Plane movementPlane;

    public UnityEvent onCutFail;
    void Start()
    {
        pickupOffset =
            pickupPoint.position -
            knife.position;


        originalLocalRotation =
            knife.localRotation;
        HandlePickup();
        holdingKnife = true;
    }



    void Update()
    {
        if (!interactable)
            return;


        if (Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene("CookingTest");
        }


        //HandlePickup();


        if (!holdingKnife)
            return;


        MoveKnife();


        HandleLowering();


        HandleCut();
    }



    public void HandlePickup()
    {
            // Drop knife
            if (holdingKnife &&
            Input.GetMouseButtonDown(0))
        {
            holdingKnife = false;

            knifeLowered = false;
            cutting = false;


            knife.DOKill();


            knife.DOLocalRotate(
                originalLocalRotation.eulerAngles,
                pickupRotateDuration)
                .SetEase(Ease.OutSine);


            return;
        }



        // Pickup knife
        if (!holdingKnife &&
            Input.GetMouseButtonDown(0))
        {
            Ray ray =
                cam.ScreenPointToRay(
                    Input.mousePosition);


            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                100f,
                knifeLayer))
            {
                holdingKnife = true;


                pickupOffset =
                    pickupPoint.position -
                    knife.position;


                knife.DOKill();


                knife.DOLocalRotate(
                    pickedUpRotation,
                    pickupRotateDuration)
                    .SetEase(pickupEase);
            }
        }
    }



    void MoveKnife()
    {
        movementPlane.SetNormalAndPosition(
            Vector3.up,
            new Vector3(
                0,
                Mathf.Lerp(
                    hoverHeight,
                    cutHeight,
                    knifeLowered ? 1 : 0),
                0));



        Ray ray =
            cam.ScreenPointToRay(
                Input.mousePosition);



        if (movementPlane.Raycast(
            ray,
            out float distance))
        {
            Vector3 mouse =
                ray.GetPoint(distance);



            Vector3 target =
                mouse -
                pickupOffset;



            knife.position =
                Vector3.MoveTowards(
                    knife.position,
                    target,
                    knifeMoveSpeed *
                    Time.deltaTime);
        }
    }



   public void HandleLowering()
    {
            // Press knife down
            if (Input.GetMouseButtonDown(1))
            {
                knifeLowered = true;


                knife.DOKill();


                knife.DOLocalRotate(
                    loweredRotation,
                    lowerRotateDuration)
                    .SetEase(lowerEase);
            }



            // Release knife
            if (Input.GetMouseButtonUp(1))
            {
                knifeLowered = false;


                knife.DOKill();


                knife.DOLocalRotate(
                    pickedUpRotation,
                    lowerRotateDuration)
                    .SetEase(Ease.OutSine);


                TryCut();
            }
    }



    void HandleCut()
    {
        // Kept empty because cut now happens on release
    }



    void TryCut()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                bladeTip.position,
                bladeCheckRadius,
                foodLayer);



        if (hits.Length == 0)
        {
            Debug.Log("No food detected");
            onCutFail?.Invoke();
            return;
        }



        foreach (Collider hit in hits)
        {
            FoodCuttable food =
                hit.GetComponent<FoodCuttable>();


            if (food != null)
            {
                Debug.Log("Knife hit food");


                food.CheckCut(
                    bladeTip.position,
                    bladeDirection.forward);


                return;
            }
        }
    }


    public void KnifeSuccess()
    {
        holdingKnife = false;

        knifeLowered = false;
        cutting = false;


        knife.DOKill();


        knife.DOLocalRotate(
            originalLocalRotation.eulerAngles,
            pickupRotateDuration)
            .SetEase(Ease.OutSine);
    }
}