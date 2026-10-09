using UnityEngine;
using DG.Tweening;

public class OliveOil : MonoBehaviour
{
    [Header("Stove Target")]
    public Stove targetStove;
    public Transform pourPosition;

    [Header("Movement")]
    public float MoveSpeed = 0.6f;

    [Header("Rotation")]
    public Vector3 RotateAmount = new Vector3(0, 0, 90);
    public float RotateSpeed = 0.3f;

    [Header("Pouring")]
    public float HoldTime = 1f;

    [Header("Pour VFX")]
    public Transform origin;
    public GameObject streamPrefab;

    public bool isPouring = false;

    private Vector3 defaultPosition;
    private Vector3 defaultRotation;

    private Sequence pourSequence;
    private Stream currentStream;

    private bool isAnimating = false;

    private void Start()
    {
        defaultPosition = transform.position;
        defaultRotation = transform.eulerAngles;
    }

    private void OnMouseDown()
    {
        PourOil();
    }

    private void Update()
    {
        // Keep the stream attached to the bottle nozzle.
        // The stream itself does not inherit bottle rotation.
        if (currentStream != null && origin != null)
        {
            currentStream.transform.position = origin.position;
            currentStream.transform.rotation = Quaternion.identity;
        }
    }

    private void PourOil()
    {
        // Prevent repeated clicks during the animation.
        if (isAnimating)
            return;

        Stove stoveToFill = targetStove;

        if (stoveToFill == null || pourPosition == null)
        {
            Debug.LogWarning("OliveOil: Assign Target Stove and Pour Position!");
            return;
        }

        isAnimating = true;

        Vector3 pourRotation = defaultRotation + RotateAmount;

        pourSequence = DOTween.Sequence();

        // Move the bottle above the stove.
        pourSequence.Append(
            transform.DOMove(pourPosition.position, MoveSpeed)
                .SetEase(Ease.InOutSine)
        );

        // Tilt the bottle.
        pourSequence.Append(
            transform.DORotate(pourRotation, RotateSpeed)
                .SetEase(Ease.InOutSine)
        );

        // Start pouring oil.
        pourSequence.AppendCallback(() =>
        {
            StartPour();
        });

        // Keep pouring for the selected duration.
        pourSequence.AppendInterval(HoldTime);

        // Refill the stove.
        pourSequence.AppendCallback(() =>
        {
            if (stoveToFill != null)
            {
                stoveToFill.TopUpOil();
                Debug.Log("Oil poured into stove!");
            }
        });

        // Stop the oil stream.
        pourSequence.AppendCallback(() =>
        {
            EndPour();
        });

        // Return the bottle upright.
        pourSequence.Append(
            transform.DORotate(defaultRotation, RotateSpeed)
                .SetEase(Ease.InOutSine)
        );

        // Return to the original position.
        pourSequence.Append(
            transform.DOMove(defaultPosition, MoveSpeed)
                .SetEase(Ease.InOutSine)
        );

        // Allow the bottle to be clicked again.
        pourSequence.OnComplete(() =>
        {
            isAnimating = false;
            pourSequence = null;
        });

        // Recover cleanly if the animation is interrupted.
        pourSequence.OnKill(() =>
        {
            EndPour();
            isAnimating = false;
            pourSequence = null;
        });
    }

    private void StartPour()
    {
        if (streamPrefab == null || origin == null)
            return;

        if (currentStream != null)
            return;

        GameObject streamObject = Instantiate(
            streamPrefab,
            origin.position,
            Quaternion.identity
        );

        currentStream = streamObject.GetComponent<Stream>();

        if (currentStream != null)
        {
            currentStream.Begin();
            isPouring = true;
            Debug.Log("Started oil stream");
        }
        else
        {
            Destroy(streamObject);
        }
    }

    private void EndPour()
    {
        if (currentStream != null)
        {
            currentStream.End();
            Destroy(currentStream.gameObject);
            currentStream = null;
        }

        isPouring = false;
    }

    private void OnDestroy()
    {
        pourSequence?.Kill();

        if (currentStream != null)
        {
            Destroy(currentStream.gameObject);
        }
    }
}