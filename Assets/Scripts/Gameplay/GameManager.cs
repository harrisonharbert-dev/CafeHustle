using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static bool GameStarted;
    public List<Table> Tables;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Table GetFirstFreeTable()
    {
        foreach (var table in Tables)
        {

            if (!table.isOccupied)
                return table;
        }
        return null;
    }


    public void Awake()
    {
        GameStarted = true;
    }
}
