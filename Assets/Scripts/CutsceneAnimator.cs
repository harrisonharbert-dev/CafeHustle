using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Yarn.Unity;


public class CutsceneAnimator : MonoBehaviour
{


    public PlayableDirector director;
    public SerializableDictionary<string, TimelineAsset> timelineAssets;

    private TimelineAsset currentAsset;
    private TimelineAsset queuedAsset;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        director = GetComponent<PlayableDirector>();
    }

    [YarnCommand("play_cutscene")]
    public void playEmote(string name)
    {
        if (!checkIsPlaying())
        {


            timelineAssets.TryGetValue(name, out TimelineAsset currentAsset);
            playDirector(currentAsset);

            if (PlayerInputController.instance.playerState == PlayerInputController.playState.none || PlayerInputController.instance.playerState == PlayerInputController.playState.carryingNonDroppable) return;
            PlayerInputController.instance.useDrop();
        } 
        else
        {
            timelineAssets.TryGetValue(name, out TimelineAsset queuedAsset);
            StartCoroutine(queueCutscene(queuedAsset));
        }
    }
    //Play as emote, drop held item

    void playDirector(TimelineAsset asset)
    {
        director.playableAsset = asset;
        director.Play();
    }

    bool checkIsPlaying()
    {
        if (director.state == PlayState.Playing)
        {
            return true;
        }

        return false;
    }

    public void playAction(string name)
    {
        if (!checkIsPlaying())
        {
            // Get current asset to be played
            timelineAssets.TryGetValue(name, out TimelineAsset currentAsset);
            director.playableAsset = currentAsset;
            //pause player movement
            float duration = (float)director.playableAsset.duration;
            StartCoroutine(pausePlayerMovement(duration + 0.1f));

            //
            director.Play();
        }
    }
    //play as action so it works with delivery zones

    private IEnumerator pausePlayerMovement(float duration)
    {
        // If player can't already move, then do nothing
        if (PlayerInputController.instance.lockMovement == true) yield break;

        PlayerInputController.instance.SetMovementLock(true);

        yield return new WaitForSeconds(duration);

        if (PlayerInputController.instance.isinDialogue == false)
        {
            PlayerInputController.instance.SetMovementLock(false);
        }

    }

    private IEnumerator queueCutscene(TimelineAsset asset)
    {

        yield return new WaitUntil(()=> director.state != PlayState.Playing);

        playDirector(asset);
        
    }
}
