using UnityEngine;

public class BurgerPlate : MonoBehaviour
{
    [Header("Stack Settings")]
    public Transform stackPoint;
    public float stackHeight = 0.3f;

    private int ingredientCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {
            
                StackIngredient(other.transform);
                other.GetComponent<DraggingScript>().enabled = false;  // Disable dragging once stacked
            
        }

        void StackIngredient(Transform ingredient)
        {
            // Disable physics while stacked
            Rigidbody rb = ingredient.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // Calculate stack position
            Vector3 newPos = stackPoint.position +
                             Vector3.up * (ingredientCount * stackHeight);

            ingredient.position = newPos;

            // Optional: parent to burger
            ingredient.SetParent(stackPoint);

            ingredientCount++;

        }
    }
}
