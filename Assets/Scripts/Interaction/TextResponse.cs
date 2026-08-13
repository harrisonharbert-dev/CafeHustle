using System.Collections;
using TMPro;
using UnityEngine;

public class TextResponse : MonoBehaviour
{
    public TextMeshProUGUI Text;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (Text != null)
        {
            Color color = Text.color;
            color.a = 0f;
            Text.color = color;
        }
    }

    public void SetText(string text)
    {
        if (Text == null)
            return;

        // Stop the previous fade.
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // Set the new text.
        Text.text = text;

        // If we're clearing the text, hide it immediately.
        if (string.IsNullOrEmpty(text))
        {
            Color clearColor = Text.color;
            clearColor.a = 0f;
            Text.color = clearColor;
            return;
        }

        // Make the new text visible.
        Color color = Text.color;
        color.a = 1f;
        Text.color = color;

        // Start a fresh fade.
        fadeCoroutine = StartCoroutine(FadeText(2f, 0f));
    }

    private IEnumerator FadeText(float duration, float targetAlpha)
    {
        float startAlpha = Text.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float blend = Mathf.Clamp01(time / duration);

            Color color = Text.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, blend);
            Text.color = color;

            yield return null;
        }

        // Make sure the final alpha is correct.
        Color finalColor = Text.color;
        finalColor.a = targetAlpha;
        Text.color = finalColor;

        // Clear the text after fading out.
        if (targetAlpha <= 0f)
        {
            Text.text = "";
        }

        fadeCoroutine = null;
    }
}