using UnityEngine;
using Yarn.Unity;

public class RunDialogue : MonoBehaviour
{
    private DialogueRunner dialogueRunner;
    private InMemoryVariableStorage variableStorage;

    private string targetVariable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (TryGetComponent<DialogueRunner>(out DialogueRunner runnerOutput))
        {
            dialogueRunner = runnerOutput;
        }

        if (TryGetComponent<InMemoryVariableStorage>(out InMemoryVariableStorage storageOutput))
        {
            variableStorage = storageOutput;
        }
    }

    public void onDialogue(string name)
    {
        if (dialogueRunner == null)
        {
            Debug.LogWarning($"DialogueRunner is not assigned on {this}.");
            return;
        }

        dialogueRunner.Stop();
        dialogueRunner.StartDialogue(name);
    }

    public void setVariable(string name)
    {
        targetVariable = name;
    }

    public void setVariableFloat(float value)
    {
        if(targetVariable == null)
        {
            Debug.LogWarning($"No target variable defined when setting variable float on {this}.");
            return;
        }
        variableStorage.SetValue(targetVariable,value);
    }

    
}
