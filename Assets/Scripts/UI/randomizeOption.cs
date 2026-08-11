using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class randomizeOption : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private Vector2 randomRotationRange = new Vector2(-3f,3f);

    private Image uiImage;
    private RectTransform rectTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        randomize();
    }

    // Update is called once per frame
    void randomize()
    {
        int randomIndex = Random.Range(0,sprites.Count);
        float randomRotation = Random.Range(randomRotationRange.x,randomRotationRange.y);

        rectTransform.rotation = Quaternion.Euler(0, 0, randomRotation);
        uiImage.sprite = sprites[randomIndex];
    }
}
