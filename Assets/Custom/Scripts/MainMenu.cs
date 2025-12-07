using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Cysharp.Threading.Tasks;

public class MainMenu : Menu
{
    UnityAction onButtonPressed;
    [OdinSerialize] TMP_Text gameVersionText;
    async void Start()
    {
        UIManager.LockCursor(false);
        UIManager.Instance.DisableInteractionHUD();
        EnvironmentalAudioManager.Instance.PlayMusic("main_menu_music");

        await NarrativeManager.Instance.RemoveAllCutscenes();
        NarrativeManager.Instance.SetupScene("10", "01");

        gameVersionText.text = "build " + Application.version;
        gameVersionText.text += "\nUnity: " + Application.unityVersion;

        UIManager.Instance.onCloseSettings += () =>
        {
            ReselectFirst();
        };
    }

    public void ButtonPressed(string _button)
    {
        switch (_button)
        {
            case "continue":
                break;
            case "new_game":
                onButtonPressed += NarrativeManager.Instance.StartNewGame;
                break;
            case "settings":
                // onButtonPressed += OpenSettingsMenu;
                onButtonPressed += UIManager.Instance.OpenSettingsMenu;
                break;
            case "quit":
                Application.Quit();
                break;
            case "feedback":
                Application.OpenURL("https://forms.gle/BxbxsNNHqJg1quaq5");
                break;
        }
        onButtonPressed?.Invoke();
        onButtonPressed = null;
    }
}
