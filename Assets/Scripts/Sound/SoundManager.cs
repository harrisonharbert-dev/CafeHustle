using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static SoundManager instance;


    [SerializeField] private SoundLibrary sfxLibrary;
    [SerializeField] private AudioSource sfx2DSource;
    [SerializeField] [Range(0f, 0.5f)] private float randomPitch;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip,pos);
        }
    }

    public void PlaySound3D(string soundName, Vector3 pos)
    {
        PlaySound3D(sfxLibrary.GetClipFromName(soundName),pos);
    }

    public void PlaySound2D(string soundName)
    {
        float newPitch = 1 - Random.Range(-randomPitch,randomPitch);
        sfx2DSource.pitch = newPitch;
        
        sfx2DSource.PlayOneShot(sfxLibrary.GetClipFromName(soundName),sfxLibrary.GetVolume(soundName));
    }
    

}
