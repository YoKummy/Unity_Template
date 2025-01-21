using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;

    [Header("DialogueUI")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI dialogueText;
    public Story currentStory;

    [Header("ChoiceUI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Sound")] 
    [SerializeField] private AudioClip typingSfx;
    private AudioSource audioSource;
    [SerializeField] private bool stopAudioSource;
    [Range(1, 5)]
    [SerializeField] private int frequencyLevel = 2;
    [Range(-3, 3)]
    [SerializeField] private float minPitch = 0.5f;
    [Range(-3, 3)]
    [SerializeField] private float maxPitch = 3f;
    
    [Header("Other")]
    [SerializeField] private float typingSpeed = 0.04f;
    private Coroutine displayLineCoroutine;
    public bool DialogueisPlaying{get; private set;}
    private bool inputEnabled = false;
    private bool canContinueToNextLine = false;

    [Header("Ref")]
    public DialogueVariables dialogueVariables;
    [SerializeField] private TextAsset loadGlobalJSON;

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
        audioSource = this.gameObject.AddComponent<AudioSource>();
        dialogueVariables = new DialogueVariables(loadGlobalJSON);
    }

    public static DialogueManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("DialogueManager instance is null! Ensure one exists in the scene.");
        }
        return instance;
    }

    private void Start()
    {
        DialogueisPlaying = false;
        dialogueUI.SetActive(false);

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        if (!DialogueisPlaying)
        {
            return;
        }
        
        if (canContinueToNextLine 
            && Mouse.current.leftButton.wasPressedThisFrame
            && currentStory.currentChoices.Count == 0)
        {
            ContinueStory();
        }
    }

    public void EnterDialogue(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        DialogueisPlaying = true;
        dialogueUI.SetActive(true);
    
        // Sync variables to the new story
        dialogueVariables.StartListening(currentStory);
    
        StartCoroutine(EnableInputAfterDelay(0.5f));
        ContinueStory();
    }


    private IEnumerator ExitDialogue()
    {
        yield return new WaitForSeconds(0.2f);
        dialogueVariables.StopListening(currentStory);
        DialogueisPlaying = false;
        dialogueUI.SetActive(false);
        dialogueText.text = "";
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            if(displayLineCoroutine != null)
            {
                StopCoroutine(displayLineCoroutine);
            }

            displayLineCoroutine = StartCoroutine(DisplayLine(currentStory.Continue()));
            
        }
        else
        {
            StartCoroutine(ExitDialogue());
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if(currentChoices.Count > choices.Length)
        {
            Debug.LogError("ChoiceError, num of choice" 
                + currentChoices.Count);
        }

        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }

        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }


    public void MakeChoice(int choiceIndex)
    {
        if (!inputEnabled) return;
        
        if(canContinueToNextLine)
        {
            currentStory.ChooseChoiceIndex(choiceIndex);
            ContinueStory();
        }
        
    }

    private IEnumerator EnableInputAfterDelay(float delay)
    {
        inputEnabled = false;
        yield return new WaitForSeconds(delay);
        inputEnabled = true;
    }

    private void HideChoices()
    {
        foreach(GameObject choiceBtn in choices)
        {
            choiceBtn.SetActive(false);
        }
    }

    private void PlayDialogueSound(int currentDisplayedCharacter)
    {
        if(currentDisplayedCharacter % frequencyLevel == 0)
        {
            /* if(stopAudioSource)
            {
                audioSource.Stop();
            } */
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(typingSfx);
        }
    }

    private IEnumerator DisplayLine(string line)
    {
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        canContinueToNextLine = false;
        HideChoices();


        foreach(char letter in line.ToCharArray())
        {
            /* if(Mouse.current.leftButton.wasPressedThisFrame)
            {
                dialogueText.maxVisibleCharacters = line.Length;
                break;
            } */

            PlayDialogueSound(dialogueText.maxVisibleCharacters);
            dialogueText.maxVisibleCharacters ++;
            yield return new WaitForSeconds(typingSpeed);
        }
        DisplayChoices();
        canContinueToNextLine = true;
    }
    
    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
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
    public static bool TrySetInkStoryVariable( Story story, string variable, object value, bool log = true )
    {
        if( story != null &&
            story.variablesState.GlobalVariableExistsWithName( variable ) )
        {
            if( log )
            {
                Debug.Log( $"[Ink] Set variable: {variable} = {value}" );
            }
            story.variablesState[variable] = value;
            return true;
        }
        return false;
    }
}