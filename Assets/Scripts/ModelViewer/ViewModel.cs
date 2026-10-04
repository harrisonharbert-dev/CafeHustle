using UnityEngine;
[CreateAssetMenu(menuName = "Model Viewer/Model",fileName ="New Model Profile")]
public class ViewModel : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;
    public Vector3 prefabScale = new Vector3 (1f,1f,1f);

    [Header("Information")]
    public string title;
    public string description;
    public string id;

      public enum colors
    {
        pink,
        blue,
        yellow,
        green
    }

    [Header("Title Banner Properties")]
    public colors titleBannerColor;
}
