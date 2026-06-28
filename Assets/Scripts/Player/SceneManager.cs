using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{

    [SerializeField] private Material transitionMaterial;
    [SerializeField] private string scaleValue = "_Scale";
    [SerializeField] private float transitionDuration = 1f;


    void Start()
    {
        if (transitionMaterial != null)
            transitionMaterial.SetFloat(scaleValue, 0f);
            LerpTransition(8f);
    }

    public void LerpTransition(float target)
    {
        StartCoroutine(LerpTransitionCoroutine(target));
    }

    private IEnumerator LerpTransitionCoroutine(float target)
    {
        float current = transitionMaterial.GetFloat(scaleValue);
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            transitionMaterial.SetFloat(scaleValue, Mathf.Lerp(current, target, t));
            yield return null;
        }

        transitionMaterial.SetFloat(scaleValue, target);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {

        
        // Run the transition to fully faded (target = 1)
        yield return StartCoroutine(LerpTransitionCoroutine(0f));

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
    }
}
