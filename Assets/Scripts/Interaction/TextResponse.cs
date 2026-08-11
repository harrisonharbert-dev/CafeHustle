using System.Collections;
using TMPro;
using UnityEngine;

public class TextResponse : MonoBehaviour
{
    public TextMeshProUGUI Text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetText(string text)
    {
        if (Text != null)
        {
            Text.text = text;
            StartCoroutine(FadeText(2f, 0f));
        }
    }

    public IEnumerator FadeText(float duration, float targetAlpha)
    {
        float startAlpha = Text.color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float blend = Mathf.Clamp01(time / duration);

            Color color = Text.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, blend);
            Text.color = color;

            yield return null;
        }
    }
}
