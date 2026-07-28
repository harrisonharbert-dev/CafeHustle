using UnityEngine;
using DG.Tweening;
public class FoodCuttable : MonoBehaviour
{
    [Header("Correct Cut")]
    public Transform cutStart;
    public Transform cutEnd;
    private LineRenderer line;


    [Header("Tolerance")]
    public float startTolerance = 0.05f;
    public float endTolerance = 0.05f;
    public float angleTolerance = 15f;
    [Header("Cut Animation")]
    public Transform topHalf;
    public Vector3 topMovePosition;
    public Vector3 topRotation;
    public float cutAnimationTime = 0.5f;
    public bool cutSuccessful = false;
    public bool IsCorrectCut(Vector3 playerStart, Vector3 playerEnd)
    {
        float startDist = Vector3.Distance(playerStart, cutStart.position);
        float endDist = Vector3.Distance(playerEnd, cutEnd.position);

        Vector3 correctDir = (cutEnd.position - cutStart.position).normalized;
        Vector3 playerDir = (playerEnd - playerStart).normalized;

        float angle = Vector3.Angle(correctDir, playerDir);

        bool success =
            startDist <= startTolerance &&
            endDist <= endTolerance &&
            angle <= angleTolerance;

        if (success && !cutSuccessful)
        {
            Debug.Log("Perfect Cut!");
            cutSuccessful = true;
            successful();
        }
        else
        {
            Debug.Log("Wrong Cut");
        }

        return success;
    }

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.useWorldSpace = true;

        // Make it a constant width
        line.startWidth = 0.01f;
        line.endWidth = 0.01f;
    }

    void LateUpdate()
    {
        Vector3 offset = transform.up * 0.05f;

        line.SetPosition(0, cutStart.position + offset);
        line.SetPosition(1, cutEnd.position + offset);
    }
    void successful()
    {
        Sequence cutSequence = DOTween.Sequence();

        cutSequence.Append(
            topHalf.DOLocalMove(topMovePosition, cutAnimationTime)
            .SetEase(Ease.OutQuad)
        );

        cutSequence.Join(
            topHalf.DOLocalRotate(topRotation, cutAnimationTime)
            .SetEase(Ease.OutBack)
        );
    }
}