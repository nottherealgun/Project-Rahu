using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.Serialization;

public class PasscodePanel : SerializedMonoBehaviour
{
    [Title("Passcode Numbers")]
    [OdinSerialize, ReadOnly] string enteredCode = "";
    int currentDigit = 0;

    [OdinSerialize, SceneObjectsOnly]
    Dictionary<string, List<Sprite>> numbers = new Dictionary<string, List<Sprite>>()
    {
        {"Purple", new List<Sprite>{} },
        {"Green", new List<Sprite>{} },
        {"Yellow", new List<Sprite>{} },
        {"Red", new List<Sprite>{} }
    };
    List<string> colorOrder = new List<string>() { "Purple", "Green", "Yellow", "Red" };

    [OdinSerialize, SceneObjectsOnly] List<Image> numberImages = new List<Image>();
    [OdinSerialize] GameObject screenBlocker;
    public UnityEvent onUnlocked;
    [OdinSerialize] Animator doorController;
    [OdinSerialize] bool fusedReplaced = false;

    void Start()
    {
        GameManager.Instance.onPasscodePanelUnlocked += TurnOffPanel;
    }

    void OnDestroy()
    {
        GameManager.Instance.onPasscodePanelUnlocked -= TurnOffPanel;
    }

    public void TurnOffPanel()
    {
        screenBlocker.SetActive(false);
    }

    public void Prepare()
    {
        PlayerController playerController = PersistentDataManager.Player.GetComponent<PlayerController>();
        onUnlocked.AddListener(() =>
        {
            playerController.ForceExitInteraction();
            playerController.DisconnectFromInteractingObject();
        });
    }

    // public void Close()
    // {
    //     onUnlocked.RemoveAllListeners();
    // }

    public void OnPanelButtonPressed(int number)
    {
        EnvironmentalAudioManager.Instance.PlaySFX("passcode_button_pressed");

        if (number == -1)
        {
            if (enteredCode.Length > 0) enteredCode = enteredCode.Substring(0, enteredCode.Length - 1);
            if (currentDigit > 0) currentDigit--;
            numberImages[currentDigit].sprite = null;
            return;
        }
        else if (enteredCode.Length == 4)
        {
            ResetNumberImages();
            enteredCode = "";
            currentDigit = 0;
        }

        enteredCode += number.ToString();
        numberImages[currentDigit].sprite = numbers[colorOrder[currentDigit]][number];
        currentDigit++;

        UnlockIfPasscodeCorrect();
    }

    void ResetNumberImages()
    {
        foreach (Image img in numberImages)
        {
            img.sprite = null;
        }
    }

    public bool UnlockIfPasscodeCorrect()
    {
        if (enteredCode.Length != 4) return false;
        if (enteredCode == "1584")
        {
            onUnlocked?.Invoke();
            doorController.SetTrigger("DoorUnlocked");
            UIManager.LockCursor(true);
            EnvironmentalAudioManager.Instance.PlaySFX("passcode_correct");

            if (PersistentDataManager.Instance.HasEventPassed("passcodeTerminalAccessed")) return true;
            NarrativeManager.Instance.CharacterSpeak("SC12_Fasai_Safe_Finish");
            PersistentDataManager.Instance.MarkEventAsPassed("passcodeTerminalAccessed");
            return true;
        }
        EnvironmentalAudioManager.Instance.PlaySFX("passcode_wrong");
        return false;
    }

    [Button(ButtonSizes.Large)]
    void ForceUnlock()
    {
        enteredCode = "1584";
        UnlockIfPasscodeCorrect();
    }

    public void CheckFuse()
    {
        if (PersistentDataManager.Instance.HasEventPassed("foundFuse"))
        {
            NarrativeManager.Instance.CharacterSpeak("SC12_Fasai_DoorPin_FuseReplaced");
        }
        else if (!PersistentDataManager.Instance.HasEventPassed("checkedFuse"))
        {
            NarrativeManager.Instance.CharacterSpeak("SC12_Fasai_Random_FuseBlown");
            PersistentDataManager.Instance.MarkEventAsPassed("checkedFuse");
        }
    }
}
