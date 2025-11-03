using UnityEngine;
using System.Collections;

using TMPro;
public class locationUIController : MonoBehaviour
{
    [Header("Enter Location Settings")]
    
    public TextMeshProUGUI locationTextObject; [Tooltip("The text pop-up when player goes to new location")]
    public static string currentLocation; //current location to be displayed
    public string mainLocation; [Tooltip("The main location player is in - eg. Tomi City")]

    [Header("Animation Settings")]

    
    public float moveDistance = 100f; [Tooltip("How far (in local UI units) the text starts above its resting position")]
    
    public float fadeDuration = 0.5f; [Tooltip("Time for fade-in and slide-in")]
    
    public float lingerDuration = 2f; [Tooltip("How long the text stays visible before leaving")]
    
    public float leaveDuration = 0.5f; [Tooltip("Time for fade-out and slide-out")]



    private Coroutine currentRoutine; //Play location popup routine
    public CanvasGroup currentCanvasGroup; [Tooltip("Reference to canvas group on the Location Text UI object")]//Canvas group to fade in and out the text

    private RectTransform textRect; // RectTransform of the text ui
    private Vector3 originalPos; // Original text position

    private void Start()
    {
        if (currentCanvasGroup == null) return;
        currentCanvasGroup.alpha = 0f;
        //hide text on start

        textRect = locationTextObject.GetComponent<RectTransform>();
        originalPos = textRect.anchoredPosition;
    }
    public void OnEnterLocation(string location)
    {
        //combine the two strings for the location text and set it
        string displayLocation = location+ ", " + mainLocation;
        locationTextObject.text = displayLocation;

        //play coroutine to show the text.
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(PlayLocationPopup());


    }

    private IEnumerator PlayLocationPopup()
    {
        //Reset Position
        textRect.anchoredPosition = originalPos;
        // Store positions
        Vector3 startPos = originalPos + Vector3.up * moveDistance; // starts above

        //Reset to starting state
        textRect.anchoredPosition = startPos;
        currentCanvasGroup.alpha = 0f;

        // --- Fade & Slide in ---

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed+= Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);

            textRect.anchoredPosition = Vector3.Lerp(startPos, originalPos, t);
            currentCanvasGroup.alpha = t;

            yield return null;
        }

        textRect.anchoredPosition = originalPos;
        currentCanvasGroup.alpha = 1f;

        //Linger on-screen

        yield return new WaitForSeconds(lingerDuration);

        // Fade out 

        elapsed = 0f;
        while (elapsed < leaveDuration)
        {
            elapsed+= Time.deltaTime;
            float t = Mathf.SmoothStep(0f,1f, elapsed / leaveDuration);

            textRect.anchoredPosition = Vector3.Lerp(originalPos, startPos, t);
            currentCanvasGroup.alpha = 1f - t;

            yield return null;
        }

        //hide
        textRect.anchoredPosition = originalPos;
        currentCanvasGroup.alpha = 0f;

    }


}
