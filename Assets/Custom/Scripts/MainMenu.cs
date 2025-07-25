using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] SceneSO StartScene;
    [SerializeField] SceneSO Settings;
    public void ButtonPressed(string _button)
    {
        switch (_button)
        {
            case "continue":
                break;
            case "new_game":
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
