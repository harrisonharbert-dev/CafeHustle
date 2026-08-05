using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Stove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }                        
    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
        if (collision.gameObject.tag == "Ingredient")
        {
            Debug.Log("Collided with ingredient: " + collision.gameObject.name);
            if (collision.gameObject.GetComponent<DraggingScript>() != null && collision.gameObject.GetComponent<DraggingScript>().isFood == true)
            {
                Debug.Log("Starting to cook: " + collision.gameObject.name);
                collision.gameObject.GetComponent<FoodStats>().StartCooking();
            }
        }
    }

    private void OnCollisionExit(UnityEngine.Collision collision)
        {
        Debug.Log("Stopping cooking: " + collision.gameObject.name);
            if (collision.gameObject.tag == "Ingredient")
            {
            if (collision.gameObject.GetComponent<DraggingScript>() != null && collision.gameObject.GetComponent<DraggingScript>().isFood == true)
            {          
                collision.gameObject.GetComponent<FoodStats>().StopCooking();
            }
            }   
        }
}
