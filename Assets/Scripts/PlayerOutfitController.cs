using UnityEngine;

public class PlayerOutfitController : MonoBehaviour
{
    [System.Serializable]
    public struct outfitItem
    {
        public GameObject outfitObject;
    }

    public outfitItem[] playerOutfitItem;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void updateItem(int index)
    {
        playerOutfitItem[index].outfitObject.SetActive(enabled);
    }
}
