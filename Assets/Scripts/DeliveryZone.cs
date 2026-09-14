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
        if (!interactable) return;
        if (other.CompareTag("Player") && PlayerInputController.instance.currentCarryItemID == requiredItemID)
        {
            PlayerInputController.instance.inCarryDeliveryZone = true;
            PlayerInputController.instance.deliveryZonePos = gameObject;
            InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.Deliver, Interactable.PromptKey.F);
            InteractPrompt.instance.Refresh();

            prompt.onUI(true);

        }
    }


    void OnTriggerExit(Collider other)
    {
        if (!interactable) return;
        if (other.CompareTag("Player"))
        {
            PlayerInputController.instance.inCarryDeliveryZone = false;
            InteractPrompt.instance.Refresh();
            switch (PlayerInputController.instance.playerState)
            {
                case PlayerInputController.playState.carryingObject:
                    InteractPrompt.instance.UpdateUIInfo(Interactable.PromptText.Drop, Interactable.PromptKey.F);
                    break;

            }
            prompt.onUI(false);
        }
    }
}
