using System.Runtime.CompilerServices;
using CsvHelper.Configuration.Attributes;
using Unity.VisualScripting;
using UnityEngine;


public class DeliveryZone : MonoBehaviour
{


    [Header("Item Requirements")]
    [SerializeField] private string requiredItemID;
    [SerializeField] private bool interactable;

    [Header("Interaction Prompt")]
    [SerializeField] private InteractPrompt3D prompt;


    public void onSetInteractable(bool option)
    {
        interactable = option;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter(Collider other)
    {
        if(!interactable) return;
        if (other.CompareTag("Player") && PlayerInputController.instance.currentCarryItemID == requiredItemID)
        {
            Debug.Log("Entered Delivery Zone");
            PlayerInputController.instance.inCarryDeliveryZone = true;
            PlayerInputController.instance.deliveryZonePos = gameObject;

            prompt.onUI(true);

        }
    }


    void OnTriggerExit(Collider other)
    {
        if(!interactable) return;
        if (other.CompareTag("Player"))
        {
            PlayerInputController.instance.inCarryDeliveryZone = false;
            prompt.onUI(false);
        }
    }
}
