using System.Collections.Generic;
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
        timelineAssets.TryGetValue(name,out TimelineAsset currentAsset);
        director.playableAsset = currentAsset;
        director.Play();
    }
    //play as action so it works with delivery zones
}
