using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractPrompt3D : MonoBehaviour
{
    public enum iconType
    {
        speak,
        hand,
        box

    }
    [System.Serializable]
    public class iconImages
    {
        public Sprite speak;
        public Sprite hand;
        public Sprite box;
    }

    [Header("Icon")]
    [SerializeField] private iconType icon;
    [SerializeField] private iconImages iconImage;
    [SerializeField] private Image image;

    [Header("References")]
    [SerializeField] private GameObject UIContainer;
    [SerializeField] private CanvasGroup canvasGroup;



    void Start()
    {
        //Hide UI on start
        onUI(false);

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        SetIcon(icon);
    }

    private void OnEnable()
    {
        onUI(false);
    }

    public void SetIcon(iconType type)
    {
        icon = type;
        UpdateIconSprite(type);
    }

    private void OnValidate()
    {
        UpdateIconSprite(icon);
    }

    private void UpdateIconSprite(iconType type)
    {
        if (image == null || iconImage == null)
        {
            return;
        }

        switch (type)
        {
            case iconType.speak:
                image.sprite = iconImage.speak;
                break;
            case iconType.hand:
                image.sprite = iconImage.hand;
                break;
            case iconType.box:
                image.sprite = iconImage.box;
                break;
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {

        // Ignore if hidden
        if (canvasGroup.alpha == 0) return;

        //Face Camera
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(0, 180, 0);
    }

    public void onUI(bool option)
    {
        if (UIContainer != null && PlayerInputController.instance != null && PlayerInputController.instance.isinDialogue == false)
        {
            foreach (Transform child in UIContainer.transform)
            {
                child.gameObject.SetActive(option);
            }
        }

    }
}
