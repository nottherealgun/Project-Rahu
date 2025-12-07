using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class InteractionPromptHUD : SerializedMonoBehaviour
{
    [OdinSerialize]
    Dictionary<string, GameObject> prompts = new Dictionary<string, GameObject>();
    [OdinSerialize, Required] GameObject promptContainer;
    bool resetPrompts = true;

    [Button(ButtonSizes.Large)]
    void PrintPrompt()
    {
        Debug.Log($"{prompts.Count} prompts registered.");
        foreach (string str in prompts.Keys)
        {
            Debug.Log(str);
        }
        Debug.Log($"==============");
    }

    void OnValidate()
    {
        if (promptContainer == null) return;
        for (int idx = 0; idx < promptContainer.transform.childCount; idx++)
        {
            GameObject prompt = promptContainer.transform.GetChild(idx).gameObject;
            string promptName = prompt.name.Split("Prompt")[0];
            promptName = promptName.ToLower();

            if (prompts.Keys.Contains(promptName)) continue;

            if (prompt.TryGetComponent<Prompt>(out Prompt _prompt) == false)
            {
                // Prompt object has no prompt script
                Debug.LogError("Prompt object has no prompt script; Check InteractionPromptHUD");
                continue;
            }

            prompts.Add(promptName, prompt);
        }
    }

    public void DisablePrompts()
    {
        foreach(string promptName in prompts.Keys)
        {
            if (prompts[promptName].activeInHierarchy == false) continue;
            prompts[promptName].SetActive(false);
        }
        resetPrompts = true;
    }
    
    public void EnablePrompts(UIManager.InteractionHUDPreset preset = UIManager.InteractionHUDPreset.DEFAULT, string customInteractionString = "")
    {
        if(resetPrompts == false)
        {
            DisablePrompts();
        }
        resetPrompts = false;
        string promptsToTurnOn = "";
        switch (preset)
        {
            case UIManager.InteractionHUDPreset.INTERACTABLE:
                promptsToTurnOn = "leave";
                break;
            case UIManager.InteractionHUDPreset.ROTATABLE:
                promptsToTurnOn = "leave,rotate,resetrotation";
                break;
            case UIManager.InteractionHUDPreset.CHOICE:
                promptsToTurnOn = "selectleft,selectright";
                break;
            case UIManager.InteractionHUDPreset.QTE:
                break;
            case UIManager.InteractionHUDPreset.PUZZLE:
                promptsToTurnOn = "interact";
                break;
            case UIManager.InteractionHUDPreset.CUSTOM:
                promptsToTurnOn = customInteractionString;
                break;
            default:
                // IntereactionHUDPreset.DEFAULT
                break;
        }

        if (promptsToTurnOn == "")
        {
            return;
        }

        string[] splitPromptsArray = promptsToTurnOn.Split(",");
        foreach(string promptName in splitPromptsArray)
        {
            prompts[promptName].SetActive(true);
        }
    }

    public void SyncPromptIcons(CurrentActiveDeviceManager.ActiveDevice activeDevice)
    {
        foreach(GameObject promptObj in prompts.Values)
        {
            Prompt _prompt = promptObj.GetComponent<Prompt>();
            _prompt.SetPromptMode( (Prompt.PromptMode) activeDevice);
        }
    }
}
