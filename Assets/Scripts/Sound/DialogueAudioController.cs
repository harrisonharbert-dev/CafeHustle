using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;
using Yarn.Unity.Attributes;
using TMPro;
using UnityEngine.Events;

public class DialogueAudioController : ActionMarkupHandler       
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool skipWhitespaceAndPunctuation = true;

    [SerializeField] private List<DialogueVoiceProfile> availableProfiles;
    [SerializeField] private DialogueVoiceProfile defaultProfile;
    [SerializeField] private UnityEvent onNewLineEvent;

    private DialogueVoiceProfile currentProfile;

    int characterCounter = 0;

    void Awake()
    {
        currentProfile = defaultProfile;
    }

    [YarnCommand("set_voice")]
    public void SetVoice(string profileID)
    {
        var match = availableProfiles.Find(p => p.profileID == profileID);

        if (match != null)
        {
            currentProfile = match;
            characterCounter = 0;
        }
        else
        {
            Debug.LogWarning($"[DialogueAudioController] No voice profile found with ID '{profileID}'.");
        }
    }

    [YarnCommand("reset_voice")]
    public void ResetVoice()
    {
        currentProfile = defaultProfile;
        characterCounter = 0;
    }

    public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
    {
        characterCounter = 0; // reset blip rhythm for each new line
        onNewLineEvent?.Invoke();
    }

    // Synchronous — void
    public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
    {
    }

    public override void OnLineDisplayComplete()
    {
    }

    // Synchronous — void
    public override void OnLineWillDismiss()
    {
    }





    public override YarnTask OnCharacterWillAppear(
        int currentCharacterIndex,
        MarkupParseResult line,
        CancellationToken cancellationToken)
    {
        var profile = currentProfile != null ? currentProfile : defaultProfile;
        if (profile == null || profile.audioClips.Length == 0)
            return YarnTask.CompletedTask;

        char c = line.Text[currentCharacterIndex];

        if (skipWhitespaceAndPunctuation && (char.IsWhiteSpace(c) || char.IsPunctuation(c)))
        {
            return YarnTask.CompletedTask;
        }

        characterCounter++;
        if (characterCounter % profile.playEveryNCharacters == 0)
        {
            audioSource.pitch = Random.Range(profile.pitchMin, profile.pitchMax);
            audioSource.PlayOneShot(profile.audioClips[Random.Range(0, profile.audioClips.Length)]);
        }

        return YarnTask.CompletedTask;
    }
}
