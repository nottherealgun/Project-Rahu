using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
public class MainMenu : Menu
{
    UnityAction onButtonPressed;
    [OdinSerialize] TMP_Text gameVersionText;
    void Start()
    {
        UIManager.LockCursor(false);
        EnvironmentalAudioManager.Instance.PlayMusic("main_menu_music");
        gameVersionText.text = "build " + Application.version;
        gameVersionText.text += "\nUnity: " + Application.unityVersion;
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
                // onButtonPressed += OpenSettingsMenu;
                onButtonPressed += () => UIManager.Instance.OpenSettingsMenu();
                break;
            case "quit":
                Application.Quit();
                break;
            case "feedback":
                Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSfgrIaDWybumbbBhgbqLWZgmpVR268cR1YK7johQJViCf0Uvw/viewform?usp=header");
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
        await NarrativeManager.Instance.PlayCutsceneSequence(true);
        UIManager.LockCursor(true);
        PersistentDataManager.Instance.ResetPuzzleData();
        ScenesManager.Instance.ShowScene();
        await UIManager.Instance.ManualFadeOut();
        PersistentDataManager.Instance.FindPlayer();
        await NarrativeManager.Instance.LateSetup();
    }
}
