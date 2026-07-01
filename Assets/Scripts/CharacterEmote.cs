using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

[System.Serializable]
public class Emote
{
    public string name;
    public Sprite sprite;
}

public class CharacterEmote : MonoBehaviour
{
    [Header("Emotes")]
    [SerializeField] private List<Emote> emotes = new();

    [Header("UI")]
    [SerializeField] private Image emoteImage;

    [Header("Transition Frames")]
    [SerializeField] private List<Sprite> transitionFrames = new();

    [Header("Timing")]
    [SerializeField] private float frameDuration = 0.08f;
    [SerializeField] private float emoteDuration = 2f;

    private Dictionary<string, Sprite> emoteDictionary;

    private void Awake()
    {
        //Set default
        emoteImage.sprite = transitionFrames[0];

        emoteDictionary = new Dictionary<string, Sprite>();

        foreach (Emote emote in emotes)
        {
            if (!emoteDictionary.ContainsKey(emote.name))
                emoteDictionary.Add(emote.name, emote.sprite);
            else
                Debug.LogWarning($"Duplicate emote name: {emote.name}");
        }
    }

    public Sprite GetEmote(string name)
    {
        emoteDictionary.TryGetValue(name, out Sprite sprite);
        return sprite;
    }

    [YarnCommand("play_emote")]
    public void PlayEmote(string name)
    {
        StartCoroutine(RunEmote(name));
    }

    public IEnumerator RunEmote(string name)
    {
        Sprite emoteSprite = GetEmote(name);

        if (emoteSprite == null)
        {
            Debug.LogWarning($"Emote '{name}' not found.");
            yield break;
        }

        if (transitionFrames == null || transitionFrames.Count == 0)
        {
            emoteImage.sprite = emoteSprite;
            yield return new WaitForSeconds(emoteDuration);
            yield break;
        }

        // ▶ TRANSITION IN 
        for (int i = 0; i < transitionFrames.Count; i++)
        {
            emoteImage.sprite = transitionFrames[i];
            yield return new WaitForSeconds(frameDuration);
        }

        //  SHOW EMOTE
        emoteImage.sprite = emoteSprite;

        yield return new WaitForSeconds(emoteDuration);

        
        for (int i = transitionFrames.Count - 1; i >= 0; i--)
        {
            emoteImage.sprite = transitionFrames[i];
            yield return new WaitForSeconds(frameDuration);
        }
    }
}
    


