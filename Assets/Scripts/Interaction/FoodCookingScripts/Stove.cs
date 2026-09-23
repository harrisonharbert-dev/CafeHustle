using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;



public class Stove : MonoBehaviour
{
    public bool FoodOnPlate;
    public UnityEvent[] CookingVFX;

    [SerializeField] private int foodOnStove = 0;

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
        if (collision.gameObject.CompareTag("Ingredient"))
        {
            DraggingScript dragging = collision.gameObject.GetComponent<DraggingScript>();
            FoodStats food = collision.gameObject.GetComponent<FoodStats>();

            if (dragging != null && dragging.isFood && food != null)
            {
                Debug.Log("Starting to cook: " + collision.gameObject.name);

                food.StartCooking();

                foodOnStove++;

                // First food placed on stove
                if (foodOnStove == 1)
                {
                    CookingVFX[0]?.Invoke();
                }
            }
        }
    }

    private void OnCollisionExit(UnityEngine.Collision collision)
    {
        if (collision.gameObject.CompareTag("Ingredient"))
        {
            DraggingScript dragging = collision.gameObject.GetComponent<DraggingScript>();
            FoodStats food = collision.gameObject.GetComponent<FoodStats>();

            if (dragging != null && dragging.isFood && food != null)
            {
                Debug.Log("Stopping cooking: " + collision.gameObject.name);

                food.StopCooking();

                foodOnStove--;
                foodOnStove = Mathf.Max(0, foodOnStove);

                // No food left on stove
                if (foodOnStove == 0)
                {
                    CookingVFX[1]?.Invoke();
                }
            }
        }
    }
}