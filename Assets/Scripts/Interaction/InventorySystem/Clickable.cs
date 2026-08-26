using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
public class Clickable : MonoBehaviour
{
    public UnityEvent Action;
    [SerializeField] private TextMeshProUGUI Description;
    [SerializeField] public string DescriptionText;
    [SerializeField] private bool HasText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (HasText)
        {
            Description = GameObject.Find("Description").GetComponent<TextMeshProUGUI>();
            Description.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        
            Action.Invoke();
           Debug.Log("The click event was invoked on: " + this);
        
    }

    void OnMouseOver()
    {
        if (HasText)
        {
            Description.transform.position = Input.mousePosition;
            Description.gameObject.SetActive(true);
            Description.text = DescriptionText;
        }
    }
    void OnMouseExit()
    {
        if (HasText)
        {
            Description.text = "";
            Description.gameObject.SetActive(false);
        }
    }
}
