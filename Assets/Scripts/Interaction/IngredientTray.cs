using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class IngredientTray : MonoBehaviour
{
    public int IngredientsNeeded;
    public int IngredientsCollected;
    public bool Completed;
    public UnityEvent Action;

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
        if (other.CompareTag("Ingredient"))
        {
            DraggingScript draggingScript = other.GetComponent<DraggingScript>();

            if (draggingScript != null)
            {
                IngredientsCollected++;
                StartCoroutine(WaitForItem(draggingScript));
            }
        }
    }

    private IEnumerator WaitForItem(DraggingScript draggingScript)
    {
        // Wait until the player lets go of the ingredient
        yield return new WaitUntil(() => draggingScript.dragging == false);

        // Then disable dragging
        draggingScript.enabled = false;
    }
}