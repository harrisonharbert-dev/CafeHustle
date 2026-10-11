using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;

public class interactableZoneIndicator : MonoBehaviour
{

    private Material mat;
    private VisualEffect visualEffect;
    private float defaultAlpha = 0.25f;
    private float transitionDuration = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mat = GetComponent<Renderer>().material;
        visualEffect = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    public void changeIndicatorVisibility(bool option)
    {
        float value = option ? defaultAlpha : 0f;
        DOTween.Kill(mat);
        DOTween.To(
            () => mat.GetFloat("_Alpha"),
            alpha => mat.SetFloat("_Alpha", alpha),
            value,
            transitionDuration)
            .SetTarget(mat);

        if (option == true)
        {
            visualEffect.Play();
        }
        else if (option == false)
        {
            visualEffect.Stop();
        }
    }
}
