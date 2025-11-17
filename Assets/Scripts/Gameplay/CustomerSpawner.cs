using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public float timer;
    [SerializeField] private List<CustomerAI> CustomerAmount = new List<CustomerAI>();
    public int NumberofCustomers => CustomerAmount.Count;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = Random.Range(10, 20 - NumberofCustomers);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.GameStarted)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
           else if (timer <= 0)
            {
                SpawnCustomer();
                timer = Random.Range(10, 20 - NumberofCustomers);
            }
            TrackCustomerAmount();
        }
    }

    public void SpawnCustomer()
    {
       

    }

    private void TrackCustomerAmount()
    {
        var customers = FindObjectsByType<CustomerAI>(FindObjectsSortMode.None);
        foreach (var customer in customers)
        {
            CustomerAmount.Add(customer);


        }
        for (int i = CustomerAmount.Count - 1; i >= 0; i--)
        {
            if (CustomerAmount[i] == null && CustomerAmount[i].served)
            {

                CustomerAmount.RemoveAt(i);
            }

        }
        
    }
}
