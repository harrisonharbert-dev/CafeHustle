
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class updateRoofState : MonoBehaviour
{


    [System.Serializable]
    public class wallEntry
    {
        public SkinnedMeshRenderer skinnedMesh;
        public int blendshapeIndex;
        [HideInInspector] public float startingValue;
    }
    [Header("Properties")]
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Wall Parts")]
    [SerializeField] private List<wallEntry> walls = new List<wallEntry>();

    [Header("Roof Materials")]
    [SerializeField] private Material[] materials;

    void Start()
    {
        foreach (var entry in materials)
        {
            entry.SetFloat("_ALPHA", 1f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (var entry in materials)
        {
            entry.DOFloat(0f, "_ALPHA", transitionDuration);
        }

        for (int i = 0; i < walls.Count; i++)
        {
            var entry = walls[i];
            if (entry.skinnedMesh == null) continue;

            TransitionBlendshape(entry.skinnedMesh, entry.blendshapeIndex, 100f, transitionDuration, i);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        foreach (var entry in materials)
        {
            entry.DOFloat(1f, "_ALPHA", transitionDuration);
        }

        for (int i = 0; i < walls.Count; i++)
        {
            var entry = walls[i];
            if (entry.skinnedMesh == null) continue;

            TransitionBlendshape(entry.skinnedMesh, entry.blendshapeIndex, 0f, transitionDuration, i);
        }
    }

    void TransitionBlendshape(SkinnedMeshRenderer mesh, int blendshapeIndex, float targetWeight, float duration, int listIndex)
    {
        DOTween.Kill(listIndex);
        Sequence sequence = DOTween.Sequence();

        sequence.SetId(listIndex);
        sequence.Join(DOTween.To(() => mesh.GetBlendShapeWeight(blendshapeIndex),
            weight => mesh.SetBlendShapeWeight(blendshapeIndex, weight),
            targetWeight, duration));
    }
}
