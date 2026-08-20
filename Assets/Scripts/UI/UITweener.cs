using System.Collections;
using System.Collections.Generic;
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

    [Header("Wait For")]
    [Tooltip("If set, this UITweener will wait for the referenced UITweener to finish any running tweens before firing its OnEnable event.")]
    [SerializeField] private UITweener WaitFor;

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
    private readonly List<Tween> activeTweens = new List<Tween>();
    private bool isWaitingForDependency = false;

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
        if (WaitFor != null)
        {
            StartCoroutine(WaitForThenInvokeVisible());
        }
        else
        {
            onVisibleEvent?.Invoke();
        }
    }

    private IEnumerator WaitForThenInvokeVisible()
    {
        // Mark that we're waiting on a dependency so anything waiting on US
        // (nested WaitFor chains) knows to keep waiting too.
        isWaitingForDependency = true;

        // Wait a frame so the WaitFor tweener (if it is also enabling right now) has a chance to start its tweens
        yield return null;

        // IsBusy() checks both the WaitFor tweener's own tweens AND whether IT
        // is waiting on a further nested WaitFor, so chains of any length resolve correctly.
        while (WaitFor != null && WaitFor.IsBusy())
        {
            yield return null;
        }

        isWaitingForDependency = false;
        onVisibleEvent?.Invoke();
    }

    /// <summary>
    /// Returns true if this UITweener currently has any running (active and playing) tweens.
    /// </summary>
    public bool IsAnimating()
    {
        for (int i = activeTweens.Count - 1; i >= 0; i--)
        {
            Tween t = activeTweens[i];
            if (t == null || !t.IsActive())
            {
                activeTweens.RemoveAt(i);
                continue;
            }

            if (t.IsPlaying())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if this UITweener is either currently animating, or is itself
    /// still waiting on a (possibly nested) WaitFor dependency. Anything waiting on
    /// this UITweener should treat this as "still busy".
    /// </summary>
    public bool IsBusy()
    {
        return isWaitingForDependency || IsAnimating();
    }

    private void TrackTween(Tween tween)
    {
        activeTweens.Add(tween);
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
        TrackTween(group.DOFade(target, fadeSettings.duration));
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

        TrackTween(rect.DOScaleX(targetx, scaleSettings.duration));
        TrackTween(rect.DOScaleY(targety, scaleSettings.duration));
        TrackTween(rect.DOScaleZ(targetz, scaleSettings.duration));
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

        TrackTween(rect.DOMove(new Vector3(targetx,targety,targetz), slideSettings.duration));
    }

    public void Punch()
    {
        TrackTween(rect.DOPunchScale(punchSettings.Punch, punchSettings.duration, punchSettings.vibrato, punchSettings.elasticity));
    }

    public void ShakeScale()
    {
        TrackTween(rect.DOShakeScale(shakeSettings.duration, shakeSettings.shake,shakeSettings.vibrato,shakeSettings.randomness,shakeSettings.fadeOut,shakeSettings.randomnessMode));
    }

    public void ShakeRotation()
    {
        TrackTween(rect.DOShakeRotation(shakeSettings.duration, shakeSettings.shake,shakeSettings.vibrato,shakeSettings.randomness,shakeSettings.fadeOut,shakeSettings.randomnessMode));
    }

}