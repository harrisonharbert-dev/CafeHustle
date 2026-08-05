using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public GameObject[] Cameras;
    public static bool isMoving;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //RestartScene
        if (Input.GetKeyDown(KeyCode.R))
        {
            
          LoadScene("CookingTest");
        }

    }
    public void LoadScene(string sceneName)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(sceneName);
    }
    public void NextStage(int stageIndex)
    {
        Debug.Log("NextStage called with index: " + stageIndex);
        Cameras[stageIndex - 1].SetActive(false);
        Cameras[stageIndex].SetActive(true);
    }
}
