using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Debugger : MonoBehaviour
{
    public bool DebuggerEnabled;
    public GameObject DebuggerUI;
    public UnityEvent[] DebuggerEvents;
    public float pollingTime = 1.0f;
    private float time;
    private int frameCount;
    public TextMeshProUGUI frameDisplay;


    //Commands

    private Dictionary<string, Action> simpleCommands = new Dictionary<string, Action>();
    private Dictionary<string, Action<string>> parameterizedCommands = new Dictionary<string, Action<string>>();

    [SerializeField] private TMP_InputField inputField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupCommands();
        DebuggerEnabled = false;
        DebuggerUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.LeftShift))
        {
            DebuggerUI.SetActive(true);
            DebuggerEnabled = true;

            PlayerInputController.instance.SetMovementLock(true);
        }
        if (DebuggerEnabled)
        {
            DebuggerToggle();
        }
    }


    void DebuggerToggle()
    {
        //Debug Console text
        inputField.ActivateInputField(); // Auto-focus text box when opened


        //FPS Counter
        time += Time.deltaTime;
        frameCount++;

        if (time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            frameDisplay.text = "FPS: " + frameRate.ToString();

            time -= pollingTime;
            frameCount = 0;
        }
        // Check for key presses and invoke corresponding events
        if (Input.GetKey(KeyCode.Keypad1))
        {
            DebuggerEvents[0].Invoke();
        }
        if (Input.GetKey(KeyCode.Keypad2))
        {
            DebuggerEvents[1].Invoke();
        }
        if (Input.GetKey(KeyCode.Keypad3))
        {
            DebuggerEvents[2].Invoke();



        }
        if (Input.GetKey(KeyCode.Keypad4))
        {
            DebuggerEnabled = false;
            DebuggerUI.SetActive(false);

            PlayerInputController.instance.SetMovementLock(false);
        }
    }

     private void SetupCommands()
    {

        // 2. Commands with Parameters
        parameterizedCommands.Add("skip_story", SkipStory);
    }

    void SkipStory(string name)
    {
        StoryManager.instance.SkipStoryPoint(name);
    }

        public void ProcessCommand()
    {
        string rawInput = inputField.text;
        
        if (string.IsNullOrWhiteSpace(rawInput)) return;

        // Clean input and split by spaces (e.g., "set_speed 15" -> ["set_speed", "15"])
        string[] splitInput = rawInput.Trim().Split(' ');
        string commandName = splitInput[0].ToLower();

        if (simpleCommands.ContainsKey(commandName))
        {
            simpleCommands[commandName].Invoke();
        }
        else if (parameterizedCommands.ContainsKey(commandName))
        {
            // Pass the rest of the string payload as the argument
            string argument = splitInput.Length > 1 ? splitInput[1] : "";
            parameterizedCommands[commandName].Invoke(argument);
        }
        else
        {
            Debug.LogWarning($"Unknown command: {commandName}");
        }

        // Clear and refocus input fields for consecutive typing
        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void RestartGame()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("MainScene");
    }
}

