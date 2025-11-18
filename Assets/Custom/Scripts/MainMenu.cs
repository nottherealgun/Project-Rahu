using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Threading.Tasks;
public class MainMenu : Menu
{
    UnityAction onButtonPressed;
    [OdinSerialize] TMP_Text gameVersionText;
    async void Start()
    {
        UIManager.LockCursor(false);
        UIManager.Instance.DisableInteractionHUD();
        await NarrativeManager.Instance.SetupScene("10", "01");
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
                Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSfgrIaDWybumbbBhgbqLWZgmpVR268cR1YK7johQJViCf0Uvw/viewform?usp=header");
                break;
        }
        onButtonPressed?.Invoke();
        onButtonPressed = null;
    }
}
