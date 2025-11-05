using UnityEngine;
using UnityEngine.Events;

public class Collision : MonoBehaviour
{
    public UnityEvent interactAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Detection")
        {
            Debug.Log("Thingy");
        }
        
    }

}
