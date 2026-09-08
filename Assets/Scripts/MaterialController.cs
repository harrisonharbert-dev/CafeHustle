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

   public void SetMaterialBoolean(string name)
    {
        //Get property block of current renderer
        renderer.GetPropertyBlock(propBlock);

        if(renderer.sharedMaterial.HasProperty(name))
        {
            //Get inverse of current boolean value and sets as newValue
            float currentValue = propBlock.HasFloat(name)
                ? propBlock.GetFloat(name)
                : renderer.sharedMaterial.GetFloat(name);
            float newValue = 1 - currentValue;

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
}