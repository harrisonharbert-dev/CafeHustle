using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Order currentOrder; // 👈 THIS is what you're missing

    private void Awake()
    {
        Instance = this;
    }
}