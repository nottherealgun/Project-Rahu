using Sirenix.Serialization;
using UnityEngine;

public class MainMenu : Menu
{
    private void Start() {
        EnvironmentalAudioManager.Instance.PlayMusic("Test");
        EnvironmentalAudioManager.Instance.PlayAmbience("Test");
    }

    public void ButtonPressed(string _button)
    {
        switch (_button)
        {
            case "continue":
                break;
            case "new_game":
                UIManager.onActionTransition += NarrativeManager.Instance.StartNewGame;
                ScenesManager.Instance.LoadScene("CH2_SC12");
                break;
            case "settings":
                ScenesManager.Instance.LoadScene("Settings");
                break;
            case "quit":
                Application.Quit();
                break;
        }
    }
}
