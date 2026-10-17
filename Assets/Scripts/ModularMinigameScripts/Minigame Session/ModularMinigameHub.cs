using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;


public class ModularMinigameHub : MonoBehaviour
{
    public static ModularMinigameHub instance { get; private set; }
    [SerializeField] private string hubSceneName;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;


    [SerializeField] private float itemDelay = 0.2f;

    [SerializeField] private MinigameRecipe loadedRecipe;

    private GameObject centerItem;
    private GameObject leftItem;
    private GameObject rightItem;

    private MinigameSession activeSession;
    private int currentIndex;
    private bool awaitingCompletion;
    private bool isLoading;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    //Load the minigame session
    public void StartSession()
    {
        MinigameSession session = loadedRecipe.session;

        if (session == null || session.minigames.Count == 0)
        {
            Debug.LogError($"Session is empty or missing");
            return;
        }

        if (isLoading || awaitingCompletion)
        {
            Debug.LogWarning($"A minigame session is already running");
            return;
        }

        activeSession = session;
        currentIndex = 0;

        StartCoroutine(LoadCurrentMinigame());
    }

    private IEnumerator LoadCurrentMinigame()
    {
        if (activeSession == null || currentIndex >= activeSession.minigames.Count)
        {
            activeSession = null;

            yield return SceneManager.LoadSceneAsync(hubSceneName);
            yield break;
        }


        //Get the entry from the active session and index
        MinigameSessionEntry entry = activeSession.minigames[currentIndex];

        if (entry == null || entry.minigame == null)
        {
            Debug.LogError(
                $"Missing minigame at session index {currentIndex}."
            );
            activeSession = null;
            yield return SceneManager.LoadSceneAsync(hubSceneName);
            yield break;
        }

        MinigameDefinition definition = entry.minigame;
        MinigameConfig config = entry.configOverride != null ? entry.configOverride : definition.defaultConfig;

        if (config == null)
        {
            Debug.LogError(
                $"No configuration assigned for {definition.name}."
            );
            activeSession = null;
            yield return SceneManager.LoadSceneAsync(hubSceneName);
            yield break;
        }

        isLoading = true;
        awaitingCompletion = false;

        yield return SceneManager.LoadSceneAsync(definition.sceneName);
        // Allow the new scene's objects to initialize.
        yield return null;

        MonoBehaviour[] behaviours = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true)).ToArray();

        IMinigame minigame = behaviours.OfType<IMinigame>().FirstOrDefault();

        if (minigame == null)
        {
            Debug.LogError(
               $"No IMinigame implementation found in " +
               $"{definition.sceneName}."
           );

            isLoading = false;
            activeSession = null;
            yield return SceneManager.LoadSceneAsync(hubSceneName);
            yield break;
        }

        isLoading = false;
        awaitingCompletion = true;

        minigame.Initialize(config, CompleteCurrentMinigame);


    }

    private void CompleteCurrentMinigame()
    {
        // Prevent multiple completion callbacks.
        if (!awaitingCompletion)
            return;

        awaitingCompletion = false;
        currentIndex++;

        StartCoroutine(LoadCurrentMinigame());
    }



    //Set up the table visually
    public void SetUpTable(MinigameRecipe recipe)
    {
        if (recipe == null) return;
        loadedRecipe = recipe;
        StartCoroutine(SetUpTableRoutine());
    }

    private IEnumerator SetUpTableRoutine()
    {
        //Clear any existing items on the table
        if (centerItem || leftItem || rightItem)
        {
            Destroy(centerItem);
            Destroy(leftItem);
            Destroy(rightItem);
        }

        //Instantiate the table item
        centerItem = Instantiate(loadedRecipe.centerItem, centerPoint);
        //Center item and set scale
        centerItem.transform.localPosition = Vector3.zero + new Vector3(0f, loadedRecipe.centerItemHeightOffset, 0f);
        centerItem.transform.localScale = loadedRecipe.centerItemScale;
        centerItem.transform.localRotation = loadedRecipe.centerItemRotation;
        //Add tweener component to animate them falling in
        centerItem.AddComponent<ObjectInOutTweener>();

        //Wait for delay to reveal all items in sequence
        yield return new WaitForSeconds(itemDelay);

        leftItem = Instantiate(loadedRecipe.leftItem, leftPoint);
        leftItem.transform.localPosition = Vector3.zero + new Vector3(0f, loadedRecipe.leftItemHeightOffset, 0f);
        leftItem.transform.localScale = loadedRecipe.leftItemScale;
        leftItem.transform.localRotation = loadedRecipe.leftItemRotation;
        leftItem.AddComponent<ObjectInOutTweener>();

        yield return new WaitForSeconds(itemDelay);

        rightItem = Instantiate(loadedRecipe.rightItem, rightPoint);
        rightItem.transform.localPosition = Vector3.zero + new Vector3(0f, loadedRecipe.rightItemHeightOffset, 0f);
        rightItem.transform.localScale = loadedRecipe.rightItemScale;
        rightItem.transform.rotation = loadedRecipe.rightItemRotation;
        rightItem.AddComponent<ObjectInOutTweener>();

        yield return new WaitForSeconds(2f);
        StartSession();
    }
}
