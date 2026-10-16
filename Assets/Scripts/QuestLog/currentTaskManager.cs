using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class currentTaskManager : MonoBehaviour
{
    public static currentTaskManager instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject sideObjectivePrefab;
    [SerializeField] private List<QuestItem> questItems;
    [SerializeField] private QuestItem currentQuestItem;
    [SerializeField] private TextMeshProUGUI mainObjectiveTextObject;
    private readonly List<GameObject> instantiatedSideObjectives = new List<GameObject>();

    [SerializeField] private float typewriterDuration = 1f;


    [SerializeField] private UnityEvent onUpdateTask;
    [SerializeField] private UnityEvent onShowTask;
    [SerializeField] private UnityEvent onHideTask;

    private readonly object taskManagerID = new object();

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        InitializeQuest();
    }
    // Update is called once per frame
    public void LoadNewQuestByName(string name)
    {
        //Find quest with matching ID in the list
        var match = questItems.Find(p => p != null && p.questID == name);
        if (match == null)
        {
            return;
        }

        currentQuestItem = match;
        InitializeQuest();
    }

    void InitializeQuest()
    {
        foreach (var sideObjective in instantiatedSideObjectives)
        {
            if (sideObjective != null)
            {
                Destroy(sideObjective);
            }
        }
        instantiatedSideObjectives.Clear();

        mainObjectiveTextObject.text = currentQuestItem.mainObjectiveText;

        foreach (var side in currentQuestItem.sideObjectives)
        {
            GameObject sideObjective = Instantiate(sideObjectivePrefab, container.transform);
            instantiatedSideObjectives.Add(sideObjective);
            TextMeshProUGUI sideObjectiveText = sideObjective.GetComponent<TextMeshProUGUI>();
            if (sideObjectiveText != null)
            {
                sideObjectiveText.text = side.objectiveText;
            }

            if (side.isHiddenOnStart == true)
            {
                sideObjective.SetActive(false);
            } else
            {
                sideObjective.SetActive(true);
            }
        }        
    }

    public void RevealSideObjective(int id)
    {
        if (id < 0 || id >= instantiatedSideObjectives.Count)
        {
            return;
        }

        GameObject sideObjective = instantiatedSideObjectives[id];
        if (sideObjective == null)
        {
            return;
        }

        sideObjective.SetActive(true);
    }
    public void CompleteSideObjective(int id)
    {
        if (id < 0 || id >= instantiatedSideObjectives.Count)
        {
            return;
        }

        GameObject sideObjective = instantiatedSideObjectives[id];
        if (sideObjective == null)
        {
            return;
        }

        TextMeshProUGUI sideObjectiveText = sideObjective.GetComponent<TextMeshProUGUI>();
        if (sideObjectiveText != null)
        {
            sideObjectiveText.fontStyle |= FontStyles.Strikethrough;
            sideObjectiveText.ForceMeshUpdate();
        }
    }
    public void onTaskVisibility(bool option)
    {
        if (option == true)
        {
            onShowTask?.Invoke();
        }
        else if (option == false)
        {
            onHideTask?.Invoke();
        }
    }
}
