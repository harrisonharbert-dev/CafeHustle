using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    [System.Serializable]
    public struct SoundEffect
    {
        public string groupID;
        [Range(0,1)] public float volume;
        public AudioClip[] clips;
    }

    public SoundEffect[] soundEffects;

    public AudioClip GetClipFromName(string name)
    {
        foreach (var soundEffect in soundEffects)
        {
            if (soundEffect.groupID == name)
            {
                return soundEffect.clips[Random.Range(0,soundEffect.clips.Length)];
            }
        }

        return null;
    }

    public float GetVolume(string name)
    {
        foreach(var soundEffect in soundEffects)
        {
            if(soundEffect.groupID == name)
            {
                return soundEffect.volume;
            }
        }
        return 0;
    }
}
