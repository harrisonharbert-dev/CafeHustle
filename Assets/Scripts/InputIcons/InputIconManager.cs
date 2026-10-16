using UnityEngine;
using UnityEngine.InputSystem;

public class InputIconManager : MonoBehaviour
{
    public static InputIconManager instance { get; private set; }

    public enum DeviceType
    {
        KeyboardMouse,
        Xbox,
        PlayStation,
        GenericGamepad
    }
    public DeviceType CurrentDevice { get; private set; }

    public delegate void OnDeviceChanged(DeviceType newDevice);
    public event OnDeviceChanged onDeviceChanged;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionStarted)
        {
            InputAction action = (InputAction)obj;
            if(action.activeControl != null)
            {
                DetectDevice(action.activeControl.device);
            }
        }
    }

    private void DetectDevice(InputDevice device)
    {
        DeviceType detectedType = DeviceType.KeyboardMouse;

        if(device is Gamepad)
        {
            string layoutName = device.layout.ToLower();

            if(layoutName.Contains("dualshock") || layoutName.Contains("ps") || layoutName.Contains("playstation"))
            {
                detectedType = DeviceType.PlayStation;
            }
            else if (layoutName.Contains("xbox") || layoutName.Contains("xinput"))
            {
                detectedType = DeviceType.Xbox;
            }
            else
            {
                detectedType = DeviceType.GenericGamepad;
            }
        }

        if (detectedType != CurrentDevice)
        {
            CurrentDevice = detectedType;
            onDeviceChanged?.Invoke(CurrentDevice);
        }
    }
}
