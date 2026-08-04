
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using DG.Tweening;

public class updateRoofState : MonoBehaviour
{
    [System.Serializable]
    public class obsEntry
    {
        public Transform roofPiece;

        [HideInInspector] public float originalPosition;

    }
    [System.Serializable]
    public class wallEntry
    {
        public SkinnedMeshRenderer skinnedMesh;
        public int blendshapeIndex;
        [HideInInspector] public float startingValue;
    }
    [Header("Properties")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float moveHeight = 5f;
    private bool playerInside = false;
    [Header("Roof Parts")]
    [SerializeField] private List<obsEntry> obstructions = new List<obsEntry>();

    [Header("Wall Parts")]
    [SerializeField] private List<wallEntry> walls = new List<wallEntry>();

    void Start()
    {
        foreach (var entry in obstructions)
        {
            if (entry.roofPiece == null) continue;
            entry.originalPosition = entry.roofPiece.position.y;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (var entry in obstructions)
        {
            if (entry.roofPiece == null) continue;
            entry.roofPiece.DOMoveY(moveHeight + entry.originalPosition, transitionDuration);        }

        foreach (var entry in walls)
        {
            if (entry.skinnedMesh == null) continue;

            transitionBlendshape(entry.skinnedMesh, entry.blendshapeIndex, 100f, transitionDuration);

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (var entry in obstructions)
        {
            if (entry.roofPiece == null) continue;

            entry.roofPiece.DOMoveY(entry.originalPosition, transitionDuration);        }

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
