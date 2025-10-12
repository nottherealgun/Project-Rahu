using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleGuidePrompt : SerializedMonoBehaviour
{
    [OdinSerialize] GameObject guideContainer;
    void Start()
    {
        UIManager.Instance.AddUILayer("GuidePrompt");
        Time.timeScale = 0f;
    }

    public void OnBack(InputValue value)
    {
        UIManager.Instance.CloseMenu();
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
