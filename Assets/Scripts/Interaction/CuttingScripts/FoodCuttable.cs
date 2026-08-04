using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class FoodCuttable : MonoBehaviour
{
    [Header("Correct Cut")]
    public Transform cutStart;
    public Transform cutEnd;

    [Header("Tolerance")]
    public float lineTolerance = 0.08f;
    public float angleTolerance = 20f;

    [Header("Slice")]
    public Material crossSectionMaterial;
    public float sliceForce = 1.5f;
    public float halfSeparation = 0.05f;

    public bool cutSuccessful;

    [Header("Cut Guide Line")]
    public LineRenderer cutGuideLine;
    public float guideHeight = 0.03f;
    public float guideWidth = 0.02f;
    void Start()
    {
        SetupCutGuide();
    }
    void SetupCutGuide()
    {
        if (cutGuideLine == null)
        {
            Debug.LogWarning("No Cut Guide LineRenderer assigned!");
            return;
        }

        cutGuideLine.positionCount = 2;
        cutGuideLine.useWorldSpace = true;

        cutGuideLine.startWidth = guideWidth;
        cutGuideLine.endWidth = guideWidth;

        Vector3 offset = Vector3.up * guideHeight;

        cutGuideLine.SetPosition(0, cutStart.position + offset);
        cutGuideLine.SetPosition(1, cutEnd.position + offset);

        cutGuideLine.enabled = true;
    }
    public bool CheckCut(Vector3 knifeDirection)
    {
        Vector3 cutDirection =
            (cutEnd.position - cutStart.position).normalized;


        float angle =
            Vector3.Angle(
                cutDirection,
                knifeDirection);


        // Allow upside down direction
        if (angle > 90f)
            angle = 180f - angle;


        if (angle > angleTolerance)
        {
            Debug.Log($"Wrong angle {angle:F1}");
            return false;
        }


        if (!cutSuccessful)
        {
            cutSuccessful = true;

            Debug.Log("Perfect Cut!");

            SliceTomato(
                cutStart.position,
                cutEnd.position);
        }


        return true;
    }

    void SliceTomato(Vector3 start, Vector3 end)
    {
        Vector3 direction =
            (end - start).normalized;


        Vector3 planeNormal =
            Vector3.Cross(
                direction,
                Vector3.up)
                .normalized;


        Vector3 slicePosition =
            (start + end) * 0.5f;


        SlicedHull hull =
            gameObject.Slice(
                slicePosition,
                planeNormal,
                crossSectionMaterial);


        if (hull == null)
        {
            Debug.LogWarning("Slice failed");
            return;
        }


        GameObject upper =
            hull.CreateUpperHull(
                gameObject,
                crossSectionMaterial);


        GameObject lower =
            hull.CreateLowerHull(
                gameObject,
                crossSectionMaterial);


    


        Rigidbody upperRB =
            upper.AddComponent<Rigidbody>();

        Rigidbody lowerRB =
            lower.AddComponent<Rigidbody>();


        upperRB.AddForce(
            planeNormal * sliceForce,
            ForceMode.Impulse);


        lowerRB.AddForce(
            -planeNormal * sliceForce,
            ForceMode.Impulse);


        Destroy(gameObject);
    }

}