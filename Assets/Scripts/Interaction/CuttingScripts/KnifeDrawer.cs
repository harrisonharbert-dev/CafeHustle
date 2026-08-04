using UnityEngine;
using UnityEngine.SceneManagement;

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


    [Header("Knife Press Animation")]
    public float lowerSpeed = 8f;
    public float raiseSpeed = 10f;

    public float pressRotationAmount = 15f;


    [Header("Cut Detection")]
    public float bladeCheckRadius = 0.04f;


    private bool holdingKnife;
    private bool knifeLowered;
    private bool rotationLocked;
    private bool cutting;


    private Quaternion lockedRotation;
    private Quaternion startRotation;
    private Quaternion pressRotation;


    private Vector3 pickupOffset;

    private Plane movementPlane;


    void Start()
    {
        pickupOffset =
            pickupPoint.position -
            knife.position;


        startRotation = knife.rotation;


        pressRotation =
            startRotation *
            Quaternion.Euler(
                pressRotationAmount,
                0,
                0);
    }



    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().name);
        }
        HandlePickup();


        if (!holdingKnife)
            return;


        MoveKnife();


        HandleLowering();


        HandleCut();
    }



    void HandlePickup()
    {
        // Drop knife
        if (holdingKnife &&
           Input.GetMouseButtonDown(0))
        {
            holdingKnife = false;

            knifeLowered = false;
            cutting = false;

            rotationLocked = false;

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



        if (rotationLocked)
        {
            knife.rotation =
                lockedRotation;
        }
    }



    void HandleLowering()
    {
        if (Input.GetMouseButton(1))
        {
            knifeLowered = true;


            knife.rotation =
                Quaternion.Lerp(
                    startRotation,
                    pressRotation,
                    Time.deltaTime * lowerSpeed);


            if (!rotationLocked)
            {
                LockRotation();
            }
        }
        else
        {
            knifeLowered = false;
            rotationLocked = false;


            knife.rotation =
                Quaternion.Lerp(
                    knife.rotation,
                    startRotation,
                    Time.deltaTime * raiseSpeed);
        }
    }



    void LockRotation()
    {
        rotationLocked = true;

        lockedRotation =
            knife.rotation;
    }



    void HandleCut()
    {
        // Only cut once when releasing right click
        if (Input.GetMouseButtonUp(1))
        {
            TryCut();
        }
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
                    bladeDirection.forward);


                return;
            }
        }
    }
}