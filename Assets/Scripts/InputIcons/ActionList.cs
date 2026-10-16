using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ActionList : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float pollInterval = 0.1f;

    [SerializeField] private GameObject moveAction;
    [SerializeField] private GameObject sprintAction;
    [SerializeField] private GameObject interactAction;
    [SerializeField] private GameObject deliverAction;
    [SerializeField] private GameObject pickUpAction;
    [SerializeField] private GameObject dropAction;
    [SerializeField] private GameObject cancelAction;
    [SerializeField] private GameObject continueAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created'
    void Start()
    {
        StateChanged();
        StartCoroutine(PollState());

    }

    private IEnumerator PollState()
    {
        while (true)
        {
            StateChanged();
            yield return new WaitForSecondsRealtime(pollInterval);
        }
    }

    private void StateChanged()
    {
        //Get all player states
        PlayerInputController.playState playerState = PlayerInputController.instance.playerState;
        bool currentInteractable = PlayerInputController.instance.currentInteractable != null;
        bool currentCarryObject = PlayerInputController.instance.currentCarryObject != null;
        bool isInDialogue = PlayerInputController.instance.isinDialogue;
        bool isViewingModel = PlayerInputController.instance.isViewingModel;
        bool isInDeliveryZone = PlayerInputController.instance.inCarryDeliveryZone;
        bool isLockedMovement = PlayerInputController.instance.lockMovement;



        //Hide or show movement controls if locked movement
        if (isLockedMovement == true)
        {
            moveAction.SetActive(false);
            sprintAction.SetActive(false);
        }
        else if (isLockedMovement == false)
        {
            moveAction.SetActive(true);
            sprintAction.SetActive(true);
        }

        //If is in delivery zone
        if (isInDeliveryZone == true)
        {
            deliverAction.SetActive(true);
        }
        else if (isInDeliveryZone == false)
        {
            deliverAction.SetActive(false);
        }

        //Check dialogue or viewing model

        if (isInDialogue && !isViewingModel)
        {
            cancelAction.SetActive(true);
            continueAction.SetActive(true);

            interactAction.SetActive(false);
            dropAction.SetActive(false);
        }
        else if (!isInDialogue && isViewingModel)
        {
            cancelAction.SetActive(true);
            continueAction.SetActive(false);
            interactAction.SetActive(false);
            dropAction.SetActive(false);
        }
        else if (!isInDialogue && !isViewingModel)
        {
            cancelAction.SetActive(false);
            continueAction.SetActive(false);
        }

        switch (playerState)
        {
            //If player is carrying nothing
            case PlayerInputController.playState.none:
                //If there is a current carryObject set pick up action
                if (currentCarryObject)
                {
                    pickUpAction.SetActive(true);
                }
                else
                {
                    pickUpAction.SetActive(false);
                }


                //Clear any drop action if player is not carrying anything
                dropAction.SetActive(false);
                break;
            case PlayerInputController.playState.carryingObject:
                dropAction.SetActive(true);
                pickUpAction.SetActive(false);
                break;

            case PlayerInputController.playState.carryingNonDroppable:
                pickUpAction.SetActive(false);
                dropAction.SetActive(false);
                break;
        }

        if (currentInteractable == true && !isInDialogue && !isViewingModel)
        {
            interactAction.SetActive(true);
        }
        else if (currentInteractable == false)
        {
            interactAction.SetActive(false);
        }

    }
}
