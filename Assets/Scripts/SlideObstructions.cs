using NUnit.Framework;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;





public class SlideObstructions : MonoBehaviour
{

    [System.Serializable]   
    public class obsEntry
    {
        public Transform obstruction;
        [Tooltip("How far to move up from the original position")]
        public float duration = 1f;
        [Tooltip("Transition duration in seconds")]
        public float moveHeight = 1f;


        [HideInInspector] public Vector3 originalPosition;
        [HideInInspector] public Vector3 targetPosition;
    }

    [Header("Obstructions")]
    [SerializeField] private List<obsEntry> obstructions = new List<obsEntry>();


    private bool playerInside = false;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;


    private void Start()
    {
        //initialize positions
        foreach (var entry in obstructions)
        {
            if (entry.obstruction == null) 
            {
                Debug.LogWarning("SlideObstructions: one of the obsEntry.obstruction is null; skipping", this);
                continue;
            }

            entry.originalPosition = entry.obstruction.position;
            entry.targetPosition = entry.originalPosition + Vector3.up * entry.moveHeight;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (enableDebugLogs) Debug.Log($"OnTriggerEnter by {other.gameObject.name}", other.gameObject);

        if (playerInside) return;
        playerInside = true;

        foreach (var entry in obstructions)
        {
            if (entry.obstruction == null) continue;

            var routine = StartCoroutine(MoveTo(entry.obstruction, entry.targetPosition, entry.duration));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (enableDebugLogs) Debug.Log($"OnTriggerExit by {other.gameObject.name}", other.gameObject);

        if (!playerInside) return;
        playerInside = false;

        foreach (var entry in obstructions)
        {
            if (entry.obstruction == null) continue;

            var routine = StartCoroutine(MoveTo(entry.obstruction, entry.originalPosition, entry.duration));
        }
    }

    private IEnumerator MoveTo(Transform target, Vector3 endPos, float time)
    {
        Vector3 startPos = target.position;
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            float easedT = t * t * (3f - 2f * t);

            target.position = Vector3.Lerp(startPos, endPos, easedT);
            yield return null;
        }

        target.position = endPos;
    }
}