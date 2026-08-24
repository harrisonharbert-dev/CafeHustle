using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public class menuContent {
        public GameObject leftPage;
        public GameObject rightPage;
    }


    [SerializeField] private menuAnimation menuAnimations;

    [SerializeField] private GameObject menu;


    public SerializableDictionary<string,menuContent> menuPages;





    private bool isMenuOpen = false;

    void Awake()
    {
        menu.SetActive(false);
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
            playAnimation(menuAnimations.openAnimation);
            isMenuOpen = true;
        }
    }

    public void onOpenToPage(string id)
    {
        if(!isMenuOpen)
        {
            playAnimation(menuAnimations.openAnimation);
            isMenuOpen = true;
            StartCoroutine(id,0f);
        }
    }

    public void onClose()
    {
        if (isMenuOpen)
        {
            playAnimation(menuAnimations.closeAnimation);
            isMenuOpen = false;
        }
    }

    public void onNewPage(string id)
    {
        if(isMenuOpen)
        {
            playAnimation(menuAnimations.newPageAnimation);
            StartCoroutine(LoadNewPageContent(id, 0.33f));
        }
    }

    private IEnumerator LoadNewPageContent(string id,float pageDelay)
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
}
