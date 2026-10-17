using UnityEngine;

[CreateAssetMenu(fileName = "New Flipping Stage", menuName = "Modular Minigame/Flipping Stage")]
public class FlippingConfig : MinigameConfig
{
    public string stageName;
    public string stageID;
    [Header("Pan Properties")]
    public GameObject panPrefab;
    public Vector3 panScale = new Vector3(1f,1f,1f);
    public Quaternion panRotation = new Quaternion(0f,0f,0f,0f);

    [Header("Food in Pan Properties")]
    public GameObject foodInPanPrefab;
    public Vector3 foodInPanScale = new Vector3(1f, 1f, 1f);
    public Quaternion foodInPanRotation = new Quaternion(0f,0f,0f,0f);
    public Vector3 foodInPanPosition = new Vector3(0f,0f,0f);

    [Header("Minigame Properties")]
    public float gameDuration = 15f;
    public Vector2 popUpTiming = new Vector2(5f,6f);
    public float popUpDuration = 3f;
    [Space(15)]
    public Vector2 targetPositionRange = new Vector2(0.5f,0.6f);
    public float targetSize = .1f;
}