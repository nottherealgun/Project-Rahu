using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class ChoiceTester : SerializedMonoBehaviour
{
    [OdinSerialize] ChoicePrompt choicePrompt;
    void Start()
    {
        StartTest();
    }
    public async void StartTest()
    {
        await UniTask.Delay(2000);

        await NarrativeManager.Instance.PlayCutsceneSequence(true);

        UIManager.LockCursor(true);
        await UIManager.Instance.ManualFadeOut();

        PersistentDataManager.Instance.FindPlayer();
        await NarrativeManager.Instance.LateSetup();

        UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.CHOICE);
        choicePrompt?.gameObject.SetActive(true);
    }
}
