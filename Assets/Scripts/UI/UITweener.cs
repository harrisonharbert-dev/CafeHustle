using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.Events;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using UnityEngine.EventSystems;


[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class UITweener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private GameObject objectToAnimate;
    
     public enum tweenIn
    {
        none,
        fadeIn,
        scaleIn,
        fadeAndScaleIn,
        fadeAndSlideIn,
    }
    
    [System.Serializable]
    public struct FadeSettings
    {
        public float duration;
    }
    
    

    [System.Serializable]
    public struct ScaleSettings
    {
        public float duration;
    }
    

    [System.Serializable] 
    public struct PunchSettings
    {
        public Vector3 Punch;
        public float duration;
        public int vibrato;
        public float elasticity;
    }

      [System.Serializable] 
    public struct ShakeSettings
    {
        public Vector3 shake;
        public float duration;
        public int vibrato;
        public float randomness;
        public bool fadeOut;
        public ShakeRandomnessMode randomnessMode;


    }

    [System.Serializable]
    public struct SlideSettings
    {
        public Vector3 distance;
        public float duration;


    }
    

    [Header("Animation Settings")]
    [SerializeField] private FadeSettings fadeSettings;
    [SerializeField] private ScaleSettings scaleSettings;
    [SerializeField] private PunchSettings punchSettings;
    [SerializeField] private ShakeSettings shakeSettings;
    [SerializeField] private SlideSettings slideSettings;
    

    //private references
    private CanvasGroup group;
    private RectTransform rect;
    private Vector3 scale;
    private Vector3 startingPos;

    [SerializeField] private UnityEvent onVisibleEvent;
    [SerializeField] private UnityEvent onDisableEvent;
    [SerializeField] private UnityEvent onHoverEvent;
    [SerializeField] private UnityEvent onLeaveEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (objectToAnimate == null)
        {
            objectToAnimate = gameObject;
            group = GetComponent<CanvasGroup>();
            rect = GetComponent<RectTransform>();
            scale = transform.localScale;
            startingPos = rect.position;
        } else
        {
            group = objectToAnimate.GetComponent<CanvasGroup>();
            rect = objectToAnimate.GetComponent<RectTransform>();
            scale = objectToAnimate.transform.localScale;
            startingPos = objectToAnimate.transform.position;
        }

        
        
    }


    void OnEnable()
    {
        onVisibleEvent?.Invoke();
    }

    void OnDisable() {
        onDisableEvent?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onLeaveEvent?.Invoke();
    }

    public void Fade (bool option)
    {
        float target = option ? 1f : 0f;
        float start = option ? 0f : 1f;

        group.alpha = start;
        group.DOFade(target, fadeSettings.duration);
    }

    public void Scale(bool option)
    {
        //Set target to scale to
        float targetx = option ? scale.x : 0f;
        float targety = option ? scale.y : 0f;
        float targetz = option ? scale.z : 0f;

        //Set the scale to opposite of target when starting
        float startx = option ? 0f : scale.x;
        float starty = option ? 0f : scale.y;
        float startz = option ? 0f : scale.z;

        rect.localScale = new Vector3 (startx,starty,startz);

        rect.DOScaleX(targetx, scaleSettings.duration);
        rect.DOScaleY(targety, scaleSettings.duration);
        rect.DOScaleZ(targetz, scaleSettings.duration);
    }

    public void Slide(bool option)
    {
         //Set target to scale to
        float targetx = option ? startingPos.x : startingPos.x+slideSettings.distance.x;
        float targety = option ? startingPos.y : startingPos.y+slideSettings.distance.y;
        float targetz = option ? startingPos.z : startingPos.z+slideSettings.distance.z;

        //Set the scale to opposite of target when starting
        float startx = option ? startingPos.x + slideSettings.distance.x : startingPos.x;
        float starty = option ? startingPos.y + slideSettings.distance.y : startingPos.y;
        float startz = option ? startingPos.z + slideSettings.distance.z : startingPos.z;

        rect.position = new Vector3(startx,starty,startz);

        rect.DOMove(new Vector3(targetx,targety,targetz), slideSettings.duration);
    }

    public void Punch()
    {
        rect.DOPunchScale(punchSettings.Punch, punchSettings.duration, punchSettings.vibrato, punchSettings.elasticity);
    }

    public void ShakeScale()
    {
        rect.DOShakeScale(shakeSettings.duration, shakeSettings.shake,shakeSettings.vibrato,shakeSettings.randomness,shakeSettings.fadeOut,shakeSettings.randomnessMode);
    }

    public void ShakeRotation()
    {
        rect.DOShakeRotation(shakeSettings.duration, shakeSettings.shake,shakeSettings.vibrato,shakeSettings.randomness,shakeSettings.fadeOut,shakeSettings.randomnessMode);
    }

}
