using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PuzzleGuidePrompt : SerializedMonoBehaviour
{
    public UnityEvent onClosePrompt; 
    void Start()
    {
        UIManager.Instance.AddUILayer("GuidePrompt");
    }

    public void OnBack(InputValue value)
    {
        UIManager.Instance.CloseMenu();
        gameObject.SetActive(false);
        onClosePrompt?.Invoke();
    }
}
