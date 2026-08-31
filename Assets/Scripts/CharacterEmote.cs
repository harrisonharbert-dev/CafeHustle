using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using Unity.VisualScripting;

public class CharacterEmote : MonoBehaviour
{
    [Header("Emotes")]
    public SerializableDictionary<string,Sprite> emoteDictionary;

    [Header("UI")]
    [SerializeField] private Image emoteImage;
    [SerializeField] private Image bubbleImage;

    [Header("Transition Frames")]
    [SerializeField] private List<Sprite> transitionFrames = new();

    [Header("Timing")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private float emoteDuration = 2f;

    private Tween transitionTween;
    private int currentIndex;


    private void Awake()
    {
        //Set default to empty sprite
        emoteImage.sprite = transitionFrames[0];
        bubbleImage.sprite = transitionFrames[0];
    }

    public Sprite GetEmote(string name)
    {
        emoteDictionary.TryGetValue(name, out Sprite sprite);
        if(sprite == null)
        {
            Debug.LogWarning($"[Emote] Emote '{name}' on {this} is null, please replace.");
        }
        return sprite;
    }

    [YarnCommand("play_emote")]
    public void PlayEmote(string name)
    {
        PlayIn(name);
    }

    void PlayIn(string name) 
    {
        transitionTween?.Kill();

        //Sets start and end frame index
        int start = 0;
        int end = transitionFrames.Count - 1;

        transitionTween = DOVirtual.Int(start, end, transitionDuration,OnTweenUpdate)
            .SetEase(Ease.Linear)
            .OnComplete(() => showEmote(name));
    }

    void PlayOut() 
    {
        int start = transitionFrames.Count - 1;
        int end = 0;

        transitionTween = DOVirtual.Int(start, end, transitionDuration, OnTweenUpdate)
            .SetEase(Ease.Linear);
    }

    void OnTweenUpdate(int value)
    {
        bubbleImage.sprite = transitionFrames[value];
    }

    public void showEmote(string name)
    {
        StartCoroutine(showEmoteRoutine(name));
    }

    private IEnumerator showEmoteRoutine(string name)
    {
        
        emoteImage.sprite = GetEmote(name);

        yield return new WaitForSeconds(emoteDuration);

        //Set to empty sprite
        PlayOut();
        emoteImage.sprite = transitionFrames[0];
    }


    [YarnCommand("play_emote_plain")]
    public void PlayEmotePlain(string name)
    {
        StartCoroutine(showEmotePlainRoutine(name));
    }

    private IEnumerator showEmotePlainRoutine(string name)
    {
      emoteImage.sprite = GetEmote(name);

      yield return new WaitForSeconds(emoteDuration);

      //
      emoteImage.sprite = transitionFrames[0];   
    }
}
    


