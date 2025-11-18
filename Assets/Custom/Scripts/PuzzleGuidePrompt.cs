using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleGuidePrompt : SerializedMonoBehaviour
{
    void Start()
    {
        UIManager.Instance.AddUILayer("GuidePrompt");
        UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.INTERACTABLE);
    }

    public void OnBack(InputValue value)
    {
        UIManager.Instance.CloseMenu();
        gameObject.SetActive(false);

        // UIManager.Instance.DisableInteractionHUD();
        UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.PUZZLE);

    }
}
