using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleGameManager : SerializedMonoBehaviour
{
    [OdinSerialize] PuzzleGuidePrompt guide;
    [OdinSerialize] string puzzleSpecificPromptString = "";
    void Start()
    {
        UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.CUSTOM,"leave");
    }

    public void OnEnable()
    {
        if(guide.gameObject.activeInHierarchy == false)
            UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.CUSTOM,puzzleSpecificPromptString);
    }

    public void OnBack(InputValue value)
    {
        PersistentDataManager.Player.GetComponent<PlayerController>().OnExitInteraction(value);
    }
}
