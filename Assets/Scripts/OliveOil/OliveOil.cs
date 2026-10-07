using UnityEngine;
using DG.Tweening;

public class OliveOil : MonoBehaviour
{
    [Header("Rotation")]
    public Vector3 RotateAmount = new Vector3(0, 0, 90);

    [Header("Animation")]
    public float RotateSpeed = 0.3f;
    public float HoldTime = 1f;

    [Header("Pour VFX")]
    public Transform origin;
    public GameObject streamPrefab;

    public bool isPouring = false;

    private Vector3 defaultRotation;
    private Sequence pourSequence;

    private Stove currentStove;
    private Stream currentStream;
    public DraggingScript draggingScript;
    private void Start()
    {
        defaultRotation = transform.localEulerAngles;
    }

    private void Update()
    {
        // Right click while bottle is on stove
        if (Input.GetMouseButtonDown(1))
        {
            if (currentStove != null)
            {
                PourOil();
            }
        }
        if (draggingScript.dragging == false)
        {
            EndPour();
        }
    }

    private void PourOil()
    {
        // Prevent spam
        if (pourSequence != null && pourSequence.IsActive())
            return;

        Stove stoveToFill = currentStove;

        Vector3 pourRotation = defaultRotation + RotateAmount;

        pourSequence = DOTween.Sequence();

        // Rotate bottle into pouring position
        pourSequence.Append(
            transform.DOLocalRotate(pourRotation, RotateSpeed)
                .SetEase(Ease.InOutSine)
        );

        // Start oil stream
        pourSequence.AppendCallback(() =>
        {
            StartPour();
        });

        // Stay tilted
        pourSequence.AppendInterval(HoldTime);

        // Add oil to stove
        pourSequence.AppendCallback(() =>
        {
            if (stoveToFill != null)
            {
                stoveToFill.TopUpOil();
                Debug.Log("Oil poured into stove!");
            }
        });

        // Stop stream
        pourSequence.AppendCallback(() =>
        {
            EndPour();
        });

        // Rotate bottle back
        pourSequence.Append(
            transform.DOLocalRotate(defaultRotation, RotateSpeed)
                .SetEase(Ease.InOutSine)
        );

        pourSequence.OnComplete(() =>
        {
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
            Quaternion.identity,
            origin
        );

        currentStream = streamObject.GetComponent<Stream>();

        if (currentStream != null)
        {
            currentStream.Begin();
            isPouring = true;

            Debug.Log("Started oil stream");
        }
    }

    private void EndPour()
    {
        if (currentStream != null)
        {
            currentStream.End();
            currentStream = null;
        }


        isPouring = false;

        Debug.Log("Stopped oil stream");
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

    private void OnDestroy()
    {
        pourSequence?.Kill();

        if (currentStream != null)
        {
            Destroy(currentStream.gameObject);
        }
    }
}