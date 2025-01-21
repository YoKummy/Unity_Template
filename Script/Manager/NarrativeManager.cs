using System.Collections;
using Ink.Runtime;
using TMPro;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
    private static NarrativeManager instance;
    [Header("NarrativeUI")]
    [SerializeField] private GameObject bottomUI;
    [SerializeField] private TextMeshProUGUI bottomText;
    [SerializeField] private TextAsset narrativeJSON;
    [SerializeField] private TextAsset loadGlobalJSON;
    private bool hasEnteredNarrative = false;
    private Story currentNarrative;
    private DialogueVariables dialogueVariables;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Warning: More than one instance of DialogueManager found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        dialogueVariables = new DialogueVariables(loadGlobalJSON);
    }

    public static NarrativeManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("DialogueManager instance is null! Ensure one exists in the scene.");
        }
        return instance;
    }

    private void Start()
    {
        bottomUI.SetActive(false);
    }

    public void EnterNarrative()
    {
        if (hasEnteredNarrative) return;

        currentNarrative = new Story(narrativeJSON.text);

        // Sync variables from DialogueVariables to this narrative story
        DialogueManager.GetInstance().dialogueVariables.SyncToStory(currentNarrative);

        bottomUI.SetActive(true);
        hasEnteredNarrative = true;

        StartCoroutine(ContinueNarrative(5f));
    }


    public void ExitNarrative()
    {
        if (!hasEnteredNarrative) return;
    
        // Sync variables from this narrative story back to DialogueVariables
        DialogueManager.GetInstance().dialogueVariables.UpdateFromStory(currentNarrative);
    
        bottomUI.SetActive(false);
        hasEnteredNarrative = false;
    }


    private IEnumerator ContinueNarrative(float duration)
    {
        while (currentNarrative.canContinue)
        {
            bottomText.text = currentNarrative.Continue();
            yield return new WaitForSeconds(duration);
        }

        ExitNarrative();
    }

    public Ink.Runtime.Object GetVariableState(string variableName)
    {
        Ink.Runtime.Object variableValue = null;
        dialogueVariables.variables.TryGetValue(variableName, out variableValue);
        if(variableValue == null)
        {
            Debug.Log("Ink variable was found to be null:" + variableName);
        }
        return variableValue;
    }

    public void SetVariableState(string variableName, object value)
    {
        currentNarrative = new Story(narrativeJSON.text);
        if (currentNarrative == null)
        {
            Debug.LogError("currentNarrative is null!");
            return;
        }

        if (currentNarrative.variablesState.GlobalVariableExistsWithName(variableName))
        {
            currentNarrative.variablesState[variableName] = value;
        }
        else
        {
            Debug.LogWarning($"Ink variable '{variableName}' does not exist.");
        }
    }

}