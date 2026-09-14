using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractPrompt3D : MonoBehaviour
{

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
        if (UIContainer != null)
        {
            foreach (Transform child in UIContainer.transform)
            {
                child.gameObject.SetActive(option);
            }
        }

    }
}
