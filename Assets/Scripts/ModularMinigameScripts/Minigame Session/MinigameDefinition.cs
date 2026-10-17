using UnityEngine;

[CreateAssetMenu(fileName = "Minigame", menuName ="Modular Minigame/Minigame Definition")]

public class MinigameDefinition : ScriptableObject 
{
    [Header("Scene")]
    public string sceneName;

    [Header("Default Configuration")]
    public MinigameConfig defaultConfig;
}