using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class QTETester : SerializedMonoBehaviour
{
    [OdinSerialize] QTEPrompt qtePrompt;
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

        // UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.QTE);
        // qtePrompt?.gameObject.SetActive(true);
    }
}
