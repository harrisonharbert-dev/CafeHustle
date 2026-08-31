using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Voice Profile",fileName ="NewVoiceProfile")]
public class DialogueVoiceProfile : ScriptableObject
{
    [Header("Identity")]
    public string profileID;

    [Header("Clips")]
    public AudioClip[] audioClips;

    [Header("Pitch")]
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    [Header("Rhythm")]
    [Min(1)] public int playEveryNCharacters = 1;


    public enum colors
    {
        pink,
        blue,
        yellow,
        green
    }

    public enum nameEffects
    {
        none,
        bold,
        wavy,
        sketchy,
        random,
        jumpy,
        rainbow,
        shaky,


    }
    [Header("Name Tag Properties")]
    public colors dialogueNameColour;
    public nameEffects nameEffect = nameEffects.none;
    
}
