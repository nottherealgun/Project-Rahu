using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class PromptContainer : SerializedMonoBehaviour
{
    [OdinSerialize] Dictionary<string, GameObject> prompts = new Dictionary<string, GameObject>();
    [OdinSerialize] Dictionary<string, bool> promptActivation = new Dictionary<string, bool>
    {
        {"leave", false },
        {"back", false }
    };

#if UNITY_EDITOR
    void OnValidate()
    {
        foreach (var key in prompts.Keys)
        {
            if (prompts[key] != null)
            {
                prompts[key].SetActive(promptActivation[key]);
            }
        }
    }
#endif
}