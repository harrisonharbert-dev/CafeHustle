using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using GLTFast.Schema;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using Yarn.Unity;


public class MenuHandler : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    [System.Serializable]
    public class menuAnimation
    {
        public PlayableAsset openAnimation;
        public PlayableAsset closeAnimation;
        public PlayableAsset newPageAnimation;
    }

    [System.Serializable]
    public class menuContent
    {
        public GameObject leftPage;
        public GameObject rightPage;
    }


    [SerializeField] private menuAnimation menuAnimations;

    [SerializeField] private GameObject menu;

    [SerializeField] private CanvasGroup BG;
    [SerializeField] private float BGFadeDuration = 0.3f;


    public SerializableDictionary<string, menuContent> menuPages;

    [SerializeField] private UnityEvent onOpenEvent;
    [SerializeField] private UnityEvent onCloseEvent;
    [SerializeField] private UnityEvent onNewPageEvent;





    private bool isMenuOpen = false;

    void Awake()
    {
        menu.SetActive(false);
        BG.alpha = 0f;
    }

    void playAnimation(PlayableAsset asset)
    {
        director.playableAsset = asset;
        director.Play();
    }

    public void onOpen()
    {
        if (!isMenuOpen)
        {
            PlayerInputController.instance.SetMovementLock(true);
            currentTaskManager.instance.onTaskVisibility(false);

            BG.DOFade(1f, BGFadeDuration);

            playAnimation(menuAnimations.openAnimation);
            isMenuOpen = true;
            onOpenEvent?.Invoke();
        }
    }

    public void onOpenToPage(string id)
    {
        if (!isMenuOpen)
        {
            PlayerInputController.instance.SetMovementLock(true);
            currentTaskManager.instance.onTaskVisibility(false);

            BG.DOFade(1f, BGFadeDuration);

            playAnimation(menuAnimations.openAnimation);
            isMenuOpen = true;
            StartCoroutine(LoadNewPageContent(id, 0f));
            onOpenEvent?.Invoke();
        }
    }

    public void onClose()
    {
        if (isMenuOpen)
        {
            PlayerInputController.instance.SetMovementLock(false);
            currentTaskManager.instance.onTaskVisibility(true);

            BG.DOFade(0f,BGFadeDuration);

            playAnimation(menuAnimations.closeAnimation);
            isMenuOpen = false;
            onCloseEvent?.Invoke();
        }
    }

    public void onNewPage(string id)
    {
        if (isMenuOpen)
        {
            playAnimation(menuAnimations.newPageAnimation);
            StartCoroutine(LoadNewPageContent(id, 0.33f));
            onNewPageEvent?.Invoke();
        }
    }

    private IEnumerator LoadNewPageContent(string id, float pageDelay)
    {
        if (menuPages == null) yield break;


        yield return new WaitForSeconds(pageDelay);
        // Activate left/right GameObjects for the target page and deactivate others
        foreach (var kvp in menuPages)
        {
            bool isTarget = kvp.Key == id;
            var content = kvp.Value;
            if (content == null)
                continue;

            if (content.leftPage != null)
                content.leftPage.SetActive(isTarget);

            if (content.rightPage != null)
                content.rightPage.SetActive(isTarget);
        }
    }


    public void closeMenu(InputAction.CallbackContext context)
    {
        if (context.performed && isMenuOpen)
        {
            onClose();


        }
    }
}
