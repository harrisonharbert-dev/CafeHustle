using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Antlr4.Runtime.Atn;

public class LevelLoader : MonoBehaviour
{
    public Animator animator;
    private float transitionTime = 2f;

    [Header("Scene Root")]
    [Tooltip("Name of the parent object containing everything in each scene.")]
    public string sceneRootName = "SceneRoot";

    public enum TransitionType
    {
        None,
        InOnly,
        OutOnly,
        InAndOut
    }

    public TransitionType transitionType;

    private bool isLoading = false;

    private void OnEnable()
    {
        ResetTransition();
    }

    private void Start()
    {
        if (animator != null)
        {
            animator.ResetTrigger("playEnd");

            if (transitionType == TransitionType.InOnly ||
                transitionType == TransitionType.InAndOut)
            {
                animator.SetTrigger("playStart");
            }
        }
    }
    //Load scene
    public void LoadNamedNonAdditiveScene(string sceneName)
    {
        StartCoroutine(LoadNonAdditiveScene(sceneName));
    }
    private IEnumerator LoadNonAdditiveScene(string sceneName)
    {
        //
        if (transitionType == TransitionType.OutOnly || transitionType == TransitionType.InAndOut)
        {
            animator.SetTrigger("playEnd");

            yield return new WaitForSeconds(transitionTime);

            SceneManager.LoadScene(sceneName);

        }
    }

    // =========================================================
    // LOAD A MINIGAME ADDITIVELY
    // =========================================================

    public void LoadNamedScene(string sceneName)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadScene(sceneName));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        isLoading = true;

        if (animator != null &&
            (transitionType == TransitionType.OutOnly ||
             transitionType == TransitionType.InAndOut))
        {
            animator.ResetTrigger("playStart");
            animator.SetTrigger("playEnd");

            yield return new WaitForSeconds(transitionTime);
        }

        Scene previousScene = SceneManager.GetActiveScene();

        // Load new scene
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive
        );

        yield return loadOperation;

        Scene newScene = GetNewestScene(sceneName);

        if (!newScene.IsValid())
        {
            Debug.LogError("Could not find loaded scene: " + sceneName);
            isLoading = false;
            yield break;
        }

        // Disable ONLY the parent of the previous scene
        SetSceneRootActive(previousScene, false);

        // Enable ONLY the parent of the new scene
        SetSceneRootActive(newScene, true);

        SceneManager.SetActiveScene(newScene);

        ResetTransition();

        isLoading = false;

        Debug.Log("Loaded scene: " + sceneName);
    }

    // =========================================================
    // RETURN TO STARTING SCENE
    // =========================================================

    public void UnloadNamedScene(string sceneName)
    {
        if (isLoading)
            return;

        StartCoroutine(UnloadScene(sceneName));
    }

    private IEnumerator UnloadScene(string sceneName)
    {
        isLoading = true;

        if (animator != null &&
            (transitionType == TransitionType.OutOnly ||
             transitionType == TransitionType.InAndOut))
        {
            animator.ResetTrigger("playStart");
            animator.SetTrigger("playEnd");

            yield return new WaitForSeconds(transitionTime);
        }

        Scene minigameScene = SceneManager.GetActiveScene();

        if (!minigameScene.IsValid())
        {
            Debug.LogError("Could not find current minigame scene.");
            isLoading = false;
            yield break;
        }

        Scene startingScene =
            SceneManager.GetSceneByName("prototype_environment");

        if (!startingScene.IsValid())
        {
            Debug.LogError("Could not find StartingScene.");
            isLoading = false;
            yield break;
        }

        // Enable the starting scene parent
        SetSceneRootActive(startingScene, true);

        SceneManager.SetActiveScene(startingScene);

        // Destroy minigame
        AsyncOperation unloadOperation =
            SceneManager.UnloadSceneAsync(minigameScene);

        yield return unloadOperation;

        Debug.Log("Destroyed minigame scene: " + sceneName);

        ResetTransition();

        isLoading = false;
    }

    // =========================================================
    // RESTART CURRENT SCENE
    // =========================================================

    public void RestartScene()
    {
        if (isLoading)
            return;

        StartCoroutine(RestartCurrentScene());
    }

    private IEnumerator RestartCurrentScene()
    {
        isLoading = true;

        Scene oldScene = SceneManager.GetActiveScene();

        string sceneName = oldScene.name;

        Debug.Log("Restarting scene: " + sceneName);

        if (animator != null &&
            (transitionType == TransitionType.OutOnly ||
             transitionType == TransitionType.InAndOut))
        {
            animator.ResetTrigger("playStart");
            animator.SetTrigger("playEnd");

            yield return new WaitForSeconds(transitionTime);
        }

        // Load fresh copy
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive
        );

        yield return loadOperation;

        Scene newScene = GetNewestScene(sceneName, oldScene);

        if (!newScene.IsValid())
        {
            Debug.LogError("Could not find newly loaded scene: " + sceneName);
            isLoading = false;
            yield break;
        }

        // Enable only the new scene parent
        SetSceneRootActive(newScene, true);

        SceneManager.SetActiveScene(newScene);

        // Destroy old copy
        AsyncOperation unloadOperation =
            SceneManager.UnloadSceneAsync(oldScene);

        yield return unloadOperation;

        ResetTransition();

        isLoading = false;

        Debug.Log("Scene restarted successfully: " + sceneName);
    }

    // =========================================================
    // FIND NEWEST SCENE
    // =========================================================

    private Scene GetNewestScene(string sceneName)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (scene.name == sceneName && scene.isLoaded)
                return scene;
        }

        return default;
    }

    private Scene GetNewestScene(string sceneName, Scene oldScene)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (scene.name == sceneName &&
                scene.isLoaded &&
                scene.handle != oldScene.handle)
            {
                return scene;
            }
        }

        return default;
    }

    // =========================================================
    // ENABLE / DISABLE SCENE PARENT
    // =========================================================

    private void SetSceneRootActive(Scene scene, bool active)
    {
        if (!scene.IsValid())
            return;

        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject obj in rootObjects)
        {
            if (obj.name == sceneRootName)
            {
                obj.SetActive(active);
                return;
            }
        }

        Debug.LogWarning(
            "Could not find '" + sceneRootName +
            "' in scene '" + scene.name + "'."
        );
    }

    // =========================================================
    // RESET TRANSITION
    // =========================================================

    private void ResetTransition()
    {
        if (animator == null)
            return;

        animator.ResetTrigger("playEnd");
        animator.ResetTrigger("playStart");

        if (transitionType == TransitionType.InOnly ||
            transitionType == TransitionType.InAndOut)
        {
            animator.SetTrigger("playStart");
        }
    }
}