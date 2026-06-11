using System.Collections.Generic;
using UnityEngine;

public class Hotbar : MonoBehaviour
{
    [Header("6 Hotbar Slots")]
    public List<GameObject> slotPrefabs = new List<GameObject>(6);

    public GameObject GetPrefab(int index)
    {
        if (index < 0 || index >= slotPrefabs.Count)
            return null;

        return slotPrefabs[index];
    }


}