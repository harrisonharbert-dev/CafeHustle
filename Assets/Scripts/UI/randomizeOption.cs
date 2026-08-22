using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class randomizeOption : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private Vector2 randomRotationRange = new Vector2(-3f,3f);

    private Image uiImage;
    private RectTransform rectTransform;
    private const string PlayerPrefKey = "randomizeOption_nextIndex";
    private static int runtimeFallbackIndex = 0;
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
        if (sprites == null || sprites.Count == 0) return;

        // Get the next index in a round-robin fashion. Prefer PlayerPrefs so
        // the sequence persists between play sessions; fall back to a static
        // runtime counter if PlayerPrefs isn't desired.
        int count = sprites.Count;
        int next = PlayerPrefs.GetInt(PlayerPrefKey, runtimeFallbackIndex);
        int index = Mathf.Abs(next) % count;

        // assign sprite and rotation
        float randomRotation = Random.Range(randomRotationRange.x, randomRotationRange.y);
        rectTransform.rotation = Quaternion.Euler(0, 0, randomRotation);
        uiImage.sprite = sprites[index];

        // advance and persist the next index
        int advanced = (index + 1) % count;
        runtimeFallbackIndex = advanced;
        PlayerPrefs.SetInt(PlayerPrefKey, advanced);
        PlayerPrefs.Save();
    }
}
