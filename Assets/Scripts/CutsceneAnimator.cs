using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Yarn.Unity;


public class CutsceneAnimator : MonoBehaviour
{


    private PlayableDirector director;
    public SerializableDictionary<string,TimelineAsset> timelineAssets;

    private TimelineAsset currentAsset;



    public static CutsceneAnimator instance {get; private set;}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(instance==null)
        {
            instance = this;
        } else
        {
            Destroy(this);
        }

        director = GetComponent<PlayableDirector>();
    }

    [YarnCommand("play_cutscene")]
    public void playEmote(string name)
    {
        

        timelineAssets.TryGetValue(name,out TimelineAsset currentAsset);
        director.playableAsset = currentAsset;
        director.Play();
        
        if(PlayerInputController.instance.playerCarryingState == PlayerInputController.carryingState.none) return;
        PlayerInputController.instance.useDrop();
    }
    //Play as emote, drop held item



    public void playAction(string name)
    {
        // Get current asset to be played
        timelineAssets.TryGetValue(name,out TimelineAsset currentAsset);
        director.playableAsset = currentAsset;
        //pause player movement
        float duration = (float)director.playableAsset.duration;
        StartCoroutine(pausePlayerMovement(duration));

        //
        director.Play();
    }
    //play as action so it works with delivery zones

    private IEnumerator pausePlayerMovement(float duration)
    {
        if(PlayerInputController.instance.lockMovement == true) yield break;
        
        PlayerInputController.instance.SetMovementLock(true);

        yield return new WaitForSeconds(duration);

        PlayerInputController.instance.SetMovementLock(false);
    }
}
