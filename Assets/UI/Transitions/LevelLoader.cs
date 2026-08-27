using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator animator;
    private float transitionTime = 1f;

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

        // Play transition out
        if (animator != null &&
            (transitionType == TransitionType.OutOnly ||
             transitionType == TransitionType.InAndOut))
        {
            animator.ResetTrigger("playStart");
            animator.SetTrigger("playEnd");

            yield return new WaitForSeconds(transitionTime);
        }

        // Remember the current scene
        Scene previousScene = SceneManager.GetActiveScene();

        // Load the new scene additively
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive
        );

        yield return loadOperation;

        // Find the newly loaded scene
        Scene newScene = GetNewestScene(sceneName);

        if (!newScene.IsValid())
        {
            Debug.LogError("Could not find loaded scene: " + sceneName);

            isLoading = false;
            yield break;
        }

        // Disable the previous scene
        SetSceneObjectsActive(previousScene, false);

        // Enable the new scene
        SetSceneObjectsActive(newScene, true);

        // Make the new scene active
        SceneManager.SetActiveScene(newScene);

        // Reset transition
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

        // Play transition out
        if (animator != null &&
            (transitionType == TransitionType.OutOnly ||
             transitionType == TransitionType.InAndOut))
        {
            animator.ResetTrigger("playStart");
            animator.SetTrigger("playEnd");

            yield return new WaitForSeconds(transitionTime);
        }

        // Find the minigame
        Scene minigameScene = SceneManager.GetActiveScene();

        if (!minigameScene.IsValid())
        {
            Debug.LogError("Could not find current minigame scene.");

            isLoading = false;
            yield break;
        }

        // Find StartingScene
        Scene startingScene = SceneManager.GetSceneByName("prototype_environment");

        if (!startingScene.IsValid())
        {
            Debug.LogError("Could not find StartingScene.");

            isLoading = false;
            yield break;
        }

        // Enable StartingScene
        SetSceneObjectsActive(startingScene, true);

        // Make StartingScene active
        SceneManager.SetActiveScene(startingScene);

        // Completely unload the minigame
        AsyncOperation unloadOperation =
            SceneManager.UnloadSceneAsync(minigameScene);

        yield return unloadOperation;

        Debug.Log("Destroyed minigame scene: " + sceneName);

        // Reset transition
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

        // Remember the OLD scene
        Scene oldScene = SceneManager.GetActiveScene();

        string sceneName = oldScene.name;

        Debug.Log("Restarting scene: " + sceneName);

        // Play transition out
        if (animator != null &&
            (transitionType == TransitionType.OutOnly ||
             transitionType == TransitionType.InAndOut))
        {
            animator.ResetTrigger("playStart");
            animator.SetTrigger("playEnd");

            yield return new WaitForSeconds(transitionTime);
        }

        // =====================================================
        // LOAD A BRAND NEW COPY FIRST
        // =====================================================

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Additive
        );

        yield return loadOperation;

        // Get the newly loaded copy
        Scene newScene = GetNewestScene(sceneName, oldScene);

        if (!newScene.IsValid())
        {
            Debug.LogError("Could not find newly loaded scene: " + sceneName);

            isLoading = false;
            yield break;
        }

        // =====================================================
        // ENABLE THE NEW SCENE
        // =====================================================

        SetSceneObjectsActive(newScene, true);

        // Make the NEW scene active
        SceneManager.SetActiveScene(newScene);

        Debug.Log("New scene loaded: " + sceneName);

        // =====================================================
        // NOW DESTROY THE OLD SCENE
        // =====================================================

        AsyncOperation unloadOperation =
            SceneManager.UnloadSceneAsync(oldScene);

        yield return unloadOperation;

        Debug.Log("Old scene destroyed: " + sceneName);

        // =====================================================
        // RESET TRANSITION
        // =====================================================

        ResetTransition();

        isLoading = false;

        Debug.Log("Scene restarted successfully: " + sceneName);
    }

    // =========================================================
    // FIND NEWEST COPY OF A SCENE
    // =========================================================

    private Scene GetNewestScene(string sceneName)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (scene.name == sceneName &&
                scene.isLoaded)
            {
                return scene;
            }
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
    // ENABLE / DISABLE SCENE ROOT OBJECTS
    // =========================================================

    private void SetSceneObjectsActive(Scene scene, bool active)
    {
        if (!scene.IsValid())
            return;

        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject obj in rootObjects)
        {
            obj.SetActive(active);
        }
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