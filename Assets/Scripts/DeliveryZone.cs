using System.Runtime.CompilerServices;
using CsvHelper.Configuration.Attributes;
using Unity.VisualScripting;
using UnityEngine;


public class DeliveryZone : MonoBehaviour
{


    [Header("Item Requirements")]
    [SerializeField] private string requiredItemID;

    [Header("Interaction Prompt")]
    [SerializeField] private InteractPrompt3D prompt;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter(Collider other)
    {
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
        if (other.CompareTag("Player"))
        {
            PlayerInputController.instance.inCarryDeliveryZone = false;
            prompt.onUI(false);
        }
    }
}
