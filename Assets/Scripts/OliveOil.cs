using UnityEngine;
using DG.Tweening;
using System.Collections;

public class OliveOil : MonoBehaviour
{
    [Header("Rotation")]
    public Vector3 RotateAmount = new Vector3(0, 0, 90);

    [Header("Animation")]
    public float RotateSpeed = 0.3f;
    public float HoldTime = 1f;

    private Vector3 defaultRotation;
    private Sequence pourSequence;

    private Stove currentStove;
    private DraggingScript draggingScript;
    private void Start()
    {
        defaultRotation = transform.localEulerAngles;
        draggingScript = GetComponent<DraggingScript>();
    }

    private void Update()
    {
        // Right Click
        if (Input.GetMouseButtonDown(1))
        {
            if (currentStove != null)
            {
                PourOil();
                StartCoroutine(ReturnOil());
            }
        }
    }
    IEnumerator ReturnOil()
    {
        yield return new WaitForSeconds(1.2f);
        draggingScript.ReturnToPosition();
    }
    public void SetStove(Stove stove)
    {
        currentStove = stove;
    }

    public void RemoveStove(Stove stove)
    {
        if (currentStove == stove)
        {
            currentStove = null;
        }
    }

    private void PourOil()
    {
        // Don't spam animation
        if (pourSequence != null && pourSequence.IsActive())
            return;

        // Remember the stove BEFORE rotating
        Stove stoveToFill = currentStove;

        Vector3 pourRotation = defaultRotation + RotateAmount;

        pourSequence = DOTween.Sequence();

        // Rotate into pouring position
        pourSequence.Append(
            transform.DOLocalRotate(pourRotation, RotateSpeed)
                .SetEase(Ease.InOutSine)
        );

        // Fill the stove
        pourSequence.AppendCallback(() =>
        {
            if (stoveToFill != null)
            {
                stoveToFill.TopUpOil();

                Debug.Log("Oil poured into stove!");
            }
        });

        // Stay tilted for 1 second
        pourSequence.AppendInterval(HoldTime);

        // Rotate back
        pourSequence.Append(
            transform.DOLocalRotate(defaultRotation, RotateSpeed)
                .SetEase(Ease.InOutSine)
        );

        pourSequence.OnComplete(() =>
        {
            pourSequence = null;
        });
    }

    private void OnDestroy()
    {
        pourSequence?.Kill();
    }
}