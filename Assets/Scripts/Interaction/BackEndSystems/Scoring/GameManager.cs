using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Order currentOrder;
    public PlateScorer plate;

    private void Awake()
    {
        Instance = this;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ServeFood()
    {
        if (plate == null)
        {
            Debug.LogError("No PlateScorer assigned to GameManager!");
            return;
        }

        if (currentOrder == null)
        {
            Debug.LogError("There is no current order!");
            return;
        }

        // Check if the plate contains exactly what the order requires.
        bool orderComplete = plate.CheckPlate();

        if (orderComplete)
        {
            Debug.Log("Order complete! Food can be served.");

            // Put whatever should happen after successfully serving
            // the food here.

            return;
        }

        Debug.Log("Order is not complete! Missing or incorrect food.");
    }
}