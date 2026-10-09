using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IngredientTray : MonoBehaviour
{
    public int IngredientsNeeded;
    public int IngredientsCollected;
    public bool Completed;
    public UnityEvent Action;

    // Ingredients currently touching the tray trigger.
    private readonly HashSet<DraggingScript> inside = new HashSet<DraggingScript>();

    // Ingredients that were already counted, so they never count twice.
    private readonly HashSet<DraggingScript> counted = new HashSet<DraggingScript>();

    // Ingredients being waited on (so each one only has one coroutine).
    private readonly HashSet<DraggingScript> waiting = new HashSet<DraggingScript>();

    void Update()
    {
        if (IngredientsNeeded <= IngredientsCollected && !Completed)
        {
            Action?.Invoke();
            Completed = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // The collider can be on the child model, so look up the hierarchy.
        DraggingScript draggingScript = other.GetComponentInParent<DraggingScript>();

        if (draggingScript == null)
            return;

        if (!other.CompareTag("Ingredient") &&
            !draggingScript.CompareTag("Ingredient"))
            return;

        // Already counted, or already being waited on.
        if (counted.Contains(draggingScript))
            return;

        inside.Add(draggingScript);

        if (waiting.Add(draggingScript))
            StartCoroutine(WaitForRelease(draggingScript));
    }

    private void OnTriggerExit(Collider other)
    {
        DraggingScript draggingScript = other.GetComponentInParent<DraggingScript>();

        if (draggingScript == null)
            return;

        // If it leaves the tray while still held, it won't be counted.
        // (Ignored for ingredients that were already counted.)
        inside.Remove(draggingScript);
    }

    private IEnumerator WaitForRelease(DraggingScript draggingScript)
    {
        // Wait until the player lets go, OR the ingredient leaves the tray.
        yield return new WaitUntil(() =>
            draggingScript == null ||
            !draggingScript.dragging ||
            !inside.Contains(draggingScript));

        waiting.Remove(draggingScript);

        // Destroyed, or it was dragged back out of the tray while held.
        if (draggingScript == null ||
            !inside.Contains(draggingScript))
            yield break;

        // Let go while inside the tray, so it counts now.
        counted.Add(draggingScript);
        IngredientsCollected++;

        // Make sure the progress bar doesn't stay stuck on this food,
        // because a disabled DraggingScript no longer gets pointer exit.
        if (draggingScript.foodStatsScript != null)
            draggingScript.foodStatsScript.IsHovering = false;

        // Then disable dragging.
        draggingScript.enabled = false;
    }
}