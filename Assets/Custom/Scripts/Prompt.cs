using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class Prompt : SerializedMonoBehaviour
{
    [OdinSerialize] GameObject keyboardIcon;
    [OdinSerialize] GameObject dualshockIcon;
    [OdinSerialize] GameObject xboxIcon;
    public enum PromptMode { Keyboard, Xbox, DualShock }
    public void SetPromptMode(PromptMode promptMode)
    {
        keyboardIcon.SetActive(false);
        dualshockIcon.SetActive(false);
        xboxIcon.SetActive(false);

        switch (promptMode)
        {
            case PromptMode.Keyboard:
                keyboardIcon.SetActive(true);
                break;
            case PromptMode.Xbox:
                xboxIcon.SetActive(true);
                break;
            case PromptMode.DualShock:
                dualshockIcon.SetActive(true);
                break;
        }
    }
}
