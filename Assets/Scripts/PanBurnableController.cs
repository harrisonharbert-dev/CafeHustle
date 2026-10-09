using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PanBurnableController : MonoBehaviour
{

    [SerializeField] private List<Material> _materials = new List<Material>();
    [SerializeField] private string _burnableControllerReference = "_BURNCONTROL";

    [SerializeField]  [Range(0f,1f)]private float baseNum = 0f;
    [SerializeField] private float fillRate = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Reset all to zero
        foreach (var material in _materials)
        {
            if (material.HasProperty(_burnableControllerReference))
            {
                material.SetFloat(_burnableControllerReference, baseNum);
            }
        }

    }

    public void addBurnAmount(float amount)
    {
        float duration = amount / fillRate;
        DOTween.To(UpdateFloat, baseNum, Mathf.Clamp01(baseNum + amount), duration)
        .SetEase(Ease.OutBack);
    }

    void UpdateFloat(float num)
    {
        foreach (var material in _materials)
        {
            if (material.HasProperty(_burnableControllerReference))
            {
                material.SetFloat(_burnableControllerReference, num);
            }
        }

        baseNum = num;
    }
}
