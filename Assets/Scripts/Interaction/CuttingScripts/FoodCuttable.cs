using UnityEngine;
using UnityEngine.Events;
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
    public float spawnLift = 0.03f;

    public bool cutSuccessful;

    [Header("Cut Guide Line")]
    public LineRenderer cutGuideLine;
    public float guideHeight = 0.03f;
    public float guideWidth = 0.02f;

    public UnityEvent onCutSuccess;
    public UnityEvent onCutFail;

    [Header("Mesh Changes")]
    public Material cutMaterial;

    private void Start()
    {
        SetupCutGuide();
    }

    private void Update()
    {
        // Keep the guide line attached to the food
        SetupCutGuide();
    }

    void SetupCutGuide()
    {
        if (cutGuideLine == null || cutStart == null || cutEnd == null)
            return;

        cutGuideLine.positionCount = 2;
        cutGuideLine.useWorldSpace = true;

        cutGuideLine.startWidth = guideWidth;
        cutGuideLine.endWidth = guideWidth;

        Vector3 offset = Vector3.up * guideHeight;

        cutGuideLine.SetPosition(0, cutStart.position + offset);
        cutGuideLine.SetPosition(1, cutEnd.position + offset);

        cutGuideLine.enabled = true;
    }

    public bool CheckCut(Vector3 knifePosition, Vector3 knifeDirection)
    {
        if (cutSuccessful)
            return false;

        if (cutStart == null || cutEnd == null)
            return false;

        // --------------------------------------------------
        // 1. CHECK KNIFE POSITION
        // --------------------------------------------------

        Vector3 start = cutStart.position;
        Vector3 end = cutEnd.position;

        Vector3 cutLine = end - start;
        float cutLength = cutLine.magnitude;

        if (cutLength <= 0.001f)
            return false;

        Vector3 cutDirection = cutLine.normalized;

        // Find the closest point on the cut line to the knife
        Vector3 startToKnife = knifePosition - start;

        float distanceAlongLine =
            Vector3.Dot(startToKnife, cutDirection);

        // Check that the knife is actually BETWEEN cutStart and cutEnd
        if (distanceAlongLine < 0f ||
            distanceAlongLine > cutLength)
        {
            Debug.Log("Knife is outside the cut area.");

            onCutFail?.Invoke();
            return false;
        }

        // Find the closest point on the intended cut line
        Vector3 closestPoint =
            start + cutDirection * distanceAlongLine;

        // Distance from knife to the intended cut line
        float distanceFromLine =
            Vector3.Distance(knifePosition, closestPoint);

        // Check line tolerance
        if (distanceFromLine > lineTolerance)
        {
            Debug.Log(
                $"Knife is too far from cut line: {distanceFromLine:F3}"
            );

            onCutFail?.Invoke();
            return false;
        }

        // --------------------------------------------------
        // 2. CHECK KNIFE ANGLE
        // --------------------------------------------------

        float angle =
            Vector3.Angle(
                cutDirection,
                knifeDirection
            );

        // Allow the knife to point either direction
        if (angle > 90f)
            angle = 180f - angle;

        if (angle > angleTolerance)
        {
            Debug.Log($"Wrong angle {angle:F1}");

            onCutFail?.Invoke();
            return false;
        }

        // --------------------------------------------------
        // 3. SUCCESSFUL CUT
        // --------------------------------------------------

        cutSuccessful = true;

        Debug.Log("Perfect Cut!");

        SliceTomato(
            cutStart.position,
            cutEnd.position
        );

        return true;
    }

    void SliceTomato(Vector3 start, Vector3 end)
    {
        Vector3 direction =
            (end - start).normalized;

        Vector3 planeNormal =
            Vector3.Cross(
                direction,
                Vector3.up
            ).normalized;

        Vector3 slicePosition =
            (start + end) * 0.5f;

        SlicedHull hull =
            gameObject.Slice(
                slicePosition,
                planeNormal,
                crossSectionMaterial
            );

        if (hull == null)
        {
            Debug.LogWarning("Slice failed");

            cutSuccessful = false;

            onCutFail?.Invoke();

            return;
        }

        GameObject upper =
            hull.CreateUpperHull(
                gameObject,
                crossSectionMaterial
            );

        GameObject lower =
            hull.CreateLowerHull(
                gameObject,
                crossSectionMaterial
            );

        // Match original tomato transform
        upper.transform.position = transform.position;
        upper.transform.rotation = transform.rotation;
        upper.transform.localScale = transform.localScale;

        lower.transform.position = transform.position;
        lower.transform.rotation = transform.rotation;
        lower.transform.localScale = transform.localScale;

        MeshRenderer upperRenderer =
            upper.GetComponent<MeshRenderer>();

        MeshRenderer lowerRenderer =
            lower.GetComponent<MeshRenderer>();

        if (upperRenderer != null && cutMaterial != null)
            upperRenderer.material = cutMaterial;

        if (lowerRenderer != null && cutMaterial != null)
            lowerRenderer.material = cutMaterial;

        SetupSlicePhysics(upper);
        SetupSlicePhysics(lower);

        // Separate halves
        upper.transform.position +=
            planeNormal * halfSeparation;

        lower.transform.position -=
            planeNormal * halfSeparation;

        // Small lift so they don't clip into board
        upper.transform.position +=
            Vector3.up * spawnLift;

        lower.transform.position +=
            Vector3.up * spawnLift;

        Rigidbody upperRB =
            upper.GetComponent<Rigidbody>();

        Rigidbody lowerRB =
            lower.GetComponent<Rigidbody>();

        if (upperRB != null)
        {
            upperRB.AddForce(
                planeNormal * sliceForce,
                ForceMode.Impulse
            );
        }

        if (lowerRB != null)
        {
            lowerRB.AddForce(
                -planeNormal * sliceForce,
                ForceMode.Impulse
            );
        }

        Success();

        Destroy(gameObject);

        Debug.Log("Cut successful event invoked");
    }

    void SetupSlicePhysics(GameObject slice)
    {
        slice.layer = gameObject.layer;

        MeshCollider meshCollider =
            slice.AddComponent<MeshCollider>();

        meshCollider.convex = true;

        Rigidbody rb =
            slice.AddComponent<Rigidbody>();

        rb.mass = 0.2f;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;
    }

    void Success()
    {
        onCutSuccess?.Invoke();
    }
}