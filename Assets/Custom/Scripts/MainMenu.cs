using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] SceneData StartScene;
    [SerializeField] SceneData Settings;

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
                ScenesManager.Instance.onSceneLoaded += NarrativeManager.Instance.StartNewGame;
                ScenesManager.Instance.LoadScene(StartScene);
                break;
            case "settings":
                ScenesManager.Instance.LoadScene(Settings);
                break;
            case "quit":
                UnityEngine.Application.Quit();
                break;
        }
    }
}
