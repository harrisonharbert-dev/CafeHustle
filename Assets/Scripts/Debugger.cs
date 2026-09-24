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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        }
        if (DebuggerEnabled)
        {
            DebuggerToggle();
        }
    }


    void DebuggerToggle()
    {
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
        }
    }

    public void RestartGame()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("MainScene");
    }
}

