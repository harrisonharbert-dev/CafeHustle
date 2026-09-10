using UnityEngine;

public class Barrier : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform RespawnPoint;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {
            other.transform.position = RespawnPoint.position;
        }
    }
}

