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
        if (!isMoving)
        {
            if (Input.GetKey(KeyCode.A))
            {
                // Switch to the first camera
                Cameras[0].SetActive(true);
                Cameras[1].SetActive(false);
                StartCoroutine(CameraMoving());
            }
            else if (Input.GetKey(KeyCode.D))
            {
                // Switch to the second camera
                Cameras[0].SetActive(false);
                Cameras[1].SetActive(true);
                StartCoroutine(CameraMoving());
            }
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    IEnumerator CameraMoving()
    {
        isMoving = true;
        yield return new WaitForSeconds(0.1f);
        isMoving = false;
    }
}
