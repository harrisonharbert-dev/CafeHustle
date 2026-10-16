using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ModularMinigameHub : MonoBehaviour
{
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;


    [SerializeField] private float itemDelay = 0.2f;

    [SerializeField] private MinigameRecipe loadedRecipe;

    private GameObject centerItem;
    private GameObject leftItem;
    private GameObject rightItem;


    void SetUpTable(MinigameRecipe recipe)
    {
        loadedRecipe = recipe;
        StartCoroutine(SetUpTableRoutine());
    }

    private IEnumerator SetUpTableRoutine()
    {
        //Clear any existing items on the table
        if (centerItem || leftItem || rightItem)
        {
            Destroy(centerItem);
            Destroy(leftItem);
            Destroy(rightItem);
        }

        //Instantiate the table item
        centerItem = Instantiate(loadedRecipe.centerItem, centerPoint);
        //Center item and set scale
        centerItem.transform.localPosition = Vector3.zero + new Vector3(0f,loadedRecipe.centerItemHeightOffset, 0f);
        centerItem.transform.localScale = loadedRecipe.centerItemScale;
        centerItem.transform.localRotation = loadedRecipe.centerItemRotation;
        //Add tweener component to animate them falling in
        centerItem.AddComponent<ObjectInOutTweener>();

        //Wait for delay to reveal all items in sequence
        yield return new WaitForSeconds(itemDelay);

        leftItem = Instantiate(loadedRecipe.leftItem, leftPoint);
        leftItem.transform.localPosition = Vector3.zero + new Vector3(0f,loadedRecipe.leftItemHeightOffset, 0f);
        leftItem.transform.localScale = loadedRecipe.leftItemScale;
        leftItem.transform.localRotation = loadedRecipe.leftItemRotation;
        leftItem.AddComponent<ObjectInOutTweener>();

        yield return new WaitForSeconds(itemDelay);

        rightItem = Instantiate(loadedRecipe.rightItem, rightPoint);
        rightItem.transform.localPosition = Vector3.zero + new Vector3(0f,loadedRecipe.rightItemHeightOffset, 0f);
        rightItem.transform.localScale = loadedRecipe.rightItemScale;
        rightItem.transform.rotation = loadedRecipe.rightItemRotation;
        rightItem.AddComponent<ObjectInOutTweener>();

    }
}
