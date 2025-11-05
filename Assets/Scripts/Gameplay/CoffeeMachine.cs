using UnityEngine;
using UnityEngine.Events;

public class CoffeeMachine : MonoBehaviour
{
    public GameObject CoffeeMachineUI;
    public bool Usingmachine = false;
    public PlayerMovement PlayerMovement;
    public UnityEvent interactAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerMovement = FindAnyObjectByType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Usingmachine == false)
        {
            CoffeeMachineUI.SetActive(false);
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                InteractMachine();
                
            }
        }
    }

    public void InteractMachine()
    {
        if (Usingmachine == false) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Usingmachine = true;
            PlayerMovement.moveSpeed = 0;
            CoffeeMachineUI.SetActive(true);
        }
        else if (Usingmachine == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Usingmachine = false;
            PlayerMovement.moveSpeed = 10;
            CoffeeMachineUI.SetActive(false);
        }
    }

}
