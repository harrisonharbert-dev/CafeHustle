using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Destroys every DontDestroyOnLoad object before restarting, so nothing " +
             "from the previous run (or its destroyed state) carries over.")]
    [SerializeField] private bool destroyPersistentObjects = true;
    [SerializeField] private UnityEvent loadSceneEvent;

    public void MainScene()
    {
        ResetGameState();
        loadSceneEvent?.Invoke();
    }

    // ============================================================
    // RESET
    // ============================================================

    private void ResetGameState()
    {
        // Saved data.
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Time / audio can be left paused by a previous run.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // DOTween tweens survive scene loads and can still be running
        // on objects from the last run.
        DOTween.KillAll();

        if (destroyPersistentObjects)
            DestroyPersistentObjects();

        // Static variables are NOT reset by loading a scene.
        // Reset any of your own here, for example:
        // CameraController.transitioning = false;
        // MyManager.SomeStaticFlag = false;
    }

    // Objects marked DontDestroyOnLoad live in a hidden scene that
    // LoadScene never clears. This finds that scene and removes them.
    private void DestroyPersistentObjects()
    {
        GameObject finder = new GameObject("PersistentObjectFinder");
        DontDestroyOnLoad(finder);

        GameObject[] roots = finder.scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            // Don't destroy the finder, or this menu if it is persistent.
            if (root == finder || transform.IsChildOf(root.transform))
                continue;

            Destroy(root);
        }

        Destroy(finder);
    }
}