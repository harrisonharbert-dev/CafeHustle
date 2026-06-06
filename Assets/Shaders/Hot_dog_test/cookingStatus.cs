using UnityEngine;

public class cookingStatus : MonoBehaviour
{

    [SerializeField] private string sliderPropertyName = "_Cooking_Stage";
    [Range(0f, 1f)] public float targetValue;

    private Renderer objectRenderer;
    private MaterialPropertyBlock propBlock;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update()
    {
        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(sliderPropertyName, targetValue);
        objectRenderer.SetPropertyBlock(propBlock);
    }
}
