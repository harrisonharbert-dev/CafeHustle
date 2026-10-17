using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class MinigameSessionEntry
{
    public MinigameDefinition minigame;
    public MinigameConfig configOverride;
}

[CreateAssetMenu(fileName = "MinigameSession", menuName = "Modular Minigame/Session")]

public class MinigameSession : ScriptableObject
{
    public List<MinigameSessionEntry> minigames = new();
}