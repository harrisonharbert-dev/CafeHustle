using UnityEngine;
using UnityEngine.UI;

public class InputIconDisplay : MonoBehaviour
{

    [SerializeField] private Image iconImage;
    [SerializeField] private InputIconSet iconSet;
    [SerializeField] private bool HideOnController;

    void Awake()
    {
        if (TryGetComponent<Image>(out Image image))
        {
            iconImage = image;
        }
        else { Debug.LogWarning($"No image attached to {this}"); }
        ;

        if (iconSet == null)
        {
            Debug.LogWarning($"Missing icon set on {this}. Assign one in inspector");
        }
    }

    void Start()
    {
        //
        
        InputIconManager.instance.onDeviceChanged += UpdateIcon;
        UpdateIcon(InputIconManager.instance.CurrentDevice);

    }

    void UpdateIcon(InputIconManager.DeviceType currentDevice)
    {
        if (HideOnController && currentDevice != InputIconManager.DeviceType.KeyboardMouse && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else if (HideOnController && currentDevice == InputIconManager.DeviceType.KeyboardMouse && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        switch (currentDevice)
        {
            case InputIconManager.DeviceType.KeyboardMouse:
                iconImage.sprite = iconSet.keyboardSprite;
                break;
            case InputIconManager.DeviceType.Xbox:
                iconImage.sprite = iconSet.xboxSprite;
                break;
            case InputIconManager.DeviceType.PlayStation:
                iconImage.sprite = iconSet.playstationSprite;
                break;
            case InputIconManager.DeviceType.GenericGamepad:
                iconImage.sprite = iconSet.xboxSprite;
                break;
        }
    }

    public void OnChangeIconSet(InputIconSet set)
    {
        iconSet = set;
        UpdateIcon(InputIconManager.instance.CurrentDevice);
    }

}
