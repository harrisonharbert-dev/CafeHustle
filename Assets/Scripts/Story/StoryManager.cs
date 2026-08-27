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
        storyPoints.TryGetValue(name, out StoryPoint point);

        if(point.storyState == true) return;

        if (point.prerequisiteID != null)
        {
            SetStoryPoint(point.prerequisiteID);
        }

        point.storyState = true;
        point.events?.Invoke();
    }
}
