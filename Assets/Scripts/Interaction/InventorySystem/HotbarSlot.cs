using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Hotbar hotbar;
    public int slotIndex;

    [SerializeField] private Camera cam;

    private Vector3 originalScale;

    private const float IncreasedScale = 1.5f;

    public int Amount;
    public TextMeshProUGUI TextAmount;

    private void Start()
    {
        cam = Camera.main;
        originalScale = transform.localScale;
        TextAmount.text = Amount.ToString();
    }
    public bool TakeOne()
    {
        if (Amount <= 0)
            
        return false;
        Amount--;
        TextAmount.text = Amount.ToString();
        if (Amount <= 0)
        {
            transform.DOScale(
                originalScale,
                0.2f
            ).SetEase(Ease.OutBack);
        }
        return true;
    }


    public void AddOne()
    {
        Amount++;
        TextAmount.text = Amount.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Amount <= 0)
            return;

        GameObject prefab = hotbar.GetPrefab(slotIndex);

        if (prefab == null)
            return;

        // Spawn in front of the camera
        Vector3 mouse = Input.mousePosition;
        mouse.z = 2f;

        Vector3 worldPos = cam.ScreenToWorldPoint(mouse);

        GameObject obj = Instantiate(
            prefab,
            worldPos,
            prefab.transform.rotation
        );

        // Only remove the ingredient if it was actually spawned.
        TakeOne();

        // Begin dragging immediately
        DraggingScript drag = obj.GetComponent<DraggingScript>();

        if (drag != null)
        {
            drag.dragging = true;

            Rigidbody rb = obj.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            MeshCollider col = obj.GetComponent<MeshCollider>();

            if (col != null)
                col.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Amount <= 0)
            return;

        transform.DOScale(
            originalScale * IncreasedScale,
            0.2f
        ).SetEase(Ease.OutBack);

    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (Amount <= 0)
            return;

        transform.DOScale(
            originalScale,
            0.2f
        ).SetEase(Ease.OutBack);
    }
}

