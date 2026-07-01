using UnityEngine;
using System.Collections;

using UnityEngine.SceneManagement;
using Antlr4.Runtime.Atn;
using CsvHelper.Configuration.Attributes;

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

    void Start()
    {
        if (transitionType == TransitionType.InOnly || transitionType == TransitionType.InAndOut)
        {
            animator.SetTrigger("playStart");
        }
    } 

    public void LoadNamedScene(string sceneName)
    {
        StartCoroutine(LoadScene(sceneName));
    }

    IEnumerator LoadScene(string sceneName)
    {
        //play animation
        if (transitionType == TransitionType.OutOnly || transitionType == TransitionType.InAndOut) {
        animator.SetTrigger("playEnd");

        yield return new WaitForSeconds(transitionTime);
        }
        //
        SceneManager.LoadScene(sceneName);

    }
}
