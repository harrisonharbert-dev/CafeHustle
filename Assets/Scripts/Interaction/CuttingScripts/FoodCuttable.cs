using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class FoodCuttable : MonoBehaviour
{
    [Header("Correct Cut")]
    public Transform cutStart;
    public Transform cutEnd;


    [Header("Tolerance")]
    public float lineTolerance = 0.08f;
    public float angleTolerance = 20f;


    [Header("Cut Animation")]
    public Transform topHalf;
    public Vector3 topMovePosition;
    public Vector3 topRotation;
    public float cutAnimationTime = 0.5f;


    public bool cutSuccessful;


    public bool CheckCut(List<Vector3> path)
    {
        if (path.Count < 2)
            return false;


        bool touchedStart = false;
        bool touchedEnd = false;


        Vector3 cutDirection =
            (cutEnd.position -
             cutStart.position).normalized;


        Vector3 playerDirection =
            (path[path.Count - 1] -
             path[0]).normalized;


        float angle =
            Vector3.Angle(
                cutDirection,
                playerDirection);


        if (angle > angleTolerance &&
            angle < 180 - angleTolerance)
        {
            Debug.Log("Wrong angle");
            return false;
        }



        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 a = path[i];
            Vector3 b = path[i + 1];


            if (Vector3.Distance(
                ClosestPointOnLine(a, b, cutStart.position),
                cutStart.position)
                <= lineTolerance)
            {
                touchedStart = true;
            }


            if (Vector3.Distance(
                ClosestPointOnLine(a, b, cutEnd.position),
                cutEnd.position)
                <= lineTolerance)
            {
                touchedEnd = true;
            }
        }



        bool success =
            touchedStart &&
            touchedEnd;



        if (success && !cutSuccessful)
        {
            cutSuccessful = true;

            Debug.Log("Perfect Cut!");

            Successful();
        }
        else
        {
            Debug.Log("Wrong Cut");
        }


        return success;
    }



    Vector3 ClosestPointOnLine(
        Vector3 a,
        Vector3 b,
        Vector3 point)
    {
        Vector3 direction = b - a;

        float length =
            direction.magnitude;


        if (length == 0)
            return a;

              
        direction /= length;


        float distance =
            Vector3.Dot(
                point - a,
                direction);


        distance =
            Mathf.Clamp(
                distance,
                0,
                length);


        return a + direction * distance;
    }



    void Successful()
    {
        Sequence sequence =
            DOTween.Sequence();


        sequence.Append(
            topHalf.DOLocalMove(
                topMovePosition,
                cutAnimationTime)
            .SetEase(Ease.OutQuad));


        sequence.Join(
            topHalf.DOLocalRotate(
                topRotation,
                cutAnimationTime)
            .SetEase(Ease.OutBack));
    }
}