using DG.Tweening;
using UnityEngine;

public class interactableZoneIndicator : MonoBehaviour
{

    private Material mat;
    private float defaultAlpha = 0.25f;
    private float transitionDuration = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    public void changeIndicatorVisibility(bool option)
    {
        float value = option ? defaultAlpha : -defaultAlpha;
        mat.SetFloat("_Alpha",value);
    }
}
