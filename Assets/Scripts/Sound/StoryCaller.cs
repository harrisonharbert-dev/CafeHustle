using UnityEngine;

public class StoryCaller : MonoBehaviour
{
    public void callStoryPoint(string id)
    {
        StoryManager.instance.SetStoryPoint(id);
    }
}
