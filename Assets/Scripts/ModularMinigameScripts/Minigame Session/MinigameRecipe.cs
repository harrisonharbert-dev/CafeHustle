using UnityEngine;

[CreateAssetMenu(fileName = "New Minigame Recipe", menuName = "Modular Minigame/Minigame Recipe")]
public class MinigameRecipe : ScriptableObject {
    public string id;

    public MinigameSession session;

    [Header("Table Items")]
    public GameObject centerItem;
    public Vector3 centerItemScale = Vector3.one;
    public Quaternion centerItemRotation;
    public float centerItemHeightOffset = 0f;
    [Space(15)]
    public GameObject leftItem;
    public Vector3 leftItemScale = Vector3.one;
    public Quaternion leftItemRotation;
    public float leftItemHeightOffset = 0f;
    [Space(15)]
    public GameObject rightItem;
    public Vector3 rightItemScale = Vector3.one;
    public Quaternion rightItemRotation;
    public float rightItemHeightOffset = 0f;
    
}