using KinematicCharacterController;
using KinematicCharacterController.Walkthrough.AddingImpulses;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Yarn.Unity;


public class Interactable : MonoBehaviour
{
    
    [HideInInspector] public bool isInRange = false; // Is the player in range to interact with this object? // Has the player already interacted with this object?
    [Header("Unity Events")]
    [Space(15)]
    public UnityEvent interactAction;
    private InteractPrompt UI;
    private DialogueRunner dialogueRunner;
    private PlayerInputController player;
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

    

    


    public void Start()
    {
        UI = GameObject.FindGameObjectWithTag("InteractPrompt").GetComponent<InteractPrompt>();
        dialogueRunner = GameObject.FindGameObjectWithTag("DialogueRunner").GetComponent<DialogueRunner>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInputController>();

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
    }




    public void InvokeEvent()
    {

        if (!useDialogue) {
        interactAction.Invoke(); //Makes unity event happen which is assigned in the inspector
        }


        if (useDialogue)
        {
            dialogueRunner.StartDialogue(dialogueName);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) {
            Debug.Log("Trigger Entered");

            isInRange = true;

            Debug.Log("Player is in range to interact with " + gameObject.name);

            PlayerInputController player = other.GetComponent<PlayerInputController>();
            player.SetCurrentInteractable(this);
            UI.SetPromptVisibility(true);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) {
            isInRange = false;
            UI.SetPromptVisibility(false);
        }

    }
}
