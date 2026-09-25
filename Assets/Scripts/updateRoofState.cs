
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using DG.Tweening;
using CsvHelper.Configuration.Attributes;

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
        foreach(var entry in materials)
        {
            entry.SetFloat("_ALPHA",1f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (var entry in materials)
        {
            entry.DOFloat(0f, "_ALPHA", transitionDuration);
        }


        foreach (var entry in walls)
        {
            if (entry.skinnedMesh == null) continue;

            transitionBlendshape(entry.skinnedMesh, entry.blendshapeIndex, 100f, transitionDuration);

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        foreach (var entry in materials)
        {
            entry.DOFloat(1f, "_ALPHA", transitionDuration);
        }

        foreach (var entry in walls)
        {
            if (entry.skinnedMesh == null) continue;

            transitionBlendshape(entry.skinnedMesh, entry.blendshapeIndex, 0f, transitionDuration);

        }
    }

    void transitionBlendshape(SkinnedMeshRenderer mesh, int index, float targetweight, float duration)
    {
        StartCoroutine(transitionRoutine(mesh, index, targetweight, duration));
    }
    private IEnumerator transitionRoutine(SkinnedMeshRenderer mesh, int index, float targetweight, float duration)
    {
        float startWeight = mesh.GetBlendShapeWeight(index);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float weight = Mathf.Lerp(startWeight, targetweight, t);
            mesh.SetBlendShapeWeight(index, weight);

            yield return null;
        }
    }
}
