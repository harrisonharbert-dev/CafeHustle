using UnityEngine;

public class StoryCaller : MonoBehaviour
{
    public void callStoryPoint(string id)
    {
        if (StoryManager.instance == null)
        {
            Debug.LogWarning($"Missing StoryManager in scene when tried to call on {this}");
            return;
        }
        StoryManager.instance.SetStoryPoint(id);
    }
}
