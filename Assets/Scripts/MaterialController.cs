using UnityEngine;

public class MaterialController : MonoBehaviour
{
    private Renderer renderer;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    public void SetMaterialBoolTrue(string name)
    {
        //Get property block of current renderer
        renderer.GetPropertyBlock(propBlock);

        if(renderer.sharedMaterial.HasFloat(name))
        {
            float newValue = 1;

            //Set new float
            propBlock.SetFloat(name,newValue);
            renderer.SetPropertyBlock(propBlock);
        }
        else
        {
            Debug.LogWarning($"[Material Controller] Invalid boolean reference name: {name} on {this}. Check spelling.");
            return;
        }
    }

    public void SetMaterialBoolFalse(string name)
    {
        renderer.GetPropertyBlock(propBlock);

        if(renderer.sharedMaterial.HasFloat(name))
        {
            float newValue = 0f;

            propBlock.SetFloat(name,newValue);
            renderer.SetPropertyBlock(propBlock);
        }
        else
        {
            Debug.LogWarning($"[Material Controller] Invalid boolean reference name: {name} on {this}. Check spelling.");
            return;
        }
    }
}