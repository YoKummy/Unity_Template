using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

public class DialogueVariables
{
    public Dictionary<string, Ink.Runtime.Object> variables {get; private set;}

    public DialogueVariables(TextAsset loadGlobalJSON)
    {
        Story globalVariableStory = new Story(loadGlobalJSON.text);

        variables = new Dictionary<string, Ink.Runtime.Object>();
        foreach(string name in globalVariableStory.variablesState)
        {
            Ink.Runtime.Object value = globalVariableStory.variablesState.GetVariableWithName(name);
            variables.Add(name, value);
            Debug.Log(name + "=" + value);
        }
    }

    public void StartListening(Story story)
    {
        VariablesToStory(story);
        story.variablesState.variableChangedEvent += VariableChange;
    }

    public void StopListening(Story story)
    {
        story.variablesState.variableChangedEvent -= VariableChange;
    }

    private void VariableChange(string name, Ink.Runtime.Object value)
    {
        Debug.Log("V changed: " + name + "=" + value);

        if(variables.ContainsKey(name))
        {
            variables.Remove(name);
            variables.Add(name, value);
        }
    }

    private void VariablesToStory(Story story)
    {
        foreach(KeyValuePair<string, Ink.Runtime.Object> variable in variables)
        {
            story.variablesState.SetGlobal(variable.Key, variable.Value);
        }
    }
    public void SyncToStory(Story targetStory)
    {
        VariablesToStory(targetStory);
    }

    public void UpdateFromStory(Story sourceStory)
    {
        foreach (string name in sourceStory.variablesState)
        {
            Ink.Runtime.Object value = sourceStory.variablesState.GetVariableWithName(name);
            if (variables.ContainsKey(name))
            {
                variables[name] = value;
            }
            else
            {
                variables.Add(name, value);
            }
        }
    }

}
