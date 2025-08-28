using UnityEngine;
using UnityEngine.Events;
public class MainMenu : Menu
{
    UnityAction onButtonPressed;
    private void Start()
    {
        EnvironmentalAudioManager.Instance.PlayMusic("main_menu_music");
        // EnvironmentalAudioManager.Instance.PlayAmbience("Test");

    }

    public void ButtonPressed(string _button)
    {
        switch (_button)
        {
            case "continue":
                break;
            case "new_game":
                onButtonPressed += StartNewGame;
                break;
            case "settings":
                onButtonPressed += OpenSettingsMenu;
                break;
            case "quit":
                Application.Quit();
                break;
        }
        onButtonPressed?.Invoke();
        onButtonPressed = null;
    }

    async void StartNewGame()
    {
        ScenesManager.Instance.ShowLoadingScreen();
        await ScenesManager.Instance.LoadScene("CH02_SC12");
        ScenesManager.Instance.HideLoadingScreen();
        await NarrativeManager.Instance.PlayCutsceneSequence();
        UIManager.SetCursorState(true);
        ScenesManager.Instance.ShowScene();
    }

    async void OpenSettingsMenu()
    {
        await ScenesManager.Instance.LoadScene("Settings");
    }
}
