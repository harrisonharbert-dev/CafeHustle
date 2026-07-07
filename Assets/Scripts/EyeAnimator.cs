using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class EyeAnimator : MonoBehaviour
{

    [SerializeField] private Renderer eyeRenderer;
    private MaterialPropertyBlock propertyBlock;



    [SerializeField] private string indexProperty = "_Index";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }


    [YarnCommand("set_eye")]
    public void SetEyeIndex(int index)
    {
        eyeRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetInt(indexProperty, index);

        eyeRenderer.SetPropertyBlock(propertyBlock);
    }
}

