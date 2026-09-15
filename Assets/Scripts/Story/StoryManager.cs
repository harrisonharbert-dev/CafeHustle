using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

public class StoryManager : MonoBehaviour
{

    [System.Serializable]
    public struct StoryPoint
    {
        public bool storyState;
        public string prerequisiteID;
        public UnityEvent events;
    }

    public SerializableDictionary<string, StoryPoint> storyPoints;



    public static StoryManager instance { get; private set; }

        private void Awake()
    {
        // Enforce a Singleton pattern so duplicate managers don't spawn
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Keeps this object alive between scenes
    }


    public bool checkStoryState(string name)
    {
        storyPoints.TryGetValue(name, out StoryPoint point);
        return point.storyState;
    }

    public void SetStoryPoint(string name)
    {
        if (!storyPoints.TryGetValue(name, out StoryPoint point)) return;

        if(point.storyState == true) return;
        point.storyState = true;
        storyPoints[name] = point;
        point.events?.Invoke();
    }

    public void SkipStoryPoint(string name)
    {
        if (!storyPoints.TryGetValue(name, out StoryPoint point)) return;

        point.storyState = true;
        storyPoints[name] = point;

        if (point.prerequisiteID != null)
        {
            SkipStoryPoint(point.prerequisiteID);
        }
        
        point.events?.Invoke();
    }
}
