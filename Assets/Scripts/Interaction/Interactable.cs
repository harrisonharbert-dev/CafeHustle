using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Walkthrough.AddingImpulses;
using KinematicCharacterController;


public class Interactable : MonoBehaviour
{
    public bool isInRange = false; // Is the player in range to interact with this object? // Has the player already interacted with this object?
    public UnityEvent interactAction;
    public GameObject UI;

    public void Start()
    {
  
    }

    public void Update()
    {
        if (isInRange)
        {
            UI.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E))
            {
                InvokeEvent();
            }
        }
        if (isInRange == false)
        {
            UI.SetActive(false);
        }
    }
    public void InvokeEvent() 
    {
        interactAction.Invoke(); //Makes unity event happen which is assigned in the inspector
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered");
     
            isInRange = true;
            Debug.Log("Player is in range to interact with " + gameObject.name);
        
    }

    private void OnTriggerExit(Collider other)
    {
        isInRange = false;
        
    }
}
