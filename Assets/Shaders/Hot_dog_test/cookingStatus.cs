using UnityEngine;

public class cookingStatus : MonoBehaviour
{

    [SerializeField] private string sliderPropertyName = "_Cooking_Stage";
    public float progress;

    private Renderer objectRenderer;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    public void UpdateShaderStatus()
    {
        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(sliderPropertyName, progress);
        objectRenderer.SetPropertyBlock(propBlock);
    }
}
