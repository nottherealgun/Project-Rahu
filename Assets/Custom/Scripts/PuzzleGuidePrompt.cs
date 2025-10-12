using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleGuidePrompt : SerializedMonoBehaviour
{
    void Start()
    {
        UIManager.Instance.AddUILayer("GuidePrompt");
    }

    public void OnBack(InputValue value)
    {
        UIManager.Instance.CloseMenu();
        gameObject.SetActive(false);
    }
}
