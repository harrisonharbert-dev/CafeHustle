using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueAudioController : ActionMarkupHandler
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool skipWhitespaceAndPunctuation = true;

    [SerializeField] private List<DialogueVoiceProfile> availableProfiles;
    [SerializeField] private DialogueVoiceProfile defaultProfile;
    [SerializeField] private UnityEvent onNewLineEvent;
    [SerializeField] private UnityEvent onCharacterEvent;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject continueButton;

    [System.Serializable]
    public class colors
    {
        public Sprite pink;
        public Sprite blue;
        public Sprite yellow;
        public Sprite green;
    }

    public colors dialogueNameColors;
    public Image dialogueNameImage;
    private DialogueVoiceProfile currentProfile;

    [Header("Punctuation Settings")]
    [Tooltip("Pause (in seconds) after a comma, semicolon, or colon.")]
    [SerializeField] float shortPauseDuration = 0.15f;
    [Tooltip("Pause (in seconds) after a period, question mark, or exclamation mark.")]
    [SerializeField] float longPauseDuration = 0.35f;    int characterCounter = 0;

    static readonly char[] ShortPauseChars = { ',', ';', ':' };
    static readonly char[] LongPauseChars = { '.', '!', '?' };


    void Awake()
    {
        currentProfile = defaultProfile;
        UpdateDialogueNameColor();
    }

    private void UpdateDialogueNameColor()
    {
        if (currentProfile == null || dialogueNameColors == null)
        {
            dialogueNameImage = null;
            return;
        }

        switch (currentProfile.dialogueNameColour)
        {
            case DialogueVoiceProfile.colors.pink:
                dialogueNameImage.sprite = dialogueNameColors.pink;
                break;
            case DialogueVoiceProfile.colors.blue:
                dialogueNameImage.sprite = dialogueNameColors.blue;
                break;
            case DialogueVoiceProfile.colors.yellow:
                dialogueNameImage.sprite = dialogueNameColors.yellow;
                break;
            case DialogueVoiceProfile.colors.green:
                dialogueNameImage.sprite = dialogueNameColors.green;
                break;
        }



    }

    private string GetNameEffect(DialogueVoiceProfile.nameEffects effect)
    {
        return effect switch
        {
            DialogueVoiceProfile.nameEffects.none => null,
            DialogueVoiceProfile.nameEffects.wavy => "wave",
            DialogueVoiceProfile.nameEffects.bold => "b",
            DialogueVoiceProfile.nameEffects.sketchy => "sketchy",
            DialogueVoiceProfile.nameEffects.shaky => "shake",
            DialogueVoiceProfile.nameEffects.random => "char",
            DialogueVoiceProfile.nameEffects.rainbow => "palette",
            DialogueVoiceProfile.nameEffects.jumpy => "jump",
            _ => null
        };
    }


    private string StripRichTextTags(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value, @"<[^>]+>", string.Empty);
    }

    private bool TryApplyVoiceFromNameText()
    {
        if (nameText == null)
        {
            return false;
        }

        string nameToMatch = StripRichTextTags(nameText.text).Trim();
        if (string.IsNullOrEmpty(nameToMatch))
        {
            return false;
        }

        var match = availableProfiles?.Find(p => p != null && p.profileID == nameToMatch);
        if (match == null)
        {
            if (currentProfile == null)
            {
                currentProfile = defaultProfile;
                UpdateDialogueNameColor();
            }

            return false;
        }

        currentProfile = match;
        characterCounter = 0;
        UpdateDialogueNameColor();
        return true;
    }

    private void ApplyNameTextEffect()
    {
        if (nameText == null || !nameText.gameObject.activeInHierarchy)
        {
            return;
        }

        var profile = currentProfile != null ? currentProfile : defaultProfile;
        if (profile == null)
        {
            return;
        }

        string tag = GetNameEffect(profile.nameEffect);
        if (string.IsNullOrEmpty(tag))
        {
            return;
        }

        string cleanName = StripRichTextTags(nameText.text);
        if (string.IsNullOrEmpty(cleanName))
        {
            return;
        }

        nameText.text = $"<{tag}>{cleanName}</{tag}>";
    }

    public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
    {
        characterCounter = 0;
        onNewLineEvent?.Invoke();
        continueButton.SetActive(false);
        TryApplyVoiceFromNameText();


        ApplyNameTextEffect();
    }

    // Synchronous — void
    public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
    {
    }

    public override void OnLineDisplayComplete()
    {
        continueButton.SetActive(true);
    }

    // Synchronous — void
    public override void OnLineWillDismiss()
    {
        continueButton.SetActive(false);
    }





    public override async YarnTask OnCharacterWillAppear(
        int currentCharacterIndex,
        MarkupParseResult line,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        // Pause based on the PREVIOUS character, since it has now finished displaying
        // and we're about to show the next one.
        if (currentCharacterIndex > 0)
        {
            char previousChar = line.Text[currentCharacterIndex - 1];

            if (System.Array.IndexOf(LongPauseChars, previousChar) >= 0)
            {
                await YarnTask.Delay((int)(longPauseDuration * 1000), cancellationToken);
            }
            else if (System.Array.IndexOf(ShortPauseChars, previousChar) >= 0)
            {
                await YarnTask.Delay((int)(shortPauseDuration * 1000), cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }

        var profile = currentProfile != null ? currentProfile : defaultProfile;
        char c = line.Text[currentCharacterIndex];

        if (profile != null && profile.audioClips.Length > 0
            && !(skipWhitespaceAndPunctuation && (char.IsWhiteSpace(c) || char.IsPunctuation(c))))
        {
            characterCounter++;
            if (characterCounter % profile.playEveryNCharacters == 0)
            {
                audioSource.pitch = Random.Range(profile.pitchMin, profile.pitchMax);
                audioSource.PlayOneShot(profile.audioClips[Random.Range(0, profile.audioClips.Length)]);
                onCharacterEvent?.Invoke();
            }
        }
    }
}
