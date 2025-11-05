using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    public bool isInRange;
    public UnityEvent interactAction;

    private PlayerMovement playerMovement; //Player movement reference




    void Start()
    {
        //Auto assign player movement by tag
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    /* void Update()
     {
         if (isInRange)
         {
             if (Input.GetKeyUp(KeyCode.E))
             {
                 interactAction.Invoke(); //Makes unity event happen which is assigned in the inspector
             }
         }
     }
    */

    public void InvokeEvent() 
    {
        Debug.Log($"UnityEvent invoked on game object: {this}");


        if (interactAction == null) 
        {
            Debug.LogWarning($"UnityEvent interactAction is not assigned on:{this}");
            return;
        }
        //
        interactAction.Invoke(); //Makes unity event happen which is assigned in the inspector
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerMovement.SetCurrentInteractable(this); // Set current interactable script that player can interact with

            isInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {

            playerMovement.SetCurrentInteractable(null); // Reset current interactable script

            isInRange = false;
        }
    }
}
