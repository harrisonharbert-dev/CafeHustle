using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
public class Clickable : MonoBehaviour
{
    public UnityEvent Action;
    private TextMeshProUGUI Description;
    public string DescriptionText;
    private bool Hovered;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Description = GameObject.Find("Description").GetComponent<TextMeshProUGUI>();
        Description.gameObject.SetActive(false);
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
      
        
        Description.transform.position = Input.mousePosition;
        Description.gameObject.SetActive(true);
        Description.text = DescriptionText;
    }

    void OnMouseExit()
    {
   
        Description.text = "";
        Description.gameObject.SetActive(false);
    }
}
