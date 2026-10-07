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

    private readonly Dictionary<string, bool> observedStoryStates = new Dictionary<string, bool>();
    private readonly List<UnityEvent> pendingStoryEvents = new List<UnityEvent>();


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

        if (storyPoints != null)
        {
            foreach (KeyValuePair<string, StoryPoint> entry in storyPoints)
            {
                observedStoryStates[entry.Key] = entry.Value.storyState;
            }
        }
    }

    private void Update()
    {
        if (storyPoints == null) return;

        pendingStoryEvents.Clear();
        foreach (KeyValuePair<string, StoryPoint> entry in storyPoints)
        {
            if (!observedStoryStates.TryGetValue(entry.Key, out bool observedState))
            {
                observedStoryStates[entry.Key] = entry.Value.storyState;
                continue;
            }

            if (observedState == entry.Value.storyState) continue;

            observedStoryStates[entry.Key] = entry.Value.storyState;
            if (entry.Value.storyState && entry.Value.events != null)
            {
                pendingStoryEvents.Add(entry.Value.events);
            }
        }

        foreach (UnityEvent storyEvent in pendingStoryEvents)
        {
            storyEvent.Invoke();
        }
    }


    public bool checkStoryState(string name)
    {
        storyPoints.TryGetValue(name, out StoryPoint point);
        return point.storyState;
    }
    [YarnCommand("set_story")]
    public void SetStoryPoint(string name)
    {
        if (!storyPoints.TryGetValue(name, out StoryPoint point)) return;

        if(point.storyState == true) return;
        point.storyState = true;
        storyPoints[name] = point;
        observedStoryStates[name] = true;
        point.events?.Invoke();
    }

    public void SkipStoryPoint(string name)
    {
        if (!storyPoints.TryGetValue(name, out StoryPoint point)) return;

        point.storyState = true;
        storyPoints[name] = point;
        observedStoryStates[name] = true;

        if (point.prerequisiteID != null)
        {
            SkipStoryPoint(point.prerequisiteID);
        }
        
        point.events?.Invoke();
    }
}
