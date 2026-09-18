using System.Diagnostics;
using UnityEngine;

public class cookingStatus : MonoBehaviour
{
    [Header("Cooking Material Status")]
    [SerializeField] private string sliderPropertyName = "_Cooking_Stage";
    public float progress;
    [SerializeField] public float failBonus;

    private Renderer objectRenderer;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    public void UpdateShaderStatus(float progressTime)
    {
        //Get property block from renderer
        objectRenderer.GetPropertyBlock(propBlock);

        //Update slider property
        propBlock.SetFloat(sliderPropertyName, progressTime + failBonus);
        objectRenderer.SetPropertyBlock(propBlock);
    }
    public void AddFailBonus(float value)
    {
        failBonus = failBonus + value;
        failBonus = Mathf.Clamp01(failBonus);
    }

    public void UpdateSide1Stage(float progressTime)
    {
        //Get property block from renderer
        objectRenderer.GetPropertyBlock(propBlock);

        //Update slider property
        propBlock.SetFloat("_SIDE1STAGE", progressTime + failBonus);
        objectRenderer.SetPropertyBlock(propBlock);
    }

        public void UpdateSide2Stage(float progressTime)
    {
        //Get property block from renderer
        objectRenderer.GetPropertyBlock(propBlock);

        //Update slider property
        propBlock.SetFloat("_SIDE2STAGE", progressTime + failBonus);
        objectRenderer.SetPropertyBlock(propBlock);
    }
}
