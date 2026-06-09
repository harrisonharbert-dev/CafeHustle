using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject[] Cameras;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            // Switch to the first camera
            Cameras[0].SetActive(true);
            Cameras[1].SetActive(false);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // Switch to the second camera
            Cameras[0].SetActive(false);
            Cameras[1].SetActive(true);
        }
    }
}
