using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest/New Quest", order = 0)]
public class QuestItem : ScriptableObject {
    public string questID;
    public string mainObjectiveText;

    [System.Serializable]
    public class SideObjective
    {
        public string objectiveText;
        public bool isHiddenOnStart;
    }
    public List<SideObjective> sideObjectives;
}