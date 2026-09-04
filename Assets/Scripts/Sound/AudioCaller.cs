using UnityEngine;

public class AudioCaller : MonoBehaviour
{
    public void callAudio(string id)
    {
        SoundManager.instance.PlaySound2D(id);
    }
}
