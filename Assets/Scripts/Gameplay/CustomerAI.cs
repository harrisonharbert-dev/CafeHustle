using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    public bool served = false;
    private Table assignedTable;
    public GameManager GameManager;
    public NavMeshAgent agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager = FindAnyObjectByType<GameManager>();

    }
    private void Awake()
    {
        assignedTable = GameManager.GetFirstFreeTable();
        if (assignedTable != null)
        {
            assignedTable.isOccupied = true;
            agent.SetDestination(assignedTable.SeatPoint.position);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
