using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;    
public class CameraController : MonoBehaviour
{
    public GameObject[] Cameras;
    public static bool isMoving;

    //UIStuff
    public GameObject FoodHotBar;
    public FoodCuttable[] CuttableFoods;
    public static bool transitioning;
    public UnityEvent onStageComplete;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FoodHotBar.SetActive(false);

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
    public void NextStage(int nextCam)
    {
        if (CuttableFoods == null || CuttableFoods.Length == 0)
            return;

        foreach (var food in CuttableFoods)
        {
            if (!food.cutSuccessful)
            {
                // At least one food hasn't been cut yet.
                return;
            }
        }

        // If we got here, every food has been cut successfully.
        onStageComplete?.Invoke();
        StartCoroutine(DelayedStage(nextCam));
    }

    IEnumerator DelayedStage(int stageIndex)
    {
        Debug.Log("RanKnifeDrop");
        KnifeDrawer knifeDrawer = FindAnyObjectByType<KnifeDrawer>();
        knifeDrawer.HandlePickup();
        knifeDrawer.GetComponent<KnifeDrawer>().holdingKnife = false;
        yield return new WaitForSeconds(2f);
        Cameras[stageIndex - 1].SetActive(false);
        Cameras[stageIndex].SetActive(true);
        if (stageIndex == 1)
        {
            FoodHotBar.SetActive(true);
        }
        else
        {
            FoodHotBar.SetActive(false);
        }
    }

}
