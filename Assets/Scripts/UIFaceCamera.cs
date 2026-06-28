using UnityEngine;

public class UIFaceCamera : MonoBehaviour
{

   
    [SerializeField] private CanvasGroup ui;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ui == null)
        ui = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (ui != null)
        {
            // Rotate canvas to face the camera
            ui.transform.LookAt(Camera.main.transform);
            ui.transform.Rotate(0, 180, 0); // Fix backwards facing
        }
    }
}
