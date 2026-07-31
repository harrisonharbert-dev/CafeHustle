using UnityEngine;

public class cookingStatus : MonoBehaviour
{

    [SerializeField] private string sliderPropertyName = "_Cooking_Stage";
    public float progress;
    [SerializeField] private float failBonus;

    private Renderer objectRenderer;
    private MaterialPropertyBlock propBlock;
    [SerializeField] private Material baseMaterial;
    [SerializeField] private string keyword;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        baseMaterial = GetComponent<Material>();
    }

    // Update is called once per frame
    public void UpdateShaderStatus(float progressTime)
    {
        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(sliderPropertyName, progressTime+failBonus);
        objectRenderer.SetPropertyBlock(propBlock);
    }
    public void AddFailBonus(float value)
    {
        failBonus = failBonus + value;
        failBonus = Mathf.Clamp01(failBonus);

        Debug.Log("Add FailBonus " + value + " to " +  "FailBonus. Total: " + failBonus);
    }
    public void ToggleMaterialKeyword(bool value)
    {

        Material material = objectRenderer.material;
        if (value)
        { material.EnableKeyword(keyword); }
        else
        { material.DisableKeyword(keyword); }

        Debug.Log("Set Keyword " + keyword + " to " + value);
    }
}
