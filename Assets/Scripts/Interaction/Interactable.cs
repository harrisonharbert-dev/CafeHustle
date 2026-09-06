using KinematicCharacterController;
using KinematicCharacterController.Walkthrough.AddingImpulses;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Yarn.Unity;
using Yarn.Unity.Attributes;


public class Interactable : MonoBehaviour
{
    public enum interactableType
    {
        none,
        interactableWithTrigger,
        interactableWithInput,

    }
    public interactableType interactType;
    [SerializeField] private bool playOnce;

    private bool played;

    [HideInInspector] public bool isInRange = false; // Is the player in range to interact with this object? // Has the player already interacted with this object?
    [Header("Unity Events")]
    [Space(15)]
    public UnityEvent interactAction;
    private DialogueRunner dialogueRunner;

    public enum dialogueType
    {
        none,
        dialogue,
        dialogueFocusedCamera
    }

    [Header("Dialogue")]
    [Space(15)]


    [SerializeField] private dialogueType dialogueOption;

    [HideInInspector] public bool useDialogue;



    [HideInInspector] public bool useDialogueCamera;


    [SerializeField] private string dialogueName;

    [Header("Interaction Prompt")]
    [SerializeField] private InteractPrompt3D prompt;
    [SerializeField] private bool isInteractable = true;
    [SerializeField] private interactableZoneIndicator zoneIndicator;


    public void Start()
    {
        dialogueRunner = GameObject.FindGameObjectWithTag("DialogueRunner").GetComponent<DialogueRunner>();

        switch (dialogueOption)
        {
            case dialogueType.none:
                useDialogue = false;
                useDialogueCamera = false;
                break;


            case dialogueType.dialogue:
                useDialogue = true;
                useDialogueCamera = false;
                break;

            case dialogueType.dialogueFocusedCamera:
                useDialogue = true;
                useDialogueCamera = true;
                break;
        }

        if (zoneIndicator != null && isInteractable)
        {
            zoneIndicator.changeIndicatorVisibility(true);
        }

        if (prompt == null)
        {
            Debug.LogWarning($"[Interactable] No interact prompt UI on {this}");
        }
    }

    public void setInteractable(bool option)
    {
        Debug.Log($"[{this}] interactable set to: {option}");
        isInteractable = option;
        if(option== false)
        {
            PlayerInputController.instance.SetCurrentInteractable(null);
        }
        if (zoneIndicator != null) {
        zoneIndicator.changeIndicatorVisibility(option);
        }
    }


    public void InvokeEvent()
    {
        
        if(isInteractable==false) return;
        if (interactType == interactableType.none && zoneIndicator != null)
        {
            zoneIndicator.changeIndicatorVisibility(false);
            return;
        }
        ;
        //
        played = true;

        if (prompt != null)
        {
            prompt.onUI(false);
        }

        if (playOnce)
        {
            setInteractable(false);
        }



        if (dialogueName != null)
        {
            dialogueRunner.StartDialogue(dialogueName);

            if (useDialogueCamera)
            {
                PlayerInputController.instance.onDialogueCamera(gameObject);
            }
        }
        if (interactAction != null)
        {
            interactAction.Invoke();
        }

    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isInteractable) return;
        if (other.gameObject.CompareTag("Player"))
        {
            if (played && playOnce) return;

            isInRange = true;


            switch (interactType)
            {
                case interactableType.interactableWithTrigger:
                    InvokeEvent();
                    break;

                case interactableType.interactableWithInput:
                    PlayerInputController.instance.SetCurrentInteractable(this);
                    if (prompt != null)
                    {
                        prompt.onUI(true);
                    }
                    break;
            }


        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!isInteractable) return;
        if (other.gameObject.CompareTag("Player") && PlayerInputController.instance.currentInteractable == this)
        {
            isInRange = false;
            PlayerInputController.instance.SetCurrentInteractable(null);
            if (prompt != null)
            {
                prompt.onUI(false);
            }
        }

    }


}
