using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class DeliveryZone : MonoBehaviour
{


    [Header("Item Requirements")]
    [SerializeField] private string requiredItemID;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && PlayerInputController.instance.currentCarryItemID == requiredItemID)
        {
            Debug.Log("Entered Delivery Zone");
            PlayerInputController.instance.inCarryDeliveryZone = true;
            PlayerInputController.instance.deliveryZonePos = gameObject;
            InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.Deliver, Interactable.PromptKey.F);
            InteractPrompt.instance.Refresh();
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInputController.instance.inCarryDeliveryZone = false;
            InteractPrompt.instance.Refresh();
            switch (PlayerInputController.instance.playerState)
            {
                case PlayerInputController.playState.carryingObject:
                    InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.Drop,Interactable.PromptKey.F);
                    break;

            }
        }
    }
}
