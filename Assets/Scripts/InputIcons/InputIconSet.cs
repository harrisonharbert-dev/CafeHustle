using UnityEngine;

[CreateAssetMenu(fileName = "new Input Icon Set", menuName = "Input Icon")]
public class InputIconSet : ScriptableObject {
    [Header("Keyboard/Mouse")]
    public Sprite keyboardSprite;
    [Space(15)]
    
    [Header("Xbox")]
    public Sprite xboxSprite;
    [Space(15)]

    [Header("PlayStation")]
    public Sprite playstationSprite;
}