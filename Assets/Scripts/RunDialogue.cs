using UnityEngine;
using Yarn.Unity;

public class RunDialogue : MonoBehaviour
{
    private DialogueRunner dialogueRunner;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueRunner = GetComponent<DialogueRunner>();
    }

    public void onDialogue(string name)
    {
        dialogueRunner.StartDialogue(name);
    }
}
