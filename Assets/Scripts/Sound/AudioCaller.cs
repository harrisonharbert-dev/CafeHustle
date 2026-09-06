using UnityEngine;

public class AudioCaller : MonoBehaviour
{
    public void callAudio(string id)
    {
        if (SoundManager.instance == null)
        {
           Debug.LogWarning($"Missing SoundManager in scene when tried to call on {this}");
           return;
        }
    ;
        SoundManager.instance.PlaySound2D(id);
    }
}
