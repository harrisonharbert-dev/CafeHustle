using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.Events;
using Yarn.Unity;


public class ModelViewerHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject viewerContainer;
    [SerializeField] private List<ViewModel> availableModels;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image bannerImage;
    [SerializeField] private Transform prefabSpawnLocation;

    private ViewModel currentModel;
    private GameObject modelInstance;
    private bool isViewerOpen = false;

    [System.Serializable]
    public class colors
    {
        public Sprite pink;
        public Sprite blue;
        public Sprite yellow;
        public Sprite green;
    }

    public colors bannerColors;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onViewEvent;
    [SerializeField] private UnityEvent onHideEvent;

    void Start()
    {
        if (viewerContainer != null)
        {
            viewerContainer.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Missing viewer container on {this}");
        }
    }

    void GetBannerColor()
    {
        //Check if variables exist
        if (bannerColors == null || currentModel == null)
        {
            Debug.LogWarning($"Could not get banner color because current model is null on {this}");
            return;
        }

        //Set banner image based on switch
        switch (currentModel.titleBannerColor)
        {
            case ViewModel.colors.pink:
                bannerImage.sprite = bannerColors.pink;
                break;

            case ViewModel.colors.green:
                bannerImage.sprite = bannerColors.green;
                break;

            case ViewModel.colors.blue:
                bannerImage.sprite = bannerColors.blue;
                break;

            case ViewModel.colors.yellow:
                bannerImage.sprite = bannerColors.yellow;
                break;
        }
    }


    bool LoadProfile(string name)
    {
        //Find matching id
        var match = availableModels?.Find(p => p != null && p.id == name);
        if (match == null)
        {
            Debug.LogWarning($"No model with id of {name} on {this}");
            return false;
        }

        //Set Current Model
        currentModel = match;
        return true;
    }

    [YarnCommand("view_model")]
    public void OpenViewModel(string name)
    {
        //Attempt to load profile, return if false
        if (LoadProfile(name) == false)
        {
            return;
        }
        
        //Destroy any pre-existing model instances
        if(modelInstance != null)
        {
            Destroy(modelInstance);
        }

        //Run on Open event only when its opening from hidden state
        if (isViewerOpen == false)
        {
            onViewEvent?.Invoke();
        }

        GetBannerColor();

        //Set text
        titleText.text = currentModel.title;
        descriptionText.text = currentModel.description;

        //Spawn, center model and rotate.
        modelInstance = Instantiate(currentModel.prefab, prefabSpawnLocation);
        modelInstance.transform.localScale = currentModel.prefabScale;
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.rotation = Quaternion.Euler(-45f, 0f, 0f);
        int uiLayer = LayerMask.NameToLayer("UI");
        foreach (Transform child in modelInstance.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = uiLayer;
        }

        //Show
        StartCoroutine(WaitForInput());
        viewerContainer.SetActive(true);
        isViewerOpen = true;

    }

    private void FixedUpdate()
    {
        if (isViewerOpen && modelInstance != null)
        {
            modelInstance.transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.World);
        }
    }

    public void CloseViewModel()
    {
        Destroy(modelInstance);
        viewerContainer.SetActive(false);

        StopCoroutine(WaitForInput());

        onHideEvent?.Invoke();
        isViewerOpen = false;
    }


    IEnumerator WaitForInput()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Escape));

        CloseViewModel();
    }

}
