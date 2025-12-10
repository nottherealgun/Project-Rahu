using System.Collections;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CreditsMenu : SerializedMonoBehaviour
{
    [OdinSerialize] ScrollRect scrollRect;
    bool isVisible = false;
    public void Show()
    {
        isVisible = true;
        UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.CUSTOM, "leave");
        Tween.Alpha(GetComponent<CanvasGroup>(), 0f, 1f, 3f);
    }

    public void Hide()
    {
        isVisible = false;
        Tween.StopAll(this);
        UIManager.Instance.DisableInteractionHUD();
        Tween.Alpha(GetComponent<CanvasGroup>(), 1f, 0f, 1f).OnComplete(() =>
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        });
    }

    void OnBack(InputValue value)
    {
        if (gameObject.activeSelf && value.isPressed && isVisible)
        {
            Hide();
        }
    }

    public void ShowScroll()
    {
        UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.CUSTOM, "leave,scroll");
    }
}
