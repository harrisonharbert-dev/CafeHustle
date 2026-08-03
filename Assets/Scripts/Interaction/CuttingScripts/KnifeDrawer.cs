using System.Collections.Generic;
using UnityEngine;

public class KnifeDrawer : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform knife;
    public Transform pickupPoint;
    public Transform bladeTip;

    [Header("Layers")]
    public LayerMask knifeLayer;
    public LayerMask foodLayer;

    [Header("Board")]
    public float boardHeight = 0f;

    [Header("Knife Movement")]
    public float hoverHeight = 0.25f;
    public float cutHeight = 0.02f;
    public float knifeSpeed = 18f;

    [Header("Cut Settings")]
    public float minimumCutDistance = 0.5f;
    public float bladeCheckRadius = 0.04f;

    private bool holdingKnife;
    private bool knifeLowered;

    private Vector3 pickupOffset;

    private List<Vector3> cutPath = new List<Vector3>();

    private FoodCuttable currentFood;

    private Vector3 lastBladePosition;
    private float cutDistance;

    private Plane movementPlane;


    void Start()
    {
        pickupOffset = pickupPoint.position - knife.position;
    }


    void Update()
    {
        HandlePickup();

        if (!holdingKnife)
            return;

        MoveKnife();
        HandleCutting();
    }


    void HandlePickup()
    {
        if (holdingKnife && Input.GetMouseButtonDown(0))
        {
            holdingKnife = false;
            knifeLowered = false;
            cutPath.Clear();
            currentFood = null;
            return;
        }


        if (!holdingKnife && Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, knifeLayer))
            {
                holdingKnife = true;

                pickupOffset =
                    pickupPoint.position - knife.position;
            }
        }
    }


    void MoveKnife()
    {
        float height = knifeLowered ? cutHeight : hoverHeight;

        movementPlane.SetNormalAndPosition(
            Vector3.up,
            new Vector3(0, height, 0));


        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (movementPlane.Raycast(ray, out float distance))
        {
            Vector3 mousePosition = ray.GetPoint(distance);

            Vector3 target =
                mousePosition - pickupOffset;


            knife.position = Vector3.MoveTowards(
                knife.position,
                target,
                knifeSpeed * Time.deltaTime);
        }
    }


    void HandleCutting()
    {
        if (Input.GetMouseButtonDown(1))
        {
            knifeLowered = true;

            cutPath.Clear();

            cutPath.Add(bladeTip.position);

            lastBladePosition = bladeTip.position;

            cutDistance = 0;

            currentFood = null;
        }


        if (knifeLowered)
        {
            float movement =
                Vector3.Distance(
                    bladeTip.position,
                    lastBladePosition);


            if (movement > 0.001f)
            {
                cutDistance += movement;

                cutPath.Add(bladeTip.position);


                Vector3 direction =
                    bladeTip.position -
                    lastBladePosition;


                if (Physics.SphereCast(
                    lastBladePosition,
                    bladeCheckRadius,
                    direction.normalized,
                    out RaycastHit hit,
                    direction.magnitude,
                    foodLayer))
                {
                    FoodCuttable food =
                        hit.collider.GetComponent<FoodCuttable>();

                    if (food != null)
                        currentFood = food;
                }


                lastBladePosition = bladeTip.position;
            }
        }


        if (Input.GetMouseButtonUp(1))
        {
            knifeLowered = false;


            if (currentFood != null &&
                cutDistance >= minimumCutDistance)
            {
                currentFood.CheckCut(cutPath);
            }


            cutPath.Clear();
            currentFood = null;
        }
    }
}