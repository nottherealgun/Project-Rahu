using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using PrimeTween;

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
                // GetComponent<PlayerInput>().enabled = false;
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

    public void ButtonOnHover(HorizontalLayoutGroup lg)
    {
        Tween.Custom(lg.padding.left, 50f, 0.5f, (val) => lg.padding.left = (int) val);
    }

    public void ButtonOnUnhover(HorizontalLayoutGroup lg)
    {
        Tween.Custom(lg.padding.left, 0, 0.5f, (val) => lg.padding.left = (int) val);
    }

    private void OnDestroy() {
        Tween.StopAll(this);    
    }

}
