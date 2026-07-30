using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;

public class MinigameSessionManager : MonoBehaviour

{
    [Header("Session Properties")]
    [SerializeField] private float playDuration = 30f;
    private float playStartTime;
    [SerializeField] private int popupCount = 10;

    [SerializeField] private cookingStatus item;

    [Header("Pop-up Spacing")]
    [SerializeField] private float minDelay = 1f;
    [SerializeField] private float maxDelay = 4f;

    [Header("Events")]
    public UnityEvent onPlayStart;
    public UnityEvent onPlayEnd;

    public UnityEvent minigameEvent;

    public float Progress { get; private set; }
    private bool playing;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartPlay()
    {
        if (playing)
            return;
        playStartTime = Time.time;
        StartCoroutine(PlayRoutine());
    }



    // play session
    private IEnumerator PlayRoutine()
    {

        Debug.Log("Begin minigame session");
        StartCoroutine(UpdateProgress());
        playing = true;
        onPlayStart?.Invoke();

        float endTime = Time.time + playDuration;
        int popUpsSpawned = 0;

        while (Time.time < endTime && popUpsSpawned < popupCount)
        {
            float wait = Random.Range(minDelay, maxDelay);

            yield return new WaitForSeconds(wait);

            if (Time.time >= endTime)
                break;
            
            minigameEvent?.Invoke();
            popUpsSpawned ++;

        }

        playing = false;
        onPlayEnd?.Invoke();
    }

    private IEnumerator UpdateProgress()
{
    float elapsed = 0f;

    while (elapsed < playDuration)
    {
        elapsed += Time.deltaTime;
        Progress = Mathf.Clamp01(elapsed / playDuration);
        item.UpdateShaderStatus(Progress);

        yield return null;
    }

    Progress = 1f;
}
}
