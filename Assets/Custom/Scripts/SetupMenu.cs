using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SetupMenu : Menu
{
    [OdinSerialize] Button keyboardButton;
    [OdinSerialize] Button controllerButton;
    bool started = false;

    void Start()
    {
        EnvironmentalAudioManager.Instance.PlayMusic("main_menu_music");
        UIManager.Instance.EnableInteractionHUD(UIManager.InteractionHUDPreset.CUSTOM,"next,selectscheme");
    }
    public async void StartDemo()
    {
        UIManager.Instance.DisableInteractionHUD();
        ScenesManager.Instance.ShowLoadingScreen();
        await ScenesManager.Instance.LoadScene("Demo_Intro");
        ScenesManager.Instance.HideLoadingScreen();
        ScenesManager.Instance.ShowScene();

        await UIManager.Instance.ManualFadeOut();
    }

    public void SelectKeyboard()
    {
        keyboardButton.Select();
        keyboardButton.onClick?.Invoke();
    }

    public void SelectController()
    {
        controllerButton.Select();
        controllerButton.onClick?.Invoke();
    }

    public void OnNext(InputValue value)
    {
        if(started) return;
        started = true;
        StartDemo();
    }
}
