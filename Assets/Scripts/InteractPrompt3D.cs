using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractPrompt3D : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;
    [SerializeField] private GameObject UIContainer;
    [SerializeField] private CanvasGroup canvasGroup;

    
    [System.Serializable]  enum TextEffects
    {
        None,
        Wavy,
        Sketchy,
        Shaky
    }

    [System.Serializable]
    enum DisplayIcon
    {
        E,
        F
    }

    [SerializeField] private Sprite e;
    [SerializeField] private Sprite f;

    [Header("Customization")]
    [SerializeField] private TextEffects textEffect;
    [SerializeField] private DisplayIcon displayIcon;
     
    void Start()
    {
        onUI(false);
        ApplyTextEffect();
        
        image.sprite = GetIcon(displayIcon);

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private string GetTextEffect(TextEffects textEffect)
    {
        return textEffect switch
        {
            TextEffects.None => null,
            TextEffects.Wavy => "wave",
            TextEffects.Sketchy => "sketchy",
            TextEffects.Shaky => "shake",
            _ => null,
        };
    }

    void ApplyTextEffect()
    {
        if(text==null) return;

        string oldText = text.text;
        string tag = GetTextEffect(textEffect);

        text.text = $"<{tag}>{oldText}</{tag}>";
    }


    private Sprite GetIcon(DisplayIcon displayIcon)
    {
        if (displayIcon == DisplayIcon.E)
        {
            return e;
        }
        else if (displayIcon == DisplayIcon.F)
        {
            return f;
        }
        return null;
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
            foreach(Transform child in UIContainer.transform)
            {
                child.gameObject.SetActive(option);
            }    
        }

    }
}
