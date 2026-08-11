using System;
using UnityEngine;
using Yarn.Unity;

public class PlayerOutfitController : MonoBehaviour
{
    [System.Serializable]
    public struct outfitItem
    {
        public GameObject outfitObject;
        public string outfitType;
    }

    public SerializableDictionary<string,outfitItem> outfits;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void wearItem(string name)
    {
        outfits.TryGetValue(name, out outfitItem item);
        string type = item.outfitType;

        foreach(var outfit in outfits.Values)
        {
            if(outfit.outfitType == type && outfit.outfitObject != item.outfitObject)
            {
                outfit.outfitObject.SetActive(false);
            }
        }


        //Set enabled
        item.outfitObject.SetActive(true);
    }

    public void clearItem(string name)
    {
        outfits.TryGetValue(name, out outfitItem item);
        string type = item.outfitType;

        foreach(var outfit in outfits.Values)
        {
            if(outfit.outfitType == type)
            {
                outfit.outfitObject.SetActive(false);
            }
        }
    }
}
